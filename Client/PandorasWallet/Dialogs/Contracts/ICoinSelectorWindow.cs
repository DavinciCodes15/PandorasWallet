using Pandora.Client.PandorasWallet.Dialogs.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Pandora.Client.PandorasWallet.Dialogs.Contracts
{
    public interface ICoinSelectorWindow : IBaseDialogWindow
    {
        long[] SelectedCurrencyIds { get; }

        void AddCurrencies(IEnumerable<CurrencyViewItemModel> aListOfCurrencyModel);

        void Clear();
    }
}