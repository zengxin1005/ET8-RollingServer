using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Security.Cryptography;


namespace ET.Server
{
	[MessageSessionHandler(SceneType.Realm)]
	public class C2R_SDKLoginHandler : MessageSessionHandler<C2R_SDKLogin, R2C_SDKLogin>
	{
		protected override async ETTask Run(Session session, C2R_SDKLogin request, R2C_SDKLogin response)
		{
			//跟第三方SDK验证忽略，如果验证失败提前返回
			long accountId = request.UserId.GetLongHashCode();
			//如果相同库的话，多个Realm 相同账号注册同一时间有可能相互覆盖，概率很小，先不考虑
			DBComponent dbComponent = session.Root().GetComponent<DBManagerComponent>().GetZoneDB(session.Zone());
			using (await session.Root().GetComponent<CoroutineLockComponent>().Wait(CoroutineLockType.Account, accountId))
			{
				AccountInfo accountInfo = await dbComponent.Query<AccountInfo>(accountId);
				if (accountInfo == null)
				{
					// 创建新账号
					accountInfo = session.Root().AddChildWithId<AccountInfo>(accountId);
					accountInfo.Account = request.UserId;
					accountInfo.Password = request.Token;
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
					accountInfo.Password =  request.Token;//每次更新Token作为密码，正常登录后，泄露的也无法再登，但是安全性也不是最好
					await dbComponent.Save(accountInfo);
				}
			}
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
		
	}
}