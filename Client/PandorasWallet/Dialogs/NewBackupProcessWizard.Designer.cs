namespace Pandora.Client.PandorasWallet.Dialogs
{
    partial class NewBackupProcessWizard
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
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(NewBackupProcessWizard));
            this.tabStepsControl = new System.Windows.Forms.TabControl();
            this.InitialTab = new System.Windows.Forms.TabPage();
            this.lblTypeProcess = new System.Windows.Forms.Label();
            this.label1 = new System.Windows.Forms.Label();
            this.SelectorTab = new System.Windows.Forms.TabPage();
            this.label3 = new System.Windows.Forms.Label();
            this.groupBox1 = new System.Windows.Forms.GroupBox();
            this.textBox2 = new System.Windows.Forms.TextBox();
            this.textBox1 = new System.Windows.Forms.TextBox();
            this.radioBtnWords = new System.Windows.Forms.RadioButton();
            this.radioBtnFile = new System.Windows.Forms.RadioButton();
            this.RestoreWordTab = new System.Windows.Forms.TabPage();
            this.label5 = new System.Windows.Forms.Label();
            this.txtBoxRecoveryWordsRestore = new System.Windows.Forms.TextBox();
            this.RestoreFileTab = new System.Windows.Forms.TabPage();
            this.btnRestoreOpenFileDialog = new System.Windows.Forms.Button();
            this.label6 = new System.Windows.Forms.Label();
            this.txtBox = new System.Windows.Forms.Label();
            this.txtBoxRestoreFilePath = new System.Windows.Forms.TextBox();
            this.BackupByFileTab = new System.Windows.Forms.TabPage();
            this.label9 = new System.Windows.Forms.Label();
            this.label8 = new System.Windows.Forms.Label();
            this.btnBackupFolder = new System.Windows.Forms.Button();
            this.txtBoxBackupSelectedDir = new System.Windows.Forms.TextBox();
            this.BackupByWordsTab = new System.Windows.Forms.TabPage();
            this.label11 = new System.Windows.Forms.Label();
            this.lblWords = new System.Windows.Forms.Label();
            this.btnCopyWords = new System.Windows.Forms.Button();
            this.label7 = new System.Windows.Forms.Label();
            this.EndTab = new System.Windows.Forms.TabPage();
            this.pictureBox2 = new System.Windows.Forms.PictureBox();
            this.lblFinish = new System.Windows.Forms.Label();
            this.btnNext = new System.Windows.Forms.Button();
            this.btnBack = new System.Windows.Forms.Button();
            this.btnCancel = new System.Windows.Forms.Button();
            this.pictureBox1 = new System.Windows.Forms.PictureBox();
            this.lblContinue = new System.Windows.Forms.Label();
            this.openFileDlg = new System.Windows.Forms.OpenFileDialog();
            this.DlgFolderBrowser = new System.Windows.Forms.FolderBrowserDialog();
            this.WizardToolTip = new System.Windows.Forms.ToolTip(this.components);
            this.autoCompleteMenu = new AutocompleteMenuNS.AutocompleteMenu();
            this.label2 = new System.Windows.Forms.Label();
            this.tabStepsControl.SuspendLayout();
            this.InitialTab.SuspendLayout();
            this.SelectorTab.SuspendLayout();
            this.groupBox1.SuspendLayout();
            this.RestoreWordTab.SuspendLayout();
            this.RestoreFileTab.SuspendLayout();
            this.BackupByFileTab.SuspendLayout();
            this.BackupByWordsTab.SuspendLayout();
            this.EndTab.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.pictureBox2)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.pictureBox1)).BeginInit();
            this.SuspendLayout();
            // 
            // tabStepsControl
            // 
            this.tabStepsControl.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.tabStepsControl.Controls.Add(this.InitialTab);
            this.tabStepsControl.Controls.Add(this.SelectorTab);
            this.tabStepsControl.Controls.Add(this.RestoreWordTab);
            this.tabStepsControl.Controls.Add(this.RestoreFileTab);
            this.tabStepsControl.Controls.Add(this.BackupByFileTab);
            this.tabStepsControl.Controls.Add(this.BackupByWordsTab);
            this.tabStepsControl.Controls.Add(this.EndTab);
            this.tabStepsControl.Location = new System.Drawing.Point(218, -7);
            this.tabStepsControl.Name = "tabStepsControl";
            this.tabStepsControl.SelectedIndex = 0;
            this.tabStepsControl.Size = new System.Drawing.Size(505, 356);
            this.tabStepsControl.TabIndex = 0;
            // 
            // InitialTab
            // 
            this.InitialTab.BackColor = System.Drawing.Color.White;
            this.InitialTab.Controls.Add(this.label2);
            this.InitialTab.Controls.Add(this.lblTypeProcess);
            this.InitialTab.Controls.Add(this.label1);
            this.InitialTab.Location = new System.Drawing.Point(4, 22);
            this.InitialTab.Name = "InitialTab";
            this.InitialTab.Padding = new System.Windows.Forms.Padding(3);
            this.InitialTab.Size = new System.Drawing.Size(497, 330);
            this.InitialTab.TabIndex = 0;
            this.InitialTab.Text = "Initial";
            // 
            // lblTypeProcess
            // 
            this.lblTypeProcess.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
            this.lblTypeProcess.BackColor = System.Drawing.Color.Transparent;
            this.lblTypeProcess.Location = new System.Drawing.Point(268, 294);
            this.lblTypeProcess.Name = "lblTypeProcess";
            this.lblTypeProcess.Size = new System.Drawing.Size(223, 13);
            this.lblTypeProcess.TabIndex = 1;
            this.lblTypeProcess.Text = "You are about to do a restore";
            this.lblTypeProcess.TextAlign = System.Drawing.ContentAlignment.MiddleRight;
            // 
            // label1
            // 
            this.label1.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.label1.Font = new System.Drawing.Font("Microsoft Sans Serif", 9.5F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.label1.Location = new System.Drawing.Point(12, 52);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(472, 116);
            this.label1.TabIndex = 0;
            this.label1.Text = "In the following steps we will be asking you how do you want to backup/restore yo" +
    "ur wallet.\r\n\r\nPlease ensure you keep in a safe place all information provided or" +
    " used in this process.\r\n";
            // 
            // SelectorTab
            // 
            this.SelectorTab.BackColor = System.Drawing.Color.White;
            this.SelectorTab.Controls.Add(this.label3);
            this.SelectorTab.Controls.Add(this.groupBox1);
            this.SelectorTab.Location = new System.Drawing.Point(4, 22);
            this.SelectorTab.Name = "SelectorTab";
            this.SelectorTab.Padding = new System.Windows.Forms.Padding(3);
            this.SelectorTab.Size = new System.Drawing.Size(497, 330);
            this.SelectorTab.TabIndex = 1;
            this.SelectorTab.Text = "Selector";
            // 
            // label3
            // 
            this.label3.Font = new System.Drawing.Font("Microsoft Sans Serif", 9.5F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.label3.Location = new System.Drawing.Point(46, 36);
            this.label3.Name = "label3";
            this.label3.Size = new System.Drawing.Size(438, 23);
            this.label3.TabIndex = 3;
            this.label3.Text = "Please select the desired method to use in the backup/restore process.";
            // 
            // groupBox1
            // 
            this.groupBox1.Controls.Add(this.textBox2);
            this.groupBox1.Controls.Add(this.textBox1);
            this.groupBox1.Controls.Add(this.radioBtnWords);
            this.groupBox1.Controls.Add(this.radioBtnFile);
            this.groupBox1.Location = new System.Drawing.Point(49, 79);
            this.groupBox1.Name = "groupBox1";
            this.groupBox1.Size = new System.Drawing.Size(420, 198);
            this.groupBox1.TabIndex = 2;
            this.groupBox1.TabStop = false;
            this.groupBox1.Text = "Choose a method:";
            // 
            // textBox2
            // 
            this.autoCompleteMenu.SetAutocompleteMenu(this.textBox2, null);
            this.textBox2.BorderStyle = System.Windows.Forms.BorderStyle.None;
            this.textBox2.Location = new System.Drawing.Point(34, 52);
            this.textBox2.Multiline = true;
            this.textBox2.Name = "textBox2";
            this.textBox2.Size = new System.Drawing.Size(355, 38);
            this.textBox2.TabIndex = 3;
            this.textBox2.Text = "This option will use a phrase of 12 or 24 words to recover your wallet keys. \r\nMa" +
    "ke sure to write them down or store them in a safe place.\r\n\r\n";
            // 
            // textBox1
            // 
            this.autoCompleteMenu.SetAutocompleteMenu(this.textBox1, null);
            this.textBox1.BorderStyle = System.Windows.Forms.BorderStyle.None;
            this.textBox1.Location = new System.Drawing.Point(34, 131);
            this.textBox1.Multiline = true;
            this.textBox1.Name = "textBox1";
            this.textBox1.Size = new System.Drawing.Size(355, 48);
            this.textBox1.TabIndex = 2;
            this.textBox1.Text = "This option will use a file in order to store your wallet data.\r\nHere you your pe" +
    "rsonal account keys are preferences are saved.\r\nDO NOT SHARE THIS FILE WITH ANYO" +
    "NE.";
            // 
            // radioBtnWords
            // 
            this.radioBtnWords.AutoSize = true;
            this.radioBtnWords.Checked = true;
            this.radioBtnWords.Font = new System.Drawing.Font("Microsoft Sans Serif", 8.25F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.radioBtnWords.Location = new System.Drawing.Point(15, 28);
            this.radioBtnWords.Name = "radioBtnWords";
            this.radioBtnWords.Size = new System.Drawing.Size(179, 17);
            this.radioBtnWords.TabIndex = 0;
            this.radioBtnWords.TabStop = true;
            this.radioBtnWords.Text = "Use wallet recovery phrase";
            this.radioBtnWords.UseVisualStyleBackColor = true;
            // 
            // radioBtnFile
            // 
            this.radioBtnFile.AutoSize = true;
            this.radioBtnFile.Font = new System.Drawing.Font("Microsoft Sans Serif", 8.25F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.radioBtnFile.Location = new System.Drawing.Point(15, 108);
            this.radioBtnFile.Name = "radioBtnFile";
            this.radioBtnFile.Size = new System.Drawing.Size(204, 17);
            this.radioBtnFile.TabIndex = 1;
            this.radioBtnFile.Text = "Use wallet recovery backup file";
            this.radioBtnFile.UseVisualStyleBackColor = true;
            // 
            // RestoreWordTab
            // 
            this.RestoreWordTab.BackColor = System.Drawing.Color.White;
            this.RestoreWordTab.Controls.Add(this.label5);
            this.RestoreWordTab.Controls.Add(this.txtBoxRecoveryWordsRestore);
            this.RestoreWordTab.Location = new System.Drawing.Point(4, 22);
            this.RestoreWordTab.Name = "RestoreWordTab";
            this.RestoreWordTab.Size = new System.Drawing.Size(497, 330);
            this.RestoreWordTab.TabIndex = 2;
            this.RestoreWordTab.Text = "RestoreWords";
            // 
            // label5
            // 
            this.label5.Font = new System.Drawing.Font("Microsoft Sans Serif", 9F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.label5.Location = new System.Drawing.Point(16, 22);
            this.label5.Name = "label5";
            this.label5.Size = new System.Drawing.Size(468, 59);
            this.label5.TabIndex = 1;
            this.label5.Text = "Please write down your wallet 12 or 24 word recovery phrase in the following box." +
    " \r\n\r\nEnsure that each word is separated by a white space.";
            // 
            // txtBoxRecoveryWordsRestore
            // 
            this.autoCompleteMenu.SetAutocompleteMenu(this.txtBoxRecoveryWordsRestore, this.autoCompleteMenu);
            this.txtBoxRecoveryWordsRestore.AutoCompleteMode = System.Windows.Forms.AutoCompleteMode.SuggestAppend;
            this.txtBoxRecoveryWordsRestore.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(224)))), ((int)(((byte)(224)))), ((int)(((byte)(224)))));
            this.txtBoxRecoveryWordsRestore.Font = new System.Drawing.Font("Microsoft Sans Serif", 10F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.txtBoxRecoveryWordsRestore.Location = new System.Drawing.Point(19, 84);
            this.txtBoxRecoveryWordsRestore.Multiline = true;
            this.txtBoxRecoveryWordsRestore.Name = "txtBoxRecoveryWordsRestore";
            this.txtBoxRecoveryWordsRestore.ShortcutsEnabled = false;
            this.txtBoxRecoveryWordsRestore.Size = new System.Drawing.Size(465, 201);
            this.txtBoxRecoveryWordsRestore.TabIndex = 0;
            this.txtBoxRecoveryWordsRestore.KeyPress += new System.Windows.Forms.KeyPressEventHandler(this.TxtBoxRecoveryWordsRestore_KeyPress);
            // 
            // RestoreFileTab
            // 
            this.RestoreFileTab.BackColor = System.Drawing.Color.White;
            this.RestoreFileTab.Controls.Add(this.btnRestoreOpenFileDialog);
            this.RestoreFileTab.Controls.Add(this.label6);
            this.RestoreFileTab.Controls.Add(this.txtBox);
            this.RestoreFileTab.Controls.Add(this.txtBoxRestoreFilePath);
            this.RestoreFileTab.Location = new System.Drawing.Point(4, 22);
            this.RestoreFileTab.Name = "RestoreFileTab";
            this.RestoreFileTab.Size = new System.Drawing.Size(497, 330);
            this.RestoreFileTab.TabIndex = 3;
            this.RestoreFileTab.Text = "RestoreByFile";
            // 
            // btnRestoreOpenFileDialog
            // 
            this.btnRestoreOpenFileDialog.Location = new System.Drawing.Point(403, 114);
            this.btnRestoreOpenFileDialog.Name = "btnRestoreOpenFileDialog";
            this.btnRestoreOpenFileDialog.Size = new System.Drawing.Size(43, 23);
            this.btnRestoreOpenFileDialog.TabIndex = 3;
            this.btnRestoreOpenFileDialog.Text = "...";
            this.btnRestoreOpenFileDialog.UseVisualStyleBackColor = true;
            this.btnRestoreOpenFileDialog.Click += new System.EventHandler(this.BtnRestoreOpenFileDialog_Click);
            // 
            // label6
            // 
            this.label6.AutoSize = true;
            this.label6.Location = new System.Drawing.Point(27, 95);
            this.label6.Name = "label6";
            this.label6.Size = new System.Drawing.Size(134, 13);
            this.label6.TabIndex = 2;
            this.label6.Text = "Select a backup file (.bkp):";
            // 
            // txtBox
            // 
            this.txtBox.Font = new System.Drawing.Font("Microsoft Sans Serif", 9F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.txtBox.Location = new System.Drawing.Point(27, 35);
            this.txtBox.Name = "txtBox";
            this.txtBox.Size = new System.Drawing.Size(340, 18);
            this.txtBox.TabIndex = 1;
            this.txtBox.Text = "Please provide a valid path to a Pandora\'s Wallet backup file.\r\n\r\n";
            // 
            // txtBoxRestoreFilePath
            // 
            this.autoCompleteMenu.SetAutocompleteMenu(this.txtBoxRestoreFilePath, null);
            this.txtBoxRestoreFilePath.Location = new System.Drawing.Point(27, 114);
            this.txtBoxRestoreFilePath.Name = "txtBoxRestoreFilePath";
            this.txtBoxRestoreFilePath.Size = new System.Drawing.Size(370, 20);
            this.txtBoxRestoreFilePath.TabIndex = 0;
            // 
            // BackupByFileTab
            // 
            this.BackupByFileTab.BackColor = System.Drawing.Color.White;
            this.BackupByFileTab.Controls.Add(this.label9);
            this.BackupByFileTab.Controls.Add(this.label8);
            this.BackupByFileTab.Controls.Add(this.btnBackupFolder);
            this.BackupByFileTab.Controls.Add(this.txtBoxBackupSelectedDir);
            this.BackupByFileTab.Location = new System.Drawing.Point(4, 22);
            this.BackupByFileTab.Name = "BackupByFileTab";
            this.BackupByFileTab.Size = new System.Drawing.Size(497, 330);
            this.BackupByFileTab.TabIndex = 4;
            this.BackupByFileTab.Text = "BackupByFile";
            // 
            // label9
            // 
            this.label9.AutoSize = true;
            this.label9.Location = new System.Drawing.Point(32, 101);
            this.label9.Name = "label9";
            this.label9.Size = new System.Drawing.Size(92, 13);
            this.label9.TabIndex = 4;
            this.label9.Text = "Select a directory:";
            // 
            // label8
            // 
            this.label8.AutoSize = true;
            this.label8.Font = new System.Drawing.Font("Microsoft Sans Serif", 9F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.label8.Location = new System.Drawing.Point(30, 39);
            this.label8.Name = "label8";
            this.label8.Size = new System.Drawing.Size(345, 15);
            this.label8.TabIndex = 3;
            this.label8.Text = "Please select a path to save your Pandora\'s Wallet backup file.";
            // 
            // btnBackupFolder
            // 
            this.btnBackupFolder.Location = new System.Drawing.Point(416, 115);
            this.btnBackupFolder.Name = "btnBackupFolder";
            this.btnBackupFolder.Size = new System.Drawing.Size(43, 23);
            this.btnBackupFolder.TabIndex = 1;
            this.btnBackupFolder.Text = "...";
            this.btnBackupFolder.UseVisualStyleBackColor = true;
            this.btnBackupFolder.Click += new System.EventHandler(this.BtnBackupFolder_Click);
            // 
            // txtBoxBackupSelectedDir
            // 
            this.autoCompleteMenu.SetAutocompleteMenu(this.txtBoxBackupSelectedDir, null);
            this.txtBoxBackupSelectedDir.Location = new System.Drawing.Point(35, 117);
            this.txtBoxBackupSelectedDir.Name = "txtBoxBackupSelectedDir";
            this.txtBoxBackupSelectedDir.ReadOnly = true;
            this.txtBoxBackupSelectedDir.Size = new System.Drawing.Size(375, 20);
            this.txtBoxBackupSelectedDir.TabIndex = 0;
            // 
            // BackupByWordsTab
            // 
            this.BackupByWordsTab.BackColor = System.Drawing.Color.White;
            this.BackupByWordsTab.Controls.Add(this.label11);
            this.BackupByWordsTab.Controls.Add(this.lblWords);
            this.BackupByWordsTab.Controls.Add(this.btnCopyWords);
            this.BackupByWordsTab.Controls.Add(this.label7);
            this.BackupByWordsTab.Location = new System.Drawing.Point(4, 22);
            this.BackupByWordsTab.Name = "BackupByWordsTab";
            this.BackupByWordsTab.Size = new System.Drawing.Size(497, 330);
            this.BackupByWordsTab.TabIndex = 5;
            this.BackupByWordsTab.Text = "BackupByWords";
            // 
            // label11
            // 
            this.label11.AutoSize = true;
            this.label11.Location = new System.Drawing.Point(51, 270);
            this.label11.Name = "label11";
            this.label11.Size = new System.Drawing.Size(89, 13);
            this.label11.TabIndex = 9;
            this.label11.Text = "Copy to clipboard";
            // 
            // lblWords
            // 
            this.lblWords.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
            this.lblWords.Font = new System.Drawing.Font("Microsoft Sans Serif", 12F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.lblWords.Location = new System.Drawing.Point(18, 102);
            this.lblWords.Name = "lblWords";
            this.lblWords.Size = new System.Drawing.Size(463, 148);
            this.lblWords.TabIndex = 8;
            this.lblWords.Text = "toss blind fall double leaf spell patch volume slight odor display spell toss bli" +
    "nd fall double leaf spell patch volume slight odor display combine";
            this.lblWords.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
            // 
            // btnCopyWords
            // 
            this.btnCopyWords.BackgroundImage = ((System.Drawing.Image)(resources.GetObject("btnCopyWords.BackgroundImage")));
            this.btnCopyWords.BackgroundImageLayout = System.Windows.Forms.ImageLayout.Stretch;
            this.btnCopyWords.Location = new System.Drawing.Point(18, 260);
            this.btnCopyWords.Name = "btnCopyWords";
            this.btnCopyWords.Size = new System.Drawing.Size(32, 32);
            this.btnCopyWords.TabIndex = 7;
            this.btnCopyWords.UseVisualStyleBackColor = true;
            this.btnCopyWords.Click += new System.EventHandler(this.BtnCopyWords_Click);
            // 
            // label7
            // 
            this.label7.Font = new System.Drawing.Font("Microsoft Sans Serif", 8.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.label7.Location = new System.Drawing.Point(22, 21);
            this.label7.Name = "label7";
            this.label7.Size = new System.Drawing.Size(459, 81);
            this.label7.TabIndex = 0;
            this.label7.Text = resources.GetString("label7.Text");
            // 
            // EndTab
            // 
            this.EndTab.BackColor = System.Drawing.Color.White;
            this.EndTab.Controls.Add(this.pictureBox2);
            this.EndTab.Controls.Add(this.lblFinish);
            this.EndTab.Location = new System.Drawing.Point(4, 22);
            this.EndTab.Name = "EndTab";
            this.EndTab.Size = new System.Drawing.Size(497, 330);
            this.EndTab.TabIndex = 6;
            this.EndTab.Text = "End";
            // 
            // pictureBox2
            // 
            this.pictureBox2.BackgroundImage = global::Pandora.Client.PandorasWallet.Properties.Resources.ok;
            this.pictureBox2.Location = new System.Drawing.Point(165, 55);
            this.pictureBox2.Name = "pictureBox2";
            this.pictureBox2.Size = new System.Drawing.Size(160, 160);
            this.pictureBox2.TabIndex = 1;
            this.pictureBox2.TabStop = false;
            // 
            // lblFinish
            // 
            this.lblFinish.Font = new System.Drawing.Font("Microsoft Sans Serif", 9.5F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.lblFinish.Location = new System.Drawing.Point(0, 235);
            this.lblFinish.Name = "lblFinish";
            this.lblFinish.Size = new System.Drawing.Size(497, 20);
            this.lblFinish.TabIndex = 0;
            this.lblFinish.Text = "You have succesfully completed the restore process.";
            this.lblFinish.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
            // 
            // btnNext
            // 
            this.btnNext.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
            this.btnNext.Location = new System.Drawing.Point(614, 359);
            this.btnNext.Name = "btnNext";
            this.btnNext.Size = new System.Drawing.Size(92, 23);
            this.btnNext.TabIndex = 100;
            this.btnNext.Text = "Next";
            this.btnNext.UseVisualStyleBackColor = true;
            this.btnNext.Click += new System.EventHandler(this.BtnNext_Click);
            // 
            // btnBack
            // 
            this.btnBack.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
            this.btnBack.Enabled = false;
            this.btnBack.Location = new System.Drawing.Point(521, 359);
            this.btnBack.Name = "btnBack";
            this.btnBack.Size = new System.Drawing.Size(87, 23);
            this.btnBack.TabIndex = 101;
            this.btnBack.Text = "Back";
            this.btnBack.UseVisualStyleBackColor = true;
            this.btnBack.Click += new System.EventHandler(this.BtnBack_Click);
            // 
            // btnCancel
            // 
            this.btnCancel.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));
            this.btnCancel.Location = new System.Drawing.Point(16, 359);
            this.btnCancel.Name = "btnCancel";
            this.btnCancel.Size = new System.Drawing.Size(91, 23);
            this.btnCancel.TabIndex = 102;
            this.btnCancel.Text = "Cancel";
            this.btnCancel.UseVisualStyleBackColor = true;
            this.btnCancel.Click += new System.EventHandler(this.BtnCancel_Click);
            // 
            // pictureBox1
            // 
            this.pictureBox1.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left)));
            this.pictureBox1.BackColor = System.Drawing.Color.WhiteSmoke;
            this.pictureBox1.BackgroundImage = ((System.Drawing.Image)(resources.GetObject("pictureBox1.BackgroundImage")));
            this.pictureBox1.BackgroundImageLayout = System.Windows.Forms.ImageLayout.Center;
            this.pictureBox1.Location = new System.Drawing.Point(-1, -1);
            this.pictureBox1.Name = "pictureBox1";
            this.pictureBox1.Size = new System.Drawing.Size(222, 350);
            this.pictureBox1.TabIndex = 3;
            this.pictureBox1.TabStop = false;
            // 
            // lblContinue
            // 
            this.lblContinue.BackColor = System.Drawing.Color.White;
            this.lblContinue.Font = new System.Drawing.Font("Microsoft Sans Serif", 8.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.lblContinue.Location = new System.Drawing.Point(557, 325);
            this.lblContinue.Name = "lblContinue";
            this.lblContinue.Size = new System.Drawing.Size(157, 16);
            this.lblContinue.TabIndex = 1;
            this.lblContinue.Text = "Click next to continue";
            this.lblContinue.TextAlign = System.Drawing.ContentAlignment.MiddleRight;
            // 
            // openFileDlg
            // 
            this.openFileDlg.FileName = "PWBackup.bkp";
            this.openFileDlg.Filter = "Backup files|*.bkp|All Files|*.*";
            this.openFileDlg.Title = "Select a file to restore";
            // 
            // autoCompleteMenu
            // 
            this.autoCompleteMenu.AllowsTabKey = true;
            this.autoCompleteMenu.AppearInterval = 250;
            this.autoCompleteMenu.Colors = ((AutocompleteMenuNS.Colors)(resources.GetObject("autoCompleteMenu.Colors")));
            this.autoCompleteMenu.Font = new System.Drawing.Font("Microsoft Sans Serif", 9F);
            this.autoCompleteMenu.ImageList = null;
            this.autoCompleteMenu.Items = new string[0];
            this.autoCompleteMenu.TargetControlWrapper = null;
            // 
            // label2
            // 
            this.label2.AutoSize = true;
            this.label2.Font = new System.Drawing.Font("Microsoft Sans Serif", 10.5F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.label2.Location = new System.Drawing.Point(14, 20);
            this.label2.Name = "label2";
            this.label2.Size = new System.Drawing.Size(385, 17);
            this.label2.TabIndex = 2;
            this.label2.Text = "Welcome to Pandora\'s Wallet backup/restore wizard";
            // 
            // NewBackupProcessWizard
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.BackColor = System.Drawing.Color.LightGray;
            this.ClientSize = new System.Drawing.Size(722, 390);
            this.Controls.Add(this.lblContinue);
            this.Controls.Add(this.pictureBox1);
            this.Controls.Add(this.btnCancel);
            this.Controls.Add(this.btnBack);
            this.Controls.Add(this.btnNext);
            this.Controls.Add(this.tabStepsControl);
            this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedSingle;
            this.Icon = ((System.Drawing.Icon)(resources.GetObject("$this.Icon")));
            this.MaximizeBox = false;
            this.Name = "NewBackupProcessWizard";
            this.StartPosition = System.Windows.Forms.FormStartPosition.CenterParent;
            this.Text = "Backup and Restore Wizard";
            this.tabStepsControl.ResumeLayout(false);
            this.InitialTab.ResumeLayout(false);
            this.InitialTab.PerformLayout();
            this.SelectorTab.ResumeLayout(false);
            this.groupBox1.ResumeLayout(false);
            this.groupBox1.PerformLayout();
            this.RestoreWordTab.ResumeLayout(false);
            this.RestoreWordTab.PerformLayout();
            this.RestoreFileTab.ResumeLayout(false);
            this.RestoreFileTab.PerformLayout();
            this.BackupByFileTab.ResumeLayout(false);
            this.BackupByFileTab.PerformLayout();
            this.BackupByWordsTab.ResumeLayout(false);
            this.BackupByWordsTab.PerformLayout();
            this.EndTab.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize)(this.pictureBox2)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.pictureBox1)).EndInit();
            this.ResumeLayout(false);

        }

        #endregion

        private System.Windows.Forms.TabControl tabStepsControl;
        private System.Windows.Forms.TabPage InitialTab;
        private System.Windows.Forms.TabPage SelectorTab;
        private System.Windows.Forms.TabPage RestoreWordTab;
        private System.Windows.Forms.TabPage RestoreFileTab;
        private System.Windows.Forms.TabPage BackupByFileTab;
        private System.Windows.Forms.TabPage BackupByWordsTab;
        private System.Windows.Forms.TabPage EndTab;
        private System.Windows.Forms.Button btnNext;
        private System.Windows.Forms.Button btnBack;
        private System.Windows.Forms.Button btnCancel;
        private System.Windows.Forms.PictureBox pictureBox1;
        private System.Windows.Forms.Label label1;
        private System.Windows.Forms.Label lblContinue;
        private System.Windows.Forms.Label lblTypeProcess;
        private System.Windows.Forms.Label label3;
        private System.Windows.Forms.GroupBox groupBox1;
        private System.Windows.Forms.RadioButton radioBtnWords;
        private System.Windows.Forms.RadioButton radioBtnFile;
        private System.Windows.Forms.Label label5;
        private System.Windows.Forms.TextBox txtBoxRecoveryWordsRestore;
        private System.Windows.Forms.Button btnRestoreOpenFileDialog;
        private System.Windows.Forms.Label label6;
        private System.Windows.Forms.Label txtBox;
        private System.Windows.Forms.TextBox txtBoxRestoreFilePath;
        private System.Windows.Forms.OpenFileDialog openFileDlg;
        private System.Windows.Forms.Label label9;
        private System.Windows.Forms.Label label8;
        private System.Windows.Forms.Button btnBackupFolder;
        private System.Windows.Forms.TextBox txtBoxBackupSelectedDir;
        private System.Windows.Forms.FolderBrowserDialog DlgFolderBrowser;
        private System.Windows.Forms.Label label7;
        private System.Windows.Forms.Label lblWords;
        private System.Windows.Forms.Button btnCopyWords;
        private System.Windows.Forms.Label label11;
        private System.Windows.Forms.ToolTip WizardToolTip;
        private System.Windows.Forms.PictureBox pictureBox2;
        private System.Windows.Forms.Label lblFinish;
        private AutocompleteMenuNS.AutocompleteMenu autoCompleteMenu;
        private System.Windows.Forms.TextBox textBox2;
        private System.Windows.Forms.TextBox textBox1;
        private System.Windows.Forms.Label label2;
    }
}