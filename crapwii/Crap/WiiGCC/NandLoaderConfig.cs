// Copyright 2010 Nejat Dilek  <imruon@gmail.com>
// Licensed under the terms of the GNU GPL, version 2
// http://www.gnu.org/licenses/old-licenses/gpl-2.0.txt
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
    public partial class NandLoaderConfig : Form
    {
        // Fields
        public MultiLanguageModuleHelper guiLang;
        private List<string> nandLoaders;

        // Methods
        public NandLoaderConfig()
        {
            this.InitializeComponent();
        }

        public NandLoaderConfig(List<string> nandLoaders)
        {
            this.InitializeComponent();
            this.nandLoaders = nandLoaders;
        }

        private void button1_Click(object sender, EventArgs e)
        {
            this.saveConfig();
        }


        private void loadMLResources()
        {
            this.label1.Text = this.guiLang.Translate("NANDLOADERSELECTION") + " :";
            this.button1.Text = this.guiLang.Translate("SAVE");
            this.Text = this.guiLang.Translate("NANDLOADERCONFIGURETITLE");
        }

        private void NandLoaderConfig_Load(object sender, EventArgs e)
        {
            string selectedNandLoader = null;
            this.loadMLResources();
            selectedNandLoader = ConfigurationSettings.AppSettings["SelectedNandLoader"];
            this.cmbNandLoaders.Items.Clear();
            for (int i = 0; i < this.nandLoaders.Count; i++)
            {
                this.cmbNandLoaders.Items.Add(this.nandLoaders[i]);
                if (selectedNandLoader.Equals(this.nandLoaders[i]))
                {
                    this.cmbNandLoaders.SelectedIndex = i;
                }
            }
        }

        private void saveConfig()
        {
            System.Configuration.Configuration config = null;
            try
            {
                config = ConfigurationManager.OpenExeConfiguration(ConfigurationUserLevel.None);
                config.AppSettings.Settings.Remove("SelectedNandLoader");
                config.AppSettings.Settings.Add("SelectedNandLoader", this.cmbNandLoaders.SelectedItem.ToString());
                config.Save(ConfigurationSaveMode.Modified);
                ConfigurationManager.RefreshSection("appSettings");
                base.Close();
            }
            catch (Exception ex)
            {
                MessageBox.Show(this.guiLang.TranslateAndReplace("SAVEERROR", @"\n", "\n") + ": " + ex.Message, "Error", MessageBoxButtons.OK, MessageBoxIcon.Hand);
            }
        }


    }
}
