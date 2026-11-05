using System;
using System.Net;
using System.Linq;

namespace ET.Server
{
    /// <summary>
    /// 热更新检查接口
    /// GET /Update/version?appVersion={ver}&platform={plat}&channel={ch}&resVersion={resVer}
    /// </summary>
    [HttpHandler(SceneType.Update, "/Update/version")]
    [FriendOf(typeof(UpdateComponent))]
    public class HttpGetUpdateHandler : IHttpHandler
    {
        public async ETTask Handle(Scene scene, HttpListenerContext context)
        {
            UpdateComponent updateComponent = scene.GetComponent<UpdateComponent>();

            // 响应：复用响应结构，只填充一个版本
            HttpGetUpdateResponse response =  HttpGetUpdateResponse.Create();

            string platform = context.Request.QueryString["platform"] ?? "";;
            string channel = context.Request.QueryString["channel"] ?? "";;
            string subChannel = context.Request.QueryString["subChannel"] ?? "";;
            

            string appVersion = context.Request.QueryString["appVersion"] ?? "0.0.0";

            // 校验appVersion格式：必须为 x.x 或者 x.x.x
            if (!IsValidAppVersion(appVersion))
            {
                HttpHelper.Response(context, new { Error = 1, Message = $"invalid appVersion format: {appVersion}, expected x.x or x.x.x" });
                return;
            }
            
            string ip = "";

            // 1. 优先从 X-Forwarded-For 头获取真实 IP
            string forwardedFor = context.Request.Headers["X-Forwarded-For"];
            if (!string.IsNullOrEmpty(forwardedFor))
            {
                // 取第一个 IP（最原始的客户端 IP）
                ip = forwardedFor.Split(',')[0].Trim();
            }
        
            // 2. 如果没有，尝试从 X-Real-IP 头获取
            if (string.IsNullOrEmpty(ip))
            {
                ip = context.Request.Headers["X-Real-IP"];
            }

            // 3. 如果都没有，则退回到直接连接的 IP
            if (string.IsNullOrEmpty(ip))
            {
                ip = context.Request.RemoteEndPoint.Address.ToString();
            }

            // 从AppVersion列表（Excel: UpdateConfig.xlsx@AppVersion）中找到客户端版本之后的版本
            // 规则：
            //   1. 如果后面的版本都是非强更，找到最高版本下发（可选更新）
            //   2. 如果后面有强更版本，找到最高的强更版本下发（必须先强更）
    
            AppVersion highestForceItem = null;
            AppVersion highestItem = null;

            foreach (AppVersion item in AppVersionCategory.Instance.DataList)
            {
                if (CompareVersion(appVersion, item.Version) >= 0)
                    continue;
                
                // 该资源版本在白名单控制中，且客户端IP不在白名单 → 跳过
                if ( !await updateComponent.IsIPWhitelisted(item.Version, ip))
                {
                    continue;
                }
                
                // 记录最高版本
                if (highestItem == null || CompareVersion(item.Version, highestItem.Version) > 0)
                {
                    highestItem = item;
                }

                // 记录最高的强更版本
                if (item.IsForceUpdate)
                {
                    if (highestForceItem == null || CompareVersion(item.Version, highestForceItem.Version) > 0)
                    {
                        highestForceItem = item;
                    }
                }
            }

            // 有强更版本 → 下发最高强更版本；否则下发最高版本
            response.AppVersion = highestForceItem ?? highestItem;
            
            int resVersion = 0;
            int.TryParse(context.Request.QueryString["resVersion"], out resVersion);
            foreach (Resource item in ResourceCategory.Instance.DataList)
            {
                if(resVersion >= item.Version)
                    continue;
                // 该资源版本在白名单控制中，且客户端IP不在白名单 → 跳过
                if (!await updateComponent.IsIPWhitelisted(item.Version.ToString(), ip))
                {
                    continue;
                }

                response.ResourceList.Add(item);
            }
            
            
            HttpHelper.Response(context, response);
            await ETTask.CompletedTask;
        }




        private static int CompareVersion(string v1, string v2)
        {
            string[] parts1 = v1.Split('.');
            string[] parts2 = v2.Split('.');
            int maxLen = Math.Max(parts1.Length, parts2.Length);
            for (int i = 0; i < maxLen; i++)
            {
                int num1 = i < parts1.Length && int.TryParse(parts1[i], out int n1) ? n1 : 0;
                int num2 = i < parts2.Length && int.TryParse(parts2[i], out int n2) ? n2 : 0;
                if (num1 < num2) return -1;
                if (num1 > num2) return 1;
            }
            return 0;
        }

        /// <summary>
        /// 校验版本号格式：必须为 x.x 或者 x.x.x（2~3 段非负整数）
        /// </summary>
        private static bool IsValidAppVersion(string version)
        {
            if (string.IsNullOrWhiteSpace(version))
                return false;
            string[] parts = version.Split('.');
            if (parts.Length < 2 || parts.Length > 3)
                return false;
            foreach (string p in parts)
            {
                // int.TryParse 会接受 "+1"/" 1"，这里只允许纯数字
                if (p.Length == 0 || !p.All(char.IsDigit))
                    return false;
            }
            return true;
        }
    }
}