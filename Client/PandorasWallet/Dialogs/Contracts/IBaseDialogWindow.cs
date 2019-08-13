using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace Pandora.Client.PandorasWallet.Dialogs.Contracts
{
    public interface IBaseDialogWindow
    {
        bool Execute();

        object ParentWindow { get; set; }
    }
}