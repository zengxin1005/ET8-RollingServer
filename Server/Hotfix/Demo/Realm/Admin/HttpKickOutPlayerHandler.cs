using System.Collections.Generic;
using System.Linq;
using System.Net;

namespace ET.Server
{
    /// <summary>
    /// HTTP接口：踢角色 （按区服 按账号）
    /// 
    /// 踢单人：    /kickout_player?zone=1&amp;account=zenggege
    /// 全服维护：  /kickout_player?zone=1&amp;endTime=1730000000
    /// endTime: 截止时间戳（秒），0=无截止；不传等价于 0，客户端按"无结束时间"展示
    ///          注意 HTTP 入参是秒，内部统一转毫秒后透传给 G2C_KickOut.EndTime
    /// </summary>
    [HttpHandler(SceneType.Realm, "/kickout_player")] 
    public class HttpKickOutPlayerHandler : IHttpHandler
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
       
            int zone = int.Parse(request.QueryString["zone"] ?? "0");
            string account = request.QueryString["account"] ?? "";
            long endTime = long.Parse(request.QueryString["endTime"] ?? "0");
            if (!string.IsNullOrEmpty(request.QueryString["zone"]) || !string.IsNullOrEmpty(account))
            {
                bool hasZone = StartZoneConfigCategory.Instance.DataList
                        .Any(m => m.Id == zone);
                if (!hasZone)
                {
                    HttpHelper.Response(context, new { Error = 1, Message = "invalid zone" });
                    return;
                }
            }
            StartSceneConfig config = RealmGateAddressHelper.GetGate(scene.Zone(), account);
            R2G_KickOutPlayer r2GKickOutPlayer = R2G_KickOutPlayer.Create();
            r2GKickOutPlayer.Account = account;
            r2GKickOutPlayer.ZoneId = zone;
            r2GKickOutPlayer.EndTime = endTime;
            scene.Fiber().Root.GetComponent<MessageSender>().Send(config.ActorId, r2GKickOutPlayer);
            HttpHelper.Response(context, new { Error = 0, Message = "kickout player success" });
            await ETTask.CompletedTask;
        }
    }
}