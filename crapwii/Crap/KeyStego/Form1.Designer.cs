namespace KeyStego
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
            this.carrier = new System.Windows.Forms.TextBox();
            this.label1 = new System.Windows.Forms.Label();
            this.hiddenHex = new System.Windows.Forms.TextBox();
            this.label2 = new System.Windows.Forms.Label();
            this.label3 = new System.Windows.Forms.Label();
            this.words1 = new System.Windows.Forms.ListBox();
            this.label4 = new System.Windows.Forms.Label();
            this.label5 = new System.Windows.Forms.Label();
            this.words2 = new System.Windows.Forms.ListBox();
            this.label6 = new System.Windows.Forms.Label();
            this.words8 = new System.Windows.Forms.ListBox();
            this.label7 = new System.Windows.Forms.Label();
            this.words4 = new System.Windows.Forms.ListBox();
            this.remarkedWords = new System.Windows.Forms.ListBox();
            this.label8 = new System.Windows.Forms.Label();
            this.hiddenText = new System.Windows.Forms.TextBox();
            this.label9 = new System.Windows.Forms.Label();
            this.button1 = new System.Windows.Forms.Button();
            this.saveFileDialog1 = new System.Windows.Forms.SaveFileDialog();
            this.SuspendLayout();
            // 
            // carrier
            // 
            this.carrier.Location = new System.Drawing.Point(13, 55);
            this.carrier.Multiline = true;
            this.carrier.Name = "carrier";
            this.carrier.Size = new System.Drawing.Size(443, 177);
            this.carrier.TabIndex = 0;
            this.carrier.TextChanged += new System.EventHandler(this.carrier_TextChanged);
            // 
            // label1
            // 
            this.label1.AutoSize = true;
            this.label1.Font = new System.Drawing.Font("Microsoft Sans Serif", 8.25F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(162)));
            this.label1.Location = new System.Drawing.Point(13, 36);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(120, 13);
            this.label1.TabIndex = 1;
            this.label1.Text = "Write your rant here";
            // 
            // hiddenHex
            // 
            this.hiddenHex.Location = new System.Drawing.Point(118, 241);
            this.hiddenHex.Name = "hiddenHex";
            this.hiddenHex.ReadOnly = true;
            this.hiddenHex.Size = new System.Drawing.Size(338, 20);
            this.hiddenHex.TabIndex = 2;
            // 
            // label2
            // 
            this.label2.AutoSize = true;
            this.label2.Font = new System.Drawing.Font("Microsoft Sans Serif", 8.25F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(162)));
            this.label2.Location = new System.Drawing.Point(10, 244);
            this.label2.Name = "label2";
            this.label2.Size = new System.Drawing.Size(102, 13);
            this.label2.TabIndex = 3;
            this.label2.Text = "Secret Info (hex)";
            // 
            // label3
            // 
            this.label3.AutoSize = true;
            this.label3.Font = new System.Drawing.Font("Microsoft Sans Serif", 8.25F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(162)));
            this.label3.Location = new System.Drawing.Point(43, 293);
            this.label3.Name = "label3";
            this.label3.Size = new System.Drawing.Size(131, 13);
            this.label3.TabIndex = 4;
            this.label3.Text = "Words to choose from";
            // 
            // words1
            // 
            this.words1.FormattingEnabled = true;
            this.words1.Location = new System.Drawing.Point(43, 340);
            this.words1.Name = "words1";
            this.words1.Size = new System.Drawing.Size(90, 264);
            this.words1.TabIndex = 5;
            // 
            // label4
            // 
            this.label4.AutoSize = true;
            this.label4.Location = new System.Drawing.Point(43, 321);
            this.label4.Name = "label4";
            this.label4.Size = new System.Drawing.Size(13, 13);
            this.label4.TabIndex = 6;
            this.label4.Text = "1";
            this.label4.Click += new System.EventHandler(this.label4_Click);
            // 
            // label5
            // 
            this.label5.AutoSize = true;
            this.label5.Location = new System.Drawing.Point(142, 321);
            this.label5.Name = "label5";
            this.label5.Size = new System.Drawing.Size(13, 13);
            this.label5.TabIndex = 8;
            this.label5.Text = "2";
            // 
            // words2
            // 
            this.words2.FormattingEnabled = true;
            this.words2.Location = new System.Drawing.Point(142, 340);
            this.words2.Name = "words2";
            this.words2.Size = new System.Drawing.Size(90, 264);
            this.words2.TabIndex = 7;
            // 
            // label6
            // 
            this.label6.AutoSize = true;
            this.label6.Location = new System.Drawing.Point(337, 321);
            this.label6.Name = "label6";
            this.label6.Size = new System.Drawing.Size(13, 13);
            this.label6.TabIndex = 12;
            this.label6.Text = "8";
            // 
            // words8
            // 
            this.words8.FormattingEnabled = true;
            this.words8.Location = new System.Drawing.Point(337, 340);
            this.words8.Name = "words8";
            this.words8.Size = new System.Drawing.Size(90, 264);
            this.words8.TabIndex = 11;
            this.words8.SelectedIndexChanged += new System.EventHandler(this.listBox3_SelectedIndexChanged);
            // 
            // label7
            // 
            this.label7.AutoSize = true;
            this.label7.Location = new System.Drawing.Point(238, 321);
            this.label7.Name = "label7";
            this.label7.Size = new System.Drawing.Size(13, 13);
            this.label7.TabIndex = 10;
            this.label7.Text = "4";
            // 
            // words4
            // 
            this.words4.FormattingEnabled = true;
            this.words4.Location = new System.Drawing.Point(238, 340);
            this.words4.Name = "words4";
            this.words4.Size = new System.Drawing.Size(90, 264);
            this.words4.TabIndex = 9;
            // 
            // remarkedWords
            // 
            this.remarkedWords.FormattingEnabled = true;
            this.remarkedWords.Location = new System.Drawing.Point(466, 55);
            this.remarkedWords.Name = "remarkedWords";
            this.remarkedWords.Size = new System.Drawing.Size(120, 550);
            this.remarkedWords.TabIndex = 13;
            // 
            // label8
            // 
            this.label8.AutoSize = true;
            this.label8.Font = new System.Drawing.Font("Microsoft Sans Serif", 8.25F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(162)));
            this.label8.Location = new System.Drawing.Point(466, 36);
            this.label8.Name = "label8";
            this.label8.Size = new System.Drawing.Size(101, 13);
            this.label8.TabIndex = 14;
            this.label8.Text = "Remarked words";
            // 
            // hiddenText
            // 
            this.hiddenText.Location = new System.Drawing.Point(118, 270);
            this.hiddenText.Name = "hiddenText";
            this.hiddenText.ReadOnly = true;
            this.hiddenText.Size = new System.Drawing.Size(338, 20);
            this.hiddenText.TabIndex = 15;
            // 
            // label9
            // 
            this.label9.AutoSize = true;
            this.label9.Font = new System.Drawing.Font("Microsoft Sans Serif", 8.25F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(162)));
            this.label9.Location = new System.Drawing.Point(10, 273);
            this.label9.Name = "label9";
            this.label9.Size = new System.Drawing.Size(108, 13);
            this.label9.TabIndex = 16;
            this.label9.Text = "Secret Info (ascii)";
            // 
            // button1
            // 
            this.button1.Location = new System.Drawing.Point(381, 293);
            this.button1.Name = "button1";
            this.button1.Size = new System.Drawing.Size(75, 23);
            this.button1.TabIndex = 17;
            this.button1.Text = "Save Data";
            this.button1.UseVisualStyleBackColor = true;
            this.button1.Click += new System.EventHandler(this.button1_Click);
            // 
            // Form1
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(597, 610);
            this.Controls.Add(this.button1);
            this.Controls.Add(this.label9);
            this.Controls.Add(this.hiddenText);
            this.Controls.Add(this.label8);
            this.Controls.Add(this.remarkedWords);
            this.Controls.Add(this.label6);
            this.Controls.Add(this.words8);
            this.Controls.Add(this.label7);
            this.Controls.Add(this.words4);
            this.Controls.Add(this.label5);
            this.Controls.Add(this.words2);
            this.Controls.Add(this.label4);
            this.Controls.Add(this.words1);
            this.Controls.Add(this.label3);
            this.Controls.Add(this.label2);
            this.Controls.Add(this.hiddenHex);
            this.Controls.Add(this.label1);
            this.Controls.Add(this.carrier);
            this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.Fixed3D;
            this.Icon = ((System.Drawing.Icon)(resources.GetObject("$this.Icon")));
            this.Name = "Form1";
            this.Text = "KeyStego 1.0 by WiiCrazy/I.R.on";
            this.Load += new System.EventHandler(this.Form1_Load);
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.TextBox carrier;
        private System.Windows.Forms.Label label1;
        private System.Windows.Forms.TextBox hiddenHex;
        private System.Windows.Forms.Label label2;
        private System.Windows.Forms.Label label3;
        private System.Windows.Forms.ListBox words1;
        private System.Windows.Forms.Label label4;
        private System.Windows.Forms.Label label5;
        private System.Windows.Forms.ListBox words2;
        private System.Windows.Forms.Label label6;
        private System.Windows.Forms.ListBox words8;
        private System.Windows.Forms.Label label7;
        private System.Windows.Forms.ListBox words4;
        private System.Windows.Forms.ListBox remarkedWords;
        private System.Windows.Forms.Label label8;
        private System.Windows.Forms.TextBox hiddenText;
        private System.Windows.Forms.Label label9;
        private System.Windows.Forms.Button button1;
        private System.Windows.Forms.SaveFileDialog saveFileDialog1;
    }
}

