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
            this.txtDataPath = new System.Windows.Forms.TextBox();
            this.lblDataPath = new System.Windows.Forms.Label();
            this.label1 = new System.Windows.Forms.Label();
            this.lblDefaultCoin = new System.Windows.Forms.Label();
            this.btnChangeDefaultCoin = new System.Windows.Forms.Button();
            this.BrowseButton = new System.Windows.Forms.Button();
            this.groupBox2 = new System.Windows.Forms.GroupBox();
            this.cmboBoxFiatCurrency = new System.Windows.Forms.ComboBox();
            this.label3 = new System.Windows.Forms.Label();
            this.lblSelectedCoin = new System.Windows.Forms.Label();
            this.label2 = new System.Windows.Forms.Label();
            this.imgCurrentCoin = new System.Windows.Forms.PictureBox();
            this.imgBoxDefaultCoin = new System.Windows.Forms.PictureBox();
            this.btnPrivKey = new System.Windows.Forms.Button();
            this.cbAutoUpdate = new System.Windows.Forms.CheckBox();
            this.SelectFolderDialog = new System.Windows.Forms.FolderBrowserDialog();
            this.lblAltPassSet = new System.Windows.Forms.Label();
            this.btnSetAlternatePassword = new System.Windows.Forms.Button();
            this.btnChangeMasterPassword = new System.Windows.Forms.Button();
            this.checkEncryptWallet = new System.Windows.Forms.CheckBox();
            this.groupBox1 = new System.Windows.Forms.GroupBox();
            this.groupBox2.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.imgCurrentCoin)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.imgBoxDefaultCoin)).BeginInit();
            this.groupBox1.SuspendLayout();
            this.SuspendLayout();
            // 
            // btnCancel
            // 
            this.btnCancel.BackColor = System.Drawing.Color.Transparent;
            this.btnCancel.Location = new System.Drawing.Point(351, 426);
            this.btnCancel.UseVisualStyleBackColor = false;
            // 
            // btnOK
            // 
            this.btnOK.BackColor = System.Drawing.Color.Transparent;
            this.btnOK.Location = new System.Drawing.Point(269, 426);
            this.btnOK.UseVisualStyleBackColor = false;
            // 
            // txtDataPath
            // 
            this.txtDataPath.Location = new System.Drawing.Point(90, 80);
            this.txtDataPath.Name = "txtDataPath";
            this.txtDataPath.Size = new System.Drawing.Size(228, 20);
            this.txtDataPath.TabIndex = 11;
            // 
            // lblDataPath
            // 
            this.lblDataPath.AutoSize = true;
            this.lblDataPath.Location = new System.Drawing.Point(16, 83);
            this.lblDataPath.Name = "lblDataPath";
            this.lblDataPath.Size = new System.Drawing.Size(58, 13);
            this.lblDataPath.TabIndex = 13;
            this.lblDataPath.Text = "Data Path:";
            // 
            // label1
            // 
            this.label1.AutoSize = true;
            this.label1.Location = new System.Drawing.Point(16, 34);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(68, 13);
            this.label1.TabIndex = 16;
            this.label1.Text = "Default Coin:";
            // 
            // lblDefaultCoin
            // 
            this.lblDefaultCoin.AutoSize = true;
            this.lblDefaultCoin.Font = new System.Drawing.Font("Calibri", 10F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.lblDefaultCoin.Location = new System.Drawing.Point(136, 30);
            this.lblDefaultCoin.Name = "lblDefaultCoin";
            this.lblDefaultCoin.Size = new System.Drawing.Size(118, 17);
            this.lblDefaultCoin.TabIndex = 17;
            this.lblDefaultCoin.Text = "No Default Chosen\r\n";
            // 
            // btnChangeDefaultCoin
            // 
            this.btnChangeDefaultCoin.BackColor = System.Drawing.Color.Transparent;
            this.btnChangeDefaultCoin.Location = new System.Drawing.Point(325, 29);
            this.btnChangeDefaultCoin.Name = "btnChangeDefaultCoin";
            this.btnChangeDefaultCoin.Size = new System.Drawing.Size(65, 23);
            this.btnChangeDefaultCoin.TabIndex = 18;
            this.btnChangeDefaultCoin.Text = "Change";
            this.btnChangeDefaultCoin.UseVisualStyleBackColor = true;
            this.btnChangeDefaultCoin.Click += new System.EventHandler(this.btnChangeDefaultCoin_Click);
            // 
            // BrowseButton
            // 
            this.BrowseButton.BackColor = System.Drawing.Color.Transparent;
            this.BrowseButton.Location = new System.Drawing.Point(325, 78);
            this.BrowseButton.Name = "BrowseButton";
            this.BrowseButton.Size = new System.Drawing.Size(65, 23);
            this.BrowseButton.TabIndex = 20;
            this.BrowseButton.Text = "Browse";
            this.BrowseButton.UseVisualStyleBackColor = true;
            this.BrowseButton.Click += new System.EventHandler(this.BrowseButton_Click);
            // 
            // groupBox2
            // 
            this.groupBox2.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.groupBox2.Controls.Add(this.cmboBoxFiatCurrency);
            this.groupBox2.Controls.Add(this.label3);
            this.groupBox2.Controls.Add(this.lblSelectedCoin);
            this.groupBox2.Controls.Add(this.label2);
            this.groupBox2.Controls.Add(this.imgCurrentCoin);
            this.groupBox2.Controls.Add(this.imgBoxDefaultCoin);
            this.groupBox2.Controls.Add(this.btnPrivKey);
            this.groupBox2.Controls.Add(this.txtDataPath);
            this.groupBox2.Controls.Add(this.lblDataPath);
            this.groupBox2.Controls.Add(this.BrowseButton);
            this.groupBox2.Controls.Add(this.btnChangeDefaultCoin);
            this.groupBox2.Controls.Add(this.lblDefaultCoin);
            this.groupBox2.Controls.Add(this.label1);
            this.groupBox2.Location = new System.Drawing.Point(16, 12);
            this.groupBox2.Name = "groupBox2";
            this.groupBox2.Size = new System.Drawing.Size(410, 234);
            this.groupBox2.TabIndex = 22;
            this.groupBox2.TabStop = false;
            this.groupBox2.Text = "Wallet";
            // 
            // cmboBoxFiatCurrency
            // 
            this.cmboBoxFiatCurrency.FormattingEnabled = true;
            this.cmboBoxFiatCurrency.Location = new System.Drawing.Point(142, 195);
            this.cmboBoxFiatCurrency.Name = "cmboBoxFiatCurrency";
            this.cmboBoxFiatCurrency.Size = new System.Drawing.Size(186, 21);
            this.cmboBoxFiatCurrency.TabIndex = 29;
            // 
            // label3
            // 
            this.label3.AutoSize = true;
            this.label3.Location = new System.Drawing.Point(19, 198);
            this.label3.Name = "label3";
            this.label3.Size = new System.Drawing.Size(117, 13);
            this.label3.TabIndex = 28;
            this.label3.Text = "Selected Fiat Currency:";
            // 
            // lblSelectedCoin
            // 
            this.lblSelectedCoin.AutoSize = true;
            this.lblSelectedCoin.Location = new System.Drawing.Point(68, 152);
            this.lblSelectedCoin.Name = "lblSelectedCoin";
            this.lblSelectedCoin.Size = new System.Drawing.Size(49, 13);
            this.lblSelectedCoin.TabIndex = 26;
            this.lblSelectedCoin.Text = "%COIN%";
            this.lblSelectedCoin.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
            // 
            // label2
            // 
            this.label2.AutoSize = true;
            this.label2.Location = new System.Drawing.Point(19, 117);
            this.label2.Name = "label2";
            this.label2.Size = new System.Drawing.Size(110, 13);
            this.label2.TabIndex = 25;
            this.label2.Text = "Current selected coin:";
            // 
            // imgCurrentCoin
            // 
            this.imgCurrentCoin.BackgroundImageLayout = System.Windows.Forms.ImageLayout.Center;
            this.imgCurrentCoin.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
            this.imgCurrentCoin.Location = new System.Drawing.Point(22, 142);
            this.imgCurrentCoin.Name = "imgCurrentCoin";
            this.imgCurrentCoin.Size = new System.Drawing.Size(40, 40);
            this.imgCurrentCoin.TabIndex = 24;
            this.imgCurrentCoin.TabStop = false;
            // 
            // imgBoxDefaultCoin
            // 
            this.imgBoxDefaultCoin.BackgroundImageLayout = System.Windows.Forms.ImageLayout.Center;
            this.imgBoxDefaultCoin.Location = new System.Drawing.Point(90, 19);
            this.imgBoxDefaultCoin.Name = "imgBoxDefaultCoin";
            this.imgBoxDefaultCoin.Size = new System.Drawing.Size(40, 40);
            this.imgBoxDefaultCoin.TabIndex = 22;
            this.imgBoxDefaultCoin.TabStop = false;
            // 
            // btnPrivKey
            // 
            this.btnPrivKey.AutoSize = true;
            this.btnPrivKey.Location = new System.Drawing.Point(202, 146);
            this.btnPrivKey.MaximumSize = new System.Drawing.Size(210, 0);
            this.btnPrivKey.Name = "btnPrivKey";
            this.btnPrivKey.Size = new System.Drawing.Size(186, 23);
            this.btnPrivKey.TabIndex = 23;
            this.btnPrivKey.Text = "View %COIN% private key...";
            this.btnPrivKey.UseVisualStyleBackColor = true;
            this.btnPrivKey.Click += new System.EventHandler(this.btnPrivKey_Click);
            // 
            // cbAutoUpdate
            // 
            this.cbAutoUpdate.AutoSize = true;
            this.cbAutoUpdate.Location = new System.Drawing.Point(16, 426);
            this.cbAutoUpdate.Name = "cbAutoUpdate";
            this.cbAutoUpdate.Size = new System.Drawing.Size(119, 17);
            this.cbAutoUpdate.TabIndex = 27;
            this.cbAutoUpdate.Text = "Enable AutoUpdate";
            this.cbAutoUpdate.UseVisualStyleBackColor = true;
            // 
            // lblAltPassSet
            // 
            this.lblAltPassSet.AutoSize = true;
            this.lblAltPassSet.Location = new System.Drawing.Point(98, 118);
            this.lblAltPassSet.Name = "lblAltPassSet";
            this.lblAltPassSet.Size = new System.Drawing.Size(220, 13);
            this.lblAltPassSet.TabIndex = 34;
            this.lblAltPassSet.Text = "You currently have an alternate password set";
            this.lblAltPassSet.Visible = false;
            // 
            // btnSetAlternatePassword
            // 
            this.btnSetAlternatePassword.Location = new System.Drawing.Point(77, 92);
            this.btnSetAlternatePassword.Name = "btnSetAlternatePassword";
            this.btnSetAlternatePassword.Size = new System.Drawing.Size(251, 23);
            this.btnSetAlternatePassword.TabIndex = 33;
            this.btnSetAlternatePassword.Text = "Set alternate password";
            this.btnSetAlternatePassword.UseVisualStyleBackColor = true;
            this.btnSetAlternatePassword.Click += new System.EventHandler(this.btnSetAlternatePassword_Click);
            // 
            // btnChangeMasterPassword
            // 
            this.btnChangeMasterPassword.Location = new System.Drawing.Point(77, 60);
            this.btnChangeMasterPassword.Name = "btnChangeMasterPassword";
            this.btnChangeMasterPassword.Size = new System.Drawing.Size(251, 23);
            this.btnChangeMasterPassword.TabIndex = 32;
            this.btnChangeMasterPassword.Text = "Change master password";
            this.btnChangeMasterPassword.UseVisualStyleBackColor = true;
            this.btnChangeMasterPassword.Click += new System.EventHandler(this.btnChangeMasterPassword_Click);
            // 
            // checkEncryptWallet
            // 
            this.checkEncryptWallet.AutoSize = true;
            this.checkEncryptWallet.Checked = true;
            this.checkEncryptWallet.CheckState = System.Windows.Forms.CheckState.Checked;
            this.checkEncryptWallet.Location = new System.Drawing.Point(19, 28);
            this.checkEncryptWallet.Name = "checkEncryptWallet";
            this.checkEncryptWallet.Size = new System.Drawing.Size(215, 17);
            this.checkEncryptWallet.TabIndex = 31;
            this.checkEncryptWallet.Text = "Ask for wallet password (Encrypt Wallet)";
            this.checkEncryptWallet.UseVisualStyleBackColor = true;
            this.checkEncryptWallet.CheckedChanged += new System.EventHandler(this.checkEncryptWallet_CheckedChanged);
            // 
            // groupBox1
            // 
            this.groupBox1.Controls.Add(this.btnSetAlternatePassword);
            this.groupBox1.Controls.Add(this.lblAltPassSet);
            this.groupBox1.Controls.Add(this.checkEncryptWallet);
            this.groupBox1.Controls.Add(this.btnChangeMasterPassword);
            this.groupBox1.Location = new System.Drawing.Point(16, 253);
            this.groupBox1.Name = "groupBox1";
            this.groupBox1.Size = new System.Drawing.Size(410, 151);
            this.groupBox1.TabIndex = 35;
            this.groupBox1.TabStop = false;
            this.groupBox1.Text = "Encryption";
            // 
            // SettingsDialog
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.BackColor = System.Drawing.SystemColors.InactiveCaption;
            this.ClientSize = new System.Drawing.Size(438, 461);
            this.Controls.Add(this.groupBox1);
            this.Controls.Add(this.groupBox2);
            this.Controls.Add(this.cbAutoUpdate);
            this.Icon = ((System.Drawing.Icon)(resources.GetObject("$this.Icon")));
            this.MaximumSize = new System.Drawing.Size(500, 500);
            this.MinimumSize = new System.Drawing.Size(444, 232);
            this.Name = "SettingsDialog";
            this.StartPosition = System.Windows.Forms.FormStartPosition.CenterScreen;
            this.Text = "Pandora\'s Wallet Settings";
            this.FormClosing += new System.Windows.Forms.FormClosingEventHandler(this.SettingsDialogDummy_FormClosing);
            this.Controls.SetChildIndex(this.cbAutoUpdate, 0);
            this.Controls.SetChildIndex(this.btnOK, 0);
            this.Controls.SetChildIndex(this.btnCancel, 0);
            this.Controls.SetChildIndex(this.groupBox2, 0);
            this.Controls.SetChildIndex(this.groupBox1, 0);
            this.groupBox2.ResumeLayout(false);
            this.groupBox2.PerformLayout();
            ((System.ComponentModel.ISupportInitialize)(this.imgCurrentCoin)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.imgBoxDefaultCoin)).EndInit();
            this.groupBox1.ResumeLayout(false);
            this.groupBox1.PerformLayout();
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion
        private System.Windows.Forms.TextBox txtDataPath;
        private System.Windows.Forms.Label lblDataPath;
        private System.Windows.Forms.Label label1;
        private System.Windows.Forms.Label lblDefaultCoin;
        private System.Windows.Forms.Button btnChangeDefaultCoin;
        private System.Windows.Forms.Button BrowseButton;
        private System.Windows.Forms.GroupBox groupBox2;
        private System.Windows.Forms.FolderBrowserDialog SelectFolderDialog;
        private System.Windows.Forms.PictureBox imgBoxDefaultCoin;
        private System.Windows.Forms.Button btnPrivKey;
        private System.Windows.Forms.Label label2;
        private System.Windows.Forms.PictureBox imgCurrentCoin;
        private System.Windows.Forms.Label lblSelectedCoin;
        private System.Windows.Forms.CheckBox cbAutoUpdate;
        private System.Windows.Forms.ComboBox cmboBoxFiatCurrency;
        private System.Windows.Forms.Label label3;
        private System.Windows.Forms.Label lblAltPassSet;
        private System.Windows.Forms.Button btnSetAlternatePassword;
        private System.Windows.Forms.Button btnChangeMasterPassword;
        private System.Windows.Forms.CheckBox checkEncryptWallet;
        private System.Windows.Forms.GroupBox groupBox1;
    }
}
