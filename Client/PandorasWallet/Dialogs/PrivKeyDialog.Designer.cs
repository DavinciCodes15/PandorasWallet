namespace Pandora.Client.PandorasWallet.Dialogs
{
    partial class PrivKeyDialog
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
            this.txtBoxPrivkey = new System.Windows.Forms.TextBox();
            this.lblTitle = new System.Windows.Forms.Label();
            this.SuspendLayout();
            // 
            // btnCancel
            // 
            this.btnCancel.Location = new System.Drawing.Point(437, 270);
            this.btnCancel.Visible = false;
            // 
            // btnOK
            // 
            this.btnOK.Location = new System.Drawing.Point(219, 270);
            this.btnOK.Click += new System.EventHandler(this.btnOK_Click);
            // 
            // txtBoxPrivkey
            // 
            this.txtBoxPrivkey.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.txtBoxPrivkey.Location = new System.Drawing.Point(14, 31);
            this.txtBoxPrivkey.Multiline = true;
            this.txtBoxPrivkey.Name = "txtBoxPrivkey";
            this.txtBoxPrivkey.ReadOnly = true;
            this.txtBoxPrivkey.Size = new System.Drawing.Size(498, 224);
            this.txtBoxPrivkey.TabIndex = 7;
            // 
            // lblTitle
            // 
            this.lblTitle.AutoSize = true;
            this.lblTitle.Location = new System.Drawing.Point(12, 9);
            this.lblTitle.Name = "lblTitle";
            this.lblTitle.Size = new System.Drawing.Size(128, 13);
            this.lblTitle.TabIndex = 8;
            this.lblTitle.Text = "Private keys for %COIN%:";
            // 
            // PrivKeyDialog
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(524, 305);
            this.Controls.Add(this.lblTitle);
            this.Controls.Add(this.txtBoxPrivkey);
            this.Name = "PrivKeyDialog";
            this.Text = "Coin Private Key";
            this.Controls.SetChildIndex(this.btnOK, 0);
            this.Controls.SetChildIndex(this.btnCancel, 0);
            this.Controls.SetChildIndex(this.txtBoxPrivkey, 0);
            this.Controls.SetChildIndex(this.lblTitle, 0);
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.TextBox txtBoxPrivkey;
        private System.Windows.Forms.Label lblTitle;
    }
}