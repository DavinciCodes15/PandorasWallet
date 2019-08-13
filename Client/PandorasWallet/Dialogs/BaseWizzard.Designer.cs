namespace Pandora.Client.PandorasWallet.Dialogs
{
    partial class BaseWizzard
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
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(BaseWizzard));
            this.btnNext = new System.Windows.Forms.Button();
            this.btnBack = new System.Windows.Forms.Button();
            this.btnCancel = new System.Windows.Forms.Button();
            this.tabControl1 = new System.Windows.Forms.TabControl();
            this.Introduction = new System.Windows.Forms.TabPage();
            this.lblIntroductionText = new System.Windows.Forms.Label();
            this.OptionChoose = new System.Windows.Forms.TabPage();
            this.rbtnWords = new System.Windows.Forms.RadioButton();
            this.rbtnFile = new System.Windows.Forms.RadioButton();
            this.lblStep1 = new System.Windows.Forms.Label();
            this.File = new System.Windows.Forms.TabPage();
            this.btnFile = new System.Windows.Forms.Button();
            this.txtFile = new System.Windows.Forms.TextBox();
            this.lblFile = new System.Windows.Forms.Label();
            this.TwelveWords = new System.Windows.Forms.TabPage();
            this.lblCopy = new System.Windows.Forms.Label();
            this.btnCopy = new System.Windows.Forms.Button();
            this.lblTwelveWords = new System.Windows.Forms.Label();
            this.twelveWords1 = new Pandora.Client.PandorasWallet.TwelveWords();
            this.Finish = new System.Windows.Forms.TabPage();
            this.lblFinish = new System.Windows.Forms.Label();
            this.label1 = new System.Windows.Forms.Label();
            this.lbl1 = new System.Windows.Forms.Label();
            this.pictureBox1 = new System.Windows.Forms.PictureBox();
            this.tabControl1.SuspendLayout();
            this.Introduction.SuspendLayout();
            this.OptionChoose.SuspendLayout();
            this.File.SuspendLayout();
            this.TwelveWords.SuspendLayout();
            this.Finish.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.pictureBox1)).BeginInit();
            this.SuspendLayout();
            // 
            // btnNext
            // 
            this.btnNext.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
            this.btnNext.Location = new System.Drawing.Point(560, 246);
            this.btnNext.Name = "btnNext";
            this.btnNext.Size = new System.Drawing.Size(99, 34);
            this.btnNext.TabIndex = 26;
            this.btnNext.Text = "Next";
            this.btnNext.UseVisualStyleBackColor = true;
            this.btnNext.Click += new System.EventHandler(this.btnNext_Click);
            // 
            // btnBack
            // 
            this.btnBack.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
            this.btnBack.Location = new System.Drawing.Point(455, 246);
            this.btnBack.Name = "btnBack";
            this.btnBack.Size = new System.Drawing.Size(99, 34);
            this.btnBack.TabIndex = 25;
            this.btnBack.Text = "Back";
            this.btnBack.UseVisualStyleBackColor = true;
            this.btnBack.Click += new System.EventHandler(this.btnBack_Click);
            // 
            // btnCancel
            // 
            this.btnCancel.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
            this.btnCancel.Location = new System.Drawing.Point(350, 246);
            this.btnCancel.Name = "btnCancel";
            this.btnCancel.Size = new System.Drawing.Size(99, 34);
            this.btnCancel.TabIndex = 24;
            this.btnCancel.Text = "Cancel";
            this.btnCancel.UseVisualStyleBackColor = true;
            this.btnCancel.Click += new System.EventHandler(this.btnCancel_Click);
            // 
            // tabControl1
            // 
            this.tabControl1.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.tabControl1.Controls.Add(this.Introduction);
            this.tabControl1.Controls.Add(this.OptionChoose);
            this.tabControl1.Controls.Add(this.File);
            this.tabControl1.Controls.Add(this.TwelveWords);
            this.tabControl1.Controls.Add(this.Finish);
            this.tabControl1.Location = new System.Drawing.Point(12, 13);
            this.tabControl1.Name = "tabControl1";
            this.tabControl1.SelectedIndex = 0;
            this.tabControl1.Size = new System.Drawing.Size(655, 215);
            this.tabControl1.TabIndex = 23;
            this.tabControl1.SelectedIndexChanged += new System.EventHandler(this.tabControl1_SelectedIndexChanged);
            // 
            // Introduction
            // 
            this.Introduction.BackColor = System.Drawing.Color.WhiteSmoke;
            this.Introduction.Controls.Add(this.pictureBox1);
            this.Introduction.Controls.Add(this.lblIntroductionText);
            this.Introduction.Location = new System.Drawing.Point(4, 22);
            this.Introduction.Name = "Introduction";
            this.Introduction.Padding = new System.Windows.Forms.Padding(3);
            this.Introduction.Size = new System.Drawing.Size(647, 189);
            this.Introduction.TabIndex = 0;
            this.Introduction.Text = "Introduction";
            // 
            // lblIntroductionText
            // 
            this.lblIntroductionText.AutoSize = true;
            this.lblIntroductionText.Font = new System.Drawing.Font("Microsoft Sans Serif", 8.25F);
            this.lblIntroductionText.Location = new System.Drawing.Point(165, 21);
            this.lblIntroductionText.Name = "lblIntroductionText";
            this.lblIntroductionText.Size = new System.Drawing.Size(208, 13);
            this.lblIntroductionText.TabIndex = 0;
            this.lblIntroductionText.Text = "This wizard will help you restore your wallet";
            // 
            // OptionChoose
            // 
            this.OptionChoose.BackColor = System.Drawing.Color.WhiteSmoke;
            this.OptionChoose.Controls.Add(this.rbtnWords);
            this.OptionChoose.Controls.Add(this.rbtnFile);
            this.OptionChoose.Controls.Add(this.lblStep1);
            this.OptionChoose.Location = new System.Drawing.Point(4, 22);
            this.OptionChoose.Name = "OptionChoose";
            this.OptionChoose.Padding = new System.Windows.Forms.Padding(3);
            this.OptionChoose.Size = new System.Drawing.Size(647, 170);
            this.OptionChoose.TabIndex = 1;
            this.OptionChoose.Text = "OptionChoose";
            // 
            // rbtnWords
            // 
            this.rbtnWords.AutoSize = true;
            this.rbtnWords.CheckAlign = System.Drawing.ContentAlignment.TopLeft;
            this.rbtnWords.Font = new System.Drawing.Font("Microsoft Sans Serif", 8.25F);
            this.rbtnWords.Location = new System.Drawing.Point(164, 110);
            this.rbtnWords.Name = "rbtnWords";
            this.rbtnWords.Size = new System.Drawing.Size(136, 17);
            this.rbtnWords.TabIndex = 5;
            this.rbtnWords.Text = "Restore using 12 words";
            this.rbtnWords.TextAlign = System.Drawing.ContentAlignment.TopLeft;
            this.rbtnWords.UseVisualStyleBackColor = true;
            // 
            // rbtnFile
            // 
            this.rbtnFile.AutoSize = true;
            this.rbtnFile.CheckAlign = System.Drawing.ContentAlignment.TopLeft;
            this.rbtnFile.Checked = true;
            this.rbtnFile.Font = new System.Drawing.Font("Microsoft Sans Serif", 8.25F);
            this.rbtnFile.ImageAlign = System.Drawing.ContentAlignment.MiddleLeft;
            this.rbtnFile.Location = new System.Drawing.Point(161, 41);
            this.rbtnFile.Name = "rbtnFile";
            this.rbtnFile.Size = new System.Drawing.Size(149, 17);
            this.rbtnFile.TabIndex = 4;
            this.rbtnFile.TabStop = true;
            this.rbtnFile.Text = "Restore from a backup file";
            this.rbtnFile.UseVisualStyleBackColor = true;
            // 
            // lblStep1
            // 
            this.lblStep1.AutoSize = true;
            this.lblStep1.Font = new System.Drawing.Font("Microsoft Sans Serif", 8.25F);
            this.lblStep1.Location = new System.Drawing.Point(161, 21);
            this.lblStep1.Name = "lblStep1";
            this.lblStep1.Size = new System.Drawing.Size(187, 13);
            this.lblStep1.TabIndex = 3;
            this.lblStep1.Text = "Step 1. Select your restoration method";
            // 
            // File
            // 
            this.File.BackColor = System.Drawing.Color.WhiteSmoke;
            this.File.Controls.Add(this.btnFile);
            this.File.Controls.Add(this.txtFile);
            this.File.Controls.Add(this.lblFile);
            this.File.Location = new System.Drawing.Point(4, 22);
            this.File.Name = "File";
            this.File.Size = new System.Drawing.Size(647, 170);
            this.File.TabIndex = 2;
            this.File.Text = "File";
            // 
            // btnFile
            // 
            this.btnFile.Location = new System.Drawing.Point(569, 59);
            this.btnFile.Name = "btnFile";
            this.btnFile.Size = new System.Drawing.Size(31, 20);
            this.btnFile.TabIndex = 2;
            this.btnFile.Text = "...";
            this.btnFile.UseVisualStyleBackColor = true;
            this.btnFile.Click += new System.EventHandler(this.FileButton_Click);
            // 
            // txtFile
            // 
            this.txtFile.Location = new System.Drawing.Point(164, 59);
            this.txtFile.Name = "txtFile";
            this.txtFile.Size = new System.Drawing.Size(396, 20);
            this.txtFile.TabIndex = 1;
            this.txtFile.TextChanged += new System.EventHandler(this.txtFile_TextChanged);
            // 
            // lblFile
            // 
            this.lblFile.AutoSize = true;
            this.lblFile.Location = new System.Drawing.Point(161, 21);
            this.lblFile.Name = "lblFile";
            this.lblFile.Size = new System.Drawing.Size(165, 13);
            this.lblFile.TabIndex = 0;
            this.lblFile.Text = "Step 2. Select your restoration file";
            // 
            // TwelveWords
            // 
            this.TwelveWords.Controls.Add(this.lblCopy);
            this.TwelveWords.Controls.Add(this.btnCopy);
            this.TwelveWords.Controls.Add(this.lblTwelveWords);
            this.TwelveWords.Controls.Add(this.twelveWords1);
            this.TwelveWords.Location = new System.Drawing.Point(4, 22);
            this.TwelveWords.Name = "TwelveWords";
            this.TwelveWords.Size = new System.Drawing.Size(647, 189);
            this.TwelveWords.TabIndex = 4;
            this.TwelveWords.Text = "TwelveWords";
            this.TwelveWords.UseVisualStyleBackColor = true;
            // 
            // lblCopy
            // 
            this.lblCopy.AutoSize = true;
            this.lblCopy.Location = new System.Drawing.Point(560, 12);
            this.lblCopy.Name = "lblCopy";
            this.lblCopy.Size = new System.Drawing.Size(40, 13);
            this.lblCopy.TabIndex = 7;
            this.lblCopy.Text = "Copied";
            // 
            // btnCopy
            // 
            this.btnCopy.BackgroundImage = ((System.Drawing.Image)(resources.GetObject("btnCopy.BackgroundImage")));
            this.btnCopy.BackgroundImageLayout = System.Windows.Forms.ImageLayout.Stretch;
            this.btnCopy.Location = new System.Drawing.Point(606, 2);
            this.btnCopy.Name = "btnCopy";
            this.btnCopy.Size = new System.Drawing.Size(32, 32);
            this.btnCopy.TabIndex = 6;
            this.btnCopy.UseVisualStyleBackColor = true;
            this.btnCopy.Click += new System.EventHandler(this.btnCopy_Click);
            // 
            // lblTwelveWords
            // 
            this.lblTwelveWords.AutoSize = true;
            this.lblTwelveWords.Location = new System.Drawing.Point(167, 10);
            this.lblTwelveWords.Name = "lblTwelveWords";
            this.lblTwelveWords.Size = new System.Drawing.Size(196, 13);
            this.lblTwelveWords.TabIndex = 4;
            this.lblTwelveWords.Text = "Step 2. Introduce in each box the words";
            // 
            // twelveWords1
            // 
            this.twelveWords1.Location = new System.Drawing.Point(170, 28);
            this.twelveWords1.Name = "twelveWords1";
            this.twelveWords1.Size = new System.Drawing.Size(474, 152);
            this.twelveWords1.TabIndex = 5;
            this.twelveWords1.Word1 = "";
            this.twelveWords1.Word10 = "";
            this.twelveWords1.Word11 = "";
            this.twelveWords1.Word12 = "";
            this.twelveWords1.Word2 = "";
            this.twelveWords1.Word3 = "";
            this.twelveWords1.Word4 = "";
            this.twelveWords1.Word5 = "";
            this.twelveWords1.Word6 = "";
            this.twelveWords1.Word7 = "";
            this.twelveWords1.Word8 = "";
            this.twelveWords1.Word9 = "";
            // 
            // Finish
            // 
            this.Finish.BackColor = System.Drawing.Color.WhiteSmoke;
            this.Finish.Controls.Add(this.lblFinish);
            this.Finish.Controls.Add(this.label1);
            this.Finish.Controls.Add(this.lbl1);
            this.Finish.Location = new System.Drawing.Point(4, 22);
            this.Finish.Name = "Finish";
            this.Finish.Size = new System.Drawing.Size(647, 170);
            this.Finish.TabIndex = 3;
            this.Finish.Text = "Finish";
            // 
            // lblFinish
            // 
            this.lblFinish.AutoSize = true;
            this.lblFinish.Location = new System.Drawing.Point(299, 51);
            this.lblFinish.Name = "lblFinish";
            this.lblFinish.Size = new System.Drawing.Size(43, 26);
            this.lblFinish.TabIndex = 2;
            this.lblFinish.Text = "backup\r\n\r\n";
            // 
            // label1
            // 
            this.label1.AutoSize = true;
            this.label1.Location = new System.Drawing.Point(161, 51);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(144, 13);
            this.label1.TabIndex = 1;
            this.label1.Text = "Your wallet was successfully \r\n";
            // 
            // lbl1
            // 
            this.lbl1.AutoSize = true;
            this.lbl1.Location = new System.Drawing.Point(161, 21);
            this.lbl1.Name = "lbl1";
            this.lbl1.Size = new System.Drawing.Size(75, 13);
            this.lbl1.TabIndex = 0;
            this.lbl1.Text = "Congratulation";
            // 
            // pictureBox1
            // 
            this.pictureBox1.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.pictureBox1.Image = ((System.Drawing.Image)(resources.GetObject("pictureBox1.Image")));
            this.pictureBox1.Location = new System.Drawing.Point(7, 7);
            this.pictureBox1.Margin = new System.Windows.Forms.Padding(4);
            this.pictureBox1.Name = "pictureBox1";
            this.pictureBox1.Size = new System.Drawing.Size(141, 179);
            this.pictureBox1.SizeMode = System.Windows.Forms.PictureBoxSizeMode.StretchImage;
            this.pictureBox1.TabIndex = 27;
            this.pictureBox1.TabStop = false;
            // 
            // BaseWizzard
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(682, 303);
            this.Controls.Add(this.btnNext);
            this.Controls.Add(this.btnBack);
            this.Controls.Add(this.btnCancel);
            this.Controls.Add(this.tabControl1);
            this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedDialog;
            this.Name = "BaseWizzard";
            this.ShowInTaskbar = false;
            this.StartPosition = System.Windows.Forms.FormStartPosition.CenterParent;
            this.Text = "RestoreWalletWizard";
            this.FormClosing += new System.Windows.Forms.FormClosingEventHandler(this.RestoreWalletWizard_FormClosing);
            this.Shown += new System.EventHandler(this.BaseWizzard_Shown);
            this.tabControl1.ResumeLayout(false);
            this.Introduction.ResumeLayout(false);
            this.Introduction.PerformLayout();
            this.OptionChoose.ResumeLayout(false);
            this.OptionChoose.PerformLayout();
            this.File.ResumeLayout(false);
            this.File.PerformLayout();
            this.TwelveWords.ResumeLayout(false);
            this.TwelveWords.PerformLayout();
            this.Finish.ResumeLayout(false);
            this.Finish.PerformLayout();
            ((System.ComponentModel.ISupportInitialize)(this.pictureBox1)).EndInit();
            this.ResumeLayout(false);

        }

        #endregion
        private System.Windows.Forms.Button btnNext;
        private System.Windows.Forms.Button btnBack;
        private System.Windows.Forms.Button btnCancel;
        private System.Windows.Forms.TabControl tabControl1;
        private System.Windows.Forms.TabPage Introduction;
        private System.Windows.Forms.Label lblIntroductionText;
        private System.Windows.Forms.TabPage OptionChoose;
        private System.Windows.Forms.RadioButton rbtnWords;
        private System.Windows.Forms.RadioButton rbtnFile;
        private System.Windows.Forms.Label lblStep1;
        private System.Windows.Forms.TabPage File;
        private System.Windows.Forms.TabPage Finish;
        private System.Windows.Forms.TabPage TwelveWords;
        private System.Windows.Forms.Label lbl1;
        private System.Windows.Forms.Label lblFinish;
        private System.Windows.Forms.Label label1;
        private System.Windows.Forms.Label lblFile;
        private System.Windows.Forms.Label lblTwelveWords;
        private System.Windows.Forms.Button btnFile;
        private System.Windows.Forms.TextBox txtFile;
        private System.Windows.Forms.PictureBox pictureBox1;
        private TwelveWords twelveWords1;
        private System.Windows.Forms.Button btnCopy;
        private System.Windows.Forms.Label lblCopy;
    }
}