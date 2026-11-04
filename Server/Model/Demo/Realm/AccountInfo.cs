using System.Collections.Generic;

namespace ET.Server
{
    /// <summary>
    /// 账号信息
    /// Id 使用账号名的 hash 值
    /// </summary>
    [ChildOf(typeof(Scene))]
    public class AccountInfo  : Entity, IAwake
    {
        /// <summary>
        /// 账号名
        /// </summary>
        public string Account { get; set; }
        
        /// <summary>
        /// 密码 (允许为空，SDK登录会校验)
        /// </summary>
        public string Password { get; set; }
        
        /// <summary>
        /// 注册时间
        /// </summary>
        public long RegisterTime { get; set; }
        
        /// <summary>
        /// 注册Ip
        /// </summary>
        public string RegisterIP { get; set; }
        
        /// <summary>
        /// 注册渠道
        /// </summary>
        public string Channel { get; set; }
        
        /// <summary>
        /// 注册子渠道
        /// </summary>
        public string SubChannel { get; set; }
        
        /// <summary>
        /// 注册平台
        /// </summary>
        public string Platform { get; set; }
        
        /// <summary>
        /// 账号状态  0.正常，1封禁 2.冻结
        /// </summary>
        public int Status { get; set; }
        
        /// <summary>
        /// 封禁结束时间
        /// </summary>
        public long BanEndTime { get; set; }
        
        
        public Dictionary<int, ZoneBanInfo> ZoneBans { get; set; } = new();
    }
    
    /// <summary>
    /// 服务器用户封禁信息
    /// Id 使用账号名的 hash 值
    /// </summary>
    [ChildOf(typeof(Scene))]
    public class ZoneBanInfo  : Entity, IAwake
    {
        /// <summary>
        /// 账号状态  0.正常，1封禁 2.冻结
        /// </summary>
        public int Status { get; set; }
        
        /// <summary>
        /// 封禁结束时间
        /// </summary>
        public long BanEndTime { get; set; }
    }
}