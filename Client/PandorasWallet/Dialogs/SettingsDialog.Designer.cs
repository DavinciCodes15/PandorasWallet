namespace Pandora.Client.PandorasWallet.Dialogs
{
    partial class SettingsDialog
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
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(SettingsDialog));
            this.lblPort = new System.Windows.Forms.Label();
            this.lblServerName = new System.Windows.Forms.Label();
            this.txtServerName = new System.Windows.Forms.TextBox();
            this.txtDataPath = new System.Windows.Forms.TextBox();
            this.lblDataPath = new System.Windows.Forms.Label();
            this.checkEncrypted = new System.Windows.Forms.CheckBox();
            this.label1 = new System.Windows.Forms.Label();
            this.lblDefaultCoin = new System.Windows.Forms.Label();
            this.btnChangeDefaultCoin = new System.Windows.Forms.Button();
            this.btnResetDefaults = new System.Windows.Forms.Button();
            this.BrowseButton = new System.Windows.Forms.Button();
            this.groupConnection = new System.Windows.Forms.GroupBox();
            this.numPort = new System.Windows.Forms.NumericUpDown();
            this.groupBox2 = new System.Windows.Forms.GroupBox();
            this.imgBoxDefaultCoin = new System.Windows.Forms.PictureBox();
            this.checkEncryptWallet = new System.Windows.Forms.CheckBox();
            this.SelectFolderDialog = new System.Windows.Forms.FolderBrowserDialog();
            this.groupConnection.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.numPort)).BeginInit();
            this.groupBox2.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.imgBoxDefaultCoin)).BeginInit();
            this.SuspendLayout();
            // 
            // btnCancel
            // 
            this.btnCancel.BackColor = System.Drawing.Color.Transparent;
            this.btnCancel.Location = new System.Drawing.Point(341, 299);
            this.btnCancel.UseVisualStyleBackColor = false;
            this.btnCancel.Click += new System.EventHandler(this.btnCancel_Click);
            // 
            // btnOK
            // 
            this.btnOK.BackColor = System.Drawing.Color.Transparent;
            this.btnOK.Location = new System.Drawing.Point(259, 299);
            this.btnOK.UseVisualStyleBackColor = false;
            this.btnOK.Click += new System.EventHandler(this.btnOK_Click);
            // 
            // lblPort
            // 
            this.lblPort.AutoSize = true;
            this.lblPort.Location = new System.Drawing.Point(18, 73);
            this.lblPort.Name = "lblPort";
            this.lblPort.Size = new System.Drawing.Size(69, 13);
            this.lblPort.TabIndex = 8;
            this.lblPort.Text = "Port Number:";
            // 
            // lblServerName
            // 
            this.lblServerName.AutoSize = true;
            this.lblServerName.Location = new System.Drawing.Point(19, 35);
            this.lblServerName.Name = "lblServerName";
            this.lblServerName.Size = new System.Drawing.Size(72, 13);
            this.lblServerName.TabIndex = 9;
            this.lblServerName.Text = "Server Name:";
            // 
            // txtServerName
            // 
            this.txtServerName.Location = new System.Drawing.Point(96, 32);
            this.txtServerName.Name = "txtServerName";
            this.txtServerName.Size = new System.Drawing.Size(290, 20);
            this.txtServerName.TabIndex = 10;
            // 
            // txtDataPath
            // 
            this.txtDataPath.Location = new System.Drawing.Point(94, 80);
            this.txtDataPath.Name = "txtDataPath";
            this.txtDataPath.Size = new System.Drawing.Size(209, 20);
            this.txtDataPath.TabIndex = 11;
            this.txtDataPath.Leave += new System.EventHandler(this.txtDataPath_Leave);
            // 
            // lblDataPath
            // 
            this.lblDataPath.AutoSize = true;
            this.lblDataPath.Location = new System.Drawing.Point(30, 83);
            this.lblDataPath.Name = "lblDataPath";
            this.lblDataPath.Size = new System.Drawing.Size(58, 13);
            this.lblDataPath.TabIndex = 13;
            this.lblDataPath.Text = "Data Path:";
            // 
            // checkEncrypted
            // 
            this.checkEncrypted.AutoSize = true;
            this.checkEncrypted.Checked = true;
            this.checkEncrypted.CheckState = System.Windows.Forms.CheckState.Checked;
            this.checkEncrypted.Location = new System.Drawing.Point(245, 73);
            this.checkEncrypted.Name = "checkEncrypted";
            this.checkEncrypted.Size = new System.Drawing.Size(119, 17);
            this.checkEncrypted.TabIndex = 15;
            this.checkEncrypted.Text = "Encrypt Connection";
            this.checkEncrypted.UseVisualStyleBackColor = true;
            // 
            // label1
            // 
            this.label1.AutoSize = true;
            this.label1.Location = new System.Drawing.Point(21, 34);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(68, 13);
            this.label1.TabIndex = 16;
            this.label1.Text = "Default Coin:";
            // 
            // lblDefaultCoin
            // 
            this.lblDefaultCoin.AutoSize = true;
            this.lblDefaultCoin.Font = new System.Drawing.Font("Calibri", 10F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.lblDefaultCoin.Location = new System.Drawing.Point(151, 34);
            this.lblDefaultCoin.Name = "lblDefaultCoin";
            this.lblDefaultCoin.Size = new System.Drawing.Size(118, 17);
            this.lblDefaultCoin.TabIndex = 17;
            this.lblDefaultCoin.Text = "No Default Chosen\r\n";
            // 
            // btnChangeDefaultCoin
            // 
            this.btnChangeDefaultCoin.BackColor = System.Drawing.Color.Transparent;
            this.btnChangeDefaultCoin.Location = new System.Drawing.Point(287, 29);
            this.btnChangeDefaultCoin.Name = "btnChangeDefaultCoin";
            this.btnChangeDefaultCoin.Size = new System.Drawing.Size(75, 23);
            this.btnChangeDefaultCoin.TabIndex = 18;
            this.btnChangeDefaultCoin.Text = "Change";
            this.btnChangeDefaultCoin.UseVisualStyleBackColor = true;
            this.btnChangeDefaultCoin.Click += new System.EventHandler(this.btnChangeDefaultCoin_Click);
            // 
            // btnResetDefaults
            // 
            this.btnResetDefaults.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));
            this.btnResetDefaults.BackColor = System.Drawing.Color.Transparent;
            this.btnResetDefaults.Location = new System.Drawing.Point(18, 299);
            this.btnResetDefaults.Name = "btnResetDefaults";
            this.btnResetDefaults.Size = new System.Drawing.Size(87, 23);
            this.btnResetDefaults.TabIndex = 19;
            this.btnResetDefaults.Text = "Reset Defaults";
            this.btnResetDefaults.UseVisualStyleBackColor = true;
            this.btnResetDefaults.Click += new System.EventHandler(this.btnResetDefaults_Click);
            // 
            // BrowseButton
            // 
            this.BrowseButton.BackColor = System.Drawing.Color.Transparent;
            this.BrowseButton.Location = new System.Drawing.Point(309, 78);
            this.BrowseButton.Name = "BrowseButton";
            this.BrowseButton.Size = new System.Drawing.Size(75, 23);
            this.BrowseButton.TabIndex = 20;
            this.BrowseButton.Text = "Browse";
            this.BrowseButton.UseVisualStyleBackColor = true;
            this.BrowseButton.Click += new System.EventHandler(this.BrowseButton_Click);
            // 
            // groupConnection
            // 
            this.groupConnection.Controls.Add(this.numPort);
            this.groupConnection.Controls.Add(this.lblPort);
            this.groupConnection.Controls.Add(this.lblServerName);
            this.groupConnection.Controls.Add(this.txtServerName);
            this.groupConnection.Controls.Add(this.checkEncrypted);
            this.groupConnection.Location = new System.Drawing.Point(14, 158);
            this.groupConnection.Name = "groupConnection";
            this.groupConnection.Size = new System.Drawing.Size(402, 132);
            this.groupConnection.TabIndex = 21;
            this.groupConnection.TabStop = false;
            this.groupConnection.Text = "Connection";
            this.groupConnection.Visible = false;
            // 
            // numPort
            // 
            this.numPort.Location = new System.Drawing.Point(96, 72);
            this.numPort.Maximum = new decimal(new int[] {
            65535,
            0,
            0,
            0});
            this.numPort.Name = "numPort";
            this.numPort.Size = new System.Drawing.Size(120, 20);
            this.numPort.TabIndex = 16;
            // 
            // groupBox2
            // 
            this.groupBox2.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.groupBox2.Controls.Add(this.imgBoxDefaultCoin);
            this.groupBox2.Controls.Add(this.checkEncryptWallet);
            this.groupBox2.Controls.Add(this.txtDataPath);
            this.groupBox2.Controls.Add(this.lblDataPath);
            this.groupBox2.Controls.Add(this.BrowseButton);
            this.groupBox2.Controls.Add(this.btnChangeDefaultCoin);
            this.groupBox2.Controls.Add(this.lblDefaultCoin);
            this.groupBox2.Controls.Add(this.label1);
            this.groupBox2.Location = new System.Drawing.Point(16, 12);
            this.groupBox2.Name = "groupBox2";
            this.groupBox2.Size = new System.Drawing.Size(400, 140);
            this.groupBox2.TabIndex = 22;
            this.groupBox2.TabStop = false;
            this.groupBox2.Text = "Wallet";
            // 
            // imgBoxDefaultCoin
            // 
            this.imgBoxDefaultCoin.Location = new System.Drawing.Point(105, 24);
            this.imgBoxDefaultCoin.Name = "imgBoxDefaultCoin";
            this.imgBoxDefaultCoin.Size = new System.Drawing.Size(40, 40);
            this.imgBoxDefaultCoin.TabIndex = 22;
            this.imgBoxDefaultCoin.TabStop = false;
            // 
            // checkEncryptWallet
            // 
            this.checkEncryptWallet.AutoSize = true;
            this.checkEncryptWallet.Checked = true;
            this.checkEncryptWallet.CheckState = System.Windows.Forms.CheckState.Checked;
            this.checkEncryptWallet.Location = new System.Drawing.Point(169, 117);
            this.checkEncryptWallet.Name = "checkEncryptWallet";
            this.checkEncryptWallet.Size = new System.Drawing.Size(215, 17);
            this.checkEncryptWallet.TabIndex = 21;
            this.checkEncryptWallet.Text = "Ask for wallet password (Encrypt Wallet)";
            this.checkEncryptWallet.UseVisualStyleBackColor = true;
            this.checkEncryptWallet.Visible = false;
            this.checkEncryptWallet.CheckedChanged += new System.EventHandler(this.checkEncryptWallet_CheckedChanged);
            // 
            // SettingsDialogDummy
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.BackColor = System.Drawing.SystemColors.InactiveCaption;
            this.ClientSize = new System.Drawing.Size(428, 334);
            this.Controls.Add(this.groupBox2);
            this.Controls.Add(this.groupConnection);
            this.Controls.Add(this.btnResetDefaults);
            this.Icon = ((System.Drawing.Icon)(resources.GetObject("$this.Icon")));
            this.MaximumSize = new System.Drawing.Size(500, 500);
            this.MinimumSize = new System.Drawing.Size(444, 232);
            this.Name = "SettingsDialogDummy";
            this.StartPosition = System.Windows.Forms.FormStartPosition.CenterScreen;
            this.Text = "Pandora\'s Wallet Settings";
            this.FormClosing += new System.Windows.Forms.FormClosingEventHandler(this.SettingsDialogDummy_FormClosing);
            this.Shown += new System.EventHandler(this.SettingsDialogDummy_Shown);
            this.Controls.SetChildIndex(this.btnOK, 0);
            this.Controls.SetChildIndex(this.btnCancel, 0);
            this.Controls.SetChildIndex(this.btnResetDefaults, 0);
            this.Controls.SetChildIndex(this.groupConnection, 0);
            this.Controls.SetChildIndex(this.groupBox2, 0);
            this.groupConnection.ResumeLayout(false);
            this.groupConnection.PerformLayout();
            ((System.ComponentModel.ISupportInitialize)(this.numPort)).EndInit();
            this.groupBox2.ResumeLayout(false);
            this.groupBox2.PerformLayout();
            ((System.ComponentModel.ISupportInitialize)(this.imgBoxDefaultCoin)).EndInit();
            this.ResumeLayout(false);

        }

        #endregion
        private System.Windows.Forms.Label lblPort;
        private System.Windows.Forms.Label lblServerName;
        private System.Windows.Forms.TextBox txtServerName;
        private System.Windows.Forms.TextBox txtDataPath;
        private System.Windows.Forms.Label lblDataPath;
        private System.Windows.Forms.CheckBox checkEncrypted;
        private System.Windows.Forms.Label label1;
        private System.Windows.Forms.Label lblDefaultCoin;
        private System.Windows.Forms.Button btnChangeDefaultCoin;
        private System.Windows.Forms.Button btnResetDefaults;
        private System.Windows.Forms.Button BrowseButton;
        private System.Windows.Forms.GroupBox groupConnection;
        private System.Windows.Forms.GroupBox groupBox2;
        private System.Windows.Forms.CheckBox checkEncryptWallet;
        private System.Windows.Forms.NumericUpDown numPort;
        private System.Windows.Forms.FolderBrowserDialog SelectFolderDialog;
        private System.Windows.Forms.PictureBox imgBoxDefaultCoin;
    }
}
