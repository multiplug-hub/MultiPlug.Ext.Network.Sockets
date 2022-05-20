using MultiPlug.Base.Diagnostics;

namespace MultiPlug.Ext.Network.Sockets.Diagnostics
{
    internal class EventLogDefinitions
    {
        internal const string DefinitionsId = "MultiPlug.Ext.Network.Sockets.EN";

        internal static EventLogDefinition[] Definitions { get; set; } = new EventLogDefinition[]
        {
            new EventLogDefinition { Code = (uint) EventLogEntryCodes.SourceSocketEndpoint, Source = (uint) EventLogEntryCodes.Reserved,  StringFormat = "SocketEndpoint", Type = EventLogEntryType.Information },
            new EventLogDefinition { Code = (uint) EventLogEntryCodes.SourceSocketEndpointListener, Source = (uint) EventLogEntryCodes.Reserved, StringFormat = "EndpointListener", Type = EventLogEntryType.Information },
            new EventLogDefinition { Code = (uint) EventLogEntryCodes.SourceSocketClient, Source = (uint) EventLogEntryCodes.Reserved, StringFormat = "SocketClient", Type = EventLogEntryType.Information },

            new EventLogDefinition { Code = (uint) EventLogEntryCodes.NoIPAddress, Source = (uint) EventLogEntryCodes.SourceSocketEndpoint,  StringFormat = "Connection aborted: No IP Address", Type = EventLogEntryType.Warning },
            new EventLogDefinition { Code = (uint) EventLogEntryCodes.IncorrectPort, Source = (uint) EventLogEntryCodes.SourceSocketEndpoint, StringFormat = "Connection aborted: Incorrect port", Type = EventLogEntryType.Warning },
            new EventLogDefinition { Code = (uint) EventLogEntryCodes.IncorrectBacklogValue, Source = (uint) EventLogEntryCodes.SourceSocketEndpoint, StringFormat = "Connection aborted: Incorrect Backlog value", Type = EventLogEntryType.Warning },
            new EventLogDefinition { Code = (uint) EventLogEntryCodes.LocalIPAddressParse, Source = (uint) EventLogEntryCodes.SourceSocketEndpoint, StringFormat = "Connection aborted: Could no parse local NIC IP Address", Type = EventLogEntryType.Warning },
            new EventLogDefinition { Code = (uint) EventLogEntryCodes.SocketBindException, Source = (uint) EventLogEntryCodes.SourceSocketEndpoint, StringFormat = "Socket Bind Exception. {0}", Type = EventLogEntryType.Error },
            new EventLogDefinition { Code = (uint) EventLogEntryCodes.WaitingForConnection, Source = (uint) EventLogEntryCodes.SourceSocketEndpointListener, StringFormat = "Waiting for a connection", Type = EventLogEntryType.Information },
            new EventLogDefinition { Code = (uint) EventLogEntryCodes.SocketEndpointShutdown, Source = (uint) EventLogEntryCodes.SourceSocketEndpointListener, StringFormat = "Socket Endpoint has shutdown", Type = EventLogEntryType.Information },
            new EventLogDefinition { Code = (uint) EventLogEntryCodes.ConnectedTo, Source = (uint) EventLogEntryCodes.SourceSocketEndpointListener, StringFormat = "Connected to {0}", Type = EventLogEntryType.Information },

            new EventLogDefinition { Code = (uint) EventLogEntryCodes.SocketEndpointSocketException, Source = (uint) EventLogEntryCodes.SourceSocketEndpointListener, StringFormat = "Socket Exception: {0}", Type = EventLogEntryType.Error },
            new EventLogDefinition { Code = (uint) EventLogEntryCodes.SocketEndpointException, Source = (uint) EventLogEntryCodes.SourceSocketEndpointListener, StringFormat = "Exception: {0}", Type = EventLogEntryType.Error },
            new EventLogDefinition { Code = (uint) EventLogEntryCodes.SocketEndpointDataReceived, Source = (uint) EventLogEntryCodes.SourceSocketEndpointListener, StringFormat = "Data Received: {0}", Type = EventLogEntryType.Information },
            new EventLogDefinition { Code = (uint) EventLogEntryCodes.SocketEndpointExceptionConnectionReset, Source = (uint) EventLogEntryCodes.SourceSocketEndpointListener, StringFormat = "Disconnected {0}", Type = EventLogEntryType.Warning },
            new EventLogDefinition { Code = (uint) EventLogEntryCodes.SocketEndpointExceptionDisconnected, Source = (uint) EventLogEntryCodes.SourceSocketEndpointListener, StringFormat = "Disconnected {0} Code {1}", Type = EventLogEntryType.Warning },
            new EventLogDefinition { Code = (uint) EventLogEntryCodes.SocketEndpointSent, Source = (uint) EventLogEntryCodes.SourceSocketEndpointListener, StringFormat = "Data Sent", Type = EventLogEntryType.Information },
            new EventLogDefinition { Code = (uint) EventLogEntryCodes.SocketEndpointSending, Source = (uint) EventLogEntryCodes.SourceSocketEndpointListener, StringFormat = "Sending: {0}", Type = EventLogEntryType.Information },
            new EventLogDefinition { Code = (uint) EventLogEntryCodes.SocketEndpointLocalIPAddressUpdated, Source = (uint) EventLogEntryCodes.SourceSocketEndpoint, StringFormat = "Local NIC IP updated to: {0}", Type = EventLogEntryType.Information },
            new EventLogDefinition { Code = (uint) EventLogEntryCodes.SocketEndpointLocalIPAddressUpdateFailed, Source = (uint) EventLogEntryCodes.SourceSocketEndpoint, StringFormat = "No Local NIC IP Address of: {0}", Type = EventLogEntryType.Warning },

            new EventLogDefinition { Code = (uint) EventLogEntryCodes.SocketEndpointSocketClosed, Source = (uint) EventLogEntryCodes.SourceSocketEndpoint, StringFormat = "Socket Closed", Type = EventLogEntryType.Information },
            new EventLogDefinition { Code = (uint) EventLogEntryCodes.SocketEndpointSocketClosingDueToReconfigure, Source = (uint) EventLogEntryCodes.SourceSocketEndpoint, StringFormat = "Socket closing due to reconfiguration", Type = EventLogEntryType.Information },
            new EventLogDefinition { Code = (uint) EventLogEntryCodes.SocketEndpointSocketClosingDueToSubscriptionControl, Source = (uint) EventLogEntryCodes.SourceSocketEndpoint, StringFormat = "Socket closing due to Subscription Control", Type = EventLogEntryType.Information },

            new EventLogDefinition { Code = (uint) EventLogEntryCodes.SocketClientNoHostName, Source = (uint) EventLogEntryCodes.SourceSocketClient, StringFormat = "Connection aborted: No HostName", Type = EventLogEntryType.Warning },
            new EventLogDefinition { Code = (uint) EventLogEntryCodes.SocketClientIncorrectPort, Source = (uint) EventLogEntryCodes.SourceSocketClient, StringFormat = "Connection aborted: Incorrect port", Type = EventLogEntryType.Warning },
            new EventLogDefinition { Code = (uint) EventLogEntryCodes.SocketClientConnectingTo, Source = (uint) EventLogEntryCodes.SourceSocketClient, StringFormat = "Connecting to {0}", Type = EventLogEntryType.Information },
            new EventLogDefinition { Code = (uint) EventLogEntryCodes.SocketClientException, Source = (uint) EventLogEntryCodes.SourceSocketClient, StringFormat = "Exception: {0}", Type = EventLogEntryType.Error },
            new EventLogDefinition { Code = (uint) EventLogEntryCodes.SocketClientConnectedTo, Source = (uint) EventLogEntryCodes.SourceSocketClient, StringFormat = "Connected to {0}", Type = EventLogEntryType.Information },
            new EventLogDefinition { Code = (uint) EventLogEntryCodes.SocketClientDataReceived, Source = (uint) EventLogEntryCodes.SourceSocketClient, StringFormat = "Data Received: {0}", Type = EventLogEntryType.Information },
            new EventLogDefinition { Code = (uint) EventLogEntryCodes.SocketClientDataSent, Source = (uint) EventLogEntryCodes.SourceSocketClient, StringFormat = "Data Sent", Type = EventLogEntryType.Information },
            new EventLogDefinition { Code = (uint) EventLogEntryCodes.SocketClientSending, Source = (uint) EventLogEntryCodes.SourceSocketClient, StringFormat = "Sending: {0}", Type = EventLogEntryType.Information },
            new EventLogDefinition { Code = (uint) EventLogEntryCodes.SocketClientSocketExceptionCode, Source = (uint) EventLogEntryCodes.SourceSocketClient, StringFormat = "Socket Exception. Code: {0}", Type = EventLogEntryType.Error },
            new EventLogDefinition { Code = (uint) EventLogEntryCodes.SocketClientSocketException, Source = (uint) EventLogEntryCodes.SourceSocketClient, StringFormat = "Socket Exception: {0}", Type = EventLogEntryType.Error },
            new EventLogDefinition { Code = (uint) EventLogEntryCodes.SocketClientSocketClosed, Source = (uint) EventLogEntryCodes.SourceSocketClient, StringFormat = "Socket Closed", Type = EventLogEntryType.Information },
            new EventLogDefinition { Code = (uint) EventLogEntryCodes.SocketClientSocketClosedException, Source = (uint) EventLogEntryCodes.SourceSocketClient, StringFormat = "Socket Closed Exception: {0}", Type = EventLogEntryType.Error },
            new EventLogDefinition { Code = (uint) EventLogEntryCodes.SocketClientSocketClosingDueToReconfigure, Source = (uint) EventLogEntryCodes.SourceSocketClient, StringFormat = "Socket closing due to reconfiguration", Type = EventLogEntryType.Information },
            new EventLogDefinition { Code = (uint) EventLogEntryCodes.SocketClientSocketClosingDueToSubscriptionControl, Source = (uint) EventLogEntryCodes.SourceSocketClient, StringFormat = "Socket closing due to Subscription Control", Type = EventLogEntryType.Information },
        };
    }
}
