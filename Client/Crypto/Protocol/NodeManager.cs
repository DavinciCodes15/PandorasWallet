using Pandora.Client.Crypto.Currencies;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Threading;
using System.Threading.Tasks;

namespace Pandora.Client.Crypto.Protocol
{
    public class NodeManager : IDisposable
    {
        private ConcurrentBag<string> FLocalNodesIP;

        private ProtocolData FProtocolData;

        private ConcurrentDictionary<int, NodeStructure> FNodes;

        public enum NodeAlertType
        {
            NodeDisconnect, NodeConnect, Critical, ActiveDisconnect, ActiveConnect, Exception
        }

        public delegate void NodeManagerAlert(NodeAlertType aType, string aDetailData, string aAddress = "");

        public event NodeEventMessageIncoming MainMessageReceived;

        public event NodeEventMessageIncoming AuxMessageReceived;

        public event NodeManagerAlert NodeManagerMessage;

        public Node MainConnection => Active == 0 ? null : FNodes[Active].Main;

        public Node AuxConnection => Active == 0 ? null : FNodes[Active].Auxiliar;

        private NodeStructure ActiveNodeStructure => Active == 0 ? null : FNodes[Active];

        public bool SimpleHandshake { get; set; }
        public int Active { get; set; }

        public bool Connected
        {
            get
            {
                if (ActiveNodeStructure == null)
                {
                    return false;
                }

                return ActiveNodeStructure.State == NodeStructureStatus.Online;
            }
        }

        public NodeManager(ProtocolData aProtocolData, IEnumerable<string> aIPCollection) : this(aProtocolData)
        {
            AddLocalIP(aIPCollection);
        }

        public NodeManager(ProtocolData aProtocolData)
        {
            FProtocolData = aProtocolData;
        }

        public void Connect()
        {
            if (FLocalNodesIP == null || FLocalNodesIP.Count == 0)
            {
                throw new Exception("No Local Addresses Set");
            }

            try
            {
                FNodes = new ConcurrentDictionary<int, NodeStructure>();

                foreach (string it in FLocalNodesIP)
                {
                    NetworkAddress lNodeAddress = new NetworkAddress(IPAddress.Parse(it), FProtocolData.DefaultPort);

                    NodeStructure lNodeStructure = new NodeStructure(lNodeAddress, this);

                    lNodeStructure.MainMessage += MainMessageReceived;

                    lNodeStructure.AuxMessage += AuxMessageReceived;

                    lNodeStructure.NodesDisconnected += LNodeStructure_NodesDisconnected;

                    FNodes.AddOrReplace(lNodeStructure.ID, lNodeStructure);
                }

                SelectActiveNode();

                //int lTimeout = 0;
                //while (NodeStructure.Active == 0)
                //{
                //    lTimeout++;
                //    Thread.Sleep(1000);
                //    if (lTimeout > 10)
                //    {
                //        throw new Exception("Failed to connect");
                //    }
                //}
            }
            catch
            {
                FNodes?.Clear();
                FNodes = null;
                throw;
            }
        }

        private void LNodeStructure_NodesDisconnected(int obj)
        {
            string lIPAddress = FNodes[obj].NetAddress.Endpoint.ToString();

            if (obj == Active)
            {
                NodeManagerMessage(NodeAlertType.ActiveDisconnect, "Connection Failed for active node", lIPAddress);
                SelectActiveNode();
            }
            else
            {
                NodeManagerMessage(NodeAlertType.NodeDisconnect, "Connection Failed for node", lIPAddress);
            }
        }

        private void SelectActiveNode()
        {
            int lActiveNode = FNodes.Where(x => x.Value.State == NodeStructureStatus.Online).Select(x => x.Key).FirstOrDefault();

            Active = lActiveNode;

            if (lActiveNode == 0)
            {
                NodeManagerMessage(NodeAlertType.Critical, "No nodes connected");

                Task.Run(() =>
                {
                    Thread.Sleep(5000);
                    SelectActiveNode();
                });
            }
            else
            {
                NodeManagerMessage(NodeAlertType.ActiveConnect, "Active node Connected", FNodes[lActiveNode].NetAddress.Endpoint.ToString());
            }
        }

        public void AddLocalIP(IEnumerable<string> aIPCollection)
        {
            if (FLocalNodesIP == null)
            {
                FLocalNodesIP = new ConcurrentBag<string>();
            }

            foreach (string lIP in aIPCollection)
            {
                FLocalNodesIP.Add(lIP);
            }
        }

        private bool TryToConnect(out Node aMainNode, out Node aAuxNode, NetworkAddress aAddress)
        {
            NodeConnectionParameters NodeConnectionParameters = new NodeConnectionParameters();
            NodeConnectionParameters.TemplateBehaviors.FindOrCreate<Behaviors.PingPongBehavior>();

            try
            {
                aMainNode = Node.Connect(FProtocolData, aAddress, NodeConnectionParameters);
                aAuxNode = Node.Connect(FProtocolData, aAddress, NodeConnectionParameters);
            }
            catch
            {
                aMainNode = null;
                aAuxNode = null;
                return false;
            }

            if (aMainNode.State == NodeState.Failed || aAuxNode.State == NodeState.Failed)
            {
                aMainNode = null;
                aAuxNode = null;
                return false;
            }

            uint lTimeoutCounter = 0;

            do
            {
                lTimeoutCounter++;

                if (lTimeoutCounter > 3)
                {
                    return false;
                }

                try
                {
                    aMainNode.VersionHandshake(aIsSimpleHandshake: SimpleHandshake);
                }
                catch
                {
                    System.Threading.Thread.Sleep(1000);
                    continue;
                }

                try
                {
                    aAuxNode.VersionHandshake(aIsSimpleHandshake: SimpleHandshake);
                }
                catch
                {
                    System.Threading.Thread.Sleep(1000);
                    continue;
                }
            } while (!(aMainNode.State == NodeState.HandShaked));

            return true;
        }

        private enum NodeStructureStatus
        {
            Empty, Online, Offline
        }

        #region IDisposable Support

        private bool disposedValue = false; // To detect redundant calls

        protected virtual void Dispose(bool disposing)
        {
            if (!disposedValue)
            {
                if (disposing)
                {
                    foreach (KeyValuePair<int, NodeStructure> it in FNodes)
                    {
                        it.Value.Dispose();
                    }
                    FNodes.Clear();
                    FNodes = null;
                }

                // TODO: free unmanaged resources (unmanaged objects) and override a finalizer below.
                // TODO: set large fields to null.

                disposedValue = true;
            }
        }

        // TODO: override a finalizer only if Dispose(bool disposing) above has code to free unmanaged resources.
        // ~NodeManager() {
        //   // Do not change this code. Put cleanup code in Dispose(bool disposing) above.
        //   Dispose(false);
        // }

        // This code added to correctly implement the disposable pattern.
        public void Dispose()
        {
            // Do not change this code. Put cleanup code in Dispose(bool disposing) above.
            Dispose(true);
            // TODO: uncomment the following line if the finalizer is overridden above.
            // GC.SuppressFinalize(this);
        }



        #endregion IDisposable Support

        private class NodeStructure : IDisposable
        {
            private static int FCurrentIDNumber;
            public NetworkAddress NetAddress { get; private set; }

            public NodeManager Parent { get; private set; }

            private CancellationTokenSource fCancelSource;
            private Task FKeepAliveTask;



            public bool isActive()
            {
                return Parent.Active == ID;
            }

            public NodeStructure(NetworkAddress aNodeAddress, NodeManager aParent)
            {
                ID = FCurrentIDNumber + 1;
                FCurrentIDNumber++;

                NetAddress = aNodeAddress;
                Parent = aParent;
                Main = null;
                Auxiliar = null;
                fCancelSource = new CancellationTokenSource();

                FKeepAliveTask = Task.Run(() => KeepAlive(fCancelSource.Token));
            }

            public int ID { get; private set; }

            public event NodeEventMessageIncoming MainMessage;

            public event NodeEventMessageIncoming AuxMessage;

            public event Action<int> NodesDisconnected;

            public NodeStructureStatus State
            {
                get; private set;
            }

            private Node FMain;

            public Node Main
            {
                get => FMain; set

                {
                    if (value == null)
                    {
                        return;
                    }

                    FMain = value;
                    FMain.MessageReceived += Main_MessageReceived;
                    FMain.StateChanged += Node_StateChanged;
                }
            }

            private void KeepAlive(CancellationToken aCancelToken)
            {
                while (!aCancelToken.IsCancellationRequested)
                {
                    if ((State == NodeStructureStatus.Empty || State == NodeStructureStatus.Offline) && !FDisconnecting)
                    {
                        if (Parent.TryToConnect(out Node lMainNode, out Node lAuxNode, NetAddress))
                        {
                            Main = lMainNode;
                            Auxiliar = lAuxNode;
                            State = NodeStructureStatus.Online;
                            Parent.NodeManagerMessage(NodeAlertType.NodeConnect, "Node connected", NetAddress.Endpoint.ToString());
                        }
                    }

                    aCancelToken.ThrowIfCancellationRequested();

                    Thread.Sleep(1000);
                }
            }

            private void Main_MessageReceived(Node node, IncomingMessage message)
            {
                if (isActive())
                {
                    MainMessage(node, message);
                }
            }

            private Node FAux;

            public Node Auxiliar
            {
                get => FAux; set

                {
                    if (value == null)
                    {
                        return;
                    }

                    FAux = value;
                    FAux.MessageReceived += Auxiliar_MessageReceived;
                    FAux.StateChanged += Node_StateChanged;
                }
            }

            private void Auxiliar_MessageReceived(Node node, IncomingMessage message)
            {
                if (isActive())
                {
                    AuxMessage(node, message);
                }
            }

            private void Node_StateChanged(Node node, NodeState oldState)
            {
                if (node.State != NodeState.HandShaked && !FDisconnecting)
                {
                    FDisconnecting = true;

                    Main.MessageReceived -= Auxiliar_MessageReceived;
                    Main.StateChanged -= Node_StateChanged;
                    Main = null;

                    Auxiliar.MessageReceived -= Auxiliar_MessageReceived;
                    Auxiliar.StateChanged -= Node_StateChanged;
                    Auxiliar = null;

                    State = NodeStructureStatus.Offline;
                    NodesDisconnected(ID);
                    FDisconnecting = false;
                }
            }

            private bool FDisconnecting;

            #region IDisposable Support

            private bool disposedValue = false; // To detect redundant calls

            protected virtual void Dispose(bool disposing)
            {
                if (!disposedValue)
                {
                    if (disposing)
                    {
                        fCancelSource.Cancel();
                        FMain = null;
                        FAux = null;
                    }

                    // TODO: free unmanaged resources (unmanaged objects) and override a finalizer below.
                    // TODO: set large fields to null.

                    disposedValue = true;
                }
            }

            // TODO: override a finalizer only if Dispose(bool disposing) above has code to free unmanaged resources.
            // ~NodeStructure() {
            //   // Do not change this code. Put cleanup code in Dispose(bool disposing) above.
            //   Dispose(false);
            // }

            // This code added to correctly implement the disposable pattern.
            public void Dispose()
            {
                // Do not change this code. Put cleanup code in Dispose(bool disposing) above.
                Dispose(true);
                // TODO: uncomment the following line if the finalizer is overridden above.
                // GC.SuppressFinalize(this);
            }

            #endregion IDisposable Support
        }
    }
}