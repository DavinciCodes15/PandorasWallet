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
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(AppMainForm));
            System.Windows.Forms.ListViewItem listViewItem1 = new System.Windows.Forms.ListViewItem(new string[] {
            "00/00.00 00:00:00 PM",
            "39sbRRh2qwRwjA5ABrFLFbYP5fToXkB5Ab"}, -1);
            this.imageListTx = new System.Windows.Forms.ImageList(this.components);
            this.TxtBoxSendToAddress = new System.Windows.Forms.TextBox();
            this.lblTotal = new System.Windows.Forms.Label();
            this.lblStatus = new System.Windows.Forms.Label();
            this.LabelStatus = new System.Windows.Forms.Label();
            this.labelTotal = new System.Windows.Forms.Label();
            this.lblCoinName = new System.Windows.Forms.Label();
            this.QuickAmountTextBox = new System.Windows.Forms.TextBox();
            this.QuickAmmountLabel = new System.Windows.Forms.Label();
            this.label4 = new System.Windows.Forms.Label();
            this.tabControl = new System.Windows.Forms.TabControl();
            this.tabPage1 = new System.Windows.Forms.TabPage();
            this.panel3 = new System.Windows.Forms.Panel();
            this.iconSearch = new System.Windows.Forms.PictureBox();
            this.txtBoxSearchCoin = new System.Windows.Forms.TextBox();
            this.lstViewCurrencies = new Pandora.Client.PandorasWallet.CurrencyView();
            this.AddCurrencyBtn = new System.Windows.Forms.Button();
            this.tabPage2 = new System.Windows.Forms.TabPage();
            this.listTransactions = new System.Windows.Forms.ListView();
            this.colDate = ((System.Windows.Forms.ColumnHeader)(new System.Windows.Forms.ColumnHeader()));
            this.colFrom = ((System.Windows.Forms.ColumnHeader)(new System.Windows.Forms.ColumnHeader()));
            this.colToAccount = ((System.Windows.Forms.ColumnHeader)(new System.Windows.Forms.ColumnHeader()));
            this.colDebit = ((System.Windows.Forms.ColumnHeader)(new System.Windows.Forms.ColumnHeader()));
            this.colCredit = ((System.Windows.Forms.ColumnHeader)(new System.Windows.Forms.ColumnHeader()));
            this.colConf = ((System.Windows.Forms.ColumnHeader)(new System.Windows.Forms.ColumnHeader()));
            this.txtNotesBox = new System.Windows.Forms.TextBox();
            this.label9 = new System.Windows.Forms.Label();
            this.tabPage3 = new System.Windows.Forms.TabPage();
            this.btnExchangeKeys = new System.Windows.Forms.Button();
            this.statsctrlExchage = new Pandora.Client.PandorasWallet.StatusControl();
            this.txtTransactionName = new System.Windows.Forms.TextBox();
            this.label11 = new System.Windows.Forms.Label();
            this.lblEstimatePrice = new System.Windows.Forms.Label();
            this.txtStopPrice = new System.Windows.Forms.TextBox();
            this.txtPrice = new System.Windows.Forms.TextBox();
            this.label1 = new System.Windows.Forms.Label();
            this.label3 = new System.Windows.Forms.Label();
            this.lblQuantity = new System.Windows.Forms.Label();
            this.btnExchange = new System.Windows.Forms.Button();
            this.txtTotal = new System.Windows.Forms.TextBox();
            this.txtQuantity = new System.Windows.Forms.TextBox();
            this.lblEstimatePriceCoin = new System.Windows.Forms.Label();
            this.lblTotalReceived = new System.Windows.Forms.Label();
            this.label10 = new System.Windows.Forms.Label();
            this.label12 = new System.Windows.Forms.Label();
            this.lstExchangeMarket = new System.Windows.Forms.ListView();
            this.columnHeader1 = ((System.Windows.Forms.ColumnHeader)(new System.Windows.Forms.ColumnHeader()));
            this.columnHeader2 = ((System.Windows.Forms.ColumnHeader)(new System.Windows.Forms.ColumnHeader()));
            this.columnHeader3 = ((System.Windows.Forms.ColumnHeader)(new System.Windows.Forms.ColumnHeader()));
            this.label13 = new System.Windows.Forms.Label();
            this.cbExchange = new System.Windows.Forms.ComboBox();
            this.lblExchange = new System.Windows.Forms.Label();
            this.label15 = new System.Windows.Forms.Label();
            this.tabPage4 = new System.Windows.Forms.TabPage();
            this.statscntrlTradeHistory = new Pandora.Client.PandorasWallet.StatusControl();
            this.chckOrderHistory = new System.Windows.Forms.CheckBox();
            this.label18 = new System.Windows.Forms.Label();
            this.lstOrderHistory = new System.Windows.Forms.ListView();
            this.columnHeader4 = ((System.Windows.Forms.ColumnHeader)(new System.Windows.Forms.ColumnHeader()));
            this.columnHeader5 = ((System.Windows.Forms.ColumnHeader)(new System.Windows.Forms.ColumnHeader()));
            this.columnHeader6 = ((System.Windows.Forms.ColumnHeader)(new System.Windows.Forms.ColumnHeader()));
            this.columnHeader7 = ((System.Windows.Forms.ColumnHeader)(new System.Windows.Forms.ColumnHeader()));
            this.columnHeader11 = ((System.Windows.Forms.ColumnHeader)(new System.Windows.Forms.ColumnHeader()));
            this.columnHeader8 = ((System.Windows.Forms.ColumnHeader)(new System.Windows.Forms.ColumnHeader()));
            this.columnHeader9 = ((System.Windows.Forms.ColumnHeader)(new System.Windows.Forms.ColumnHeader()));
            this.columnHeader10 = ((System.Windows.Forms.ColumnHeader)(new System.Windows.Forms.ColumnHeader()));
            this.menuStrip1 = new System.Windows.Forms.MenuStrip();
            this.fileToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.connectToolStripMenuItem1 = new System.Windows.Forms.ToolStripMenuItem();
            this.backupWalletKeyToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.toolStripMenuItem1 = new System.Windows.Forms.ToolStripSeparator();
            this.exitToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.optionsToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.changePasswordToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.toolStripMenuItem2 = new System.Windows.Forms.ToolStripSeparator();
            this.settingsToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.helpToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.aboutToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.label8 = new System.Windows.Forms.Label();
            this.txtBoxReceiveAddress = new System.Windows.Forms.TextBox();
            this.panel1 = new System.Windows.Forms.Panel();
            this.picCoinImage = new System.Windows.Forms.PictureBox();
            this.pictureBox1 = new System.Windows.Forms.PictureBox();
            this.label2 = new System.Windows.Forms.Label();
            this.pictureBoxBalanceInfo = new System.Windows.Forms.PictureBox();
            this.panel2 = new System.Windows.Forms.Panel();
            this.lblUnconfirmed = new System.Windows.Forms.Label();
            this.lblNameUnconfirmed = new System.Windows.Forms.Label();
            this.statusStripWallet = new System.Windows.Forms.StatusStrip();
            this.toolStripConnectionStatus = new System.Windows.Forms.ToolStripStatusLabel();
            this.toolStripStatusLabel1 = new System.Windows.Forms.ToolStripStatusLabel();
            this.toolStripStatusUsername = new System.Windows.Forms.ToolStripStatusLabel();
            this.toolStripStatusLabel2 = new System.Windows.Forms.ToolStripStatusLabel();
            this.toolStripStatusEmail = new System.Windows.Forms.ToolStripStatusLabel();
            this.toolTipBalance = new System.Windows.Forms.ToolTip(this.components);
            this.ctxMenuOrderMenu = new System.Windows.Forms.ContextMenuStrip(this.components);
            this.menuItemCancel = new System.Windows.Forms.ToolStripMenuItem();
            this.groupBox1 = new System.Windows.Forms.GroupBox();
            this.QuickSendButton = new Pandora.Client.PandorasWallet.MenuButton();
            this.coinTooltip = new System.Windows.Forms.ToolTip(this.components);
            this.toolTipHelp = new System.Windows.Forms.ToolTip(this.components);
            this.pandorasWalletGuideToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.toolStripMenuItem3 = new System.Windows.Forms.ToolStripSeparator();
            this.tabControl.SuspendLayout();
            this.tabPage1.SuspendLayout();
            this.panel3.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.iconSearch)).BeginInit();
            this.tabPage2.SuspendLayout();
            this.tabPage3.SuspendLayout();
            this.tabPage4.SuspendLayout();
            this.menuStrip1.SuspendLayout();
            this.panel1.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.picCoinImage)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.pictureBox1)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.pictureBoxBalanceInfo)).BeginInit();
            this.panel2.SuspendLayout();
            this.statusStripWallet.SuspendLayout();
            this.ctxMenuOrderMenu.SuspendLayout();
            this.groupBox1.SuspendLayout();
            this.SuspendLayout();
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
            // lblTotal
            // 
            this.lblTotal.AutoSize = true;
            this.lblTotal.Font = new System.Drawing.Font("Calibri", 10F);
            this.lblTotal.ForeColor = System.Drawing.Color.Black;
            this.lblTotal.Location = new System.Drawing.Point(100, 4);
            this.lblTotal.MaximumSize = new System.Drawing.Size(160, 0);
            this.lblTotal.Name = "lblTotal";
            this.lblTotal.Size = new System.Drawing.Size(75, 17);
            this.lblTotal.TabIndex = 22;
            this.lblTotal.Text = "0.00000000";
            this.lblTotal.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
            // 
            // lblStatus
            // 
            this.lblStatus.AutoSize = true;
            this.lblStatus.Font = new System.Drawing.Font("Calibri", 10F);
            this.lblStatus.ForeColor = System.Drawing.Color.Black;
            this.lblStatus.Location = new System.Drawing.Point(183, 89);
            this.lblStatus.Name = "lblStatus";
            this.lblStatus.Size = new System.Drawing.Size(83, 17);
            this.lblStatus.TabIndex = 21;
            this.lblStatus.Text = "Maintenance";
            // 
            // LabelStatus
            // 
            this.LabelStatus.Font = new System.Drawing.Font("Calibri", 10F, System.Drawing.FontStyle.Bold);
            this.LabelStatus.Location = new System.Drawing.Point(130, 89);
            this.LabelStatus.Name = "LabelStatus";
            this.LabelStatus.Size = new System.Drawing.Size(63, 18);
            this.LabelStatus.TabIndex = 20;
            this.LabelStatus.Text = "Status:";
            // 
            // labelTotal
            // 
            this.labelTotal.AutoSize = true;
            this.labelTotal.Font = new System.Drawing.Font("Calibri", 10F);
            this.labelTotal.Location = new System.Drawing.Point(17, 4);
            this.labelTotal.Margin = new System.Windows.Forms.Padding(0);
            this.labelTotal.Name = "labelTotal";
            this.labelTotal.Size = new System.Drawing.Size(75, 17);
            this.labelTotal.TabIndex = 19;
            this.labelTotal.Text = "Total Coins :";
            this.labelTotal.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
            // 
            // lblCoinName
            // 
            this.lblCoinName.AutoSize = true;
            this.lblCoinName.Font = new System.Drawing.Font("Calibri", 16F, System.Drawing.FontStyle.Bold);
            this.lblCoinName.Location = new System.Drawing.Point(128, 54);
            this.lblCoinName.Name = "lblCoinName";
            this.lblCoinName.Size = new System.Drawing.Size(112, 27);
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
            this.tabControl.Controls.Add(this.tabPage1);
            this.tabControl.Controls.Add(this.tabPage2);
            this.tabControl.Controls.Add(this.tabPage3);
            this.tabControl.Controls.Add(this.tabPage4);
            this.tabControl.Font = new System.Drawing.Font("Microsoft Sans Serif", 8.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.tabControl.Location = new System.Drawing.Point(12, 218);
            this.tabControl.Name = "tabControl";
            this.tabControl.SelectedIndex = 0;
            this.tabControl.Size = new System.Drawing.Size(1129, 551);
            this.tabControl.TabIndex = 24;
            // 
            // tabPage1
            // 
            this.tabPage1.Controls.Add(this.panel3);
            this.tabPage1.Controls.Add(this.lstViewCurrencies);
            this.tabPage1.Controls.Add(this.AddCurrencyBtn);
            this.tabPage1.Location = new System.Drawing.Point(4, 22);
            this.tabPage1.Name = "tabPage1";
            this.tabPage1.Padding = new System.Windows.Forms.Padding(3);
            this.tabPage1.Size = new System.Drawing.Size(1121, 525);
            this.tabPage1.TabIndex = 0;
            this.tabPage1.Text = "Cryptocurrencies";
            this.tabPage1.UseVisualStyleBackColor = true;
            // 
            // panel3
            // 
            this.panel3.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.panel3.Controls.Add(this.iconSearch);
            this.panel3.Controls.Add(this.txtBoxSearchCoin);
            this.panel3.Location = new System.Drawing.Point(901, 0);
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
            this.lstViewCurrencies.CurrencyIds = new long[0];
            this.lstViewCurrencies.Location = new System.Drawing.Point(3, 35);
            this.lstViewCurrencies.Name = "lstViewCurrencies";
            this.lstViewCurrencies.SelectedCurrencyId = ((long)(0));
            this.lstViewCurrencies.Size = new System.Drawing.Size(1115, 487);
            this.lstViewCurrencies.TabIndex = 20;
            this.lstViewCurrencies.OnSelectedIndexChanged += new System.EventHandler(this.CurrencyListView_OnSelectedIndexChanged);
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
            // tabPage2
            // 
            this.tabPage2.Controls.Add(this.listTransactions);
            this.tabPage2.Controls.Add(this.txtNotesBox);
            this.tabPage2.Controls.Add(this.label9);
            this.tabPage2.Location = new System.Drawing.Point(4, 22);
            this.tabPage2.Name = "tabPage2";
            this.tabPage2.Padding = new System.Windows.Forms.Padding(3);
            this.tabPage2.Size = new System.Drawing.Size(1121, 525);
            this.tabPage2.TabIndex = 1;
            this.tabPage2.Text = "Transactions";
            this.tabPage2.UseVisualStyleBackColor = true;
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
            this.listTransactions.Size = new System.Drawing.Size(1106, 445);
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
            this.txtNotesBox.Location = new System.Drawing.Point(9, 473);
            this.txtNotesBox.Multiline = true;
            this.txtNotesBox.Name = "txtNotesBox";
            this.txtNotesBox.ReadOnly = true;
            this.txtNotesBox.Size = new System.Drawing.Size(1106, 49);
            this.txtNotesBox.TabIndex = 17;
            // 
            // label9
            // 
            this.label9.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));
            this.label9.AutoSize = true;
            this.label9.Location = new System.Drawing.Point(6, 454);
            this.label9.Name = "label9";
            this.label9.Size = new System.Drawing.Size(38, 13);
            this.label9.TabIndex = 16;
            this.label9.Text = "Notes:";
            // 
            // tabPage3
            // 
            this.tabPage3.Controls.Add(this.btnExchangeKeys);
            this.tabPage3.Controls.Add(this.statsctrlExchage);
            this.tabPage3.Controls.Add(this.txtTransactionName);
            this.tabPage3.Controls.Add(this.label11);
            this.tabPage3.Controls.Add(this.lblEstimatePrice);
            this.tabPage3.Controls.Add(this.txtStopPrice);
            this.tabPage3.Controls.Add(this.txtPrice);
            this.tabPage3.Controls.Add(this.label1);
            this.tabPage3.Controls.Add(this.label3);
            this.tabPage3.Controls.Add(this.lblQuantity);
            this.tabPage3.Controls.Add(this.btnExchange);
            this.tabPage3.Controls.Add(this.txtTotal);
            this.tabPage3.Controls.Add(this.txtQuantity);
            this.tabPage3.Controls.Add(this.lblEstimatePriceCoin);
            this.tabPage3.Controls.Add(this.lblTotalReceived);
            this.tabPage3.Controls.Add(this.label10);
            this.tabPage3.Controls.Add(this.label12);
            this.tabPage3.Controls.Add(this.lstExchangeMarket);
            this.tabPage3.Controls.Add(this.label13);
            this.tabPage3.Controls.Add(this.cbExchange);
            this.tabPage3.Controls.Add(this.lblExchange);
            this.tabPage3.Controls.Add(this.label15);
            this.tabPage3.Location = new System.Drawing.Point(4, 22);
            this.tabPage3.Name = "tabPage3";
            this.tabPage3.Padding = new System.Windows.Forms.Padding(3);
            this.tabPage3.Size = new System.Drawing.Size(1121, 525);
            this.tabPage3.TabIndex = 2;
            this.tabPage3.Text = "Exchange";
            this.tabPage3.UseVisualStyleBackColor = true;
            // 
            // btnExchangeKeys
            // 
            this.btnExchangeKeys.BackgroundImage = global::Pandora.Client.PandorasWallet.Properties.Resources.icons8_key_30;
            this.btnExchangeKeys.BackgroundImageLayout = System.Windows.Forms.ImageLayout.Stretch;
            this.btnExchangeKeys.Location = new System.Drawing.Point(309, 15);
            this.btnExchangeKeys.Name = "btnExchangeKeys";
            this.btnExchangeKeys.Size = new System.Drawing.Size(24, 24);
            this.btnExchangeKeys.TabIndex = 79;
            this.toolTipHelp.SetToolTip(this.btnExchangeKeys, "Change exchange keys");
            this.btnExchangeKeys.UseVisualStyleBackColor = true;
            this.btnExchangeKeys.Click += new System.EventHandler(this.btnExchangeKeys_Click);
            // 
            // statsctrlExchage
            // 
            this.statsctrlExchage.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.statsctrlExchage.Location = new System.Drawing.Point(350, 16);
            this.statsctrlExchage.Name = "statsctrlExchage";
            this.statsctrlExchage.Size = new System.Drawing.Size(768, 503);
            this.statsctrlExchage.StatusName = "";
            this.statsctrlExchage.TabIndex = 78;
            // 
            // txtTransactionName
            // 
            this.txtTransactionName.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));
            this.txtTransactionName.Location = new System.Drawing.Point(12, 466);
            this.txtTransactionName.Name = "txtTransactionName";
            this.txtTransactionName.Size = new System.Drawing.Size(320, 20);
            this.txtTransactionName.TabIndex = 77;
            this.txtTransactionName.TextChanged += new System.EventHandler(this.txtTransactionName_TextChanged);
            // 
            // label11
            // 
            this.label11.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));
            this.label11.AutoSize = true;
            this.label11.Location = new System.Drawing.Point(9, 450);
            this.label11.Name = "label11";
            this.label11.Size = new System.Drawing.Size(97, 13);
            this.label11.TabIndex = 76;
            this.label11.Text = "Transaction Name:";
            // 
            // lblEstimatePrice
            // 
            this.lblEstimatePrice.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));
            this.lblEstimatePrice.Location = new System.Drawing.Point(185, 285);
            this.lblEstimatePrice.Name = "lblEstimatePrice";
            this.lblEstimatePrice.RightToLeft = System.Windows.Forms.RightToLeft.Yes;
            this.lblEstimatePrice.Size = new System.Drawing.Size(147, 13);
            this.lblEstimatePrice.TabIndex = 75;
            this.lblEstimatePrice.Text = "0";
            this.lblEstimatePrice.Click += new System.EventHandler(this.lblEstimatePrice_Click);
            // 
            // txtStopPrice
            // 
            this.txtStopPrice.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));
            this.txtStopPrice.Location = new System.Drawing.Point(113, 344);
            this.txtStopPrice.Name = "txtStopPrice";
            this.txtStopPrice.Size = new System.Drawing.Size(219, 20);
            this.txtStopPrice.TabIndex = 74;
            this.txtStopPrice.TextAlign = System.Windows.Forms.HorizontalAlignment.Right;
            this.txtStopPrice.TextChanged += new System.EventHandler(this.txtStopPrice_TextChanged);
            // 
            // txtPrice
            // 
            this.txtPrice.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));
            this.txtPrice.Location = new System.Drawing.Point(113, 314);
            this.txtPrice.Name = "txtPrice";
            this.txtPrice.Size = new System.Drawing.Size(219, 20);
            this.txtPrice.TabIndex = 74;
            this.txtPrice.TextAlign = System.Windows.Forms.HorizontalAlignment.Right;
            this.txtPrice.TextChanged += new System.EventHandler(this.txtPrice_TextChanged);
            // 
            // label1
            // 
            this.label1.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));
            this.label1.AutoSize = true;
            this.label1.Location = new System.Drawing.Point(12, 345);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(89, 13);
            this.label1.TabIndex = 73;
            this.label1.Text = "Stop Price (BTC):";
            // 
            // label3
            // 
            this.label3.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));
            this.label3.AutoSize = true;
            this.label3.Location = new System.Drawing.Point(12, 317);
            this.label3.Name = "label3";
            this.label3.Size = new System.Drawing.Size(98, 13);
            this.label3.TabIndex = 73;
            this.label3.Text = "Target Price (BTC):";
            // 
            // lblQuantity
            // 
            this.lblQuantity.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));
            this.lblQuantity.AutoSize = true;
            this.lblQuantity.Location = new System.Drawing.Point(12, 368);
            this.lblQuantity.Name = "lblQuantity";
            this.lblQuantity.Size = new System.Drawing.Size(69, 13);
            this.lblQuantity.TabIndex = 72;
            this.lblQuantity.Text = "Bitcoin (BTC)";
            // 
            // btnExchange
            // 
            this.btnExchange.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));
            this.btnExchange.Location = new System.Drawing.Point(12, 492);
            this.btnExchange.Name = "btnExchange";
            this.btnExchange.Size = new System.Drawing.Size(320, 23);
            this.btnExchange.TabIndex = 71;
            this.btnExchange.Text = "Exchange...";
            this.btnExchange.UseVisualStyleBackColor = true;
            this.btnExchange.Click += new System.EventHandler(this.btnExchange_Click);
            // 
            // txtTotal
            // 
            this.txtTotal.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));
            this.txtTotal.BackColor = System.Drawing.SystemColors.Window;
            this.txtTotal.Location = new System.Drawing.Point(113, 413);
            this.txtTotal.Name = "txtTotal";
            this.txtTotal.Size = new System.Drawing.Size(219, 20);
            this.txtTotal.TabIndex = 70;
            this.txtTotal.TextAlign = System.Windows.Forms.HorizontalAlignment.Right;
            this.txtTotal.KeyPress += new System.Windows.Forms.KeyPressEventHandler(this.txtQuantity_KeyPress);
            this.txtTotal.Leave += new System.EventHandler(this.txtTotal_Leave);
            // 
            // txtQuantity
            // 
            this.txtQuantity.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));
            this.txtQuantity.Location = new System.Drawing.Point(113, 379);
            this.txtQuantity.Name = "txtQuantity";
            this.txtQuantity.Size = new System.Drawing.Size(219, 20);
            this.txtQuantity.TabIndex = 69;
            this.txtQuantity.TextAlign = System.Windows.Forms.HorizontalAlignment.Right;
            this.txtQuantity.KeyPress += new System.Windows.Forms.KeyPressEventHandler(this.txtQuantity_KeyPress);
            this.txtQuantity.Leave += new System.EventHandler(this.txtQuantity_Leave);
            // 
            // lblEstimatePriceCoin
            // 
            this.lblEstimatePriceCoin.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));
            this.lblEstimatePriceCoin.AutoSize = true;
            this.lblEstimatePriceCoin.Location = new System.Drawing.Point(9, 285);
            this.lblEstimatePriceCoin.Name = "lblEstimatePriceCoin";
            this.lblEstimatePriceCoin.Size = new System.Drawing.Size(133, 13);
            this.lblEstimatePriceCoin.TabIndex = 68;
            this.lblEstimatePriceCoin.Text = "Estimated Current price in :";
            // 
            // lblTotalReceived
            // 
            this.lblTotalReceived.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));
            this.lblTotalReceived.AutoSize = true;
            this.lblTotalReceived.Location = new System.Drawing.Point(12, 401);
            this.lblTotalReceived.Name = "lblTotalReceived";
            this.lblTotalReceived.Size = new System.Drawing.Size(79, 13);
            this.lblTotalReceived.TabIndex = 67;
            this.lblTotalReceived.Text = "Maxcoin (MAX)";
            // 
            // label10
            // 
            this.label10.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));
            this.label10.AutoSize = true;
            this.label10.Location = new System.Drawing.Point(12, 414);
            this.label10.Name = "label10";
            this.label10.Size = new System.Drawing.Size(83, 26);
            this.label10.TabIndex = 66;
            this.label10.Text = "Total Received:\r\n(approx)";
            // 
            // label12
            // 
            this.label12.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));
            this.label12.AutoSize = true;
            this.label12.Location = new System.Drawing.Point(12, 381);
            this.label12.Name = "label12";
            this.label12.Size = new System.Drawing.Size(90, 13);
            this.label12.TabIndex = 65;
            this.label12.Text = "Amount to spend:";
            // 
            // lstExchangeMarket
            // 
            this.lstExchangeMarket.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left)));
            this.lstExchangeMarket.Columns.AddRange(new System.Windows.Forms.ColumnHeader[] {
            this.columnHeader1,
            this.columnHeader2,
            this.columnHeader3});
            this.lstExchangeMarket.FullRowSelect = true;
            this.lstExchangeMarket.HideSelection = false;
            this.lstExchangeMarket.Location = new System.Drawing.Point(12, 56);
            this.lstExchangeMarket.MultiSelect = false;
            this.lstExchangeMarket.Name = "lstExchangeMarket";
            this.lstExchangeMarket.Size = new System.Drawing.Size(320, 226);
            this.lstExchangeMarket.Sorting = System.Windows.Forms.SortOrder.Ascending;
            this.lstExchangeMarket.TabIndex = 64;
            this.lstExchangeMarket.UseCompatibleStateImageBehavior = false;
            this.lstExchangeMarket.View = System.Windows.Forms.View.Details;
            this.lstExchangeMarket.SelectedIndexChanged += new System.EventHandler(this.lstCoinAvailable_OnSelectedIndexChanged);
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
            this.label13.Location = new System.Drawing.Point(9, 40);
            this.label13.Name = "label13";
            this.label13.Size = new System.Drawing.Size(115, 13);
            this.label13.TabIndex = 63;
            this.label13.Text = "Select Currency to Buy";
            // 
            // cbExchange
            // 
            this.cbExchange.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.cbExchange.FormattingEnabled = true;
            this.cbExchange.Location = new System.Drawing.Point(122, 16);
            this.cbExchange.Name = "cbExchange";
            this.cbExchange.Size = new System.Drawing.Size(181, 21);
            this.cbExchange.TabIndex = 62;
            this.cbExchange.SelectedIndexChanged += new System.EventHandler(this.cbExchange_SelectedIndexChanged);
            // 
            // lblExchange
            // 
            this.lblExchange.AutoSize = true;
            this.lblExchange.Location = new System.Drawing.Point(9, 19);
            this.lblExchange.Name = "lblExchange";
            this.lblExchange.Size = new System.Drawing.Size(107, 13);
            this.lblExchange.TabIndex = 61;
            this.lblExchange.Text = "Exchange Bitcoin On";
            // 
            // label15
            // 
            this.label15.AutoSize = true;
            this.label15.Location = new System.Drawing.Point(17, 259);
            this.label15.Name = "label15";
            this.label15.Size = new System.Drawing.Size(0, 13);
            this.label15.TabIndex = 60;
            // 
            // tabPage4
            // 
            this.tabPage4.Controls.Add(this.statscntrlTradeHistory);
            this.tabPage4.Controls.Add(this.chckOrderHistory);
            this.tabPage4.Controls.Add(this.label18);
            this.tabPage4.Controls.Add(this.lstOrderHistory);
            this.tabPage4.Location = new System.Drawing.Point(4, 22);
            this.tabPage4.Name = "tabPage4";
            this.tabPage4.Padding = new System.Windows.Forms.Padding(3);
            this.tabPage4.Size = new System.Drawing.Size(1121, 525);
            this.tabPage4.TabIndex = 3;
            this.tabPage4.Text = "Trade History";
            this.tabPage4.UseVisualStyleBackColor = true;
            // 
            // statscntrlTradeHistory
            // 
            this.statscntrlTradeHistory.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.statscntrlTradeHistory.Location = new System.Drawing.Point(3, 349);
            this.statscntrlTradeHistory.Name = "statscntrlTradeHistory";
            this.statscntrlTradeHistory.Size = new System.Drawing.Size(1112, 169);
            this.statscntrlTradeHistory.StatusName = "";
            this.statscntrlTradeHistory.TabIndex = 44;
            // 
            // chckOrderHistory
            // 
            this.chckOrderHistory.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.chckOrderHistory.AutoSize = true;
            this.chckOrderHistory.Enabled = false;
            this.chckOrderHistory.Location = new System.Drawing.Point(989, 6);
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
            this.columnHeader5,
            this.columnHeader6,
            this.columnHeader7,
            this.columnHeader11,
            this.columnHeader8,
            this.columnHeader9,
            this.columnHeader10});
            this.lstOrderHistory.FullRowSelect = true;
            this.lstOrderHistory.HideSelection = false;
            this.lstOrderHistory.Location = new System.Drawing.Point(6, 30);
            this.lstOrderHistory.MultiSelect = false;
            this.lstOrderHistory.Name = "lstOrderHistory";
            this.lstOrderHistory.Size = new System.Drawing.Size(1109, 313);
            this.lstOrderHistory.TabIndex = 39;
            this.lstOrderHistory.UseCompatibleStateImageBehavior = false;
            this.lstOrderHistory.View = System.Windows.Forms.View.Details;
            this.lstOrderHistory.SelectedIndexChanged += new System.EventHandler(this.lstOrderHistory_OnSelectedIndexChanged);
            this.lstOrderHistory.MouseClick += new System.Windows.Forms.MouseEventHandler(this.LstOrderHistory_MouseClick);
            // 
            // columnHeader4
            // 
            this.columnHeader4.Text = "Transaction Name";
            this.columnHeader4.Width = 196;
            // 
            // columnHeader5
            // 
            this.columnHeader5.Text = "Sold";
            this.columnHeader5.Width = 68;
            // 
            // columnHeader6
            // 
            this.columnHeader6.Text = "Received";
            this.columnHeader6.Width = 75;
            // 
            // columnHeader7
            // 
            this.columnHeader7.Text = "Price";
            this.columnHeader7.Width = 125;
            // 
            // columnHeader11
            // 
            this.columnHeader11.Text = "Stop";
            this.columnHeader11.Width = 125;
            // 
            // columnHeader8
            // 
            this.columnHeader8.Text = "Exchange";
            this.columnHeader8.Width = 112;
            // 
            // columnHeader9
            // 
            this.columnHeader9.Text = "Date Time";
            this.columnHeader9.Width = 116;
            // 
            // columnHeader10
            // 
            this.columnHeader10.Text = "Status";
            this.columnHeader10.Width = 115;
            // 
            // menuStrip1
            // 
            this.menuStrip1.BackColor = System.Drawing.Color.Transparent;
            this.menuStrip1.Items.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.fileToolStripMenuItem,
            this.optionsToolStripMenuItem,
            this.helpToolStripMenuItem});
            this.menuStrip1.Location = new System.Drawing.Point(0, 0);
            this.menuStrip1.Name = "menuStrip1";
            this.menuStrip1.Size = new System.Drawing.Size(1153, 24);
            this.menuStrip1.TabIndex = 28;
            this.menuStrip1.Text = "menuStrip1";
            // 
            // fileToolStripMenuItem
            // 
            this.fileToolStripMenuItem.DropDownItems.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.connectToolStripMenuItem1,
            this.backupWalletKeyToolStripMenuItem,
            this.toolStripMenuItem1,
            this.exitToolStripMenuItem});
            this.fileToolStripMenuItem.Name = "fileToolStripMenuItem";
            this.fileToolStripMenuItem.Size = new System.Drawing.Size(37, 20);
            this.fileToolStripMenuItem.Text = "&File";
            // 
            // connectToolStripMenuItem1
            // 
            this.connectToolStripMenuItem1.Name = "connectToolStripMenuItem1";
            this.connectToolStripMenuItem1.Size = new System.Drawing.Size(180, 22);
            this.connectToolStripMenuItem1.Text = "Connect...";
            this.connectToolStripMenuItem1.Click += new System.EventHandler(this.connectToolStripMenuItem1_Click);
            // 
            // backupWalletKeyToolStripMenuItem
            // 
            this.backupWalletKeyToolStripMenuItem.Name = "backupWalletKeyToolStripMenuItem";
            this.backupWalletKeyToolStripMenuItem.Size = new System.Drawing.Size(180, 22);
            this.backupWalletKeyToolStripMenuItem.Text = "&Backup Wallet Key...";
            this.backupWalletKeyToolStripMenuItem.Click += new System.EventHandler(this.backupWalletKeyToolStripMenuItem_Click);
            // 
            // toolStripMenuItem1
            // 
            this.toolStripMenuItem1.Name = "toolStripMenuItem1";
            this.toolStripMenuItem1.Size = new System.Drawing.Size(177, 6);
            // 
            // exitToolStripMenuItem
            // 
            this.exitToolStripMenuItem.Name = "exitToolStripMenuItem";
            this.exitToolStripMenuItem.ShortcutKeys = ((System.Windows.Forms.Keys)((System.Windows.Forms.Keys.Alt | System.Windows.Forms.Keys.F4)));
            this.exitToolStripMenuItem.Size = new System.Drawing.Size(180, 22);
            this.exitToolStripMenuItem.Text = "E&xit";
            this.exitToolStripMenuItem.Click += new System.EventHandler(this.exitToolStripMenuItem_Click);
            // 
            // optionsToolStripMenuItem
            // 
            this.optionsToolStripMenuItem.DropDownItems.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.changePasswordToolStripMenuItem,
            this.toolStripMenuItem2,
            this.settingsToolStripMenuItem});
            this.optionsToolStripMenuItem.Name = "optionsToolStripMenuItem";
            this.optionsToolStripMenuItem.Size = new System.Drawing.Size(61, 20);
            this.optionsToolStripMenuItem.Text = "&Options";
            // 
            // changePasswordToolStripMenuItem
            // 
            this.changePasswordToolStripMenuItem.Name = "changePasswordToolStripMenuItem";
            this.changePasswordToolStripMenuItem.Size = new System.Drawing.Size(213, 22);
            this.changePasswordToolStripMenuItem.Text = "&Change Wallet Password...";
            this.changePasswordToolStripMenuItem.Click += new System.EventHandler(this.changePasswordToolStripMenuItem_Click);
            // 
            // toolStripMenuItem2
            // 
            this.toolStripMenuItem2.Name = "toolStripMenuItem2";
            this.toolStripMenuItem2.Size = new System.Drawing.Size(210, 6);
            // 
            // settingsToolStripMenuItem
            // 
            this.settingsToolStripMenuItem.Name = "settingsToolStripMenuItem";
            this.settingsToolStripMenuItem.Size = new System.Drawing.Size(213, 22);
            this.settingsToolStripMenuItem.Text = "&Settings...";
            this.settingsToolStripMenuItem.Click += new System.EventHandler(this.settingsToolStripMenuItem_Click);
            // 
            // helpToolStripMenuItem
            // 
            this.helpToolStripMenuItem.DropDownItems.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.pandorasWalletGuideToolStripMenuItem,
            this.toolStripMenuItem3,
            this.aboutToolStripMenuItem});
            this.helpToolStripMenuItem.Name = "helpToolStripMenuItem";
            this.helpToolStripMenuItem.Size = new System.Drawing.Size(44, 20);
            this.helpToolStripMenuItem.Text = "&Help";
            // 
            // aboutToolStripMenuItem
            // 
            this.aboutToolStripMenuItem.Name = "aboutToolStripMenuItem";
            this.aboutToolStripMenuItem.Size = new System.Drawing.Size(205, 22);
            this.aboutToolStripMenuItem.Text = "&About";
            this.aboutToolStripMenuItem.Click += new System.EventHandler(this.aboutToolStripMenuItem_Click);
            // 
            // label8
            // 
            this.label8.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.label8.AutoSize = true;
            this.label8.Font = new System.Drawing.Font("Segoe UI", 8.25F, System.Drawing.FontStyle.Bold);
            this.label8.Location = new System.Drawing.Point(727, 163);
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
            this.txtBoxReceiveAddress.Location = new System.Drawing.Point(730, 179);
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
            this.panel1.Controls.Add(this.picCoinImage);
            this.panel1.Controls.Add(this.pictureBox1);
            this.panel1.Controls.Add(this.label2);
            this.panel1.Controls.Add(this.pictureBoxBalanceInfo);
            this.panel1.Controls.Add(this.panel2);
            this.panel1.Controls.Add(this.lblUnconfirmed);
            this.panel1.Controls.Add(this.lblStatus);
            this.panel1.Controls.Add(this.LabelStatus);
            this.panel1.Controls.Add(this.lblCoinName);
            this.panel1.Controls.Add(this.lblNameUnconfirmed);
            this.panel1.Font = new System.Drawing.Font("Segoe UI", 8.25F);
            this.panel1.Location = new System.Drawing.Point(19, 39);
            this.panel1.Name = "panel1";
            this.panel1.Size = new System.Drawing.Size(702, 161);
            this.panel1.TabIndex = 21;
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
            // label2
            // 
            this.label2.Anchor = System.Windows.Forms.AnchorStyles.Left;
            this.label2.Font = new System.Drawing.Font("Calibri", 11F, System.Drawing.FontStyle.Bold);
            this.label2.Location = new System.Drawing.Point(423, 35);
            this.label2.Name = "label2";
            this.label2.Size = new System.Drawing.Size(178, 23);
            this.label2.TabIndex = 25;
            this.label2.Text = "Balance details:";
            // 
            // pictureBoxBalanceInfo
            // 
            this.pictureBoxBalanceInfo.Image = ((System.Drawing.Image)(resources.GetObject("pictureBoxBalanceInfo.Image")));
            this.pictureBoxBalanceInfo.Location = new System.Drawing.Point(767, 3);
            this.pictureBoxBalanceInfo.Name = "pictureBoxBalanceInfo";
            this.pictureBoxBalanceInfo.Size = new System.Drawing.Size(15, 15);
            this.pictureBoxBalanceInfo.TabIndex = 24;
            this.pictureBoxBalanceInfo.TabStop = false;
            this.pictureBoxBalanceInfo.Visible = false;
            // 
            // panel2
            // 
            this.panel2.Anchor = System.Windows.Forms.AnchorStyles.Left;
            this.panel2.AutoSize = true;
            this.panel2.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
            this.panel2.Controls.Add(this.lblTotal);
            this.panel2.Controls.Add(this.labelTotal);
            this.panel2.Font = new System.Drawing.Font("Calibri", 8F);
            this.panel2.Location = new System.Drawing.Point(417, 89);
            this.panel2.Margin = new System.Windows.Forms.Padding(0);
            this.panel2.Name = "panel2";
            this.panel2.Padding = new System.Windows.Forms.Padding(2);
            this.panel2.Size = new System.Drawing.Size(206, 25);
            this.panel2.TabIndex = 23;
            // 
            // lblUnconfirmed
            // 
            this.lblUnconfirmed.Anchor = System.Windows.Forms.AnchorStyles.Left;
            this.lblUnconfirmed.AutoSize = true;
            this.lblUnconfirmed.Font = new System.Drawing.Font("Calibri", 10F);
            this.lblUnconfirmed.ForeColor = System.Drawing.Color.Gray;
            this.lblUnconfirmed.Location = new System.Drawing.Point(519, 66);
            this.lblUnconfirmed.MaximumSize = new System.Drawing.Size(160, 0);
            this.lblUnconfirmed.Name = "lblUnconfirmed";
            this.lblUnconfirmed.Size = new System.Drawing.Size(75, 17);
            this.lblUnconfirmed.TabIndex = 22;
            this.lblUnconfirmed.Text = "0.00000000";
            // 
            // lblNameUnconfirmed
            // 
            this.lblNameUnconfirmed.Anchor = System.Windows.Forms.AnchorStyles.Left;
            this.lblNameUnconfirmed.AutoSize = true;
            this.lblNameUnconfirmed.Font = new System.Drawing.Font("Calibri", 10F);
            this.lblNameUnconfirmed.ForeColor = System.Drawing.Color.Gray;
            this.lblNameUnconfirmed.Location = new System.Drawing.Point(423, 65);
            this.lblNameUnconfirmed.Name = "lblNameUnconfirmed";
            this.lblNameUnconfirmed.Size = new System.Drawing.Size(88, 17);
            this.lblNameUnconfirmed.TabIndex = 19;
            this.lblNameUnconfirmed.Text = "Unconfirmed :";
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
            this.statusStripWallet.Location = new System.Drawing.Point(0, 772);
            this.statusStripWallet.Name = "statusStripWallet";
            this.statusStripWallet.Size = new System.Drawing.Size(1153, 20);
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
            this.toolStripConnectionStatus.Size = new System.Drawing.Size(54, 15);
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
            // ctxMenuOrderMenu
            // 
            this.ctxMenuOrderMenu.Items.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.menuItemCancel});
            this.ctxMenuOrderMenu.Name = "ctxMenuOrderMenu";
            this.ctxMenuOrderMenu.Size = new System.Drawing.Size(111, 26);
            // 
            // menuItemCancel
            // 
            this.menuItemCancel.Name = "menuItemCancel";
            this.menuItemCancel.Size = new System.Drawing.Size(110, 22);
            this.menuItemCancel.Text = "Cancel";
            this.menuItemCancel.Click += new System.EventHandler(this.MenuItemCancel_Click);
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
            this.groupBox1.Location = new System.Drawing.Point(730, 32);
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
            // pandorasWalletGuideToolStripMenuItem
            // 
            this.pandorasWalletGuideToolStripMenuItem.Name = "pandorasWalletGuideToolStripMenuItem";
            this.pandorasWalletGuideToolStripMenuItem.Size = new System.Drawing.Size(205, 22);
            this.pandorasWalletGuideToolStripMenuItem.Text = "Pandora\'s Wallet Guide...";
            this.pandorasWalletGuideToolStripMenuItem.Click += new System.EventHandler(this.pandorasWalletGuideToolStripMenuItem_Click);
            // 
            // toolStripMenuItem3
            // 
            this.toolStripMenuItem3.Name = "toolStripMenuItem3";
            this.toolStripMenuItem3.Size = new System.Drawing.Size(202, 6);
            // 
            // AppMainForm
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.BackColor = System.Drawing.SystemColors.InactiveCaption;
            this.ClientSize = new System.Drawing.Size(1153, 791);
            this.Controls.Add(this.groupBox1);
            this.Controls.Add(this.statusStripWallet);
            this.Controls.Add(this.panel1);
            this.Controls.Add(this.txtBoxReceiveAddress);
            this.Controls.Add(this.label8);
            this.Controls.Add(this.tabControl);
            this.Controls.Add(this.menuStrip1);
            this.Icon = ((System.Drawing.Icon)(resources.GetObject("$this.Icon")));
            this.MainMenuStrip = this.menuStrip1;
            this.MinimumSize = new System.Drawing.Size(1119, 830);
            this.Name = "AppMainForm";
            this.StartPosition = System.Windows.Forms.FormStartPosition.CenterScreen;
            this.Text = "Pandora\'s Wallet";
            this.FormClosing += new System.Windows.Forms.FormClosingEventHandler(this.MainWallet_FormClosing);
            this.Load += new System.EventHandler(this.PandoraClientMainWindow_Load);
            this.Shown += new System.EventHandler(this.MainWallet_Shown);
            this.tabControl.ResumeLayout(false);
            this.tabPage1.ResumeLayout(false);
            this.panel3.ResumeLayout(false);
            this.panel3.PerformLayout();
            ((System.ComponentModel.ISupportInitialize)(this.iconSearch)).EndInit();
            this.tabPage2.ResumeLayout(false);
            this.tabPage2.PerformLayout();
            this.tabPage3.ResumeLayout(false);
            this.tabPage3.PerformLayout();
            this.tabPage4.ResumeLayout(false);
            this.tabPage4.PerformLayout();
            this.menuStrip1.ResumeLayout(false);
            this.menuStrip1.PerformLayout();
            this.panel1.ResumeLayout(false);
            this.panel1.PerformLayout();
            ((System.ComponentModel.ISupportInitialize)(this.picCoinImage)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.pictureBox1)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.pictureBoxBalanceInfo)).EndInit();
            this.panel2.ResumeLayout(false);
            this.panel2.PerformLayout();
            this.statusStripWallet.ResumeLayout(false);
            this.statusStripWallet.PerformLayout();
            this.ctxMenuOrderMenu.ResumeLayout(false);
            this.groupBox1.ResumeLayout(false);
            this.groupBox1.PerformLayout();
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion
        private System.Windows.Forms.ImageList imageListTx;
        private System.Windows.Forms.TextBox TxtBoxSendToAddress;
        private System.Windows.Forms.Label lblStatus;
        private System.Windows.Forms.Label LabelStatus;
        private System.Windows.Forms.Label labelTotal;
        private System.Windows.Forms.Label lblCoinName;
        private System.Windows.Forms.PictureBox picCoinImage;
        private System.Windows.Forms.TextBox QuickAmountTextBox;
        private System.Windows.Forms.Label QuickAmmountLabel;
        private System.Windows.Forms.Label label4;
        private System.Windows.Forms.TabControl tabControl;
        private System.Windows.Forms.TabPage tabPage1;
        private System.Windows.Forms.TextBox txtBoxSearchCoin;
        private System.Windows.Forms.TabPage tabPage2;
        private System.Windows.Forms.TextBox txtNotesBox;
        private System.Windows.Forms.Label label9;
        private System.Windows.Forms.Button AddCurrencyBtn;
        private System.Windows.Forms.MenuStrip menuStrip1;
        private System.Windows.Forms.ToolStripMenuItem fileToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem backupWalletKeyToolStripMenuItem;
        private System.Windows.Forms.ToolStripSeparator toolStripMenuItem1;
        private System.Windows.Forms.ToolStripMenuItem exitToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem optionsToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem changePasswordToolStripMenuItem;
        private System.Windows.Forms.ToolStripSeparator toolStripMenuItem2;
        private System.Windows.Forms.ToolStripMenuItem settingsToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem helpToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem aboutToolStripMenuItem;
        private System.Windows.Forms.Label label8;
        private System.Windows.Forms.TextBox txtBoxReceiveAddress;
        private System.Windows.Forms.Label lblTotal;
        private System.Windows.Forms.ToolStripMenuItem connectToolStripMenuItem1;
        private CurrencyView lstViewCurrencies;
        private System.Windows.Forms.Panel panel1;
        private System.Windows.Forms.StatusStrip statusStripWallet;
        private System.Windows.Forms.ToolStripStatusLabel toolStripStatusLabel1;
        private System.Windows.Forms.ToolStripStatusLabel toolStripStatusUsername;
        private System.Windows.Forms.ToolStripStatusLabel toolStripStatusLabel2;
        private System.Windows.Forms.ToolStripStatusLabel toolStripStatusEmail;
        private System.Windows.Forms.ToolStripStatusLabel toolStripConnectionStatus;
        private MenuButton QuickSendButton;
        private System.Windows.Forms.TabPage tabPage3;
        private System.Windows.Forms.TextBox txtTransactionName;
        private System.Windows.Forms.Label label11;
        private System.Windows.Forms.Label lblEstimatePrice;
        private System.Windows.Forms.TextBox txtPrice;
        private System.Windows.Forms.Label label3;
        private System.Windows.Forms.Label lblQuantity;
        private System.Windows.Forms.Button btnExchange;
        private System.Windows.Forms.TextBox txtTotal;
        private System.Windows.Forms.TextBox txtQuantity;
        private System.Windows.Forms.Label lblEstimatePriceCoin;
        private System.Windows.Forms.Label lblTotalReceived;
        private System.Windows.Forms.Label label10;
        private System.Windows.Forms.Label label12;
        private System.Windows.Forms.ListView lstExchangeMarket;
        private System.Windows.Forms.ColumnHeader columnHeader1;
        private System.Windows.Forms.ColumnHeader columnHeader2;
        private System.Windows.Forms.ColumnHeader columnHeader3;
        private System.Windows.Forms.Label label13;
        private System.Windows.Forms.ComboBox cbExchange;
        private System.Windows.Forms.Label lblExchange;
        private System.Windows.Forms.Label label15;
        private System.Windows.Forms.TabPage tabPage4;
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
        private System.Windows.Forms.Label lblNameUnconfirmed;
        private System.Windows.Forms.Label lblUnconfirmed;
        private StatusControl statsctrlExchage;
        private StatusControl statscntrlTradeHistory;
        private System.Windows.Forms.Panel panel2;
        private System.Windows.Forms.ToolTip toolTipBalance;
        private System.Windows.Forms.TextBox txtStopPrice;
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
        private System.Windows.Forms.ContextMenuStrip ctxMenuOrderMenu;
        private System.Windows.Forms.ToolStripMenuItem menuItemCancel;
        private System.Windows.Forms.GroupBox groupBox1;
        private System.Windows.Forms.Label label2;
        private System.Windows.Forms.PictureBox pictureBoxBalanceInfo;
        private System.Windows.Forms.PictureBox pictureBox1;
        private System.Windows.Forms.ToolTip coinTooltip;
        private System.Windows.Forms.Button btnExchangeKeys;
        private System.Windows.Forms.ToolTip toolTipHelp;
        private System.Windows.Forms.ToolStripMenuItem pandorasWalletGuideToolStripMenuItem;
        private System.Windows.Forms.ToolStripSeparator toolStripMenuItem3;
    }
}

