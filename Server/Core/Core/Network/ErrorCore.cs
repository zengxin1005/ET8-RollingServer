namespace ET
{
    [UniqueId(100000, 500000)]
    public static class ErrorCore
    {
        public const int ERR_MyErrorCode = 110000;
        
        public const int ERR_KcpConnectTimeout = 100205;
        public const int ERR_KcpAcceptTimeout = 100206;
        public const int ERR_KcpReadWriteTimeout = 100207;
        public const int ERR_PeerDisconnect = 100208;
        public const int ERR_SocketCantSend = 100209;
        public const int ERR_SocketError = 100210;
        public const int ERR_KcpWaitSendSizeTooLarge = 100211;
        public const int ERR_KcpCreateError = 100212;
        public const int ERR_SendMessageNotFoundTChannel = 100213;
        public const int ERR_TChannelRecvError = 100214;
        public const int ERR_MessageSocketParserError = 100215;
        public const int ERR_KcpNotFoundChannel = 100216;

        public const int ERR_WebsocketSendError = 100217;
        public const int ERR_WebsocketPeerReset = 100218;
        public const int ERR_WebsocketMessageTooBig = 100219;
        public const int ERR_WebsocketRecvError = 100220;
        
        public const int ERR_KcpReadNotSame = 100230;
        public const int ERR_KcpSplitError = 100231;
        public const int ERR_KcpSplitCountError = 100232;

        public const int ERR_MessageLocationSenderTimeout = 110004;
        public const int ERR_PacketParserError = 110005;
        public const int ERR_KcpChannelAcceptTimeout = 110206;
        public const int ERR_KcpRemoteDisconnect = 110207;
        public const int ERR_WebsocketError = 110303;
        public const int ERR_WebsocketConnectError = 110304;
        public const int ERR_RpcFail = 110307;
        public const int ERR_ReloadFail = 110308;
        public const int ERR_ConnectGateKeyError = 110309;
        public const int ERR_SessionSendOrRecvTimeout = 110311;
        public const int ERR_OuterSessionRecvInnerMessage = 110312;
        public const int ERR_NotFoundActor = 110313;
        public const int ERR_MessageTimeout = 110315;
        public const int ERR_UnverifiedSessionSendMessage = 110316;
        public const int ERR_ActorLocationSenderTimeout2 = 110317;
        public const int ERR_ActorLocationSenderTimeout3 = 110318;
        public const int ERR_ActorLocationSenderTimeout4 = 110319;
        public const int ERR_ActorLocationSenderTimeout5 = 110320;
        
        public const int ERR_KcpRouterTimeout = 110401;
        public const int ERR_KcpRouterTooManyPackets = 110402;
        
        // 区服登录错误码
        public const int ERR_ZoneMaintenance = 110500;
        public const int ERR_ZoneCannotRegister = 110501;
        public const int ERR_AccountBanned = 110502;
        public const int ERR_AccountFrozen = 110503;
        public const int ERR_PasswordError = 110504;
        public const int ERR_NotInWhitelist = 110505;
        public const int ERR_NotFoundGate = 110506;
        public const int ERR_NotFoundMap = 110507;
        public const int ERR_EnterMapError = 110508;
        public const int ERR_NotFoundAccount = 110509;
        public const int ERR_ZonePlayerBanned = 110510;
        public const int ERR_ZonePlayerFrozen = 110511;
        public const int ERR_SessionAccountMismatch = 110512;
        public const int ERR_UnitAlreadyExists = 110513;
        public const int ERR_NickNamePoolEmpty = 110514;
        public const int ERR_NickNameDuplicate = 110515;
        public const int ERR_SessionReplaced = 110516;
        public const int ERR_ZoneNotOpen = 110517;
        public const int ERR_KcpRouterSame = 110403;
        public const int ERR_KcpRouterConnectFail = 110404;
        public const int ERR_KcpRouterRouterSyncCountTooMuchTimes = 110405;
        public const int ERR_KcpRouterSyncCountTooMuchTimes = 110406;
        
        // 110000 以上，避免跟SocketError冲突


        //-----------------------------------

        // 小于这个Rpc会抛异常，大于这个异常的error需要自己判断处理，也就是说需要处理的错误应该要大于该值
        public const int ERR_Exception = 200000;

        public const int ERR_Cancel = 200001;
        public const int ERR_Timeout = 200002;

        public static bool IsRpcNeedThrowException(int error)
        {
            if (error == 0)
            {
                return false;
            }
            // ws平台返回错误专用的值
            if (error == -1)
            {
                return false;
            }

            if (error > ERR_Exception)
            {
                return false;
            }

            return true;
        }
    }
}