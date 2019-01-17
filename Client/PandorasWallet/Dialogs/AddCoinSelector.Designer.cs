namespace Pandora.Client.PandorasWallet.Dialogs
{
    partial class AddCoinSelector
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
            this.lstViewAddCurrency = new Pandora.Client.PandorasWallet.CurrencyView();
            this.SuspendLayout();
            // 
            // btnCancel
            // 
            this.btnCancel.Location = new System.Drawing.Point(182, 290);
            this.btnCancel.Click += new System.EventHandler(this.btnCancel_Click);
            // 
            // btnOK
            // 
            this.btnOK.Location = new System.Drawing.Point(100, 290);
            this.btnOK.Click += new System.EventHandler(this.btnOK_Click);
            // 
            // lstViewAddCurrency
            // 
            this.lstViewAddCurrency.CurrencyIds = new long[0];
            this.lstViewAddCurrency.Location = new System.Drawing.Point(13, 13);
            this.lstViewAddCurrency.Name = "lstViewAddCurrency";
            this.lstViewAddCurrency.Size = new System.Drawing.Size(300, 271);
            this.lstViewAddCurrency.TabIndex = 9;
            // 
            // AddCoinSelectorDummy
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.BackColor = System.Drawing.SystemColors.InactiveCaption;
            this.ClientSize = new System.Drawing.Size(325, 325);
            this.Controls.Add(this.lstViewAddCurrency);
            this.Name = "AddCoinSelectorDummy";
            this.Text = "Add Coin(s)";
            this.FormClosing += new System.Windows.Forms.FormClosingEventHandler(this.AddCoinSelector_FormClosing);
            this.Shown += new System.EventHandler(this.AddCoinSelectorDummy_Shown);
            this.Controls.SetChildIndex(this.btnOK, 0);
            this.Controls.SetChildIndex(this.btnCancel, 0);
            this.Controls.SetChildIndex(this.lstViewAddCurrency, 0);
            this.ResumeLayout(false);

        }

        #endregion
        private CurrencyView lstViewAddCurrency;
    }
}
