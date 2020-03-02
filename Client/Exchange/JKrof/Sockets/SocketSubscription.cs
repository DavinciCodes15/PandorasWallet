﻿using System;
using Newtonsoft.Json.Linq;

namespace Pandora.Client.Exchange.JKrof.Sockets
{
    /// <summary>
    /// Socket subscription
    /// </summary>
    public class SocketSubscription
    {
        /// <summary>
        /// Exception event
        /// </summary>
        public event Action<Exception> Exception;

        /// <summary>
        /// Message handlers for this subscription. Should return true if the message is handled and should not be distributed to the other handlers
        /// </summary>
        public Action<SocketConnection, JToken> MessageHandler { get; set; }

        /// <summary>
        /// Request object
        /// </summary>
        public object Request { get; set; }
        /// <summary>
        /// Subscription identifier
        /// </summary>
        public string Identifier { get; set; }
        /// <summary>
        /// Is user subscription or generic
        /// </summary>
        public bool UserSubscription { get; set; }
        
        /// <summary>
        /// If the subscription has been confirmed
        /// </summary>
        public bool Confirmed { get; set; }

        private SocketSubscription(object request, string identifier, bool userSubscription, Action<SocketConnection, JToken> dataHandler)
        {
            UserSubscription = userSubscription;
            MessageHandler = dataHandler;
            Request = request;
            Identifier = identifier;
        }

        /// <summary>
        /// Create SocketSubscription for a request
        /// </summary>
        /// <param name="request"></param>
        /// <param name="userSubscription"></param>
        /// <param name="dataHandler"></param>
        /// <returns></returns>
        public static SocketSubscription CreateForRequest(object request, bool userSubscription,
            Action<SocketConnection, JToken> dataHandler)
        {
            return new SocketSubscription(request, null, userSubscription, dataHandler);
        }

        /// <summary>
        /// Create SocketSubscription for an identifier
        /// </summary>
        /// <param name="identifier"></param>
        /// <param name="userSubscription"></param>
        /// <param name="dataHandler"></param>
        /// <returns></returns>
        public static SocketSubscription CreateForIdentifier(string identifier, bool userSubscription,
            Action<SocketConnection, JToken> dataHandler)
        {
            return new SocketSubscription(null, identifier, userSubscription, dataHandler);
        }

        /// <summary>
        /// Invoke the exception event
        /// </summary>
        /// <param name="e"></param>
        public void InvokeExceptionHandler(Exception e)
        {
            Exception?.Invoke(e);
        }
    }
}
