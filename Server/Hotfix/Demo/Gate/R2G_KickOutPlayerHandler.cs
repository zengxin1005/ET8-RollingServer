using System;


namespace ET.Server
{
	[MessageHandler(SceneType.Gate)]
	[FriendOf(typeof(PlayerComponent))]
	public class R2G_KickOutPlayerHandler: MessageHandler<Scene, R2G_KickOutPlayer>
	{
		protected override async ETTask Run(Scene scene, R2G_KickOutPlayer request)
		{
			PlayerComponent playerComponent = scene.GetComponent<PlayerComponent>(); 

			if (string.IsNullOrEmpty(request.Account))
			{
				foreach (EntityRef<Player> er in playerComponent.dictionary.Values)
				{
					Player player = er;
					if (player == null) 
						continue;

					if (request.ZoneId > 0 && player.ZoneId != request.ZoneId)
						continue; 

					Session session = player.GetComponent<PlayerSessionComponent>()?.Session;
					
					G2C_KickOut g2CKickOut = G2C_KickOut.Create();
					g2CKickOut.Reason = 1;
					g2CKickOut.EndTime = request.EndTime;
					session?.Send(g2CKickOut);
					GateRealmStatusHelper.Disconnect(session,true).Coroutine();
				}
			}
			else
			{
				string zoneAccount = $"{request.Account}_{request.ZoneId}";//如果同一个账号不同服可以共享的话
				Player player = playerComponent.GetByAccount(zoneAccount);
				if (player != null && player.ZoneId == request.ZoneId)
				{
					Session session = player.GetComponent<PlayerSessionComponent>()?.Session;
					G2C_KickOut g2CKickOut = G2C_KickOut.Create();
					g2CKickOut.Reason = 2;
					g2CKickOut.EndTime = request.EndTime;
					session?.Send(g2CKickOut);
					GateRealmStatusHelper.Disconnect(session,true).Coroutine();
				}
			}
			await ETTask.CompletedTask;
		}
	}
}