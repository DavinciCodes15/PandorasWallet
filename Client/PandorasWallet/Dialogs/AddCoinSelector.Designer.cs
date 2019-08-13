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
            this.label1 = new System.Windows.Forms.Label();
            this.btnSelectAll = new System.Windows.Forms.Button();
            this.lblMaintenanceWarning = new System.Windows.Forms.Label();
            this.pictureWarning = new System.Windows.Forms.PictureBox();
            ((System.ComponentModel.ISupportInitialize)(this.pictureWarning)).BeginInit();
            this.SuspendLayout();
            // 
            // btnCancel
            // 
            this.btnCancel.Location = new System.Drawing.Point(277, 378);
            this.btnCancel.Click += new System.EventHandler(this.btnCancel_Click);
            // 
            // btnOK
            // 
            this.btnOK.Location = new System.Drawing.Point(195, 378);
            this.btnOK.Click += new System.EventHandler(this.btnOK_Click);
            // 
            // lstViewAddCurrency
            // 
            this.lstViewAddCurrency.CurrencyIds = new long[0];
            this.lstViewAddCurrency.ForeColor = System.Drawing.Color.Transparent;
            this.lstViewAddCurrency.Location = new System.Drawing.Point(9, 35);
            this.lstViewAddCurrency.Name = "lstViewAddCurrency";
            this.lstViewAddCurrency.SelectedCurrencyId = ((long)(0));
            this.lstViewAddCurrency.Size = new System.Drawing.Size(342, 300);
            this.lstViewAddCurrency.TabIndex = 9;
            // 
            // label1
            // 
            this.label1.AutoSize = true;
            this.label1.Font = new System.Drawing.Font("Microsoft Sans Serif", 9F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.label1.Location = new System.Drawing.Point(10, 13);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(166, 15);
            this.label1.TabIndex = 10;
            this.label1.Text = "Select currencies to add:";
            // 
            // btnSelectAll
            // 
            this.btnSelectAll.Font = new System.Drawing.Font("Microsoft Sans Serif", 8.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.btnSelectAll.Location = new System.Drawing.Point(293, 12);
            this.btnSelectAll.Name = "btnSelectAll";
            this.btnSelectAll.Size = new System.Drawing.Size(59, 20);
            this.btnSelectAll.TabIndex = 11;
            this.btnSelectAll.Text = "Select all";
            this.btnSelectAll.UseVisualStyleBackColor = true;
            this.btnSelectAll.Click += new System.EventHandler(this.btnSelectAll_Click);
            // 
            // lblMaintenanceWarning
            // 
            this.lblMaintenanceWarning.Location = new System.Drawing.Point(40, 339);
            this.lblMaintenanceWarning.Name = "lblMaintenanceWarning";
            this.lblMaintenanceWarning.Size = new System.Drawing.Size(311, 38);
            this.lblMaintenanceWarning.TabIndex = 13;
            this.lblMaintenanceWarning.Text = "Please consider that with maintenance mode currencies you can receive coins but y" +
    "ou will not be able to sent them";
            // 
            // pictureWarning
            // 
            this.pictureWarning.BackgroundImage = global::Pandora.Client.PandorasWallet.Properties.Resources.warning;
            this.pictureWarning.BackgroundImageLayout = System.Windows.Forms.ImageLayout.Stretch;
            this.pictureWarning.Location = new System.Drawing.Point(9, 339);
            this.pictureWarning.Name = "pictureWarning";
            this.pictureWarning.Size = new System.Drawing.Size(28, 26);
            this.pictureWarning.TabIndex = 12;
            this.pictureWarning.TabStop = false;
            // 
            // AddCoinSelector
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.BackColor = System.Drawing.SystemColors.InactiveCaption;
            this.ClientSize = new System.Drawing.Size(362, 410);
            this.Controls.Add(this.lblMaintenanceWarning);
            this.Controls.Add(this.pictureWarning);
            this.Controls.Add(this.btnSelectAll);
            this.Controls.Add(this.label1);
            this.Controls.Add(this.lstViewAddCurrency);
            this.Name = "AddCoinSelector";
            this.Text = "Available Currencies";
            this.FormClosing += new System.Windows.Forms.FormClosingEventHandler(this.AddCoinSelector_FormClosing);
            this.Shown += new System.EventHandler(this.AddCoinSelector_Shown);
            this.Controls.SetChildIndex(this.btnOK, 0);
            this.Controls.SetChildIndex(this.btnCancel, 0);
            this.Controls.SetChildIndex(this.lstViewAddCurrency, 0);
            this.Controls.SetChildIndex(this.label1, 0);
            this.Controls.SetChildIndex(this.btnSelectAll, 0);
            this.Controls.SetChildIndex(this.pictureWarning, 0);
            this.Controls.SetChildIndex(this.lblMaintenanceWarning, 0);
            ((System.ComponentModel.ISupportInitialize)(this.pictureWarning)).EndInit();
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion
        private CurrencyView lstViewAddCurrency;
        private System.Windows.Forms.Label label1;
        private System.Windows.Forms.Button btnSelectAll;
        private System.Windows.Forms.PictureBox pictureWarning;
        private System.Windows.Forms.Label lblMaintenanceWarning;
    }
}
