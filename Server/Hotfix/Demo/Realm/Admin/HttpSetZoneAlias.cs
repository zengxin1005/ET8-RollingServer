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
            
            if (string.IsNullOrEmpty(request.QueryString["zone"]))
            {
                HttpHelper.Response(context, new { Error = 1, Message = "zone params are required" });
                return;
            }
            // 解析参数: /set_zone_status?zone=1&status=0
            int zone = int.Parse(request.QueryString["zone"] ?? "0");
            string alias = request.QueryString["alias"] ?? "";
            
            bool hasZone = StartZoneConfigCategory.Instance.DataList
                    .Any(m => m.Id == zone);
            if (!hasZone)
            {
                HttpHelper.Response(context, new { Error = 1, Message = "invalid zone" });
                return;
            }

            using (await scene.Root().GetComponent<CoroutineLockComponent>().Wait(CoroutineLockType.Zone, zone))
            {
                DBComponent dbComponent = scene.Root().GetComponent<DBManagerComponent>().GetZoneDB(scene.Zone());
                ZoneStatus zoneStatus = await dbComponent.Query<ZoneStatus>(zone);
                if (zoneStatus == null)
                {
                    zoneStatus = scene.Root().AddChildWithId<ZoneStatus>(zone);
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

            HttpHelper.Response(context, new { Error = 0, Message = $"zone {zone} alias set to {alias}" });
            await ETTask.CompletedTask;
        }
    }
}