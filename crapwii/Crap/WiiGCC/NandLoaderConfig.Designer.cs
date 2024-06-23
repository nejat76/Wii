namespace WiiGSC
{
    partial class NandLoaderConfig
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
            this.cmbNandLoaders = new System.Windows.Forms.ComboBox();
            this.button1 = new System.Windows.Forms.Button();
            this.SuspendLayout();
            // 
            // label1
            // 
            this.label1.Location = new System.Drawing.Point(12, 22);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(172, 18);
            this.label1.TabIndex = 5;
            this.label1.Text = "Select nand loader/base wad : ";
            this.label1.TextAlign = System.Drawing.ContentAlignment.TopRight;
            // 
            // cmbNandLoaders
            // 
            this.cmbNandLoaders.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.cmbNandLoaders.FormattingEnabled = true;
            this.cmbNandLoaders.Location = new System.Drawing.Point(190, 19);
            this.cmbNandLoaders.Name = "cmbNandLoaders";
            this.cmbNandLoaders.Size = new System.Drawing.Size(188, 21);
            this.cmbNandLoaders.TabIndex = 4;
            // 
            // button1
            // 
            this.button1.Location = new System.Drawing.Point(384, 17);
            this.button1.Name = "button1";
            this.button1.Size = new System.Drawing.Size(75, 23);
            this.button1.TabIndex = 3;
            this.button1.Text = "Save";
            this.button1.UseVisualStyleBackColor = true;
            this.button1.Click += new System.EventHandler(this.button1_Click);
            // 
            // NandLoaderConfig
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(485, 59);
            this.Controls.Add(this.label1);
            this.Controls.Add(this.cmbNandLoaders);
            this.Controls.Add(this.button1);
            this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedDialog;
            this.MaximizeBox = false;
            this.Name = "NandLoaderConfig";
            this.Text = "NandLoaderConfig";
            this.Load += new System.EventHandler(this.NandLoaderConfig_Load);
            this.ResumeLayout(false);

        }

        #endregion

        private System.Windows.Forms.Label label1;
        private System.Windows.Forms.ComboBox cmbNandLoaders;
        private System.Windows.Forms.Button button1;
    }
}