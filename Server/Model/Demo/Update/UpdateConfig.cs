using System.Collections.Generic;

namespace ET.Server
{
    /// <summary>
    /// 热更新配置（从JSON文件加载，修改后自动热加载无需重启）
    /// </summary>
    [EnableClass]
    public class UpdateConfig
    {
        /// <summary>
        /// App版本列表（按版本号升序排列）
        /// 服务端只下发客户端当前版本之后的第一个版本
        /// </summary>
        public List<AppVersionItem> AppVersionList { get; } = new();

        /// <summary>
        /// 资源包列表
        /// </summary>
        public List<ResourceItem> ResourceList { get; } = new();
        
        
    }

    [EnableClass]
    public class AppVersionItem
    {
        /// <summary>
        /// App版本号
        /// </summary>
        public string Version { get; set; } = string.Empty;

        /// <summary>
        /// 是否强制更新（低于此版本必须强更）
        /// </summary>
        public bool IsForceUpdate { get; set; }

        /// <summary>
        /// 下载地址
        /// </summary>
        public string DownloadUrl { get; set; } = string.Empty;

        /// <summary>
        /// 更新描述
        /// </summary>
        public string UpdateDesc { get; set; } = string.Empty;
    }

    [EnableClass]
    public class ResourceItem
    {
        public int Version { get; set; }
        public long Size { get; set; }
        public string MD5 { get; set; } = string.Empty;
        public string DownloadUrl { get; set; } = string.Empty;
    }
}