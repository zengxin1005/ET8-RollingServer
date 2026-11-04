using Unity.Mathematics;
namespace ET.Server
{
	[MessageSessionHandler(SceneType.Gate)]
	public class C2G_EnterMapHandler : MessageSessionHandler<C2G_EnterMap, G2C_EnterMap>
	{
		protected override async ETTask Run(Session session, C2G_EnterMap request, G2C_EnterMap response)
		{
			int zoneId = request.ZoneId > 0 ? request.ZoneId : session.Zone();
			Player player = session.GetComponent<SessionPlayerComponent>()?.Player;
			//如果player 为空是没有登录Gate,如果ZoneId不一致绝对是客户端被篡改了
			if (player == null || player.ZoneId != zoneId)
			{
				response.Error = ErrorCore.ERR_SessionAccountMismatch;
				response.Message = "登录状态异常，请重新登录";
				return;
			}
			
			if (player.GetComponent<PlayerSessionComponent>()?.Session == null)	// 正在断开中
			{
				return;
			}
			if (player.GetComponent<PlayerSessionComponent>()?.Session != session)
			{
				response.Error = ErrorCore.ERR_SessionReplaced;
				response.Message = "账号已在其他地方登录";
				return;
			}
			
			
			// 在Gate上动态创建一个Map Scene，把Unit从DB中加载放进来，然后传送到真正的Map中，这样登陆跟传送的逻辑就完全一样了
			
			GateMapComponent gateMapComponent = player.GetComponent<GateMapComponent>()??player.AddComponent<GateMapComponent>();
			
			gateMapComponent.Scene ??=
						await GateMapFactory.Create(gateMapComponent, player.Id, IdGenerater.Instance.GenerateInstanceId(), "GateMap");
			

			Scene scene = gateMapComponent.Scene;

			//因为是滚服架构，也可能是任意类型的游戏比如卡牌，这里玩家信息直接使用Unit也没有问题，严格意义上来说他的地图上的单位！
			
			UnitComponent unitComponent = scene.GetComponent<UnitComponent>();
			Unit unit =  unitComponent.Get(player.Id);
			if (unit != null) //传送中：由先到的那条负责传送
			{
				response.UnitId = player.Id;
				return;
			}
			using (await session.Root().GetComponent<CoroutineLockComponent>().Wait(CoroutineLockType.Unit, player.Id))
			{
				DBComponent dbComponent = session.Root().GetComponent<DBManagerComponent>().GetZoneDB(zoneId);
				unit = await dbComponent.Query<Unit>(player.Id);
				if (player.GetComponent<PlayerSessionComponent>()?.Session == null) // 网络断开了，肯定比不能传送了
				{
					return;
				}
				if (player.GetComponent<PlayerSessionComponent>()?.Session != session) //在其他地方登录肯定不能传送了
				{
					response.Error = ErrorCore.ERR_SessionReplaced;
					response.Message = "账号已在其他地方登录";
					return;
				}
				if (unit == null)
				{
					response.UnitId = 0; //要创建了,没有角色
					return;
				}
				if (unitComponent.Get(player.Id) != null)
				{
					response.UnitId = player.Id; //传送中：由先到的那条负责传送
					return;
				}

				unitComponent.AddChild(unit);
				// unit.AddComponent<MoveComponent>();
				// unit.Position = new float3(-10, 0, -10);
				//
				// NumericComponent numericComponent = unit.AddComponent<NumericComponent>();
				// numericComponent.Set(NumericType.Speed, 6f); // 速度是6米每秒
				// numericComponent.Set(NumericType.AOI, 15000); // 视野15米
				unitComponent.Add(unit);
				
				unit.LastLoginTime = TimeInfo.Instance.ClientNow();
				await dbComponent.Save(unit);
				
				if (player.GetComponent<PlayerSessionComponent>()?.Session == null)	// 网络断开了，肯定比不能传送了
				{
					unitComponent.Remove(player.Id);
					return;
				}
				if (player.GetComponent<PlayerSessionComponent>()?.Session != session)	//在其他地方登录肯定不能传送了
				{
					response.Error = ErrorCore.ERR_SessionReplaced;
					response.Message = "账号已在其他地方登录";
					unitComponent.Remove(player.Id);
					return;
				}
			}
			// 加入aoi
			unit.AddComponent<AOIEntity, int, float3>(9 * 1000, unit.Position);
			//如果多个服共享多个Map,（这种情况有可能是在所有Map里面通过ZoneId进行逻辑分服，比如所有服共用一个库，不同区的用户在一张表，以后合服很方便）
			//，否则使用session.Zone(), 单个服独占一个Map,使用ZoneId，当然这种独立的库，合服需要写脚本合并数据或者通过载入程序合并清洗。
			StartSceneConfig startSceneConfig = StartSceneConfigCategory.Instance.GetBySceneName(zoneId, "Map1");
			response.UnitId = player.Id;

			// 等到一帧的最后面再传送，先让G2C_EnterMap返回，否则传送消息可能比G2C_EnterMap还早
			TransferHelper.TransferAtFrameFinish(unit, startSceneConfig.ActorId, startSceneConfig.Name).Coroutine();
		}
	}
}