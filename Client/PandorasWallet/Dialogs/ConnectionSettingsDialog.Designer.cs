namespace Pandora.Client.PandorasWallet.Dialogs
{
    partial class ConnectionSettingsDialog
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
            this.gbConnection = new System.Windows.Forms.GroupBox();
            this.txbServerName = new System.Windows.Forms.TextBox();
            this.chbEncryptConnection = new System.Windows.Forms.CheckBox();
            this.nudPortNumber = new System.Windows.Forms.NumericUpDown();
            this.lblPortNumber = new System.Windows.Forms.Label();
            this.lblServerName = new System.Windows.Forms.Label();
            this.gbConnection.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.nudPortNumber)).BeginInit();
            this.SuspendLayout();
            // 
            // btnCancel
            // 
            this.btnCancel.Location = new System.Drawing.Point(297, 110);
            this.btnCancel.Click += new System.EventHandler(this.BtnCancel_Click);
            // 
            // btnOK
            // 
            this.btnOK.Location = new System.Drawing.Point(216, 110);
            this.btnOK.Click += new System.EventHandler(this.BtnOK_Click);
            // 
            // gbConnection
            // 
            this.gbConnection.Controls.Add(this.txbServerName);
            this.gbConnection.Controls.Add(this.chbEncryptConnection);
            this.gbConnection.Controls.Add(this.nudPortNumber);
            this.gbConnection.Controls.Add(this.lblPortNumber);
            this.gbConnection.Controls.Add(this.lblServerName);
            this.gbConnection.Location = new System.Drawing.Point(12, 12);
            this.gbConnection.Name = "gbConnection";
            this.gbConnection.Size = new System.Drawing.Size(378, 92);
            this.gbConnection.TabIndex = 7;
            this.gbConnection.TabStop = false;
            this.gbConnection.Text = "Connection";
            // 
            // txbServerName
            // 
            this.txbServerName.Location = new System.Drawing.Point(94, 24);
            this.txbServerName.Name = "txbServerName";
            this.txbServerName.Size = new System.Drawing.Size(266, 20);
            this.txbServerName.TabIndex = 4;
            // 
            // chbEncryptConnection
            // 
            this.chbEncryptConnection.AutoSize = true;
            this.chbEncryptConnection.Checked = true;
            this.chbEncryptConnection.CheckState = System.Windows.Forms.CheckState.Checked;
            this.chbEncryptConnection.Location = new System.Drawing.Point(241, 55);
            this.chbEncryptConnection.Name = "chbEncryptConnection";
            this.chbEncryptConnection.Size = new System.Drawing.Size(119, 17);
            this.chbEncryptConnection.TabIndex = 3;
            this.chbEncryptConnection.Text = "Encrypt Connection";
            this.chbEncryptConnection.UseVisualStyleBackColor = true;
            // 
            // nudPortNumber
            // 
            this.nudPortNumber.Location = new System.Drawing.Point(94, 54);
            this.nudPortNumber.Name = "nudPortNumber";
            this.nudPortNumber.Size = new System.Drawing.Size(120, 20);
            this.nudPortNumber.TabIndex = 2;
            // 
            // lblPortNumber
            // 
            this.lblPortNumber.AutoSize = true;
            this.lblPortNumber.Location = new System.Drawing.Point(18, 54);
            this.lblPortNumber.Name = "lblPortNumber";
            this.lblPortNumber.Size = new System.Drawing.Size(69, 13);
            this.lblPortNumber.TabIndex = 1;
            this.lblPortNumber.Text = "Port Number:";
            // 
            // lblServerName
            // 
            this.lblServerName.AutoSize = true;
            this.lblServerName.Location = new System.Drawing.Point(18, 27);
            this.lblServerName.Name = "lblServerName";
            this.lblServerName.Size = new System.Drawing.Size(72, 13);
            this.lblServerName.TabIndex = 0;
            this.lblServerName.Text = "Server Name:";
            // 
            // ConnectionSettingsDialog
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.BackColor = System.Drawing.SystemColors.InactiveCaption;
            this.ClientSize = new System.Drawing.Size(393, 136);
            this.Controls.Add(this.gbConnection);
            this.Name = "ConnectionSettingsDialog";
            this.Text = "ConnectionSettingsDialog";
            this.FormClosing += new System.Windows.Forms.FormClosingEventHandler(this.ConnectionSettingsDialog_FormClosing);
            this.Load += new System.EventHandler(this.ConnectionSettingsDialog_Load);
            this.Controls.SetChildIndex(this.btnOK, 0);
            this.Controls.SetChildIndex(this.btnCancel, 0);
            this.Controls.SetChildIndex(this.gbConnection, 0);
            this.gbConnection.ResumeLayout(false);
            this.gbConnection.PerformLayout();
            ((System.ComponentModel.ISupportInitialize)(this.nudPortNumber)).EndInit();
            this.ResumeLayout(false);

        }

        #endregion

        private System.Windows.Forms.GroupBox gbConnection;
        private System.Windows.Forms.Label lblServerName;
        private System.Windows.Forms.Label lblPortNumber;
        private System.Windows.Forms.NumericUpDown nudPortNumber;
        private System.Windows.Forms.TextBox txbServerName;
        private System.Windows.Forms.CheckBox chbEncryptConnection;
    }
}