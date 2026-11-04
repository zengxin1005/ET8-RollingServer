using System.Linq;
using System.Net;

namespace ET.Server
{
    [HttpHandler(SceneType.Realm, "/set_zone_open_time")]
    public class HttpSetZoneOpenTimeHandler : IHttpHandler
    {
        public async ETTask Handle(Scene scene, HttpListenerContext context)
        {
            HttpListenerRequest request = context.Request;

            // 1. 从 URL 参数或 Header 中获取 token
            string token = request.QueryString["token"] ?? request.Headers["X-API-Key"];
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

            int zone = int.Parse(request.QueryString["zone"] ?? "0");
            bool hasZone = StartZoneConfigCategory.Instance.DataList.Any(m => m.Id == zone);
            if (!hasZone)
            {
                HttpHelper.Response(context, new { Error = 1, Message = "invalid zone" });
                return;
            }

            if (string.IsNullOrEmpty(request.QueryString["openTime"]))
            {
                HttpHelper.Response(context, new { Error = 1, Message = "openTime params are required (UTC ms, 0 = 不限制)" });
                return;
            }
            
            long openTime = long.Parse(request.QueryString["openTime"]);
            if (openTime < 0)
            {
                HttpHelper.Response(context, new { Error = 1, Message = "openTime must be >= 0" });
                return;
            }

            using (await scene.Root().GetComponent<CoroutineLockComponent>().Wait(CoroutineLockType.Zone, zone))
            {
                DBComponent dbComponent = scene.Root().GetComponent<DBManagerComponent>().GetZoneDB(scene.Zone());
                ZoneStatus zoneStatus = await dbComponent.Query<ZoneStatus>(zone);
                if (zoneStatus == null)
                {
                    zoneStatus = scene.Root().AddChildWithId<ZoneStatus>(zone);
                    zoneStatus.OpenTime = openTime;
                    await dbComponent.Save(zoneStatus);
                    zoneStatus.Dispose();
                }
                else
                {
                    zoneStatus.OpenTime = openTime;
                    await dbComponent.Save(zoneStatus);
                }
            }
            
            HttpHelper.Response(context, new { Error = 0, Message = $"zone {zone} OpenTime set to {openTime}" });
            await ETTask.CompletedTask;
        }
    }
}
