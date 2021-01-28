using Pandora.Client.ClientLib;
using Pandora.Client.ClientLib.Contracts;
using Pandora.Client.Crypto.Currencies;
using Pandora.Client.PandorasWallet.Models;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Numerics;
using System.Text;
using System.Threading.Tasks;

namespace Pandora.Client.PandorasWallet.Dialogs.Models
{
    public class GUIToken : IGUICurrency, IGUIToken
    {
        private GUICurrencyTxAndBalanceHelper FGUICurrencyHelper;

        public GUIToken(IGUICurrency aParentCurrency)
        {
            ParentCurrency = aParentCurrency ?? throw new ArgumentNullException(nameof(aParentCurrency));
            FGUICurrencyHelper = new GUICurrencyTxAndBalanceHelper(this);
        }

        public GUIToken(IClientCurrencyToken aToken, IGUICurrency aParentCurrency) : this(aParentCurrency)
        {
            Id = (aToken?.ID ?? throw new ArgumentNullException(nameof(aToken))) > 0 ? throw new ArgumentOutOfRangeException(nameof(aToken), "Token ID must be negative") : aToken.ID;
            Name = aToken.Name;
            Ticker = aToken.Ticker;
            Precision = aToken.Precision;
            Icon = aToken.Icon;
            ContractAddress = aToken.ContractAddress;
        }

        public IStatusDetails StatusDetails => ParentCurrency.StatusDetails;
        public IGUICurrency ParentCurrency { get; private set; }

        public decimal DefaultCurrencyPricePerCoin { get; set; }
        public long BlockHeight { get => ParentCurrency.BlockHeight; set => ParentCurrency.BlockHeight = value; }
        public IEnumerable<GUIAccount> Addresses { get => ParentCurrency.Addresses; set => ParentCurrency.Addresses = value; }

        public string LastAddress => ParentCurrency.LastAddress;

        public long Id { get; set; }

        public string Name { get; set; }

        public string Ticker { get; set; }

        public ushort Precision { get; set; }

        public DateTime LiveDate => ParentCurrency.LiveDate;

        public int MinConfirmations => ParentCurrency.MinConfirmations;

        public byte[] Icon { get; set; }

        public long FeePerKb => ParentCurrency.FeePerKb;

        public ChainParams ChainParamaters => ParentCurrency.ChainParamaters;

        public CurrencyStatus CurrentStatus { get => ParentCurrency.CurrentStatus; set => ParentCurrency.CurrentStatus = value; }

        public string ContractAddress { get; set; }

        public IGUICurrencyBalance Balances => FGUICurrencyHelper;

        Icon IGUIToken.Icon { get => Globals.BytesToIcon(Icon); set => Icon = Globals.IconToBytes(value); }

        public IGUICurrencyTransactional Transactions => FGUICurrencyHelper;

        public long ParentCurrencyID => ParentCurrency.Id;

        public decimal AmountToDecimal(BigInteger aAmount)
        {
            return (decimal) aAmount / Convert.ToDecimal(Math.Pow(10, Precision));
        }

        public long AmountToLong(decimal aAmount)
        {
            return Convert.ToInt64(aAmount * Convert.ToDecimal(Math.Pow(10, Precision)));
        }

        public BigInteger AmountToBigInteger(decimal aAmount)
        {
            return new BigInteger(aAmount * Convert.ToDecimal(Math.Pow(10, Precision)));
        }

        public ICurrencyItem CopyFrom(ICurrencyItem aDestinationItem)
        {
            return new GUIToken(this.ParentCurrency)
            {
                Id = Id,
                Name = Name,
                Ticker = Ticker,
                Precision = Precision,
                ContractAddress = ContractAddress,
                Icon = Icon
            };
        }
    }
}