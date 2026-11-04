using System;
using System.Linq;
using System.Collections.Generic;

namespace ET.Server
{
    [MessageSessionHandler(SceneType.Gate)]
    public class C2G_LoginGateHandler : MessageSessionHandler<C2G_LoginGate, G2C_LoginGate>
    {
        protected override async ETTask Run(Session session, C2G_LoginGate request, G2C_LoginGate response)
        {
            Scene root = session.Root();
            string account = root.GetComponent<GateSessionKeyComponent>().Get(request.Key);
            if (account == null)
            {
                response.Error = ErrorCore.ERR_ConnectGateKeyError;
                response.Message = "Gate key验证失败!";
                return;
            }
            
            int zoneId = request.ZoneId > 0 ? request.ZoneId : session.Zone();
            
            bool hasMap = StartSceneConfigCategory.Instance.Maps
                    .Any(m => m.Zone == zoneId);
            if (!hasMap)
            {
                response.Error = ErrorCore.ERR_NotFoundMap;
                response.Message = "服务器列表已更新，请重新选择";
                return;
            }
            

            string zoneAccount = $"{account}_{zoneId}";
            
            //这种情况就是同一个Session登录两个不同的账号了，后端不得不防
            SessionPlayerComponent sessionPlayerComponent = session.GetComponent<SessionPlayerComponent>();
            if (sessionPlayerComponent?.Player != null && !sessionPlayerComponent.Player.Account.Equals(zoneAccount))
            {
                response.Error = ErrorCore.ERR_SessionAccountMismatch;
                response.Message = "该连接已绑定其他账号";
                return;
            }
            
            if (!await GateRealmStatusHelper.CheckAccountAndZoneStatus(session, account, zoneId, response))
            {
                return;
            }
            
            
            session.RemoveComponent<SessionAcceptTimeoutComponent>();

            long zoneAccountId = zoneAccount.GetLongHashCode();
            
            using (await root.GetComponent<CoroutineLockComponent>().Wait(CoroutineLockType.Account, zoneAccountId))
            { 
                //如果同一个账号不同服可以共享的话
                PlayerComponent playerComponent = root.GetComponent<PlayerComponent>();
                Player player = playerComponent.GetByAccount(zoneAccount);
                if (player == null)
                {
                    player = playerComponent.AddChildWithId<Player, string>(zoneAccountId,zoneAccount);
                    playerComponent.Add(player);
                    PlayerSessionComponent playerSessionComponent = player.AddComponent<PlayerSessionComponent>();
                    playerSessionComponent.AddComponent<MailBoxComponent, MailBoxType>(MailBoxType.GateSession);
                    await playerSessionComponent.AddLocation(LocationType.GateSession);
			
                    player.AddComponent<MailBoxComponent, MailBoxType>(MailBoxType.UnOrderedMessage);
                    await player.AddLocation(LocationType.Player);
                    sessionPlayerComponent = session.GetComponent<SessionPlayerComponent>()
                            ?? session.AddComponent<SessionPlayerComponent>();
                    sessionPlayerComponent.Player = player;
                    playerSessionComponent.Session = session;
                }
                else
                {
                    // 判断是否在战斗
                    // PlayerRoomComponent playerRoomComponent = player.GetComponent<PlayerRoomComponent>();
                    // if (playerRoomComponent.RoomActorId != default)
                    // {
                    //     CheckRoom(player, session).Coroutine();
                    // }
                    // else
                    {

                        PlayerSessionComponent playerSessionComponent = player.GetComponent<PlayerSessionComponent>();
                        if (playerSessionComponent.Session != session)
                        {
                            G2C_KickOut g2CKickOut = G2C_KickOut.Create();
                            g2CKickOut.Reason = 0;
                            playerSessionComponent.Session?.Send(g2CKickOut);
                            GateRealmStatusHelper.Disconnect(playerSessionComponent.Session).Coroutine();
                            sessionPlayerComponent = session.GetComponent<SessionPlayerComponent>()
                                    ?? session.AddComponent<SessionPlayerComponent>();
                            sessionPlayerComponent.Player = player;
                            playerSessionComponent.Session = session;
                        }
                    }
                }
                player.ZoneId = zoneId;
                response.PlayerId = player.Id;
            }
            await ETTask.CompletedTask;
        }

        

        
        private static async ETTask CheckRoom(Player player, Session session)
        {
            Fiber fiber = player.Fiber();
            await fiber.WaitFrameFinish();

            // G2Room_Reconnect g2RoomReconnect = G2Room_Reconnect.Create();
            // g2RoomReconnect.PlayerId = player.Id;
            // using Room2G_Reconnect room2GateReconnect = await fiber.Root.GetComponent<MessageSender>().Call(
            //     player.GetComponent<PlayerRoomComponent>().RoomActorId,
            //     g2RoomReconnect) as Room2G_Reconnect;
            // G2C_Reconnect g2CReconnect = G2C_Reconnect.Create();
            // g2CReconnect.StartTime = room2GateReconnect.StartTime;
            // g2CReconnect.Frame = room2GateReconnect.Frame;
            // g2CReconnect.UnitInfos.AddRange(room2GateReconnect.UnitInfos);
            // session.Send(g2CReconnect);
            
            session.AddComponent<SessionPlayerComponent>().Player = player;
            player.GetComponent<PlayerSessionComponent>().Session = session;
        }
    }
}