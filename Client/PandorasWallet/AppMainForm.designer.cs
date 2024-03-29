using Pandora;

namespace Pandora.Client.PandorasWallet
{
    partial class AppMainForm
    {
        /// <summary>
        /// Required designer variable.
        /// </summary>
        private System.ComponentModel.IContainer components = null;

        /// <summary>
        /// Clean up any resources being used.
        /// </summary>
        /// <param name="disposing">true if managed resources should be disposed; otherwise, false.</param>
        protected override void Dispose(bool disposing)
        {
            if (disposing && (components != null))
            {
                components.Dispose();
            }
            base.Dispose(disposing);
        }

        #region Windows Form Designer generated code

        /// <summary>
        /// Required method for Designer support - do not modify
        /// the contents of this method with the code editor.
        /// </summary>
        private void InitializeComponent()
        {
            this.components = new System.ComponentModel.Container();
            System.Windows.Forms.Label labelTotal;
            System.Windows.Forms.Label label14;
            System.Windows.Forms.GroupBox grpBalance;
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(AppMainForm));
            System.Windows.Forms.ListViewItem listViewItem1 = new System.Windows.Forms.ListViewItem(new string[] {
            "00/00.00 00:00:00 PM",
            "39sbRRh2qwRwjA5ABrFLFbYP5fToXkB5Ab"}, -1);
            System.Windows.Forms.DataVisualization.Charting.ChartArea chartArea1 = new System.Windows.Forms.DataVisualization.Charting.ChartArea();
            System.Windows.Forms.DataVisualization.Charting.Legend legend1 = new System.Windows.Forms.DataVisualization.Charting.Legend();
            this.lblCurrencyValue = new Pandora.Client.PandorasWallet.CoinAmountLabel();
            this.lblMarketTickerValue = new System.Windows.Forms.Label();
            this.lblUnconfirmed = new Pandora.Client.PandorasWallet.CoinAmountLabel();
            this.lblFiatValue = new Pandora.Client.PandorasWallet.CoinAmountLabel();
            this.lblFiatTickerValue = new System.Windows.Forms.Label();
            this.lblTotal = new Pandora.Client.PandorasWallet.CoinAmountLabel();
            this.grpPrices = new System.Windows.Forms.GroupBox();
            this.lblCurrencyPrice = new Pandora.Client.PandorasWallet.CoinAmountLabel();
            this.lblMarketTickerPrice = new System.Windows.Forms.Label();
            this.lblFiatPrice = new Pandora.Client.PandorasWallet.CoinAmountLabel();
            this.lblFiatTickerPrice = new System.Windows.Forms.Label();
            this.imageListTx = new System.Windows.Forms.ImageList(this.components);
            this.TxtBoxSendToAddress = new System.Windows.Forms.TextBox();
            this.lblStatus = new System.Windows.Forms.Label();
            this.LabelStatus = new System.Windows.Forms.Label();
            this.lblCoinName = new System.Windows.Forms.Label();
            this.QuickAmountTextBox = new System.Windows.Forms.TextBox();
            this.QuickAmmountLabel = new System.Windows.Forms.Label();
            this.label4 = new System.Windows.Forms.Label();
            this.tabControl = new System.Windows.Forms.TabControl();
            this.tabCryptoCurrency = new System.Windows.Forms.TabPage();
            this.panel3 = new System.Windows.Forms.Panel();
            this.iconSearch = new System.Windows.Forms.PictureBox();
            this.txtBoxSearchCoin = new System.Windows.Forms.TextBox();
            this.lstViewCurrencies = new Pandora.Client.PandorasWallet.CurrencyView();
            this.contextMenuCurrency = new System.Windows.Forms.ContextMenuStrip(this.components);
            this.toolStripMenuRemove = new System.Windows.Forms.ToolStripMenuItem();
            this.btnAddToken = new System.Windows.Forms.Button();
            this.AddCurrencyBtn = new System.Windows.Forms.Button();
            this.tabTransactions = new System.Windows.Forms.TabPage();
            this.listTransactions = new System.Windows.Forms.ListView();
            this.colDate = ((System.Windows.Forms.ColumnHeader)(new System.Windows.Forms.ColumnHeader()));
            this.colFrom = ((System.Windows.Forms.ColumnHeader)(new System.Windows.Forms.ColumnHeader()));
            this.colToAccount = ((System.Windows.Forms.ColumnHeader)(new System.Windows.Forms.ColumnHeader()));
            this.colDebit = ((System.Windows.Forms.ColumnHeader)(new System.Windows.Forms.ColumnHeader()));
            this.colCredit = ((System.Windows.Forms.ColumnHeader)(new System.Windows.Forms.ColumnHeader()));
            this.colConf = ((System.Windows.Forms.ColumnHeader)(new System.Windows.Forms.ColumnHeader()));
            this.txtNotesBox = new System.Windows.Forms.TextBox();
            this.label9 = new System.Windows.Forms.Label();
            this.tabExchange = new System.Windows.Forms.TabPage();
            this.lblLoading = new System.Windows.Forms.Label();
            this.lblChartTitle = new System.Windows.Forms.Label();
            this.cmboBoxChartInterval = new System.Windows.Forms.ComboBox();
            this.groupBox2 = new System.Windows.Forms.GroupBox();
            this.txtExchangeTargetPrice = new Pandora.Client.PandorasWallet.TickerTextBox();
            this.txtTotalReceived = new Pandora.Client.PandorasWallet.TickerTextBox();
            this.txtQuantity = new Pandora.Client.PandorasWallet.TickerTextBox();
            this.txtTransactionName = new System.Windows.Forms.TextBox();
            this.txtStopPrice = new Pandora.Client.PandorasWallet.TickerTextBox();
            this.label11 = new System.Windows.Forms.Label();
            this.label12 = new System.Windows.Forms.Label();
            this.label10 = new System.Windows.Forms.Label();
            this.label1 = new System.Windows.Forms.Label();
            this.btnExchange = new System.Windows.Forms.Button();
            this.label3 = new System.Windows.Forms.Label();
            this.marketPriceChart = new System.Windows.Forms.DataVisualization.Charting.Chart();
            this.lstExchangers = new System.Windows.Forms.ListBox();
            this.lblEstimatePrice = new System.Windows.Forms.Label();
            this.lblEstimatePriceCoin = new System.Windows.Forms.Label();
            this.btnExchangeKeys = new System.Windows.Forms.Button();
            this.statsctrlExchage = new Pandora.Client.PandorasWallet.StatusControl();
            this.lstExchangeMarket = new System.Windows.Forms.ListView();
            this.columnHeader1 = ((System.Windows.Forms.ColumnHeader)(new System.Windows.Forms.ColumnHeader()));
            this.columnHeader2 = ((System.Windows.Forms.ColumnHeader)(new System.Windows.Forms.ColumnHeader()));
            this.columnHeader3 = ((System.Windows.Forms.ColumnHeader)(new System.Windows.Forms.ColumnHeader()));
            this.label13 = new System.Windows.Forms.Label();
            this.lblExchange = new System.Windows.Forms.Label();
            this.label15 = new System.Windows.Forms.Label();
            this.tabTradeHistory = new System.Windows.Forms.TabPage();
            this.statscntrlTradeHistory = new Pandora.Client.PandorasWallet.StatusControl();
            this.chckOrderHistory = new System.Windows.Forms.CheckBox();
            this.label18 = new System.Windows.Forms.Label();
            this.lstOrderHistory = new System.Windows.Forms.ListView();
            this.columnHeader4 = ((System.Windows.Forms.ColumnHeader)(new System.Windows.Forms.ColumnHeader()));
            this.columnHeader9 = ((System.Windows.Forms.ColumnHeader)(new System.Windows.Forms.ColumnHeader()));
            this.columnHeader5 = ((System.Windows.Forms.ColumnHeader)(new System.Windows.Forms.ColumnHeader()));
            this.columnHeader6 = ((System.Windows.Forms.ColumnHeader)(new System.Windows.Forms.ColumnHeader()));
            this.columnHeader7 = ((System.Windows.Forms.ColumnHeader)(new System.Windows.Forms.ColumnHeader()));
            this.columnHeader11 = ((System.Windows.Forms.ColumnHeader)(new System.Windows.Forms.ColumnHeader()));
            this.columnHeader8 = ((System.Windows.Forms.ColumnHeader)(new System.Windows.Forms.ColumnHeader()));
            this.columnHeader10 = ((System.Windows.Forms.ColumnHeader)(new System.Windows.Forms.ColumnHeader()));
            this.label8 = new System.Windows.Forms.Label();
            this.txtBoxReceiveAddress = new System.Windows.Forms.TextBox();
            this.panel1 = new System.Windows.Forms.Panel();
            this.grpBoxWalletTotals = new System.Windows.Forms.GroupBox();
            this.lblCurrencyTotal = new Pandora.Client.PandorasWallet.CoinAmountLabel();
            this.lblTickerCurrencyTotal = new System.Windows.Forms.Label();
            this.lblFiatTotal = new Pandora.Client.PandorasWallet.CoinAmountLabel();
            this.lblTickerFiatTotal = new System.Windows.Forms.Label();
            this.picCoinImage = new System.Windows.Forms.PictureBox();
            this.pictureBox1 = new System.Windows.Forms.PictureBox();
            this.pictureBoxBalanceInfo = new System.Windows.Forms.PictureBox();
            this.statusStripWallet = new System.Windows.Forms.StatusStrip();
            this.toolStripConnectionStatus = new System.Windows.Forms.ToolStripStatusLabel();
            this.toolStripStatusLabel1 = new System.Windows.Forms.ToolStripStatusLabel();
            this.toolStripStatusUsername = new System.Windows.Forms.ToolStripStatusLabel();
            this.toolStripStatusLabel2 = new System.Windows.Forms.ToolStripStatusLabel();
            this.toolStripStatusEmail = new System.Windows.Forms.ToolStripStatusLabel();
            this.toolTipBalance = new System.Windows.Forms.ToolTip(this.components);
            this.contextMenuOrderMenu = new System.Windows.Forms.ContextMenuStrip(this.components);
            this.menuItemCancel = new System.Windows.Forms.ToolStripMenuItem();
            this.menuItemSellHalf = new System.Windows.Forms.ToolStripMenuItem();
            this.groupBox1 = new System.Windows.Forms.GroupBox();
            this.QuickSendButton = new Pandora.Client.PandorasWallet.MenuButton();
            this.coinTooltip = new System.Windows.Forms.ToolTip(this.components);
            this.toolTipHelp = new System.Windows.Forms.ToolTip(this.components);
            this.menuStripMain = new System.Windows.Forms.MenuStrip();
            this.fileToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.connectToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.backupWalletKeyToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.toolStripSeparator1 = new System.Windows.Forms.ToolStripSeparator();
            this.exitToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.optionsToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.ExportTxtoolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.exportTradeOrdersToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.toolStripSeparator2 = new System.Windows.Forms.ToolStripSeparator();
            this.clearWalletCacheToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.signAMessageToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.toolStripSeparator4 = new System.Windows.Forms.ToolStripSeparator();
            this.settingsToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.helpToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.pandorasWalletGuideToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.toolStripSeparator3 = new System.Windows.Forms.ToolStripSeparator();
            this.aboutToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.ExportTxFileSaveDialog = new System.Windows.Forms.SaveFileDialog();
            labelTotal = new System.Windows.Forms.Label();
            label14 = new System.Windows.Forms.Label();
            grpBalance = new System.Windows.Forms.GroupBox();
            grpBalance.SuspendLayout();
            this.grpPrices.SuspendLayout();
            this.tabControl.SuspendLayout();
            this.tabCryptoCurrency.SuspendLayout();
            this.panel3.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.iconSearch)).BeginInit();
            this.contextMenuCurrency.SuspendLayout();
            this.tabTransactions.SuspendLayout();
            this.tabExchange.SuspendLayout();
            this.groupBox2.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.marketPriceChart)).BeginInit();
            this.tabTradeHistory.SuspendLayout();
            this.panel1.SuspendLayout();
            this.grpBoxWalletTotals.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.picCoinImage)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.pictureBox1)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.pictureBoxBalanceInfo)).BeginInit();
            this.statusStripWallet.SuspendLayout();
            this.contextMenuOrderMenu.SuspendLayout();
            this.groupBox1.SuspendLayout();
            this.menuStripMain.SuspendLayout();
            this.SuspendLayout();
            // 
            // labelTotal
            // 
            labelTotal.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            labelTotal.AutoSize = true;
            labelTotal.Font = new System.Drawing.Font("Calibri", 10F);
            labelTotal.Location = new System.Drawing.Point(39, 49);
            labelTotal.Margin = new System.Windows.Forms.Padding(0);
            labelTotal.Name = "labelTotal";
            labelTotal.Size = new System.Drawing.Size(75, 17);
            labelTotal.TabIndex = 28;
            labelTotal.Text = "Total Coins :";
            labelTotal.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
            // 
            // label14
            // 
            label14.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            label14.AutoSize = true;
            label14.Font = new System.Drawing.Font("Calibri", 10F);
            label14.ForeColor = System.Drawing.Color.Gray;
            label14.Location = new System.Drawing.Point(26, 25);
            label14.Name = "label14";
            label14.Size = new System.Drawing.Size(88, 17);
            label14.TabIndex = 19;
            label14.Text = "Unconfirmed :";
            // 
            // grpBalance
            // 
            grpBalance.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            grpBalance.AutoSize = true;
            grpBalance.AutoSizeMode = System.Windows.Forms.AutoSizeMode.GrowAndShrink;
            grpBalance.Controls.Add(this.lblCurrencyValue);
            grpBalance.Controls.Add(this.lblMarketTickerValue);
            grpBalance.Controls.Add(this.lblUnconfirmed);
            grpBalance.Controls.Add(this.lblFiatValue);
            grpBalance.Controls.Add(label14);
            grpBalance.Controls.Add(this.lblFiatTickerValue);
            grpBalance.Controls.Add(this.lblTotal);
            grpBalance.Controls.Add(labelTotal);
            grpBalance.Font = new System.Drawing.Font("Calibri", 10F);
            grpBalance.Location = new System.Drawing.Point(626, 9);
            grpBalance.Name = "grpBalance";
            grpBalance.Size = new System.Drawing.Size(238, 141);
            grpBalance.TabIndex = 36;
            grpBalance.TabStop = false;
            grpBalance.Text = "Balance Info";
            // 
            // lblCurrencyValue
            // 
            this.lblCurrencyValue.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.lblCurrencyValue.Font = new System.Drawing.Font("Calibri", 10F);
            this.lblCurrencyValue.Location = new System.Drawing.Point(118, 81);
            this.lblCurrencyValue.Name = "lblCurrencyValue";
            this.lblCurrencyValue.Size = new System.Drawing.Size(114, 18);
            this.lblCurrencyValue.TabIndex = 29;
            this.lblCurrencyValue.Text = "0.000";
            this.lblCurrencyValue.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
            // 
            // lblMarketTickerValue
            // 
            this.lblMarketTickerValue.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.lblMarketTickerValue.Font = new System.Drawing.Font("Calibri", 10F);
            this.lblMarketTickerValue.Location = new System.Drawing.Point(7, 82);
            this.lblMarketTickerValue.Name = "lblMarketTickerValue";
            this.lblMarketTickerValue.RightToLeft = System.Windows.Forms.RightToLeft.No;
            this.lblMarketTickerValue.Size = new System.Drawing.Size(107, 17);
            this.lblMarketTickerValue.TabIndex = 27;
            this.lblMarketTickerValue.Text = "BTC Value :";
            this.lblMarketTickerValue.TextAlign = System.Drawing.ContentAlignment.MiddleRight;
            // 
            // lblUnconfirmed
            // 
            this.lblUnconfirmed.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.lblUnconfirmed.Font = new System.Drawing.Font("Calibri", 10F);
            this.lblUnconfirmed.Location = new System.Drawing.Point(117, 26);
            this.lblUnconfirmed.Name = "lblUnconfirmed";
            this.lblUnconfirmed.Size = new System.Drawing.Size(110, 18);
            this.lblUnconfirmed.TabIndex = 29;
            this.lblUnconfirmed.Text = "0.000";
            this.lblUnconfirmed.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
            // 
            // lblFiatValue
            // 
            this.lblFiatValue.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.lblFiatValue.Font = new System.Drawing.Font("Calibri", 10F);
            this.lblFiatValue.Location = new System.Drawing.Point(118, 103);
            this.lblFiatValue.Name = "lblFiatValue";
            this.lblFiatValue.Size = new System.Drawing.Size(109, 18);
            this.lblFiatValue.TabIndex = 29;
            this.lblFiatValue.Text = "0.000";
            this.lblFiatValue.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
            // 
            // lblFiatTickerValue
            // 
            this.lblFiatTickerValue.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.lblFiatTickerValue.Font = new System.Drawing.Font("Calibri", 10F);
            this.lblFiatTickerValue.Location = new System.Drawing.Point(10, 104);
            this.lblFiatTickerValue.Name = "lblFiatTickerValue";
            this.lblFiatTickerValue.Size = new System.Drawing.Size(104, 17);
            this.lblFiatTickerValue.TabIndex = 27;
            this.lblFiatTickerValue.Text = "USD Value :";
            this.lblFiatTickerValue.TextAlign = System.Drawing.ContentAlignment.MiddleRight;
            // 
            // lblTotal
            // 
            this.lblTotal.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.lblTotal.Font = new System.Drawing.Font("Calibri", 10F);
            this.lblTotal.Location = new System.Drawing.Point(117, 48);
            this.lblTotal.Name = "lblTotal";
            this.lblTotal.Size = new System.Drawing.Size(110, 18);
            this.lblTotal.TabIndex = 29;
            this.lblTotal.Text = "0.000";
            this.lblTotal.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
            // 
            // grpPrices
            // 
            this.grpPrices.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.grpPrices.Controls.Add(this.lblCurrencyPrice);
            this.grpPrices.Controls.Add(this.lblMarketTickerPrice);
            this.grpPrices.Controls.Add(this.lblFiatPrice);
            this.grpPrices.Controls.Add(this.lblFiatTickerPrice);
            this.grpPrices.Font = new System.Drawing.Font("Calibri", 10F);
            this.grpPrices.Location = new System.Drawing.Point(392, 7);
            this.grpPrices.Name = "grpPrices";
            this.grpPrices.Size = new System.Drawing.Size(213, 70);
            this.grpPrices.TabIndex = 36;
            this.grpPrices.TabStop = false;
            this.grpPrices.Text = "Market Prices";
            // 
            // lblCurrencyPrice
            // 
            this.lblCurrencyPrice.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.lblCurrencyPrice.Font = new System.Drawing.Font("Calibri", 10F);
            this.lblCurrencyPrice.Location = new System.Drawing.Point(104, 21);
            this.lblCurrencyPrice.Name = "lblCurrencyPrice";
            this.lblCurrencyPrice.Size = new System.Drawing.Size(96, 18);
            this.lblCurrencyPrice.TabIndex = 29;
            this.lblCurrencyPrice.Text = "0.000";
            this.lblCurrencyPrice.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
            // 
            // lblMarketTickerPrice
            // 
            this.lblMarketTickerPrice.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.lblMarketTickerPrice.Font = new System.Drawing.Font("Calibri", 10F);
            this.lblMarketTickerPrice.Location = new System.Drawing.Point(8, 21);
            this.lblMarketTickerPrice.Name = "lblMarketTickerPrice";
            this.lblMarketTickerPrice.RightToLeft = System.Windows.Forms.RightToLeft.No;
            this.lblMarketTickerPrice.Size = new System.Drawing.Size(92, 16);
            this.lblMarketTickerPrice.TabIndex = 27;
            this.lblMarketTickerPrice.Text = "BTC Price :";
            this.lblMarketTickerPrice.TextAlign = System.Drawing.ContentAlignment.MiddleRight;
            // 
            // lblFiatPrice
            // 
            this.lblFiatPrice.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.lblFiatPrice.Font = new System.Drawing.Font("Calibri", 10F);
            this.lblFiatPrice.Location = new System.Drawing.Point(103, 43);
            this.lblFiatPrice.Name = "lblFiatPrice";
            this.lblFiatPrice.Size = new System.Drawing.Size(96, 18);
            this.lblFiatPrice.TabIndex = 29;
            this.lblFiatPrice.Text = "0.000";
            this.lblFiatPrice.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
            // 
            // lblFiatTickerPrice
            // 
            this.lblFiatTickerPrice.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.lblFiatTickerPrice.Font = new System.Drawing.Font("Calibri", 10F);
            this.lblFiatTickerPrice.Location = new System.Drawing.Point(6, 41);
            this.lblFiatTickerPrice.Name = "lblFiatTickerPrice";
            this.lblFiatTickerPrice.Size = new System.Drawing.Size(94, 19);
            this.lblFiatTickerPrice.TabIndex = 27;
            this.lblFiatTickerPrice.Text = "USD Price :";
            this.lblFiatTickerPrice.TextAlign = System.Drawing.ContentAlignment.MiddleRight;
            // 
            // imageListTx
            // 
            this.imageListTx.ImageStream = ((System.Windows.Forms.ImageListStreamer)(resources.GetObject("imageListTx.ImageStream")));
            this.imageListTx.TransparentColor = System.Drawing.Color.Transparent;
            this.imageListTx.Images.SetKeyName(0, "Credit");
            this.imageListTx.Images.SetKeyName(1, "Debit");
            this.imageListTx.Images.SetKeyName(2, "Both");
            this.imageListTx.Images.SetKeyName(3, "unknown");
            // 
            // TxtBoxSendToAddress
            // 
            this.TxtBoxSendToAddress.Font = new System.Drawing.Font("Courier New", 8.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.TxtBoxSendToAddress.Location = new System.Drawing.Point(17, 43);
            this.TxtBoxSendToAddress.Name = "TxtBoxSendToAddress";
            this.TxtBoxSendToAddress.Size = new System.Drawing.Size(365, 20);
            this.TxtBoxSendToAddress.TabIndex = 17;
            this.TxtBoxSendToAddress.TextChanged += new System.EventHandler(this.TxtBoxSendToAddress_TextChanged);
            this.TxtBoxSendToAddress.KeyPress += new System.Windows.Forms.KeyPressEventHandler(this.TxtBoxSendToAddress_KeyPress);
            // 
            // lblStatus
            // 
            this.lblStatus.AutoSize = true;
            this.lblStatus.Font = new System.Drawing.Font("Calibri", 10F);
            this.lblStatus.ForeColor = System.Drawing.Color.Black;
            this.lblStatus.Location = new System.Drawing.Point(180, 86);
            this.lblStatus.Name = "lblStatus";
            this.lblStatus.Size = new System.Drawing.Size(83, 17);
            this.lblStatus.TabIndex = 21;
            this.lblStatus.Text = "Maintenance";
            // 
            // LabelStatus
            // 
            this.LabelStatus.Font = new System.Drawing.Font("Calibri", 10F, System.Drawing.FontStyle.Bold);
            this.LabelStatus.Location = new System.Drawing.Point(127, 86);
            this.LabelStatus.Name = "LabelStatus";
            this.LabelStatus.Size = new System.Drawing.Size(63, 18);
            this.LabelStatus.TabIndex = 20;
            this.LabelStatus.Text = "Status:";
            // 
            // lblCoinName
            // 
            this.lblCoinName.AutoEllipsis = true;
            this.lblCoinName.Font = new System.Drawing.Font("Calibri", 16F, System.Drawing.FontStyle.Bold);
            this.lblCoinName.Location = new System.Drawing.Point(125, 54);
            this.lblCoinName.Name = "lblCoinName";
            this.lblCoinName.Size = new System.Drawing.Size(267, 32);
            this.lblCoinName.TabIndex = 17;
            this.lblCoinName.Text = "Coin Name";
            // 
            // QuickAmountTextBox
            // 
            this.QuickAmountTextBox.Location = new System.Drawing.Point(17, 84);
            this.QuickAmountTextBox.Name = "QuickAmountTextBox";
            this.QuickAmountTextBox.Size = new System.Drawing.Size(250, 22);
            this.QuickAmountTextBox.TabIndex = 19;
            this.QuickAmountTextBox.Text = "0";
            this.QuickAmountTextBox.TextChanged += new System.EventHandler(this.QuickAmountTextBox_TextChanged);
            this.QuickAmountTextBox.KeyPress += new System.Windows.Forms.KeyPressEventHandler(this.QuickAmmountTextBox_KeyPress);
            // 
            // QuickAmmountLabel
            // 
            this.QuickAmmountLabel.AutoSize = true;
            this.QuickAmmountLabel.Font = new System.Drawing.Font("Segoe UI", 8.25F, System.Drawing.FontStyle.Bold);
            this.QuickAmmountLabel.Location = new System.Drawing.Point(14, 68);
            this.QuickAmmountLabel.Name = "QuickAmmountLabel";
            this.QuickAmmountLabel.Size = new System.Drawing.Size(53, 13);
            this.QuickAmmountLabel.TabIndex = 21;
            this.QuickAmmountLabel.Text = "Amount:";
            // 
            // label4
            // 
            this.label4.AutoSize = true;
            this.label4.Font = new System.Drawing.Font("Segoe UI", 8.25F, System.Drawing.FontStyle.Bold);
            this.label4.Location = new System.Drawing.Point(14, 27);
            this.label4.Name = "label4";
            this.label4.Size = new System.Drawing.Size(96, 13);
            this.label4.TabIndex = 22;
            this.label4.Text = "Send To Address:";
            // 
            // tabControl
            // 
            this.tabControl.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.tabControl.Controls.Add(this.tabCryptoCurrency);
            this.tabControl.Controls.Add(this.tabTransactions);
            this.tabControl.Controls.Add(this.tabExchange);
            this.tabControl.Controls.Add(this.tabTradeHistory);
            this.tabControl.Font = new System.Drawing.Font("Microsoft Sans Serif", 8.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.tabControl.Location = new System.Drawing.Point(12, 218);
            this.tabControl.Name = "tabControl";
            this.tabControl.SelectedIndex = 0;
            this.tabControl.Size = new System.Drawing.Size(1306, 568);
            this.tabControl.TabIndex = 24;
            // 
            // tabCryptoCurrency
            // 
            this.tabCryptoCurrency.Controls.Add(this.panel3);
            this.tabCryptoCurrency.Controls.Add(this.lstViewCurrencies);
            this.tabCryptoCurrency.Controls.Add(this.btnAddToken);
            this.tabCryptoCurrency.Controls.Add(this.AddCurrencyBtn);
            this.tabCryptoCurrency.Location = new System.Drawing.Point(4, 22);
            this.tabCryptoCurrency.Name = "tabCryptoCurrency";
            this.tabCryptoCurrency.Padding = new System.Windows.Forms.Padding(3);
            this.tabCryptoCurrency.Size = new System.Drawing.Size(1298, 542);
            this.tabCryptoCurrency.TabIndex = 0;
            this.tabCryptoCurrency.Text = "Cryptocurrencies";
            this.tabCryptoCurrency.UseVisualStyleBackColor = true;
            // 
            // panel3
            // 
            this.panel3.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.panel3.Controls.Add(this.iconSearch);
            this.panel3.Controls.Add(this.txtBoxSearchCoin);
            this.panel3.Location = new System.Drawing.Point(1078, 0);
            this.panel3.Name = "panel3";
            this.panel3.Size = new System.Drawing.Size(217, 30);
            this.panel3.TabIndex = 34;
            // 
            // iconSearch
            // 
            this.iconSearch.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.iconSearch.BackgroundImage = global::Pandora.Client.PandorasWallet.Properties.Resources.magnifiying_glass;
            this.iconSearch.BackgroundImageLayout = System.Windows.Forms.ImageLayout.Stretch;
            this.iconSearch.Location = new System.Drawing.Point(193, 9);
            this.iconSearch.Name = "iconSearch";
            this.iconSearch.Size = new System.Drawing.Size(16, 16);
            this.iconSearch.TabIndex = 35;
            this.iconSearch.TabStop = false;
            // 
            // txtBoxSearchCoin
            // 
            this.txtBoxSearchCoin.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.txtBoxSearchCoin.Location = new System.Drawing.Point(7, 6);
            this.txtBoxSearchCoin.Name = "txtBoxSearchCoin";
            this.txtBoxSearchCoin.Size = new System.Drawing.Size(206, 20);
            this.txtBoxSearchCoin.TabIndex = 16;
            this.txtBoxSearchCoin.TextChanged += new System.EventHandler(this.txtBoxSearchCoin_TextChanged);
            // 
            // lstViewCurrencies
            // 
            this.lstViewCurrencies.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.lstViewCurrencies.ContextMenuStrip = this.contextMenuCurrency;
            this.lstViewCurrencies.CurrencyIds = new long[0];
            this.lstViewCurrencies.Location = new System.Drawing.Point(3, 35);
            this.lstViewCurrencies.Name = "lstViewCurrencies";
            this.lstViewCurrencies.RightToLeft = System.Windows.Forms.RightToLeft.Yes;
            this.lstViewCurrencies.SelectedCurrencyId = ((long)(0));
            this.lstViewCurrencies.Size = new System.Drawing.Size(1292, 504);
            this.lstViewCurrencies.TabIndex = 20;
            this.lstViewCurrencies.OnSelectedIndexChanged += new System.EventHandler(this.CurrencyListView_OnSelectedIndexChanged);
            // 
            // contextMenuCurrency
            // 
            this.contextMenuCurrency.Items.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.toolStripMenuRemove});
            this.contextMenuCurrency.Name = "contextMenuCurrency";
            this.contextMenuCurrency.Size = new System.Drawing.Size(118, 26);
            // 
            // toolStripMenuRemove
            // 
            this.toolStripMenuRemove.Name = "toolStripMenuRemove";
            this.toolStripMenuRemove.Size = new System.Drawing.Size(117, 22);
            this.toolStripMenuRemove.Text = "Remove";
            this.toolStripMenuRemove.Click += new System.EventHandler(this.toolStripMenuRemove_Click);
            // 
            // btnAddToken
            // 
            this.btnAddToken.Font = new System.Drawing.Font("Microsoft Sans Serif", 8.25F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.btnAddToken.Location = new System.Drawing.Point(132, 7);
            this.btnAddToken.Name = "btnAddToken";
            this.btnAddToken.Size = new System.Drawing.Size(120, 23);
            this.btnAddToken.TabIndex = 19;
            this.btnAddToken.Text = "Add Token...";
            this.btnAddToken.UseVisualStyleBackColor = true;
            this.btnAddToken.Click += new System.EventHandler(this.AddTokenBtn_Click);
            // 
            // AddCurrencyBtn
            // 
            this.AddCurrencyBtn.Font = new System.Drawing.Font("Microsoft Sans Serif", 8.25F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.AddCurrencyBtn.Location = new System.Drawing.Point(6, 7);
            this.AddCurrencyBtn.Name = "AddCurrencyBtn";
            this.AddCurrencyBtn.Size = new System.Drawing.Size(120, 23);
            this.AddCurrencyBtn.TabIndex = 19;
            this.AddCurrencyBtn.Text = "Add Currency...";
            this.AddCurrencyBtn.UseVisualStyleBackColor = true;
            this.AddCurrencyBtn.Click += new System.EventHandler(this.AddCurrencyBtn_Click);
            // 
            // tabTransactions
            // 
            this.tabTransactions.Controls.Add(this.listTransactions);
            this.tabTransactions.Controls.Add(this.txtNotesBox);
            this.tabTransactions.Controls.Add(this.label9);
            this.tabTransactions.Location = new System.Drawing.Point(4, 22);
            this.tabTransactions.Name = "tabTransactions";
            this.tabTransactions.Padding = new System.Windows.Forms.Padding(3);
            this.tabTransactions.Size = new System.Drawing.Size(1298, 542);
            this.tabTransactions.TabIndex = 1;
            this.tabTransactions.Text = "Transactions";
            this.tabTransactions.UseVisualStyleBackColor = true;
            // 
            // listTransactions
            // 
            this.listTransactions.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.listTransactions.Columns.AddRange(new System.Windows.Forms.ColumnHeader[] {
            this.colDate,
            this.colFrom,
            this.colToAccount,
            this.colDebit,
            this.colCredit,
            this.colConf});
            this.listTransactions.Font = new System.Drawing.Font("Segoe UI", 9F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.listTransactions.FullRowSelect = true;
            this.listTransactions.HideSelection = false;
            this.listTransactions.Items.AddRange(new System.Windows.Forms.ListViewItem[] {
            listViewItem1});
            this.listTransactions.Location = new System.Drawing.Point(9, 6);
            this.listTransactions.MultiSelect = false;
            this.listTransactions.Name = "listTransactions";
            this.listTransactions.Size = new System.Drawing.Size(1283, 455);
            this.listTransactions.SmallImageList = this.imageListTx;
            this.listTransactions.Sorting = System.Windows.Forms.SortOrder.Descending;
            this.listTransactions.TabIndex = 18;
            this.listTransactions.UseCompatibleStateImageBehavior = false;
            this.listTransactions.View = System.Windows.Forms.View.Details;
            this.listTransactions.ItemSelectionChanged += new System.Windows.Forms.ListViewItemSelectionChangedEventHandler(this.listTransactions_ItemSelectionChanged);
            // 
            // colDate
            // 
            this.colDate.Text = "Date";
            this.colDate.Width = 164;
            // 
            // colFrom
            // 
            this.colFrom.Text = "From";
            this.colFrom.Width = 250;
            // 
            // colToAccount
            // 
            this.colToAccount.Text = "To Account";
            this.colToAccount.Width = 250;
            // 
            // colDebit
            // 
            this.colDebit.Text = "Debit";
            this.colDebit.Width = 120;
            // 
            // colCredit
            // 
            this.colCredit.Text = "Credit";
            this.colCredit.Width = 120;
            // 
            // colConf
            // 
            this.colConf.Text = "Confirmed";
            this.colConf.Width = 75;
            // 
            // txtNotesBox
            // 
            this.txtNotesBox.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.txtNotesBox.Font = new System.Drawing.Font("Courier New", 8.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.txtNotesBox.Location = new System.Drawing.Point(9, 483);
            this.txtNotesBox.Multiline = true;
            this.txtNotesBox.Name = "txtNotesBox";
            this.txtNotesBox.ReadOnly = true;
            this.txtNotesBox.Size = new System.Drawing.Size(1283, 49);
            this.txtNotesBox.TabIndex = 17;
            // 
            // label9
            // 
            this.label9.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));
            this.label9.AutoSize = true;
            this.label9.Location = new System.Drawing.Point(6, 464);
            this.label9.Name = "label9";
            this.label9.Size = new System.Drawing.Size(38, 13);
            this.label9.TabIndex = 16;
            this.label9.Text = "Notes:";
            // 
            // tabExchange
            // 
            this.tabExchange.Controls.Add(this.lblLoading);
            this.tabExchange.Controls.Add(this.lblChartTitle);
            this.tabExchange.Controls.Add(this.cmboBoxChartInterval);
            this.tabExchange.Controls.Add(this.groupBox2);
            this.tabExchange.Controls.Add(this.marketPriceChart);
            this.tabExchange.Controls.Add(this.lstExchangers);
            this.tabExchange.Controls.Add(this.lblEstimatePrice);
            this.tabExchange.Controls.Add(this.lblEstimatePriceCoin);
            this.tabExchange.Controls.Add(this.btnExchangeKeys);
            this.tabExchange.Controls.Add(this.statsctrlExchage);
            this.tabExchange.Controls.Add(this.lstExchangeMarket);
            this.tabExchange.Controls.Add(this.label13);
            this.tabExchange.Controls.Add(this.lblExchange);
            this.tabExchange.Controls.Add(this.label15);
            this.tabExchange.Location = new System.Drawing.Point(4, 22);
            this.tabExchange.Name = "tabExchange";
            this.tabExchange.Padding = new System.Windows.Forms.Padding(3);
            this.tabExchange.Size = new System.Drawing.Size(1298, 542);
            this.tabExchange.TabIndex = 2;
            this.tabExchange.Text = "Exchange";
            this.tabExchange.UseVisualStyleBackColor = true;
            // 
            // lblLoading
            // 
            this.lblLoading.AutoSize = true;
            this.lblLoading.BackColor = System.Drawing.Color.Gainsboro;
            this.lblLoading.Font = new System.Drawing.Font("Microsoft Sans Serif", 14.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.lblLoading.Location = new System.Drawing.Point(121, 176);
            this.lblLoading.Name = "lblLoading";
            this.lblLoading.Size = new System.Drawing.Size(93, 24);
            this.lblLoading.TabIndex = 89;
            this.lblLoading.Text = "Loading...";
            this.lblLoading.Visible = false;
            // 
            // lblChartTitle
            // 
            this.lblChartTitle.AutoSize = true;
            this.lblChartTitle.Location = new System.Drawing.Point(345, 18);
            this.lblChartTitle.Name = "lblChartTitle";
            this.lblChartTitle.Size = new System.Drawing.Size(62, 13);
            this.lblChartTitle.TabIndex = 88;
            this.lblChartTitle.Text = "Price Chart:";
            // 
            // cmboBoxChartInterval
            // 
            this.cmboBoxChartInterval.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.cmboBoxChartInterval.Enabled = false;
            this.cmboBoxChartInterval.FormattingEnabled = true;
            this.cmboBoxChartInterval.Items.AddRange(new object[] {
            "Daily",
            "Hourly",
            "5 Minutes"});
            this.cmboBoxChartInterval.Location = new System.Drawing.Point(1134, 15);
            this.cmboBoxChartInterval.MaxDropDownItems = 5;
            this.cmboBoxChartInterval.Name = "cmboBoxChartInterval";
            this.cmboBoxChartInterval.Size = new System.Drawing.Size(121, 21);
            this.cmboBoxChartInterval.TabIndex = 87;
            this.cmboBoxChartInterval.SelectedIndexChanged += new System.EventHandler(this.cmboBoxChartInterval_SelectedIndexChanged);
            // 
            // groupBox2
            // 
            this.groupBox2.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));
            this.groupBox2.Controls.Add(this.txtExchangeTargetPrice);
            this.groupBox2.Controls.Add(this.txtTotalReceived);
            this.groupBox2.Controls.Add(this.txtQuantity);
            this.groupBox2.Controls.Add(this.txtTransactionName);
            this.groupBox2.Controls.Add(this.txtStopPrice);
            this.groupBox2.Controls.Add(this.label11);
            this.groupBox2.Controls.Add(this.label12);
            this.groupBox2.Controls.Add(this.label10);
            this.groupBox2.Controls.Add(this.label1);
            this.groupBox2.Controls.Add(this.btnExchange);
            this.groupBox2.Controls.Add(this.label3);
            this.groupBox2.Location = new System.Drawing.Point(348, 288);
            this.groupBox2.Name = "groupBox2";
            this.groupBox2.Size = new System.Drawing.Size(385, 236);
            this.groupBox2.TabIndex = 86;
            this.groupBox2.TabStop = false;
            this.groupBox2.Text = "3. Set your order";
            // 
            // txtExchangeTargetPrice
            // 
            this.txtExchangeTargetPrice.Amount = new decimal(new int[] {
            0,
            0,
            0,
            0});
            this.txtExchangeTargetPrice.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));
            this.txtExchangeTargetPrice.BackColor = System.Drawing.Color.Transparent;
            this.txtExchangeTargetPrice.CurrencyTicker = "BTC";
            this.txtExchangeTargetPrice.Location = new System.Drawing.Point(109, 33);
            this.txtExchangeTargetPrice.Margin = new System.Windows.Forms.Padding(0);
            this.txtExchangeTargetPrice.MaximumSize = new System.Drawing.Size(1000, 20);
            this.txtExchangeTargetPrice.Name = "txtExchangeTargetPrice";
            this.txtExchangeTargetPrice.Precision = ((uint)(8u));
            this.txtExchangeTargetPrice.Size = new System.Drawing.Size(221, 20);
            this.txtExchangeTargetPrice.TabIndex = 83;
            this.txtExchangeTargetPrice.UseOptionsMenu = false;
            this.txtExchangeTargetPrice.OnAmountChanged += new System.Action(this.txtTargetPrice_OnAmountChanged);
            // 
            // txtTotalReceived
            // 
            this.txtTotalReceived.Amount = new decimal(new int[] {
            0,
            0,
            0,
            0});
            this.txtTotalReceived.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));
            this.txtTotalReceived.BackColor = System.Drawing.Color.Transparent;
            this.txtTotalReceived.CurrencyTicker = "BTC";
            this.txtTotalReceived.Location = new System.Drawing.Point(109, 123);
            this.txtTotalReceived.Margin = new System.Windows.Forms.Padding(0);
            this.txtTotalReceived.MaximumSize = new System.Drawing.Size(1000, 20);
            this.txtTotalReceived.Name = "txtTotalReceived";
            this.txtTotalReceived.Precision = ((uint)(8u));
            this.txtTotalReceived.Size = new System.Drawing.Size(221, 20);
            this.txtTotalReceived.TabIndex = 80;
            this.txtTotalReceived.UseOptionsMenu = false;
            this.txtTotalReceived.OnAmountChanged += new System.Action(this.txtReceived_OnAmountChanged);
            // 
            // txtQuantity
            // 
            this.txtQuantity.Amount = new decimal(new int[] {
            0,
            0,
            0,
            0});
            this.txtQuantity.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));
            this.txtQuantity.BackColor = System.Drawing.Color.Transparent;
            this.txtQuantity.CurrencyTicker = "BTC";
            this.txtQuantity.Location = new System.Drawing.Point(109, 92);
            this.txtQuantity.Margin = new System.Windows.Forms.Padding(0);
            this.txtQuantity.MaximumSize = new System.Drawing.Size(1000, 20);
            this.txtQuantity.Name = "txtQuantity";
            this.txtQuantity.Precision = ((uint)(8u));
            this.txtQuantity.Size = new System.Drawing.Size(221, 20);
            this.txtQuantity.TabIndex = 81;
            this.txtQuantity.UseOptionsMenu = true;
            this.txtQuantity.OnAmountChanged += new System.Action(this.txtQuantity_OnAmountChanged);
            // 
            // txtTransactionName
            // 
            this.txtTransactionName.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));
            this.txtTransactionName.Location = new System.Drawing.Point(9, 184);
            this.txtTransactionName.Name = "txtTransactionName";
            this.txtTransactionName.Size = new System.Drawing.Size(320, 20);
            this.txtTransactionName.TabIndex = 77;
            this.txtTransactionName.TextChanged += new System.EventHandler(this.txtTransactionName_TextChanged);
            // 
            // txtStopPrice
            // 
            this.txtStopPrice.Amount = new decimal(new int[] {
            0,
            0,
            0,
            0});
            this.txtStopPrice.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));
            this.txtStopPrice.BackColor = System.Drawing.Color.Transparent;
            this.txtStopPrice.CurrencyTicker = "BTC";
            this.txtStopPrice.Location = new System.Drawing.Point(109, 63);
            this.txtStopPrice.Margin = new System.Windows.Forms.Padding(0);
            this.txtStopPrice.MaximumSize = new System.Drawing.Size(1000, 20);
            this.txtStopPrice.Name = "txtStopPrice";
            this.txtStopPrice.Precision = ((uint)(8u));
            this.txtStopPrice.Size = new System.Drawing.Size(221, 20);
            this.txtStopPrice.TabIndex = 82;
            this.txtStopPrice.UseOptionsMenu = false;
            this.txtStopPrice.OnAmountChanged += new System.Action(this.txtStopPrice_OnAmountChanged);
            // 
            // label11
            // 
            this.label11.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));
            this.label11.AutoSize = true;
            this.label11.Location = new System.Drawing.Point(6, 168);
            this.label11.Name = "label11";
            this.label11.Size = new System.Drawing.Size(97, 13);
            this.label11.TabIndex = 76;
            this.label11.Text = "Transaction Name:";
            // 
            // label12
            // 
            this.label12.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));
            this.label12.AutoSize = true;
            this.label12.Location = new System.Drawing.Point(14, 94);
            this.label12.Name = "label12";
            this.label12.Size = new System.Drawing.Size(90, 13);
            this.label12.TabIndex = 65;
            this.label12.Text = "Amount to spend:";
            // 
            // label10
            // 
            this.label10.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));
            this.label10.AutoSize = true;
            this.label10.Location = new System.Drawing.Point(9, 124);
            this.label10.Name = "label10";
            this.label10.Size = new System.Drawing.Size(96, 26);
            this.label10.TabIndex = 66;
            this.label10.Text = "Amount to receive:\r\n(approx)";
            // 
            // label1
            // 
            this.label1.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));
            this.label1.AutoSize = true;
            this.label1.Location = new System.Drawing.Point(44, 66);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(59, 13);
            this.label1.TabIndex = 73;
            this.label1.Text = "Stop Price:";
            // 
            // btnExchange
            // 
            this.btnExchange.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));
            this.btnExchange.Location = new System.Drawing.Point(9, 208);
            this.btnExchange.Name = "btnExchange";
            this.btnExchange.Size = new System.Drawing.Size(320, 23);
            this.btnExchange.TabIndex = 71;
            this.btnExchange.Text = "Exchange...";
            this.btnExchange.UseVisualStyleBackColor = true;
            this.btnExchange.Click += new System.EventHandler(this.btnExchange_Click);
            // 
            // label3
            // 
            this.label3.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));
            this.label3.AutoSize = true;
            this.label3.Location = new System.Drawing.Point(35, 36);
            this.label3.Name = "label3";
            this.label3.Size = new System.Drawing.Size(68, 13);
            this.label3.TabIndex = 73;
            this.label3.Text = "Target Price:";
            // 
            // marketPriceChart
            // 
            this.marketPriceChart.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.marketPriceChart.BackColor = System.Drawing.Color.Transparent;
            this.marketPriceChart.BorderlineColor = System.Drawing.Color.Black;
            this.marketPriceChart.BorderlineDashStyle = System.Windows.Forms.DataVisualization.Charting.ChartDashStyle.Solid;
            this.marketPriceChart.BorderSkin.BorderColor = System.Drawing.Color.DimGray;
            chartArea1.Name = "ChartArea1";
            this.marketPriceChart.ChartAreas.Add(chartArea1);
            this.marketPriceChart.Enabled = false;
            legend1.Enabled = false;
            legend1.Name = "Legend1";
            this.marketPriceChart.Legends.Add(legend1);
            this.marketPriceChart.Location = new System.Drawing.Point(348, 40);
            this.marketPriceChart.Margin = new System.Windows.Forms.Padding(1);
            this.marketPriceChart.Name = "marketPriceChart";
            this.marketPriceChart.Size = new System.Drawing.Size(907, 243);
            this.marketPriceChart.TabIndex = 85;
            this.marketPriceChart.TabStop = false;
            this.marketPriceChart.Text = "chart1";
            // 
            // lstExchangers
            // 
            this.lstExchangers.Font = new System.Drawing.Font("Microsoft Sans Serif", 10F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.lstExchangers.FormattingEnabled = true;
            this.lstExchangers.ItemHeight = 16;
            this.lstExchangers.Location = new System.Drawing.Point(12, 40);
            this.lstExchangers.Name = "lstExchangers";
            this.lstExchangers.Size = new System.Drawing.Size(317, 68);
            this.lstExchangers.TabIndex = 84;
            this.lstExchangers.SelectedIndexChanged += new System.EventHandler(this.lstExchangers_SelectedIndexChanged);
            // 
            // lblEstimatePrice
            // 
            this.lblEstimatePrice.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));
            this.lblEstimatePrice.Cursor = System.Windows.Forms.Cursors.Hand;
            this.lblEstimatePrice.Location = new System.Drawing.Point(185, 510);
            this.lblEstimatePrice.Name = "lblEstimatePrice";
            this.lblEstimatePrice.RightToLeft = System.Windows.Forms.RightToLeft.Yes;
            this.lblEstimatePrice.Size = new System.Drawing.Size(147, 13);
            this.lblEstimatePrice.TabIndex = 75;
            this.lblEstimatePrice.Text = "0";
            this.lblEstimatePrice.Click += new System.EventHandler(this.lblEstimatePrice_Click);
            // 
            // lblEstimatePriceCoin
            // 
            this.lblEstimatePriceCoin.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));
            this.lblEstimatePriceCoin.AutoSize = true;
            this.lblEstimatePriceCoin.Location = new System.Drawing.Point(9, 510);
            this.lblEstimatePriceCoin.Name = "lblEstimatePriceCoin";
            this.lblEstimatePriceCoin.Size = new System.Drawing.Size(133, 13);
            this.lblEstimatePriceCoin.TabIndex = 68;
            this.lblEstimatePriceCoin.Text = "Estimated Current price in :";
            // 
            // btnExchangeKeys
            // 
            this.btnExchangeKeys.BackgroundImage = global::Pandora.Client.PandorasWallet.Properties.Resources.icons8_key_30;
            this.btnExchangeKeys.BackgroundImageLayout = System.Windows.Forms.ImageLayout.Stretch;
            this.btnExchangeKeys.Location = new System.Drawing.Point(305, 13);
            this.btnExchangeKeys.Name = "btnExchangeKeys";
            this.btnExchangeKeys.Size = new System.Drawing.Size(24, 24);
            this.btnExchangeKeys.TabIndex = 79;
            this.toolTipHelp.SetToolTip(this.btnExchangeKeys, "Change exchange keys");
            this.btnExchangeKeys.UseVisualStyleBackColor = true;
            this.btnExchangeKeys.Click += new System.EventHandler(this.btnExchangeKeys_Click);
            // 
            // statsctrlExchage
            // 
            this.statsctrlExchage.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.statsctrlExchage.Location = new System.Drawing.Point(750, 295);
            this.statsctrlExchage.Name = "statsctrlExchage";
            this.statsctrlExchage.Size = new System.Drawing.Size(505, 236);
            this.statsctrlExchage.StatusName = "No orders";
            this.statsctrlExchage.TabIndex = 78;
            // 
            // lstExchangeMarket
            // 
            this.lstExchangeMarket.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left)));
            this.lstExchangeMarket.Columns.AddRange(new System.Windows.Forms.ColumnHeader[] {
            this.columnHeader1,
            this.columnHeader2,
            this.columnHeader3});
            this.lstExchangeMarket.Enabled = false;
            this.lstExchangeMarket.FullRowSelect = true;
            this.lstExchangeMarket.HideSelection = false;
            this.lstExchangeMarket.Location = new System.Drawing.Point(12, 127);
            this.lstExchangeMarket.MultiSelect = false;
            this.lstExchangeMarket.Name = "lstExchangeMarket";
            this.lstExchangeMarket.Size = new System.Drawing.Size(317, 380);
            this.lstExchangeMarket.Sorting = System.Windows.Forms.SortOrder.Ascending;
            this.lstExchangeMarket.TabIndex = 64;
            this.lstExchangeMarket.UseCompatibleStateImageBehavior = false;
            this.lstExchangeMarket.View = System.Windows.Forms.View.Details;
            this.lstExchangeMarket.SelectedIndexChanged += new System.EventHandler(this.lstCoinAvailable_OnSelectedIndexChanged);
            this.lstExchangeMarket.MouseDoubleClick += new System.Windows.Forms.MouseEventHandler(this.lstExchangeMarket_MouseDoubleClick);
            // 
            // columnHeader1
            // 
            this.columnHeader1.Text = "Currency Name";
            this.columnHeader1.Width = 108;
            // 
            // columnHeader2
            // 
            this.columnHeader2.Text = "Ticker";
            this.columnHeader2.Width = 71;
            // 
            // columnHeader3
            // 
            this.columnHeader3.Text = "Price";
            this.columnHeader3.TextAlign = System.Windows.Forms.HorizontalAlignment.Right;
            this.columnHeader3.Width = 114;
            // 
            // label13
            // 
            this.label13.AutoSize = true;
            this.label13.Location = new System.Drawing.Point(11, 111);
            this.label13.Name = "label13";
            this.label13.Size = new System.Drawing.Size(136, 13);
            this.label13.TabIndex = 63;
            this.label13.Text = "2. Choose Currency to Buy:";
            // 
            // lblExchange
            // 
            this.lblExchange.AutoSize = true;
            this.lblExchange.Location = new System.Drawing.Point(9, 19);
            this.lblExchange.Name = "lblExchange";
            this.lblExchange.Size = new System.Drawing.Size(146, 13);
            this.lblExchange.TabIndex = 61;
            this.lblExchange.Text = "1. Select Exchange to Trade:";
            // 
            // label15
            // 
            this.label15.AutoSize = true;
            this.label15.Location = new System.Drawing.Point(17, 470);
            this.label15.Name = "label15";
            this.label15.Size = new System.Drawing.Size(0, 13);
            this.label15.TabIndex = 60;
            // 
            // tabTradeHistory
            // 
            this.tabTradeHistory.Controls.Add(this.statscntrlTradeHistory);
            this.tabTradeHistory.Controls.Add(this.chckOrderHistory);
            this.tabTradeHistory.Controls.Add(this.label18);
            this.tabTradeHistory.Controls.Add(this.lstOrderHistory);
            this.tabTradeHistory.Location = new System.Drawing.Point(4, 22);
            this.tabTradeHistory.Name = "tabTradeHistory";
            this.tabTradeHistory.Padding = new System.Windows.Forms.Padding(3);
            this.tabTradeHistory.Size = new System.Drawing.Size(1298, 542);
            this.tabTradeHistory.TabIndex = 3;
            this.tabTradeHistory.Text = "Trade History";
            this.tabTradeHistory.UseVisualStyleBackColor = true;
            // 
            // statscntrlTradeHistory
            // 
            this.statscntrlTradeHistory.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.statscntrlTradeHistory.Location = new System.Drawing.Point(3, 404);
            this.statscntrlTradeHistory.Name = "statscntrlTradeHistory";
            this.statscntrlTradeHistory.Size = new System.Drawing.Size(1289, 132);
            this.statscntrlTradeHistory.StatusName = "";
            this.statscntrlTradeHistory.TabIndex = 44;
            // 
            // chckOrderHistory
            // 
            this.chckOrderHistory.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.chckOrderHistory.AutoSize = true;
            this.chckOrderHistory.Enabled = false;
            this.chckOrderHistory.Location = new System.Drawing.Point(1166, 6);
            this.chckOrderHistory.Name = "chckOrderHistory";
            this.chckOrderHistory.Size = new System.Drawing.Size(126, 17);
            this.chckOrderHistory.TabIndex = 43;
            this.chckOrderHistory.Text = "Show all trade history";
            this.chckOrderHistory.UseVisualStyleBackColor = true;
            this.chckOrderHistory.CheckStateChanged += new System.EventHandler(this.chckOrderHistory_CheckedChanged);
            // 
            // label18
            // 
            this.label18.AutoSize = true;
            this.label18.Location = new System.Drawing.Point(3, 14);
            this.label18.Name = "label18";
            this.label18.Size = new System.Drawing.Size(68, 13);
            this.label18.TabIndex = 40;
            this.label18.Text = "Order History";
            // 
            // lstOrderHistory
            // 
            this.lstOrderHistory.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.lstOrderHistory.Columns.AddRange(new System.Windows.Forms.ColumnHeader[] {
            this.columnHeader4,
            this.columnHeader9,
            this.columnHeader5,
            this.columnHeader6,
            this.columnHeader7,
            this.columnHeader11,
            this.columnHeader8,
            this.columnHeader10});
            this.lstOrderHistory.FullRowSelect = true;
            this.lstOrderHistory.HideSelection = false;
            this.lstOrderHistory.Location = new System.Drawing.Point(6, 30);
            this.lstOrderHistory.MultiSelect = false;
            this.lstOrderHistory.Name = "lstOrderHistory";
            this.lstOrderHistory.Size = new System.Drawing.Size(1286, 372);
            this.lstOrderHistory.TabIndex = 39;
            this.lstOrderHistory.UseCompatibleStateImageBehavior = false;
            this.lstOrderHistory.View = System.Windows.Forms.View.Details;
            this.lstOrderHistory.SelectedIndexChanged += new System.EventHandler(this.lstOrderHistory_OnSelectedIndexChanged);
            this.lstOrderHistory.MouseClick += new System.Windows.Forms.MouseEventHandler(this.LstOrderHistory_MouseClick);
            // 
            // columnHeader4
            // 
            this.columnHeader4.DisplayIndex = 1;
            this.columnHeader4.Text = "Transaction Name";
            this.columnHeader4.Width = 196;
            // 
            // columnHeader9
            // 
            this.columnHeader9.DisplayIndex = 0;
            this.columnHeader9.Text = "Time";
            this.columnHeader9.Width = 116;
            // 
            // columnHeader5
            // 
            this.columnHeader5.Text = "Sold";
            this.columnHeader5.Width = 68;
            // 
            // columnHeader6
            // 
            this.columnHeader6.Text = "Estimated to Receive";
            this.columnHeader6.Width = 114;
            // 
            // columnHeader7
            // 
            this.columnHeader7.Text = "Limit Price";
            this.columnHeader7.Width = 125;
            // 
            // columnHeader11
            // 
            this.columnHeader11.Text = "Stop Price";
            this.columnHeader11.Width = 125;
            // 
            // columnHeader8
            // 
            this.columnHeader8.Text = "Exchange";
            this.columnHeader8.Width = 112;
            // 
            // columnHeader10
            // 
            this.columnHeader10.Text = "Status";
            this.columnHeader10.Width = 115;
            // 
            // label8
            // 
            this.label8.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.label8.AutoSize = true;
            this.label8.Font = new System.Drawing.Font("Segoe UI", 8.25F, System.Drawing.FontStyle.Bold);
            this.label8.Location = new System.Drawing.Point(904, 163);
            this.label8.Name = "label8";
            this.label8.Size = new System.Drawing.Size(94, 13);
            this.label8.TabIndex = 29;
            this.label8.Text = "Receive Address:";
            // 
            // txtBoxReceiveAddress
            // 
            this.txtBoxReceiveAddress.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.txtBoxReceiveAddress.BackColor = System.Drawing.Color.MintCream;
            this.txtBoxReceiveAddress.Font = new System.Drawing.Font("Microsoft Sans Serif", 9F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.txtBoxReceiveAddress.Location = new System.Drawing.Point(907, 179);
            this.txtBoxReceiveAddress.Name = "txtBoxReceiveAddress";
            this.txtBoxReceiveAddress.ReadOnly = true;
            this.txtBoxReceiveAddress.Size = new System.Drawing.Size(395, 21);
            this.txtBoxReceiveAddress.TabIndex = 30;
            // 
            // panel1
            // 
            this.panel1.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.panel1.BackColor = System.Drawing.Color.WhiteSmoke;
            this.panel1.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
            this.panel1.Controls.Add(this.grpBoxWalletTotals);
            this.panel1.Controls.Add(this.grpPrices);
            this.panel1.Controls.Add(this.picCoinImage);
            this.panel1.Controls.Add(grpBalance);
            this.panel1.Controls.Add(this.pictureBox1);
            this.panel1.Controls.Add(this.pictureBoxBalanceInfo);
            this.panel1.Controls.Add(this.lblStatus);
            this.panel1.Controls.Add(this.LabelStatus);
            this.panel1.Controls.Add(this.lblCoinName);
            this.panel1.Font = new System.Drawing.Font("Segoe UI", 8.25F);
            this.panel1.Location = new System.Drawing.Point(19, 39);
            this.panel1.Name = "panel1";
            this.panel1.Size = new System.Drawing.Size(879, 161);
            this.panel1.TabIndex = 21;
            // 
            // grpBoxWalletTotals
            // 
            this.grpBoxWalletTotals.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.grpBoxWalletTotals.Controls.Add(this.lblCurrencyTotal);
            this.grpBoxWalletTotals.Controls.Add(this.lblTickerCurrencyTotal);
            this.grpBoxWalletTotals.Controls.Add(this.lblFiatTotal);
            this.grpBoxWalletTotals.Controls.Add(this.lblTickerFiatTotal);
            this.grpBoxWalletTotals.Font = new System.Drawing.Font("Calibri", 10F);
            this.grpBoxWalletTotals.Location = new System.Drawing.Point(392, 80);
            this.grpBoxWalletTotals.Name = "grpBoxWalletTotals";
            this.grpBoxWalletTotals.Size = new System.Drawing.Size(213, 70);
            this.grpBoxWalletTotals.TabIndex = 36;
            this.grpBoxWalletTotals.TabStop = false;
            this.grpBoxWalletTotals.Text = "Wallet Summary";
            // 
            // lblCurrencyTotal
            // 
            this.lblCurrencyTotal.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.lblCurrencyTotal.Font = new System.Drawing.Font("Calibri", 10F);
            this.lblCurrencyTotal.Location = new System.Drawing.Point(104, 21);
            this.lblCurrencyTotal.Name = "lblCurrencyTotal";
            this.lblCurrencyTotal.Size = new System.Drawing.Size(96, 18);
            this.lblCurrencyTotal.TabIndex = 29;
            this.lblCurrencyTotal.Text = "0.000";
            this.lblCurrencyTotal.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
            // 
            // lblTickerCurrencyTotal
            // 
            this.lblTickerCurrencyTotal.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.lblTickerCurrencyTotal.Font = new System.Drawing.Font("Calibri", 10F);
            this.lblTickerCurrencyTotal.Location = new System.Drawing.Point(8, 21);
            this.lblTickerCurrencyTotal.Name = "lblTickerCurrencyTotal";
            this.lblTickerCurrencyTotal.RightToLeft = System.Windows.Forms.RightToLeft.No;
            this.lblTickerCurrencyTotal.Size = new System.Drawing.Size(92, 16);
            this.lblTickerCurrencyTotal.TabIndex = 27;
            this.lblTickerCurrencyTotal.Text = "BTC Total :";
            this.lblTickerCurrencyTotal.TextAlign = System.Drawing.ContentAlignment.MiddleRight;
            // 
            // lblFiatTotal
            // 
            this.lblFiatTotal.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.lblFiatTotal.Font = new System.Drawing.Font("Calibri", 10F);
            this.lblFiatTotal.Location = new System.Drawing.Point(103, 43);
            this.lblFiatTotal.Name = "lblFiatTotal";
            this.lblFiatTotal.Size = new System.Drawing.Size(96, 18);
            this.lblFiatTotal.TabIndex = 29;
            this.lblFiatTotal.Text = "0.000";
            this.lblFiatTotal.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
            // 
            // lblTickerFiatTotal
            // 
            this.lblTickerFiatTotal.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.lblTickerFiatTotal.Font = new System.Drawing.Font("Calibri", 10F);
            this.lblTickerFiatTotal.Location = new System.Drawing.Point(6, 41);
            this.lblTickerFiatTotal.Name = "lblTickerFiatTotal";
            this.lblTickerFiatTotal.Size = new System.Drawing.Size(94, 19);
            this.lblTickerFiatTotal.TabIndex = 27;
            this.lblTickerFiatTotal.Text = "USD Total :";
            this.lblTickerFiatTotal.TextAlign = System.Drawing.ContentAlignment.MiddleRight;
            // 
            // picCoinImage
            // 
            this.picCoinImage.Image = global::Pandora.Client.PandorasWallet.Properties.Resources.goldbars;
            this.picCoinImage.Location = new System.Drawing.Point(43, 76);
            this.picCoinImage.Name = "picCoinImage";
            this.picCoinImage.Size = new System.Drawing.Size(32, 32);
            this.picCoinImage.TabIndex = 18;
            this.picCoinImage.TabStop = false;
            // 
            // pictureBox1
            // 
            this.pictureBox1.BackgroundImage = ((System.Drawing.Image)(resources.GetObject("pictureBox1.BackgroundImage")));
            this.pictureBox1.BackgroundImageLayout = System.Windows.Forms.ImageLayout.Stretch;
            this.pictureBox1.Location = new System.Drawing.Point(22, 30);
            this.pictureBox1.Name = "pictureBox1";
            this.pictureBox1.Size = new System.Drawing.Size(100, 100);
            this.pictureBox1.TabIndex = 26;
            this.pictureBox1.TabStop = false;
            // 
            // pictureBoxBalanceInfo
            // 
            this.pictureBoxBalanceInfo.Image = ((System.Drawing.Image)(resources.GetObject("pictureBoxBalanceInfo.Image")));
            this.pictureBoxBalanceInfo.Location = new System.Drawing.Point(22, 9);
            this.pictureBoxBalanceInfo.Name = "pictureBoxBalanceInfo";
            this.pictureBoxBalanceInfo.Size = new System.Drawing.Size(15, 15);
            this.pictureBoxBalanceInfo.TabIndex = 24;
            this.pictureBoxBalanceInfo.TabStop = false;
            this.pictureBoxBalanceInfo.Visible = false;
            // 
            // statusStripWallet
            // 
            this.statusStripWallet.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.statusStripWallet.AutoSize = false;
            this.statusStripWallet.Dock = System.Windows.Forms.DockStyle.None;
            this.statusStripWallet.Items.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.toolStripConnectionStatus,
            this.toolStripStatusLabel1,
            this.toolStripStatusUsername,
            this.toolStripStatusLabel2,
            this.toolStripStatusEmail});
            this.statusStripWallet.Location = new System.Drawing.Point(0, 789);
            this.statusStripWallet.Name = "statusStripWallet";
            this.statusStripWallet.Size = new System.Drawing.Size(1330, 20);
            this.statusStripWallet.TabIndex = 32;
            this.statusStripWallet.Text = "statusStrip1";
            // 
            // toolStripConnectionStatus
            // 
            this.toolStripConnectionStatus.BackColor = System.Drawing.Color.Transparent;
            this.toolStripConnectionStatus.BorderSides = System.Windows.Forms.ToolStripStatusLabelBorderSides.Right;
            this.toolStripConnectionStatus.BorderStyle = System.Windows.Forms.Border3DStyle.Etched;
            this.toolStripConnectionStatus.Font = new System.Drawing.Font("Segoe UI", 9F);
            this.toolStripConnectionStatus.Name = "toolStripConnectionStatus";
            this.toolStripConnectionStatus.Padding = new System.Windows.Forms.Padding(0, 0, 3, 0);
            this.toolStripConnectionStatus.Size = new System.Drawing.Size(52, 15);
            this.toolStripConnectionStatus.Text = "STATUS";
            // 
            // toolStripStatusLabel1
            // 
            this.toolStripStatusLabel1.BackColor = System.Drawing.Color.Transparent;
            this.toolStripStatusLabel1.Font = new System.Drawing.Font("Segoe UI", 9F, System.Drawing.FontStyle.Bold);
            this.toolStripStatusLabel1.Name = "toolStripStatusLabel1";
            this.toolStripStatusLabel1.Padding = new System.Windows.Forms.Padding(3, 0, 0, 0);
            this.toolStripStatusLabel1.Size = new System.Drawing.Size(70, 15);
            this.toolStripStatusLabel1.Text = "Username:";
            // 
            // toolStripStatusUsername
            // 
            this.toolStripStatusUsername.BackColor = System.Drawing.Color.Transparent;
            this.toolStripStatusUsername.Name = "toolStripStatusUsername";
            this.toolStripStatusUsername.Size = new System.Drawing.Size(60, 15);
            this.toolStripStatusUsername.Text = "Username";
            // 
            // toolStripStatusLabel2
            // 
            this.toolStripStatusLabel2.BackColor = System.Drawing.Color.Transparent;
            this.toolStripStatusLabel2.Font = new System.Drawing.Font("Segoe UI", 9F, System.Drawing.FontStyle.Bold);
            this.toolStripStatusLabel2.Name = "toolStripStatusLabel2";
            this.toolStripStatusLabel2.Size = new System.Drawing.Size(39, 15);
            this.toolStripStatusLabel2.Text = "Email:";
            // 
            // toolStripStatusEmail
            // 
            this.toolStripStatusEmail.BackColor = System.Drawing.Color.Transparent;
            this.toolStripStatusEmail.Font = new System.Drawing.Font("Segoe UI", 9F);
            this.toolStripStatusEmail.Name = "toolStripStatusEmail";
            this.toolStripStatusEmail.Size = new System.Drawing.Size(36, 15);
            this.toolStripStatusEmail.Text = "Email";
            // 
            // contextMenuOrderMenu
            // 
            this.contextMenuOrderMenu.Items.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.menuItemCancel,
            this.menuItemSellHalf});
            this.contextMenuOrderMenu.Name = "ctxMenuOrderMenu";
            this.contextMenuOrderMenu.Size = new System.Drawing.Size(182, 48);
            // 
            // menuItemCancel
            // 
            this.menuItemCancel.Name = "menuItemCancel";
            this.menuItemCancel.Size = new System.Drawing.Size(181, 22);
            this.menuItemCancel.Text = "Cancel";
            this.menuItemCancel.Click += new System.EventHandler(this.MenuItemCancel_Click);
            // 
            // menuItemSellHalf
            // 
            this.menuItemSellHalf.Name = "menuItemSellHalf";
            this.menuItemSellHalf.Size = new System.Drawing.Size(181, 22);
            this.menuItemSellHalf.Text = "Sell half on a double";
            this.menuItemSellHalf.Click += new System.EventHandler(this.menuItemSellHalf_Click);
            // 
            // groupBox1
            // 
            this.groupBox1.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.groupBox1.Controls.Add(this.TxtBoxSendToAddress);
            this.groupBox1.Controls.Add(this.QuickAmountTextBox);
            this.groupBox1.Controls.Add(this.QuickSendButton);
            this.groupBox1.Controls.Add(this.QuickAmmountLabel);
            this.groupBox1.Controls.Add(this.label4);
            this.groupBox1.Font = new System.Drawing.Font("Segoe UI", 8.25F);
            this.groupBox1.Location = new System.Drawing.Point(907, 32);
            this.groupBox1.Name = "groupBox1";
            this.groupBox1.Size = new System.Drawing.Size(395, 124);
            this.groupBox1.TabIndex = 34;
            this.groupBox1.TabStop = false;
            this.groupBox1.Text = "Quick Send";
            // 
            // QuickSendButton
            // 
            this.QuickSendButton.Font = new System.Drawing.Font("Segoe UI", 8.25F, System.Drawing.FontStyle.Bold);
            this.QuickSendButton.Location = new System.Drawing.Point(273, 82);
            this.QuickSendButton.Name = "QuickSendButton";
            this.QuickSendButton.Size = new System.Drawing.Size(109, 23);
            this.QuickSendButton.TabIndex = 33;
            this.QuickSendButton.Text = "Send";
            this.QuickSendButton.UseVisualStyleBackColor = true;
            this.QuickSendButton.Click += new System.EventHandler(this.QuickSendButton_Click);
            // 
            // menuStripMain
            // 
            this.menuStripMain.BackColor = System.Drawing.Color.Transparent;
            this.menuStripMain.Items.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.fileToolStripMenuItem,
            this.optionsToolStripMenuItem,
            this.helpToolStripMenuItem});
            this.menuStripMain.Location = new System.Drawing.Point(0, 0);
            this.menuStripMain.Name = "menuStripMain";
            this.menuStripMain.Size = new System.Drawing.Size(1330, 24);
            this.menuStripMain.TabIndex = 35;
            this.menuStripMain.Text = "menuStrip2";
            // 
            // fileToolStripMenuItem
            // 
            this.fileToolStripMenuItem.DropDownItems.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.connectToolStripMenuItem,
            this.backupWalletKeyToolStripMenuItem,
            this.toolStripSeparator1,
            this.exitToolStripMenuItem});
            this.fileToolStripMenuItem.Name = "fileToolStripMenuItem";
            this.fileToolStripMenuItem.Size = new System.Drawing.Size(37, 20);
            this.fileToolStripMenuItem.Text = "File";
            // 
            // connectToolStripMenuItem
            // 
            this.connectToolStripMenuItem.Name = "connectToolStripMenuItem";
            this.connectToolStripMenuItem.ShortcutKeyDisplayString = "";
            this.connectToolStripMenuItem.Size = new System.Drawing.Size(180, 22);
            this.connectToolStripMenuItem.Text = "Connect...";
            this.connectToolStripMenuItem.Click += new System.EventHandler(this.connectToolStripMenuItem1_Click);
            // 
            // backupWalletKeyToolStripMenuItem
            // 
            this.backupWalletKeyToolStripMenuItem.Name = "backupWalletKeyToolStripMenuItem";
            this.backupWalletKeyToolStripMenuItem.Size = new System.Drawing.Size(180, 22);
            this.backupWalletKeyToolStripMenuItem.Text = "Backup Wallet Key...";
            this.backupWalletKeyToolStripMenuItem.Click += new System.EventHandler(this.backupWalletKeyToolStripMenuItem_Click);
            // 
            // toolStripSeparator1
            // 
            this.toolStripSeparator1.Name = "toolStripSeparator1";
            this.toolStripSeparator1.Size = new System.Drawing.Size(177, 6);
            // 
            // exitToolStripMenuItem
            // 
            this.exitToolStripMenuItem.Name = "exitToolStripMenuItem";
            this.exitToolStripMenuItem.ShortcutKeys = ((System.Windows.Forms.Keys)((System.Windows.Forms.Keys.Alt | System.Windows.Forms.Keys.F4)));
            this.exitToolStripMenuItem.Size = new System.Drawing.Size(180, 22);
            this.exitToolStripMenuItem.Text = "Exit";
            this.exitToolStripMenuItem.Click += new System.EventHandler(this.exitToolStripMenuItem_Click);
            // 
            // optionsToolStripMenuItem
            // 
            this.optionsToolStripMenuItem.Checked = true;
            this.optionsToolStripMenuItem.CheckState = System.Windows.Forms.CheckState.Checked;
            this.optionsToolStripMenuItem.DropDownItems.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.ExportTxtoolStripMenuItem,
            this.exportTradeOrdersToolStripMenuItem,
            this.toolStripSeparator2,
            this.clearWalletCacheToolStripMenuItem,
            this.signAMessageToolStripMenuItem,
            this.toolStripSeparator4,
            this.settingsToolStripMenuItem});
            this.optionsToolStripMenuItem.Name = "optionsToolStripMenuItem";
            this.optionsToolStripMenuItem.Size = new System.Drawing.Size(46, 20);
            this.optionsToolStripMenuItem.Text = "Tools";
            // 
            // ExportTxtoolStripMenuItem
            // 
            this.ExportTxtoolStripMenuItem.Name = "ExportTxtoolStripMenuItem";
            this.ExportTxtoolStripMenuItem.Size = new System.Drawing.Size(209, 22);
            this.ExportTxtoolStripMenuItem.Text = "Export Transactions (CSV)";
            this.ExportTxtoolStripMenuItem.Click += new System.EventHandler(this.ExportTxtoolStripMenuItem_Click);
            // 
            // exportTradeOrdersToolStripMenuItem
            // 
            this.exportTradeOrdersToolStripMenuItem.Name = "exportTradeOrdersToolStripMenuItem";
            this.exportTradeOrdersToolStripMenuItem.Size = new System.Drawing.Size(209, 22);
            this.exportTradeOrdersToolStripMenuItem.Text = "Export Trade Orders (CSV)";
            this.exportTradeOrdersToolStripMenuItem.Click += new System.EventHandler(this.exportTradeOrdersToolStripMenuItem_Click);
            // 
            // toolStripSeparator2
            // 
            this.toolStripSeparator2.Name = "toolStripSeparator2";
            this.toolStripSeparator2.Size = new System.Drawing.Size(206, 6);
            // 
            // clearWalletCacheToolStripMenuItem
            // 
            this.clearWalletCacheToolStripMenuItem.Name = "clearWalletCacheToolStripMenuItem";
            this.clearWalletCacheToolStripMenuItem.Size = new System.Drawing.Size(209, 22);
            this.clearWalletCacheToolStripMenuItem.Text = "Clear Wallet Cache";
            this.clearWalletCacheToolStripMenuItem.Click += new System.EventHandler(this.clearWalletCacheToolStripMenuItem_Click);
            // 
            // signAMessageToolStripMenuItem
            // 
            this.signAMessageToolStripMenuItem.Name = "signAMessageToolStripMenuItem";
            this.signAMessageToolStripMenuItem.Size = new System.Drawing.Size(209, 22);
            this.signAMessageToolStripMenuItem.Text = "Sign a Message...";
            this.signAMessageToolStripMenuItem.Click += new System.EventHandler(this.signMessageToolStripMenuItem_Click);
            // 
            // toolStripSeparator4
            // 
            this.toolStripSeparator4.Name = "toolStripSeparator4";
            this.toolStripSeparator4.Size = new System.Drawing.Size(206, 6);
            // 
            // settingsToolStripMenuItem
            // 
            this.settingsToolStripMenuItem.Name = "settingsToolStripMenuItem";
            this.settingsToolStripMenuItem.Size = new System.Drawing.Size(209, 22);
            this.settingsToolStripMenuItem.Text = "Settings....";
            this.settingsToolStripMenuItem.Click += new System.EventHandler(this.settingsToolStripMenuItem_Click);
            // 
            // helpToolStripMenuItem
            // 
            this.helpToolStripMenuItem.DropDownItems.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.pandorasWalletGuideToolStripMenuItem,
            this.toolStripSeparator3,
            this.aboutToolStripMenuItem});
            this.helpToolStripMenuItem.Name = "helpToolStripMenuItem";
            this.helpToolStripMenuItem.Size = new System.Drawing.Size(44, 20);
            this.helpToolStripMenuItem.Text = "Help";
            // 
            // pandorasWalletGuideToolStripMenuItem
            // 
            this.pandorasWalletGuideToolStripMenuItem.Name = "pandorasWalletGuideToolStripMenuItem";
            this.pandorasWalletGuideToolStripMenuItem.Size = new System.Drawing.Size(208, 22);
            this.pandorasWalletGuideToolStripMenuItem.Text = "Pandora\'s Wallet Guide....";
            this.pandorasWalletGuideToolStripMenuItem.Click += new System.EventHandler(this.pandorasWalletGuideToolStripMenuItem_Click);
            // 
            // toolStripSeparator3
            // 
            this.toolStripSeparator3.Name = "toolStripSeparator3";
            this.toolStripSeparator3.Size = new System.Drawing.Size(205, 6);
            // 
            // aboutToolStripMenuItem
            // 
            this.aboutToolStripMenuItem.Name = "aboutToolStripMenuItem";
            this.aboutToolStripMenuItem.Size = new System.Drawing.Size(208, 22);
            this.aboutToolStripMenuItem.Text = "About";
            this.aboutToolStripMenuItem.Click += new System.EventHandler(this.aboutToolStripMenuItem_Click);
            // 
            // ExportTxFileSaveDialog
            // 
            this.ExportTxFileSaveDialog.DefaultExt = "csv";
            this.ExportTxFileSaveDialog.Filter = "csv files (*.csv)|*.csv|All files (*.*)|*.*";
            // 
            // AppMainForm
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.BackColor = System.Drawing.SystemColors.InactiveCaption;
            this.ClientSize = new System.Drawing.Size(1330, 808);
            this.Controls.Add(this.groupBox1);
            this.Controls.Add(this.statusStripWallet);
            this.Controls.Add(this.menuStripMain);
            this.Controls.Add(this.panel1);
            this.Controls.Add(this.txtBoxReceiveAddress);
            this.Controls.Add(this.label8);
            this.Controls.Add(this.tabControl);
            this.Icon = ((System.Drawing.Icon)(resources.GetObject("$this.Icon")));
            this.MinimumSize = new System.Drawing.Size(1346, 847);
            this.Name = "AppMainForm";
            this.StartPosition = System.Windows.Forms.FormStartPosition.CenterScreen;
            this.Text = "Pandora\'s Wallet";
            this.FormClosing += new System.Windows.Forms.FormClosingEventHandler(this.MainWallet_FormClosing);
            this.Load += new System.EventHandler(this.PandoraClientMainWindow_Load);
            this.Shown += new System.EventHandler(this.MainWallet_Shown);
            grpBalance.ResumeLayout(false);
            grpBalance.PerformLayout();
            this.grpPrices.ResumeLayout(false);
            this.tabControl.ResumeLayout(false);
            this.tabCryptoCurrency.ResumeLayout(false);
            this.panel3.ResumeLayout(false);
            this.panel3.PerformLayout();
            ((System.ComponentModel.ISupportInitialize)(this.iconSearch)).EndInit();
            this.contextMenuCurrency.ResumeLayout(false);
            this.tabTransactions.ResumeLayout(false);
            this.tabTransactions.PerformLayout();
            this.tabExchange.ResumeLayout(false);
            this.tabExchange.PerformLayout();
            this.groupBox2.ResumeLayout(false);
            this.groupBox2.PerformLayout();
            ((System.ComponentModel.ISupportInitialize)(this.marketPriceChart)).EndInit();
            this.tabTradeHistory.ResumeLayout(false);
            this.tabTradeHistory.PerformLayout();
            this.panel1.ResumeLayout(false);
            this.panel1.PerformLayout();
            this.grpBoxWalletTotals.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize)(this.picCoinImage)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.pictureBox1)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.pictureBoxBalanceInfo)).EndInit();
            this.statusStripWallet.ResumeLayout(false);
            this.statusStripWallet.PerformLayout();
            this.contextMenuOrderMenu.ResumeLayout(false);
            this.groupBox1.ResumeLayout(false);
            this.groupBox1.PerformLayout();
            this.menuStripMain.ResumeLayout(false);
            this.menuStripMain.PerformLayout();
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion
        private System.Windows.Forms.ImageList imageListTx;
        private System.Windows.Forms.TextBox TxtBoxSendToAddress;
        private System.Windows.Forms.Label lblStatus;
        private System.Windows.Forms.Label LabelStatus;
        private System.Windows.Forms.Label lblCoinName;
        private System.Windows.Forms.PictureBox picCoinImage;
        private System.Windows.Forms.TextBox QuickAmountTextBox;
        private System.Windows.Forms.Label QuickAmmountLabel;
        private System.Windows.Forms.Label label4;
        private System.Windows.Forms.TabControl tabControl;
        private System.Windows.Forms.TabPage tabCryptoCurrency;
        private System.Windows.Forms.TextBox txtBoxSearchCoin;
        private System.Windows.Forms.TabPage tabTransactions;
        private System.Windows.Forms.TextBox txtNotesBox;
        private System.Windows.Forms.Label label9;
        private System.Windows.Forms.Button AddCurrencyBtn;
        private System.Windows.Forms.Label label8;
        private System.Windows.Forms.TextBox txtBoxReceiveAddress;
        private CurrencyView lstViewCurrencies;
        private System.Windows.Forms.Panel panel1;
        private System.Windows.Forms.StatusStrip statusStripWallet;
        private System.Windows.Forms.ToolStripStatusLabel toolStripStatusLabel1;
        private System.Windows.Forms.ToolStripStatusLabel toolStripStatusUsername;
        private System.Windows.Forms.ToolStripStatusLabel toolStripStatusLabel2;
        private System.Windows.Forms.ToolStripStatusLabel toolStripStatusEmail;
        private System.Windows.Forms.ToolStripStatusLabel toolStripConnectionStatus;
        private MenuButton QuickSendButton;
        private System.Windows.Forms.TabPage tabExchange;
        private System.Windows.Forms.TextBox txtTransactionName;
        private System.Windows.Forms.Label label11;
        private System.Windows.Forms.Label lblEstimatePrice;
        private System.Windows.Forms.Label label3;
        private System.Windows.Forms.Button btnExchange;
        private System.Windows.Forms.Label lblEstimatePriceCoin;
        private System.Windows.Forms.Label label10;
        private System.Windows.Forms.Label label12;
        private System.Windows.Forms.ListView lstExchangeMarket;
        private System.Windows.Forms.ColumnHeader columnHeader1;
        private System.Windows.Forms.ColumnHeader columnHeader2;
        private System.Windows.Forms.ColumnHeader columnHeader3;
        private System.Windows.Forms.Label label13;
        private System.Windows.Forms.Label lblExchange;
        private System.Windows.Forms.Label label15;
        private System.Windows.Forms.TabPage tabTradeHistory;
        private System.Windows.Forms.CheckBox chckOrderHistory;
        private System.Windows.Forms.Label label18;
        private System.Windows.Forms.ListView lstOrderHistory;
        private System.Windows.Forms.ColumnHeader columnHeader4;
        private System.Windows.Forms.ColumnHeader columnHeader5;
        private System.Windows.Forms.ColumnHeader columnHeader6;
        private System.Windows.Forms.ColumnHeader columnHeader7;
        private System.Windows.Forms.ColumnHeader columnHeader8;
        private System.Windows.Forms.ColumnHeader columnHeader9;
        private System.Windows.Forms.ColumnHeader columnHeader10;
        private StatusControl statsctrlExchage;
        private StatusControl statscntrlTradeHistory;
        private System.Windows.Forms.ToolTip toolTipBalance;
        private System.Windows.Forms.Label label1;
        private System.Windows.Forms.ColumnHeader columnHeader11;
        private System.Windows.Forms.ListView listTransactions;
        private System.Windows.Forms.ColumnHeader colDate;
        private System.Windows.Forms.ColumnHeader colFrom;
        private System.Windows.Forms.ColumnHeader colToAccount;
        private System.Windows.Forms.ColumnHeader colDebit;
        private System.Windows.Forms.ColumnHeader colCredit;
        private System.Windows.Forms.ColumnHeader colConf;
        private System.Windows.Forms.Panel panel3;
        private System.Windows.Forms.PictureBox iconSearch;
        private System.Windows.Forms.ContextMenuStrip contextMenuOrderMenu;
        private System.Windows.Forms.ToolStripMenuItem menuItemCancel;
        private System.Windows.Forms.GroupBox groupBox1;
        private System.Windows.Forms.PictureBox pictureBoxBalanceInfo;
        private System.Windows.Forms.PictureBox pictureBox1;
        private System.Windows.Forms.ToolTip coinTooltip;
        private System.Windows.Forms.Button btnExchangeKeys;
        private System.Windows.Forms.ToolTip toolTipHelp;
        private TickerTextBox txtTotalReceived;
        private TickerTextBox txtQuantity;
        private TickerTextBox txtStopPrice;
        private TickerTextBox txtExchangeTargetPrice;
        private System.Windows.Forms.ToolStripMenuItem menuItemSellHalf;
        private System.Windows.Forms.MenuStrip menuStripMain;
        private System.Windows.Forms.ToolStripMenuItem fileToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem connectToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem backupWalletKeyToolStripMenuItem;
        private System.Windows.Forms.ToolStripSeparator toolStripSeparator1;
        private System.Windows.Forms.ToolStripMenuItem exitToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem optionsToolStripMenuItem;
        private System.Windows.Forms.ToolStripSeparator toolStripSeparator2;
        private System.Windows.Forms.ToolStripMenuItem settingsToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem helpToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem pandorasWalletGuideToolStripMenuItem;
        private System.Windows.Forms.ToolStripSeparator toolStripSeparator3;
        private System.Windows.Forms.ToolStripMenuItem aboutToolStripMenuItem;
        private System.Windows.Forms.ContextMenuStrip contextMenuCurrency;
        private System.Windows.Forms.ToolStripMenuItem toolStripMenuRemove;
        private System.Windows.Forms.Button btnAddToken;
        private System.Windows.Forms.ToolStripMenuItem clearWalletCacheToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem signAMessageToolStripMenuItem;
        private CoinAmountLabel lblCurrencyPrice;
        private CoinAmountLabel lblFiatPrice;
        private CoinAmountLabel lblUnconfirmed;
        private CoinAmountLabel lblTotal;
        private CoinAmountLabel lblCurrencyValue;
        private CoinAmountLabel lblFiatValue;
        private System.Windows.Forms.Label lblMarketTickerPrice;
        private System.Windows.Forms.Label lblFiatTickerPrice;
        private System.Windows.Forms.Label lblMarketTickerValue;
        private System.Windows.Forms.Label lblFiatTickerValue;
        private System.Windows.Forms.GroupBox grpPrices;
        private System.Windows.Forms.GroupBox grpBoxWalletTotals;
        private CoinAmountLabel lblCurrencyTotal;
        private System.Windows.Forms.Label lblTickerCurrencyTotal;
        private CoinAmountLabel lblFiatTotal;
        private System.Windows.Forms.Label lblTickerFiatTotal;
        private System.Windows.Forms.ToolStripMenuItem ExportTxtoolStripMenuItem;
        private System.Windows.Forms.SaveFileDialog ExportTxFileSaveDialog;
        private System.Windows.Forms.DataVisualization.Charting.Chart marketPriceChart;
        private System.Windows.Forms.ListBox lstExchangers;
        private System.Windows.Forms.GroupBox groupBox2;
        private System.Windows.Forms.ComboBox cmboBoxChartInterval;
        private System.Windows.Forms.Label lblChartTitle;
        private System.Windows.Forms.Label lblLoading;
        private System.Windows.Forms.ToolStripMenuItem exportTradeOrdersToolStripMenuItem;
        private System.Windows.Forms.ToolStripSeparator toolStripSeparator4;
    }
}

