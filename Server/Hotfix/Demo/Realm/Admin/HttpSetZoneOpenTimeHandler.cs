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

            if (string.IsNullOrEmpty(request.QueryString["zoneId"]))
            {
                HttpHelper.Response(context, new { Error = 1, Message = "zoneId params are required" });
                return;
            }

            int zoneId = int.Parse(request.QueryString["zoneId"] ?? "0");
            bool hasZone = StartZoneConfigCategory.Instance.DataList.Any(m => m.Id == zoneId);
            if (!hasZone)
            {
                HttpHelper.Response(context, new { Error = 1, Message = "invalid zoneId" });
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

            using (await scene.Root().GetComponent<CoroutineLockComponent>().Wait(CoroutineLockType.Zone, zoneId))
            {
                DBComponent dbComponent = scene.Root().GetComponent<DBManagerComponent>().GetZoneDB(scene.Zone());
                ZoneStatus zoneStatus = await dbComponent.Query<ZoneStatus>(zoneId);
                if (zoneStatus == null)
                {
                    zoneStatus = scene.Root().AddChildWithId<ZoneStatus>(zoneId);
                    zoneStatus.OpenTime = openTime * 1000;
                    await dbComponent.Save(zoneStatus);
                    zoneStatus.Dispose();
                }
                else
                {
                    zoneStatus.OpenTime = openTime * 1000;
                    await dbComponent.Save(zoneStatus);
                }
            }
            
            HttpHelper.Response(context, new { Error = 0, Message = $"zoneId {zoneId} OpenTime set to {openTime}" });
            await ETTask.CompletedTask;
        }
    }
}
