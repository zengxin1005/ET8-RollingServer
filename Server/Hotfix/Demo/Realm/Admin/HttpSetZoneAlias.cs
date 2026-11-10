using System.Collections.Generic;
using System.Net;
using System.Linq;
namespace ET.Server
{
    [HttpHandler(SceneType.Realm, "/set_zone_alias")]
    public class HttpSetZoneAlias : IHttpHandler
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
            
            if (string.IsNullOrEmpty(request.QueryString["zoneId"]))
            {
                HttpHelper.Response(context, new { Error = 1, Message = "zoneId params are required" });
                return;
            }
            // 解析参数: /set_zone_status?zoneId=1&status=0
            int zoneId = int.Parse(request.QueryString["zoneId"] ?? "0");
            string alias = request.QueryString["alias"] ?? "";
            
            bool hasZone = StartZoneConfigCategory.Instance.DataList
                    .Any(m => m.Id == zoneId);
            if (!hasZone)
            {
                HttpHelper.Response(context, new { Error = 1, Message = "invalid zoneId" });
                return;
            }

            using (await scene.Root().GetComponent<CoroutineLockComponent>().Wait(CoroutineLockType.Zone, zoneId))
            {
                DBComponent dbComponent = scene.Root().GetComponent<DBManagerComponent>().GetZoneDB(scene.Zone());
                ZoneStatus zoneStatus = await dbComponent.Query<ZoneStatus>(zoneId);
                if (zoneStatus == null)
                {
                    zoneStatus = scene.Root().AddChildWithId<ZoneStatus>(zoneId);
                    zoneStatus.Alias = alias;
                    await dbComponent.Save(zoneStatus);
                    zoneStatus.Dispose();
                }
                else
                {
                    zoneStatus.Alias = alias;
                    await dbComponent.Save(zoneStatus);
                }
            }

            HttpHelper.Response(context, new { Error = 0, Message = $"zoneId {zoneId} alias set to {alias}" });
            await ETTask.CompletedTask;
        }
    }
}