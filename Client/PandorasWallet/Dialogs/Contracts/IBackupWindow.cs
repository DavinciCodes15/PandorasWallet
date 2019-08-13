using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Pandora.Client.PandorasWallet.Dialogs.Contracts
{
    public interface IBackupWindow : IBaseDialogWindow
    {
        event Func<string[]> On12WordsNeeded;

        event Action<string> OnBackupByFileNeeded;

        void SetRecoveryPhraseAutoCompleteWords(string[] aSetOfWords);
    }
}