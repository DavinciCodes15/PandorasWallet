using Pandora;

namespace Pandora.Client
{
    partial class PandoraClientMainWindow
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
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(PandoraClientMainWindow));
            this.imageListCoin = new System.Windows.Forms.ImageList(this.components);
            this.imageListTx = new System.Windows.Forms.ImageList(this.components);
            this.FTextBoxSendToAddress = new System.Windows.Forms.TextBox();
            this.groupBox1 = new System.Windows.Forms.GroupBox();
            this.lblTotal = new System.Windows.Forms.Label();
            this.lblStatus = new System.Windows.Forms.Label();
            this.LabelStatus = new System.Windows.Forms.Label();
            this.labelTotal = new System.Windows.Forms.Label();
            this.lblCoinName = new System.Windows.Forms.Label();
            this.picCoinImage = new System.Windows.Forms.PictureBox();
            this.QuickAmountTextBox = new System.Windows.Forms.TextBox();
            this.QuickSendButton = new System.Windows.Forms.Button();
            this.QuickAmmountLabel = new System.Windows.Forms.Label();
            this.label4 = new System.Windows.Forms.Label();
            this.tabControl = new System.Windows.Forms.TabControl();
            this.tabPage1 = new System.Windows.Forms.TabPage();
            this.AddCurrencyBtn = new System.Windows.Forms.Button();
            this.isTickerCheckBox = new System.Windows.Forms.CheckBox();
            this.label2 = new System.Windows.Forms.Label();
            this.CoinSearchBox = new System.Windows.Forms.TextBox();
            this.listViewCryptoCoins = new System.Windows.Forms.ListView();
            this.colCoinName = ((System.Windows.Forms.ColumnHeader)(new System.Windows.Forms.ColumnHeader()));
            this.colTicker = ((System.Windows.Forms.ColumnHeader)(new System.Windows.Forms.ColumnHeader()));
            this.colTotal = ((System.Windows.Forms.ColumnHeader)(new System.Windows.Forms.ColumnHeader()));
            this.colStat = ((System.Windows.Forms.ColumnHeader)(new System.Windows.Forms.ColumnHeader()));
            this.tabPage2 = new System.Windows.Forms.TabPage();
            this.TxNotesBox = new System.Windows.Forms.TextBox();
            this.label9 = new System.Windows.Forms.Label();
            this.listTransactions = new System.Windows.Forms.ListView();
            this.colDate = ((System.Windows.Forms.ColumnHeader)(new System.Windows.Forms.ColumnHeader()));
            this.colFrom = ((System.Windows.Forms.ColumnHeader)(new System.Windows.Forms.ColumnHeader()));
            this.colToAccount = ((System.Windows.Forms.ColumnHeader)(new System.Windows.Forms.ColumnHeader()));
            this.colDebit = ((System.Windows.Forms.ColumnHeader)(new System.Windows.Forms.ColumnHeader()));
            this.colCredit = ((System.Windows.Forms.ColumnHeader)(new System.Windows.Forms.ColumnHeader()));
            this.colConf = ((System.Windows.Forms.ColumnHeader)(new System.Windows.Forms.ColumnHeader()));
            this.tabPage3 = new System.Windows.Forms.TabPage();
            this.label7 = new System.Windows.Forms.Label();
            this.label6 = new System.Windows.Forms.Label();
            this.label5 = new System.Windows.Forms.Label();
            this.txtFees = new System.Windows.Forms.TextBox();
            this.textBox5 = new System.Windows.Forms.TextBox();
            this.listView1 = new System.Windows.Forms.ListView();
            this.columnHeader1 = ((System.Windows.Forms.ColumnHeader)(new System.Windows.Forms.ColumnHeader()));
            this.columnHeader2 = ((System.Windows.Forms.ColumnHeader)(new System.Windows.Forms.ColumnHeader()));
            this.label1 = new System.Windows.Forms.Label();
            this.textBox2 = new System.Windows.Forms.TextBox();
            this.menuStrip1 = new System.Windows.Forms.MenuStrip();
            this.fileToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.ConnectToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
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
            this.imageList1 = new System.Windows.Forms.ImageList(this.components);
            this.btnBackup = new System.Windows.Forms.Button();
            this.folderBrowserDialog1 = new System.Windows.Forms.FolderBrowserDialog();
            this.groupBox1.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.picCoinImage)).BeginInit();
            this.tabControl.SuspendLayout();
            this.tabPage1.SuspendLayout();
            this.tabPage2.SuspendLayout();
            this.tabPage3.SuspendLayout();
            this.menuStrip1.SuspendLayout();
            this.SuspendLayout();
            // 
            // imageListCoin
            // 
            this.imageListCoin.ImageStream = ((System.Windows.Forms.ImageListStreamer)(resources.GetObject("imageListCoin.ImageStream")));
            this.imageListCoin.TransparentColor = System.Drawing.Color.Transparent;
            this.imageListCoin.Images.SetKeyName(0, "moneybag_dollar.png");
            this.imageListCoin.Images.SetKeyName(1, "currency_euro.png");
            this.imageListCoin.Images.SetKeyName(2, "currency_yen.png");
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
            // FTextBoxSendToAddress
            // 
            this.FTextBoxSendToAddress.Font = new System.Drawing.Font("Courier New", 8.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.FTextBoxSendToAddress.Location = new System.Drawing.Point(318, 61);
            this.FTextBoxSendToAddress.Name = "FTextBoxSendToAddress";
            this.FTextBoxSendToAddress.Size = new System.Drawing.Size(323, 20);
            this.FTextBoxSendToAddress.TabIndex = 17;
            // 
            // groupBox1
            // 
            this.groupBox1.BackColor = System.Drawing.Color.White;
            this.groupBox1.Controls.Add(this.lblTotal);
            this.groupBox1.Controls.Add(this.lblStatus);
            this.groupBox1.Controls.Add(this.LabelStatus);
            this.groupBox1.Controls.Add(this.labelTotal);
            this.groupBox1.Controls.Add(this.lblCoinName);
            this.groupBox1.Controls.Add(this.picCoinImage);
            this.groupBox1.Location = new System.Drawing.Point(16, 45);
            this.groupBox1.Name = "groupBox1";
            this.groupBox1.Size = new System.Drawing.Size(278, 96);
            this.groupBox1.TabIndex = 18;
            this.groupBox1.TabStop = false;
            // 
            // lblTotal
            // 
            this.lblTotal.AutoSize = true;
            this.lblTotal.Font = new System.Drawing.Font("Courier New", 9.75F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.lblTotal.ForeColor = System.Drawing.Color.Black;
            this.lblTotal.Location = new System.Drawing.Point(116, 45);
            this.lblTotal.Name = "lblTotal";
            this.lblTotal.Size = new System.Drawing.Size(88, 16);
            this.lblTotal.TabIndex = 22;
            this.lblTotal.Text = "0.00000000";
            // 
            // lblStatus
            // 
            this.lblStatus.AutoSize = true;
            this.lblStatus.Font = new System.Drawing.Font("Courier New", 9.75F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.lblStatus.ForeColor = System.Drawing.Color.Black;
            this.lblStatus.Location = new System.Drawing.Point(116, 66);
            this.lblStatus.Name = "lblStatus";
            this.lblStatus.Size = new System.Drawing.Size(136, 16);
            this.lblStatus.TabIndex = 21;
            this.lblStatus.Text = "Maintanance Mode";
            // 
            // LabelStatus
            // 
            this.LabelStatus.AutoSize = true;
            this.LabelStatus.Font = new System.Drawing.Font("Courier New", 9.75F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.LabelStatus.Location = new System.Drawing.Point(6, 65);
            this.LabelStatus.Name = "LabelStatus";
            this.LabelStatus.Size = new System.Drawing.Size(112, 16);
            this.LabelStatus.TabIndex = 20;
            this.LabelStatus.Text = "Status      :";
            // 
            // labelTotal
            // 
            this.labelTotal.AutoSize = true;
            this.labelTotal.Font = new System.Drawing.Font("Courier New", 9.75F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.labelTotal.Location = new System.Drawing.Point(6, 45);
            this.labelTotal.Name = "labelTotal";
            this.labelTotal.Size = new System.Drawing.Size(112, 16);
            this.labelTotal.TabIndex = 19;
            this.labelTotal.Text = "Total Coins :";
            // 
            // lblCoinName
            // 
            this.lblCoinName.AutoSize = true;
            this.lblCoinName.Font = new System.Drawing.Font("Courier New", 10.75F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.lblCoinName.Location = new System.Drawing.Point(44, 20);
            this.lblCoinName.Name = "lblCoinName";
            this.lblCoinName.Size = new System.Drawing.Size(89, 17);
            this.lblCoinName.TabIndex = 17;
            this.lblCoinName.Text = "Coin Name";
            // 
            // picCoinImage
            // 
            this.picCoinImage.Location = new System.Drawing.Point(6, 10);
            this.picCoinImage.Name = "picCoinImage";
            this.picCoinImage.Size = new System.Drawing.Size(32, 32);
            this.picCoinImage.TabIndex = 18;
            this.picCoinImage.TabStop = false;
            // 
            // QuickAmountTextBox
            // 
            this.QuickAmountTextBox.Location = new System.Drawing.Point(318, 100);
            this.QuickAmountTextBox.Name = "QuickAmountTextBox";
            this.QuickAmountTextBox.Size = new System.Drawing.Size(149, 20);
            this.QuickAmountTextBox.TabIndex = 19;
            this.QuickAmountTextBox.Text = "0";
            this.QuickAmountTextBox.TextChanged += new System.EventHandler(this.QuickAmountTextBox_TextChanged);
            this.QuickAmountTextBox.KeyPress += new System.Windows.Forms.KeyPressEventHandler(this.QuickAmmountTextBox_KeyPress);
            // 
            // QuickSendButton
            // 
            this.QuickSendButton.BackgroundImageLayout = System.Windows.Forms.ImageLayout.None;
            this.QuickSendButton.Font = new System.Drawing.Font("Microsoft Sans Serif", 8.25F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.QuickSendButton.Location = new System.Drawing.Point(531, 100);
            this.QuickSendButton.Name = "QuickSendButton";
            this.QuickSendButton.Size = new System.Drawing.Size(110, 23);
            this.QuickSendButton.TabIndex = 20;
            this.QuickSendButton.Text = "Send...";
            this.QuickSendButton.UseVisualStyleBackColor = true;
            this.QuickSendButton.Click += new System.EventHandler(this.QuickSendButton_Click);
            // 
            // QuickAmmountLabel
            // 
            this.QuickAmmountLabel.AutoSize = true;
            this.QuickAmmountLabel.Font = new System.Drawing.Font("Microsoft Sans Serif", 8.25F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.QuickAmmountLabel.Location = new System.Drawing.Point(315, 84);
            this.QuickAmmountLabel.Name = "QuickAmmountLabel";
            this.QuickAmmountLabel.Size = new System.Drawing.Size(184, 13);
            this.QuickAmmountLabel.TabIndex = 21;
            this.QuickAmmountLabel.Text = "Amount (19998.02... remaining)";
            // 
            // label4
            // 
            this.label4.AutoSize = true;
            this.label4.Font = new System.Drawing.Font("Microsoft Sans Serif", 8.25F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.label4.Location = new System.Drawing.Point(315, 45);
            this.label4.Name = "label4";
            this.label4.Size = new System.Drawing.Size(141, 13);
            this.label4.TabIndex = 22;
            this.label4.Text = "Quick Send To Address";
            // 
            // tabControl
            // 
            this.tabControl.Controls.Add(this.tabPage1);
            this.tabControl.Controls.Add(this.tabPage2);
            this.tabControl.Controls.Add(this.tabPage3);
            this.tabControl.Font = new System.Drawing.Font("Microsoft Sans Serif", 8.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.tabControl.Location = new System.Drawing.Point(12, 152);
            this.tabControl.Name = "tabControl";
            this.tabControl.SelectedIndex = 0;
            this.tabControl.Size = new System.Drawing.Size(910, 397);
            this.tabControl.TabIndex = 24;
            // 
            // tabPage1
            // 
            this.tabPage1.Controls.Add(this.AddCurrencyBtn);
            this.tabPage1.Controls.Add(this.isTickerCheckBox);
            this.tabPage1.Controls.Add(this.label2);
            this.tabPage1.Controls.Add(this.CoinSearchBox);
            this.tabPage1.Controls.Add(this.listViewCryptoCoins);
            this.tabPage1.Location = new System.Drawing.Point(4, 22);
            this.tabPage1.Name = "tabPage1";
            this.tabPage1.Padding = new System.Windows.Forms.Padding(3);
            this.tabPage1.Size = new System.Drawing.Size(902, 371);
            this.tabPage1.TabIndex = 0;
            this.tabPage1.Text = "Crypto Currencies";
            this.tabPage1.UseVisualStyleBackColor = true;
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
            // isTickerCheckBox
            // 
            this.isTickerCheckBox.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.isTickerCheckBox.AutoSize = true;
            this.isTickerCheckBox.Font = new System.Drawing.Font("Microsoft Sans Serif", 8.25F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.isTickerCheckBox.Location = new System.Drawing.Point(499, 11);
            this.isTickerCheckBox.Name = "isTickerCheckBox";
            this.isTickerCheckBox.Size = new System.Drawing.Size(106, 17);
            this.isTickerCheckBox.TabIndex = 18;
            this.isTickerCheckBox.Text = "Search Ticker";
            this.isTickerCheckBox.UseVisualStyleBackColor = true;
            // 
            // label2
            // 
            this.label2.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.label2.AutoSize = true;
            this.label2.Font = new System.Drawing.Font("Microsoft Sans Serif", 8.25F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.label2.Location = new System.Drawing.Point(652, 12);
            this.label2.Name = "label2";
            this.label2.Size = new System.Drawing.Size(35, 13);
            this.label2.TabIndex = 17;
            this.label2.Text = "Find:";
            // 
            // CoinSearchBox
            // 
            this.CoinSearchBox.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.CoinSearchBox.Location = new System.Drawing.Point(699, 9);
            this.CoinSearchBox.Name = "CoinSearchBox";
            this.CoinSearchBox.Size = new System.Drawing.Size(197, 20);
            this.CoinSearchBox.TabIndex = 16;
            this.CoinSearchBox.TextChanged += new System.EventHandler(this.CoinSearchBox_TextChanged);
            // 
            // listViewCryptoCoins
            // 
            this.listViewCryptoCoins.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.listViewCryptoCoins.Columns.AddRange(new System.Windows.Forms.ColumnHeader[] {
            this.colCoinName,
            this.colTicker,
            this.colTotal,
            this.colStat});
            this.listViewCryptoCoins.FullRowSelect = true;
            this.listViewCryptoCoins.GridLines = true;
            this.listViewCryptoCoins.HideSelection = false;
            this.listViewCryptoCoins.Location = new System.Drawing.Point(6, 35);
            this.listViewCryptoCoins.MultiSelect = false;
            this.listViewCryptoCoins.Name = "listViewCryptoCoins";
            this.listViewCryptoCoins.Size = new System.Drawing.Size(890, 333);
            this.listViewCryptoCoins.SmallImageList = this.imageListCoin;
            this.listViewCryptoCoins.Sorting = System.Windows.Forms.SortOrder.Ascending;
            this.listViewCryptoCoins.TabIndex = 14;
            this.listViewCryptoCoins.UseCompatibleStateImageBehavior = false;
            this.listViewCryptoCoins.View = System.Windows.Forms.View.Details;
            this.listViewCryptoCoins.ItemSelectionChanged += new System.Windows.Forms.ListViewItemSelectionChangedEventHandler(this.listViewCryptoCoins_ItemSelectionChanged);
            // 
            // colCoinName
            // 
            this.colCoinName.Text = "CoinName";
            this.colCoinName.Width = 305;
            // 
            // colTicker
            // 
            this.colTicker.Text = "Ticker";
            this.colTicker.Width = 180;
            // 
            // colTotal
            // 
            this.colTotal.Text = "Total";
            this.colTotal.Width = 220;
            // 
            // colStat
            // 
            this.colStat.Text = "Status";
            this.colStat.Width = 180;
            // 
            // tabPage2
            // 
            this.tabPage2.Controls.Add(this.TxNotesBox);
            this.tabPage2.Controls.Add(this.label9);
            this.tabPage2.Controls.Add(this.listTransactions);
            this.tabPage2.Location = new System.Drawing.Point(4, 22);
            this.tabPage2.Name = "tabPage2";
            this.tabPage2.Padding = new System.Windows.Forms.Padding(3);
            this.tabPage2.Size = new System.Drawing.Size(902, 371);
            this.tabPage2.TabIndex = 1;
            this.tabPage2.Text = "Transactions";
            this.tabPage2.UseVisualStyleBackColor = true;
            // 
            // TxNotesBox
            // 
            this.TxNotesBox.Location = new System.Drawing.Point(6, 293);
            this.TxNotesBox.Multiline = true;
            this.TxNotesBox.Name = "TxNotesBox";
            this.TxNotesBox.Size = new System.Drawing.Size(881, 72);
            this.TxNotesBox.TabIndex = 17;
            // 
            // label9
            // 
            this.label9.AutoSize = true;
            this.label9.Location = new System.Drawing.Point(3, 274);
            this.label9.Name = "label9";
            this.label9.Size = new System.Drawing.Size(38, 13);
            this.label9.TabIndex = 16;
            this.label9.Text = "Notes:";
            // 
            // listTransactions
            // 
            this.listTransactions.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.listTransactions.Columns.AddRange(new System.Windows.Forms.ColumnHeader[] {
            this.colDate,
            this.colFrom,
            this.colToAccount,
            this.colDebit,
            this.colCredit,
            this.colConf});
            this.listTransactions.FullRowSelect = true;
            this.listTransactions.HideSelection = false;
            this.listTransactions.Location = new System.Drawing.Point(9, 6);
            this.listTransactions.MultiSelect = false;
            this.listTransactions.Name = "listTransactions";
            this.listTransactions.Size = new System.Drawing.Size(893, 265);
            this.listTransactions.SmallImageList = this.imageListTx;
            this.listTransactions.Sorting = System.Windows.Forms.SortOrder.Descending;
            this.listTransactions.TabIndex = 15;
            this.listTransactions.UseCompatibleStateImageBehavior = false;
            this.listTransactions.View = System.Windows.Forms.View.Details;
            this.listTransactions.ItemSelectionChanged += new System.Windows.Forms.ListViewItemSelectionChangedEventHandler(this.listTransactions_ItemSelectionChanged);
            // 
            // colDate
            // 
            this.colDate.Text = "Date";
            this.colDate.Width = 150;
            // 
            // colFrom
            // 
            this.colFrom.Text = "From";
            this.colFrom.Width = 207;
            // 
            // colToAccount
            // 
            this.colToAccount.Text = "To Account";
            this.colToAccount.Width = 220;
            // 
            // colDebit
            // 
            this.colDebit.Text = "Debit";
            this.colDebit.Width = 119;
            // 
            // colCredit
            // 
            this.colCredit.Text = "Credit";
            this.colCredit.Width = 111;
            // 
            // colConf
            // 
            this.colConf.Text = "Confirmations";
            this.colConf.Width = 82;
            // 
            // tabPage3
            // 
            this.tabPage3.Controls.Add(this.label7);
            this.tabPage3.Controls.Add(this.label6);
            this.tabPage3.Controls.Add(this.label5);
            this.tabPage3.Controls.Add(this.txtFees);
            this.tabPage3.Controls.Add(this.textBox5);
            this.tabPage3.Controls.Add(this.listView1);
            this.tabPage3.Controls.Add(this.label1);
            this.tabPage3.Controls.Add(this.textBox2);
            this.tabPage3.Location = new System.Drawing.Point(4, 22);
            this.tabPage3.Name = "tabPage3";
            this.tabPage3.Padding = new System.Windows.Forms.Padding(3);
            this.tabPage3.Size = new System.Drawing.Size(902, 371);
            this.tabPage3.TabIndex = 2;
            this.tabPage3.Text = "Advance Send";
            this.tabPage3.UseVisualStyleBackColor = true;
            // 
            // label7
            // 
            this.label7.AutoSize = true;
            this.label7.Location = new System.Drawing.Point(6, 73);
            this.label7.Name = "label7";
            this.label7.Size = new System.Drawing.Size(92, 13);
            this.label7.TabIndex = 25;
            this.label7.Text = "Transaction Note:";
            // 
            // label6
            // 
            this.label6.AutoSize = true;
            this.label6.Location = new System.Drawing.Point(6, 165);
            this.label6.Name = "label6";
            this.label6.Size = new System.Drawing.Size(73, 13);
            this.label6.TabIndex = 24;
            this.label6.Text = "Address Book";
            // 
            // label5
            // 
            this.label5.AutoSize = true;
            this.label5.Location = new System.Drawing.Point(6, 44);
            this.label5.Name = "label5";
            this.label5.Size = new System.Drawing.Size(30, 13);
            this.label5.TabIndex = 23;
            this.label5.Text = "Fees";
            // 
            // txtFees
            // 
            this.txtFees.Location = new System.Drawing.Point(91, 41);
            this.txtFees.Name = "txtFees";
            this.txtFees.Size = new System.Drawing.Size(149, 20);
            this.txtFees.TabIndex = 22;
            this.txtFees.Text = "2";
            // 
            // textBox5
            // 
            this.textBox5.Location = new System.Drawing.Point(9, 89);
            this.textBox5.Multiline = true;
            this.textBox5.Name = "textBox5";
            this.textBox5.Size = new System.Drawing.Size(881, 72);
            this.textBox5.TabIndex = 21;
            // 
            // listView1
            // 
            this.listView1.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.listView1.Columns.AddRange(new System.Windows.Forms.ColumnHeader[] {
            this.columnHeader1,
            this.columnHeader2});
            this.listView1.FullRowSelect = true;
            this.listView1.HideSelection = false;
            this.listView1.Location = new System.Drawing.Point(9, 181);
            this.listView1.MultiSelect = false;
            this.listView1.Name = "listView1";
            this.listView1.Size = new System.Drawing.Size(889, 184);
            this.listView1.SmallImageList = this.imageListTx;
            this.listView1.TabIndex = 20;
            this.listView1.UseCompatibleStateImageBehavior = false;
            this.listView1.View = System.Windows.Forms.View.Details;
            // 
            // columnHeader1
            // 
            this.columnHeader1.Text = "Account Label";
            this.columnHeader1.Width = 300;
            // 
            // columnHeader2
            // 
            this.columnHeader2.Text = "Label Address";
            this.columnHeader2.Width = 260;
            // 
            // label1
            // 
            this.label1.AutoSize = true;
            this.label1.Location = new System.Drawing.Point(6, 18);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(79, 13);
            this.label1.TabIndex = 19;
            this.label1.Text = "Account Label:";
            // 
            // textBox2
            // 
            this.textBox2.Font = new System.Drawing.Font("Courier New", 8.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.textBox2.Location = new System.Drawing.Point(91, 15);
            this.textBox2.Name = "textBox2";
            this.textBox2.Size = new System.Drawing.Size(323, 20);
            this.textBox2.TabIndex = 18;
            // 
            // menuStrip1
            // 
            this.menuStrip1.Items.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.fileToolStripMenuItem,
            this.optionsToolStripMenuItem,
            this.helpToolStripMenuItem});
            this.menuStrip1.Location = new System.Drawing.Point(0, 0);
            this.menuStrip1.Name = "menuStrip1";
            this.menuStrip1.Size = new System.Drawing.Size(934, 24);
            this.menuStrip1.TabIndex = 28;
            this.menuStrip1.Text = "menuStrip1";
            // 
            // fileToolStripMenuItem
            // 
            this.fileToolStripMenuItem.DropDownItems.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.ConnectToolStripMenuItem,
            this.backupWalletKeyToolStripMenuItem,
            this.toolStripMenuItem1,
            this.exitToolStripMenuItem});
            this.fileToolStripMenuItem.Name = "fileToolStripMenuItem";
            this.fileToolStripMenuItem.Size = new System.Drawing.Size(37, 20);
            this.fileToolStripMenuItem.Text = "&File";
            // 
            // ConnectToolStripMenuItem
            // 
            this.ConnectToolStripMenuItem.Name = "ConnectToolStripMenuItem";
            this.ConnectToolStripMenuItem.Size = new System.Drawing.Size(180, 22);
            this.ConnectToolStripMenuItem.Text = "Disconnect";
            this.ConnectToolStripMenuItem.Click += new System.EventHandler(this.DisconnectToolStripMenuItem_Click);
            // 
            // backupWalletKeyToolStripMenuItem
            // 
            this.backupWalletKeyToolStripMenuItem.Name = "backupWalletKeyToolStripMenuItem";
            this.backupWalletKeyToolStripMenuItem.Size = new System.Drawing.Size(180, 22);
            this.backupWalletKeyToolStripMenuItem.Text = "&Backup Wallet Key...";
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
            this.changePasswordToolStripMenuItem.Size = new System.Drawing.Size(177, 22);
            this.changePasswordToolStripMenuItem.Text = "&Change Password...";
            // 
            // toolStripMenuItem2
            // 
            this.toolStripMenuItem2.Name = "toolStripMenuItem2";
            this.toolStripMenuItem2.Size = new System.Drawing.Size(174, 6);
            // 
            // settingsToolStripMenuItem
            // 
            this.settingsToolStripMenuItem.Name = "settingsToolStripMenuItem";
            this.settingsToolStripMenuItem.Size = new System.Drawing.Size(177, 22);
            this.settingsToolStripMenuItem.Text = "&Settings...";
            this.settingsToolStripMenuItem.Click += new System.EventHandler(this.settingsToolStripMenuItem_Click);
            // 
            // helpToolStripMenuItem
            // 
            this.helpToolStripMenuItem.DropDownItems.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.aboutToolStripMenuItem});
            this.helpToolStripMenuItem.Name = "helpToolStripMenuItem";
            this.helpToolStripMenuItem.Size = new System.Drawing.Size(44, 20);
            this.helpToolStripMenuItem.Text = "&Help";
            // 
            // aboutToolStripMenuItem
            // 
            this.aboutToolStripMenuItem.Name = "aboutToolStripMenuItem";
            this.aboutToolStripMenuItem.Size = new System.Drawing.Size(107, 22);
            this.aboutToolStripMenuItem.Text = "&About";
            this.aboutToolStripMenuItem.Click += new System.EventHandler(this.aboutToolStripMenuItem_Click);
            // 
            // label8
            // 
            this.label8.AutoSize = true;
            this.label8.Font = new System.Drawing.Font("Microsoft Sans Serif", 8.25F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.label8.Location = new System.Drawing.Point(315, 139);
            this.label8.Name = "label8";
            this.label8.Size = new System.Drawing.Size(107, 13);
            this.label8.TabIndex = 29;
            this.label8.Text = "Receive Address:";
            // 
            // txtBoxReceiveAddress
            // 
            this.txtBoxReceiveAddress.Location = new System.Drawing.Point(419, 136);
            this.txtBoxReceiveAddress.Name = "txtBoxReceiveAddress";
            this.txtBoxReceiveAddress.ReadOnly = true;
            this.txtBoxReceiveAddress.Size = new System.Drawing.Size(484, 20);
            this.txtBoxReceiveAddress.TabIndex = 30;
            // 
            // imageList1
            // 
            this.imageList1.ColorDepth = System.Windows.Forms.ColorDepth.Depth8Bit;
            this.imageList1.ImageSize = new System.Drawing.Size(16, 16);
            this.imageList1.TransparentColor = System.Drawing.Color.Transparent;
            // 
            // btnBackup
            // 
            this.btnBackup.Font = new System.Drawing.Font("Microsoft Sans Serif", 8.25F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.btnBackup.Location = new System.Drawing.Point(822, 35);
            this.btnBackup.Name = "btnBackup";
            this.btnBackup.Size = new System.Drawing.Size(100, 23);
            this.btnBackup.TabIndex = 31;
            this.btnBackup.Text = "Backup Wallet";
            this.btnBackup.UseVisualStyleBackColor = true;
            this.btnBackup.Click += new System.EventHandler(this.btnBackup_Click);
            // 
            // PandoraClientMainWindow
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.BackColor = System.Drawing.SystemColors.InactiveCaption;
            this.ClientSize = new System.Drawing.Size(934, 561);
            this.Controls.Add(this.btnBackup);
            this.Controls.Add(this.txtBoxReceiveAddress);
            this.Controls.Add(this.label8);
            this.Controls.Add(this.tabControl);
            this.Controls.Add(this.label4);
            this.Controls.Add(this.QuickAmmountLabel);
            this.Controls.Add(this.QuickSendButton);
            this.Controls.Add(this.QuickAmountTextBox);
            this.Controls.Add(this.groupBox1);
            this.Controls.Add(this.FTextBoxSendToAddress);
            this.Controls.Add(this.menuStrip1);
            this.Icon = ((System.Drawing.Icon)(resources.GetObject("$this.Icon")));
            this.MainMenuStrip = this.menuStrip1;
            this.MinimumSize = new System.Drawing.Size(950, 600);
            this.Name = "PandoraClientMainWindow";
            this.StartPosition = System.Windows.Forms.FormStartPosition.CenterScreen;
            this.Text = "Pandora\'s Key - [Account Name]";
            this.FormClosing += new System.Windows.Forms.FormClosingEventHandler(this.MainWallet_FormClosing);
            this.Shown += new System.EventHandler(this.MainWallet_Shown);
            this.groupBox1.ResumeLayout(false);
            this.groupBox1.PerformLayout();
            ((System.ComponentModel.ISupportInitialize)(this.picCoinImage)).EndInit();
            this.tabControl.ResumeLayout(false);
            this.tabPage1.ResumeLayout(false);
            this.tabPage1.PerformLayout();
            this.tabPage2.ResumeLayout(false);
            this.tabPage2.PerformLayout();
            this.tabPage3.ResumeLayout(false);
            this.tabPage3.PerformLayout();
            this.menuStrip1.ResumeLayout(false);
            this.menuStrip1.PerformLayout();
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.ImageList imageListCoin;
        private System.Windows.Forms.ImageList imageListTx;
        private System.Windows.Forms.TextBox FTextBoxSendToAddress;
        private System.Windows.Forms.GroupBox groupBox1;
        private System.Windows.Forms.Label lblStatus;
        private System.Windows.Forms.Label LabelStatus;
        private System.Windows.Forms.Label labelTotal;
        private System.Windows.Forms.Label lblCoinName;
        private System.Windows.Forms.PictureBox picCoinImage;
        private System.Windows.Forms.TextBox QuickAmountTextBox;
        private System.Windows.Forms.Button QuickSendButton;
        private System.Windows.Forms.Label QuickAmmountLabel;
        private System.Windows.Forms.Label label4;
        private System.Windows.Forms.TabControl tabControl;
        private System.Windows.Forms.TabPage tabPage1;
        private System.Windows.Forms.CheckBox isTickerCheckBox;
        private System.Windows.Forms.Label label2;
        private System.Windows.Forms.TextBox CoinSearchBox;
        private System.Windows.Forms.ListView listViewCryptoCoins;
        private System.Windows.Forms.ColumnHeader colCoinName;
        private System.Windows.Forms.ColumnHeader colTicker;
        private System.Windows.Forms.ColumnHeader colTotal;
        private System.Windows.Forms.ColumnHeader colStat;
        private System.Windows.Forms.TabPage tabPage2;
        private System.Windows.Forms.TextBox TxNotesBox;
        private System.Windows.Forms.Label label9;
        private System.Windows.Forms.ListView listTransactions;
        private System.Windows.Forms.ColumnHeader colDate;
        private System.Windows.Forms.ColumnHeader colFrom;
        private System.Windows.Forms.ColumnHeader colToAccount;
        private System.Windows.Forms.ColumnHeader colDebit;
        private System.Windows.Forms.ColumnHeader colCredit;
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
        private System.Windows.Forms.TabPage tabPage3;
        private System.Windows.Forms.Label label7;
        private System.Windows.Forms.Label label6;
        private System.Windows.Forms.Label label5;
        private System.Windows.Forms.TextBox txtFees;
        private System.Windows.Forms.TextBox textBox5;
        private System.Windows.Forms.ListView listView1;
        private System.Windows.Forms.ColumnHeader columnHeader1;
        private System.Windows.Forms.ColumnHeader columnHeader2;
        private System.Windows.Forms.Label label1;
        private System.Windows.Forms.TextBox textBox2;
        private System.Windows.Forms.ToolStripMenuItem ConnectToolStripMenuItem;
        private System.Windows.Forms.Label label8;
        private System.Windows.Forms.TextBox txtBoxReceiveAddress;
        private System.Windows.Forms.Label lblTotal;
        private System.Windows.Forms.ColumnHeader colConf;
        private System.Windows.Forms.ImageList imageList1;
        private System.Windows.Forms.Button btnBackup;
        private System.Windows.Forms.FolderBrowserDialog folderBrowserDialog1;
    }
}

