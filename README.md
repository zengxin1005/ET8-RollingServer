# ET8-RollingServer

> 🚀 **基于 ET8 打造的高度可扩展滚服/合服游戏服务端基础设施**。
> 剥离了具体的业务逻辑，直接提供商业级游戏所需的**一切滚服架构底层支持**。开箱即用，助你快速搭建稳定、高并发的分布式游戏后端。

---

## ✨ 核心特性 (Core Features)

本项目提供了一套完整的滚服游戏基础设施，涵盖从服务器运维、账号安全、分区隔离到断线重连的全链路解决方案：

### 🌐 1. 动态运维与热更新 (Hot Update & Ops)
*   **热更新服务器**：支持不停机热更新核心逻辑与状态。
*   **热更白名单控制**：精细控制特定玩家或测试账号优先体验热更内容。
*   **运维控制接口钩子 (Hooks)**：提供丰富的生命周期事件钩子，完美对接外部运维/运营后台系统。

### 🗺️ 2. 滚服与分区隔离 (Rolling Server & Isolation)
*   **服务器列表动态下发**：支持滚服列表、分区隔离，数据互不影响。
*   **全维度区服状态管理**：
    *   支持维护状态、禁止注册、新服标记、开服时间。
    *   支持负载监控、推荐权重、服务器冠名。
    *   支持区服级 IP 白名单及相关运维控制接口。

### 🔐 3. 账号与安全控制 (Account & Security)
*   **多层级控制叠加**：支持**账号级别**（冻结/封禁）与**服务器级别**双重控制拦截。
*   **完整登录链路**：包含 SDK 校验、登录、选服、创角、顶号处理的完整闭环。
*   **玩家数据管理**：安全的数据管理与跨服/全服广播通讯机制。

### 🔄 4. 会话与网络高可用 (Session & Network)
*   **断线重连/重登逻辑**：内置优雅的断线重连机制，保障玩家在线体验。
*   **并发原子性与幂等保证**：基于 ET8 的协程锁与 Actor 模型，保证高并发下的数据准确性。
*   **断线落库保护**：确保极端网络环境下玩家数据不丢失，断线安全落库。
*   **异常边界保护**：保护消息逻辑边界的健壮性。
*   **强用户体验**：任何操作均有日志与精确状态反馈。

### 🛡️ 5. 本地化与合规 (Localization & Compliance)
*   **敏感字检测**：内置高性能多叉树敏感词过滤，适合国内合规要求。
*   **随机取名库**：附带符合国内习惯的随机名字生成系统。

---


## 运营/运维接口: Realm/Admin  Update/Admin


* **Realm**:账号校验/状态校验，服务器状态校验，登录，SDK校验，选服，服务器拉取(服务器配置规则直接取ZoneConfig)等。
* **Gate**:登录绑定连接，顶号处理，创角，临界状态验证，进入地图，下线逻辑，消息转发等。
* **Update**: 所有热更新逻辑，只支持补丁包模式，同时支持资源包跟安装包，H5单文件热更新模式请结合清单文件直接使用对象存储。
* **Notice**: 公告系统，支持自定义Excel表格配置，支持H5页面配置。

## 未来有时间我会把相应的客户端配套代码全部Push。


## 💡 设计理念 (Philosophy)

> **“除了业务逻辑没有，一切的滚服架构游戏的基础设施全有！”**

本项目致力于解决游戏开发中最繁琐、最容易出 Bug 的底层基建问题。你只需要专注于编写你的核心玩法（如战斗、背包、关卡），剩下的滚服架构的基础设施统统交给它。

目前只在Localhost进行参考配置，请在IDE运行Debug模式。

鸣谢：

https://github.com/Alex-Rachel/GameNetty.git  感谢Alex-Rachel对ET8进行了前后端分离， 本项目也是基于其main分支。

https://github.com/egametang/ET   感谢熊猫大大开源的ET框架。


相关ET文档请查阅这两个开源项目。



<img width="645" height="930" alt="img_v3_0216v_06eaa8ae-1a1e-44eb-8612-1a8b7a2803cg" src="https://github.com/user-attachments/assets/5d7688af-1042-4037-89b9-c2c0e2abce1c" />

<img width="702" height="958" alt="img_v3_0216v_ea3b0681-da98-46fc-864a-3fc7840460dg" src="https://github.com/user-attachments/assets/fd0b80ba-fb6d-4264-aa8c-d0ad2311676f" />


<img width="692" height="930" alt="img_v3_0216v_a446dca6-f282-4f4a-8d1a-f4047103b5ag" src="https://github.com/user-attachments/assets/51c4a3cb-3da0-428f-ad14-915c5e57db1e" />


<img width="639" height="1074" alt="img_v3_0216v_f3f2eb1f-534c-422c-934c-7405725f4bcg" src="https://github.com/user-attachments/assets/fe48803b-2ef5-4043-9d10-7caf5f85850b" />


<img width="655" height="861" alt="b7219cccdacc914e31e3f7dc9c8d5c5c" src="https://github.com/user-attachments/assets/6e2f60a8-2b01-4311-93ff-1412f7d696b7" />
