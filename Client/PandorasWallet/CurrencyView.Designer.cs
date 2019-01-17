namespace Pandora.Client.PandorasWallet
{
    partial class CurrencyView
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

        #region Component Designer generated code

        /// <summary> 
        /// Required method for Designer support - do not modify 
        /// the contents of this method with the code editor.
        /// </summary>
        private void InitializeComponent()
        {
            this.components = new System.ComponentModel.Container();
            this.FListView = new System.Windows.Forms.ListView();
            this.colCoinName = ((System.Windows.Forms.ColumnHeader)(new System.Windows.Forms.ColumnHeader()));
            this.colTicker = ((System.Windows.Forms.ColumnHeader)(new System.Windows.Forms.ColumnHeader()));
            this.colTotal = ((System.Windows.Forms.ColumnHeader)(new System.Windows.Forms.ColumnHeader()));
            this.colStat = ((System.Windows.Forms.ColumnHeader)(new System.Windows.Forms.ColumnHeader()));
            this.FImageList = new System.Windows.Forms.ImageList(this.components);
            this.SuspendLayout();
            // 
            // FListView
            // 
            this.FListView.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.FListView.Columns.AddRange(new System.Windows.Forms.ColumnHeader[] {
            this.colCoinName,
            this.colTicker,
            this.colTotal,
            this.colStat});
            this.FListView.FullRowSelect = true;
            this.FListView.GridLines = true;
            this.FListView.HideSelection = false;
            this.FListView.Location = new System.Drawing.Point(0, 0);
            this.FListView.MultiSelect = false;
            this.FListView.Name = "FListView";
            this.FListView.Size = new System.Drawing.Size(914, 352);
            this.FListView.Sorting = System.Windows.Forms.SortOrder.Ascending;
            this.FListView.TabIndex = 15;
            this.FListView.UseCompatibleStateImageBehavior = false;
            this.FListView.View = System.Windows.Forms.View.Details;
            this.FListView.ItemChecked += new System.Windows.Forms.ItemCheckedEventHandler(this.FListView_ItemChecked);
            this.FListView.SelectedIndexChanged += new System.EventHandler(this.FListView_SelectedIndexChanged);
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
            // FImageList
            // 
            this.FImageList.ColorDepth = System.Windows.Forms.ColorDepth.Depth32Bit;
            this.FImageList.ImageSize = new System.Drawing.Size(16, 16);
            this.FImageList.TransparentColor = System.Drawing.Color.Transparent;
            // 
            // CurrencyView
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.Controls.Add(this.FListView);
            this.Name = "CurrencyView";
            this.Size = new System.Drawing.Size(914, 352);
            this.ResumeLayout(false);

        }

        #endregion

        private System.Windows.Forms.ListView FListView;
        private System.Windows.Forms.ColumnHeader colCoinName;
        private System.Windows.Forms.ColumnHeader colTicker;
        private System.Windows.Forms.ColumnHeader colTotal;
        private System.Windows.Forms.ColumnHeader colStat;
        private System.Windows.Forms.ImageList FImageList;
    }
}
