using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Pandora.Client.PandorasWallet.Dialogs.Contracts
{
    public delegate void ExecuteRestoreDelegate(bool aIsByFile);

    public interface IRestoreWindow : IBaseDialogWindow
    {
        event ExecuteRestoreDelegate OnExecuteRestore;

        event EventHandler OnFinishRestore;

        string Words { get; }
        string RestoreFilePath { get; }
    }
}