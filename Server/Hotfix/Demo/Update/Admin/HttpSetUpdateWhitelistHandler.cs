using System.Net;
using System.Collections.Generic;
namespace ET.Server
{
    /// <summary>
    /// HTTP接口：管理IP白名单（按资源版本）
    ///
    /// 添加IP：      /set_update_whitelist?action=add&version=3
    /// 移除IP：      /set_update_whitelist?action=remove&version=3
    /// 清除版本：    /set_update_whitelist?action=clear&version=3
    /// 清除所有：    /set_update_whitelist?action=clear
    /// </summary>
    [HttpHandler(SceneType.Update, "/set_update_whitelist")]
    [FriendOf(typeof(UpdateComponent))]
    public class HttpSetUpdateWhitelistHandler : IHttpHandler
    {
        public async ETTask Handle(Scene scene, HttpListenerContext context)
        {
            HttpListenerRequest request = context.Request;
            UpdateComponent updateComponent = scene.Root().GetComponent<UpdateComponent>();
            
            // 1. 从 URL 参数或 Header 中获取 token
            string token = request.QueryString["token"] ?? request.Headers["X-API-Key"];

            // 2. 比对固定密钥（可从配置读取）
            if (string.IsNullOrEmpty(token) || token != ConstValue.AdminApiKey)
            {
                HttpHelper.Response(context, new { Error = 403, Message = "Unauthorized" });
                return;
            }
            
            
            string action = request.QueryString["action"] ?? "";
            string version = request.QueryString["version"] ?? "";
            string ip = request.QueryString["ip"] ?? "";
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


            switch (action.ToLower())
            {
                case "add":
                    if (string.IsNullOrEmpty(ip) || string.IsNullOrEmpty(version))
                    {
                        HttpHelper.Response(context, new { Error = 2, Message = "version and ip params are required" });
                        return;
                    }
                    await updateComponent.AddWhitelistIP(version, ip);
                    HttpHelper.Response(context, new
                    {
                        Error = 0,
                        Message = $"ip {ip} added to version {version} whitelist",
                        Version = version,
                        IP = ip
                    });
                    break;

                case "remove":
                    if (string.IsNullOrEmpty(ip) || string.IsNullOrEmpty(version))
                    {
                        HttpHelper.Response(context, new { Error = 2, Message = "version and ip params are required" });
                        return;
                    }
                    await updateComponent.RemoveWhitelistIP(version, ip);
                    HttpHelper.Response(context, new
                    {
                        Error = 0,
                        Message = $"ip {ip} removed from version {version} whitelist",
                        Version = version,
                        IP = ip
                    });
                    break;

                case "clear":
                    if (!string.IsNullOrEmpty(version))
                    {
                        await updateComponent.ClearWhitelist(version);
                        HttpHelper.Response(context, new
                        {
                            Error = 0,
                            Message = $"version {version} whitelist cleared",
                            Version = version
                        });
                    }
                    else
                    {
                        await updateComponent.ClearAllWhitelist();
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