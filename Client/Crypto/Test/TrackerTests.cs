using System;
using System.Collections.Generic;
using System.IO;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System.Net;

namespace Pandora.Client.Crypto.Test
{
    [TestClass]
    public class TrackerTests
    {
        private ushort[] Get12NumbersOf11Bits(string a32ByteHex)
        {
            List<ushort> lResult = new List<ushort>();
            ushort lLast4BitSum = 0;
            ulong lCurrentNum = 0;
            int lRemainder = 0;
            if (string.IsNullOrEmpty(a32ByteHex) || a32ByteHex.Length != 32)
                throw new ArgumentOutOfRangeException("String must have a length of 32.");
            string lHex;
            for (int i = 0; i < 11; i++)
            {
                if (i == 10)
                    lHex = a32ByteHex.Substring(i * 3, 2);
                else
                    lHex = a32ByteHex.Substring(i * 3, 3);
                ushort lNum = ushort.Parse(lHex, System.Globalization.NumberStyles.HexNumber);
                lCurrentNum |= (ushort)(lNum << lRemainder);
                lResult.Add((ushort)(lCurrentNum & 2047));
                lCurrentNum >>= 11;
                lRemainder++;
                lLast4BitSum ^= (ushort)((lNum & 0xf) ^ ((lNum >> 4) & 0xf) ^ (lNum >> 8));
            }
            lCurrentNum |= (ushort)(lLast4BitSum << 7);
            lResult.Add((ushort)lCurrentNum);
            return lResult.ToArray();
        }

        [TestMethod]
        public void TestGet12()
        {
            var Value = Get12NumbersOf11Bits("618AF711D8A040F6B95D41ABE455D92F");
            Assert.AreEqual(12, Value.Length);
        }

        //    [TestMethod]
        //    public void TestChainDownload()
        //    {
        //        // parse address from command line
        //        //var addr = BitcoinAddress.Create("2N9UbQou3sp7Y8HzLF1XrBkcTjoWFaf3mLL", FNetwork);
        //        //var addr = BitcoinAddress.Create("mqAMJHKcw3aNuoZU9tpepP1oiQaAxBMLLs");
        //        // use local trusted node
        //        //NetworkAddress peer = new NetworkAddress(IPAddress.Parse("82.202.207.182"), FNetwork == Network.TestNet ? 18333 : 8333); ;

        //        List<NetworkAddress> peer = new List<NetworkAddress>();
        //        peer.Add(new NetworkAddress(IPAddress.Parse("192.168.10.71"), FNetwork.DefaultPort));

        //        //foreach (IPAddress it in Dns.GetHostAddresses("dnsseed.dash.org"))
        //        //{
        //        //    if (it.IsIPv4())
        //        //    {
        //        //        peer.Add(new NetworkAddress(it, 9999));
        //        //    }
        //        //}

        //        // initial scan location
        //        var scanLocation = new BlockLocator();
        //        scanLocation.Blocks.Add(FNetwork.GetGenesis().GetHash());
        //        var skipBefore = DateTimeOffset.Parse("2018-07-03 16:00:00");

        //        // load chain
        //        var chain = new ConcurrentChain(FNetwork);
        //        //if (File.Exists(ChainFile))
        //        //{
        //        //    chain.Load(File.ReadAllBytes(ChainFile), FNetwork);
        //        //}

        //        // connect node
        //        ConnectNode(peer, chain, scanLocation, skipBefore);

        //        uint counter = 0;

        //        int chainHeight = 0;
        //        while (true)
        //        {
        //            if (chain.Height != chainHeight)
        //            {
        //                chainHeight = chain.Height;
        //                System.Diagnostics.Debug.WriteLine("Chain height: {0}", chain.Height);
        //                //using (var fs = File.Open(ChainFile, FileMode.Create))
        //                //    chain.WriteTo(fs);
        //                break;
        //            }
        //            System.Threading.Thread.Sleep(5000);
        //            counter++;
        //            if (counter > 5)
        //                break;
        //        }

        //        Assert.IsTrue(chainHeight > 0);
        //    }

        //    private void ConnectNode(List<NetworkAddress> peer, ConcurrentChain chain, BlockLocator scanLocation, DateTimeOffset skipBefore)
        //    {
        //        //var script = addr.ScriptPubKey; // standard "pay to pubkey hash" script

        //        var parameters = new NodeConnectionParameters();

        //        // ping pong
        //        parameters.TemplateBehaviors.FindOrCreate<PingPongBehavior>();

        //        ChainBehavior lBehavior = new ChainBehavior(chain);

        //        lBehavior.SkipPoWCheck = true;

        //        // chain behavior keep chain in sync
        //        parameters.TemplateBehaviors.Add(lBehavior);

        //        // tracker behavior tracks our address
        //        parameters.TemplateBehaviors.Add(new TrackerBehavior(new Tracker(), chain));

        //        var addressManager = new AddressManager();
        //        addressManager.Add(peer, IPAddress.Loopback);

        //        parameters.TemplateBehaviors.Add(new AddressManagerBehavior(addressManager));

        //        var group = new NodesGroup(FNetwork, parameters);
        //        group.AllowSameGroup = true;
        //        group.MaximumNodeConnection = 5;
        //        group.Requirements.SupportSPV = true;
        //        group.Connect();
        //        group.ConnectedNodes.Added += (s, e) =>
        //        {
        //            var node = e.Node;
        //            node.MessageReceived += (node1, message) =>
        //            {
        //                if (message.Message.Command != "headers" && message.Message.Command != "merkleblock")
        //                {
        //                    //if (message.Message.Payload is TxPayload)
        //                    //{
        //                    //    var txPayload = (TxPayload)message.Message.Payload;
        //                    //    foreach (var output in txPayload.Object.Outputs)
        //                    //        if (output.ScriptPubKey == script)
        //                    //            System.Diagnostics.Debug.WriteLine("tx {0}", txPayload.Object.GetHash());
        //                    //}
        //                    //else
        //                    //    System.Diagnostics.Debug.WriteLine(message.Message.Command);
        //                }
        //            };

        //            node.Disconnected += n =>
        //            {
        //                // TrackerBehavior has probably disconnected the node because of too many false positives...
        //                System.Diagnostics.Debug.WriteLine("Disconnected!");

        //                // save progress
        //                var _trackerBehavior = n.Behaviors.Find<TrackerBehavior>();
        //                scanLocation = _trackerBehavior.CurrentProgress;
        //            };

        //            // start tracker scanning
        //            //var trackerBehavior = node.Behaviors.Find<TrackerBehavior>();
        //            //System.Diagnostics.Debug.WriteLine("Tracking {0} ({1})", addr, script);
        //            //trackerBehavior.Tracker.Add(script);
        //            //trackerBehavior.Tracker.NewOperation += (Tracker sender, Tracker.IOperation trackerOperation) =>
        //            //{
        //            //    System.Diagnostics.Debug.WriteLine("tracker operation: {0}", trackerOperation.ToString());
        //            //};

        //            //trackerBehavior.Scan(scanLocation, skipBefore);
        //            //trackerBehavior.SendMessageAsync(new MempoolPayload());

        //            //trackerBehavior.RefreshBloomFilter();
        //        };
        //    }
    }
}