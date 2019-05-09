namespace Pandora.Client.ThreadingMgrProposal
{
    partial class Form1
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
            this.btnWorking = new System.Windows.Forms.Button();
            this.txtFinish = new System.Windows.Forms.TextBox();
            this.txtWorking = new System.Windows.Forms.TextBox();
            this.textBox1 = new System.Windows.Forms.TextBox();
            this.txtWorking2 = new System.Windows.Forms.TextBox();
            this.txtFinish2 = new System.Windows.Forms.TextBox();
            this.btnProcess2 = new System.Windows.Forms.Button();
            this.comboBox1 = new System.Windows.Forms.ComboBox();
            this.btnEnd2 = new System.Windows.Forms.Button();
            this.SuspendLayout();
            // 
            // btnWorking
            // 
            this.btnWorking.Location = new System.Drawing.Point(42, 30);
            this.btnWorking.Name = "btnWorking";
            this.btnWorking.Size = new System.Drawing.Size(247, 23);
            this.btnWorking.TabIndex = 0;
            this.btnWorking.Text = "Process";
            this.btnWorking.UseVisualStyleBackColor = true;
            this.btnWorking.Click += new System.EventHandler(this.Button1_Click);
            // 
            // txtFinish
            // 
            this.txtFinish.Location = new System.Drawing.Point(42, 306);
            this.txtFinish.Name = "txtFinish";
            this.txtFinish.ReadOnly = true;
            this.txtFinish.Size = new System.Drawing.Size(247, 20);
            this.txtFinish.TabIndex = 1;
            // 
            // txtWorking
            // 
            this.txtWorking.Location = new System.Drawing.Point(42, 71);
            this.txtWorking.Multiline = true;
            this.txtWorking.Name = "txtWorking";
            this.txtWorking.ReadOnly = true;
            this.txtWorking.ScrollBars = System.Windows.Forms.ScrollBars.Vertical;
            this.txtWorking.Size = new System.Drawing.Size(247, 229);
            this.txtWorking.TabIndex = 2;
            // 
            // textBox1
            // 
            this.textBox1.Location = new System.Drawing.Point(295, 71);
            this.textBox1.Multiline = true;
            this.textBox1.Name = "textBox1";
            this.textBox1.Size = new System.Drawing.Size(189, 109);
            this.textBox1.TabIndex = 3;
            // 
            // txtWorking2
            // 
            this.txtWorking2.Location = new System.Drawing.Point(490, 71);
            this.txtWorking2.Multiline = true;
            this.txtWorking2.Name = "txtWorking2";
            this.txtWorking2.ReadOnly = true;
            this.txtWorking2.ScrollBars = System.Windows.Forms.ScrollBars.Vertical;
            this.txtWorking2.Size = new System.Drawing.Size(247, 229);
            this.txtWorking2.TabIndex = 4;
            // 
            // txtFinish2
            // 
            this.txtFinish2.Location = new System.Drawing.Point(490, 306);
            this.txtFinish2.Name = "txtFinish2";
            this.txtFinish2.ReadOnly = true;
            this.txtFinish2.Size = new System.Drawing.Size(247, 20);
            this.txtFinish2.TabIndex = 5;
            // 
            // btnProcess2
            // 
            this.btnProcess2.Location = new System.Drawing.Point(490, 30);
            this.btnProcess2.Name = "btnProcess2";
            this.btnProcess2.Size = new System.Drawing.Size(111, 23);
            this.btnProcess2.TabIndex = 6;
            this.btnProcess2.Text = "Process";
            this.btnProcess2.UseVisualStyleBackColor = true;
            this.btnProcess2.Click += new System.EventHandler(this.BtnProcess2_Click);
            // 
            // comboBox1
            // 
            this.comboBox1.FormattingEnabled = true;
            this.comboBox1.Items.AddRange(new object[] {
            "One",
            "Two",
            "House",
            "Dog",
            "BitCoin",
            "Beach",
            "Bla"});
            this.comboBox1.Location = new System.Drawing.Point(295, 186);
            this.comboBox1.Name = "comboBox1";
            this.comboBox1.Size = new System.Drawing.Size(189, 21);
            this.comboBox1.TabIndex = 7;
            // 
            // btnEnd2
            // 
            this.btnEnd2.Location = new System.Drawing.Point(626, 30);
            this.btnEnd2.Name = "btnEnd2";
            this.btnEnd2.Size = new System.Drawing.Size(111, 23);
            this.btnEnd2.TabIndex = 8;
            this.btnEnd2.Text = "End";
            this.btnEnd2.UseVisualStyleBackColor = true;
            this.btnEnd2.Click += new System.EventHandler(this.BtnEnd2_Click);
            // 
            // Form1
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(764, 349);
            this.Controls.Add(this.btnEnd2);
            this.Controls.Add(this.comboBox1);
            this.Controls.Add(this.btnProcess2);
            this.Controls.Add(this.txtFinish2);
            this.Controls.Add(this.txtWorking2);
            this.Controls.Add(this.textBox1);
            this.Controls.Add(this.txtWorking);
            this.Controls.Add(this.txtFinish);
            this.Controls.Add(this.btnWorking);
            this.Name = "Form1";
            this.Text = "Form1";
            this.FormClosing += new System.Windows.Forms.FormClosingEventHandler(this.Form1_FormClosing);
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.Button btnWorking;
        private System.Windows.Forms.TextBox txtFinish;
        private System.Windows.Forms.TextBox txtWorking;
        private System.Windows.Forms.TextBox textBox1;
        private System.Windows.Forms.TextBox txtWorking2;
        private System.Windows.Forms.TextBox txtFinish2;
        private System.Windows.Forms.Button btnProcess2;
        private System.Windows.Forms.ComboBox comboBox1;
        private System.Windows.Forms.Button btnEnd2;
    }
}

