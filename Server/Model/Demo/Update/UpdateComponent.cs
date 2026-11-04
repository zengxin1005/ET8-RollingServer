using System.Collections.Generic;

namespace ET.Server
{
    /// <summary>
    /// 热更新组件，管理客户端热更新配置
    /// </summary>
    [ComponentOf(typeof(Scene))]
    public class UpdateComponent : Entity, IAwake
    {
        public UpdateConfig UpdateConfig { get; set; }
    }
}
