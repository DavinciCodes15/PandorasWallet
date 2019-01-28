using Pandora.Client.Crypto.Currencies;
using Pandora.Client.Crypto.Protocol.Behaviors;
using Pandora.Client.Crypto.Protocol.Filters;
using Pandora.Client.Crypto.Protocol.Payloads;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Reflection;
using System.Runtime.ExceptionServices;
using System.Threading;
using System.Threading.Tasks;

namespace Pandora.Client.Crypto.Protocol
{
    public enum NodeState : int
    {
        Failed,
        Offline,
        Disconnecting,
        Connected,
        HandShaked
    }

    public class NodeDisconnectReason
    {
        public string Reason
        {
            get;
            set;
        }

        public Exception Exception
        {
            get;
            set;
        }
    }

    public class NodeRequirement
    {
        public uint? MinVersion
        {
            get;
            set;
        }

        public ProtocolCapabilities MinProtocolCapabilities
        {
            get; set;
        }

        public NodeServices RequiredServices
        {
            get;
            set;
        }

        public bool SupportSPV
        {
            get;
            set;
        }

        public virtual bool Check(VersionPayload version, ProtocolCapabilities capabilities)
        {
#pragma warning disable CS0618 // Type or member is obsolete
            if (!Check(version))
            {
#pragma warning restore CS0618 // Type or member is obsolete
                return false;
            }

            if (capabilities.PeerTooOld)
            {
                return false;
            }

            if (MinProtocolCapabilities == null)
            {
                return true;
            }

            if (SupportSPV)
            {
                if (capabilities.SupportNodeBloom && ((version.Services & NodeServices.NODE_BLOOM) == 0))
                {
                    return false;
                }
            }

            return capabilities.IsSupersetOf(MinProtocolCapabilities);
        }

#pragma warning disable CS0618 // Type or member is obsolete

        [Obsolete("Use Check(VersionPayload, ProtocolCapabilities capabilities) instead")]
        public virtual bool Check(VersionPayload version)
        {
            if (MinVersion != null)
            {
                if (version.Version < MinVersion.Value)
                {
                    return false;
                }
            }
            if ((RequiredServices & version.Services) != RequiredServices)
            {
                return false;
            }
            return true;
        }

#pragma warning restore CS0618 // Type or member is obsolete
    }

    public class SynchronizeChainOptions
    {
        /// <summary>
        /// Location until which synchronization should be stopped (default: null)
        /// </summary>
        public uint256 HashStop
        {
            get; set;
        }

        /// <summary>
        /// Skip PoW check
        /// </summary>
        public bool SkipPoWCheck
        {
            get; set;
        }

        /// <summary>
        /// Strip headers from the retrieved chain
        /// </summary>
        public bool StripHeaders
        {
            get; set;
        }
    }

    public delegate void NodeEventHandler(Node node);

    public delegate void NodeEventMessageIncoming(Node node, IncomingMessage message);

    public delegate void NodeStateEventHandler(Node node, NodeState oldState);

    public class Node : IDisposable
    {
        internal class SentMessage
        {
            public Payload Payload;
            public TaskCompletionSource<bool> Completion;
            public Guid ActivityId;
        }

        public class NodeConnection
        {
            private readonly Node _Node;

            public Node Node => _Node;

            private readonly Socket _Socket;

            public Socket Socket => _Socket;

            private readonly ManualResetEvent _Disconnected;

            public ManualResetEvent Disconnected => _Disconnected;

            private readonly CancellationTokenSource _Cancel;

            public CancellationTokenSource Cancel => _Cancel;

#if NOTRACESOURCE
			internal
#else

            public
#endif
 TraceCorrelation TraceCorrelation => Node.TraceCorrelation;

            public NodeConnection(Node node, Socket socket)
            {
                _Node = node;
                _Socket = socket;
                _Disconnected = new ManualResetEvent(false);
                _Cancel = new CancellationTokenSource();
            }

            internal BlockingCollection<SentMessage> Messages = new BlockingCollection<SentMessage>(new ConcurrentQueue<SentMessage>());

            public void BeginListen()
            {
                new Thread(() =>
                {
                    SentMessage processing = null;
                    Exception unhandledException = null;
                    //bool isVerbose = NodeServerTrace.Trace.Switch.ShouldTrace(TraceEventType.Verbose);
                    ManualResetEvent ar = new ManualResetEvent(false);
                    SocketAsyncEventArgs evt = new SocketAsyncEventArgs
                    {
                        SocketFlags = SocketFlags.None
                    };
                    evt.Completed += (a, b) =>
                    {
                        Utils.SafeSet(ar);
                    };
                    try

                    {
                        foreach (SentMessage kv in Messages.GetConsumingEnumerable(Cancel.Token))
                        {
                            processing = kv;
                            Payload payload = kv.Payload;
                            Message message = new Message
                            {
                                Magic = _Node.Network.Magic,
                                Payload = payload
                            };
                            if (payload is GetHeadersPayload getHeaders)
                            {
                                getHeaders.Version = Node.Version;
                            }
                            if (payload is GetBlocksPayload getBlocks)
                            {
                                getBlocks.Version = Node.Version;
                            }
                            //if (isVerbose)
                            //{
                            //    Trace.CorrelationManager.ActivityId = kv.ActivityId;
                            //    if (kv.ActivityId != TraceCorrelation.Activity)
                            //    {
                            //        NodeServerTrace.Transfer(TraceCorrelation.Activity);
                            //        Trace.CorrelationManager.ActivityId = TraceCorrelation.Activity;
                            //    }
                            //    NodeServerTrace.Verbose("Sending message " + message);
                            //}
                            MemoryStream ms = new MemoryStream();
                            message.ReadWrite(new CoinStream(ms, true)
                            {
                                ConsensusFactory = _Node.Network.Consensus,
                                ProtocolVersion = Node.Version,
                                TransactionOptions = Node.SupportedTransactionOptions,
                                FProtocolData = Node.Network
                            });
                            byte[] bytes = ms.ToArrayEfficient();
                            evt.SetBuffer(bytes, 0, bytes.Length);
                            _Node.Counter.AddWritten(bytes.Length);
                            ar.Reset();
                            if (!Socket.SendAsync(evt))
                            {
                                Utils.SafeSet(ar);
                            }

                            WaitHandle.WaitAny(new WaitHandle[] { ar, Cancel.Token.WaitHandle }, -1);
                            if (!Cancel.Token.IsCancellationRequested)
                            {
                                if (evt.SocketError != SocketError.Success)
                                {
                                    throw new SocketException((int)evt.SocketError);
                                }

                                processing.Completion.SetResult(true);
                                processing = null;
                            }
                        }
                    }
                    catch (OperationCanceledException)
                    {
                    }
                    catch (Exception ex)
                    {
                        unhandledException = ex;
                    }
                    finally
                    {
                        evt.Dispose();
                        ar.Dispose();
                    }

                    if (processing != null)
                    {
                        Messages.Add(processing);
                    }

                    foreach (SentMessage pending in Messages)
                    {
                        //if (isVerbose)
                        //{
                        //    Trace.CorrelationManager.ActivityId = pending.ActivityId;
                        //    if (pending != processing && pending.ActivityId != TraceCorrelation.Activity)
                        //        NodeServerTrace.Transfer(TraceCorrelation.Activity);
                        //    Trace.CorrelationManager.ActivityId = TraceCorrelation.Activity;
                        //    NodeServerTrace.Verbose("The connection cancelled before the message was sent");
                        //}
                        pending.Completion.SetException(new OperationCanceledException("The peer has been disconnected"));
                    }
                    Messages = new BlockingCollection<SentMessage>(new ConcurrentQueue<SentMessage>());
                    //NodeServerTrace.Information("Stop sending");
                    Cleanup(unhandledException);
                }).Start();
                new Thread(() =>
                {
                    _ListenerThreadId = Thread.CurrentThread.ManagedThreadId;
                    //using (TraceCorrelation.Open(false))
                    //{
                    //NodeServerTrace.Information("Listening");
                    Exception unhandledException = null;
                    try
                    {
                        NetworkStream stream = new NetworkStream(Socket, false);
                        while (!Cancel.Token.IsCancellationRequested)
                        {
                            Message message = Message.ReadNext(stream, Node.Network, Node.Version, Cancel.Token, out Currencies.PerformanceCounter counter);
                            //if (NodeServerTrace.Trace.Switch.ShouldTrace(TraceEventType.Verbose))
                            //    NodeServerTrace.Verbose("Receiving message : " + message.Command + " (" + message.Payload + ")");
                            Node.LastSeen = DateTimeOffset.UtcNow;
                            Node.Counter.Add(counter);
                            Node.OnMessageReceived(new IncomingMessage()
                            {
                                Message = message,
                                Socket = Socket,
                                Length = counter.ReadenBytes,
                                Node = Node
                            });
                        }
                    }
                    catch (OperationCanceledException)
                    {
                    }
                    catch (Exception ex)
                    {
                        unhandledException = ex;
                    }
                    //NodeServerTrace.Information("Stop listening");
                    Cleanup(unhandledException);
                    //}
                }).Start();
            }

            private int _CleaningUp;
            public int _ListenerThreadId;

            private void Cleanup(Exception unhandledException)
            {
                if (Interlocked.CompareExchange(ref _CleaningUp, 1, 0) == 1)
                {
                    return;
                }

                if (!Cancel.IsCancellationRequested)
                {
                    //NodeServerTrace.Error("Connection to server stopped unexpectedly", unhandledException);
                    Node.DisconnectReason = new NodeDisconnectReason()
                    {
                        Reason = "Unexpected exception while connecting to socket",
                        Exception = unhandledException
                    };
                    Node.State = NodeState.Failed;
                }

                if (Node.State != NodeState.Failed)
                {
                    Node.State = NodeState.Offline;
                }

                _Cancel.Cancel();
                Utils.SafeCloseSocket(Socket);
                _Disconnected.Set(); //Set before behavior detach to prevent deadlock
                foreach (INodeBehavior behavior in _Node.Behaviors)
                {
                    try
                    {
                        behavior.Detach();
                    }
                    catch (Exception)
                    {
                        //NodeServerTrace.Error("Error while detaching behavior " + behavior.GetType().FullName, ex);
                    }
                }
            }
        }

        public DateTimeOffset ConnectedAt
        {
            get;
            private set;
        }

        private volatile NodeState _State = NodeState.Offline;

        public NodeState State
        {
            get => _State;
            private set
            {
                //TraceCorrelation.LogInside(() => NodeServerTrace.Information("State changed from " + _State + " to " + value));
                NodeState previous = _State;
                _State = value;
                if (previous != _State)
                {
                    OnStateChanged(previous);
                    if (value == NodeState.Failed || value == NodeState.Offline)
                    {
                        //TraceCorrelation.LogInside(() => NodeServerTrace.Trace.TraceEvent(TraceEventType.Stop, 0, "Communication closed"));
                        OnDisconnected();
                    }
                }
            }
        }

        public event NodeStateEventHandler StateChanged;

        private void OnStateChanged(NodeState previous)
        {
            NodeStateEventHandler stateChanged = StateChanged;
            if (stateChanged != null)
            {
                foreach (NodeStateEventHandler handler in stateChanged.GetInvocationList().Cast<NodeStateEventHandler>())
                {
                    try
                    {
                        handler.DynamicInvoke(this, previous);
                    }
                    catch (TargetInvocationException)
                    {
                        //TraceCorrelation.LogInside(() => NodeServerTrace.Error("Error while StateChanged event raised", ex.InnerException));
                    }
                }
            }
        }

        private readonly NodeFiltersCollection _Filters = new NodeFiltersCollection();

        public NodeFiltersCollection Filters => _Filters;

        public event NodeEventMessageIncoming MessageReceived;

        protected void OnMessageReceived(IncomingMessage message)
        {
            VersionPayload version = message.Message.Payload as VersionPayload;
            if (version != null && State == NodeState.HandShaked)
            {
                if (message.Node.ProtocolCapabilities.SupportReject)
                {
                    message.Node.SendMessageAsync(new RejectPayload()
                    {
                        Code = RejectCode.DUPLICATE
                    });
                }
            }
            if (version != null)
            {
                TimeOffset = DateTimeOffset.Now - version.Timestamp;
                if ((version.Services & NodeServices.NODE_WITNESS) != 0)
                {
                    _SupportedTransactionOptions |= TransactionOptions.Witness;
                }
            }
            HaveWitnessPayload havewitness = message.Message.Payload as HaveWitnessPayload;
            if (havewitness != null)
            {
                _SupportedTransactionOptions |= TransactionOptions.Witness;
            }

            ActionFilter last = new ActionFilter((m, n) =>
            {
                MessageProducer.PushMessage(m);
                NodeEventMessageIncoming messageReceived = MessageReceived;
                if (messageReceived != null)
                {
                    foreach (NodeEventMessageIncoming handler in messageReceived.GetInvocationList().Cast<NodeEventMessageIncoming>())
                    {
                        try
                        {
                            handler.DynamicInvoke(this, m);
                        }
                        catch (TargetInvocationException)
                        {
                            //TraceCorrelation.LogInside(() => NodeServerTrace.Error("Error while OnMessageReceived event raised", ex.InnerException), false);
                        }
                    }
                }
            });

            IEnumerator<INodeFilter> enumerator = Filters.Concat(new[] { last }).GetEnumerator();
            FireFilters(enumerator, message);
        }

        private void OnSendingMessage(Payload payload, Action final)
        {
            IEnumerator<INodeFilter> enumerator = Filters.Concat(new[] { new ActionFilter(null, (n, p, a) => final()) }).GetEnumerator();
            FireFilters(enumerator, payload);
        }

        private void FireFilters(IEnumerator<INodeFilter> enumerator, Payload payload)
        {
            if (enumerator.MoveNext())
            {
                INodeFilter filter = enumerator.Current;
                try
                {
                    filter.OnSendingMessage(this, payload, () => FireFilters(enumerator, payload));
                }
                catch (Exception)
                {
                    //TraceCorrelation.LogInside(() => NodeServerTrace.Error("Unhandled exception raised by a node filter (OnSendingMessage)", ex.InnerException), false);
                }
            }
        }

        private void FireFilters(IEnumerator<INodeFilter> enumerator, IncomingMessage message)
        {
            if (enumerator.MoveNext())
            {
                INodeFilter filter = enumerator.Current;
                try
                {
                    filter.OnReceivingMessage(message, () => FireFilters(enumerator, message));
                }
                catch (Exception)
                {
                    //TraceCorrelation.LogInside(() => NodeServerTrace.Error("Unhandled exception raised by a node filter (OnReceivingMessage)", ex.InnerException), false);
                }
            }
        }

        public event NodeEventHandler Disconnected;

        private void OnDisconnected()
        {
            NodeEventHandler disconnected = Disconnected;
            if (disconnected != null)
            {
                foreach (NodeEventHandler handler in disconnected.GetInvocationList().Cast<NodeEventHandler>())
                {
                    try
                    {
                        handler.DynamicInvoke(this);
                    }
                    catch (TargetInvocationException)
                    {
                        //TraceCorrelation.LogInside(() => NodeServerTrace.Error("Error while Disconnected event raised", ex.InnerException));
                    }
                }
            }
        }

        internal readonly NodeConnection _Connection;

        /// <summary>
        /// Connect to a random node on the network
        /// </summary>
        /// <param name="network">The network to connect to</param>
        /// <param name="addrman">The addrman used for finding peers</param>
        /// <param name="parameters">The parameters used by the found node</param>
        /// <param name="connectedEndpoints">The already connected endpoints, the new endpoint will be select outside of existing groups</param>
        /// <returns></returns>
        public static Node Connect(ProtocolData network, AddressManager addrman, NodeConnectionParameters parameters = null, IPEndPoint[] connectedEndpoints = null)
        {
            parameters = parameters ?? new NodeConnectionParameters();
            AddressManagerBehavior.SetAddrman(parameters, addrman);
            return Connect(network, parameters, connectedEndpoints);
        }

        /// <summary>
        /// Connect to a random node on the network
        /// </summary>
        /// <param name="network">The network to connect to</param>
        /// <param name="parameters">The parameters used by the found node, use AddressManagerBehavior.GetAddrman for finding peers</param>
        /// <param name="connectedEndpoints">The already connected endpoints, the new endpoint will be select outside of existing groups</param>
        /// <param name="getGroup">Group selector, by default NBicoin.IpExtensions.GetGroup</param>
        /// <returns></returns>
        public static Node Connect(ProtocolData network, NodeConnectionParameters parameters = null, IPEndPoint[] connectedEndpoints = null, Func<IPEndPoint, byte[]> getGroup = null)
        {
            getGroup = getGroup ?? new Func<IPEndPoint, byte[]>((a) => IpExtensions.GetGroup(a.Address));
            connectedEndpoints = connectedEndpoints ?? new IPEndPoint[0];
            parameters = parameters ?? new NodeConnectionParameters();
            AddressManagerBehavior addrmanBehavior = parameters.TemplateBehaviors.FindOrCreate(() => new AddressManagerBehavior(new AddressManager()));
            AddressManager addrman = AddressManagerBehavior.GetAddrman(parameters);
            DateTimeOffset start = DateTimeOffset.UtcNow;
            while (true)
            {
                parameters.ConnectCancellation.ThrowIfCancellationRequested();
                if (addrman.Count == 0 || DateTimeOffset.UtcNow - start > TimeSpan.FromSeconds(60))
                {
                    addrmanBehavior.DiscoverPeers(network, parameters);
                    start = DateTimeOffset.UtcNow;
                }
                NetworkAddress addr = null;
                int groupFail = 0;
                while (true)
                {
                    if (groupFail > 50)
                    {
                        parameters.ConnectCancellation.WaitHandle.WaitOne((int)TimeSpan.FromSeconds(60).TotalMilliseconds);
                        break;
                    }
                    addr = addrman.Select();
                    if (addr == null)
                    {
                        parameters.ConnectCancellation.WaitHandle.WaitOne(1000);
                        break;
                    }
                    if (!addr.Endpoint.Address.IsValid())
                    {
                        continue;
                    }

                    bool groupExist = connectedEndpoints.Any(a => getGroup(a).SequenceEqual(getGroup(addr.Endpoint)));
                    if (groupExist)
                    {
                        groupFail++;
                        continue;
                    }
                    break;
                }
                if (addr == null)
                {
                    continue;
                }

                try
                {
                    CancellationTokenSource timeout = new CancellationTokenSource(5000);
                    NodeConnectionParameters param2 = parameters.Clone();
                    param2.ConnectCancellation = CancellationTokenSource.CreateLinkedTokenSource(parameters.ConnectCancellation, timeout.Token).Token;
                    Node node = Node.Connect(network, addr.Endpoint, param2);
                    return node;
                }
                catch (OperationCanceledException ex)
                {
                    if (ex.CancellationToken == parameters.ConnectCancellation)
                    {
                        throw;
                    }
                }
                catch (SocketException)
                {
                    parameters.ConnectCancellation.WaitHandle.WaitOne(500);
                }
            }
        }

        /// <summary>
        /// Connect to the node of this machine
        /// </summary>
        /// <param name="network"></param>
        /// <param name="parameters"></param>
        /// <returns></returns>
        public static Node ConnectToLocal(ProtocolData network,
                                NodeConnectionParameters parameters)
        {
            return Connect(network, Utils.ParseIpEndpoint("localhost", network.DefaultPort), parameters);
        }

        public static Node ConnectToLocal(ProtocolData network,
                                uint? myVersion = null,
                                bool isRelay = true,
                                CancellationToken cancellation = default(CancellationToken))
        {
            return ConnectToLocal(network, new NodeConnectionParameters()
            {
                ConnectCancellation = cancellation,
                IsRelay = isRelay,
                Version = myVersion
            });
        }

        public static Node Connect(ProtocolData network,
                                 string endpoint, NodeConnectionParameters parameters)
        {
            return Connect(network, Utils.ParseIpEndpoint(endpoint, network.DefaultPort), parameters);
        }

        public static Node Connect(ProtocolData network,
                                 string endpoint,
                                 uint? myVersion = null,
                                bool isRelay = true,
                                CancellationToken cancellation = default(CancellationToken))
        {
            return Connect(network, Utils.ParseIpEndpoint(endpoint, network.DefaultPort), myVersion, isRelay, cancellation);
        }

        public static Node Connect(ProtocolData network,
                             NetworkAddress endpoint,
                             NodeConnectionParameters parameters)
        {
            return new Node(endpoint, network, parameters);
        }

        public static Node Connect(ProtocolData network,
                             IPEndPoint endpoint,
                             NodeConnectionParameters parameters)
        {
            NetworkAddress peer = new NetworkAddress()
            {
                Time = DateTimeOffset.UtcNow,
                Endpoint = endpoint
            };

            return new Node(peer, network, parameters);
        }

        public static Node Connect(ProtocolData network,
                                 IPEndPoint endpoint,
                                 uint? myVersion = null,
                                bool isRelay = true,
                                CancellationToken cancellation = default(CancellationToken))
        {
            return Connect(network, endpoint, new NodeConnectionParameters()
            {
                ConnectCancellation = cancellation,
                IsRelay = isRelay,
                Version = myVersion,
                Services = NodeServices.Nothing,
            });
        }

        internal Node(NetworkAddress peer, ProtocolData network, NodeConnectionParameters parameters)
        {
            parameters = parameters ?? new NodeConnectionParameters();
            AddressManager addrman = AddressManagerBehavior.GetAddrman(parameters);
            Inbound = false;
            _Behaviors = new NodeBehaviorsCollection(this);
            _MyVersion = parameters.CreateVersion(peer.Endpoint, network);
            Network = network;
            SetVersion(_MyVersion.Version);
            _Peer = peer;
            LastSeen = peer.Time;

            Socket socket = new Socket(AddressFamily.InterNetworkV6, SocketType.Stream, ProtocolType.Tcp);
            socket.SetSocketOption(SocketOptionLevel.IPv6, SocketOptionName.IPv6Only, false);

            _Connection = new NodeConnection(this, socket);
            socket.ReceiveBufferSize = parameters.ReceiveBufferSize;
            socket.SendBufferSize = parameters.SendBufferSize;
            //using (TraceCorrelation.Open())
            //{
            try
            {
                ManualResetEvent completed = new ManualResetEvent(false);
                SocketAsyncEventArgs args = new SocketAsyncEventArgs
                {
                    RemoteEndPoint = peer.Endpoint
                };
                args.Completed += (s, a) =>
                {
                    Utils.SafeSet(completed);
                };
                if (!socket.ConnectAsync(args))
                {
                    completed.Set();
                }

                WaitHandle.WaitAny(new WaitHandle[] { completed, parameters.ConnectCancellation.WaitHandle });
                parameters.ConnectCancellation.ThrowIfCancellationRequested();
                if (args.SocketError != SocketError.Success)
                {
                    throw new SocketException((int)args.SocketError);
                }

                IPEndPoint remoteEndpoint = (IPEndPoint)(socket.RemoteEndPoint ?? args.RemoteEndPoint);
                _RemoteSocketAddress = remoteEndpoint.Address;
                _RemoteSocketEndpoint = remoteEndpoint;
                _RemoteSocketPort = remoteEndpoint.Port;
                State = NodeState.Connected;
                ConnectedAt = DateTimeOffset.UtcNow;
                //NodeServerTrace.Information("Outbound connection successfull");
                if (addrman != null)
                {
                    addrman.Attempt(Peer);
                }
            }
            catch (OperationCanceledException)
            {
                Utils.SafeCloseSocket(socket);
                //NodeServerTrace.Information("Connection to node cancelled");
                State = NodeState.Offline;
                if (addrman != null)
                {
                    addrman.Attempt(Peer);
                }

                throw;
            }
            catch (Exception ex)
            {
                Utils.SafeCloseSocket(socket);
                //NodeServerTrace.Error("Error connecting to the remote endpoint ", ex);
                DisconnectReason = new NodeDisconnectReason()
                {
                    Reason = "Unexpected exception while connecting to socket",
                    Exception = ex
                };
                State = NodeState.Failed;
                if (addrman != null)
                {
                    addrman.Attempt(Peer);
                }

                throw;
            }
            InitDefaultBehaviors(parameters);
            _Connection.BeginListen();
        }

        //}

        private void SetVersion(uint version)
        {
            Version = version;
            ProtocolCapabilities = Network.GetProtocolCapabilities(version);
        }

        internal Node(NetworkAddress peer, ProtocolData network, NodeConnectionParameters parameters, Socket socket, VersionPayload peerVersion)
        {
            _RemoteSocketAddress = ((IPEndPoint)socket.RemoteEndPoint).Address;
            _RemoteSocketEndpoint = ((IPEndPoint)socket.RemoteEndPoint);
            _RemoteSocketPort = ((IPEndPoint)socket.RemoteEndPoint).Port;
            Inbound = true;
            _Behaviors = new NodeBehaviorsCollection(this);
            _MyVersion = parameters.CreateVersion(peer.Endpoint, network);
            if (peerVersion == null)
            {
                SetVersion(_MyVersion.Version);
            }

            Network = network;
            _Peer = peer;
            _Connection = new NodeConnection(this, socket);
            _PeerVersion = peerVersion;
            if (peerVersion != null)
            {
                SetVersion(Math.Min(_MyVersion.Version, _PeerVersion.Version));
            }

            LastSeen = peer.Time;
            ConnectedAt = DateTimeOffset.UtcNow;
            //TraceCorrelation.LogInside(() =>
            //{
            //NodeServerTrace.Information("Connected to advertised node " + _Peer.Endpoint);
            State = NodeState.Connected;
            //});
            InitDefaultBehaviors(parameters);
            _Connection.BeginListen();
        }

        private IPAddress _RemoteSocketAddress;

        public IPAddress RemoteSocketAddress => _RemoteSocketAddress;

        private IPEndPoint _RemoteSocketEndpoint;

        public IPEndPoint RemoteSocketEndpoint => _RemoteSocketEndpoint;

        private int _RemoteSocketPort;

        public int RemoteSocketPort => _RemoteSocketPort;

        public bool Inbound
        {
            get;
            private set;
        }

        private void InitDefaultBehaviors(NodeConnectionParameters parameters)
        {
            Advertize = parameters.Advertize;
            PreferredTransactionOptions = parameters.PreferredTransactionOptions;
            _Behaviors.DelayAttach = true;
            foreach (INodeBehavior behavior in parameters.TemplateBehaviors)
            {
                _Behaviors.Add(behavior.Clone());
            }
            _Behaviors.DelayAttach = false;
        }

        private readonly NodeBehaviorsCollection _Behaviors;

        public NodeBehaviorsCollection Behaviors => _Behaviors;

        private readonly NetworkAddress _Peer;

        public NetworkAddress Peer => _Peer;

        public DateTimeOffset LastSeen
        {
            get;
            private set;
        }

        public TimeSpan? TimeOffset
        {
            get;
            private set;
        }

        private TraceCorrelation _TraceCorrelation = null;

        [DebuggerBrowsable(DebuggerBrowsableState.Never)]
#if NOTRACESOURCE
		internal
#else
        public
#endif
 TraceCorrelation TraceCorrelation
        {
            get
            {
                if (_TraceCorrelation == null)
                {
                    //_TraceCorrelation = new TraceCorrelation(NodeServerTrace.Trace, "Communication with " + Peer.Endpoint.ToString());
                }
                return _TraceCorrelation;
            }
        }

        /// <summary>
        /// Send a message to the peer asynchronously
        /// </summary>
        /// <param name="payload">The payload to send</param>
        /// <param name="System.OperationCanceledException">The node has been disconnected</param>
        public Task SendMessageAsync(Payload payload)
        {
            if (payload == null)
            {
                throw new ArgumentNullException(nameof(payload));
            }

            TaskCompletionSource<bool> completion = new TaskCompletionSource<bool>();
            if (!IsConnected)
            {
                completion.SetException(new OperationCanceledException("The peer has been disconnected"));
                return completion.Task;
            }
            Guid activity = Trace.CorrelationManager.ActivityId;
            Action final = () =>
            {
                _Connection.Messages.Add(new SentMessage()
                {
                    Payload = payload,
                    ActivityId = activity,
                    Completion = completion
                });
            };
            OnSendingMessage(payload, final);
            return completion.Task;
        }

        /// <summary>
        /// Send a message to the peer synchronously
        /// </summary>
        /// <param name="payload">The payload to send</param>
        /// <exception cref="System.ArgumentNullException">Payload is null</exception>
        /// <param name="System.OperationCanceledException">The node has been disconnected, or the cancellation token has been set to canceled</param>
        public void SendMessage(Payload payload, CancellationToken cancellation = default(CancellationToken))
        {
            try
            {
                SendMessageAsync(payload).Wait(cancellation);
            }
            catch (AggregateException aex)
            {
                ExceptionDispatchInfo.Capture(aex.InnerException).Throw();
                throw;
            }
        }

        private Currencies.PerformanceCounter _Counter;

        public Currencies.PerformanceCounter Counter
        {
            get
            {
                if (_Counter == null)
                {
                    _Counter = new Currencies.PerformanceCounter();
                }

                return _Counter;
            }
        }

        /// <summary>
        /// The negociated protocol version (minimum of supported version between MyVersion and the PeerVersion)
        /// </summary>
        public uint Version
        {
            get;
            internal set;
        }

        public ProtocolCapabilities ProtocolCapabilities
        {
            get;
            internal set;
        }

        public bool IsConnected => State == NodeState.Connected || State == NodeState.HandShaked;

        private readonly MessageProducer<IncomingMessage> _MessageProducer = new MessageProducer<IncomingMessage>();

        public MessageProducer<IncomingMessage> MessageProducer => _MessageProducer;

        public TPayload ReceiveMessage<TPayload>(TimeSpan timeout) where TPayload : Payload
        {
            CancellationTokenSource source = new CancellationTokenSource();
            source.CancelAfter(timeout);
            return ReceiveMessage<TPayload>(source.Token);
        }

        public TPayload ReceiveMessage<TPayload>(CancellationToken cancellationToken = default(CancellationToken)) where TPayload : Payload
        {
            using (NodeListener listener = new NodeListener(this))
            {
                return listener.ReceivePayload<TPayload>(cancellationToken);
            }
        }

        /// <summary>
        /// Send addr unsollicited message of the AddressFrom peer when passing to Handshaked state
        /// </summary>
        public bool Advertize
        {
            get;
            set;
        }

        private readonly VersionPayload _MyVersion;

        public VersionPayload MyVersion => _MyVersion;

        private VersionPayload _PeerVersion;

        public VersionPayload PeerVersion => _PeerVersion;

        public void VersionHandshake(CancellationToken cancellationToken = default(CancellationToken), bool aIsSimpleHandshake = false)
        {
            VersionHandshake(null, cancellationToken, aIsSimpleHandshake);
        }

        public void VersionHandshake(NodeRequirement requirements, CancellationToken cancellationToken = default(CancellationToken), bool aIsSimpleHandshake = false)
        {
            requirements = requirements ?? new NodeRequirement();
            using (NodeListener listener = CreateListener()
                                    .Where(p => p.Message.Payload is VersionPayload ||
                                                p.Message.Payload is RejectPayload ||
                                                p.Message.Payload is VerAckPayload))
            {
                SendMessageAsync(MyVersion);

                if (!aIsSimpleHandshake)
                {
                    Payload payload = listener.ReceivePayload<Payload>(cancellationToken);
                    if (payload is RejectPayload)
                    {
                        throw new ProtocolException("Handshake rejected : " + ((RejectPayload)payload).Reason);
                    }

                    VersionPayload version = (VersionPayload)payload;
                    _PeerVersion = version;
                    SetVersion(Math.Min(MyVersion.Version, version.Version));
                    if (!version.AddressReceiver.Address.Equals(MyVersion.AddressFrom.Address))
                    {
                        //NodeServerTrace.Warning("Different external address detected by the node " + version.AddressReceiver.Address + " instead of " + MyVersion.AddressFrom.Address);
                    }
                    if (ProtocolCapabilities.PeerTooOld)
                    {
                        //NodeServerTrace.Warning("Outdated version " + version.Version + " disconnecting");
                        Disconnect("Outdated version");
                        return;
                    }

                    if (!requirements.Check(version, ProtocolCapabilities))
                    {
                        Disconnect("The peer does not support the required services requirement");
                        return;
                    }

                    SendMessageAsync(new VerAckPayload());
                    listener.ReceivePayload<VerAckPayload>(cancellationToken);
                }
                State = NodeState.HandShaked;
                if (Advertize && MyVersion.AddressFrom.Address.IsRoutable(true))
                {
                    SendMessageAsync(new AddrPayload(new NetworkAddress(MyVersion.AddressFrom)
                    {
                        Time = DateTimeOffset.UtcNow
                    }));
                }
            }
        }

        /// <summary>
        ///
        /// </summary>
        /// <param name="cancellation"></param>
        public void RespondToHandShake(CancellationToken cancellation = default(CancellationToken))
        {
            //using (TraceCorrelation.Open())
            //{
            using (NodeListener list = CreateListener().Where(m => m.Message.Payload is VerAckPayload || m.Message.Payload is RejectPayload))
            {
                //NodeServerTrace.Information("Responding to handshake");
                SendMessageAsync(MyVersion);
                IncomingMessage message = list.ReceiveMessage(cancellation);
                RejectPayload reject = message.Message.Payload as RejectPayload;
                if (reject != null)
                {
                    throw new ProtocolException("Version rejected " + reject.Code + " : " + reject.Reason);
                }

                SendMessageAsync(new VerAckPayload());
                State = NodeState.HandShaked;
            }
            //}
        }

        public void Disconnect()
        {
            Disconnect(null, null);
        }

        private int _Disconnecting;

        public void Disconnect(string reason, Exception exception = null)
        {
            DisconnectAsync(reason, exception);
            AssertNoListeningThread();
            _Connection.Disconnected.WaitOne();
        }

        private void AssertNoListeningThread()
        {
            if (_Connection._ListenerThreadId == Thread.CurrentThread.ManagedThreadId)
            {
                throw new InvalidOperationException("Using Disconnect on this thread would result in a deadlock, use DisconnectAsync instead");
            }
        }

        public void DisconnectAsync()
        {
            DisconnectAsync(null, null);
        }

        public void DisconnectAsync(string reason, Exception exception = null)
        {
            if (!IsConnected)
            {
                return;
            }

            if (Interlocked.CompareExchange(ref _Disconnecting, 1, 0) == 1)
            {
                return;
            }
            //using (TraceCorrelation.Open())
            //{
            //NodeServerTrace.Information("Disconnection request " + reason);
            State = NodeState.Disconnecting;
            _Connection.Cancel.Cancel();
            if (DisconnectReason == null)
            {
                DisconnectReason = new NodeDisconnectReason()
                {
                    Reason = reason,
                    Exception = exception
                };
            }
            //}
        }

        private TransactionOptions _PreferredTransactionOptions = TransactionOptions.All;

        /// <summary>
        /// Transaction options we would like
        /// </summary>
        public TransactionOptions PreferredTransactionOptions
        {
            get => _PreferredTransactionOptions;
            set => _PreferredTransactionOptions = value;
        }

        private TransactionOptions _SupportedTransactionOptions = TransactionOptions.None;

        /// <summary>
        /// Transaction options supported by the peer
        /// </summary>
        public TransactionOptions SupportedTransactionOptions => _SupportedTransactionOptions;

        /// <summary>
        /// Transaction options we prefer and which is also supported by peer
        /// </summary>
        public TransactionOptions ActualTransactionOptions => PreferredTransactionOptions & SupportedTransactionOptions;

        public NodeDisconnectReason DisconnectReason
        {
            get;
            private set;
        }

        public override string ToString()
        {
            return string.Format("{0} ({1})", State, Peer.Endpoint);
        }

        private Socket Socket => _Connection.Socket;

        internal TimeSpan PollHeaderDelay = TimeSpan.FromMinutes(1.0);

        /// <summary>
        /// Create a listener that will queue messages until diposed
        /// </summary>
        /// <returns>The listener</returns>
        /// <exception cref="System.InvalidOperationException">Thrown if used on the listener's thread, as it would result in a deadlock</exception>
        public NodeListener CreateListener()
        {
            AssertNoListeningThread();
            return new NodeListener(this);
        }

        private void AssertState(NodeState nodeState, CancellationToken cancellationToken = default(CancellationToken))
        {
            if (nodeState == NodeState.HandShaked && State == NodeState.Connected)
            {
                VersionHandshake(cancellationToken);
            }

            if (nodeState != State)
            {
                throw new InvalidOperationException("Invalid Node state, needed=" + nodeState + ", current= " + State);
            }
        }

        /// <summary>
        /// Retrieve transactions from the mempool by ids
        /// </summary>
        /// <param name="txIds">Transaction ids to retrieve</param>
        /// <param name="cancellationToken">Cancellation token</param>
        /// <returns>The transactions, if a transaction is not found, then it is not returned in the array.</returns>
        public Transaction[] GetMempoolTransactions(uint256[] txIds, CancellationToken cancellationToken = default(CancellationToken))
        {
            AssertState(NodeState.HandShaked);
            if (txIds.Length == 0)
            {
                return new Transaction[0];
            }

            List<Transaction> result = new List<Transaction>();
            using (NodeListener listener = CreateListener().Where(m => m.Message.Payload is TxPayload || m.Message.Payload is NotFoundPayload))
            {
                foreach (List<uint256> batch in txIds.Partition(500))
                {
                    SendMessageAsync(new GetDataPayload(batch.Select(txid => new InventoryVector()
                    {
                        Type = AddSupportedOptions(InventoryType.MSG_TX),
                        Hash = txid
                    }).ToArray()));
                    try
                    {
                        List<Transaction> batchResult = new List<Transaction>();
                        while (batchResult.Count < batch.Count)
                        {
                            CancellationTokenSource timeout = new CancellationTokenSource(TimeSpan.FromSeconds(10.0));
                            Payload payload = listener.ReceivePayload<Payload>(CancellationTokenSource.CreateLinkedTokenSource(cancellationToken, timeout.Token).Token);
                            if (payload is NotFoundPayload)
                            {
                                batchResult.Add(null);
                            }
                            else
                            {
                                batchResult.Add(((TxPayload)payload).Object);
                            }
                        }
                        result.AddRange(batchResult);
                    }
                    catch (OperationCanceledException)
                    {
                        if (cancellationToken.IsCancellationRequested)
                        {
                            throw;
                        }
                    }
                }
            }
            return result.Where(r => r != null).ToArray();
        }

        /// <summary>
        /// Add supported option to the input inventory type
        /// </summary>
        /// <param name="inventoryType">Inventory type (like MSG_TX)</param>
        /// <returns>Inventory type with options (MSG_TX | MSG_WITNESS_FLAG)</returns>
        public InventoryType AddSupportedOptions(InventoryType inventoryType)
        {
            if ((ActualTransactionOptions & TransactionOptions.Witness) != 0)
            {
                inventoryType |= InventoryType.MSG_WITNESS_FLAG;
            }

            return inventoryType;
        }

        public ProtocolData Network
        {
            get;
            set;
        }

        #region IDisposable Members

        public void Dispose()
        {
            Disconnect("Node disposed");
        }

        #endregion IDisposable Members

        /// <summary>
        /// Emit a ping and wait the pong
        /// </summary>
        /// <param name="cancellation"></param>
        /// <returns>Latency</returns>
        public TimeSpan PingPong(CancellationToken cancellation = default(CancellationToken))
        {
            using (NodeListener listener = CreateListener().OfType<PongPayload>())
            {
                PingPayload ping = new PingPayload()
                {
                    Nonce = RandomUtils.GetUInt64()
                };
                DateTimeOffset before = DateTimeOffset.UtcNow;
                SendMessageAsync(ping);

                while (listener.ReceivePayload<PongPayload>(cancellation).Nonce != ping.Nonce)
                {
                }
                DateTimeOffset after = DateTimeOffset.UtcNow;
                return after - before;
            }
        }
    }
}