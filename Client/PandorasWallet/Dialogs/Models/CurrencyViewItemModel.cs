using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Pandora.Client.PandorasWallet.Dialogs.Models
{
    public class CurrencyViewItemModel
    {
        public CurrencyViewItemModel()
        {

        }

        public CurrencyViewItemModel(long aCurrencyID, string aCurrencyName, string aCurrencySymbol, Icon aCurrencyIcon, string aStatus)
        {
            CurrencyID = aCurrencyID;
            CurrencyName = aCurrencyName;
            CurrencySymbol = aCurrencySymbol;
            CurrencyIcon = aCurrencyIcon;
            Status = aStatus;
        }

        public long CurrencyID { get; set; }
        public string CurrencyName { get; set; }
        public string CurrencySymbol { get; set; }
        public Icon CurrencyIcon { get; set; }
        public string Status { get; set; }
    }
}