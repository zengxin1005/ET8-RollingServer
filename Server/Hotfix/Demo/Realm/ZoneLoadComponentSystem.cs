using System.Collections.Generic;

namespace ET.Server
{
    [EntitySystemOf(typeof(ZoneLoadComponent))]
    public static partial class ZoneLoadComponentSystem
    {
        /// <summary>刷新间隔。选服列表对实时性要求不高，10 秒足够，也把 N 台 Gate 的 RPC 摊平。</summary>
        private const int RefreshIntervalMs = 10000;

        
        [EntitySystem]
        private static void Awake(this ZoneLoadComponent self)
        {
            self.RefreshLoop().Coroutine();
        }

        [EntitySystem]
        private static void Destroy(this ZoneLoadComponent self)
        {
        }
        /// <summary>
        /// 常驻刷新循环：拉一轮 → 等 5s → 再拉，退出条件是组件被销毁。
        /// </summary>
        private static async ETTask RefreshLoop(this ZoneLoadComponent self)
        {
            TimerComponent timerComponent = self.Root().GetComponent<TimerComponent>();
            while (!self.IsDisposed)
            {
                await self.Refresh();

                if (self.IsDisposed)
                {
                    return;
                }

                await timerComponent.WaitAsync(RefreshIntervalMs);
            }
        }

        /// <summary>
        /// 拉一轮：并发问所有 Gate，把各 Gate 报的 zoneId→人数 累加，然后**整体替换**缓存。
        /// </summary>
        private static async ETTask Refresh(this ZoneLoadComponent self)
        {
            Dictionary<int, int> counts = new();
            List<ETTask> tasks = new();

            foreach (List<StartSceneConfig> gates in StartSceneConfigCategory.Instance.Gates.Values)
            {
                foreach (StartSceneConfig gateConfig in gates)
                {
                    tasks.Add(FetchFromGate(self, gateConfig, counts));
                }
            }

            await ETTaskHelper.WaitAll(tasks);

            self.OnlineCounts = counts;
        }

        /// <summary>
        /// 单台 Gate 的拉取。失败（进程没起来 / 重启中 / 超时）只当这台本轮 0 人，不影响整轮其他 Gate。
        /// </summary>
        private static async ETTask FetchFromGate(this ZoneLoadComponent self, StartSceneConfig gateConfig, Dictionary<int, int> counts)
        {
            MessageSender messageSender = self.Root().GetComponent<MessageSender>();
            G2R_GetZoneLoad response = (G2R_GetZoneLoad)await messageSender.Call(
                gateConfig.ActorId, R2G_GetZoneLoad.Create(), needException: false);
            foreach (ZoneOnlineCount zoneOnlineCount in response.Zones)
            {
                counts.TryGetValue(zoneOnlineCount.ZoneId, out int count);
                counts[zoneOnlineCount.ZoneId] = count + zoneOnlineCount.Count;
            }
        }

        /// <summary>
        /// 取某区服的实时负载档位：0=流畅，1=繁忙，2=爆满。
        /// 没拉到数据（Gate 还没起）按 0 处理，不让选服列表因为"暂时没人报"就显示爆满。
        /// </summary>
        public static int GetLoad(this ZoneLoadComponent self, int zoneId)
        {
            int playerCount = self.GetOnlineCount(zoneId);
            if (playerCount < ZoneLoadComponent.SmoothMax)
            {
                return 0;
            }
            return playerCount < ZoneLoadComponent.BusyMax? 1 : 2;
        }

        /// <summary>取某区服的全集群在线人数（多台 Gate 累加后）。没有数据 = 0。</summary>
        private static int GetOnlineCount(this ZoneLoadComponent self, int zoneId)
        {
            return self.OnlineCounts.GetValueOrDefault(zoneId);
        }
        
        /// <summary>
        /// 单服人数上限
        /// </summary>
        public static bool IsOverCapacity(this ZoneLoadComponent self, int zoneId)
        {
            return self.GetOnlineCount(zoneId) > ZoneLoadComponent.Capacity;
        }
    }
}
