namespace Pandora.Client.PandorasWallet.Dialogs
{
    partial class SendingTxDialog
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
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(SendingTxDialog));
            this.StatusLabel = new System.Windows.Forms.Label();
            this.TxId = new System.Windows.Forms.TextBox();
            this.TxIdLabel = new System.Windows.Forms.Label();
            this.StatusPictureBox = new System.Windows.Forms.PictureBox();
            ((System.ComponentModel.ISupportInitialize)(this.StatusPictureBox)).BeginInit();
            this.SuspendLayout();
            // 
            // btnCancel
            // 
            this.btnCancel.Location = new System.Drawing.Point(324, 261);
            this.btnCancel.Visible = false;
            // 
            // btnOK
            // 
            this.btnOK.Location = new System.Drawing.Point(158, 251);
            // 
            // StatusLabel
            // 
            this.StatusLabel.Location = new System.Drawing.Point(28, 170);
            this.StatusLabel.Name = "StatusLabel";
            this.StatusLabel.Size = new System.Drawing.Size(348, 28);
            this.StatusLabel.TabIndex = 7;
            this.StatusLabel.Text = "StatusLabel";
            // 
            // TxId
            // 
            this.TxId.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));
            this.TxId.Location = new System.Drawing.Point(31, 214);
            this.TxId.Name = "TxId";
            this.TxId.ReadOnly = true;
            this.TxId.Size = new System.Drawing.Size(345, 20);
            this.TxId.TabIndex = 8;
            this.TxId.Visible = false;
            // 
            // TxIdLabel
            // 
            this.TxIdLabel.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));
            this.TxIdLabel.AutoSize = true;
            this.TxIdLabel.Location = new System.Drawing.Point(28, 198);
            this.TxIdLabel.Name = "TxIdLabel";
            this.TxIdLabel.Size = new System.Drawing.Size(80, 13);
            this.TxIdLabel.TabIndex = 10;
            this.TxIdLabel.Text = "Transaction ID:";
            // 
            // StatusPictureBox
            // 
            this.StatusPictureBox.InitialImage = ((System.Drawing.Image)(resources.GetObject("StatusPictureBox.InitialImage")));
            this.StatusPictureBox.Location = new System.Drawing.Point(137, 26);
            this.StatusPictureBox.Name = "StatusPictureBox";
            this.StatusPictureBox.Size = new System.Drawing.Size(128, 128);
            this.StatusPictureBox.SizeMode = System.Windows.Forms.PictureBoxSizeMode.StretchImage;
            this.StatusPictureBox.TabIndex = 9;
            this.StatusPictureBox.TabStop = false;
            // 
            // SendingTxDialog
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.ClientSize = new System.Drawing.Size(411, 296);
            this.Controls.Add(this.TxIdLabel);
            this.Controls.Add(this.StatusPictureBox);
            this.Controls.Add(this.TxId);
            this.Controls.Add(this.StatusLabel);
            this.Name = "SendingTxDialog";
            this.Text = "";
            this.Shown += new System.EventHandler(this.SendingTxDialog_Shown);
            this.Controls.SetChildIndex(this.StatusLabel, 0);
            this.Controls.SetChildIndex(this.TxId, 0);
            this.Controls.SetChildIndex(this.btnOK, 0);
            this.Controls.SetChildIndex(this.btnCancel, 0);
            this.Controls.SetChildIndex(this.StatusPictureBox, 0);
            this.Controls.SetChildIndex(this.TxIdLabel, 0);
            ((System.ComponentModel.ISupportInitialize)(this.StatusPictureBox)).EndInit();
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.Label StatusLabel;
        private System.Windows.Forms.TextBox TxId;
        private System.Windows.Forms.PictureBox StatusPictureBox;
        private System.Windows.Forms.Label TxIdLabel;
    }
}
