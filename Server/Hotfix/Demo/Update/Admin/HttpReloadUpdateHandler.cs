using System.Net;

namespace ET.Server
{
    /// <summary>
    /// 强制重载热更新配置
    /// GET /Update/reload
    /// </summary>
    [HttpHandler(SceneType.Update, "/reload_update")]
    public class HttpReloadUpdateHandler : IHttpHandler
    {
        public async ETTask Handle(Scene scene, HttpListenerContext context)
        {
            UpdateComponent updateComponent = scene.GetComponent<UpdateComponent>();
            updateComponent.LoadConfig();
            HttpHelper.Response(context, new { Error = 0, Message = "reload success" });
            await ETTask.CompletedTask;
        }
    }
}