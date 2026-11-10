using System.Collections.Generic;

namespace ET.Server
{
    /// <summary>
    /// 区服实时负载
    /// </summary>
    [ComponentOf(typeof(Scene))]
    public class ZoneLoadComponent: Entity, IAwake, IDestroy
    {
        // ======================== 分档阈值（人数阶梯写死，不走配置表）========================
        //
        // 口径：按「在线人数占单服设计承载的水位」分三档，这是选服列表（流畅/繁忙/爆满）行业通用的做法 ——
        //   水位 < 1/3  → 流畅（服务器很闲，推荐进）
        //   1/3 ~ 4/5   → 繁忙（正常，但高峰期可能排队/卡顿）
        //   ≥ 4/5       → 爆满（提示玩家换服，**不阻止登录**）
        //
        // 数字来源：SLG 手游单服设计承载（在线峰值）按 3000 算 —— 这是 SLG 品类的常见规格，
        // 再高就得上分线/合服，再低说明服开得太碎。所以把 3000 的 1/3 和 4/5 落成常量。

        /// <summary>单服设计承载（在线峰值）。只用于解释下面两个阈值的由来，代码不读它。</summary>
        public const int Capacity = 3000;

        /// <summary>流畅上限：在线人数 &lt; 1000（≈ 承载 1/3）。</summary>
        public const int SmoothMax = 1000;

        /// <summary>繁忙上限：1000 ≤ 在线人数 &lt; 2400（≈ 承载 4/5）。到 2400 就是爆满。</summary>
        public const int BusyMax = 2400;

        /// <summary>
        /// zoneId → 该区服全集群在线人数（多台 Gate 累加后）。
        /// 一个区服可能被多台 Gate 同时承载（Realm 按账号 hash 分流），Realm 侧累加成一份。
        /// </summary>
        public Dictionary<int, int> OnlineCounts { get; set; } = new();
    }
}
