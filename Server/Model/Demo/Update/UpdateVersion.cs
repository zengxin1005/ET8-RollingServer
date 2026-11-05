using System.Collections.Generic;
namespace ET.Server
{
    /// <summary>
    /// 区服状态（MongoDB持久化）
    /// </summary>
    [ChildOf(typeof(UpdateComponent))]
    public class UpdateVersion : Entity, IAwake
    {
        public string Version { get; set; }

        /// <summary>
        /// 资源版本白名单
        /// </summary>
        public HashSet<string> WhitelistIPs { get; set; } = new();

    }
}