//using Bittrex.Net.Sockets;
//using Pandora.Client.Exchange.CustomAPI.Objects;
//using Pandora.Client.Exchange.CustomAPI.Poloniex.Net.Objects;
//using Pandora.Client.Exchange.CustomAPI.Sockets;
//using System;
//using System.Collections.Generic;
//using System.Linq;
//using System.Text;
//using System.Threading.Tasks;

//namespace Pandora.Client.Exchange.CustomAPI.Poloniex.Net
//{
//    public class PoloniexSocketClient : SocketClient
//    {
//        private static PoloniexSocketClientOptions defaultOptions = new PoloniexSocketClientOptions();
//        private object FBackgroundSocketLock;

//        private static PoloniexSocketClientOptions DefaultOptions => defaultOptions.Copy<PoloniexSocketClientOptions>();

//        /// <summary>
//        /// Create a new instance of BittrexClient using the default options
//        /// </summary>
//        public PoloniexSocketClient() : this(DefaultOptions)
//        {
//        }

//        /// <summary>
//        /// Create a new instance of the BittrexClient with the provided options
//        /// </summary>
//        public PoloniexSocketClient(PoloniexSocketClientOptions options) : base(options, options.ApiCredentials == null ? null : new PoloniexAuthenticationProvider(options.ApiCredentials))
//        {
//            SocketFactory = new ConnectionFactory();
//            FBackgroundSocketLock = new object();
//        }

//        /// <summary>
//        /// Sets the default options to use for new clients
//        /// </summary>
//        /// <param name="options">The options to use for new clients</param>
//        public static void SetDefaultOptions(PoloniexSocketClientOptions options)
//        {
//            defaultOptions = options;
//        }

//        public async Task<CallResult<object[]>> SubscribeToTickerDataUpdates()
//        {
//            return await Subscribe(new ConnectionRequest(false,string.Empty, string[]{ ""}))
//        }

//        private async Task<CallResult<UpdateSubscription>> Subscribe<T>(ConnectionRequest request, Action<T> onData)
//        {
//            var connectResult = await CreateAndConnectSocket(request.Signed, true, onData).ConfigureAwait(false);
//            if (!connectResult.Success)
//                return new CallResult<UpdateSubscription>(null, connectResult.Error);

//            return await Subscribe(connectResult.Data, request).ConfigureAwait(false);
//        }

//        private async Task<CallResult<SocketSubscription>> CreateAndConnectSocket<T>(bool authenticated, bool subscribing, Action<T> onData)
//        {
//            var socket = CreateSocket(BaseAddress);
//            var subscription = new SocketSubscription(socket);
//            if (subscribing)
//                subscription.MessageHandlers.Add(DataHandlerName, (subs, data) => UpdateHandler(data, onData));

//            var connectResult = await ConnectSocket(subscription).ConfigureAwait(false);
//            if (!connectResult.Success)
//                return new CallResult<SocketSubscription>(null, connectResult.Error);

//            return new CallResult<SocketSubscription>(subscription, null);
//        }

//        private bool UpdateHandler<T>(JToken data, Action<T> onData)
//        {
//            if (data["A"] == null)
//                return false;

//            var decData = DecodeData((string) ((JArray) data["A"])[0]).Result;
//            if (!decData.Success)
//            {
//                log.Write(LogVerbosity.Warning, "Failed to decode data: " + decData.Error);
//                return false;
//            }

//            if (typeof(T) == typeof(string))
//            {
//                onData((T) Convert.ChangeType(decData.Data, typeof(T)));
//                return true;
//            }

//            var desData = Deserialize<T>(decData.Data);
//            if (!desData.Success)
//            {
//                log.Write(LogVerbosity.Warning, $"Failed to deserialize data into {typeof(T).Name}: " + desData.Error);
//                return false;
//            }

//            onData(desData.Data);
//            return true;
//        }

//        private async Task<CallResult<UpdateSubscription>> Subscribe(SocketSubscription subscription, ConnectionRequest request)
//        {
//            if (request.RequestName != null)
//            {
//                var subResult = await ((ISignalRSocket) subscription.Socket).InvokeProxy<bool>(request.RequestName, request.Parameters).ConfigureAwait(false);
//                if (!subResult.Success || !subResult.Data)
//                {
//                    var closeTask = subscription.Close();
//                    return new CallResult<UpdateSubscription>(null, subResult.Error ?? new ServerError("Subscribe returned false"));
//                }
//            }

//            subscription.Request = request;
//            subscription.Socket.ShouldReconnect = true;
//            return new CallResult<UpdateSubscription>(new UpdateSubscription(subscription), null);
//        }

//        protected override bool SocketReconnect(SocketSubscription subscription, TimeSpan disconnectedTime)
//        {
//            throw new NotImplementedException();
//        }
//    }
//}