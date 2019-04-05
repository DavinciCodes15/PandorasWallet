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
            this.lbl1 = new System.Windows.Forms.Label();
            this.lblSelectedCoin = new System.Windows.Forms.Label();
            this.SuspendLayout();
            // 
            // btnCancel
            // 
            this.btnCancel.Location = new System.Drawing.Point(239, 61);
            this.btnCancel.Visible = false;
            // 
            // btnOK
            // 
            this.btnOK.Location = new System.Drawing.Point(121, 61);
            this.btnOK.Click += new System.EventHandler(this.btnOK_Click);
            // 
            // txtBoxPrivkey
            // 
            this.txtBoxPrivkey.Location = new System.Drawing.Point(12, 25);
            this.txtBoxPrivkey.Name = "txtBoxPrivkey";
            this.txtBoxPrivkey.ReadOnly = true;
            this.txtBoxPrivkey.Size = new System.Drawing.Size(302, 20);
            this.txtBoxPrivkey.TabIndex = 7;
            // 
            // lbl1
            // 
            this.lbl1.AutoSize = true;
            this.lbl1.Location = new System.Drawing.Point(12, 9);
            this.lbl1.Name = "lbl1";
            this.lbl1.Size = new System.Drawing.Size(75, 13);
            this.lbl1.TabIndex = 8;
            this.lbl1.Text = "Private key for";
            // 
            // lblSelectedCoin
            // 
            this.lblSelectedCoin.AutoSize = true;
            this.lblSelectedCoin.Location = new System.Drawing.Point(84, 9);
            this.lblSelectedCoin.Name = "lblSelectedCoin";
            this.lblSelectedCoin.Size = new System.Drawing.Size(28, 13);
            this.lblSelectedCoin.TabIndex = 9;
            this.lblSelectedCoin.Text = "Coin";
            this.lblSelectedCoin.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
            // 
            // PrivKeyDialog
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(326, 96);
            this.Controls.Add(this.lblSelectedCoin);
            this.Controls.Add(this.lbl1);
            this.Controls.Add(this.txtBoxPrivkey);
            this.Name = "PrivKeyDialog";
            this.Text = "Coin Private Key";
            this.Controls.SetChildIndex(this.btnOK, 0);
            this.Controls.SetChildIndex(this.btnCancel, 0);
            this.Controls.SetChildIndex(this.txtBoxPrivkey, 0);
            this.Controls.SetChildIndex(this.lbl1, 0);
            this.Controls.SetChildIndex(this.lblSelectedCoin, 0);
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.TextBox txtBoxPrivkey;
        private System.Windows.Forms.Label lbl1;
        private System.Windows.Forms.Label lblSelectedCoin;
    }
}