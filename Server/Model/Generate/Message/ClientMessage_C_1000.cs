using MemoryPack;
using System.Collections.Generic;

namespace ET
{
    [MemoryPackable]
    [Message(ClientMessage.Main2NetClient_Login)]
    [ResponseType(nameof(NetClient2Main_Login))]
    public partial class Main2NetClient_Login : MessageObject, IRequest
    {
        public static Main2NetClient_Login Create(bool isFromPool = false)
        {
            return ObjectPool.Instance.Fetch(typeof(Main2NetClient_Login), isFromPool) as Main2NetClient_Login;
        }

        [MemoryPackOrder(0)]
        public int RpcId { get; set; }

        [MemoryPackOrder(1)]
        public int OwnerFiberId { get; set; }

        /// <summary>
        /// 账号
        /// </summary>
        [MemoryPackOrder(2)]
        public string Account { get; set; }

        /// <summary>
        /// 密码
        /// </summary>
        [MemoryPackOrder(3)]
        public string Password { get; set; }

        /// <summary>
        /// 选择的区服ID
        /// </summary>
        [MemoryPackOrder(3)]
        public int ZoneId { get; set; }

        /// <summary>
        /// 平台: iOS/Android/PC
        /// </summary>
        [MemoryPackOrder(4)]
        public string Platform { get; set; }

        /// <summary>
        /// 渠道: WeChat/QQ/AppStore
        /// </summary>
        [MemoryPackOrder(5)]
        public string Channel { get; set; }

        /// <summary>
        /// 子渠道
        /// </summary>
        [MemoryPackOrder(6)]
        public string SubChannel { get; set; }

        public override void Dispose()
        {
            if (!this.IsFromPool)
            {
                return;
            }

            this.RpcId = default;
            this.OwnerFiberId = default;
            this.Account = default;
            this.Password = default;
            this.ZoneId = default;
            this.Platform = default;
            this.Channel = default;
            this.SubChannel = default;

            ObjectPool.Instance.Recycle(this);
        }
    }

    [MemoryPackable]
    [Message(ClientMessage.NetClient2Main_Login)]
    public partial class NetClient2Main_Login : MessageObject, IResponse
    {
        public static NetClient2Main_Login Create(bool isFromPool = false)
        {
            return ObjectPool.Instance.Fetch(typeof(NetClient2Main_Login), isFromPool) as NetClient2Main_Login;
        }

        [MemoryPackOrder(0)]
        public int RpcId { get; set; }

        [MemoryPackOrder(1)]
        public int Error { get; set; }

        [MemoryPackOrder(2)]
        public string Message { get; set; }

        [MemoryPackOrder(3)]
        public long PlayerId { get; set; }

        public override void Dispose()
        {
            if (!this.IsFromPool)
            {
                return;
            }

            this.RpcId = default;
            this.Error = default;
            this.Message = default;
            this.PlayerId = default;

            ObjectPool.Instance.Recycle(this);
        }
    }

    [MemoryPackable]
    [Message(ClientMessage.Main2NetClient_GetServerList)]
    [ResponseType(nameof(NetClient2Main_GetServerList))]
    public partial class Main2NetClient_GetServerList : MessageObject, IRequest
    {
        public static Main2NetClient_GetServerList Create(bool isFromPool = false)
        {
            return ObjectPool.Instance.Fetch(typeof(Main2NetClient_GetServerList), isFromPool) as Main2NetClient_GetServerList;
        }

        [MemoryPackOrder(0)]
        public int RpcId { get; set; }

        [MemoryPackOrder(1)]
        public int OwnerFiberId { get; set; }

        /// <summary>
        /// 帐号
        /// </summary>
        [MemoryPackOrder(2)]
        public string Account { get; set; }

        /// <summary>
        /// 平台: iOS/Android/PC
        /// </summary>
        [MemoryPackOrder(3)]
        public string Platform { get; set; }

        /// <summary>
        /// 渠道: WeChat/QQ/AppStore
        /// </summary>
        [MemoryPackOrder(4)]
        public string Channel { get; set; }

        /// <summary>
        /// 子渠道
        /// </summary>
        [MemoryPackOrder(5)]
        public string SubChannel { get; set; }

        public override void Dispose()
        {
            if (!this.IsFromPool)
            {
                return;
            }

            this.RpcId = default;
            this.OwnerFiberId = default;
            this.Account = default;
            this.Platform = default;
            this.Channel = default;
            this.SubChannel = default;

            ObjectPool.Instance.Recycle(this);
        }
    }

    [MemoryPackable]
    [Message(ClientMessage.NetClient2Main_GetServerList)]
    public partial class NetClient2Main_GetServerList : MessageObject, IResponse
    {
        public static NetClient2Main_GetServerList Create(bool isFromPool = false)
        {
            return ObjectPool.Instance.Fetch(typeof(NetClient2Main_GetServerList), isFromPool) as NetClient2Main_GetServerList;
        }

        [MemoryPackOrder(0)]
        public int RpcId { get; set; }

        [MemoryPackOrder(1)]
        public int Error { get; set; }

        [MemoryPackOrder(2)]
        public string Message { get; set; }

        [MemoryPackOrder(3)]
        public List<ServerInfo> ServerList { get; set; } = new();

        [MemoryPackOrder(4)]
        public List<UnitInfo> UnitInfoList { get; set; } = new();

        public override void Dispose()
        {
            if (!this.IsFromPool)
            {
                return;
            }

            this.RpcId = default;
            this.Error = default;
            this.Message = default;
            this.ServerList.Clear();
            this.UnitInfoList.Clear();

            ObjectPool.Instance.Recycle(this);
        }
    }

    public static class ClientMessage
    {
        public const ushort Main2NetClient_Login = 1001;
        public const ushort NetClient2Main_Login = 1002;
        public const ushort Main2NetClient_GetServerList = 1003;
        public const ushort NetClient2Main_GetServerList = 1004;
    }
}