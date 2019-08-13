namespace Pandora.Client.PandorasWallet.Dialogs
{
    partial class PasswordDialog
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
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(PasswordDialog));
            this.controlPanel = new System.Windows.Forms.Panel();
            this.lockPictureBock = new System.Windows.Forms.PictureBox();
            this.secondLabel = new System.Windows.Forms.Label();
            this.firstLabel = new System.Windows.Forms.Label();
            this.WarningLabel = new System.Windows.Forms.Label();
            this.passwordBoxCheck = new System.Windows.Forms.TextBox();
            this.passwordBox = new System.Windows.Forms.TextBox();
            this.unlockPictureBox = new System.Windows.Forms.PictureBox();
            this.checkBox1 = new System.Windows.Forms.CheckBox();
            this.controlPanel.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.lockPictureBock)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.unlockPictureBox)).BeginInit();
            this.SuspendLayout();
            // 
            // btnCancel
            // 
            this.btnCancel.Location = new System.Drawing.Point(238, 174);
            this.btnCancel.Click += new System.EventHandler(this.btnCancel_Click);
            // 
            // btnOK
            // 
            this.btnOK.DialogResult = System.Windows.Forms.DialogResult.None;
            this.btnOK.Location = new System.Drawing.Point(157, 174);
            this.btnOK.Click += new System.EventHandler(this.btnOK_Click);
            // 
            // controlPanel
            // 
            this.controlPanel.AutoSizeMode = System.Windows.Forms.AutoSizeMode.GrowAndShrink;
            this.controlPanel.Controls.Add(this.checkBox1);
            this.controlPanel.Controls.Add(this.lockPictureBock);
            this.controlPanel.Controls.Add(this.secondLabel);
            this.controlPanel.Controls.Add(this.firstLabel);
            this.controlPanel.Controls.Add(this.WarningLabel);
            this.controlPanel.Controls.Add(this.passwordBoxCheck);
            this.controlPanel.Controls.Add(this.passwordBox);
            this.controlPanel.Location = new System.Drawing.Point(12, 12);
            this.controlPanel.Name = "controlPanel";
            this.controlPanel.Size = new System.Drawing.Size(298, 147);
            this.controlPanel.TabIndex = 11;
            // 
            // lockPictureBock
            // 
            this.lockPictureBock.Location = new System.Drawing.Point(116, 42);
            this.lockPictureBock.Name = "lockPictureBock";
            this.lockPictureBock.Size = new System.Drawing.Size(56, 56);
            this.lockPictureBock.SizeMode = System.Windows.Forms.PictureBoxSizeMode.StretchImage;
            this.lockPictureBock.TabIndex = 16;
            this.lockPictureBock.TabStop = false;
            // 
            // secondLabel
            // 
            this.secondLabel.AutoSize = true;
            this.secondLabel.Location = new System.Drawing.Point(3, 48);
            this.secondLabel.Name = "secondLabel";
            this.secondLabel.Size = new System.Drawing.Size(93, 13);
            this.secondLabel.TabIndex = 3;
            this.secondLabel.Text = "Repeat password:";
            // 
            // firstLabel
            // 
            this.firstLabel.AutoSize = true;
            this.firstLabel.Location = new System.Drawing.Point(3, 0);
            this.firstLabel.Name = "firstLabel";
            this.firstLabel.Size = new System.Drawing.Size(190, 13);
            this.firstLabel.TabIndex = 1;
            this.firstLabel.Text = "Insert password used to decrypt wallet:";
            // 
            // WarningLabel
            // 
            this.WarningLabel.Font = new System.Drawing.Font("Microsoft Sans Serif", 8F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.WarningLabel.Location = new System.Drawing.Point(6, 110);
            this.WarningLabel.Name = "WarningLabel";
            this.WarningLabel.Size = new System.Drawing.Size(289, 28);
            this.WarningLabel.TabIndex = 13;
            this.WarningLabel.Text = "WARNING";
            this.WarningLabel.TextAlign = System.Drawing.ContentAlignment.TopCenter;
            // 
            // passwordBoxCheck
            // 
            this.passwordBoxCheck.Location = new System.Drawing.Point(6, 64);
            this.passwordBoxCheck.Name = "passwordBoxCheck";
            this.passwordBoxCheck.PasswordChar = '•';
            this.passwordBoxCheck.Size = new System.Drawing.Size(289, 20);
            this.passwordBoxCheck.TabIndex = 4;
            // 
            // passwordBox
            // 
            this.passwordBox.Location = new System.Drawing.Point(6, 16);
            this.passwordBox.Name = "passwordBox";
            this.passwordBox.PasswordChar = '•';
            this.passwordBox.Size = new System.Drawing.Size(289, 20);
            this.passwordBox.TabIndex = 2;
            // 
            // unlockPictureBox
            // 
            this.unlockPictureBox.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));
            this.unlockPictureBox.InitialImage = global::Pandora.Client.PandorasWallet.Properties.Resources.unlocked;
            this.unlockPictureBox.Location = new System.Drawing.Point(18, 164);
            this.unlockPictureBox.Name = "unlockPictureBox";
            this.unlockPictureBox.Size = new System.Drawing.Size(33, 33);
            this.unlockPictureBox.SizeMode = System.Windows.Forms.PictureBoxSizeMode.StretchImage;
            this.unlockPictureBox.TabIndex = 12;
            this.unlockPictureBox.TabStop = false;
            // 
            // checkBox1
            // 
            this.checkBox1.AutoSize = true;
            this.checkBox1.Location = new System.Drawing.Point(6, 90);
            this.checkBox1.Name = "checkBox1";
            this.checkBox1.Size = new System.Drawing.Size(100, 17);
            this.checkBox1.TabIndex = 5;
            this.checkBox1.Text = "Save Password";
            this.checkBox1.UseVisualStyleBackColor = true;
            this.checkBox1.Visible = false;
            // 
            // PasswordDialog
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoSize = true;
            this.AutoSizeMode = System.Windows.Forms.AutoSizeMode.GrowOnly;
            this.ClientSize = new System.Drawing.Size(325, 209);
            this.Controls.Add(this.unlockPictureBox);
            this.Controls.Add(this.controlPanel);
            this.Icon = ((System.Drawing.Icon)(resources.GetObject("$this.Icon")));
            this.Name = "PasswordDialog";
            this.StartPosition = System.Windows.Forms.FormStartPosition.CenterScreen;
            this.Text = "Wallet Password";
            this.FormClosing += new System.Windows.Forms.FormClosingEventHandler(this.WalletPasswordBox_FormClosing);
            this.Shown += new System.EventHandler(this.WalletPasswordBox_Shown);
            this.Controls.SetChildIndex(this.controlPanel, 0);
            this.Controls.SetChildIndex(this.btnOK, 0);
            this.Controls.SetChildIndex(this.btnCancel, 0);
            this.Controls.SetChildIndex(this.unlockPictureBox, 0);
            this.controlPanel.ResumeLayout(false);
            this.controlPanel.PerformLayout();
            ((System.ComponentModel.ISupportInitialize)(this.lockPictureBock)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.unlockPictureBox)).EndInit();
            this.ResumeLayout(false);

        }

        #endregion

        private System.Windows.Forms.Panel controlPanel;
        private System.Windows.Forms.Label secondLabel;
        private System.Windows.Forms.Label firstLabel;
        private System.Windows.Forms.Label WarningLabel;
        private System.Windows.Forms.TextBox passwordBoxCheck;
        private System.Windows.Forms.TextBox passwordBox;
        private System.Windows.Forms.PictureBox lockPictureBock;
        private System.Windows.Forms.PictureBox unlockPictureBox;
        private System.Windows.Forms.CheckBox checkBox1;
    }
}
