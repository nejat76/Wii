namespace WiiGSC
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
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(Form1));
            this.statusStrip1 = new System.Windows.Forms.StatusStrip();
            this.toolStripProgressBar1 = new System.Windows.Forms.ToolStripProgressBar();
            this.toolStripStatusLabel1 = new System.Windows.Forms.ToolStripStatusLabel();
            this.openFileDialog1 = new System.Windows.Forms.OpenFileDialog();
            this.openSaveFileDialog = new System.Windows.Forms.OpenFileDialog();
            this.openSaveFileDlg = new System.Windows.Forms.FolderBrowserDialog();
            this.openFileDialog2 = new System.Windows.Forms.OpenFileDialog();
            this.panelPartition = new System.Windows.Forms.Panel();
            this.lstExtraParameters = new System.Windows.Forms.ListBox();
            this.label26 = new System.Windows.Forms.Label();
            this.txtExtraParameters = new System.Windows.Forms.TextBox();
            this.label25 = new System.Windows.Forms.Label();
            this.cmbPartition = new System.Windows.Forms.ComboBox();
            this.panelOptions = new System.Windows.Forms.Panel();
            this.label17 = new System.Windows.Forms.Label();
            this.cmbLoaderType = new System.Windows.Forms.ComboBox();
            this.cmbDolList = new System.Windows.Forms.ComboBox();
            this.label20 = new System.Windows.Forms.Label();
            this.groupBox2 = new System.Windows.Forms.GroupBox();
            this.chkNewStyle002Fix = new System.Windows.Forms.CheckBox();
            this.chkOldStyle002Fix = new System.Windows.Forms.CheckBox();
            this.chkAnti002Fix = new System.Windows.Forms.CheckBox();
            this.label22 = new System.Windows.Forms.Label();
            this.cmbAltDolType = new System.Windows.Forms.ComboBox();
            this.chkVerbose = new System.Windows.Forms.CheckBox();
            this.cmbRegion = new System.Windows.Forms.ComboBox();
            this.label1 = new System.Windows.Forms.Label();
            this.chkForceVideoMode = new System.Windows.Forms.CheckBox();
            this.cmbLanguage = new System.Windows.Forms.ComboBox();
            this.chkOcarinaSupport = new System.Windows.Forms.CheckBox();
            this.label12 = new System.Windows.Forms.Label();
            this.panelWBFS = new System.Windows.Forms.Panel();
            this.button2 = new System.Windows.Forms.Button();
            this.groupBox3 = new System.Windows.Forms.GroupBox();
            this.listGames = new System.Windows.Forms.ListView();
            this.id = new System.Windows.Forms.ColumnHeader();
            this.code = new System.Windows.Forms.ColumnHeader();
            this.name = new System.Windows.Forms.ColumnHeader();
            this.size = new System.Windows.Forms.ColumnHeader();
            this.path = new System.Windows.Forms.ColumnHeader();
            this.btnRefresh = new System.Windows.Forms.Button();
            this.button4 = new System.Windows.Forms.Button();
            this.cmbDriveList = new System.Windows.Forms.ComboBox();
            this.btnCreateSelected = new System.Windows.Forms.Button();
            this.button3 = new System.Windows.Forms.Button();
            this.label23 = new System.Windows.Forms.Label();
            this.panelBatch = new System.Windows.Forms.Panel();
            this.listBox1 = new System.Windows.Forms.ListBox();
            this.btnBatchCreate = new System.Windows.Forms.Button();
            this.btnDismiss = new System.Windows.Forms.Button();
            this.groupBox4 = new System.Windows.Forms.GroupBox();
            this.lblChannel = new System.Windows.Forms.Label();
            this.btnChannelSelect = new System.Windows.Forms.Button();
            this.txtChannel = new System.Windows.Forms.TextBox();
            this.lblFileName = new System.Windows.Forms.Label();
            this.txtDataFile = new System.Windows.Forms.TextBox();
            this.button1 = new System.Windows.Forms.Button();
            this.btnBrowse = new System.Windows.Forms.Button();
            this.label19 = new System.Windows.Forms.Label();
            this.txtIsoFile = new System.Windows.Forms.TextBox();
            this.btnSelectIso = new System.Windows.Forms.Button();
            this.label2 = new System.Windows.Forms.Label();
            this.groupBox1 = new System.Windows.Forms.GroupBox();
            this.radBtn3 = new System.Windows.Forms.RadioButton();
            this.radBtn2 = new System.Windows.Forms.RadioButton();
            this.radBtn1 = new System.Windows.Forms.RadioButton();
            this.lblGameName = new System.Windows.Forms.Label();
            this.lblGameNameLbl = new System.Windows.Forms.Label();
            this.lblSdCardSupport = new System.Windows.Forms.Label();
            this.label16 = new System.Windows.Forms.Label();
            this.lblForceLanguageSupport = new System.Windows.Forms.Label();
            this.lblOcarinaSupport = new System.Windows.Forms.Label();
            this.lblForceVideoModeSupport = new System.Windows.Forms.Label();
            this.label15 = new System.Windows.Forms.Label();
            this.label14 = new System.Windows.Forms.Label();
            this.label13 = new System.Windows.Forms.Label();
            this.btnTest = new System.Windows.Forms.Button();
            this.lblVerboseLog = new System.Windows.Forms.Label();
            this.label11 = new System.Windows.Forms.Label();
            this.lblDolFilename = new System.Windows.Forms.Label();
            this.label10 = new System.Windows.Forms.Label();
            this.lblConfigPlaceholder = new System.Windows.Forms.Label();
            this.lblDefaultDiscId = new System.Windows.Forms.Label();
            this.lblRegionOverride = new System.Windows.Forms.Label();
            this.lblModder = new System.Windows.Forms.Label();
            this.lblAuthor = new System.Windows.Forms.Label();
            this.label9 = new System.Windows.Forms.Label();
            this.label8 = new System.Windows.Forms.Label();
            this.label7 = new System.Windows.Forms.Label();
            this.label6 = new System.Windows.Forms.Label();
            this.label5 = new System.Windows.Forms.Label();
            this.lblLoader = new System.Windows.Forms.Label();
            this.cmbLoaders = new System.Windows.Forms.ComboBox();
            this.btnCreate = new System.Windows.Forms.Button();
            this.txtDiscId = new System.Windows.Forms.Label();
            this.label4 = new System.Windows.Forms.Label();
            this.txtTitleId = new System.Windows.Forms.TextBox();
            this.label3 = new System.Windows.Forms.Label();
            this.label18 = new System.Windows.Forms.Label();
            this.lblError002Fix = new System.Windows.Forms.Label();
            this.label21 = new System.Windows.Forms.Label();
            this.lblAltDolSupport = new System.Windows.Forms.Label();
            this.menuStrip1 = new System.Windows.Forms.MenuStrip();
            this.openBannerToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.openChannelWADToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.openISOToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.wBFSDriveToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.viewWBFSDriveToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.exitToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.languageToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.englishToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.turToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.germanToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.toolStripMenuItem1 = new System.Windows.Forms.ToolStripMenuItem();
            this.frenchToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.spanishToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.ıtalianToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.portoqueseToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.japaneseToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.sChineseToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.tChineseToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.koreanToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.russianToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.dutchToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.finnishToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.swedishToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.danishToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.configureToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.ipAddressToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.updatesToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.helpToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.ınformationToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.officialSiteToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.officialDiscussionToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.donationToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.statusStrip1.SuspendLayout();
            this.panelPartition.SuspendLayout();
            this.panelOptions.SuspendLayout();
            this.groupBox2.SuspendLayout();
            this.panelWBFS.SuspendLayout();
            this.groupBox3.SuspendLayout();
            this.panelBatch.SuspendLayout();
            this.groupBox4.SuspendLayout();
            this.groupBox1.SuspendLayout();
            this.menuStrip1.SuspendLayout();
            this.SuspendLayout();
            // 
            // statusStrip1
            // 
            this.statusStrip1.Items.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.toolStripProgressBar1,
            this.toolStripStatusLabel1});
            this.statusStrip1.Location = new System.Drawing.Point(0, 520);
            this.statusStrip1.Name = "statusStrip1";
            this.statusStrip1.Size = new System.Drawing.Size(820, 22);
            this.statusStrip1.TabIndex = 133;
            this.statusStrip1.Text = "statusStrip1";
            // 
            // toolStripProgressBar1
            // 
            this.toolStripProgressBar1.Name = "toolStripProgressBar1";
            this.toolStripProgressBar1.Size = new System.Drawing.Size(100, 16);
            // 
            // toolStripStatusLabel1
            // 
            this.toolStripStatusLabel1.Name = "toolStripStatusLabel1";
            this.toolStripStatusLabel1.Size = new System.Drawing.Size(0, 17);
            // 
            // openFileDialog1
            // 
            this.openFileDialog1.DefaultExt = "iso";
            this.openFileDialog1.Filter = "ISO files|*.iso";
            // 
            // openSaveFileDialog
            // 
            this.openSaveFileDialog.DefaultExt = "bnr";
            this.openSaveFileDialog.FileName = "*.bnr";
            this.openSaveFileDialog.Filter = "Disc Banner Files|*.bnr|Channel Banner Files|*.cbnr";
            // 
            // openFileDialog2
            // 
            this.openFileDialog2.DefaultExt = "wad";
            this.openFileDialog2.Filter = "Channel files|*.wad";
            // 
            // panelPartition
            // 
            this.panelPartition.Controls.Add(this.lstExtraParameters);
            this.panelPartition.Controls.Add(this.label26);
            this.panelPartition.Controls.Add(this.txtExtraParameters);
            this.panelPartition.Controls.Add(this.label25);
            this.panelPartition.Controls.Add(this.cmbPartition);
            this.panelPartition.Location = new System.Drawing.Point(1, 186);
            this.panelPartition.Name = "panelPartition";
            this.panelPartition.Size = new System.Drawing.Size(303, 213);
            this.panelPartition.TabIndex = 132;
            this.panelPartition.Visible = false;
            // 
            // lstExtraParameters
            // 
            this.lstExtraParameters.FormattingEnabled = true;
            this.lstExtraParameters.Location = new System.Drawing.Point(17, 78);
            this.lstExtraParameters.Name = "lstExtraParameters";
            this.lstExtraParameters.Size = new System.Drawing.Size(265, 108);
            this.lstExtraParameters.TabIndex = 4;
            this.lstExtraParameters.DoubleClick += new System.EventHandler(this.lstExtraParameters_DoubleClick);
            // 
            // label26
            // 
            this.label26.Location = new System.Drawing.Point(14, 49);
            this.label26.Name = "label26";
            this.label26.Size = new System.Drawing.Size(100, 16);
            this.label26.TabIndex = 3;
            this.label26.Text = "Extra parameters : ";
            this.label26.TextAlign = System.Drawing.ContentAlignment.TopRight;
            // 
            // txtExtraParameters
            // 
            this.txtExtraParameters.Location = new System.Drawing.Point(114, 45);
            this.txtExtraParameters.Name = "txtExtraParameters";
            this.txtExtraParameters.Size = new System.Drawing.Size(168, 20);
            this.txtExtraParameters.TabIndex = 2;
            // 
            // label25
            // 
            this.label25.Location = new System.Drawing.Point(5, 21);
            this.label25.Name = "label25";
            this.label25.Size = new System.Drawing.Size(109, 16);
            this.label25.TabIndex = 1;
            this.label25.Text = "Game Partition : ";
            this.label25.TextAlign = System.Drawing.ContentAlignment.TopRight;
            // 
            // cmbPartition
            // 
            this.cmbPartition.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.cmbPartition.FormattingEnabled = true;
            this.cmbPartition.Items.AddRange(new object[] {
            "0 - wbfs1",
            "1 - wbfs2",
            "2 - wbfs3",
            "3 - wbfs4",
            "4 - fat1",
            "5 - fat2",
            "6 - fat3",
            "7 - fat4",
            "8 - ntfs1",
            "9 - ntfs2",
            "A - ntfs3",
            "B - ntfs4 "});
            this.cmbPartition.Location = new System.Drawing.Point(114, 17);
            this.cmbPartition.Name = "cmbPartition";
            this.cmbPartition.Size = new System.Drawing.Size(168, 21);
            this.cmbPartition.TabIndex = 0;
            this.cmbPartition.SelectedIndexChanged += new System.EventHandler(this.cmbPartition_SelectedIndexChanged);
            // 
            // panelOptions
            // 
            this.panelOptions.Controls.Add(this.label17);
            this.panelOptions.Controls.Add(this.cmbLoaderType);
            this.panelOptions.Controls.Add(this.cmbDolList);
            this.panelOptions.Controls.Add(this.label20);
            this.panelOptions.Controls.Add(this.groupBox2);
            this.panelOptions.Controls.Add(this.label22);
            this.panelOptions.Controls.Add(this.cmbAltDolType);
            this.panelOptions.Controls.Add(this.chkVerbose);
            this.panelOptions.Controls.Add(this.cmbRegion);
            this.panelOptions.Controls.Add(this.label1);
            this.panelOptions.Controls.Add(this.chkForceVideoMode);
            this.panelOptions.Controls.Add(this.cmbLanguage);
            this.panelOptions.Controls.Add(this.chkOcarinaSupport);
            this.panelOptions.Controls.Add(this.label12);
            this.panelOptions.Location = new System.Drawing.Point(1, 186);
            this.panelOptions.Name = "panelOptions";
            this.panelOptions.Size = new System.Drawing.Size(306, 213);
            this.panelOptions.TabIndex = 131;
            // 
            // label17
            // 
            this.label17.Location = new System.Drawing.Point(7, 4);
            this.label17.Name = "label17";
            this.label17.Size = new System.Drawing.Size(106, 13);
            this.label17.TabIndex = 55;
            this.label17.Text = "Type :";
            this.label17.TextAlign = System.Drawing.ContentAlignment.TopRight;
            // 
            // cmbLoaderType
            // 
            this.cmbLoaderType.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.cmbLoaderType.Enabled = false;
            this.cmbLoaderType.FormattingEnabled = true;
            this.cmbLoaderType.Items.AddRange(new object[] {
            "USB Loader",
            "SD/SDHC Loader"});
            this.cmbLoaderType.Location = new System.Drawing.Point(114, 1);
            this.cmbLoaderType.Name = "cmbLoaderType";
            this.cmbLoaderType.Size = new System.Drawing.Size(121, 21);
            this.cmbLoaderType.TabIndex = 54;
            // 
            // cmbDolList
            // 
            this.cmbDolList.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.cmbDolList.Enabled = false;
            this.cmbDolList.FormattingEnabled = true;
            this.cmbDolList.Location = new System.Drawing.Point(114, 25);
            this.cmbDolList.Name = "cmbDolList";
            this.cmbDolList.Size = new System.Drawing.Size(121, 21);
            this.cmbDolList.TabIndex = 72;
            // 
            // label20
            // 
            this.label20.Location = new System.Drawing.Point(5, 28);
            this.label20.Name = "label20";
            this.label20.Size = new System.Drawing.Size(108, 13);
            this.label20.TabIndex = 73;
            this.label20.Text = "Alt. Dol List :";
            this.label20.TextAlign = System.Drawing.ContentAlignment.TopRight;
            // 
            // groupBox2
            // 
            this.groupBox2.Controls.Add(this.chkNewStyle002Fix);
            this.groupBox2.Controls.Add(this.chkOldStyle002Fix);
            this.groupBox2.Controls.Add(this.chkAnti002Fix);
            this.groupBox2.Location = new System.Drawing.Point(10, 125);
            this.groupBox2.Name = "groupBox2";
            this.groupBox2.Size = new System.Drawing.Size(293, 34);
            this.groupBox2.TabIndex = 74;
            this.groupBox2.TabStop = false;
            this.groupBox2.Text = "Fixes";
            // 
            // chkNewStyle002Fix
            // 
            this.chkNewStyle002Fix.AutoSize = true;
            this.chkNewStyle002Fix.Enabled = false;
            this.chkNewStyle002Fix.Location = new System.Drawing.Point(108, 14);
            this.chkNewStyle002Fix.Name = "chkNewStyle002Fix";
            this.chkNewStyle002Fix.Size = new System.Drawing.Size(90, 17);
            this.chkNewStyle002Fix.TabIndex = 67;
            this.chkNewStyle002Fix.Text = "Newstyle 002";
            this.chkNewStyle002Fix.UseVisualStyleBackColor = true;
            // 
            // chkOldStyle002Fix
            // 
            this.chkOldStyle002Fix.AutoSize = true;
            this.chkOldStyle002Fix.Enabled = false;
            this.chkOldStyle002Fix.Location = new System.Drawing.Point(25, 14);
            this.chkOldStyle002Fix.Name = "chkOldStyle002Fix";
            this.chkOldStyle002Fix.Size = new System.Drawing.Size(84, 17);
            this.chkOldStyle002Fix.TabIndex = 64;
            this.chkOldStyle002Fix.Text = "Oldstyle 002";
            this.chkOldStyle002Fix.UseVisualStyleBackColor = true;
            // 
            // chkAnti002Fix
            // 
            this.chkAnti002Fix.AutoSize = true;
            this.chkAnti002Fix.Enabled = false;
            this.chkAnti002Fix.Location = new System.Drawing.Point(198, 14);
            this.chkAnti002Fix.Name = "chkAnti002Fix";
            this.chkAnti002Fix.Size = new System.Drawing.Size(86, 17);
            this.chkAnti002Fix.TabIndex = 68;
            this.chkAnti002Fix.Text = "Anti 002       ";
            this.chkAnti002Fix.UseVisualStyleBackColor = true;
            // 
            // label22
            // 
            this.label22.Location = new System.Drawing.Point(5, 53);
            this.label22.Name = "label22";
            this.label22.Size = new System.Drawing.Size(108, 13);
            this.label22.TabIndex = 80;
            this.label22.Text = "Alt Dol Type :";
            this.label22.TextAlign = System.Drawing.ContentAlignment.TopRight;
            // 
            // cmbAltDolType
            // 
            this.cmbAltDolType.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.cmbAltDolType.FormattingEnabled = true;
            this.cmbAltDolType.Items.AddRange(new object[] {
            "Don\'t use Alt-dol",
            "Alt dol from NAND",
            "Alt dol from SD",
            "Alt dol from DISC"});
            this.cmbAltDolType.Location = new System.Drawing.Point(114, 50);
            this.cmbAltDolType.Name = "cmbAltDolType";
            this.cmbAltDolType.Size = new System.Drawing.Size(121, 21);
            this.cmbAltDolType.TabIndex = 79;
            // 
            // chkVerbose
            // 
            this.chkVerbose.AutoSize = true;
            this.chkVerbose.Enabled = false;
            this.chkVerbose.Location = new System.Drawing.Point(114, 78);
            this.chkVerbose.Name = "chkVerbose";
            this.chkVerbose.Size = new System.Drawing.Size(159, 17);
            this.chkVerbose.TabIndex = 19;
            this.chkVerbose.Text = "Verbose output in the loader";
            this.chkVerbose.UseVisualStyleBackColor = true;
            // 
            // cmbRegion
            // 
            this.cmbRegion.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.cmbRegion.Enabled = false;
            this.cmbRegion.FormattingEnabled = true;
            this.cmbRegion.Location = new System.Drawing.Point(115, 161);
            this.cmbRegion.Name = "cmbRegion";
            this.cmbRegion.Size = new System.Drawing.Size(71, 21);
            this.cmbRegion.TabIndex = 20;
            // 
            // label1
            // 
            this.label1.Location = new System.Drawing.Point(5, 164);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(108, 13);
            this.label1.TabIndex = 21;
            this.label1.Text = "Region Override :";
            this.label1.TextAlign = System.Drawing.ContentAlignment.TopRight;
            // 
            // chkForceVideoMode
            // 
            this.chkForceVideoMode.AutoSize = true;
            this.chkForceVideoMode.Enabled = false;
            this.chkForceVideoMode.Location = new System.Drawing.Point(114, 94);
            this.chkForceVideoMode.Name = "chkForceVideoMode";
            this.chkForceVideoMode.Size = new System.Drawing.Size(217, 17);
            this.chkForceVideoMode.TabIndex = 42;
            this.chkForceVideoMode.Text = "Force to console video mode                  ";
            this.chkForceVideoMode.UseVisualStyleBackColor = true;
            // 
            // cmbLanguage
            // 
            this.cmbLanguage.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.cmbLanguage.Enabled = false;
            this.cmbLanguage.FormattingEnabled = true;
            this.cmbLanguage.Items.AddRange(new object[] {
            "0 - System Default",
            "1- Japanese",
            "2- English",
            "3- German",
            "4- French",
            "5- Spanish",
            "6- Italian",
            "7- Dutch",
            "8- S.Chinese",
            "9- T.Chinese",
            "A- Korean",
            "B- Turkish (just joking!)"});
            this.cmbLanguage.Location = new System.Drawing.Point(115, 186);
            this.cmbLanguage.Name = "cmbLanguage";
            this.cmbLanguage.Size = new System.Drawing.Size(121, 21);
            this.cmbLanguage.TabIndex = 43;
            // 
            // chkOcarinaSupport
            // 
            this.chkOcarinaSupport.AutoSize = true;
            this.chkOcarinaSupport.Enabled = false;
            this.chkOcarinaSupport.Location = new System.Drawing.Point(114, 110);
            this.chkOcarinaSupport.Name = "chkOcarinaSupport";
            this.chkOcarinaSupport.Size = new System.Drawing.Size(99, 17);
            this.chkOcarinaSupport.TabIndex = 51;
            this.chkOcarinaSupport.Text = "Enable Ocarina";
            this.chkOcarinaSupport.UseVisualStyleBackColor = true;
            // 
            // label12
            // 
            this.label12.Location = new System.Drawing.Point(4, 189);
            this.label12.Name = "label12";
            this.label12.Size = new System.Drawing.Size(109, 13);
            this.label12.TabIndex = 44;
            this.label12.Text = "Language :";
            this.label12.TextAlign = System.Drawing.ContentAlignment.TopRight;
            // 
            // panelWBFS
            // 
            this.panelWBFS.Controls.Add(this.button2);
            this.panelWBFS.Controls.Add(this.groupBox3);
            this.panelWBFS.Controls.Add(this.btnRefresh);
            this.panelWBFS.Controls.Add(this.button4);
            this.panelWBFS.Controls.Add(this.cmbDriveList);
            this.panelWBFS.Controls.Add(this.btnCreateSelected);
            this.panelWBFS.Controls.Add(this.button3);
            this.panelWBFS.Controls.Add(this.label23);
            this.panelWBFS.Location = new System.Drawing.Point(323, 126);
            this.panelWBFS.Name = "panelWBFS";
            this.panelWBFS.Size = new System.Drawing.Size(483, 380);
            this.panelWBFS.TabIndex = 127;
            this.panelWBFS.Visible = false;
            // 
            // button2
            // 
            this.button2.Location = new System.Drawing.Point(131, 354);
            this.button2.Name = "button2";
            this.button2.Size = new System.Drawing.Size(235, 23);
            this.button2.TabIndex = 7;
            this.button2.Text = "And justice for all";
            this.button2.UseVisualStyleBackColor = true;
            this.button2.Click += new System.EventHandler(this.button2_Click_1);
            // 
            // groupBox3
            // 
            this.groupBox3.Controls.Add(this.listGames);
            this.groupBox3.Location = new System.Drawing.Point(15, 30);
            this.groupBox3.Name = "groupBox3";
            this.groupBox3.Size = new System.Drawing.Size(459, 293);
            this.groupBox3.TabIndex = 2;
            this.groupBox3.TabStop = false;
            this.groupBox3.Text = "Game List";
            // 
            // listGames
            // 
            this.listGames.Columns.AddRange(new System.Windows.Forms.ColumnHeader[] {
            this.id,
            this.code,
            this.name,
            this.size,
            this.path});
            this.listGames.FullRowSelect = true;
            this.listGames.Location = new System.Drawing.Point(7, 19);
            this.listGames.Name = "listGames";
            this.listGames.Size = new System.Drawing.Size(441, 263);
            this.listGames.TabIndex = 1;
            this.listGames.UseCompatibleStateImageBehavior = false;
            this.listGames.View = System.Windows.Forms.View.Details;
            // 
            // id
            // 
            this.id.Tag = "id";
            this.id.Text = "#";
            this.id.Width = 31;
            // 
            // code
            // 
            this.code.Tag = "code";
            this.code.Text = "Disc Id";
            // 
            // name
            // 
            this.name.Tag = "name";
            this.name.Text = "Name";
            this.name.Width = 242;
            // 
            // size
            // 
            this.size.Tag = "size";
            this.size.Text = "Size";
            this.size.Width = 102;
            // 
            // path
            // 
            this.path.Text = "Path";
            // 
            // btnRefresh
            // 
            this.btnRefresh.Location = new System.Drawing.Point(223, 3);
            this.btnRefresh.Name = "btnRefresh";
            this.btnRefresh.Size = new System.Drawing.Size(171, 23);
            this.btnRefresh.TabIndex = 4;
            this.btnRefresh.Text = "Refresh drive list";
            this.btnRefresh.UseVisualStyleBackColor = true;
            this.btnRefresh.Click += new System.EventHandler(this.btnRefresh_Click_1);
            // 
            // button4
            // 
            this.button4.Location = new System.Drawing.Point(256, 329);
            this.button4.Name = "button4";
            this.button4.Size = new System.Drawing.Size(207, 23);
            this.button4.TabIndex = 6;
            this.button4.Text = "Hide WBFS Selection";
            this.button4.UseVisualStyleBackColor = true;
            this.button4.Click += new System.EventHandler(this.button4_Click_1);
            // 
            // cmbDriveList
            // 
            this.cmbDriveList.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.cmbDriveList.FormattingEnabled = true;
            this.cmbDriveList.Location = new System.Drawing.Point(131, 3);
            this.cmbDriveList.Name = "cmbDriveList";
            this.cmbDriveList.Size = new System.Drawing.Size(79, 21);
            this.cmbDriveList.TabIndex = 0;
            this.cmbDriveList.SelectedIndexChanged += new System.EventHandler(this.cmbDriveList_SelectedIndexChanged);
            // 
            // btnCreateSelected
            // 
            this.btnCreateSelected.Location = new System.Drawing.Point(22, 329);
            this.btnCreateSelected.Name = "btnCreateSelected";
            this.btnCreateSelected.Size = new System.Drawing.Size(207, 23);
            this.btnCreateSelected.TabIndex = 5;
            this.btnCreateSelected.Text = "Use for Channel Creation";
            this.btnCreateSelected.UseVisualStyleBackColor = true;
            this.btnCreateSelected.Click += new System.EventHandler(this.btnCreateSelected_Click_1);
            // 
            // button3
            // 
            this.button3.Location = new System.Drawing.Point(400, 3);
            this.button3.Name = "button3";
            this.button3.Size = new System.Drawing.Size(74, 23);
            this.button3.TabIndex = 3;
            this.button3.Text = "Get List";
            this.button3.UseVisualStyleBackColor = true;
            this.button3.Visible = false;
            this.button3.Click += new System.EventHandler(this.button3_Click);
            // 
            // label23
            // 
            this.label23.AutoSize = true;
            this.label23.Location = new System.Drawing.Point(12, 6);
            this.label23.Name = "label23";
            this.label23.Size = new System.Drawing.Size(89, 13);
            this.label23.TabIndex = 1;
            this.label23.Text = "Select drive letter";
            this.label23.TextAlign = System.Drawing.ContentAlignment.TopRight;
            // 
            // panelBatch
            // 
            this.panelBatch.Controls.Add(this.listBox1);
            this.panelBatch.Controls.Add(this.btnBatchCreate);
            this.panelBatch.Controls.Add(this.btnDismiss);
            this.panelBatch.Location = new System.Drawing.Point(337, 126);
            this.panelBatch.Name = "panelBatch";
            this.panelBatch.Size = new System.Drawing.Size(479, 375);
            this.panelBatch.TabIndex = 130;
            this.panelBatch.Visible = false;
            // 
            // listBox1
            // 
            this.listBox1.Font = new System.Drawing.Font("Microsoft Sans Serif", 6.75F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(162)));
            this.listBox1.FormattingEnabled = true;
            this.listBox1.ItemHeight = 12;
            this.listBox1.Location = new System.Drawing.Point(3, 12);
            this.listBox1.Name = "listBox1";
            this.listBox1.Size = new System.Drawing.Size(445, 292);
            this.listBox1.TabIndex = 56;
            this.listBox1.Visible = false;
            // 
            // btnBatchCreate
            // 
            this.btnBatchCreate.Location = new System.Drawing.Point(71, 329);
            this.btnBatchCreate.Name = "btnBatchCreate";
            this.btnBatchCreate.Size = new System.Drawing.Size(111, 23);
            this.btnBatchCreate.TabIndex = 57;
            this.btnBatchCreate.Text = "Batch Create";
            this.btnBatchCreate.UseVisualStyleBackColor = true;
            this.btnBatchCreate.Visible = false;
            this.btnBatchCreate.Click += new System.EventHandler(this.btnBatchCreate_Click);
            // 
            // btnDismiss
            // 
            this.btnDismiss.Location = new System.Drawing.Point(222, 329);
            this.btnDismiss.Name = "btnDismiss";
            this.btnDismiss.Size = new System.Drawing.Size(75, 23);
            this.btnDismiss.TabIndex = 62;
            this.btnDismiss.Text = "Dismiss";
            this.btnDismiss.UseVisualStyleBackColor = true;
            this.btnDismiss.Visible = false;
            this.btnDismiss.Click += new System.EventHandler(this.btnDismiss_Click);
            // 
            // groupBox4
            // 
            this.groupBox4.BackColor = System.Drawing.SystemColors.Control;
            this.groupBox4.Controls.Add(this.lblChannel);
            this.groupBox4.Controls.Add(this.btnChannelSelect);
            this.groupBox4.Controls.Add(this.txtChannel);
            this.groupBox4.Controls.Add(this.lblFileName);
            this.groupBox4.Controls.Add(this.txtDataFile);
            this.groupBox4.Controls.Add(this.button1);
            this.groupBox4.Controls.Add(this.btnBrowse);
            this.groupBox4.Controls.Add(this.label19);
            this.groupBox4.Controls.Add(this.txtIsoFile);
            this.groupBox4.Controls.Add(this.btnSelectIso);
            this.groupBox4.Location = new System.Drawing.Point(8, 26);
            this.groupBox4.Name = "groupBox4";
            this.groupBox4.Size = new System.Drawing.Size(798, 75);
            this.groupBox4.TabIndex = 128;
            this.groupBox4.TabStop = false;
            this.groupBox4.Text = "Source";
            // 
            // lblChannel
            // 
            this.lblChannel.Location = new System.Drawing.Point(378, 22);
            this.lblChannel.Name = "lblChannel";
            this.lblChannel.Size = new System.Drawing.Size(62, 16);
            this.lblChannel.TabIndex = 84;
            this.lblChannel.Text = "Channel :";
            this.lblChannel.TextAlign = System.Drawing.ContentAlignment.TopRight;
            // 
            // btnChannelSelect
            // 
            this.btnChannelSelect.Location = new System.Drawing.Point(624, 18);
            this.btnChannelSelect.Name = "btnChannelSelect";
            this.btnChannelSelect.Size = new System.Drawing.Size(43, 23);
            this.btnChannelSelect.TabIndex = 83;
            this.btnChannelSelect.Text = "...";
            this.btnChannelSelect.UseVisualStyleBackColor = true;
            this.btnChannelSelect.Click += new System.EventHandler(this.btnChannelSelect_Click);
            // 
            // txtChannel
            // 
            this.txtChannel.Enabled = false;
            this.txtChannel.Location = new System.Drawing.Point(446, 19);
            this.txtChannel.Name = "txtChannel";
            this.txtChannel.Size = new System.Drawing.Size(172, 20);
            this.txtChannel.TabIndex = 82;
            // 
            // lblFileName
            // 
            this.lblFileName.Location = new System.Drawing.Point(25, 21);
            this.lblFileName.Margin = new System.Windows.Forms.Padding(1, 0, 1, 0);
            this.lblFileName.Name = "lblFileName";
            this.lblFileName.Size = new System.Drawing.Size(74, 13);
            this.lblFileName.TabIndex = 1;
            this.lblFileName.Text = "Banner :";
            this.lblFileName.TextAlign = System.Drawing.ContentAlignment.TopRight;
            // 
            // txtDataFile
            // 
            this.txtDataFile.Enabled = false;
            this.txtDataFile.Location = new System.Drawing.Point(102, 18);
            this.txtDataFile.Name = "txtDataFile";
            this.txtDataFile.Size = new System.Drawing.Size(222, 20);
            this.txtDataFile.TabIndex = 0;
            // 
            // button1
            // 
            this.button1.Location = new System.Drawing.Point(446, 44);
            this.button1.Name = "button1";
            this.button1.Size = new System.Drawing.Size(221, 23);
            this.button1.TabIndex = 81;
            this.button1.Text = "Open WBFS Drive";
            this.button1.UseVisualStyleBackColor = true;
            this.button1.Click += new System.EventHandler(this.button1_Click_1);
            // 
            // btnBrowse
            // 
            this.btnBrowse.Location = new System.Drawing.Point(330, 17);
            this.btnBrowse.Name = "btnBrowse";
            this.btnBrowse.Size = new System.Drawing.Size(33, 22);
            this.btnBrowse.TabIndex = 4;
            this.btnBrowse.Text = "...";
            this.btnBrowse.UseVisualStyleBackColor = true;
            this.btnBrowse.Click += new System.EventHandler(this.btnBrowse_Click);
            // 
            // label19
            // 
            this.label19.Location = new System.Drawing.Point(3, 49);
            this.label19.Name = "label19";
            this.label19.Size = new System.Drawing.Size(96, 13);
            this.label19.TabIndex = 71;
            this.label19.Text = "ISO/WBFS File :";
            this.label19.TextAlign = System.Drawing.ContentAlignment.TopRight;
            // 
            // txtIsoFile
            // 
            this.txtIsoFile.Enabled = false;
            this.txtIsoFile.Location = new System.Drawing.Point(102, 46);
            this.txtIsoFile.Name = "txtIsoFile";
            this.txtIsoFile.Size = new System.Drawing.Size(222, 20);
            this.txtIsoFile.TabIndex = 69;
            // 
            // btnSelectIso
            // 
            this.btnSelectIso.Location = new System.Drawing.Point(330, 44);
            this.btnSelectIso.Name = "btnSelectIso";
            this.btnSelectIso.Size = new System.Drawing.Size(33, 23);
            this.btnSelectIso.TabIndex = 70;
            this.btnSelectIso.Text = "...";
            this.btnSelectIso.UseVisualStyleBackColor = true;
            this.btnSelectIso.Click += new System.EventHandler(this.btnSelectIso_Click);
            // 
            // label2
            // 
            this.label2.AutoSize = true;
            this.label2.Location = new System.Drawing.Point(358, 126);
            this.label2.Name = "label2";
            this.label2.Size = new System.Drawing.Size(251, 13);
            this.label2.TabIndex = 123;
            this.label2.Text = "To activate batch mode, drag&&drop banners here... ";
            // 
            // groupBox1
            // 
            this.groupBox1.Controls.Add(this.radBtn3);
            this.groupBox1.Controls.Add(this.radBtn2);
            this.groupBox1.Controls.Add(this.radBtn1);
            this.groupBox1.Location = new System.Drawing.Point(9, 397);
            this.groupBox1.Name = "groupBox1";
            this.groupBox1.Size = new System.Drawing.Size(295, 72);
            this.groupBox1.TabIndex = 122;
            this.groupBox1.TabStop = false;
            this.groupBox1.Text = "Wad Naming";
            // 
            // radBtn3
            // 
            this.radBtn3.AutoSize = true;
            this.radBtn3.Checked = true;
            this.radBtn3.Location = new System.Drawing.Point(8, 51);
            this.radBtn3.Name = "radBtn3";
            this.radBtn3.Size = new System.Drawing.Size(244, 17);
            this.radBtn3.TabIndex = 3;
            this.radBtn3.TabStop = true;
            this.radBtn3.Text = "{GameName} - {DiscId} - {TitleId}.wad             ";
            this.radBtn3.UseVisualStyleBackColor = true;
            // 
            // radBtn2
            // 
            this.radBtn2.AutoSize = true;
            this.radBtn2.Location = new System.Drawing.Point(8, 34);
            this.radBtn2.Name = "radBtn2";
            this.radBtn2.Size = new System.Drawing.Size(246, 17);
            this.radBtn2.TabIndex = 2;
            this.radBtn2.Text = "{GameName} - {DiscId}.wad                             ";
            this.radBtn2.UseVisualStyleBackColor = true;
            // 
            // radBtn1
            // 
            this.radBtn1.AutoSize = true;
            this.radBtn1.Location = new System.Drawing.Point(8, 18);
            this.radBtn1.Name = "radBtn1";
            this.radBtn1.Size = new System.Drawing.Size(245, 17);
            this.radBtn1.TabIndex = 1;
            this.radBtn1.Text = "{DiscId}.wad                                                     ";
            this.radBtn1.UseVisualStyleBackColor = true;
            // 
            // lblGameName
            // 
            this.lblGameName.AutoSize = true;
            this.lblGameName.Font = new System.Drawing.Font("Microsoft Sans Serif", 9.75F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(162)));
            this.lblGameName.Location = new System.Drawing.Point(115, 113);
            this.lblGameName.Name = "lblGameName";
            this.lblGameName.Size = new System.Drawing.Size(0, 16);
            this.lblGameName.TabIndex = 121;
            // 
            // lblGameNameLbl
            // 
            this.lblGameNameLbl.Location = new System.Drawing.Point(9, 113);
            this.lblGameNameLbl.Name = "lblGameNameLbl";
            this.lblGameNameLbl.Size = new System.Drawing.Size(104, 13);
            this.lblGameNameLbl.TabIndex = 120;
            this.lblGameNameLbl.Text = "Game :";
            this.lblGameNameLbl.TextAlign = System.Drawing.ContentAlignment.TopRight;
            // 
            // lblSdCardSupport
            // 
            this.lblSdCardSupport.AutoSize = true;
            this.lblSdCardSupport.Location = new System.Drawing.Point(523, 350);
            this.lblSdCardSupport.Name = "lblSdCardSupport";
            this.lblSdCardSupport.Size = new System.Drawing.Size(0, 13);
            this.lblSdCardSupport.TabIndex = 119;
            // 
            // label16
            // 
            this.label16.Font = new System.Drawing.Font("Microsoft Sans Serif", 8.25F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(162)));
            this.label16.Location = new System.Drawing.Point(358, 350);
            this.label16.Name = "label16";
            this.label16.Size = new System.Drawing.Size(154, 13);
            this.label16.TabIndex = 118;
            this.label16.Text = "Loading from SD/SDHC";
            this.label16.TextAlign = System.Drawing.ContentAlignment.TopRight;
            // 
            // lblForceLanguageSupport
            // 
            this.lblForceLanguageSupport.AutoSize = true;
            this.lblForceLanguageSupport.Location = new System.Drawing.Point(523, 329);
            this.lblForceLanguageSupport.Name = "lblForceLanguageSupport";
            this.lblForceLanguageSupport.Size = new System.Drawing.Size(0, 13);
            this.lblForceLanguageSupport.TabIndex = 117;
            // 
            // lblOcarinaSupport
            // 
            this.lblOcarinaSupport.AutoSize = true;
            this.lblOcarinaSupport.Location = new System.Drawing.Point(523, 288);
            this.lblOcarinaSupport.Name = "lblOcarinaSupport";
            this.lblOcarinaSupport.Size = new System.Drawing.Size(0, 13);
            this.lblOcarinaSupport.TabIndex = 115;
            // 
            // lblForceVideoModeSupport
            // 
            this.lblForceVideoModeSupport.AutoSize = true;
            this.lblForceVideoModeSupport.Location = new System.Drawing.Point(523, 309);
            this.lblForceVideoModeSupport.Name = "lblForceVideoModeSupport";
            this.lblForceVideoModeSupport.Size = new System.Drawing.Size(0, 13);
            this.lblForceVideoModeSupport.TabIndex = 114;
            // 
            // label15
            // 
            this.label15.Font = new System.Drawing.Font("Microsoft Sans Serif", 8.25F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(162)));
            this.label15.Location = new System.Drawing.Point(358, 329);
            this.label15.Name = "label15";
            this.label15.Size = new System.Drawing.Size(154, 13);
            this.label15.TabIndex = 113;
            this.label15.Text = "Force Language support";
            this.label15.TextAlign = System.Drawing.ContentAlignment.TopRight;
            // 
            // label14
            // 
            this.label14.Font = new System.Drawing.Font("Microsoft Sans Serif", 8.25F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(162)));
            this.label14.Location = new System.Drawing.Point(355, 309);
            this.label14.Name = "label14";
            this.label14.Size = new System.Drawing.Size(157, 13);
            this.label14.TabIndex = 112;
            this.label14.Text = "Force video mode support";
            this.label14.TextAlign = System.Drawing.ContentAlignment.TopRight;
            // 
            // label13
            // 
            this.label13.Font = new System.Drawing.Font("Microsoft Sans Serif", 8.25F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(162)));
            this.label13.Location = new System.Drawing.Point(361, 288);
            this.label13.Name = "label13";
            this.label13.Size = new System.Drawing.Size(151, 13);
            this.label13.TabIndex = 111;
            this.label13.Text = "Ocarina support";
            this.label13.TextAlign = System.Drawing.ContentAlignment.TopRight;
            // 
            // btnTest
            // 
            this.btnTest.Location = new System.Drawing.Point(157, 487);
            this.btnTest.Name = "btnTest";
            this.btnTest.Size = new System.Drawing.Size(138, 23);
            this.btnTest.TabIndex = 110;
            this.btnTest.Text = "Test / Install";
            this.btnTest.UseVisualStyleBackColor = true;
            this.btnTest.Click += new System.EventHandler(this.btnTest_Click);
            // 
            // lblVerboseLog
            // 
            this.lblVerboseLog.AutoSize = true;
            this.lblVerboseLog.Location = new System.Drawing.Point(523, 268);
            this.lblVerboseLog.Name = "lblVerboseLog";
            this.lblVerboseLog.Size = new System.Drawing.Size(0, 13);
            this.lblVerboseLog.TabIndex = 109;
            // 
            // label11
            // 
            this.label11.Font = new System.Drawing.Font("Microsoft Sans Serif", 8.25F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(162)));
            this.label11.Location = new System.Drawing.Point(358, 268);
            this.label11.Name = "label11";
            this.label11.Size = new System.Drawing.Size(154, 13);
            this.label11.TabIndex = 108;
            this.label11.Text = "Verbose output support";
            this.label11.TextAlign = System.Drawing.ContentAlignment.TopRight;
            // 
            // lblDolFilename
            // 
            this.lblDolFilename.AutoSize = true;
            this.lblDolFilename.Location = new System.Drawing.Point(522, 153);
            this.lblDolFilename.Name = "lblDolFilename";
            this.lblDolFilename.Size = new System.Drawing.Size(0, 13);
            this.lblDolFilename.TabIndex = 107;
            // 
            // label10
            // 
            this.label10.Font = new System.Drawing.Font("Microsoft Sans Serif", 8.25F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(162)));
            this.label10.Location = new System.Drawing.Point(358, 153);
            this.label10.Name = "label10";
            this.label10.Size = new System.Drawing.Size(154, 13);
            this.label10.TabIndex = 106;
            this.label10.Text = "Filename";
            this.label10.TextAlign = System.Drawing.ContentAlignment.TopRight;
            // 
            // lblConfigPlaceholder
            // 
            this.lblConfigPlaceholder.AutoSize = true;
            this.lblConfigPlaceholder.Location = new System.Drawing.Point(523, 228);
            this.lblConfigPlaceholder.Name = "lblConfigPlaceholder";
            this.lblConfigPlaceholder.Size = new System.Drawing.Size(0, 13);
            this.lblConfigPlaceholder.TabIndex = 105;
            // 
            // lblDefaultDiscId
            // 
            this.lblDefaultDiscId.AutoSize = true;
            this.lblDefaultDiscId.Location = new System.Drawing.Point(523, 208);
            this.lblDefaultDiscId.Name = "lblDefaultDiscId";
            this.lblDefaultDiscId.Size = new System.Drawing.Size(0, 13);
            this.lblDefaultDiscId.TabIndex = 104;
            // 
            // lblRegionOverride
            // 
            this.lblRegionOverride.AutoSize = true;
            this.lblRegionOverride.Location = new System.Drawing.Point(522, 248);
            this.lblRegionOverride.Name = "lblRegionOverride";
            this.lblRegionOverride.Size = new System.Drawing.Size(0, 13);
            this.lblRegionOverride.TabIndex = 103;
            // 
            // lblModder
            // 
            this.lblModder.AutoSize = true;
            this.lblModder.Location = new System.Drawing.Point(522, 190);
            this.lblModder.Name = "lblModder";
            this.lblModder.Size = new System.Drawing.Size(0, 13);
            this.lblModder.TabIndex = 102;
            // 
            // lblAuthor
            // 
            this.lblAuthor.AutoSize = true;
            this.lblAuthor.Location = new System.Drawing.Point(522, 171);
            this.lblAuthor.Name = "lblAuthor";
            this.lblAuthor.Size = new System.Drawing.Size(0, 13);
            this.lblAuthor.TabIndex = 101;
            // 
            // label9
            // 
            this.label9.Font = new System.Drawing.Font("Microsoft Sans Serif", 8.25F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(162)));
            this.label9.Location = new System.Drawing.Point(358, 228);
            this.label9.Name = "label9";
            this.label9.Size = new System.Drawing.Size(155, 13);
            this.label9.TabIndex = 100;
            this.label9.Text = "Config placeholder";
            this.label9.TextAlign = System.Drawing.ContentAlignment.TopRight;
            // 
            // label8
            // 
            this.label8.Font = new System.Drawing.Font("Microsoft Sans Serif", 8.25F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(162)));
            this.label8.Location = new System.Drawing.Point(358, 208);
            this.label8.Name = "label8";
            this.label8.Size = new System.Drawing.Size(155, 13);
            this.label8.TabIndex = 99;
            this.label8.Text = "Default Disc Id";
            this.label8.TextAlign = System.Drawing.ContentAlignment.TopRight;
            // 
            // label7
            // 
            this.label7.Font = new System.Drawing.Font("Microsoft Sans Serif", 8.25F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(162)));
            this.label7.Location = new System.Drawing.Point(358, 248);
            this.label7.Name = "label7";
            this.label7.Size = new System.Drawing.Size(154, 13);
            this.label7.TabIndex = 98;
            this.label7.Text = "Region override";
            this.label7.TextAlign = System.Drawing.ContentAlignment.TopRight;
            // 
            // label6
            // 
            this.label6.Font = new System.Drawing.Font("Microsoft Sans Serif", 8.25F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(162)));
            this.label6.Location = new System.Drawing.Point(358, 190);
            this.label6.Name = "label6";
            this.label6.Size = new System.Drawing.Size(154, 13);
            this.label6.TabIndex = 97;
            this.label6.Text = "Modder";
            this.label6.TextAlign = System.Drawing.ContentAlignment.TopRight;
            // 
            // label5
            // 
            this.label5.Font = new System.Drawing.Font("Microsoft Sans Serif", 8.25F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(162)));
            this.label5.Location = new System.Drawing.Point(358, 171);
            this.label5.Name = "label5";
            this.label5.Size = new System.Drawing.Size(155, 13);
            this.label5.TabIndex = 96;
            this.label5.Text = "Author";
            this.label5.TextAlign = System.Drawing.ContentAlignment.TopRight;
            // 
            // lblLoader
            // 
            this.lblLoader.Location = new System.Drawing.Point(6, 143);
            this.lblLoader.Name = "lblLoader";
            this.lblLoader.Size = new System.Drawing.Size(108, 13);
            this.lblLoader.TabIndex = 95;
            this.lblLoader.Text = "Loader :";
            this.lblLoader.TextAlign = System.Drawing.ContentAlignment.TopRight;
            // 
            // cmbLoaders
            // 
            this.cmbLoaders.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.cmbLoaders.Location = new System.Drawing.Point(115, 140);
            this.cmbLoaders.Name = "cmbLoaders";
            this.cmbLoaders.Size = new System.Drawing.Size(203, 21);
            this.cmbLoaders.TabIndex = 94;
            this.cmbLoaders.SelectedIndexChanged += new System.EventHandler(this.cmbLoaders_SelectedIndexChanged);
            // 
            // btnCreate
            // 
            this.btnCreate.Enabled = false;
            this.btnCreate.Location = new System.Drawing.Point(17, 487);
            this.btnCreate.Name = "btnCreate";
            this.btnCreate.Size = new System.Drawing.Size(134, 23);
            this.btnCreate.TabIndex = 93;
            this.btnCreate.Text = "Create Channel";
            this.btnCreate.UseVisualStyleBackColor = true;
            this.btnCreate.Click += new System.EventHandler(this.button6_Click);
            // 
            // txtDiscId
            // 
            this.txtDiscId.AutoSize = true;
            this.txtDiscId.Location = new System.Drawing.Point(269, 167);
            this.txtDiscId.Name = "txtDiscId";
            this.txtDiscId.Size = new System.Drawing.Size(49, 13);
            this.txtDiscId.TabIndex = 92;
            this.txtDiscId.Text = "XXXXXX";
            // 
            // label4
            // 
            this.label4.Location = new System.Drawing.Point(182, 167);
            this.label4.Name = "label4";
            this.label4.Size = new System.Drawing.Size(88, 13);
            this.label4.TabIndex = 91;
            this.label4.Text = "Disc Id :";
            this.label4.TextAlign = System.Drawing.ContentAlignment.TopRight;
            // 
            // txtTitleId
            // 
            this.txtTitleId.Location = new System.Drawing.Point(115, 164);
            this.txtTitleId.MaxLength = 4;
            this.txtTitleId.Name = "txtTitleId";
            this.txtTitleId.Size = new System.Drawing.Size(63, 20);
            this.txtTitleId.TabIndex = 90;
            // 
            // label3
            // 
            this.label3.Location = new System.Drawing.Point(9, 167);
            this.label3.Name = "label3";
            this.label3.Size = new System.Drawing.Size(105, 13);
            this.label3.TabIndex = 89;
            this.label3.Text = "Title Id :";
            this.label3.TextAlign = System.Drawing.ContentAlignment.TopRight;
            // 
            // label18
            // 
            this.label18.Font = new System.Drawing.Font("Microsoft Sans Serif", 8.25F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(162)));
            this.label18.Location = new System.Drawing.Point(361, 370);
            this.label18.Name = "label18";
            this.label18.Size = new System.Drawing.Size(151, 13);
            this.label18.TabIndex = 116;
            this.label18.Text = "Support for Fixes";
            this.label18.TextAlign = System.Drawing.ContentAlignment.TopRight;
            // 
            // lblError002Fix
            // 
            this.lblError002Fix.AutoSize = true;
            this.lblError002Fix.Location = new System.Drawing.Point(523, 370);
            this.lblError002Fix.Name = "lblError002Fix";
            this.lblError002Fix.Size = new System.Drawing.Size(0, 13);
            this.lblError002Fix.TabIndex = 124;
            // 
            // label21
            // 
            this.label21.Font = new System.Drawing.Font("Microsoft Sans Serif", 8.25F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(162)));
            this.label21.Location = new System.Drawing.Point(361, 390);
            this.label21.Name = "label21";
            this.label21.Size = new System.Drawing.Size(151, 13);
            this.label21.TabIndex = 125;
            this.label21.Text = "Alt-dol support";
            this.label21.TextAlign = System.Drawing.ContentAlignment.TopRight;
            // 
            // lblAltDolSupport
            // 
            this.lblAltDolSupport.AutoSize = true;
            this.lblAltDolSupport.Location = new System.Drawing.Point(523, 390);
            this.lblAltDolSupport.Name = "lblAltDolSupport";
            this.lblAltDolSupport.Size = new System.Drawing.Size(0, 13);
            this.lblAltDolSupport.TabIndex = 126;
            // 
            // menuStrip1
            // 
            this.menuStrip1.BackColor = System.Drawing.Color.Tan;
            this.menuStrip1.Items.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.openBannerToolStripMenuItem,
            this.languageToolStripMenuItem,
            this.configureToolStripMenuItem,
            this.helpToolStripMenuItem});
            this.menuStrip1.Location = new System.Drawing.Point(0, 0);
            this.menuStrip1.Name = "menuStrip1";
            this.menuStrip1.RenderMode = System.Windows.Forms.ToolStripRenderMode.System;
            this.menuStrip1.Size = new System.Drawing.Size(820, 24);
            this.menuStrip1.TabIndex = 129;
            this.menuStrip1.Text = "menuStrip1";
            // 
            // openBannerToolStripMenuItem
            // 
            this.openBannerToolStripMenuItem.DropDownItems.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.openChannelWADToolStripMenuItem,
            this.openISOToolStripMenuItem,
            this.wBFSDriveToolStripMenuItem,
            this.viewWBFSDriveToolStripMenuItem,
            this.exitToolStripMenuItem});
            this.openBannerToolStripMenuItem.Name = "openBannerToolStripMenuItem";
            this.openBannerToolStripMenuItem.Size = new System.Drawing.Size(35, 20);
            this.openBannerToolStripMenuItem.Text = "File";
            // 
            // openChannelWADToolStripMenuItem
            // 
            this.openChannelWADToolStripMenuItem.Name = "openChannelWADToolStripMenuItem";
            this.openChannelWADToolStripMenuItem.Size = new System.Drawing.Size(174, 22);
            this.openChannelWADToolStripMenuItem.Text = "Open Channel(WAD)";
            this.openChannelWADToolStripMenuItem.Click += new System.EventHandler(this.openChannelWADToolStripMenuItem_Click);
            // 
            // openISOToolStripMenuItem
            // 
            this.openISOToolStripMenuItem.Name = "openISOToolStripMenuItem";
            this.openISOToolStripMenuItem.Size = new System.Drawing.Size(174, 22);
            this.openISOToolStripMenuItem.Text = "Open Banner";
            this.openISOToolStripMenuItem.Click += new System.EventHandler(this.openISOToolStripMenuItem_Click);
            // 
            // wBFSDriveToolStripMenuItem
            // 
            this.wBFSDriveToolStripMenuItem.Name = "wBFSDriveToolStripMenuItem";
            this.wBFSDriveToolStripMenuItem.Size = new System.Drawing.Size(174, 22);
            this.wBFSDriveToolStripMenuItem.Text = "Open ISO";
            this.wBFSDriveToolStripMenuItem.Click += new System.EventHandler(this.wBFSDriveToolStripMenuItem_Click);
            // 
            // viewWBFSDriveToolStripMenuItem
            // 
            this.viewWBFSDriveToolStripMenuItem.Name = "viewWBFSDriveToolStripMenuItem";
            this.viewWBFSDriveToolStripMenuItem.Size = new System.Drawing.Size(174, 22);
            this.viewWBFSDriveToolStripMenuItem.Text = "View WBFS Drive";
            this.viewWBFSDriveToolStripMenuItem.Click += new System.EventHandler(this.viewWBFSDriveToolStripMenuItem_Click);
            // 
            // exitToolStripMenuItem
            // 
            this.exitToolStripMenuItem.Name = "exitToolStripMenuItem";
            this.exitToolStripMenuItem.Size = new System.Drawing.Size(174, 22);
            this.exitToolStripMenuItem.Text = "Exit";
            this.exitToolStripMenuItem.Click += new System.EventHandler(this.exitToolStripMenuItem_Click);
            // 
            // languageToolStripMenuItem
            // 
            this.languageToolStripMenuItem.DropDownItems.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.englishToolStripMenuItem,
            this.turToolStripMenuItem,
            this.germanToolStripMenuItem,
            this.toolStripMenuItem1,
            this.frenchToolStripMenuItem,
            this.spanishToolStripMenuItem,
            this.ıtalianToolStripMenuItem,
            this.portoqueseToolStripMenuItem,
            this.japaneseToolStripMenuItem,
            this.sChineseToolStripMenuItem,
            this.tChineseToolStripMenuItem,
            this.koreanToolStripMenuItem,
            this.russianToolStripMenuItem,
            this.dutchToolStripMenuItem,
            this.finnishToolStripMenuItem,
            this.swedishToolStripMenuItem,
            this.danishToolStripMenuItem});
            this.languageToolStripMenuItem.Name = "languageToolStripMenuItem";
            this.languageToolStripMenuItem.Size = new System.Drawing.Size(66, 20);
            this.languageToolStripMenuItem.Text = "Language";
            // 
            // englishToolStripMenuItem
            // 
            this.englishToolStripMenuItem.Name = "englishToolStripMenuItem";
            this.englishToolStripMenuItem.Size = new System.Drawing.Size(129, 22);
            this.englishToolStripMenuItem.Text = "English";
            this.englishToolStripMenuItem.Click += new System.EventHandler(this.englishToolStripMenuItem_Click);
            // 
            // turToolStripMenuItem
            // 
            this.turToolStripMenuItem.Name = "turToolStripMenuItem";
            this.turToolStripMenuItem.Size = new System.Drawing.Size(129, 22);
            this.turToolStripMenuItem.Text = "Turkish";
            this.turToolStripMenuItem.Click += new System.EventHandler(this.turToolStripMenuItem_Click);
            // 
            // germanToolStripMenuItem
            // 
            this.germanToolStripMenuItem.Name = "germanToolStripMenuItem";
            this.germanToolStripMenuItem.Size = new System.Drawing.Size(129, 22);
            this.germanToolStripMenuItem.Text = "Deutsch";
            this.germanToolStripMenuItem.Click += new System.EventHandler(this.germanToolStripMenuItem_Click);
            // 
            // toolStripMenuItem1
            // 
            this.toolStripMenuItem1.Name = "toolStripMenuItem1";
            this.toolStripMenuItem1.Size = new System.Drawing.Size(129, 22);
            this.toolStripMenuItem1.Text = "French-1";
            this.toolStripMenuItem1.Click += new System.EventHandler(this.toolStripMenuItem1_Click);
            // 
            // frenchToolStripMenuItem
            // 
            this.frenchToolStripMenuItem.Name = "frenchToolStripMenuItem";
            this.frenchToolStripMenuItem.Size = new System.Drawing.Size(129, 22);
            this.frenchToolStripMenuItem.Text = "French-2";
            this.frenchToolStripMenuItem.Click += new System.EventHandler(this.frenchToolStripMenuItem_Click);
            // 
            // spanishToolStripMenuItem
            // 
            this.spanishToolStripMenuItem.Name = "spanishToolStripMenuItem";
            this.spanishToolStripMenuItem.Size = new System.Drawing.Size(129, 22);
            this.spanishToolStripMenuItem.Text = "Spanish";
            this.spanishToolStripMenuItem.Click += new System.EventHandler(this.spanishToolStripMenuItem_Click);
            // 
            // ıtalianToolStripMenuItem
            // 
            this.ıtalianToolStripMenuItem.Name = "ıtalianToolStripMenuItem";
            this.ıtalianToolStripMenuItem.Size = new System.Drawing.Size(129, 22);
            this.ıtalianToolStripMenuItem.Text = "Italian";
            this.ıtalianToolStripMenuItem.Click += new System.EventHandler(this.ıtalianToolStripMenuItem_Click);
            // 
            // portoqueseToolStripMenuItem
            // 
            this.portoqueseToolStripMenuItem.Name = "portoqueseToolStripMenuItem";
            this.portoqueseToolStripMenuItem.Size = new System.Drawing.Size(129, 22);
            this.portoqueseToolStripMenuItem.Text = "Portoquese";
            this.portoqueseToolStripMenuItem.Click += new System.EventHandler(this.portoqueseToolStripMenuItem_Click);
            // 
            // japaneseToolStripMenuItem
            // 
            this.japaneseToolStripMenuItem.Name = "japaneseToolStripMenuItem";
            this.japaneseToolStripMenuItem.Size = new System.Drawing.Size(129, 22);
            this.japaneseToolStripMenuItem.Text = "Japanese";
            this.japaneseToolStripMenuItem.Click += new System.EventHandler(this.japaneseToolStripMenuItem_Click);
            // 
            // sChineseToolStripMenuItem
            // 
            this.sChineseToolStripMenuItem.Name = "sChineseToolStripMenuItem";
            this.sChineseToolStripMenuItem.Size = new System.Drawing.Size(129, 22);
            this.sChineseToolStripMenuItem.Text = "S.Chinese";
            this.sChineseToolStripMenuItem.Click += new System.EventHandler(this.sChineseToolStripMenuItem_Click);
            // 
            // tChineseToolStripMenuItem
            // 
            this.tChineseToolStripMenuItem.Name = "tChineseToolStripMenuItem";
            this.tChineseToolStripMenuItem.Size = new System.Drawing.Size(129, 22);
            this.tChineseToolStripMenuItem.Text = "T.Chinese";
            this.tChineseToolStripMenuItem.Click += new System.EventHandler(this.tChineseToolStripMenuItem_Click);
            // 
            // koreanToolStripMenuItem
            // 
            this.koreanToolStripMenuItem.Name = "koreanToolStripMenuItem";
            this.koreanToolStripMenuItem.Size = new System.Drawing.Size(129, 22);
            this.koreanToolStripMenuItem.Text = "Korean";
            this.koreanToolStripMenuItem.Click += new System.EventHandler(this.koreanToolStripMenuItem_Click);
            // 
            // russianToolStripMenuItem
            // 
            this.russianToolStripMenuItem.Name = "russianToolStripMenuItem";
            this.russianToolStripMenuItem.Size = new System.Drawing.Size(129, 22);
            this.russianToolStripMenuItem.Text = "Russian";
            this.russianToolStripMenuItem.Click += new System.EventHandler(this.russianToolStripMenuItem_Click);
            // 
            // dutchToolStripMenuItem
            // 
            this.dutchToolStripMenuItem.Name = "dutchToolStripMenuItem";
            this.dutchToolStripMenuItem.Size = new System.Drawing.Size(129, 22);
            this.dutchToolStripMenuItem.Text = "Dutch";
            this.dutchToolStripMenuItem.Click += new System.EventHandler(this.dutchToolStripMenuItem_Click);
            // 
            // finnishToolStripMenuItem
            // 
            this.finnishToolStripMenuItem.Name = "finnishToolStripMenuItem";
            this.finnishToolStripMenuItem.Size = new System.Drawing.Size(129, 22);
            this.finnishToolStripMenuItem.Text = "Finnish";
            this.finnishToolStripMenuItem.Click += new System.EventHandler(this.finnishToolStripMenuItem_Click);
            // 
            // swedishToolStripMenuItem
            // 
            this.swedishToolStripMenuItem.Name = "swedishToolStripMenuItem";
            this.swedishToolStripMenuItem.Size = new System.Drawing.Size(129, 22);
            this.swedishToolStripMenuItem.Text = "Swedish";
            this.swedishToolStripMenuItem.Click += new System.EventHandler(this.swedishToolStripMenuItem_Click);
            // 
            // danishToolStripMenuItem
            // 
            this.danishToolStripMenuItem.Name = "danishToolStripMenuItem";
            this.danishToolStripMenuItem.Size = new System.Drawing.Size(129, 22);
            this.danishToolStripMenuItem.Text = "Danish";
            this.danishToolStripMenuItem.Click += new System.EventHandler(this.danishToolStripMenuItem_Click);
            // 
            // configureToolStripMenuItem
            // 
            this.configureToolStripMenuItem.DropDownItems.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.ipAddressToolStripMenuItem,
            this.updatesToolStripMenuItem});
            this.configureToolStripMenuItem.Name = "configureToolStripMenuItem";
            this.configureToolStripMenuItem.Size = new System.Drawing.Size(66, 20);
            this.configureToolStripMenuItem.Text = "Configure";
            // 
            // ipAddressToolStripMenuItem
            // 
            this.ipAddressToolStripMenuItem.Name = "ipAddressToolStripMenuItem";
            this.ipAddressToolStripMenuItem.Size = new System.Drawing.Size(114, 22);
            this.ipAddressToolStripMenuItem.Text = "Settings";
            this.ipAddressToolStripMenuItem.Click += new System.EventHandler(this.ipAddressToolStripMenuItem_Click);
            // 
            // updatesToolStripMenuItem
            // 
            this.updatesToolStripMenuItem.Name = "updatesToolStripMenuItem";
            this.updatesToolStripMenuItem.Size = new System.Drawing.Size(114, 22);
            this.updatesToolStripMenuItem.Text = "Updates";
            this.updatesToolStripMenuItem.Click += new System.EventHandler(this.updatesToolStripMenuItem_Click);
            // 
            // helpToolStripMenuItem
            // 
            this.helpToolStripMenuItem.DropDownItems.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.ınformationToolStripMenuItem,
            this.officialSiteToolStripMenuItem,
            this.officialDiscussionToolStripMenuItem,
            this.donationToolStripMenuItem});
            this.helpToolStripMenuItem.Name = "helpToolStripMenuItem";
            this.helpToolStripMenuItem.Size = new System.Drawing.Size(40, 20);
            this.helpToolStripMenuItem.Text = "Help";
            // 
            // ınformationToolStripMenuItem
            // 
            this.ınformationToolStripMenuItem.Name = "ınformationToolStripMenuItem";
            this.ınformationToolStripMenuItem.Size = new System.Drawing.Size(159, 22);
            this.ınformationToolStripMenuItem.Text = "Information";
            this.ınformationToolStripMenuItem.Click += new System.EventHandler(this.ınformationToolStripMenuItem_Click);
            // 
            // officialSiteToolStripMenuItem
            // 
            this.officialSiteToolStripMenuItem.Name = "officialSiteToolStripMenuItem";
            this.officialSiteToolStripMenuItem.Size = new System.Drawing.Size(159, 22);
            this.officialSiteToolStripMenuItem.Text = "Official Site";
            this.officialSiteToolStripMenuItem.Click += new System.EventHandler(this.officialSiteToolStripMenuItem_Click);
            // 
            // officialDiscussionToolStripMenuItem
            // 
            this.officialDiscussionToolStripMenuItem.Name = "officialDiscussionToolStripMenuItem";
            this.officialDiscussionToolStripMenuItem.Size = new System.Drawing.Size(159, 22);
            this.officialDiscussionToolStripMenuItem.Text = "Official Discussion";
            this.officialDiscussionToolStripMenuItem.Click += new System.EventHandler(this.officialDiscussionToolStripMenuItem_Click);
            // 
            // donationToolStripMenuItem
            // 
            this.donationToolStripMenuItem.Name = "donationToolStripMenuItem";
            this.donationToolStripMenuItem.Size = new System.Drawing.Size(159, 22);
            this.donationToolStripMenuItem.Text = "Donation";
            this.donationToolStripMenuItem.Click += new System.EventHandler(this.donationToolStripMenuItem_Click);
            // 
            // Form1
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(820, 542);
            this.Controls.Add(this.statusStrip1);
            this.Controls.Add(this.panelPartition);
            this.Controls.Add(this.panelOptions);
            this.Controls.Add(this.panelWBFS);
            this.Controls.Add(this.panelBatch);
            this.Controls.Add(this.groupBox4);
            this.Controls.Add(this.label2);
            this.Controls.Add(this.groupBox1);
            this.Controls.Add(this.lblGameName);
            this.Controls.Add(this.lblGameNameLbl);
            this.Controls.Add(this.lblSdCardSupport);
            this.Controls.Add(this.label16);
            this.Controls.Add(this.lblForceLanguageSupport);
            this.Controls.Add(this.lblOcarinaSupport);
            this.Controls.Add(this.lblForceVideoModeSupport);
            this.Controls.Add(this.label15);
            this.Controls.Add(this.label14);
            this.Controls.Add(this.label13);
            this.Controls.Add(this.btnTest);
            this.Controls.Add(this.lblVerboseLog);
            this.Controls.Add(this.label11);
            this.Controls.Add(this.lblDolFilename);
            this.Controls.Add(this.label10);
            this.Controls.Add(this.lblConfigPlaceholder);
            this.Controls.Add(this.lblDefaultDiscId);
            this.Controls.Add(this.lblRegionOverride);
            this.Controls.Add(this.lblModder);
            this.Controls.Add(this.lblAuthor);
            this.Controls.Add(this.label9);
            this.Controls.Add(this.label8);
            this.Controls.Add(this.label7);
            this.Controls.Add(this.label6);
            this.Controls.Add(this.label5);
            this.Controls.Add(this.lblLoader);
            this.Controls.Add(this.cmbLoaders);
            this.Controls.Add(this.btnCreate);
            this.Controls.Add(this.txtDiscId);
            this.Controls.Add(this.label4);
            this.Controls.Add(this.txtTitleId);
            this.Controls.Add(this.label3);
            this.Controls.Add(this.label18);
            this.Controls.Add(this.lblError002Fix);
            this.Controls.Add(this.label21);
            this.Controls.Add(this.lblAltDolSupport);
            this.Controls.Add(this.menuStrip1);
            this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.Fixed3D;
            this.Icon = ((System.Drawing.Icon)(resources.GetObject("$this.Icon")));
            this.MaximizeBox = false;
            this.Name = "Form1";
            this.Text = "WiiGSC 1.06b - Wii Game Shortcut Creator by WiiCrazy/I.R.on";
            this.Load += new System.EventHandler(this.Form1_Load);
            this.statusStrip1.ResumeLayout(false);
            this.statusStrip1.PerformLayout();
            this.panelPartition.ResumeLayout(false);
            this.panelPartition.PerformLayout();
            this.panelOptions.ResumeLayout(false);
            this.panelOptions.PerformLayout();
            this.groupBox2.ResumeLayout(false);
            this.groupBox2.PerformLayout();
            this.panelWBFS.ResumeLayout(false);
            this.panelWBFS.PerformLayout();
            this.groupBox3.ResumeLayout(false);
            this.panelBatch.ResumeLayout(false);
            this.groupBox4.ResumeLayout(false);
            this.groupBox4.PerformLayout();
            this.groupBox1.ResumeLayout(false);
            this.groupBox1.PerformLayout();
            this.menuStrip1.ResumeLayout(false);
            this.menuStrip1.PerformLayout();
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.StatusStrip statusStrip1;
        private System.Windows.Forms.ToolStripProgressBar toolStripProgressBar1;
        private System.Windows.Forms.ToolStripStatusLabel toolStripStatusLabel1;
        private System.Windows.Forms.OpenFileDialog openFileDialog1;
        private System.Windows.Forms.OpenFileDialog openSaveFileDialog;
        private System.Windows.Forms.FolderBrowserDialog openSaveFileDlg;
        private System.Windows.Forms.OpenFileDialog openFileDialog2;
        private System.Windows.Forms.Panel panelPartition;
        private System.Windows.Forms.ListBox lstExtraParameters;
        private System.Windows.Forms.Label label26;
        private System.Windows.Forms.TextBox txtExtraParameters;
        private System.Windows.Forms.Label label25;
        private System.Windows.Forms.ComboBox cmbPartition;
        private System.Windows.Forms.Panel panelOptions;
        private System.Windows.Forms.Label label17;
        private System.Windows.Forms.ComboBox cmbLoaderType;
        private System.Windows.Forms.ComboBox cmbDolList;
        private System.Windows.Forms.Label label20;
        private System.Windows.Forms.GroupBox groupBox2;
        private System.Windows.Forms.CheckBox chkNewStyle002Fix;
        private System.Windows.Forms.CheckBox chkOldStyle002Fix;
        private System.Windows.Forms.CheckBox chkAnti002Fix;
        private System.Windows.Forms.Label label22;
        private System.Windows.Forms.ComboBox cmbAltDolType;
        private System.Windows.Forms.CheckBox chkVerbose;
        private System.Windows.Forms.ComboBox cmbRegion;
        private System.Windows.Forms.Label label1;
        private System.Windows.Forms.CheckBox chkForceVideoMode;
        private System.Windows.Forms.ComboBox cmbLanguage;
        private System.Windows.Forms.CheckBox chkOcarinaSupport;
        private System.Windows.Forms.Label label12;
        private System.Windows.Forms.Panel panelWBFS;
        private System.Windows.Forms.Button button2;
        private System.Windows.Forms.GroupBox groupBox3;
        private System.Windows.Forms.ListView listGames;
        private System.Windows.Forms.ColumnHeader id;
        private System.Windows.Forms.ColumnHeader code;
        private System.Windows.Forms.ColumnHeader name;
        private System.Windows.Forms.ColumnHeader size;
        private System.Windows.Forms.ColumnHeader path;
        private System.Windows.Forms.Button btnRefresh;
        private System.Windows.Forms.Button button4;
        private System.Windows.Forms.ComboBox cmbDriveList;
        private System.Windows.Forms.Button btnCreateSelected;
        private System.Windows.Forms.Button button3;
        private System.Windows.Forms.Label label23;
        private System.Windows.Forms.Panel panelBatch;
        private System.Windows.Forms.ListBox listBox1;
        private System.Windows.Forms.Button btnBatchCreate;
        private System.Windows.Forms.Button btnDismiss;
        private System.Windows.Forms.GroupBox groupBox4;
        private System.Windows.Forms.Label lblChannel;
        private System.Windows.Forms.Button btnChannelSelect;
        private System.Windows.Forms.TextBox txtChannel;
        private System.Windows.Forms.Label lblFileName;
        private System.Windows.Forms.TextBox txtDataFile;
        private System.Windows.Forms.Button button1;
        private System.Windows.Forms.Button btnBrowse;
        private System.Windows.Forms.Label label19;
        private System.Windows.Forms.TextBox txtIsoFile;
        private System.Windows.Forms.Button btnSelectIso;
        private System.Windows.Forms.Label label2;
        private System.Windows.Forms.GroupBox groupBox1;
        private System.Windows.Forms.RadioButton radBtn3;
        private System.Windows.Forms.RadioButton radBtn2;
        private System.Windows.Forms.RadioButton radBtn1;
        private System.Windows.Forms.Label lblGameName;
        private System.Windows.Forms.Label lblGameNameLbl;
        private System.Windows.Forms.Label lblSdCardSupport;
        private System.Windows.Forms.Label label16;
        private System.Windows.Forms.Label lblForceLanguageSupport;
        private System.Windows.Forms.Label lblOcarinaSupport;
        private System.Windows.Forms.Label lblForceVideoModeSupport;
        private System.Windows.Forms.Label label15;
        private System.Windows.Forms.Label label14;
        private System.Windows.Forms.Label label13;
        private System.Windows.Forms.Button btnTest;
        private System.Windows.Forms.Label lblVerboseLog;
        private System.Windows.Forms.Label label11;
        private System.Windows.Forms.Label lblDolFilename;
        private System.Windows.Forms.Label label10;
        private System.Windows.Forms.Label lblConfigPlaceholder;
        private System.Windows.Forms.Label lblDefaultDiscId;
        private System.Windows.Forms.Label lblRegionOverride;
        private System.Windows.Forms.Label lblModder;
        private System.Windows.Forms.Label lblAuthor;
        private System.Windows.Forms.Label label9;
        private System.Windows.Forms.Label label8;
        private System.Windows.Forms.Label label7;
        private System.Windows.Forms.Label label6;
        private System.Windows.Forms.Label label5;
        private System.Windows.Forms.Label lblLoader;
        private System.Windows.Forms.ComboBox cmbLoaders;
        private System.Windows.Forms.Button btnCreate;
        private System.Windows.Forms.Label txtDiscId;
        private System.Windows.Forms.Label label4;
        private System.Windows.Forms.TextBox txtTitleId;
        private System.Windows.Forms.Label label3;
        private System.Windows.Forms.Label label18;
        private System.Windows.Forms.Label lblError002Fix;
        private System.Windows.Forms.Label label21;
        private System.Windows.Forms.Label lblAltDolSupport;
        private System.Windows.Forms.MenuStrip menuStrip1;
        private System.Windows.Forms.ToolStripMenuItem openBannerToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem openChannelWADToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem openISOToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem wBFSDriveToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem viewWBFSDriveToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem exitToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem languageToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem englishToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem turToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem germanToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem toolStripMenuItem1;
        private System.Windows.Forms.ToolStripMenuItem frenchToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem spanishToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem ıtalianToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem portoqueseToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem japaneseToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem sChineseToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem tChineseToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem koreanToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem russianToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem dutchToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem finnishToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem swedishToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem danishToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem configureToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem ipAddressToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem helpToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem ınformationToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem officialSiteToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem officialDiscussionToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem donationToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem updatesToolStripMenuItem;
    }
}