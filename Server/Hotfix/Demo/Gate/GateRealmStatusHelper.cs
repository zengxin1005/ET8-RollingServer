using System;
using System.Collections.Generic;


namespace ET.Server
{
	public static partial class GateRealmStatusHelper
	{
        public static async ETTask Disconnect( Session session, bool skipSessionCheck = false)
        {
            if (session == null || session.IsDisposed)
            {
                return;
            }
            SessionPlayerComponent sessionPlayerComponent = session.GetComponent<SessionPlayerComponent>();
            PlayerSessionComponent playerSessionComponent = sessionPlayerComponent?.Player?.GetComponent<PlayerSessionComponent>();
            if (playerSessionComponent?.Session == session)
            {
                playerSessionComponent.Session = null;
            }
            long instanceId = session.InstanceId;

            TimerComponent timerComponent = session.Root().GetComponent<TimerComponent>();
            await timerComponent.WaitAsync(1000);

            if (session.InstanceId != instanceId)
            {
                return;
            }
            sessionPlayerComponent = session.GetComponent<SessionPlayerComponent>();
            playerSessionComponent = sessionPlayerComponent?.Player?.GetComponent<PlayerSessionComponent>();
            if (!skipSessionCheck && playerSessionComponent?.Session == session)
            {
                return;
            }
            session.Dispose();
        }
	    public static async ETTask<bool> CheckAccountAndZoneStatus( Session session, string account, int zoneId, IResponse response)
        {
            //防止各种原因被踢后，又来登录,再查一次
            long instanceId = session.InstanceId;
            DBComponent dbComponent = session.Root().GetComponent<DBManagerComponent>().GetZoneDB(session.Zone());
            ZoneStatus zoneStatus =  await dbComponent.Query<ZoneStatus>(zoneId);
            // 1. IP白名单校验（白名单开启时，IP必须在目标区服的白名单中）
            HashSet<string> loginWhiteIpList = zoneStatus?.LoginWhiteIpList;
            string clientIP = session.RemoteAddress.Address.ToString();
            if (loginWhiteIpList?.Count > 0 && !loginWhiteIpList.Contains(clientIP))
            {
                response.Error = ErrorCore.ERR_NotInWhitelist;
                response.Message = "该服务器暂未开放登录，请选择其他服务器";
                return false;
            }
            
            // 2. 区服状态校验
            if (zoneStatus?.Status == 1)
            {
                response.Error = ErrorCore.ERR_ZoneMaintenance;
                response.Message = "服务器维护中，请稍后再试";
                return false;
            }
            
            // 3. 开服时间校验
            if (TimeInfo.Instance.ClientNow() < zoneStatus?.OpenTime)
            {
                response.Error = ErrorCore.ERR_ZoneNotOpen;
                DateTime openDateTime = new DateTime(1970, 1, 1, 8, 0, 0, DateTimeKind.Utc).AddMilliseconds(zoneStatus.OpenTime);
                response.Message = $"服务器将于 {openDateTime:yyyy-MM-dd HH:mm} 开启，暂时无法登录";
                return false;
            }
            long accountId = account.GetLongHashCode();
            AccountInfo accountInfo = await dbComponent.Query<AccountInfo>(accountId);
            if (accountInfo == null)
            {
                response.Error = ErrorCore.ERR_NotFoundAccount;
                response.Message = "找不到账号信息";
                return false;
            }

            switch (accountInfo.Status)
            {
                case 1:
                    // 检查封禁是否到期
                    if (accountInfo.BanEndTime <= 0 || accountInfo.BanEndTime > TimeInfo.Instance.ClientNow())
                    {
                        response.Error = ErrorCore.ERR_AccountBanned;
                        response.Message = "账号已被封禁，如有疑问请联系客服";
                        return false;
                    }
                    break;
                case 2:
                    response.Error = ErrorCore.ERR_AccountFrozen;
                    response.Message = "账号已被冻结，如有疑问请联系客服";
                    return false;
            }
                
            if (accountInfo.ZoneBans.TryGetValue(zoneId, out ZoneBanInfo zoneBan) && zoneBan != null)
            {
                switch (zoneBan.Status)
                {
                    case 1:
                        // 检查封禁是否到期
                        if (zoneBan.BanEndTime <= 0 || zoneBan.BanEndTime > TimeInfo.Instance.ClientNow())
                        {
                            response.Error = ErrorCore.ERR_ZonePlayerBanned;
                            response.Message = "该区角色已被封禁，请更换服务器或联系客服";
                            return false;
                        }
                        break;
                    case 2:
                        response.Error = ErrorCore.ERR_ZonePlayerFrozen;
                        response.Message = "该区角色已被冻结，请更换服务器或联系客服";
                        return false;
                }
            }
            if (session.InstanceId != instanceId)
            {
                return false;
            }
            return true;
        }
	}
}
