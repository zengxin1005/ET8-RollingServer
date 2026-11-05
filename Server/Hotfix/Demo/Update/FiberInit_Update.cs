using System.Net;

namespace ET.Server
{
    [Invoke((long)SceneType.Update)]
    public class FiberInit_Update: AInvokeHandler<FiberInit, ETTask>
    {
        public override async ETTask Handle(FiberInit fiberInit)
        {
            Scene root = fiberInit.Fiber.Root;
            StartSceneConfig startSceneConfig = StartSceneConfigCategory.Instance.Get((int)root.Id);
            root.AddComponent<HttpComponent, string>($"http://*:{startSceneConfig.Port}/");
            root.AddComponent<DBManagerComponent>();
            root.AddComponent<CoroutineLockComponent>();
            UpdateComponent updateComponent = root.AddComponent<UpdateComponent>();
            await updateComponent.LoadAllVersion();
            await ETTask.CompletedTask;
        }
    }
}