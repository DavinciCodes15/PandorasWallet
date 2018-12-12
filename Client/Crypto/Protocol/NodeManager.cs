using Pandora.Client.Crypto.Currencies;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Net;
using System.Threading.Tasks;

namespace Pandora.Client.Crypto.Protocol
{
    public class NodeManager
    {
        private ConcurrentBag<string> FIP;
        private string FDNSAddress;
        private ProtocolData FProtocolData;

        private Node FLocalNode;
        private Node FAuxLocal;

        private Node FDnsNode;
        private Node FAuxDns;

        public delegate void NodeManagerAlert(string aAddress, string aDetailData = "");

        public event NodeEventMessageIncoming MainMessageReceived;

        public event NodeEventMessageIncoming AuxMessageReceived;

        public event NodeManagerAlert DNSFailure;

        public event NodeManagerAlert LocalNodeFailure;

        public Node MainConnection
        {
            get
            {
                if (FLocalNode == null || FLocalNode?.State != NodeState.HandShaked)
                {
                    return FDnsNode;
                }
                else
                {
                    return FLocalNode;
                }
            }
        }

        public Node AuxConnection
        {
            get
            {
                if (FLocalNode == null || FLocalNode?.State != NodeState.HandShaked)
                {
                    return FAuxDns;
                }
                else
                {
                    return FAuxLocal;
                }
            }
        }

        public bool SimpleHandshake { get; set; }

        public bool Connected => (FLocalNode?.State == NodeState.HandShaked) || (FDnsNode?.State == NodeState.HandShaked);

        public NodeManager(string aDNSAddress, ProtocolData aProtocolData, IEnumerable<string> aIPCollection) : this(aDNSAddress, aProtocolData)
        {
            AddLocalIP(aIPCollection);
        }

        public NodeManager(string aDNSAddress, ProtocolData aProtocolData)
        {
            FDNSAddress = aDNSAddress;
            FProtocolData = aProtocolData;
        }

        public void AddLocalIP(IEnumerable<string> aIPCollection)
        {
            if (FIP == null)
            {
                FIP = new ConcurrentBag<string>();
            }

            foreach (string lIP in aIPCollection)
            {
                FIP.Add(lIP);
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

                if (lTimeoutCounter > 6)
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

        private bool TryToConnect(out Node aMainNode, out Node aAuxNode, string aIP, int aPort = -1)
        {
            int lPort = aPort != -1 ? aPort : FProtocolData.DefaultPort;

            NetworkAddress lAddress = new NetworkAddress(IPAddress.Parse(aIP), lPort);

            return TryToConnect(out aMainNode, out aAuxNode, lAddress);
        }

        public void Connect()
        {
            if (FIP == null || FIP.Count == 0)
            {
                throw new Exception("No Local Addresses Set");
            }

            Task.Run(() =>
            {
                ConnectToLocal();

                if (!string.IsNullOrEmpty(FDNSAddress))
                {
                    ConnectToDNS();
                }

                if (FLocalNode == null && (FDnsNode != null && FDnsNode.State == NodeState.HandShaked))
                {
                    if (!string.IsNullOrEmpty(FDNSAddress))
                    {
                        FDnsNode.MessageReceived += Main_MessageReceived;
                        FAuxDns.MessageReceived += Aux_MessageReceived;
                    }
                    else
                    {
                        throw new Exception("Failed To Connect");
                    }

                    if (FLocalNode != null)
                    {
                        FLocalNode.MessageReceived -= Main_MessageReceived;
                        FAuxLocal.MessageReceived -= Aux_MessageReceived;
                    }
                }
                else
                {
                    if (FLocalNode == null)
                    {
                        throw new Exception("Failed To Connect");
                    }

                    FLocalNode.MessageReceived += Main_MessageReceived;
                    FAuxLocal.MessageReceived += Aux_MessageReceived;

                    if (!string.IsNullOrEmpty(FDNSAddress))
                    {
                        FDnsNode.MessageReceived -= Main_MessageReceived;
                        FAuxDns.MessageReceived -= Aux_MessageReceived;
                    }
                }
            });
        }

        private void ConnectToLocal()
        {
            foreach (string aIP in FIP)
            {
                if (TryToConnect(out FLocalNode, out FAuxLocal, aIP))
                {
                    break;
                }
                LocalNodeFailure(aIP, "Unable to Connect to Local Node");
            }

            if (FLocalNode != null)
            {
                FLocalNode.StateChanged += Node_StateChanged;
            }
            else
            {
                Task.Run(() =>
                {
                    System.Threading.Thread.Sleep(5000);
                    ConnectToLocal();
                });
            }
        }

        private void ConnectToDNS()
        {
            bool lDisconnectedFlag = true;
            do
            {
                List<NetworkAddress> lDnsIPs = GetDnsIPs(FDNSAddress);
                foreach (NetworkAddress it in lDnsIPs)
                {
                    if (TryToConnect(out FDnsNode, out FAuxDns, it))
                    {
                        lDisconnectedFlag = false;
                        break;
                    }
                }
                if (lDisconnectedFlag)
                {
                    DNSFailure(FDNSAddress, "Failed to connect to nodes given by DNS");
                    System.Threading.Thread.Sleep(10);
                }
            } while (lDisconnectedFlag);

            FDnsNode.StateChanged += FDnsNode_StateChanged;
        }

        private void FDnsNode_StateChanged(Node node, NodeState oldState)
        {
            if (node.State == NodeState.Disconnecting || node.State == NodeState.Failed || node.State == NodeState.Offline)
            {
                Task.Run(() => ConnectToDNS());
            }
        }

        private void Aux_MessageReceived(Node anode, IncomingMessage amessage)
        {
            AuxMessageReceived(anode, amessage);
        }

        private void Main_MessageReceived(Node anode, IncomingMessage amessage)
        {
            MainMessageReceived(anode, amessage);
        }

        private List<NetworkAddress> GetDnsIPs(string aDNSAddress)
        {
            List<NetworkAddress> lDnsIps = new List<NetworkAddress>();

            if (!string.IsNullOrEmpty(aDNSAddress))
            {
                try
                {
                    foreach (IPAddress it in Dns.GetHostAddresses(aDNSAddress))
                    {
                        if (it.IsIPv4())
                        {
                            lDnsIps.Add(new NetworkAddress(it, FProtocolData.DefaultPort));
                        }
                    }
                }
                catch (Exception ex)
                {
                    DNSFailure(aDNSAddress, ex.Message);
                }
            }

            return lDnsIps;
        }

        private void Node_StateChanged(Node node, NodeState oldState)
        {
            if (node.State == NodeState.Disconnecting || node.State == NodeState.Failed || node.State == NodeState.Offline)
            {
                Connect();
            }
        }
    }
}