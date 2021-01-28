namespace Pandora.Client.PandorasWallet.Dialogs
{
    partial class AddTokenDialog
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
            this.lstViewTokens = new System.Windows.Forms.ListView();
            this.columnName = ((System.Windows.Forms.ColumnHeader)(new System.Windows.Forms.ColumnHeader()));
            this.columnSymbol = ((System.Windows.Forms.ColumnHeader)(new System.Windows.Forms.ColumnHeader()));
            this.label1 = new System.Windows.Forms.Label();
            this.picToken = new System.Windows.Forms.PictureBox();
            this.TxtBoxTokenAddress = new System.Windows.Forms.TextBox();
            this.label2 = new System.Windows.Forms.Label();
            this.txtBoxTokenName = new System.Windows.Forms.TextBox();
            this.txtBoxTokenSymbol = new System.Windows.Forms.TextBox();
            this.txtBoxTokenDecimals = new System.Windows.Forms.TextBox();
            this.label3 = new System.Windows.Forms.Label();
            this.label4 = new System.Windows.Forms.Label();
            this.label5 = new System.Windows.Forms.Label();
            this.comboBoxNetwork = new System.Windows.Forms.ComboBox();
            this.TokenIconsList = new System.Windows.Forms.ImageList(this.components);
            this.label6 = new System.Windows.Forms.Label();
            this.groupBox1 = new System.Windows.Forms.GroupBox();
            this.groupBox2 = new System.Windows.Forms.GroupBox();
            ((System.ComponentModel.ISupportInitialize)(this.picToken)).BeginInit();
            this.groupBox1.SuspendLayout();
            this.groupBox2.SuspendLayout();
            this.SuspendLayout();
            // 
            // btnCancel
            // 
            this.btnCancel.Location = new System.Drawing.Point(397, 419);
            // 
            // btnOK
            // 
            this.btnOK.Enabled = false;
            this.btnOK.Location = new System.Drawing.Point(315, 419);
            this.btnOK.Text = "Add";
            this.btnOK.Click += new System.EventHandler(this.btnOK_Click);
            // 
            // lstViewTokens
            // 
            this.lstViewTokens.Columns.AddRange(new System.Windows.Forms.ColumnHeader[] {
            this.columnName,
            this.columnSymbol});
            this.lstViewTokens.Enabled = false;
            this.lstViewTokens.FullRowSelect = true;
            this.lstViewTokens.GridLines = true;
            this.lstViewTokens.HideSelection = false;
            this.lstViewTokens.Location = new System.Drawing.Point(9, 26);
            this.lstViewTokens.MultiSelect = false;
            this.lstViewTokens.Name = "lstViewTokens";
            this.lstViewTokens.Size = new System.Drawing.Size(195, 375);
            this.lstViewTokens.TabIndex = 7;
            this.lstViewTokens.UseCompatibleStateImageBehavior = false;
            this.lstViewTokens.View = System.Windows.Forms.View.Details;
            this.lstViewTokens.SelectedIndexChanged += new System.EventHandler(this.lstViewTokens_SelectedIndexChanged);
            // 
            // columnName
            // 
            this.columnName.Text = "Name";
            this.columnName.Width = 131;
            // 
            // columnSymbol
            // 
            this.columnSymbol.Text = "Symbol";
            this.columnSymbol.Width = 66;
            // 
            // label1
            // 
            this.label1.AutoSize = true;
            this.label1.Location = new System.Drawing.Point(9, 10);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(79, 13);
            this.label1.TabIndex = 8;
            this.label1.Text = "Select a token:";
            // 
            // picToken
            // 
            this.picToken.BackgroundImage = global::Pandora.Client.PandorasWallet.Properties.Resources.goldbars;
            this.picToken.BackgroundImageLayout = System.Windows.Forms.ImageLayout.Stretch;
            this.picToken.InitialImage = null;
            this.picToken.Location = new System.Drawing.Point(93, 29);
            this.picToken.Name = "picToken";
            this.picToken.Size = new System.Drawing.Size(64, 64);
            this.picToken.TabIndex = 9;
            this.picToken.TabStop = false;
            // 
            // TxtBoxTokenAddress
            // 
            this.TxtBoxTokenAddress.Location = new System.Drawing.Point(14, 164);
            this.TxtBoxTokenAddress.Name = "TxtBoxTokenAddress";
            this.TxtBoxTokenAddress.Size = new System.Drawing.Size(221, 20);
            this.TxtBoxTokenAddress.TabIndex = 10;
            this.TxtBoxTokenAddress.TextChanged += new System.EventHandler(this.TxtBoxTokenAddress_TextChanged);
            // 
            // label2
            // 
            this.label2.AutoSize = true;
            this.label2.Location = new System.Drawing.Point(14, 148);
            this.label2.Name = "label2";
            this.label2.Size = new System.Drawing.Size(82, 13);
            this.label2.TabIndex = 11;
            this.label2.Text = "Token Address:";
            // 
            // txtBoxTokenName
            // 
            this.txtBoxTokenName.Location = new System.Drawing.Point(15, 38);
            this.txtBoxTokenName.Name = "txtBoxTokenName";
            this.txtBoxTokenName.ReadOnly = true;
            this.txtBoxTokenName.Size = new System.Drawing.Size(221, 20);
            this.txtBoxTokenName.TabIndex = 10;
            // 
            // txtBoxTokenSymbol
            // 
            this.txtBoxTokenSymbol.Location = new System.Drawing.Point(15, 86);
            this.txtBoxTokenSymbol.Name = "txtBoxTokenSymbol";
            this.txtBoxTokenSymbol.ReadOnly = true;
            this.txtBoxTokenSymbol.Size = new System.Drawing.Size(221, 20);
            this.txtBoxTokenSymbol.TabIndex = 10;
            // 
            // txtBoxTokenDecimals
            // 
            this.txtBoxTokenDecimals.Location = new System.Drawing.Point(15, 133);
            this.txtBoxTokenDecimals.Name = "txtBoxTokenDecimals";
            this.txtBoxTokenDecimals.ReadOnly = true;
            this.txtBoxTokenDecimals.Size = new System.Drawing.Size(221, 20);
            this.txtBoxTokenDecimals.TabIndex = 10;
            this.txtBoxTokenDecimals.Text = "0";
            // 
            // label3
            // 
            this.label3.AutoSize = true;
            this.label3.Location = new System.Drawing.Point(15, 22);
            this.label3.Name = "label3";
            this.label3.Size = new System.Drawing.Size(35, 13);
            this.label3.TabIndex = 12;
            this.label3.Text = "Name";
            // 
            // label4
            // 
            this.label4.AutoSize = true;
            this.label4.Location = new System.Drawing.Point(15, 70);
            this.label4.Name = "label4";
            this.label4.Size = new System.Drawing.Size(41, 13);
            this.label4.TabIndex = 12;
            this.label4.Text = "Symbol";
            // 
            // label5
            // 
            this.label5.AutoSize = true;
            this.label5.Location = new System.Drawing.Point(12, 117);
            this.label5.Name = "label5";
            this.label5.Size = new System.Drawing.Size(50, 13);
            this.label5.TabIndex = 12;
            this.label5.Text = "Decimals";
            // 
            // comboBoxNetwork
            // 
            this.comboBoxNetwork.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.comboBoxNetwork.FormattingEnabled = true;
            this.comboBoxNetwork.Location = new System.Drawing.Point(14, 120);
            this.comboBoxNetwork.Name = "comboBoxNetwork";
            this.comboBoxNetwork.Size = new System.Drawing.Size(221, 21);
            this.comboBoxNetwork.TabIndex = 13;
            // 
            // TokenIconsList
            // 
            this.TokenIconsList.ColorDepth = System.Windows.Forms.ColorDepth.Depth8Bit;
            this.TokenIconsList.ImageSize = new System.Drawing.Size(16, 16);
            this.TokenIconsList.TransparentColor = System.Drawing.Color.Transparent;
            // 
            // label6
            // 
            this.label6.AutoSize = true;
            this.label6.Location = new System.Drawing.Point(11, 104);
            this.label6.Name = "label6";
            this.label6.Size = new System.Drawing.Size(81, 13);
            this.label6.TabIndex = 14;
            this.label6.Text = "Token Network";
            // 
            // groupBox1
            // 
            this.groupBox1.Controls.Add(this.txtBoxTokenName);
            this.groupBox1.Controls.Add(this.txtBoxTokenSymbol);
            this.groupBox1.Controls.Add(this.txtBoxTokenDecimals);
            this.groupBox1.Controls.Add(this.label5);
            this.groupBox1.Controls.Add(this.label3);
            this.groupBox1.Controls.Add(this.label4);
            this.groupBox1.Location = new System.Drawing.Point(216, 229);
            this.groupBox1.Name = "groupBox1";
            this.groupBox1.Size = new System.Drawing.Size(249, 172);
            this.groupBox1.TabIndex = 15;
            this.groupBox1.TabStop = false;
            this.groupBox1.Text = "Token details";
            // 
            // groupBox2
            // 
            this.groupBox2.Controls.Add(this.picToken);
            this.groupBox2.Controls.Add(this.TxtBoxTokenAddress);
            this.groupBox2.Controls.Add(this.label6);
            this.groupBox2.Controls.Add(this.label2);
            this.groupBox2.Controls.Add(this.comboBoxNetwork);
            this.groupBox2.Location = new System.Drawing.Point(216, 22);
            this.groupBox2.Name = "groupBox2";
            this.groupBox2.Size = new System.Drawing.Size(249, 201);
            this.groupBox2.TabIndex = 16;
            this.groupBox2.TabStop = false;
            this.groupBox2.Text = "Insert your token address";
            // 
            // AddTokenDialog
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(484, 454);
            this.Controls.Add(this.groupBox2);
            this.Controls.Add(this.groupBox1);
            this.Controls.Add(this.label1);
            this.Controls.Add(this.lstViewTokens);
            this.Name = "AddTokenDialog";
            this.Text = "Add new currency token";
            this.FormClosing += new System.Windows.Forms.FormClosingEventHandler(this.AddTokenDialog_FormClosing);
            this.Controls.SetChildIndex(this.btnOK, 0);
            this.Controls.SetChildIndex(this.btnCancel, 0);
            this.Controls.SetChildIndex(this.lstViewTokens, 0);
            this.Controls.SetChildIndex(this.label1, 0);
            this.Controls.SetChildIndex(this.groupBox1, 0);
            this.Controls.SetChildIndex(this.groupBox2, 0);
            ((System.ComponentModel.ISupportInitialize)(this.picToken)).EndInit();
            this.groupBox1.ResumeLayout(false);
            this.groupBox1.PerformLayout();
            this.groupBox2.ResumeLayout(false);
            this.groupBox2.PerformLayout();
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.ListView lstViewTokens;
        private System.Windows.Forms.Label label1;
        private System.Windows.Forms.PictureBox picToken;
        private System.Windows.Forms.TextBox TxtBoxTokenAddress;
        private System.Windows.Forms.Label label2;
        private System.Windows.Forms.ColumnHeader columnName;
        private System.Windows.Forms.ColumnHeader columnSymbol;
        private System.Windows.Forms.TextBox txtBoxTokenName;
        private System.Windows.Forms.TextBox txtBoxTokenSymbol;
        private System.Windows.Forms.TextBox txtBoxTokenDecimals;
        private System.Windows.Forms.Label label3;
        private System.Windows.Forms.Label label4;
        private System.Windows.Forms.Label label5;
        private System.Windows.Forms.ComboBox comboBoxNetwork;
        private System.Windows.Forms.ImageList TokenIconsList;
        private System.Windows.Forms.Label label6;
        private System.Windows.Forms.GroupBox groupBox1;
        private System.Windows.Forms.GroupBox groupBox2;
    }
}