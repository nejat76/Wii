namespace WiiGSC
{
    partial class AppConfig
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
            this.button1 = new System.Windows.Forms.Button();
            this.lblIpAddress = new System.Windows.Forms.Label();
            this.txtIpAddress = new System.Windows.Forms.TextBox();
            this.lstGameNamePreference = new System.Windows.Forms.ListBox();
            this.lblGamePreferenceOrder = new System.Windows.Forms.Label();
            this.btnUp = new System.Windows.Forms.Button();
            this.btnDown = new System.Windows.Forms.Button();
            this.lbllNandLoader = new System.Windows.Forms.Label();
            this.cmbNandLoaders = new System.Windows.Forms.ComboBox();
            this.lblGameBlockageType = new System.Windows.Forms.Label();
            this.cmbBlockageType = new System.Windows.Forms.ComboBox();
            this.lblError = new System.Windows.Forms.Label();
            this.chkUseProxy = new System.Windows.Forms.CheckBox();
            this.txtProxyServer = new System.Windows.Forms.TextBox();
            this.txtProxyUser = new System.Windows.Forms.TextBox();
            this.txtProxyPass = new System.Windows.Forms.TextBox();
            this.lblProxyServer = new System.Windows.Forms.Label();
            this.lblProxyUser = new System.Windows.Forms.Label();
            this.lblProxyPass = new System.Windows.Forms.Label();
            this.SuspendLayout();
            // 
            // button1
            // 
            this.button1.Location = new System.Drawing.Point(449, 370);
            this.button1.Name = "button1";
            this.button1.Size = new System.Drawing.Size(75, 23);
            this.button1.TabIndex = 40;
            this.button1.Text = "Save";
            this.button1.UseVisualStyleBackColor = true;
            this.button1.Click += new System.EventHandler(this.button1_Click);
            // 
            // lblIpAddress
            // 
            this.lblIpAddress.Location = new System.Drawing.Point(6, 9);
            this.lblIpAddress.Name = "lblIpAddress";
            this.lblIpAddress.Size = new System.Drawing.Size(177, 17);
            this.lblIpAddress.TabIndex = 4;
            this.lblIpAddress.Text = "Ip Address of your Wii";
            this.lblIpAddress.TextAlign = System.Drawing.ContentAlignment.MiddleRight;
            // 
            // txtIpAddress
            // 
            this.txtIpAddress.Location = new System.Drawing.Point(187, 6);
            this.txtIpAddress.Name = "txtIpAddress";
            this.txtIpAddress.Size = new System.Drawing.Size(133, 20);
            this.txtIpAddress.TabIndex = 1;
            this.txtIpAddress.Leave += new System.EventHandler(this.txtIpAddress_Leave);
            // 
            // lstGameNamePreference
            // 
            this.lstGameNamePreference.FormattingEnabled = true;
            this.lstGameNamePreference.Location = new System.Drawing.Point(187, 84);
            this.lstGameNamePreference.Name = "lstGameNamePreference";
            this.lstGameNamePreference.Size = new System.Drawing.Size(246, 121);
            this.lstGameNamePreference.TabIndex = 10;
            this.lstGameNamePreference.SelectedIndexChanged += new System.EventHandler(this.lstGameNamePreference_SelectedIndexChanged);
            // 
            // lblGamePreferenceOrder
            // 
            this.lblGamePreferenceOrder.Location = new System.Drawing.Point(6, 84);
            this.lblGamePreferenceOrder.Name = "lblGamePreferenceOrder";
            this.lblGamePreferenceOrder.Size = new System.Drawing.Size(179, 18);
            this.lblGamePreferenceOrder.TabIndex = 7;
            this.lblGamePreferenceOrder.Text = "Game name preference order";
            this.lblGamePreferenceOrder.TextAlign = System.Drawing.ContentAlignment.MiddleRight;
            this.lblGamePreferenceOrder.Click += new System.EventHandler(this.label2_Click);
            // 
            // btnUp
            // 
            this.btnUp.Location = new System.Drawing.Point(449, 120);
            this.btnUp.Name = "btnUp";
            this.btnUp.Size = new System.Drawing.Size(75, 23);
            this.btnUp.TabIndex = 25;
            this.btnUp.Text = "Up";
            this.btnUp.UseVisualStyleBackColor = true;
            this.btnUp.Click += new System.EventHandler(this.btnUp_Click);
            // 
            // btnDown
            // 
            this.btnDown.Location = new System.Drawing.Point(449, 150);
            this.btnDown.Name = "btnDown";
            this.btnDown.Size = new System.Drawing.Size(75, 23);
            this.btnDown.TabIndex = 30;
            this.btnDown.Text = "Down";
            this.btnDown.UseVisualStyleBackColor = true;
            this.btnDown.Click += new System.EventHandler(this.btnDown_Click);
            // 
            // lbllNandLoader
            // 
            this.lbllNandLoader.Location = new System.Drawing.Point(6, 42);
            this.lbllNandLoader.Name = "lbllNandLoader";
            this.lbllNandLoader.Size = new System.Drawing.Size(179, 18);
            this.lbllNandLoader.TabIndex = 11;
            this.lbllNandLoader.Text = "Select nand loader/base wad : ";
            this.lbllNandLoader.TextAlign = System.Drawing.ContentAlignment.MiddleRight;
            this.lbllNandLoader.Click += new System.EventHandler(this.labelNandLoader_Click);
            // 
            // cmbNandLoaders
            // 
            this.cmbNandLoaders.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.cmbNandLoaders.FormattingEnabled = true;
            this.cmbNandLoaders.Location = new System.Drawing.Point(188, 39);
            this.cmbNandLoaders.Name = "cmbNandLoaders";
            this.cmbNandLoaders.Size = new System.Drawing.Size(245, 21);
            this.cmbNandLoaders.TabIndex = 5;
            // 
            // lblGameBlockageType
            // 
            this.lblGameBlockageType.Location = new System.Drawing.Point(6, 253);
            this.lblGameBlockageType.Name = "lblGameBlockageType";
            this.lblGameBlockageType.Size = new System.Drawing.Size(179, 18);
            this.lblGameBlockageType.TabIndex = 12;
            this.lblGameBlockageType.Text = "Blocked Game List";
            this.lblGameBlockageType.TextAlign = System.Drawing.ContentAlignment.MiddleRight;
            // 
            // cmbBlockageType
            // 
            this.cmbBlockageType.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.cmbBlockageType.FormattingEnabled = true;
            this.cmbBlockageType.Location = new System.Drawing.Point(187, 253);
            this.cmbBlockageType.Name = "cmbBlockageType";
            this.cmbBlockageType.Size = new System.Drawing.Size(246, 21);
            this.cmbBlockageType.TabIndex = 15;
            // 
            // lblError
            // 
            this.lblError.AutoSize = true;
            this.lblError.Font = new System.Drawing.Font("Wingdings", 14.25F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(2)));
            this.lblError.ForeColor = System.Drawing.Color.Red;
            this.lblError.Location = new System.Drawing.Point(321, 6);
            this.lblError.Name = "lblError";
            this.lblError.Size = new System.Drawing.Size(28, 21);
            this.lblError.TabIndex = 31;
            this.lblError.Text = "ı";
            // 
            // chkUseProxy
            // 
            this.chkUseProxy.AutoSize = true;
            this.chkUseProxy.Location = new System.Drawing.Point(187, 297);
            this.chkUseProxy.Name = "chkUseProxy";
            this.chkUseProxy.Size = new System.Drawing.Size(166, 17);
            this.chkUseProxy.TabIndex = 20;
            this.chkUseProxy.Text = "Use Proxy for Internet Access";
            this.chkUseProxy.UseVisualStyleBackColor = true;
            this.chkUseProxy.CheckedChanged += new System.EventHandler(this.chkUseProxy_CheckedChanged);
            // 
            // txtProxyServer
            // 
            this.txtProxyServer.Location = new System.Drawing.Point(187, 320);
            this.txtProxyServer.Name = "txtProxyServer";
            this.txtProxyServer.Size = new System.Drawing.Size(246, 20);
            this.txtProxyServer.TabIndex = 25;
            // 
            // txtProxyUser
            // 
            this.txtProxyUser.Location = new System.Drawing.Point(187, 347);
            this.txtProxyUser.Name = "txtProxyUser";
            this.txtProxyUser.Size = new System.Drawing.Size(100, 20);
            this.txtProxyUser.TabIndex = 30;
            // 
            // txtProxyPass
            // 
            this.txtProxyPass.Location = new System.Drawing.Point(187, 374);
            this.txtProxyPass.Name = "txtProxyPass";
            this.txtProxyPass.PasswordChar = '*';
            this.txtProxyPass.Size = new System.Drawing.Size(100, 20);
            this.txtProxyPass.TabIndex = 35;
            // 
            // lblProxyServer
            // 
            this.lblProxyServer.Location = new System.Drawing.Point(42, 320);
            this.lblProxyServer.Name = "lblProxyServer";
            this.lblProxyServer.Size = new System.Drawing.Size(141, 23);
            this.lblProxyServer.TabIndex = 36;
            this.lblProxyServer.Text = "Proxy Server";
            this.lblProxyServer.TextAlign = System.Drawing.ContentAlignment.MiddleRight;
            // 
            // lblProxyUser
            // 
            this.lblProxyUser.Location = new System.Drawing.Point(42, 346);
            this.lblProxyUser.Name = "lblProxyUser";
            this.lblProxyUser.Size = new System.Drawing.Size(141, 23);
            this.lblProxyUser.TabIndex = 37;
            this.lblProxyUser.Text = "Username";
            this.lblProxyUser.TextAlign = System.Drawing.ContentAlignment.MiddleRight;
            // 
            // lblProxyPass
            // 
            this.lblProxyPass.Location = new System.Drawing.Point(40, 371);
            this.lblProxyPass.Name = "lblProxyPass";
            this.lblProxyPass.Size = new System.Drawing.Size(141, 23);
            this.lblProxyPass.TabIndex = 38;
            this.lblProxyPass.Text = "Password";
            this.lblProxyPass.TextAlign = System.Drawing.ContentAlignment.MiddleRight;
            // 
            // AppConfig
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(624, 414);
            this.Controls.Add(this.lblProxyPass);
            this.Controls.Add(this.lblProxyUser);
            this.Controls.Add(this.lblProxyServer);
            this.Controls.Add(this.txtProxyPass);
            this.Controls.Add(this.txtProxyUser);
            this.Controls.Add(this.txtProxyServer);
            this.Controls.Add(this.chkUseProxy);
            this.Controls.Add(this.lblError);
            this.Controls.Add(this.cmbBlockageType);
            this.Controls.Add(this.lblGameBlockageType);
            this.Controls.Add(this.lbllNandLoader);
            this.Controls.Add(this.cmbNandLoaders);
            this.Controls.Add(this.btnDown);
            this.Controls.Add(this.btnUp);
            this.Controls.Add(this.lblGamePreferenceOrder);
            this.Controls.Add(this.lstGameNamePreference);
            this.Controls.Add(this.button1);
            this.Controls.Add(this.lblIpAddress);
            this.Controls.Add(this.txtIpAddress);
            this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedDialog;
            this.MaximizeBox = false;
            this.Name = "AppConfig";
            this.Text = "Configure";
            this.Load += new System.EventHandler(this.AppConfig_Load);
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.Button button1;
        private System.Windows.Forms.Label lblIpAddress;
        private System.Windows.Forms.TextBox txtIpAddress;
        private System.Windows.Forms.ListBox lstGameNamePreference;
        private System.Windows.Forms.Label lblGamePreferenceOrder;
        private System.Windows.Forms.Button btnUp;
        private System.Windows.Forms.Button btnDown;
        private System.Windows.Forms.Label lbllNandLoader;
        private System.Windows.Forms.ComboBox cmbNandLoaders;
        private System.Windows.Forms.Label lblGameBlockageType;
        private System.Windows.Forms.ComboBox cmbBlockageType;
        private System.Windows.Forms.Label lblError;
        private System.Windows.Forms.CheckBox chkUseProxy;
        private System.Windows.Forms.TextBox txtProxyServer;
        private System.Windows.Forms.TextBox txtProxyUser;
        private System.Windows.Forms.TextBox txtProxyPass;
        private System.Windows.Forms.Label lblProxyServer;
        private System.Windows.Forms.Label lblProxyUser;
        private System.Windows.Forms.Label lblProxyPass;
    }
}