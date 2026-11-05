using System.Collections.Generic;

namespace ET.Server
{
    [EnableClass]
    public class HttpGetUpdateResponse
    {
        /// <summary>
        /// APP版本
        /// </summary>
        public AppVersion AppVersion { get; set; }

        /// <summary>
        /// 资源包列表
        /// </summary>
        public List<Resource> ResourceList { get; private set; } = new();

        
        public static HttpGetUpdateResponse Create(bool isFromPool = false)
        {
            return ObjectPool.Instance.Fetch(typeof(HttpGetUpdateResponse), isFromPool) as HttpGetUpdateResponse;
        }
    }
}