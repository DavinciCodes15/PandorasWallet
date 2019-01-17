namespace Pandora.Client.PandorasWallet.Dialogs
{
    partial class DefaultCoinSelector
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
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(DefaultCoinSelector));
            this.label1 = new System.Windows.Forms.Label();
            this.picCoinImage = new System.Windows.Forms.PictureBox();
            this.lblCoinName = new System.Windows.Forms.Label();
            this.TickerLabel = new System.Windows.Forms.Label();
            this.openFileDialog1 = new System.Windows.Forms.OpenFileDialog();
            this.lstViewDefaultCoin = new Pandora.Client.PandorasWallet.CurrencyView();
            ((System.ComponentModel.ISupportInitialize)(this.picCoinImage)).BeginInit();
            this.SuspendLayout();
            // 
            // btnCancel
            // 
            this.btnCancel.Location = new System.Drawing.Point(144, 305);
            this.btnCancel.Click += new System.EventHandler(this.btnCancel_Click);
            // 
            // btnOK
            // 
            this.btnOK.Location = new System.Drawing.Point(44, 305);
            this.btnOK.Click += new System.EventHandler(this.btnOK_Click);
            // 
            // label1
            // 
            this.label1.AutoSize = true;
            this.label1.Location = new System.Drawing.Point(20, 20);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(140, 13);
            this.label1.TabIndex = 8;
            this.label1.Text = "Please select a default coin:";
            // 
            // picCoinImage
            // 
            this.picCoinImage.Location = new System.Drawing.Point(32, 46);
            this.picCoinImage.Name = "picCoinImage";
            this.picCoinImage.Size = new System.Drawing.Size(40, 40);
            this.picCoinImage.SizeMode = System.Windows.Forms.PictureBoxSizeMode.StretchImage;
            this.picCoinImage.TabIndex = 19;
            this.picCoinImage.TabStop = false;
            // 
            // lblCoinName
            // 
            this.lblCoinName.AutoSize = true;
            this.lblCoinName.Font = new System.Drawing.Font("Courier New", 10F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.lblCoinName.Location = new System.Drawing.Point(94, 46);
            this.lblCoinName.Name = "lblCoinName";
            this.lblCoinName.Size = new System.Drawing.Size(80, 16);
            this.lblCoinName.TabIndex = 20;
            this.lblCoinName.Text = "Coin Name";
            // 
            // TickerLabel
            // 
            this.TickerLabel.AutoSize = true;
            this.TickerLabel.Font = new System.Drawing.Font("Courier New", 9.75F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.TickerLabel.Location = new System.Drawing.Point(94, 62);
            this.TickerLabel.Name = "TickerLabel";
            this.TickerLabel.Size = new System.Drawing.Size(56, 16);
            this.TickerLabel.TabIndex = 21;
            this.TickerLabel.Text = "TICKER";
            // 
            // openFileDialog1
            // 
            this.openFileDialog1.FileName = "openFileDialog1";
            // 
            // lstViewDefaultCoin
            // 
            this.lstViewDefaultCoin.CurrencyIds = new long[0];
            this.lstViewDefaultCoin.Location = new System.Drawing.Point(23, 92);
            this.lstViewDefaultCoin.Name = "lstViewDefaultCoin";
            this.lstViewDefaultCoin.SelectedCurrencyId = ((long)(0));
            this.lstViewDefaultCoin.Size = new System.Drawing.Size(220, 197);
            this.lstViewDefaultCoin.TabIndex = 22;
            // 
            // DefaultCoinSelectorDummy
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.BackColor = System.Drawing.SystemColors.InactiveCaption;
            this.ClientSize = new System.Drawing.Size(261, 340);
            this.Controls.Add(this.lstViewDefaultCoin);
            this.Controls.Add(this.TickerLabel);
            this.Controls.Add(this.lblCoinName);
            this.Controls.Add(this.picCoinImage);
            this.Controls.Add(this.label1);
            this.Icon = ((System.Drawing.Icon)(resources.GetObject("$this.Icon")));
            this.Name = "DefaultCoinSelectorDummy";
            this.ShowInTaskbar = true;
            this.StartPosition = System.Windows.Forms.FormStartPosition.CenterScreen;
            this.Text = "Select Default Coin";
            this.FormClosing += new System.Windows.Forms.FormClosingEventHandler(this.DefaultCoinSelector_FormClosing);
            this.Shown += new System.EventHandler(this.DefaultCoinSelectorDummy_Shown);
            this.Controls.SetChildIndex(this.btnOK, 0);
            this.Controls.SetChildIndex(this.btnCancel, 0);
            this.Controls.SetChildIndex(this.label1, 0);
            this.Controls.SetChildIndex(this.picCoinImage, 0);
            this.Controls.SetChildIndex(this.lblCoinName, 0);
            this.Controls.SetChildIndex(this.TickerLabel, 0);
            this.Controls.SetChildIndex(this.lstViewDefaultCoin, 0);
            ((System.ComponentModel.ISupportInitialize)(this.picCoinImage)).EndInit();
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion
        private System.Windows.Forms.Label label1;
        private System.Windows.Forms.PictureBox picCoinImage;
        private System.Windows.Forms.Label lblCoinName;
        private System.Windows.Forms.Label TickerLabel;
        private System.Windows.Forms.OpenFileDialog openFileDialog1;
        private CurrencyView lstViewDefaultCoin;
    }
}
