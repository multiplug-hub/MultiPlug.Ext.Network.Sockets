﻿
namespace MultiPlug.Ext.Network.Sockets.Diagnostics
{
    internal enum EventLogEntryCodes
    {
        Reserved = 0,
        SourceSocketEndpoint = 1,
        SourceSocketEndpointListener = 2,
        SourceSocketClient = 3,
        SourceHttpRequest = 4,
        SourceHttpEndpoint = 5,
        SourceSSDP = 6,

        NoIPAddress = 21,
        IncorrectPort = 22,
        IncorrectBacklogValue = 23,
        LocalIPAddressParse = 24,
        SocketBindException = 25,
        WaitingForConnection = 26,
        SocketEndpointShutdown = 27,
        ConnectedTo = 28,
        SocketEndpointSocketException = 29,
        SocketEndpointException = 30,
        SocketEndpointDataReceived = 31,
        SocketEndpointExceptionConnectionReset = 32,
        SocketEndpointExceptionDisconnected = 33,
        SocketEndpointSent = 34,
        SocketEndpointSending = 35,
        SocketEndpointLocalIPAddressUpdated = 36,
        SocketEndpointLocalIPAddressUpdateFailed = 37,
        SocketEndpointSocketClosed = 38,
        SocketEndpointSocketClosingDueToReconfigure = 39,
        SocketEndpointSocketClosingDueToSubscriptionControl = 40,

        SocketClientNoHostName = 55,
        SocketClientIncorrectPort = 56,
        SocketClientConnectingTo = 57,
        SocketClientException = 58,
        SocketClientConnectedTo = 59,
        SocketClientDataReceived = 60,
        SocketClientDataSent = 61,
        SocketClientSending = 62,
        SocketClientSocketExceptionCode = 63,
        SocketClientSocketException = 64,
        SocketClientSocketClosed = 65,
        SocketClientSocketClosedException = 66,
        SocketClientSocketClosingDueToReconfigure = 67,
        SocketClientSocketClosingDueToSubscriptionControl = 68
    }
}
