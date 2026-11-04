using System.Collections.Generic;

namespace ET.Server
{
    /// <summary>
    /// 区服状态（MongoDB持久化）
    /// </summary>
    [ChildOf(typeof(Scene))]
    public class ZoneStatus : Entity, IAwake
    {
        /// <summary>
        /// 硬状态：0=正常, 1=维护 , 2=禁止创角
        /// </summary>
        public int Status { get; set; }

        /// <summary>
        /// 负载状态：0=流畅，1=繁忙，2=爆满
        /// </summary>
        public int Load { get; set; }  //todo  改成动态监控
        
        /// <summary>
        /// 是否新服
        /// </summary>
        public bool IsNewZone  { get; set; }
        
        /// <summary>
        /// 推荐权重
        /// </summary>
        public int RecommendWeight{ get; set; }

        /// <summary>
        /// 开服时间
        /// </summary>
        public long OpenTime { get; set; }

        /// <summary>
        /// 冠名
        /// </summary>
        public string Alias{ get; set; }
        

        /// <summary>
        /// 登录白名单列表
        /// </summary>
        public HashSet<string> LoginWhiteIpList { set; get; } = new();
        
    }
}