
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
        SocketEndpointExceptionDiconnected = 33,
        SocketEndpointSent = 34,
        SocketClientNoHostName = 35,
        SocketClientIncorrectPort = 36,
        SocketClientConnectingTo = 37,
        SocketClientException = 38,
        SocketClientConnectedTo = 39,
        SocketClientReceivedData = 40,
        SocketClientDataSent = 41,
        SocketClientSending = 42,
        SocketClientSocketExceptionCode = 43,
        SocketClientSocketException = 44
    }
}
