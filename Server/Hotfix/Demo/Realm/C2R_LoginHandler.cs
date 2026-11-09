using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Security.Cryptography;


namespace ET.Server
{
	[MessageSessionHandler(SceneType.Realm)]
	public class C2R_LoginHandler : MessageSessionHandler<C2R_Login, R2C_Login>
	{
		protected override async ETTask Run(Session session, C2R_Login request, R2C_Login response)
		{
			// 使用客户端传入的ZoneId分配Gate，而非session.Zone()
			//如果逻辑服都要至少一个Gate，这里是用ZoneId, 共享的话就用session.Zone()
			if (!StartSceneConfigCategory.Instance.Gates.ContainsKey(session.Zone()))
			{
				response.Error = ErrorCore.ERR_NotFoundGate;
				response.Message = "登录异常，请稍后重试";
				CloseSession(session).Coroutine();
				return;
			}
			int zoneId = request.ZoneId > 0? request.ZoneId : session.Zone();
			//一个服独占一个Map咯，不要那种多个多对多
			bool hasMap = StartSceneConfigCategory.Instance.Maps
					.Any(m => m.Zone == zoneId);
			if (!hasMap)
			{
				response.Error = ErrorCore.ERR_NotFoundMap;
				response.Message = "服务器列表已更新，请重新选择";
				CloseSession(session).Coroutine();
				return;
			}
			//所有Realm 都要配置成相同的Zone数据库哦！！！！！！！！
			DBComponent dbComponent = session.Root().GetComponent<DBManagerComponent>().GetZoneDB(session.Zone());
			ZoneStatus zoneStatus =  await dbComponent.Query<ZoneStatus>(zoneId);
			// 1. IP白名单校验（白名单开启时，IP必须在目标区服的白名单中）
			HashSet<string> loginWhiteIpList = zoneStatus?.LoginWhiteIpList;
			string clientIP = session.RemoteAddress.Address.ToString();
			if (loginWhiteIpList?.Count > 0 && !loginWhiteIpList.Contains(clientIP))
			{
				response.Error = ErrorCore.ERR_NotInWhitelist;
				response.Message = "该服务器暂未开放登录，请选择其他服务器";
				CloseSession(session).Coroutine();
				return;
			}
			
			// 2. 区服状态校验
			if (zoneStatus?.Status == 1)
			{
				response.Error = ErrorCore.ERR_ZoneMaintenance;
				response.Message = "服务器维护中，请稍后再试";
				CloseSession(session).Coroutine();
				return;
			}
			
			// 3. 开服时间校验：未到 OpenTime 一律拒绝（维护优先于未开服，所以放在维护判断之后）
			if (TimeInfo.Instance.ClientNow() < zoneStatus?.OpenTime)
			{
				response.Error = ErrorCore.ERR_ZoneNotOpen;
				// 1970 起点写成 8 点 = 按 UTC+8（北京时间）展示，玩家看到的才是本地钟
				DateTime openDateTime = new DateTime(1970, 1, 1, 8, 0, 0, DateTimeKind.Utc).AddMilliseconds(zoneStatus.OpenTime);
				response.Message = $"服务器将于 {openDateTime:yyyy-MM-dd HH:mm} 开启，暂时无法登录";
				CloseSession(session).Coroutine();
				return;
			}
			
			// 4. 人数上限校验：该 Zone 在线人数超过单服承载就不再放人进来
			ZoneLoadComponent zoneStatusComponent = session.Root().GetComponent<ZoneLoadComponent>();
			if (zoneStatusComponent != null && zoneStatusComponent.IsOverCapacity(zoneId))
			{
				response.Error = ErrorCore.ERR_ZonePlayerFull;
				response.Message = "当前服务器人数已满，请稍后再试或选择其他服务器";
				CloseSession(session).Coroutine();
				return;
			}
			
			// 5. 加载或创建账号信息
			long accountId = request.Account.GetLongHashCode();
			//如果相同库的话，多个Realm 相同账号注册同一时间有可能相互覆盖，概率很小，先不考虑
			using (await session.Root().GetComponent<CoroutineLockComponent>().Wait(CoroutineLockType.Account, accountId))
			{
				AccountInfo accountInfo = await dbComponent.Query<AccountInfo>(accountId);
				if (accountInfo == null)
				{
					// 创建新账号
					accountInfo = session.Root().AddChildWithId<AccountInfo>(accountId);
					accountInfo.Account = request.Account;
					accountInfo.Password = HashPassword(request.Password);
					accountInfo.RegisterTime = TimeInfo.Instance.ClientNow();
					accountInfo.RegisterIP = session.RemoteAddress.ToString();
					accountInfo.Channel = request.Channel;
					accountInfo.SubChannel = request.SubChannel;
					accountInfo.Platform = request.Platform;
					accountInfo.Status = 0; // 正常
					await dbComponent.Save(accountInfo);
					accountInfo.Dispose();
				}
				else
				{
					// 密码校验
					string hashedPassword = HashPassword(request.Password);
					if (accountInfo.Password != hashedPassword)
					{
						response.Error = ErrorCore.ERR_PasswordError;
						response.Message = "密码错误";
						CloseSession(session).Coroutine();
						return;
					}

					// 6. 账号状态校验
					switch (accountInfo.Status)
					{
						case 1:
							// 检查封禁是否到期
							if (accountInfo.BanEndTime > 0 && accountInfo.BanEndTime <= TimeInfo.Instance.ClientNow())
							{
								// 封禁已到期，自动解封
								accountInfo.Status = 0;
								accountInfo.BanEndTime = 0;
								await dbComponent.Save(accountInfo);
							}
							else
							{
								response.Error = ErrorCore.ERR_AccountBanned;
								response.Message = "账号已被封禁，如有疑问请联系客服";
								CloseSession(session).Coroutine();
								return;
							}
							break;
						case 2:
							response.Error = ErrorCore.ERR_AccountFrozen;
							response.Message = "账号已被冻结，如有疑问请联系客服";
							CloseSession(session).Coroutine();
							return;
					}
					
					if (accountInfo.ZoneBans.TryGetValue(zoneId, out ZoneBanInfo zoneBan) && zoneBan != null)
					{
						switch (zoneBan.Status)
						{
							case 1:
								// 检查封禁是否到期
								if (zoneBan.BanEndTime > 0 && zoneBan.BanEndTime <= TimeInfo.Instance.ClientNow())
								{
									// 封禁已到期，自动解封
									accountInfo.ZoneBans.Remove(zoneId);
									await dbComponent.Save(accountInfo);
								}
								else
								{
									response.Error = ErrorCore.ERR_ZonePlayerBanned;
									response.Message = "该区角色已被封禁，请更换服务器或联系客服";
									CloseSession(session).Coroutine();
									return;
								}
								break;
							case 2:
								response.Error = ErrorCore.ERR_ZonePlayerFrozen;
								response.Message = "该区角色已被冻结，请更换服务器或联系客服";
								CloseSession(session).Coroutine();
								return;
						}
					}
				}
			}

			//随机分配一个Gate  如果逻辑服都要至少一个Gate，这里是用ZoneId, 共享的话就用session.Zone()
			//客户端也根据account的Hash索引Realm 所以account的分配的Realm跟Gate是固定的
			StartSceneConfig config = RealmGateAddressHelper.GetGate(session.Zone(), request.Account);
			Log.Debug($"gate address: {config}");
			
			// 向gate请求一个key,客户端可以拿着这个key连接gate
			R2G_GetLoginKey r2GGetLoginKey = R2G_GetLoginKey.Create();
			r2GGetLoginKey.Account = request.Account;
			G2R_GetLoginKey g2RGetLoginKey = (G2R_GetLoginKey) await session.Fiber().Root.GetComponent<MessageSender>().Call(
				config.ActorId, r2GGetLoginKey);

			response.Address = config.InnerIPPort.ToString();
			response.Key = g2RGetLoginKey.Key;
			response.GateId = g2RGetLoginKey.GateId;
			
			CloseSession(session).Coroutine();
		}

		private async ETTask CloseSession(Session session)
		{
			if (session == null || session.IsDisposed)
			{
				return;
			}
			long instanceId = session.InstanceId;
			await session.Root().GetComponent<TimerComponent>().WaitAsync(1000);
			if (session.InstanceId != instanceId)
			{
				return;
			}
			session.Dispose();
		}

		/// <summary>
		/// 密码加盐哈希
		/// </summary>
		private static string HashPassword(string password)
		{
			using SHA256 sha256 = SHA256.Create();
			byte[] bytes = Encoding.UTF8.GetBytes(password + ConstValue.SaltValue);
			byte[] hash = sha256.ComputeHash(bytes);
			return BitConverter.ToString(hash).Replace("-", "").ToLowerInvariant();
		}
	}
}