namespace CrazyInstaller
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
            this.btnBrowse = new System.Windows.Forms.Button();
            this.label1 = new System.Windows.Forms.Label();
            this.txtWadFile = new System.Windows.Forms.TextBox();
            this.btnCreateInstallerDol = new System.Windows.Forms.Button();
            this.txtIosToUse = new System.Windows.Forms.TextBox();
            this.label2 = new System.Windows.Forms.Label();
            this.btnCreateInstallerExe = new System.Windows.Forms.Button();
            this.openFileDialog1 = new System.Windows.Forms.OpenFileDialog();
            this.saveFileDialog1 = new System.Windows.Forms.SaveFileDialog();
            this.SuspendLayout();
            // 
            // btnBrowse
            // 
            this.btnBrowse.Location = new System.Drawing.Point(506, 25);
            this.btnBrowse.Name = "btnBrowse";
            this.btnBrowse.Size = new System.Drawing.Size(43, 23);
            this.btnBrowse.TabIndex = 0;
            this.btnBrowse.Text = "...";
            this.btnBrowse.UseVisualStyleBackColor = true;
            this.btnBrowse.Click += new System.EventHandler(this.btnBrowse_Click);
            // 
            // label1
            // 
            this.label1.AutoSize = true;
            this.label1.Location = new System.Drawing.Point(12, 30);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(96, 13);
            this.label1.TabIndex = 1;
            this.label1.Text = "Selected channel :";
            // 
            // txtWadFile
            // 
            this.txtWadFile.Location = new System.Drawing.Point(114, 27);
            this.txtWadFile.Name = "txtWadFile";
            this.txtWadFile.ReadOnly = true;
            this.txtWadFile.Size = new System.Drawing.Size(386, 20);
            this.txtWadFile.TabIndex = 2;
            // 
            // btnCreateInstallerDol
            // 
            this.btnCreateInstallerDol.Location = new System.Drawing.Point(15, 95);
            this.btnCreateInstallerDol.Name = "btnCreateInstallerDol";
            this.btnCreateInstallerDol.Size = new System.Drawing.Size(190, 23);
            this.btnCreateInstallerDol.TabIndex = 3;
            this.btnCreateInstallerDol.Text = "Create installer dol";
            this.btnCreateInstallerDol.UseVisualStyleBackColor = true;
            this.btnCreateInstallerDol.Click += new System.EventHandler(this.btnCreateInstallerDol_Click);
            // 
            // txtIosToUse
            // 
            this.txtIosToUse.Location = new System.Drawing.Point(148, 54);
            this.txtIosToUse.Name = "txtIosToUse";
            this.txtIosToUse.Size = new System.Drawing.Size(51, 20);
            this.txtIosToUse.TabIndex = 4;
            // 
            // label2
            // 
            this.label2.AutoSize = true;
            this.label2.Location = new System.Drawing.Point(12, 60);
            this.label2.Name = "label2";
            this.label2.Size = new System.Drawing.Size(133, 13);
            this.label2.TabIndex = 5;
            this.label2.Text = "IOS to use for installation : ";
            // 
            // btnCreateInstallerExe
            // 
            this.btnCreateInstallerExe.Location = new System.Drawing.Point(212, 95);
            this.btnCreateInstallerExe.Name = "btnCreateInstallerExe";
            this.btnCreateInstallerExe.Size = new System.Drawing.Size(337, 23);
            this.btnCreateInstallerExe.TabIndex = 6;
            this.btnCreateInstallerExe.Text = "Create installer exe that installs the channel using wiiload";
            this.btnCreateInstallerExe.UseVisualStyleBackColor = true;
            this.btnCreateInstallerExe.Click += new System.EventHandler(this.btnCreateInstallerExe_Click);
            // 
            // openFileDialog1
            // 
            this.openFileDialog1.Filter = "Wii channels|*.wad";
            // 
            // saveFileDialog1
            // 
            this.saveFileDialog1.DefaultExt = "dol";
            this.saveFileDialog1.Filter = "Wii executable files|*.dol";
            this.saveFileDialog1.FileOk += new System.ComponentModel.CancelEventHandler(this.saveFileDialog1_FileOk);
            // 
            // Form1
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(573, 133);
            this.Controls.Add(this.btnCreateInstallerExe);
            this.Controls.Add(this.label2);
            this.Controls.Add(this.txtIosToUse);
            this.Controls.Add(this.btnCreateInstallerDol);
            this.Controls.Add(this.txtWadFile);
            this.Controls.Add(this.label1);
            this.Controls.Add(this.btnBrowse);
            this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedDialog;
            this.Icon = ((System.Drawing.Icon)(resources.GetObject("$this.Icon")));
            this.Name = "Form1";
            this.Text = "Crazy Installer v0.1 by WiiCrazy/I.R.on";
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.Button btnBrowse;
        private System.Windows.Forms.Label label1;
        private System.Windows.Forms.TextBox txtWadFile;
        private System.Windows.Forms.Button btnCreateInstallerDol;
        private System.Windows.Forms.TextBox txtIosToUse;
        private System.Windows.Forms.Label label2;
        private System.Windows.Forms.Button btnCreateInstallerExe;
        private System.Windows.Forms.OpenFileDialog openFileDialog1;
        private System.Windows.Forms.SaveFileDialog saveFileDialog1;
    }
}

