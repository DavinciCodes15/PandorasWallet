namespace Pandora.Client.PandorasWallet.Dialogs
{
    partial class SendTransactionDialog
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
            this.label1 = new System.Windows.Forms.Label();
            this.label2 = new System.Windows.Forms.Label();
            this.label3 = new System.Windows.Forms.Label();
            this.lblAmount = new System.Windows.Forms.Label();
            this.label5 = new System.Windows.Forms.Label();
            this.lblToAddress = new System.Windows.Forms.Label();
            this.label6 = new System.Windows.Forms.Label();
            this.lblFromAddress = new System.Windows.Forms.Label();
            this.label4 = new System.Windows.Forms.Label();
            this.label7 = new System.Windows.Forms.Label();
            this.label8 = new System.Windows.Forms.Label();
            this.lblFeeRateTitle = new System.Windows.Forms.Label();
            this.groupBox1 = new System.Windows.Forms.GroupBox();
            this.lblTxFee = new System.Windows.Forms.Label();
            this.groupBox2 = new System.Windows.Forms.GroupBox();
            this.label11 = new System.Windows.Forms.Label();
            this.lblDiscounted = new System.Windows.Forms.Label();
            this.lblTxFeeRate = new System.Windows.Forms.Label();
            this.lblBalanceAfter = new System.Windows.Forms.Label();
            this.lblBalanceBefore = new System.Windows.Forms.Label();
            this.lblWarning = new System.Windows.Forms.Label();
            this.pictureBox = new System.Windows.Forms.PictureBox();
            this.checkBoxAdvanced = new System.Windows.Forms.CheckBox();
            this.label9 = new System.Windows.Forms.Label();
            this.lblNonce = new System.Windows.Forms.Label();
            this.groupBoxAdvanced = new System.Windows.Forms.GroupBox();
            this.numericCustomFee = new System.Windows.Forms.NumericUpDown();
            this.numericNonce = new System.Windows.Forms.NumericUpDown();
            this.chckBoxSubsFee = new System.Windows.Forms.CheckBox();
            this.groupBox1.SuspendLayout();
            this.groupBox2.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.pictureBox)).BeginInit();
            this.groupBoxAdvanced.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.numericCustomFee)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.numericNonce)).BeginInit();
            this.SuspendLayout();
            // 
            // btnCancel
            // 
            this.btnCancel.Location = new System.Drawing.Point(803, 392);
            this.btnCancel.Margin = new System.Windows.Forms.Padding(4);
            this.btnCancel.Size = new System.Drawing.Size(112, 30);
            this.btnCancel.Click += new System.EventHandler(this.btnCancel_Click);
            // 
            // btnOK
            // 
            this.btnOK.Location = new System.Drawing.Point(680, 392);
            this.btnOK.Margin = new System.Windows.Forms.Padding(4);
            this.btnOK.Size = new System.Drawing.Size(112, 30);
            this.btnOK.Text = "Confirm";
            this.btnOK.Click += new System.EventHandler(this.btnOK_Click);
            // 
            // label1
            // 
            this.label1.AutoSize = true;
            this.label1.Font = new System.Drawing.Font("Microsoft Sans Serif", 12.25F, System.Drawing.FontStyle.Bold);
            this.label1.Location = new System.Drawing.Point(125, 25);
            this.label1.Margin = new System.Windows.Forms.Padding(4, 0, 4, 0);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(411, 20);
            this.label1.TabIndex = 7;
            this.label1.Text = "Are you sure you want to send this transaction?";
            // 
            // label2
            // 
            this.label2.AutoSize = true;
            this.label2.Font = new System.Drawing.Font("Microsoft Sans Serif", 10.25F);
            this.label2.Location = new System.Drawing.Point(126, 61);
            this.label2.Margin = new System.Windows.Forms.Padding(4, 0, 4, 0);
            this.label2.Name = "label2";
            this.label2.Size = new System.Drawing.Size(353, 17);
            this.label2.TabIndex = 8;
            this.label2.Text = "Once you confirm this transaction it can not be undone";
            this.label2.TextAlign = System.Drawing.ContentAlignment.BottomLeft;
            // 
            // label3
            // 
            this.label3.AutoSize = true;
            this.label3.Font = new System.Drawing.Font("Microsoft Sans Serif", 10.25F, System.Drawing.FontStyle.Bold);
            this.label3.Location = new System.Drawing.Point(83, 33);
            this.label3.Margin = new System.Windows.Forms.Padding(4, 0, 4, 0);
            this.label3.Name = "label3";
            this.label3.Size = new System.Drawing.Size(67, 17);
            this.label3.TabIndex = 9;
            this.label3.Text = "Amount:";
            // 
            // lblAmount
            // 
            this.lblAmount.AutoSize = true;
            this.lblAmount.Font = new System.Drawing.Font("Microsoft Sans Serif", 10.25F);
            this.lblAmount.Location = new System.Drawing.Point(158, 33);
            this.lblAmount.Margin = new System.Windows.Forms.Padding(4, 0, 4, 0);
            this.lblAmount.Name = "lblAmount";
            this.lblAmount.Size = new System.Drawing.Size(56, 17);
            this.lblAmount.TabIndex = 10;
            this.lblAmount.Text = "Amount";
            // 
            // label5
            // 
            this.label5.AutoSize = true;
            this.label5.Font = new System.Drawing.Font("Microsoft Sans Serif", 10.25F, System.Drawing.FontStyle.Bold);
            this.label5.Location = new System.Drawing.Point(118, 128);
            this.label5.Margin = new System.Windows.Forms.Padding(4, 0, 4, 0);
            this.label5.Name = "label5";
            this.label5.Size = new System.Drawing.Size(32, 17);
            this.label5.TabIndex = 11;
            this.label5.Text = "To:";
            // 
            // lblToAddress
            // 
            this.lblToAddress.AutoSize = true;
            this.lblToAddress.Font = new System.Drawing.Font("Microsoft Sans Serif", 10.25F);
            this.lblToAddress.Location = new System.Drawing.Point(158, 128);
            this.lblToAddress.Margin = new System.Windows.Forms.Padding(4, 0, 4, 0);
            this.lblToAddress.Name = "lblToAddress";
            this.lblToAddress.Size = new System.Drawing.Size(77, 17);
            this.lblToAddress.TabIndex = 12;
            this.lblToAddress.Text = "ToAddress";
            // 
            // label6
            // 
            this.label6.AutoSize = true;
            this.label6.Font = new System.Drawing.Font("Microsoft Sans Serif", 10.25F, System.Drawing.FontStyle.Bold);
            this.label6.Location = new System.Drawing.Point(101, 94);
            this.label6.Margin = new System.Windows.Forms.Padding(4, 0, 4, 0);
            this.label6.Name = "label6";
            this.label6.Size = new System.Drawing.Size(49, 17);
            this.label6.TabIndex = 13;
            this.label6.Text = "From:";
            // 
            // lblFromAddress
            // 
            this.lblFromAddress.AutoSize = true;
            this.lblFromAddress.Enabled = false;
            this.lblFromAddress.Font = new System.Drawing.Font("Microsoft Sans Serif", 10.25F);
            this.lblFromAddress.Location = new System.Drawing.Point(158, 94);
            this.lblFromAddress.Margin = new System.Windows.Forms.Padding(4, 0, 4, 0);
            this.lblFromAddress.Name = "lblFromAddress";
            this.lblFromAddress.Size = new System.Drawing.Size(92, 17);
            this.lblFromAddress.TabIndex = 14;
            this.lblFromAddress.Text = "FromAddress";
            // 
            // label4
            // 
            this.label4.AutoSize = true;
            this.label4.Font = new System.Drawing.Font("Microsoft Sans Serif", 10.25F, System.Drawing.FontStyle.Bold);
            this.label4.Location = new System.Drawing.Point(19, 63);
            this.label4.Margin = new System.Windows.Forms.Padding(4, 0, 4, 0);
            this.label4.Name = "label4";
            this.label4.Size = new System.Drawing.Size(131, 17);
            this.label4.TabIndex = 11;
            this.label4.Text = "Transaction Fee:";
            // 
            // label7
            // 
            this.label7.AutoSize = true;
            this.label7.Font = new System.Drawing.Font("Microsoft Sans Serif", 10.25F, System.Drawing.FontStyle.Bold);
            this.label7.Location = new System.Drawing.Point(44, 35);
            this.label7.Margin = new System.Windows.Forms.Padding(4, 0, 4, 0);
            this.label7.Name = "label7";
            this.label7.Size = new System.Drawing.Size(124, 17);
            this.label7.TabIndex = 11;
            this.label7.Text = "Balance Before:";
            // 
            // label8
            // 
            this.label8.AutoSize = true;
            this.label8.Font = new System.Drawing.Font("Microsoft Sans Serif", 10.25F, System.Drawing.FontStyle.Bold);
            this.label8.Location = new System.Drawing.Point(57, 94);
            this.label8.Margin = new System.Windows.Forms.Padding(4, 0, 4, 0);
            this.label8.Name = "label8";
            this.label8.Size = new System.Drawing.Size(111, 17);
            this.label8.TabIndex = 11;
            this.label8.Text = "Balance After:";
            // 
            // lblFeeRateTitle
            // 
            this.lblFeeRateTitle.Font = new System.Drawing.Font("Microsoft Sans Serif", 10.25F, System.Drawing.FontStyle.Bold);
            this.lblFeeRateTitle.Location = new System.Drawing.Point(24, 128);
            this.lblFeeRateTitle.Margin = new System.Windows.Forms.Padding(4, 0, 4, 0);
            this.lblFeeRateTitle.Name = "lblFeeRateTitle";
            this.lblFeeRateTitle.Size = new System.Drawing.Size(144, 17);
            this.lblFeeRateTitle.TabIndex = 11;
            this.lblFeeRateTitle.Text = "Fee Rate (per Kb):";
            this.lblFeeRateTitle.TextAlign = System.Drawing.ContentAlignment.MiddleRight;
            // 
            // groupBox1
            // 
            this.groupBox1.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.groupBox1.Controls.Add(this.label3);
            this.groupBox1.Controls.Add(this.lblTxFee);
            this.groupBox1.Controls.Add(this.lblAmount);
            this.groupBox1.Controls.Add(this.lblFromAddress);
            this.groupBox1.Controls.Add(this.label5);
            this.groupBox1.Controls.Add(this.label6);
            this.groupBox1.Controls.Add(this.label4);
            this.groupBox1.Controls.Add(this.lblToAddress);
            this.groupBox1.Location = new System.Drawing.Point(8, 119);
            this.groupBox1.Name = "groupBox1";
            this.groupBox1.Size = new System.Drawing.Size(548, 169);
            this.groupBox1.TabIndex = 16;
            this.groupBox1.TabStop = false;
            this.groupBox1.Text = "Transaction Details";
            // 
            // lblTxFee
            // 
            this.lblTxFee.AutoSize = true;
            this.lblTxFee.Font = new System.Drawing.Font("Microsoft Sans Serif", 10.25F);
            this.lblTxFee.Location = new System.Drawing.Point(158, 63);
            this.lblTxFee.Margin = new System.Windows.Forms.Padding(4, 0, 4, 0);
            this.lblTxFee.Name = "lblTxFee";
            this.lblTxFee.Size = new System.Drawing.Size(47, 17);
            this.lblTxFee.TabIndex = 10;
            this.lblTxFee.Text = "TxFee";
            // 
            // groupBox2
            // 
            this.groupBox2.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.groupBox2.Controls.Add(this.label7);
            this.groupBox2.Controls.Add(this.label11);
            this.groupBox2.Controls.Add(this.label8);
            this.groupBox2.Controls.Add(this.lblDiscounted);
            this.groupBox2.Controls.Add(this.lblTxFeeRate);
            this.groupBox2.Controls.Add(this.lblBalanceAfter);
            this.groupBox2.Controls.Add(this.lblBalanceBefore);
            this.groupBox2.Controls.Add(this.lblFeeRateTitle);
            this.groupBox2.Location = new System.Drawing.Point(562, 119);
            this.groupBox2.Name = "groupBox2";
            this.groupBox2.Size = new System.Drawing.Size(353, 169);
            this.groupBox2.TabIndex = 17;
            this.groupBox2.TabStop = false;
            this.groupBox2.Text = "Balance Status";
            // 
            // label11
            // 
            this.label11.AutoSize = true;
            this.label11.Font = new System.Drawing.Font("Microsoft Sans Serif", 10.25F, System.Drawing.FontStyle.Bold);
            this.label11.Location = new System.Drawing.Point(74, 63);
            this.label11.Margin = new System.Windows.Forms.Padding(4, 0, 4, 0);
            this.label11.Name = "label11";
            this.label11.Size = new System.Drawing.Size(94, 17);
            this.label11.TabIndex = 11;
            this.label11.Text = "Discounted:";
            // 
            // lblDiscounted
            // 
            this.lblDiscounted.AutoSize = true;
            this.lblDiscounted.Font = new System.Drawing.Font("Microsoft Sans Serif", 10.25F, System.Drawing.FontStyle.Bold);
            this.lblDiscounted.ForeColor = System.Drawing.Color.Black;
            this.lblDiscounted.Location = new System.Drawing.Point(176, 63);
            this.lblDiscounted.Margin = new System.Windows.Forms.Padding(4, 0, 4, 0);
            this.lblDiscounted.Name = "lblDiscounted";
            this.lblDiscounted.Size = new System.Drawing.Size(89, 17);
            this.lblDiscounted.TabIndex = 10;
            this.lblDiscounted.Text = "Discounted";
            // 
            // lblTxFeeRate
            // 
            this.lblTxFeeRate.AutoSize = true;
            this.lblTxFeeRate.Font = new System.Drawing.Font("Microsoft Sans Serif", 10.25F);
            this.lblTxFeeRate.Location = new System.Drawing.Point(176, 128);
            this.lblTxFeeRate.Margin = new System.Windows.Forms.Padding(4, 0, 4, 0);
            this.lblTxFeeRate.Name = "lblTxFeeRate";
            this.lblTxFeeRate.Size = new System.Drawing.Size(77, 17);
            this.lblTxFeeRate.TabIndex = 10;
            this.lblTxFeeRate.Text = "TxFeeRate";
            // 
            // lblBalanceAfter
            // 
            this.lblBalanceAfter.AutoSize = true;
            this.lblBalanceAfter.Font = new System.Drawing.Font("Microsoft Sans Serif", 10.25F);
            this.lblBalanceAfter.Location = new System.Drawing.Point(176, 94);
            this.lblBalanceAfter.Margin = new System.Windows.Forms.Padding(4, 0, 4, 0);
            this.lblBalanceAfter.Name = "lblBalanceAfter";
            this.lblBalanceAfter.Size = new System.Drawing.Size(89, 17);
            this.lblBalanceAfter.TabIndex = 10;
            this.lblBalanceAfter.Text = "BalanceAfter";
            // 
            // lblBalanceBefore
            // 
            this.lblBalanceBefore.AutoSize = true;
            this.lblBalanceBefore.Font = new System.Drawing.Font("Microsoft Sans Serif", 10.25F);
            this.lblBalanceBefore.Location = new System.Drawing.Point(176, 35);
            this.lblBalanceBefore.Margin = new System.Windows.Forms.Padding(4, 0, 4, 0);
            this.lblBalanceBefore.Name = "lblBalanceBefore";
            this.lblBalanceBefore.Size = new System.Drawing.Size(101, 17);
            this.lblBalanceBefore.TabIndex = 10;
            this.lblBalanceBefore.Text = "BalanceBefore";
            // 
            // lblWarning
            // 
            this.lblWarning.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
            this.lblWarning.Font = new System.Drawing.Font("Microsoft Sans Serif", 10.25F);
            this.lblWarning.ForeColor = System.Drawing.Color.Red;
            this.lblWarning.Location = new System.Drawing.Point(562, 370);
            this.lblWarning.Name = "lblWarning";
            this.lblWarning.Size = new System.Drawing.Size(353, 17);
            this.lblWarning.TabIndex = 19;
            this.lblWarning.Text = "Warning";
            this.lblWarning.TextAlign = System.Drawing.ContentAlignment.MiddleRight;
            this.lblWarning.Visible = false;
            // 
            // pictureBox
            // 
            this.pictureBox.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
            this.pictureBox.Image = global::Pandora.Client.PandorasWallet.Properties.Resources.Bang;
            this.pictureBox.Location = new System.Drawing.Point(30, 12);
            this.pictureBox.Name = "pictureBox";
            this.pictureBox.Size = new System.Drawing.Size(77, 75);
            this.pictureBox.SizeMode = System.Windows.Forms.PictureBoxSizeMode.StretchImage;
            this.pictureBox.TabIndex = 15;
            this.pictureBox.TabStop = false;
            // 
            // checkBoxAdvanced
            // 
            this.checkBoxAdvanced.AutoSize = true;
            this.checkBoxAdvanced.Font = new System.Drawing.Font("Microsoft Sans Serif", 9F);
            this.checkBoxAdvanced.Location = new System.Drawing.Point(803, 103);
            this.checkBoxAdvanced.Name = "checkBoxAdvanced";
            this.checkBoxAdvanced.Size = new System.Drawing.Size(113, 19);
            this.checkBoxAdvanced.TabIndex = 21;
            this.checkBoxAdvanced.Text = "Show advanced";
            this.checkBoxAdvanced.UseVisualStyleBackColor = true;
            this.checkBoxAdvanced.CheckedChanged += new System.EventHandler(this.checkBoxAdvanced_CheckedChanged);
            // 
            // label9
            // 
            this.label9.AutoSize = true;
            this.label9.Font = new System.Drawing.Font("Microsoft Sans Serif", 10.25F);
            this.label9.Location = new System.Drawing.Point(19, 32);
            this.label9.Name = "label9";
            this.label9.Size = new System.Drawing.Size(157, 17);
            this.label9.TabIndex = 0;
            this.label9.Text = "Custom transaction fee:";
            // 
            // lblNonce
            // 
            this.lblNonce.AutoSize = true;
            this.lblNonce.Font = new System.Drawing.Font("Microsoft Sans Serif", 10.25F);
            this.lblNonce.Location = new System.Drawing.Point(351, 34);
            this.lblNonce.Name = "lblNonce";
            this.lblNonce.Size = new System.Drawing.Size(130, 17);
            this.lblNonce.TabIndex = 2;
            this.lblNonce.Text = "Transaction nonce:";
            // 
            // groupBoxAdvanced
            // 
            this.groupBoxAdvanced.Controls.Add(this.numericCustomFee);
            this.groupBoxAdvanced.Controls.Add(this.numericNonce);
            this.groupBoxAdvanced.Controls.Add(this.lblNonce);
            this.groupBoxAdvanced.Controls.Add(this.label9);
            this.groupBoxAdvanced.Location = new System.Drawing.Point(8, 294);
            this.groupBoxAdvanced.Name = "groupBoxAdvanced";
            this.groupBoxAdvanced.Size = new System.Drawing.Size(907, 70);
            this.groupBoxAdvanced.TabIndex = 22;
            this.groupBoxAdvanced.TabStop = false;
            this.groupBoxAdvanced.Text = "Advanced transaction options";
            this.groupBoxAdvanced.Visible = false;
            // 
            // numericCustomFee
            // 
            this.numericCustomFee.DecimalPlaces = 8;
            this.numericCustomFee.Font = new System.Drawing.Font("Microsoft Sans Serif", 10F);
            this.numericCustomFee.Increment = new decimal(new int[] {
            1,
            0,
            0,
            524288});
            this.numericCustomFee.Location = new System.Drawing.Point(182, 31);
            this.numericCustomFee.Maximum = new decimal(new int[] {
            1569325055,
            23283064,
            0,
            0});
            this.numericCustomFee.Name = "numericCustomFee";
            this.numericCustomFee.Size = new System.Drawing.Size(120, 23);
            this.numericCustomFee.TabIndex = 23;
            this.numericCustomFee.ValueChanged += new System.EventHandler(this.numericCustomFee_ValueChanged);
            // 
            // numericNonce
            // 
            this.numericNonce.Font = new System.Drawing.Font("Microsoft Sans Serif", 10F);
            this.numericNonce.Location = new System.Drawing.Point(484, 32);
            this.numericNonce.Maximum = new decimal(new int[] {
            1569325055,
            23283064,
            0,
            0});
            this.numericNonce.Name = "numericNonce";
            this.numericNonce.Size = new System.Drawing.Size(120, 23);
            this.numericNonce.TabIndex = 23;
            // 
            // chckBoxSubsFee
            // 
            this.chckBoxSubsFee.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));
            this.chckBoxSubsFee.AutoSize = true;
            this.chckBoxSubsFee.Font = new System.Drawing.Font("Microsoft Sans Serif", 10.25F);
            this.chckBoxSubsFee.Location = new System.Drawing.Point(30, 398);
            this.chckBoxSubsFee.Name = "chckBoxSubsFee";
            this.chckBoxSubsFee.Size = new System.Drawing.Size(249, 21);
            this.chckBoxSubsFee.TabIndex = 23;
            this.chckBoxSubsFee.Text = "Substract Fee from amount to send";
            this.chckBoxSubsFee.UseVisualStyleBackColor = true;
            this.chckBoxSubsFee.CheckedChanged += new System.EventHandler(this.checkSubtFee_CheckedChanged);
            // 
            // SendTransactionDialog
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(9F, 17F);
            this.BackColor = System.Drawing.SystemColors.InactiveCaption;
            this.ClientSize = new System.Drawing.Size(933, 437);
            this.Controls.Add(this.chckBoxSubsFee);
            this.Controls.Add(this.groupBoxAdvanced);
            this.Controls.Add(this.checkBoxAdvanced);
            this.Controls.Add(this.lblWarning);
            this.Controls.Add(this.groupBox2);
            this.Controls.Add(this.groupBox1);
            this.Controls.Add(this.pictureBox);
            this.Controls.Add(this.label2);
            this.Controls.Add(this.label1);
            this.Font = new System.Drawing.Font("Microsoft Sans Serif", 10.25F, System.Drawing.FontStyle.Bold);
            this.Margin = new System.Windows.Forms.Padding(4);
            this.Name = "SendTransactionDialog";
            this.ShowIcon = false;
            this.Text = "Confirmation";
            this.Controls.SetChildIndex(this.label1, 0);
            this.Controls.SetChildIndex(this.label2, 0);
            this.Controls.SetChildIndex(this.pictureBox, 0);
            this.Controls.SetChildIndex(this.groupBox1, 0);
            this.Controls.SetChildIndex(this.groupBox2, 0);
            this.Controls.SetChildIndex(this.lblWarning, 0);
            this.Controls.SetChildIndex(this.checkBoxAdvanced, 0);
            this.Controls.SetChildIndex(this.groupBoxAdvanced, 0);
            this.Controls.SetChildIndex(this.btnOK, 0);
            this.Controls.SetChildIndex(this.btnCancel, 0);
            this.Controls.SetChildIndex(this.chckBoxSubsFee, 0);
            this.groupBox1.ResumeLayout(false);
            this.groupBox1.PerformLayout();
            this.groupBox2.ResumeLayout(false);
            this.groupBox2.PerformLayout();
            ((System.ComponentModel.ISupportInitialize)(this.pictureBox)).EndInit();
            this.groupBoxAdvanced.ResumeLayout(false);
            this.groupBoxAdvanced.PerformLayout();
            ((System.ComponentModel.ISupportInitialize)(this.numericCustomFee)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.numericNonce)).EndInit();
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.Label label1;
        private System.Windows.Forms.Label label2;
        private System.Windows.Forms.Label label3;
        private System.Windows.Forms.Label label5;
        private System.Windows.Forms.Label lblAmount;
        private System.Windows.Forms.Label lblToAddress;
        private System.Windows.Forms.Label label6;
        private System.Windows.Forms.Label lblFromAddress;
        private System.Windows.Forms.PictureBox pictureBox;
        private System.Windows.Forms.Label label4;
        private System.Windows.Forms.Label label7;
        private System.Windows.Forms.Label label8;
        private System.Windows.Forms.Label lblFeeRateTitle;
        private System.Windows.Forms.GroupBox groupBox1;
        private System.Windows.Forms.Label lblTxFee;
        private System.Windows.Forms.GroupBox groupBox2;
        private System.Windows.Forms.Label lblTxFeeRate;
        private System.Windows.Forms.Label lblBalanceAfter;
        private System.Windows.Forms.Label lblBalanceBefore;
        private System.Windows.Forms.Label lblWarning;
        private System.Windows.Forms.Label label11;
        private System.Windows.Forms.Label lblDiscounted;
        private System.Windows.Forms.CheckBox checkBoxAdvanced;
        private System.Windows.Forms.Label label9;
        private System.Windows.Forms.Label lblNonce;
        private System.Windows.Forms.GroupBox groupBoxAdvanced;
        private System.Windows.Forms.NumericUpDown numericNonce;
        private System.Windows.Forms.CheckBox chckBoxSubsFee;
        private System.Windows.Forms.NumericUpDown numericCustomFee;
        //private System.Windows.Forms.Timer timer1;
    }
}
