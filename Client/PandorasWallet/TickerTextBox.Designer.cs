namespace Pandora.Client.PandorasWallet
{
    partial class TickerTextBox
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
            this.txtBoxAmount = new System.Windows.Forms.TextBox();
            this.lblTicker = new System.Windows.Forms.Label();
            this.paneltxtbox = new System.Windows.Forms.Panel();
            this.pictureDownArrow = new System.Windows.Forms.PictureBox();
            this.tableLayoutPanelTicker = new System.Windows.Forms.TableLayoutPanel();
            this.contextMenuDownArrow = new System.Windows.Forms.ContextMenuStrip(this.components);
            this.toolStripMenuItem1p = new System.Windows.Forms.ToolStripMenuItem();
            this.toolStripMenuItem5p = new System.Windows.Forms.ToolStripMenuItem();
            this.toolStripMenuItem10p = new System.Windows.Forms.ToolStripMenuItem();
            this.toolStripMenuItem25p = new System.Windows.Forms.ToolStripMenuItem();
            this.toolStripMenuItem50p = new System.Windows.Forms.ToolStripMenuItem();
            this.toolStripMenuItem75p = new System.Windows.Forms.ToolStripMenuItem();
            this.paneltxtbox.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.pictureDownArrow)).BeginInit();
            this.tableLayoutPanelTicker.SuspendLayout();
            this.contextMenuDownArrow.SuspendLayout();
            this.SuspendLayout();
            // 
            // txtBoxAmount
            // 
            this.txtBoxAmount.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Left | System.Windows.Forms.AnchorStyles.Right)));
            this.txtBoxAmount.BorderStyle = System.Windows.Forms.BorderStyle.None;
            this.txtBoxAmount.Location = new System.Drawing.Point(1, 3);
            this.txtBoxAmount.Margin = new System.Windows.Forms.Padding(10, 10, 3, 10);
            this.txtBoxAmount.Name = "txtBoxAmount";
            this.txtBoxAmount.Size = new System.Drawing.Size(103, 13);
            this.txtBoxAmount.TabIndex = 0;
            this.txtBoxAmount.Text = "0.00000000";
            this.txtBoxAmount.TextAlign = System.Windows.Forms.HorizontalAlignment.Right;
            this.txtBoxAmount.TextChanged += new System.EventHandler(this.txtBoxAmount_TextChanged);
            this.txtBoxAmount.KeyPress += new System.Windows.Forms.KeyPressEventHandler(this.txtBoxAmount_KeyPress);
            this.txtBoxAmount.Leave += new System.EventHandler(this.txtBoxAmount_Leave);
            // 
            // lblTicker
            // 
            this.lblTicker.Anchor = System.Windows.Forms.AnchorStyles.Right;
            this.lblTicker.AutoSize = true;
            this.lblTicker.BackColor = System.Drawing.Color.Transparent;
            this.lblTicker.Font = new System.Drawing.Font("Microsoft Sans Serif", 8F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.lblTicker.ImageAlign = System.Drawing.ContentAlignment.MiddleRight;
            this.lblTicker.Location = new System.Drawing.Point(0, 2);
            this.lblTicker.Margin = new System.Windows.Forms.Padding(0);
            this.lblTicker.Name = "lblTicker";
            this.lblTicker.Size = new System.Drawing.Size(14, 13);
            this.lblTicker.TabIndex = 1;
            this.lblTicker.Text = "A";
            this.lblTicker.TextAlign = System.Drawing.ContentAlignment.MiddleRight;
            // 
            // paneltxtbox
            // 
            this.paneltxtbox.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.paneltxtbox.BackColor = System.Drawing.SystemColors.Window;
            this.paneltxtbox.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
            this.paneltxtbox.Controls.Add(this.pictureDownArrow);
            this.paneltxtbox.Controls.Add(this.txtBoxAmount);
            this.paneltxtbox.Location = new System.Drawing.Point(0, 0);
            this.paneltxtbox.Name = "paneltxtbox";
            this.paneltxtbox.Padding = new System.Windows.Forms.Padding(0, 0, 20, 0);
            this.paneltxtbox.Size = new System.Drawing.Size(127, 20);
            this.paneltxtbox.TabIndex = 2;
            this.paneltxtbox.EnabledChanged += new System.EventHandler(this.paneltxtbox_EnabledChanged);
            // 
            // pictureDownArrow
            // 
            this.pictureDownArrow.BackColor = System.Drawing.Color.Transparent;
            this.pictureDownArrow.BackgroundImage = global::Pandora.Client.PandorasWallet.Properties.Resources.icons8_expand_arrow_24;
            this.pictureDownArrow.BackgroundImageLayout = System.Windows.Forms.ImageLayout.Stretch;
            this.pictureDownArrow.Cursor = System.Windows.Forms.Cursors.Hand;
            this.pictureDownArrow.Location = new System.Drawing.Point(0, 0);
            this.pictureDownArrow.Name = "pictureDownArrow";
            this.pictureDownArrow.Size = new System.Drawing.Size(18, 18);
            this.pictureDownArrow.TabIndex = 1;
            this.pictureDownArrow.TabStop = false;
            this.pictureDownArrow.Click += new System.EventHandler(this.pictureDownArrow_Click);
            // 
            // tableLayoutPanelTicker
            // 
            this.tableLayoutPanelTicker.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.tableLayoutPanelTicker.AutoSize = true;
            this.tableLayoutPanelTicker.BackColor = System.Drawing.Color.White;
            this.tableLayoutPanelTicker.ColumnCount = 1;
            this.tableLayoutPanelTicker.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle());
            this.tableLayoutPanelTicker.Controls.Add(this.lblTicker, 0, 0);
            this.tableLayoutPanelTicker.Font = new System.Drawing.Font("Microsoft Sans Serif", 8F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.tableLayoutPanelTicker.Location = new System.Drawing.Point(110, 2);
            this.tableLayoutPanelTicker.Name = "tableLayoutPanelTicker";
            this.tableLayoutPanelTicker.RightToLeft = System.Windows.Forms.RightToLeft.Yes;
            this.tableLayoutPanelTicker.RowCount = 1;
            this.tableLayoutPanelTicker.RowStyles.Add(new System.Windows.Forms.RowStyle());
            this.tableLayoutPanelTicker.Size = new System.Drawing.Size(14, 17);
            this.tableLayoutPanelTicker.TabIndex = 1;
            // 
            // contextMenuDownArrow
            // 
            this.contextMenuDownArrow.Items.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.toolStripMenuItem1p,
            this.toolStripMenuItem5p,
            this.toolStripMenuItem10p,
            this.toolStripMenuItem25p,
            this.toolStripMenuItem50p,
            this.toolStripMenuItem75p});
            this.contextMenuDownArrow.Name = "contextMenuDownArrow";
            this.contextMenuDownArrow.ShowImageMargin = false;
            this.contextMenuDownArrow.Size = new System.Drawing.Size(83, 136);
            this.contextMenuDownArrow.Closed += new System.Windows.Forms.ToolStripDropDownClosedEventHandler(this.contextMenuDownArrow_Closed);
            this.contextMenuDownArrow.Opening += new System.ComponentModel.CancelEventHandler(this.contextMenuDownArrow_Opening);
            this.contextMenuDownArrow.ItemClicked += new System.Windows.Forms.ToolStripItemClickedEventHandler(this.contextMenuDownArrow_ItemClicked);
            // 
            // toolStripMenuItem1p
            // 
            this.toolStripMenuItem1p.Name = "toolStripMenuItem1p";
            this.toolStripMenuItem1p.Size = new System.Drawing.Size(82, 22);
            this.toolStripMenuItem1p.Tag = "0.01";
            this.toolStripMenuItem1p.Text = "1% ()";
            // 
            // toolStripMenuItem5p
            // 
            this.toolStripMenuItem5p.Name = "toolStripMenuItem5p";
            this.toolStripMenuItem5p.Size = new System.Drawing.Size(82, 22);
            this.toolStripMenuItem5p.Tag = "0.05";
            this.toolStripMenuItem5p.Text = "5% ()";
            // 
            // toolStripMenuItem10p
            // 
            this.toolStripMenuItem10p.Name = "toolStripMenuItem10p";
            this.toolStripMenuItem10p.Size = new System.Drawing.Size(82, 22);
            this.toolStripMenuItem10p.Tag = "0.1";
            this.toolStripMenuItem10p.Text = "10% ()";
            // 
            // toolStripMenuItem25p
            // 
            this.toolStripMenuItem25p.Name = "toolStripMenuItem25p";
            this.toolStripMenuItem25p.Size = new System.Drawing.Size(82, 22);
            this.toolStripMenuItem25p.Tag = "0.25";
            this.toolStripMenuItem25p.Text = "25% ()";
            // 
            // toolStripMenuItem50p
            // 
            this.toolStripMenuItem50p.Name = "toolStripMenuItem50p";
            this.toolStripMenuItem50p.Size = new System.Drawing.Size(82, 22);
            this.toolStripMenuItem50p.Tag = "0.5";
            this.toolStripMenuItem50p.Text = "50% ()";
            // 
            // toolStripMenuItem75p
            // 
            this.toolStripMenuItem75p.Name = "toolStripMenuItem75p";
            this.toolStripMenuItem75p.Size = new System.Drawing.Size(82, 22);
            this.toolStripMenuItem75p.Tag = "0.75";
            this.toolStripMenuItem75p.Text = "75% ()";
            // 
            // TickerTextBox
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.BackColor = System.Drawing.Color.Transparent;
            this.Controls.Add(this.tableLayoutPanelTicker);
            this.Controls.Add(this.paneltxtbox);
            this.Margin = new System.Windows.Forms.Padding(0);
            this.MaximumSize = new System.Drawing.Size(1000, 20);
            this.Name = "TickerTextBox";
            this.Size = new System.Drawing.Size(127, 20);
            this.paneltxtbox.ResumeLayout(false);
            this.paneltxtbox.PerformLayout();
            ((System.ComponentModel.ISupportInitialize)(this.pictureDownArrow)).EndInit();
            this.tableLayoutPanelTicker.ResumeLayout(false);
            this.tableLayoutPanelTicker.PerformLayout();
            this.contextMenuDownArrow.ResumeLayout(false);
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.TextBox txtBoxAmount;
        private System.Windows.Forms.Label lblTicker;
        private System.Windows.Forms.Panel paneltxtbox;
        private System.Windows.Forms.TableLayoutPanel tableLayoutPanelTicker;
        private System.Windows.Forms.PictureBox pictureDownArrow;
        private System.Windows.Forms.ContextMenuStrip contextMenuDownArrow;
        private System.Windows.Forms.ToolStripMenuItem toolStripMenuItem1p;
        private System.Windows.Forms.ToolStripMenuItem toolStripMenuItem5p;
        private System.Windows.Forms.ToolStripMenuItem toolStripMenuItem10p;
        private System.Windows.Forms.ToolStripMenuItem toolStripMenuItem25p;
        private System.Windows.Forms.ToolStripMenuItem toolStripMenuItem50p;
        private System.Windows.Forms.ToolStripMenuItem toolStripMenuItem75p;
    }
}
