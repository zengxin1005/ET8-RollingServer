using System.Collections.Generic;
using System.Linq;
using System.Net;

namespace ET.Server
{
    /// <summary>
    /// HTTP接口：管理IP白名单（按区服）
    /// 
    /// 添加IP：    /set_zone_whitelist?action=add&zoneId=1&ip=127.0.0.1
    /// 移除IP：    /set_zone_whitelist?action=remove&zoneId=1&ip=127.0.0.1
    /// 清除单服：  /set_zone_whitelist?action=clear&zoneId=1&ip=127.0.0.1
    /// 清除版本：  /set_zone_whitelist?action=clear&zoneId=1
    /// 清除所有：  /set_zone_whitelist?action=clear
    /// </summary>
    [HttpHandler(SceneType.Realm, "/set_zone_whitelist")]
    public class HttpSetZoneWhitelistHandler : IHttpHandler
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
            string action = request.QueryString["action"] ?? "";
            int zoneId = int.Parse(request.QueryString["zoneId"] ?? "0");
            string ip = request.QueryString["ip"] ?? "";
            if (!string.IsNullOrEmpty(request.QueryString["zoneId"]) || !string.IsNullOrEmpty(ip))
            {
                bool hasZone = StartZoneConfigCategory.Instance.DataList
                        .Any(m => m.Id == zoneId);
                if (!hasZone)
                {
                    HttpHelper.Response(context, new { Error = 1, Message = "invalid zoneId" });
                    return;
                }
            }
            if (string.IsNullOrEmpty(ip))
            {
                // 1. 优先从 X-Forwarded-For 头获取真实 IP
                string forwardedFor = request.Headers["X-Forwarded-For"];
                if (!string.IsNullOrEmpty(forwardedFor))
                {
                    // 取第一个 IP（最原始的客户端 IP）
                    ip = forwardedFor.Split(',')[0].Trim();
                }

                // 2. 如果没有，尝试从 X-Real-IP 头获取
                if (string.IsNullOrEmpty(ip))
                {
                    ip = request.Headers["X-Real-IP"];
                }

                // 3. 如果都没有，则退回到直接连接的 IP
                if (string.IsNullOrEmpty(ip))
                {
                    ip = request.RemoteEndPoint.Address.ToString();
                }
            }

            DBComponent dbComponent = scene.Root().GetComponent<DBManagerComponent>().GetZoneDB(scene.Zone());
            switch (action.ToLower())
            {
                case "add":
                    if (string.IsNullOrEmpty(ip) ||string.IsNullOrEmpty(request.QueryString["zoneId"]))
                    {
                        HttpHelper.Response(context, new { Error = 2, Message = "zoneId and ip params are required" });
                        return;
                    }
                    using (await scene.Root().GetComponent<CoroutineLockComponent>().Wait(CoroutineLockType.Zone, zoneId))
                    {
                        ZoneStatus zoneStatus = await dbComponent.Query<ZoneStatus>(zoneId);
                        if (zoneStatus == null)
                        {
                            zoneStatus = scene.Root().AddChildWithId<ZoneStatus>(zoneId);
                            zoneStatus.LoginWhiteIpList.Add(ip);
                            await dbComponent.Save(zoneStatus);
                            zoneStatus.Dispose();
                        }
                        else
                        {
                            zoneStatus.LoginWhiteIpList.Add(ip);
                            await dbComponent.Save(zoneStatus);
                        }
                    }
                    HttpHelper.Response(context, new
                    {
                        Error = 0,
                        Message = $"ip {ip} added to zoneId {zoneId} whitelist",
                        ZoneId = zoneId,
                        IP = ip
                    });
                    break;

                case "remove":
                    if (string.IsNullOrEmpty(ip) || string.IsNullOrEmpty(request.QueryString["zoneId"]))
                    {
                        HttpHelper.Response(context, new { Error = 2, Message = "zoneId and ip params are required" });
                        return;
                    }
                    using (await scene.Root().GetComponent<CoroutineLockComponent>().Wait(CoroutineLockType.Zone, zoneId))
                    {
                        ZoneStatus zoneStatus = await dbComponent.Query<ZoneStatus>(zoneId);
                        if (zoneStatus != null)
                        {
                            zoneStatus.LoginWhiteIpList.Remove(ip);
                            await dbComponent.Save(zoneStatus);
                        }
                    }
                    HttpHelper.Response(context, new
                    {
                        Error = 0,
                        Message = $"ip {ip} removed from zoneId {zoneId} whitelist",
                        ZoneId = zoneId,
                        IP = ip
                    });
                    break;

                case "clear":
                    if (!string.IsNullOrEmpty(request.QueryString["zoneId"]))
                    {
                        using (await scene.Root().GetComponent<CoroutineLockComponent>().Wait(CoroutineLockType.Zone, zoneId))
                        {
                            ZoneStatus zoneStatus = await dbComponent.Query<ZoneStatus>(zoneId);
                            if (zoneStatus != null)
                            {
                                zoneStatus.LoginWhiteIpList.Clear();
                                await dbComponent.Save(zoneStatus);
                            }
                        }
                        HttpHelper.Response(context, new
                        {
                            Error = 0,
                            Message = $"zoneId {zoneId} whitelist cleared",
                            ZoneId = zoneId
                        });
                    }
                    else
                    {
                        List<StartZoneConfig> zoneConfigs = StartZoneConfigCategory.Instance.DataList;
                        foreach (StartZoneConfig zoneConfig in zoneConfigs)
                        {
                            using (await scene.Root().GetComponent<CoroutineLockComponent>().Wait(CoroutineLockType.Zone, zoneConfig.Id))
                            {
                                ZoneStatus zoneStatus = await dbComponent.Query<ZoneStatus>(zoneConfig.Id);
                                if (zoneStatus != null)
                                {
                                    zoneStatus.LoginWhiteIpList.Clear();
                                    await dbComponent.Save(zoneStatus);
                                }
                            }
                        }
                        HttpHelper.Response(context, new
                        {
                            Error = 0,
                            Message = "all whitelists cleared"
                        });
                    }
                    break;
                default:
                    HttpHelper.Response(context, new { Error = 3, Message = "unknown action, use: add/remove/clear/list" });
                    break;
            }

            await ETTask.CompletedTask;
        }
    }
}