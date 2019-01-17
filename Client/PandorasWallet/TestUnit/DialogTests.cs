using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Threading.Tasks;
using System.Windows.Forms;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Pandora.Client.PandorasWallet.Dialogs;

namespace Pandora.Client.PandorasWallet.TestUnit
{
    [TestClass]
    public class DialogTest
    {
        private WalletPasswordDialogDummy FEncryptDialog;

        [TestMethod]
        public void TestSettingsDialog()
        {
            var FSettingsdialog = new SettingsDialogDummy();
            FSettingsdialog.DefaultPath = Environment.CurrentDirectory;
            FSettingsdialog.DefaultServer = "192.168.10.1";
            FSettingsdialog.DefaultPort = 181;
            FSettingsdialog.DefaultDefaultCoin = "Bitcoin";
            FSettingsdialog.ConnectionSettingsVisible = true;
            if (FSettingsdialog.Execute())
            {
            }
        }

        [TestMethod]
        public void TestSendDialog()
        {
            var FSendDialog = new SendTransactionDialog();
            FSendDialog.Balance = (decimal)50;
            FSendDialog.TxFee = (decimal)0.00004;
            FSendDialog.TxFeeRate = (decimal)0.0000001;
            FSendDialog.Amount = 50;
            FSendDialog.FromAddress = "2NBSkFYnDrcteRqM81vyXqVki2c2Pb93oEk";
            FSendDialog.ToAddress = "2NBSkFYnDrcteRqM81vyXqVki2c2Pb93oEk";

            if (FSendDialog.Execute())
            {
            }
        }

        [TestMethod]
        public void TestSendingDialog()
        {
            var FSendingDialog = new SendingTxDialog();
            FSendingDialog.Status = "Sending...";

            Task.Run(() =>
            {
                string lTxID = "aowkdopawkdopawidpo556262626262TXID";

                Random n = new Random();

                int prob = n.Next(100);

                bool Response = (prob > 50);

                System.Threading.Thread.Sleep(5000);

                if (Response)
                {
                    FSendingDialog?.BeginInvoke((System.Windows.Forms.MethodInvoker)delegate () { FSendingDialog.Response("Failed To Send."); });
                }
                else
                {
                    FSendingDialog?.BeginInvoke((System.Windows.Forms.MethodInvoker)delegate () { FSendingDialog.Response("Transaction Sent", lTxID); });
                }
            });

            FSendingDialog.Execute();
        }

        private void OnOkButtonClickHandler(object sender, EventArgs e)
        {
            Task.Run(() =>
            {
                string lError = "Error decrypting.";

                Random n = new Random();

                int prob = n.Next(100);

                bool Response = (prob > 50);

                System.Threading.Thread.Sleep(5000);

                if (Response)
                {
                    FEncryptDialog?.BeginInvoke((System.Windows.Forms.MethodInvoker)delegate () { FEncryptDialog.SetResult(); });
                }
                else
                {
                    FEncryptDialog?.BeginInvoke((System.Windows.Forms.MethodInvoker)delegate () { FEncryptDialog.SetResult(lError); });
                }
            });
        }

        [TestMethod]
        public void TestDecryptingDialog()
        {
            FEncryptDialog = new WalletPasswordDialogDummy();

            Form lAuxForm = new Form();
            lAuxForm.Show();

            FEncryptDialog.OnOkButtonClick += OnOkButtonClickHandler;
            FEncryptDialog.Execute();
        }

        [TestMethod]
        public void TestAddCurrencyDialog()
        {
            var FAddCurrencyDialog = new AddCoinSelectorDummy();

            List<AddCoinSelectorDummy.lstCurrencyViewItem> asd = new List<AddCoinSelectorDummy.lstCurrencyViewItem>();
            asd.Add(new AddCoinSelectorDummy.lstCurrencyViewItem { CurrencyIcon = Properties.Resources.Bitcoin, CurrencyID = 1, CurrencyName = "Bitcoin", CurrencySimbol = "BTC" });
            FAddCurrencyDialog.AddItemsToShow(asd);

            if (FAddCurrencyDialog.Execute())
            {
                Assert.IsTrue(FAddCurrencyDialog.SelectedItems[0] == 1);
            }
        }

        //[TestMethod]
        //public void TestDefaultCoinSelectorDialog()
        //{
        //    var FDialog = new DefaultCoinSelectorDummy();

        //    Form lAuxForm = new Form();
        //    lAuxForm.Show();

        //    List<DefaultCoinSelectorDummy.lstCurrencyListItem> asd = new List<DefaultCoinSelectorDummy.lstCurrencyListItem>();
        //    asd.Add(new DefaultCoinSelectorDummy.lstCurrencyListItem { CurrencyIcon = Properties.Resources.Bitcoin, CurrencyID = 1, CurrencyName = "Bitcoin", CurrencySimbol = "BTC" });
        //    FDialog.DataSource = asd;
        //    FDialog.DisplayMember = "CurrencyName";

        //    if (FDialog.Execute())
        //    {
        //        Assert.IsTrue(FDialog.SelectedCurrencyID == 1);
        //    }
        //}
    }
}