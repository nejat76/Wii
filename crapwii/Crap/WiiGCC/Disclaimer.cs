using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Text;
using System.Windows.Forms;
using Org.Irduco.MultiLanguage;
using System.Configuration;

namespace WiiGSC
{
    public partial class Disclaimer : Form
    {
        public MultiLanguageModuleHelper guiLang;

        public Disclaimer()
        {
            InitializeComponent();
        }

        private void Disclaimer_Load(object sender, EventArgs e)
        {
            this.Text = this.guiLang.Translate("DISCLAIMERHEADER");
            this.lblDisclaimerText.Text = this.guiLang.TranslateAndReplace("DISCLAIMER", @"\n", "\n");
            this.chkDisclaimer.Text = this.guiLang.Translate("HIDE_DISCLAIMER");
        }

        private void Disclaimer_FormClosing(object sender, FormClosingEventArgs e)
        {
            if (chkDisclaimer.Checked)
            {
                saveConfig();
            }
        }

        private void saveConfig()
        {
            System.Configuration.Configuration config = null;
            try
            {
                config = ConfigurationManager.OpenExeConfiguration(ConfigurationUserLevel.None);
                config.AppSettings.Settings.Remove("Disclaimer");
                config.AppSettings.Settings.Add("Disclaimer", "off");
                config.Save(ConfigurationSaveMode.Modified);
            }
            catch (Exception ex)
            {
                MessageBox.Show(this.guiLang.TranslateAndReplace("SAVEERROR", @"\n", "\n") + ": " + ex.Message, "Error", MessageBoxButtons.OK, MessageBoxIcon.Hand);
            }
        }

    }
}
