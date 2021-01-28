using Pandora.Client.ClientLib.Contracts;
using Pandora.Client.PandorasWallet.Dialogs.Models;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace Pandora.Client.PandorasWallet.Dialogs
{
    public partial class AddTokenDialog : BaseDialog
    {
        private bool FOkPressed;
        private Dictionary<string, IGUIToken> FTokens;
        private List<IGUICurrency> FParentCurrencies;
        private static ConcurrentDictionary<string, IGUIToken> FCacheTokens;

        public event Func<AddTokenDialog, long, string, ICurrencyToken> OnTokenAddressChanged;

        static AddTokenDialog()
        {
            FCacheTokens = new ConcurrentDictionary<string, IGUIToken>();
        }

        public AddTokenDialog()
        {
            InitializeComponent();
            FTokens = new Dictionary<string, IGUIToken>();
            FParentCurrencies = new List<IGUICurrency>();
            lstViewTokens.SmallImageList = TokenIconsList;
            lstViewTokens.LargeImageList = TokenIconsList;
            comboBoxNetwork.DisplayMember = "Name";
            comboBoxNetwork.ValueMember = "Id";
        }

        public int TokenDecimals { get => int.Parse(txtBoxTokenDecimals.Text); set => txtBoxTokenDecimals.Text = value.ToString(); }
        public string TokenAddress { get => TxtBoxTokenAddress.Text; set => TxtBoxTokenAddress.Text = value; }
        public string TokenName { get => txtBoxTokenName.Text; set => txtBoxTokenName.Text = value; }
        public string TokenSymbol { get => txtBoxTokenSymbol.Text; set => txtBoxTokenSymbol.Text = value; }
        public GUICurrency SelectedCurrencyNetwork => comboBoxNetwork.SelectedItem as GUICurrency;
        public Image TokenIcon { get => picToken.Image; set => picToken.Image = value; }
        public IGUIToken SelectedToken { get; private set; }

        private void TxtBoxTokenAddress_TextChanged(object sender, EventArgs e)
        {
            var lAddress = TxtBoxTokenAddress.Text;
            if (Regex.IsMatch(lAddress, @"^0[Xx][a-fA-F0-9]{40}$"))
            {
                try
                {
                    this.SetWaitCursor();
                    if (!FTokens.ContainsKey(TokenAddress) && !FCacheTokens.ContainsKey(TokenAddress))
                    {
                        lstViewTokens.Enabled = false;
                        btnOK.Enabled = false;
                        this.SetWaitCursor();
                        SetTokenInfoLoading();
                        TxtBoxTokenAddress.Enabled = false;
                        OnTokenAddressChanged?.BeginInvoke(this, ((IGUICurrency) comboBoxNetwork.SelectedItem)?.Id ?? -1, lAddress, AddressChangedCallback, null);
                    }
                    else
                    {
                        if (FTokens.TryGetValue(TokenAddress, out IGUIToken lTokenItem))
                        {
                            var lListViewItem = lstViewTokens.Items.Find(lTokenItem.ContractAddress, false).FirstOrDefault();
                            lListViewItem.Selected = true;
                        }
                        else if (FCacheTokens.TryGetValue(TokenAddress, out lTokenItem))
                        {
                            SetTokenInfo(lTokenItem);
                            btnOK.Enabled = true;
                        }
                    }
                }
                catch (Exception ex)
                {
                    this.StandardExceptionMsgBox(ex);
                }
                finally
                {
                    this.SetArrowCursor();
                }
            }
            else
            {
                SetTokenInfoNoData();
                btnOK.Enabled = false;
            }
        }

        private void AddressChangedCallback(IAsyncResult aResult)
        {
            try
            {
                var lCurrencyToken = (ICurrencyToken) OnTokenAddressChanged.EndInvoke(aResult);
                if (lCurrencyToken != null)
                {
                    var lBitmapIcon = new Bitmap(picToken.BackgroundImage);

                    this.Invoke(new Action(() =>
                    {
                        SelectedToken = new GUIToken(SelectedCurrencyNetwork)
                        {
                            ContractAddress = lCurrencyToken.ContractAddress,
                            Ticker = lCurrencyToken.Ticker,
                            Precision = lCurrencyToken.Precision,
                            Name = lCurrencyToken.Name,
                            Icon = Globals.IconToBytes(Icon.FromHandle(lBitmapIcon.GetHicon()))
                        };
                        FCacheTokens.TryAdd(SelectedToken.ContractAddress, SelectedToken);
                        SetTokenInfo(SelectedToken);
                        btnOK.Enabled = true;
                    }));
                }
                else
                {
                    TxtBoxTokenAddress.Focus();
                    this.Invoke(new Action(SetTokenInfoNoData));
                }
            }
            catch (Exception ex)
            {
                this.Invoke(new Action(() => this.StandardErrorMsgBox("Failed to retrieve token data", ex.Message)));
            }
            finally
            {
                this.Invoke(new Action(() =>
                {
                    this.SetArrowCursor();
                    lstViewTokens.Enabled = true;
                    TxtBoxTokenAddress.Enabled = true;
                }));
            }
        }

        private void SetTokenInfo(IGUIToken aTokenModel)
        {
            TokenName = aTokenModel.Name;
            TokenSymbol = aTokenModel.Ticker;
            TokenDecimals = aTokenModel.Precision;
        }

        private void SetTokenInfoLoading()
        {
            TokenName = "Loading...";
            TokenSymbol = "Loading...";
            txtBoxTokenDecimals.Text = "Loading...";
        }

        private void SetTokenInfoNoData()
        {
            TokenName = "-NO DATA-";
            TokenSymbol = "-NO DATA-";
            txtBoxTokenDecimals.Text = "-NO DATA-";
        }

        public void AddTokenItem(IGUIToken aTokenItem)
        {
            if (aTokenItem == null || FTokens.ContainsKey(aTokenItem.ContractAddress))
                return;
            FTokens.Add(aTokenItem.ContractAddress, aTokenItem);
            SetParentCurrency(aTokenItem.ParentCurrency);
            TokenIconsList.Images.Add(aTokenItem.ContractAddress, aTokenItem.Icon);
            lstViewTokens.Items.Add(ConstructListViewItem(aTokenItem));
            if (lstViewTokens.Enabled == false)
                lstViewTokens.Enabled = true;
        }

        public void AddParentCurrency(ICurrencyItem aCurrencyTokenModel)
        {
            SetParentCurrency(new GUICurrency(aCurrencyTokenModel));
        }

        private void SetParentCurrency(IGUICurrency aCurrencyModel)
        {
            if (!FParentCurrencies.Any(lCurrency => lCurrency.Id == aCurrencyModel.Id))
            {
                FParentCurrencies.Add(aCurrencyModel);
                comboBoxNetwork.Items.Add(aCurrencyModel);
                comboBoxNetwork.SelectedIndex = 0;
            }
        }

        private ListViewItem ConstructListViewItem(IGUIToken aTokenItem)
        {
            var lListViewItem = new ListViewItem();
            lListViewItem.Text = aTokenItem.Name;
            lListViewItem.ImageKey = aTokenItem.ContractAddress;
            lListViewItem.Name = aTokenItem.ContractAddress;
            lListViewItem.Tag = aTokenItem;
            lListViewItem.SubItems.Add(aTokenItem.Ticker);
            return lListViewItem;
        }

        private void AddTokenDialog_FormClosing(object sender, FormClosingEventArgs e)
        {
            if (FOkPressed)
            {
                FOkPressed = false;
                if (TxtBoxTokenAddress.ReadOnly)
                    SelectedToken = (IGUIToken) lstViewTokens.SelectedItems[0].Tag;
                else
                    SelectedToken = FCacheTokens[TokenAddress];
            }
        }

        private void btnOK_Click(object sender, EventArgs e) => FOkPressed = true;

        private void lstViewTokens_SelectedIndexChanged(object sender, EventArgs e)
        {
            TxtBoxTokenAddress.TextChanged -= TxtBoxTokenAddress_TextChanged;

            if (lstViewTokens.SelectedItems.Count == 0)
            {
                TokenAddress = string.Empty;
                TxtBoxTokenAddress.ReadOnly = false;
                btnOK.Enabled = false;
                SetTokenInfoNoData();
            }
            else
            {
                var lSelectedItemToken = (IGUIToken) lstViewTokens.SelectedItems[0].Tag;
                TokenAddress = lSelectedItemToken.ContractAddress;
                TxtBoxTokenAddress.ReadOnly = true;
                comboBoxNetwork.SelectedItem = comboBoxNetwork.Items.IndexOf(lSelectedItemToken.ParentCurrency.Id);
                SetTokenInfo(lSelectedItemToken);
                btnOK.Enabled = true;
            }

            TxtBoxTokenAddress.TextChanged += TxtBoxTokenAddress_TextChanged;
        }
    }
}