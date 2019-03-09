﻿using System;
using System.Threading.Tasks;

namespace Pandora.Client.Exchange.JKrof.Sockets
{
    public class UpdateSubscription
    {
        private readonly SocketSubscription subscription;

        /// <summary>
        /// Event when the connection is lost. The socket will automatically reconnect when possible.
        /// </summary>
        public event Action ConnectionLost
        {
            add => subscription.ConnectionLost += value;
            remove => subscription.ConnectionLost -= value;
        }

        /// <summary>
        /// Event when the connection is restored. Timespan parameter indicates the time the socket has been offline for before reconnecting
        /// </summary>
        public event Action<TimeSpan> ConnectionRestored
        {
            add => subscription.ConnectionRestored += value;
            remove => subscription.ConnectionRestored -= value;
        }

        /// <summary>
        /// Event when an exception happened
        /// </summary>
        public event Action<Exception> Exception
        {
            add => subscription.Exception += value;
            remove => subscription.Exception -= value;
        }

        /// <summary>
        /// The id of the socket
        /// </summary>
        public int Id => subscription.Socket.Id;

        public UpdateSubscription(SocketSubscription sub)
        {
            subscription = sub;
        }

        /// <summary>
        /// Close the subscription
        /// </summary>
        /// <returns></returns>
        public async Task Close()
        {
            await subscription.Close().ConfigureAwait(false);
        }
    }
}
