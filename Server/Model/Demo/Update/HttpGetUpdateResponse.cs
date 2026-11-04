using System.Collections.Generic;

namespace ET.Server
{
    [EnableClass]
    public class HttpGetUpdateResponse
    {
        /// <summary>
        /// APP版本
        /// </summary>
        public AppVersionItem AppVersion { get;} = new();

        /// <summary>
        /// 资源包列表
        /// </summary>
        public List<ResourceItem> ResourceList { get; } = new();

        
        public static HttpGetUpdateResponse Create(bool isFromPool = false)
        {
            return ObjectPool.Instance.Fetch(typeof(HttpGetUpdateResponse), isFromPool) as HttpGetUpdateResponse;
        }
    }
}