using System.Collections.Generic;
using System.Linq;

namespace ET.Server
{
    [MessageSessionHandler(SceneType.Realm)]
    public class C2R_GetServerListHandler : MessageSessionHandler<C2R_GetServerList, R2C_GetServerList>
    {
        protected override async ETTask Run(Session session, C2R_GetServerList request, R2C_GetServerList response)
        {
            List<StartZoneConfig> zoneConfigs = StartZoneConfigCategory.Instance.DataList;
            //所有Realm 都要配置成相同的Zone数据库哦！！！！！！！！
            DBComponent dbComponent = session.Root().GetComponent<DBManagerComponent>().GetZoneDB(session.Zone());
            foreach (StartZoneConfig zoneConfig in zoneConfigs)
            {
                // 该区必须同时配置了Gate和Map才算可用,前提是一个服配置了一个Gate
                // if (!StartSceneConfigCategory.Instance.Gates.ContainsKey(zoneConfig.Id))
                // {
                //     Log.Warning($"gate config not found: {zoneConfig.Id}");
                //     continue;
                // }
                //一个服独占一个Map咯，不要那种多个多对多
                bool hasMap = StartSceneConfigCategory.Instance.Maps
                    .Any(m => m.Zone == zoneConfig.Id);
                if (!hasMap)
                {
                    Log.Warning($"map config not found: {zoneConfig.Id}");
                    continue;
                }
                
                ZoneStatus zoneStatus =  await dbComponent.Query<ZoneStatus>(zoneConfig.Id);
                // 白名单校验：白名单开启时，IP不在该区白名单中则跳过
                HashSet<string> loginWhiteIpList = zoneStatus?.LoginWhiteIpList;
                string clientIP = session.RemoteAddress.Address.ToString();
                if (loginWhiteIpList?.Count > 0 && !loginWhiteIpList.Contains(clientIP))
                {
                    continue;
                }
                
                ServerInfo serverInfo = ServerInfo.Create();
                serverInfo.ZoneId = zoneConfig.Id;
                var zoneDesc   = zoneConfig.Desc.Split('_');
                serverInfo.GroupName = zoneDesc .Length > 0 ? zoneDesc [0] : "";
               
                if (string.IsNullOrEmpty(zoneStatus?.Alias))
                {
                    serverInfo.ServerName = zoneDesc .Length > 1 ? zoneDesc [1] : "";
                }
                else
                {
                    serverInfo.ServerName = zoneStatus?.Alias;
                }
                serverInfo.ServerDesc = zoneDesc.Length > 2 ? zoneDesc [2] : "";
                serverInfo.Status = zoneStatus?.Status ?? 0;                      // 0 = 正常
                serverInfo.Load = zoneStatus?.Load ?? 0;                          // 0 = 流畅
                serverInfo.IsNewZone = zoneStatus?.IsNewZone ?? false;
                serverInfo.RecommendWeight = zoneStatus?.RecommendWeight ?? 0;
                serverInfo.OpenTime = zoneStatus?.OpenTime??0;
                response.ServerList.Add(serverInfo);
                if (!string.IsNullOrEmpty(request.Account))
                {
                    DBComponent zoneDBComponent = session.Root().GetComponent<DBManagerComponent>().GetZoneDB(serverInfo.ZoneId);
                    string zoneAccount = $"{request.Account}_{serverInfo.ZoneId}";
                    Unit unit = await zoneDBComponent.Query<Unit>(zoneAccount.GetLongHashCode());
                    if (unit != null)
                    {
                         UnitInfo unitInfo = UnitInfo.Create();
                         unitInfo.UnitId = unit.Id;
                         unitInfo.ConfigId = unit.ConfigId;
                         unitInfo.Position = unit.Position;
                         unitInfo.Forward = unit.Forward;
                         unitInfo.ZoneId = unit.ZoneId;
                         unitInfo.Sex = unit.Sex;
                         unitInfo.Level = unit.Level;
                         unitInfo.NickName = unit.NickName; 
                         unitInfo.Avatar = unit.Avatar;
                         unitInfo.LastLoginTime = unit.LastLoginTime;
                         response.UnitInfoList.Add(unitInfo);
                    }
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