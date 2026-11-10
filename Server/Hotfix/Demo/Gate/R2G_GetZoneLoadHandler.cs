using System.Linq;

namespace ET.Server
{
    /// <summary>
    /// Realm 定时来拉本 Gate 的在线人数（回调），直接把整个字典回给它。
    /// 一台 Gate 可承载多个区服，所以一次响应里会带多条。
    /// </summary>
    [MessageHandler(SceneType.Gate)]
    public class R2G_GetZoneLoadHandler : MessageHandler<Scene, R2G_GetZoneLoad, G2R_GetZoneLoad>
    {
        protected override async ETTask Run(Scene scene, R2G_GetZoneLoad request, G2R_GetZoneLoad response)
        {
            foreach (IGrouping<int, Player> group in scene.GetComponent<PlayerComponent>().Children.Values
                    .OfType<Player>()
                    .Where(IsOnline)
                    .GroupBy(player => player.ZoneId))
            {
                ZoneOnlineCount zoneOnlineCount = ZoneOnlineCount.Create();
                zoneOnlineCount.ZoneId = group.Key;
                zoneOnlineCount.Count = group.Count();
                response.Zones.Add(zoneOnlineCount);
            }

            await ETTask.CompletedTask;
        }

        /// <summary>在线判据：Session 存在且没被销毁。Session 为空 = 已下线或正在断开。</summary>
        private static bool IsOnline(Player player)
        {
            return !player.IsDisposed
                    && player.ZoneId > 0
                    && player.GetComponent<PlayerSessionComponent>()?.Session is { IsDisposed: false };
        }
    }
}
