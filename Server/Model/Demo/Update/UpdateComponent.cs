namespace ET.Server
{
    /// <summary>
    /// 热更新组件，管理客户端热更新配置
    /// 配置本体（整包版本 / 热更资源）在 AppVersionCategory / ResourceCategory，由 ConfigLoader 统一加载；
    /// 这里只挂 UpdateVersion 白名单子实体（DB 持久化）。
    /// </summary>
    [ComponentOf(typeof(Scene))]
    public class UpdateComponent : Entity, IAwake
    {
    }
}
