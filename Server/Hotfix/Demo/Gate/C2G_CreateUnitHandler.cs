using Unity.Mathematics;
using System;
using System.Collections.Generic;
namespace ET.Server
{
	[MessageSessionHandler(SceneType.Gate)]
	public class C2G_CreateUnitHandler : MessageSessionHandler<C2G_CreateUnit, G2C_CreateUnit>
	{
		protected override async ETTask Run(Session session, C2G_CreateUnit request, G2C_CreateUnit response)
		{
			int zoneId = request.ZoneId > 0 ? request.ZoneId : session.Zone();
			Player player = session.GetComponent<SessionPlayerComponent>()?.Player;
			
			if (player == null || player.ZoneId != zoneId)
			{
				response.Error = ErrorCore.ERR_SessionAccountMismatch;
				response.Message = "登录状态异常，请重新登录";
				return;
			}
			
			if (player.GetComponent<PlayerSessionComponent>()?.Session == null)	//  正在断开中
			{
				return;
			}
			
			if (player.GetComponent<PlayerSessionComponent>()?.Session != session)
			{
				response.Error = ErrorCore.ERR_SessionReplaced;
				response.Message = "账号已在其他地方登录";
				return;
			}
			UnitComponent unitComponent = player.GetComponent<GateMapComponent>()?.Scene?.GetComponent<UnitComponent>();
			if (unitComponent?.Get(player.Id) != null)
			{
				response.Error = ErrorCore.ERR_UnitAlreadyExists;
				response.Message = "角色已存在，请重新登录";
				return;
			}


			if (string.IsNullOrWhiteSpace(request.NickName)|| request.NickName.Length <2 || request.NickName.Length > 7 )
			{
				response.Error = ErrorCore.ERR_NickNamePoolEmpty;
				response.Message = "昵称长度需为2~7个字符";
				return;
			}
			
			// 昵称不能含敏感词（精确 + 按字序匹配，实现见 GateHelper.HasFilterWord）
			if (GateHelper.HasFilterWord(request.NickName))
			{
				response.Error = ErrorCore.ERR_NickNamePoolEmpty;
				response.Message = "昵称包含敏感词，请更换";
				return;
			}
			
			
			using (await session.Root().GetComponent<CoroutineLockComponent>().Wait(CoroutineLockType.Unit, player.Id))
			{
				DBComponent dbComponent = session.Root().GetComponent<DBManagerComponent>().GetZoneDB(zoneId);
				Unit unit = await dbComponent.Query<Unit>(player.Id);
				if (player.GetComponent<PlayerSessionComponent>()?.Session == null) // 网络断开了，连返回都没有，就直接失败了更好
				{
					return;
				}
				if (player.GetComponent<PlayerSessionComponent>()?.Session != session) //你不能偷别人的号去注册
				{
					response.Error = ErrorCore.ERR_SessionReplaced;
					response.Message = "账号已在其他地方登录";
					return;
				}
				if (unit != null)
				{
					response.Error = ErrorCore.ERR_UnitAlreadyExists;
					response.Message = "角色已存在，请重新登录";
					return;
				}
				ZoneStatus zoneStatus = await dbComponent.Query<ZoneStatus>(zoneId);
				if (zoneStatus?.Status == 1)
				{
					response.Error = ErrorCore.ERR_ZoneCannotRegister;
					response.Message = "该服务器暂未开放创建角色，请选择其他服务器";
					return;
				}
				List<Unit> sameNickNames = await dbComponent.Query<Unit>(u => u.NickName == request.NickName);
				if (sameNickNames.Count > 0)
				{
					response.Error = ErrorCore.ERR_NickNameDuplicate;
					response.Message = "昵称已被占用，请换一个";
					return;
				}
				if (player.GetComponent<PlayerSessionComponent>()?.Session == null) // 网络断开了，连返回都没有，就直接失败了更好
				{
					return;
				}
				if (player.GetComponent<PlayerSessionComponent>()?.Session != session) //你不能偷别人的号去注册
				{
					response.Error = ErrorCore.ERR_SessionReplaced;
					response.Message = "账号已在其他地方登录";
					return;
				}
				//这里直接作为玩家信息，暂时没有额外的配置，如果有自行配置ConfigId
				unit = unitComponent.AddChildWithId<Unit, int>(player.Id, 1001); 
				unit.Sex = request.Sex;
				unit.ZoneId = request.ZoneId;
				unit.NickName = request.NickName;
				unit.Avatar = request.Avatar;
				await dbComponent.Save(unit);
				unit.Dispose();
				response.UnitId = player.Id;
			}
		}
	}
}
