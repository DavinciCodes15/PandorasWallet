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
            this.btnPrivKey = new System.Windows.Forms.Button();
            this.imgBoxDefaultCoin = new System.Windows.Forms.PictureBox();
            this.checkEncryptWallet = new System.Windows.Forms.CheckBox();
            this.SelectFolderDialog = new System.Windows.Forms.FolderBrowserDialog();
            this.groupBox2.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.imgBoxDefaultCoin)).BeginInit();
            this.SuspendLayout();
            // 
            // btnCancel
            // 
            this.btnCancel.BackColor = System.Drawing.Color.Transparent;
            this.btnCancel.Location = new System.Drawing.Point(341, 158);
            this.btnCancel.UseVisualStyleBackColor = false;
            this.btnCancel.Click += new System.EventHandler(this.btnCancel_Click);
            // 
            // btnOK
            // 
            this.btnOK.BackColor = System.Drawing.Color.Transparent;
            this.btnOK.Location = new System.Drawing.Point(259, 158);
            this.btnOK.UseVisualStyleBackColor = false;
            this.btnOK.Click += new System.EventHandler(this.btnOK_Click);
            // 
            // txtDataPath
            // 
            this.txtDataPath.Location = new System.Drawing.Point(90, 80);
            this.txtDataPath.Name = "txtDataPath";
            this.txtDataPath.Size = new System.Drawing.Size(228, 20);
            this.txtDataPath.TabIndex = 11;
            this.txtDataPath.Leave += new System.EventHandler(this.txtDataPath_Leave);
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
            this.lblDefaultCoin.Location = new System.Drawing.Point(136, 34);
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
            this.groupBox2.Controls.Add(this.btnPrivKey);
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
            // btnPrivKey
            // 
            this.btnPrivKey.Location = new System.Drawing.Point(325, 109);
            this.btnPrivKey.Name = "btnPrivKey";
            this.btnPrivKey.Size = new System.Drawing.Size(65, 23);
            this.btnPrivKey.TabIndex = 23;
            this.btnPrivKey.Text = "Priv. Key";
            this.btnPrivKey.UseVisualStyleBackColor = true;
            this.btnPrivKey.Click += new System.EventHandler(this.btnPrivKey_Click);
            // 
            // imgBoxDefaultCoin
            // 
            this.imgBoxDefaultCoin.Location = new System.Drawing.Point(90, 19);
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
            this.checkEncryptWallet.Location = new System.Drawing.Point(19, 113);
            this.checkEncryptWallet.Name = "checkEncryptWallet";
            this.checkEncryptWallet.Size = new System.Drawing.Size(215, 17);
            this.checkEncryptWallet.TabIndex = 21;
            this.checkEncryptWallet.Text = "Ask for wallet password (Encrypt Wallet)";
            this.checkEncryptWallet.UseVisualStyleBackColor = true;
            this.checkEncryptWallet.CheckedChanged += new System.EventHandler(this.checkEncryptWallet_CheckedChanged);
            // 
            // SettingsDialog
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.BackColor = System.Drawing.SystemColors.InactiveCaption;
            this.ClientSize = new System.Drawing.Size(428, 193);
            this.Controls.Add(this.groupBox2);
            this.Icon = ((System.Drawing.Icon)(resources.GetObject("$this.Icon")));
            this.MaximumSize = new System.Drawing.Size(500, 500);
            this.MinimumSize = new System.Drawing.Size(444, 232);
            this.Name = "SettingsDialog";
            this.StartPosition = System.Windows.Forms.FormStartPosition.CenterScreen;
            this.Text = "Pandora\'s Wallet Settings";
            this.FormClosing += new System.Windows.Forms.FormClosingEventHandler(this.SettingsDialogDummy_FormClosing);
            this.Shown += new System.EventHandler(this.SettingsDialogDummy_Shown);
            this.Controls.SetChildIndex(this.btnOK, 0);
            this.Controls.SetChildIndex(this.btnCancel, 0);
            this.Controls.SetChildIndex(this.groupBox2, 0);
            this.groupBox2.ResumeLayout(false);
            this.groupBox2.PerformLayout();
            ((System.ComponentModel.ISupportInitialize)(this.imgBoxDefaultCoin)).EndInit();
            this.ResumeLayout(false);

        }

        #endregion
        private System.Windows.Forms.TextBox txtDataPath;
        private System.Windows.Forms.Label lblDataPath;
        private System.Windows.Forms.Label label1;
        private System.Windows.Forms.Label lblDefaultCoin;
        private System.Windows.Forms.Button btnChangeDefaultCoin;
        private System.Windows.Forms.Button BrowseButton;
        private System.Windows.Forms.GroupBox groupBox2;
        private System.Windows.Forms.CheckBox checkEncryptWallet;
        private System.Windows.Forms.FolderBrowserDialog SelectFolderDialog;
        private System.Windows.Forms.PictureBox imgBoxDefaultCoin;
        private System.Windows.Forms.Button btnPrivKey;
    }
}
