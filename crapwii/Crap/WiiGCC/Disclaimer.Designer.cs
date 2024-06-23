namespace WiiGSC
{
    partial class Disclaimer
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
            this.lblDisclaimerText = new System.Windows.Forms.Label();
            this.chkDisclaimer = new System.Windows.Forms.CheckBox();
            this.SuspendLayout();
            // 
            // lblDisclaimerText
            // 
            this.lblDisclaimerText.Location = new System.Drawing.Point(13, 13);
            this.lblDisclaimerText.Name = "lblDisclaimerText";
            this.lblDisclaimerText.Size = new System.Drawing.Size(500, 56);
            this.lblDisclaimerText.TabIndex = 0;
            // 
            // chkDisclaimer
            // 
            this.chkDisclaimer.AutoSize = true;
            this.chkDisclaimer.Font = new System.Drawing.Font("Microsoft Sans Serif", 8.25F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(162)));
            this.chkDisclaimer.Location = new System.Drawing.Point(54, 78);
            this.chkDisclaimer.Name = "chkDisclaimer";
            this.chkDisclaimer.Size = new System.Drawing.Size(433, 17);
            this.chkDisclaimer.TabIndex = 1;
            this.chkDisclaimer.Text = "Ok, I know the consequences. Do not show this disclaimer to me again.";
            this.chkDisclaimer.UseVisualStyleBackColor = true;
            // 
            // Disclaimer
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(525, 107);
            this.Controls.Add(this.chkDisclaimer);
            this.Controls.Add(this.lblDisclaimerText);
            this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedDialog;
            this.Name = "Disclaimer";
            this.Text = "Disclaimer";
            this.Load += new System.EventHandler(this.Disclaimer_Load);
            this.FormClosing += new System.Windows.Forms.FormClosingEventHandler(this.Disclaimer_FormClosing);
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.Label lblDisclaimerText;
        private System.Windows.Forms.CheckBox chkDisclaimer;
    }
}