﻿using System;
using System.Threading.Tasks;
using Pandora.Client.Exchange.JKrof.Objects;
using Pandora.Client.Exchange.JKrof.Sockets;

namespace Pandora.Client.Exchange.JKrof.Interfaces
{
    /// <summary>
    /// Base class for socket API implementations
    /// </summary>
    public interface ISocketClient: IDisposable
    {
        /// <summary>
        /// The factory for creating sockets. Used for unit testing
        /// </summary>
        IWebsocketFactory SocketFactory { get; set; }

        /// <summary>
        /// The time in between reconnect attempts
        /// </summary>
        TimeSpan ReconnectInterval { get; }
        
        /// <summary>
        /// Whether the client should try to auto reconnect when losing connection
        /// </summary>
        bool AutoReconnect { get; }

        /// <summary>
        /// The base address of the API
        /// </summary>
        string BaseAddress { get; }

        /// <inheritdoc cref="SocketClientOptions.SocketResponseTimeout"/>
        TimeSpan ResponseTimeout { get; }

        /// <inheritdoc cref="SocketClientOptions.SocketNoDataTimeout"/>
        TimeSpan SocketNoDataTimeout { get; }

        /// <summary>
        /// The max amount of concurrent socket connections
        /// </summary>
        int MaxSocketConnections { get; }

        /// <inheritdoc cref="SocketClientOptions.SocketSubscriptionsCombineTarget"/>
        int SocketCombineTarget { get; }

        /// <summary>
        /// Unsubscribe from a stream
        /// </summary>
        /// <param name="subscription">The subscription to unsubscribe</param>
        /// <returns></returns>
        Task Unsubscribe(UpdateSubscription subscription);

        /// <summary>
        /// Unsubscribe all subscriptions
        /// </summary>
        /// <returns></returns>
        Task UnsubscribeAll();
    }
}