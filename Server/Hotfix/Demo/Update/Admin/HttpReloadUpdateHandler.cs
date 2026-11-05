using System.Net;

namespace ET.Server
{
    /// <summary>
    /// 强制重载热更新配置（Excel 导表后不用重启）
    /// GET /reload_update?token=xxx      也支持 Header: X-API-Key
    /// </summary>
    [HttpHandler(SceneType.Update, "/reload_update")]
    public class HttpReloadUpdateHandler : IHttpHandler
    {
        public async ETTask Handle(Scene scene, HttpListenerContext context)
        {
            // 与管理端其它接口一致的鉴权：重载会替换全局配置单例，不能让外部随意触发
            string token = context.Request.QueryString["token"] ?? context.Request.Headers["X-API-Key"];
            if (string.IsNullOrEmpty(token) || token != ConstValue.AdminApiKey)
            {
                HttpHelper.Response(context, new { Error = 403, Message = "Unauthorized" });
                return;
            }

            // 重新读 ../Config/Generate/AppVersionCategory.bytes、ResourceCategory.bytes，重建 Category 单例
            // World.AddSingleton 是覆盖式写入（singletons[type] = singleton），并发重载不会抛异常
            await ConfigLoader.Instance.Reload(typeof(AppVersionCategory));
            await ConfigLoader.Instance.Reload(typeof(ResourceCategory));
            HttpHelper.Response(context, new { Error = 0, Message = "reload success" });
        }
    }
}
