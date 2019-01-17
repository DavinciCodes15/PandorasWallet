using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Pandora.Client.Crypto.Currencies
{
    internal class ActionDisposable : IDisposable
    {
        private Action onEnter, onLeave;

        public ActionDisposable(Action onEnter, Action onLeave)
        {
            this.onEnter = onEnter;
            this.onLeave = onLeave;
            onEnter();
        }

        #region IDisposable Members

        public void Dispose()
        {
            onLeave();
        }

        #endregion IDisposable Members
    }
}