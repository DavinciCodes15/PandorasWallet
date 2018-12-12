#if !NOSOCKET

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Pandora.Client.Crypto.Protocol.Filters
{
    public class ActionFilter : INodeFilter
    {
        private Action<IncomingMessage, Action> _OnIncoming;
        private Action<Node, Payload, Action> _OnSending;

        public ActionFilter(Action<IncomingMessage, Action> onIncoming = null, Action<Node, Payload, Action> onSending = null)
        {
            _OnIncoming = onIncoming ?? new Action<IncomingMessage, Action>((m, n) => n());
            _OnSending = onSending ?? new Action<Node, Payload, Action>((m, p, n) => n());
        }

        #region INodeFilter Members

        public void OnReceivingMessage(IncomingMessage message, Action next)
        {
            _OnIncoming(message, next);
        }

        public void OnSendingMessage(Node node, Payload payload, Action next)
        {
            _OnSending(node, payload, next);
        }

        #endregion INodeFilter Members
    }
}

#endif