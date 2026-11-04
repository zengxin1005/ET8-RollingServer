using System.Net;
using System.Linq;
namespace ET.Server
{
    /// <summary>
    /// HTTP接口：修改玩家账号状态
    /// GET /set_player_status?account=xxx&status=2&banEndTime=0
    /// status: 0=正常, 1=封禁, 2=冻结
    /// zone: 只限制某个服
    /// banEndTime: 封禁结束时间戳(秒), 对status=2有效
    /// </summary>
    [HttpHandler(SceneType.Realm, "/set_account_status")]
    public class HttpSetAccountStatusHandler : IHttpHandler
    {
        public async ETTask Handle(Scene scene, HttpListenerContext context)
        {
            HttpListenerRequest request = context.Request;
            
            // 1. 从 URL 参数或 Header 中获取 token
            string token = request.QueryString["token"] ?? request.Headers["X-API-Key"];

            // 2. 比对固定密钥（可从配置读取）
            if (string.IsNullOrEmpty(token) || token != ConstValue.AdminApiKey)
            {
                HttpHelper.Response(context, new { Error = 403, Message = "Unauthorized" });
                return;
            }
            
            
            string account = request.QueryString["account"];
            int status = int.Parse(request.QueryString["status"] ?? "0");
            long banEndTime = long.Parse(request.QueryString["banEndTime"] ?? "0");

            if (string.IsNullOrEmpty(account))
            {
                HttpHelper.Response(context, new { Error = 1, Message = "account is required" });
                return;
            }

            if (status < 0 || status > 2)
            {
                HttpHelper.Response(context, new { Error = 2, Message = "invalid status, must be 0/1/2" });
                return;
            }
            int zone = int.Parse(request.QueryString["zone"] ?? "0");
            if (!string.IsNullOrEmpty(request.QueryString["zone"]))
            {
                bool hasZone = StartZoneConfigCategory.Instance.DataList
                        .Any(m => m.Id == zone);
                if (!hasZone)
                {
                    HttpHelper.Response(context, new { Error = 1, Message = "invalid zone" });
                    return;
                }
            }
            
            long accountId = account.GetLongHashCode();
            //如果相同库的话，多个Realm 相同账号修改同一时间有可能相互覆盖，概率很小，先不考虑
            using (await scene.Root().GetComponent<CoroutineLockComponent>().Wait(CoroutineLockType.Account, accountId))
            {
                DBComponent dbComponent = scene.Root().GetComponent<DBManagerComponent>().GetZoneDB(scene.Zone());
                AccountInfo accountInfo = await dbComponent.Query<AccountInfo>(accountId);

                if (accountInfo == null)
                {
                    HttpHelper.Response(context, new { Error = 3, Message = "account not found" });
                    return;
                }

                if (string.IsNullOrEmpty(request.QueryString["zone"]))
                {
                    accountInfo.Status = status;
                    if (accountInfo.Status == 1)
                    {
                        accountInfo.BanEndTime = banEndTime *1000;
                    }
                }
                else
                {
                    if (!accountInfo.ZoneBans.TryGetValue(zone, out ZoneBanInfo zoneBan))
                    {
                        zoneBan = new ZoneBanInfo();
                        accountInfo.ZoneBans.Add(zone, zoneBan);
                    }

                    zoneBan.Status = status;
                    if (zoneBan.Status == 1)
                    {
                        zoneBan.BanEndTime = banEndTime *1000;
                    }
                }
                await dbComponent.Save(accountInfo);
                
                if (status == 1 || status == 2)
                {
                    StartSceneConfig config = RealmGateAddressHelper.GetGate(scene.Zone(), account);
                    R2G_KickOutPlayer r2GKickOutPlayer = R2G_KickOutPlayer.Create();
                    r2GKickOutPlayer.Account = account;
                    r2GKickOutPlayer.ZoneId = zone;
                    if (string.IsNullOrEmpty(request.QueryString["zone"]))
                    {
                        if (accountInfo.Status == 1)
                        {
                            r2GKickOutPlayer.EndTime = accountInfo.BanEndTime;
                        }
                    }
                    else if (accountInfo.ZoneBans.TryGetValue(zone, out ZoneBanInfo zoneBan))
                    {
                        if (zoneBan.Status == 1)
                        {
                            r2GKickOutPlayer.EndTime = zoneBan.BanEndTime;
                        }
                    }
                    scene.Fiber().Root.GetComponent<MessageSender>().Send(config.ActorId, r2GKickOutPlayer);
                }
            }
            
            string statusName = status switch
            {
                0 => "正常",
                1 => "封禁",
                2 => "冻结",
                _ => "未知"
            };
            HttpHelper.Response(context, new { Error = 0, Message = $"账号 {account} 状态已设为 {statusName}" });
            await ETTask.CompletedTask;
        }
    }
}