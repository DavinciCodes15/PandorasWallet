#if !NOSOCKET

using Pandora.Client.Crypto.Protocol.Payloads;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace Pandora.Client.Crypto.Protocol.Behaviors
{
    [Flags]
    public enum PingPongMode
    {
        SendPing = 1,
        RespondPong = 2,
        Both = 3
    }

    /// <summary>
    /// The PingPongBehavior is responsible for firing ping message every PingInterval and responding with pong message, and close the connection if the Ping has not been completed after TimeoutInterval.
    /// </summary>
    public class PingPongBehavior : NodeBehavior
    {
        public PingPongBehavior()
        {
            Mode = PingPongMode.Both;
            TimeoutInterval = TimeSpan.FromMinutes(20.0); //Long time, if in middle of download of a large bunch of blocks, it can takes time
            PingInterval = TimeSpan.FromMinutes(2.0);
        }

        private PingPongMode _Mode;

        /// <summary>
        /// Whether the behavior send Ping and respond with Pong (Default : Both)
        /// </summary>
        public PingPongMode Mode
        {
            get
            {
                return _Mode;
            }
            set
            {
                AssertNotAttached();
                _Mode = value;
            }
        }

        private TimeSpan _TimeoutInterval;

        /// <summary>
        /// Interval after which an unresponded Ping will result in a disconnection. (Default : 20 minutes)
        /// </summary>
        public TimeSpan TimeoutInterval
        {
            get
            {
                return _TimeoutInterval;
            }
            set
            {
                AssertNotAttached();
                _TimeoutInterval = value;
            }
        }

        private TimeSpan _PingInterval;

        /// <summary>
        /// Interval after which a Ping message is fired after the last received Pong (Default : 2 minutes)
        /// </summary>
        public TimeSpan PingInterval
        {
            get
            {
                return _PingInterval;
            }
            set
            {
                AssertNotAttached();
                _PingInterval = value;
            }
        }

        protected override void AttachCore()
        {
            if (AttachedNode.PeerVersion != null && !PingVersion()) //If not handshaked, stil attach (the callback will also check version)
                return;
            AttachedNode.MessageReceived += AttachedNode_MessageReceived;
            AttachedNode.StateChanged += AttachedNode_StateChanged;
            RegisterDisposable(new Timer(Ping, null, 0, (int)PingInterval.TotalMilliseconds));
        }

        private bool PingVersion()
        {
            var node = AttachedNode;
            return node != null && node.ProtocolCapabilities.SupportPingPong;
        }

        private void AttachedNode_StateChanged(Node node, NodeState oldState)
        {
            if (node.State == NodeState.HandShaked)
                Ping(null);
        }

        private object cs = new object();

        private void Ping(object unused)
        {
            if (Monitor.TryEnter(cs))
            {
                try
                {
                    var node = AttachedNode;
                    if (node == null)
                        return;
                    if (!PingVersion())
                        return;
                    if (node.State != NodeState.HandShaked)
                        return;
                    if (_CurrentPing != null)
                        return;
                    _CurrentPing = new PingPayload();
                    _DateSent = DateTimeOffset.UtcNow;
                    node.SendMessageAsync(_CurrentPing);
                    _PingTimeoutTimer = new Timer(PingTimeout, _CurrentPing, (int)TimeoutInterval.TotalMilliseconds, Timeout.Infinite);
                }
                finally
                {
                    Monitor.Exit(cs);
                }
            }
        }

        /// <summary>
        /// Send a ping asynchronously
        /// </summary>
        public void Probe()
        {
            Ping(null);
        }

        private void PingTimeout(object ping)
        {
            var node = AttachedNode;
            if (node != null && ((PingPayload)ping) == _CurrentPing)
                node.DisconnectAsync("Pong timeout for " + ((PingPayload)ping).Nonce);
        }

        private Timer _PingTimeoutTimer;
        private volatile PingPayload _CurrentPing;
        private DateTimeOffset _DateSent;

        public TimeSpan Latency
        {
            get;
            private set;
        }

        private void AttachedNode_MessageReceived(Node node, IncomingMessage message)
        {
            if (!PingVersion())
                return;
            var ping = message.Message.Payload as PingPayload;
            if (ping != null && Mode.HasFlag(PingPongMode.RespondPong))
            {
                node.SendMessageAsync(new PongPayload()
                {
                    Nonce = ping.Nonce
                });
            }
            var pong = message.Message.Payload as PongPayload;
            if (pong != null &&
                Mode.HasFlag(PingPongMode.SendPing) &&
                _CurrentPing != null &&
                _CurrentPing.Nonce == pong.Nonce)
            {
                Latency = DateTimeOffset.UtcNow - _DateSent;
                ClearCurrentPing();
            }
        }

        private void ClearCurrentPing()
        {
            lock (cs)
            {
                _CurrentPing = null;
                _DateSent = default(DateTimeOffset);
                var timeout = _PingTimeoutTimer;
                if (timeout != null)
                {
                    timeout.Dispose();
                    _PingTimeoutTimer = null;
                }
            }
        }

        protected override void DetachCore()
        {
            AttachedNode.MessageReceived -= AttachedNode_MessageReceived;
            AttachedNode.StateChanged -= AttachedNode_StateChanged;
            ClearCurrentPing();
        }

        #region ICloneable Members

        public override object Clone()
        {
            return new PingPongBehavior()
            {
                Mode = Mode,
                PingInterval = PingInterval,
                TimeoutInterval = TimeoutInterval
            };
        }

        #endregion ICloneable Members
    }
}

#endif