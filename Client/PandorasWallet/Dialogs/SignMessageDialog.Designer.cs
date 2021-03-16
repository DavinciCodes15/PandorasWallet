
namespace Pandora.Client.PandorasWallet.Dialogs
{
    partial class SignMessageDialog
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
            this.components = new System.ComponentModel.Container();
            this.txtBoxMsgToSign = new System.Windows.Forms.TextBox();
            this.lblMsgToSign = new System.Windows.Forms.Label();
            this.txtBoxSignature = new System.Windows.Forms.TextBox();
            this.lblMsgSigned = new System.Windows.Forms.Label();
            this.btnSign = new System.Windows.Forms.Button();
            this.btnVerifySignature = new System.Windows.Forms.Button();
            this.btnCopy = new System.Windows.Forms.Button();
            this.label1 = new System.Windows.Forms.Label();
            this.comboBoxCurrency = new System.Windows.Forms.ComboBox();
            this.toolTipCopy = new System.Windows.Forms.ToolTip(this.components);
            this.label2 = new System.Windows.Forms.Label();
            this.txtBoxAddress = new System.Windows.Forms.TextBox();
            this.groupBox1 = new System.Windows.Forms.GroupBox();
            this.btnClear = new System.Windows.Forms.Button();
            this.panelHelp = new System.Windows.Forms.Panel();
            this.label4 = new System.Windows.Forms.Label();
            this.label3 = new System.Windows.Forms.Label();
            this.listBox1 = new System.Windows.Forms.ListBox();
            this.btnHelp = new System.Windows.Forms.Button();
            this.groupBox1.SuspendLayout();
            this.panelHelp.SuspendLayout();
            this.SuspendLayout();
            // 
            // btnCancel
            // 
            this.btnCancel.Location = new System.Drawing.Point(482, 313);
            this.btnCancel.Text = "Close";
            // 
            // btnOK
            // 
            this.btnOK.Location = new System.Drawing.Point(379, 313);
            this.btnOK.Visible = false;
            // 
            // txtBoxMsgToSign
            // 
            this.txtBoxMsgToSign.Location = new System.Drawing.Point(99, 78);
            this.txtBoxMsgToSign.Multiline = true;
            this.txtBoxMsgToSign.Name = "txtBoxMsgToSign";
            this.txtBoxMsgToSign.Size = new System.Drawing.Size(440, 65);
            this.txtBoxMsgToSign.TabIndex = 7;
            // 
            // lblMsgToSign
            // 
            this.lblMsgToSign.AutoSize = true;
            this.lblMsgToSign.Location = new System.Drawing.Point(28, 78);
            this.lblMsgToSign.Name = "lblMsgToSign";
            this.lblMsgToSign.Size = new System.Drawing.Size(53, 13);
            this.lblMsgToSign.TabIndex = 8;
            this.lblMsgToSign.Text = "Message:";
            // 
            // txtBoxSignature
            // 
            this.txtBoxSignature.Location = new System.Drawing.Point(99, 168);
            this.txtBoxSignature.Multiline = true;
            this.txtBoxSignature.Name = "txtBoxSignature";
            this.txtBoxSignature.Size = new System.Drawing.Size(440, 53);
            this.txtBoxSignature.TabIndex = 7;
            this.txtBoxSignature.TextChanged += new System.EventHandler(this.txtBoxSignedMsg_TextChanged);
            // 
            // lblMsgSigned
            // 
            this.lblMsgSigned.AutoSize = true;
            this.lblMsgSigned.Location = new System.Drawing.Point(28, 168);
            this.lblMsgSigned.Name = "lblMsgSigned";
            this.lblMsgSigned.Size = new System.Drawing.Size(55, 13);
            this.lblMsgSigned.TabIndex = 8;
            this.lblMsgSigned.Text = "Signature:";
            // 
            // btnSign
            // 
            this.btnSign.Location = new System.Drawing.Point(10, 262);
            this.btnSign.Name = "btnSign";
            this.btnSign.Size = new System.Drawing.Size(92, 23);
            this.btnSign.TabIndex = 9;
            this.btnSign.Text = "Sign Message";
            this.btnSign.UseVisualStyleBackColor = true;
            this.btnSign.Click += new System.EventHandler(this.btnSign_Click);
            // 
            // btnVerifySignature
            // 
            this.btnVerifySignature.Location = new System.Drawing.Point(108, 262);
            this.btnVerifySignature.Name = "btnVerifySignature";
            this.btnVerifySignature.Size = new System.Drawing.Size(92, 23);
            this.btnVerifySignature.TabIndex = 10;
            this.btnVerifySignature.Text = "Verify Signature";
            this.btnVerifySignature.UseVisualStyleBackColor = true;
            this.btnVerifySignature.Click += new System.EventHandler(this.btnVerifySignature_Click);
            // 
            // btnCopy
            // 
            this.btnCopy.Enabled = false;
            this.btnCopy.Location = new System.Drawing.Point(462, 227);
            this.btnCopy.Name = "btnCopy";
            this.btnCopy.Size = new System.Drawing.Size(77, 23);
            this.btnCopy.TabIndex = 11;
            this.btnCopy.Text = "Copy";
            this.toolTipCopy.SetToolTip(this.btnCopy, "Copied");
            this.btnCopy.UseVisualStyleBackColor = true;
            this.btnCopy.Click += new System.EventHandler(this.btnCopy_Click);
            // 
            // label1
            // 
            this.label1.AutoSize = true;
            this.label1.Location = new System.Drawing.Point(16, 36);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(67, 13);
            this.label1.TabIndex = 12;
            this.label1.Text = "Signing with:";
            // 
            // comboBoxCurrency
            // 
            this.comboBoxCurrency.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.comboBoxCurrency.FormattingEnabled = true;
            this.comboBoxCurrency.Location = new System.Drawing.Point(99, 33);
            this.comboBoxCurrency.Name = "comboBoxCurrency";
            this.comboBoxCurrency.Size = new System.Drawing.Size(101, 21);
            this.comboBoxCurrency.TabIndex = 13;
            this.comboBoxCurrency.SelectedIndexChanged += new System.EventHandler(this.comboBoxCurrency_SelectedIndexChanged);
            // 
            // toolTipCopy
            // 
            this.toolTipCopy.Active = false;
            // 
            // label2
            // 
            this.label2.AutoSize = true;
            this.label2.Location = new System.Drawing.Point(242, 20);
            this.label2.Name = "label2";
            this.label2.Size = new System.Drawing.Size(80, 13);
            this.label2.TabIndex = 14;
            this.label2.Text = "Public Address:";
            // 
            // txtBoxAddress
            // 
            this.txtBoxAddress.Location = new System.Drawing.Point(245, 36);
            this.txtBoxAddress.Name = "txtBoxAddress";
            this.txtBoxAddress.ReadOnly = true;
            this.txtBoxAddress.Size = new System.Drawing.Size(294, 20);
            this.txtBoxAddress.TabIndex = 15;
            // 
            // groupBox1
            // 
            this.groupBox1.Controls.Add(this.btnClear);
            this.groupBox1.Controls.Add(this.txtBoxAddress);
            this.groupBox1.Controls.Add(this.comboBoxCurrency);
            this.groupBox1.Controls.Add(this.label1);
            this.groupBox1.Controls.Add(this.btnCopy);
            this.groupBox1.Controls.Add(this.txtBoxMsgToSign);
            this.groupBox1.Controls.Add(this.btnVerifySignature);
            this.groupBox1.Controls.Add(this.lblMsgToSign);
            this.groupBox1.Controls.Add(this.btnSign);
            this.groupBox1.Controls.Add(this.label2);
            this.groupBox1.Controls.Add(this.lblMsgSigned);
            this.groupBox1.Controls.Add(this.txtBoxSignature);
            this.groupBox1.Location = new System.Drawing.Point(12, 12);
            this.groupBox1.Name = "groupBox1";
            this.groupBox1.Size = new System.Drawing.Size(556, 291);
            this.groupBox1.TabIndex = 16;
            this.groupBox1.TabStop = false;
            this.groupBox1.Text = "Sign/Verify a message";
            // 
            // btnClear
            // 
            this.btnClear.Location = new System.Drawing.Point(381, 227);
            this.btnClear.Name = "btnClear";
            this.btnClear.Size = new System.Drawing.Size(75, 23);
            this.btnClear.TabIndex = 16;
            this.btnClear.Text = "Clear";
            this.btnClear.UseVisualStyleBackColor = true;
            this.btnClear.Click += new System.EventHandler(this.btnClear_Click);
            // 
            // panelHelp
            // 
            this.panelHelp.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
            this.panelHelp.Controls.Add(this.label4);
            this.panelHelp.Controls.Add(this.label3);
            this.panelHelp.Controls.Add(this.listBox1);
            this.panelHelp.Location = new System.Drawing.Point(12, 10);
            this.panelHelp.Name = "panelHelp";
            this.panelHelp.Size = new System.Drawing.Size(556, 295);
            this.panelHelp.TabIndex = 16;
            this.panelHelp.Visible = false;
            // 
            // label4
            // 
            this.label4.AutoSize = true;
            this.label4.Location = new System.Drawing.Point(9, 5);
            this.label4.Name = "label4";
            this.label4.Size = new System.Drawing.Size(118, 13);
            this.label4.TabIndex = 4;
            this.label4.Text = "How to use signatures?";
            // 
            // label3
            // 
            this.label3.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.label3.AutoSize = true;
            this.label3.Font = new System.Drawing.Font("Microsoft Sans Serif", 7F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.label3.Location = new System.Drawing.Point(537, 3);
            this.label3.Name = "label3";
            this.label3.Size = new System.Drawing.Size(14, 13);
            this.label3.TabIndex = 3;
            this.label3.Text = "X";
            this.label3.Click += new System.EventHandler(this.btn_close_help_Click);
            // 
            // listBox1
            // 
            this.listBox1.FormattingEnabled = true;
            this.listBox1.Items.AddRange(new object[] {
            "****** Sign a message *****",
            "",
            "1.- Select the currency you want to sign with.",
            "",
            "2.- Write the message you want to sign, for example: \"I Have DJ15 Token at <Your " +
                "public adress>\"",
            "",
            "3.- Click \"Sign Message\" button and check your password to verify your identity i" +
                "n pop-up.",
            "",
            "4.- Copy your Signature message by clicking on the \'Copy\' button below the Signat" +
                "ure Text box.",
            "",
            "5.- Share your message with your negotiants to verify the veracity of your portfo" +
                "lio without having ",
            "to share your credentials. it\'s no fees, absolutely free.",
            "",
            "",
            "***** Verify a message ownership *****",
            "",
            "1.- Add your message and signature in the respective text boxes.",
            "",
            "2.- Click \"Verify Signature\" button.",
            "",
            "3.- You will see if it\'s a verified message of your Pandoras Wallet."});
            this.listBox1.Location = new System.Drawing.Point(7, 26);
            this.listBox1.Name = "listBox1";
            this.listBox1.Size = new System.Drawing.Size(542, 251);
            this.listBox1.TabIndex = 0;
            // 
            // btnHelp
            // 
            this.btnHelp.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));
            this.btnHelp.Location = new System.Drawing.Point(12, 313);
            this.btnHelp.Name = "btnHelp";
            this.btnHelp.Size = new System.Drawing.Size(75, 23);
            this.btnHelp.TabIndex = 21;
            this.btnHelp.Text = "Help";
            this.btnHelp.UseVisualStyleBackColor = true;
            this.btnHelp.Click += new System.EventHandler(this.btnHelp_Click);
            // 
            // SignMessageDialog
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(578, 348);
            this.Controls.Add(this.panelHelp);
            this.Controls.Add(this.btnHelp);
            this.Controls.Add(this.groupBox1);
            this.Name = "SignMessageDialog";
            this.Text = "Sign Message and Verify Signature";
            this.Controls.SetChildIndex(this.groupBox1, 0);
            this.Controls.SetChildIndex(this.btnHelp, 0);
            this.Controls.SetChildIndex(this.panelHelp, 0);
            this.Controls.SetChildIndex(this.btnOK, 0);
            this.Controls.SetChildIndex(this.btnCancel, 0);
            this.groupBox1.ResumeLayout(false);
            this.groupBox1.PerformLayout();
            this.panelHelp.ResumeLayout(false);
            this.panelHelp.PerformLayout();
            this.ResumeLayout(false);

        }

        #endregion

        private System.Windows.Forms.TextBox txtBoxMsgToSign;
        private System.Windows.Forms.Label lblMsgToSign;
        private System.Windows.Forms.TextBox txtBoxSignature;
        private System.Windows.Forms.Label lblMsgSigned;
        private System.Windows.Forms.Button btnSign;
        private System.Windows.Forms.Button btnVerifySignature;
        private System.Windows.Forms.Button btnCopy;
        private System.Windows.Forms.Label label1;
        private System.Windows.Forms.ComboBox comboBoxCurrency;
        private System.Windows.Forms.ToolTip toolTipCopy;
        private System.Windows.Forms.Label label2;
        private System.Windows.Forms.TextBox txtBoxAddress;
        private System.Windows.Forms.GroupBox groupBox1;
        private System.Windows.Forms.Button btnHelp;
        private System.Windows.Forms.Panel panelHelp;
        private System.Windows.Forms.ListBox listBox1;
        private System.Windows.Forms.Button btnClear;
        private System.Windows.Forms.Label label3;
        private System.Windows.Forms.Label label4;
    }
}