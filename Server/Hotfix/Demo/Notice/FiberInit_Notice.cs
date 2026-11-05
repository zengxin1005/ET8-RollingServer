namespace ET.Server
{
    [Invoke((long)SceneType.Notice)]
    public class FiberInit_Notice: AInvokeHandler<FiberInit, ETTask>
    {
        public override async ETTask Handle(FiberInit fiberInit)
        {
            Scene root = fiberInit.Fiber.Root;
            StartSceneConfig startSceneConfig = StartSceneConfigCategory.Instance.Get((int)root.Id);
            root.AddComponent<HttpComponent, string>($"http://*:{startSceneConfig.Port}/");
            await ETTask.CompletedTask;
        }
    }
}