// Copyright 2010 Nejat Dilek  <imruon@gmail.com>
// Licensed under the terms of the GNU GPL, version 2
// http://www.gnu.org/licenses/old-licenses/gpl-2.0.txt
namespace WiiGSC
{    
    using System;
    using System.ComponentModel;
    using System.Configuration;
    using System.Drawing;
    using System.Windows.Forms;
    using System.Collections.Generic;
    using Org.Irduco.MultiLanguage;
    using Org.Irduco.UpdateManager;
using System.Text.RegularExpressions;

    public partial class AppConfig : Form
    {
        public MultiLanguageModuleHelper guiLang;
        public MultiLanguageModuleHelper mainLang;
        private List<string> nandLoaders;

        public enum BannerLanguage
        {
            Japanese,
            English,
            German,
            French,
            Spanish,
            Italian,
            Dutch,
            Unknown1,
            Unknown2,
            Korean
        }

        public string[] bannerLanguageKeys = { "MENU_LANG_JAPANESE", "MENU_LANG_ENGLISH", "MENU_LANG_DEUTSCH", "MENU_LANG_FRENCH", "MENU_LANG_SPANISH", "MENU_LANG_ITALIAN", "MENU_LANG_DUTCH", "", "", "MENU_LANG_KOREAN" };
        public string[] gameBlockageTypeKeys = { "BLOCKING_TYPE_NONE", "BLOCKING_TYPE_INTERNET_THEN_LOCAL", "BLOCKING_TYPE_INTERNET",  "BLOCKING_TYPE_LOCAL" };


        public AppConfig()
        {
            this.InitializeComponent();
        }

        public AppConfig(List<string> nandLoaders)
        {
            this.InitializeComponent();
            this.nandLoaders = nandLoaders;
        }

        private void AppConfig_Load(object sender, EventArgs e)
        {
            this.loadMLResources();
            NandLoaderConfigLoad();
            LanguagePreferencesConfigLoad();
            GameBlockageTypeConfigLoad();
            ProxyPreferencesLoad();

            lblError.Visible = false;
            this.txtIpAddress.Text = ConfigurationSettings.AppSettings["IpAddressOfWii"];
            this.btnDown.Enabled = false;
            this.btnUp.Enabled = false;
        }

        private void button1_Click(object sender, EventArgs e)
        {
            this.saveConfig();
        }

        private void loadMLResources()
        {
            this.button1.Text = this.guiLang.Translate("SAVE");
            this.Text = this.guiLang.Translate("CONFIGURE");
            this.lblIpAddress.Text = this.guiLang.Translate("IPADDRESS") + " :";
            this.lbllNandLoader.Text = this.guiLang.Translate("NANDLOADERSELECTION") + " :";
            this.lblGamePreferenceOrder.Text = this.guiLang.Translate("GAME_PREFERENCE_ORDER") + " :";
            this.lblGameBlockageType.Text = this.guiLang.Translate("GAME_BLOCKAGE_TYPE") + " :";
            this.btnDown.Text = this.guiLang.Translate("LANGUAGE_DOWN");
            this.btnUp.Text = this.guiLang.Translate("LANGUAGE_UP");
            this.lblProxyPass.Text = this.guiLang.Translate("PROXY_PASS");
            this.lblProxyUser.Text = this.guiLang.Translate("PROXY_USER");
            this.lblProxyServer.Text = this.guiLang.Translate("PROXY_SERVER");
            this.chkUseProxy.Text = this.guiLang.Translate("USE_PROXY");
        }

        private void saveConfig()
        {
            if (ValidateIpAddress())
            {
                System.Configuration.Configuration configuration = null;
                try
                {
                    configuration = ConfigurationManager.OpenExeConfiguration(ConfigurationUserLevel.None);
                    configuration.AppSettings.Settings.Remove("IpAddressOfWii");
                    configuration.AppSettings.Settings.Add("IpAddressOfWii", this.txtIpAddress.Text);
                    string bannerLanguagePreference = GetBannerLanguagePreference();
                    configuration.AppSettings.Settings.Remove("BannerLanguagePreference");
                    configuration.AppSettings.Settings.Add("BannerLanguagePreference", bannerLanguagePreference);
                    configuration.AppSettings.Settings.Remove("SelectedNandLoader");
                    configuration.AppSettings.Settings.Add("SelectedNandLoader", this.cmbNandLoaders.SelectedItem.ToString());
                    configuration.AppSettings.Settings.Remove("GameBlockageType");
                    configuration.AppSettings.Settings.Add("GameBlockageType", this.cmbBlockageType.SelectedIndex.ToString());



                    configuration.AppSettings.Settings.Remove("UseProxy");
                    configuration.AppSettings.Settings.Add("UseProxy", this.chkUseProxy.Checked ? "on" : "off");

                    configuration.AppSettings.Settings.Remove("ProxyUser");
                    configuration.AppSettings.Settings.Add("ProxyUser", this.txtProxyUser.Text.ToString());

                    configuration.AppSettings.Settings.Remove("ProxyPass");
                    configuration.AppSettings.Settings.Add("ProxyPass", this.txtProxyPass.Text.ToString());

                    configuration.AppSettings.Settings.Remove("ProxyServer");
                    configuration.AppSettings.Settings.Add("ProxyServer", this.txtProxyServer.Text.ToString());


                    configuration.Save(ConfigurationSaveMode.Modified);
                    ConfigurationManager.RefreshSection("appSettings");
                    base.Close();
                }
                catch (Exception exception)
                {
                    MessageBox.Show(this.guiLang.TranslateAndReplace("SAVEERROR", @"\n", "\n") + ": " + exception.Message, "Error", MessageBoxButtons.OK, MessageBoxIcon.Hand);
                }
            }
        }

        private string GetBannerLanguagePreference()
        {
            string preferences = "";
            for (int i = 0; i < lstGameNamePreference.Items.Count; i++)
            {
                ComboItem item = (ComboItem)lstGameNamePreference.Items[i];
                preferences = preferences + item.value + ",";
            }
            return preferences.Substring(0, preferences.Length - 1);
        }

        //Up button click
        private void btnUp_Click(object sender, EventArgs e)
        {
            ExchangeItem(-1);
        }

        //Down button click
        private void btnDown_Click(object sender, EventArgs e)
        {            
            ExchangeItem(1);
        }

        private void ExchangeItem(int neighbourRange)
        {
            int index = this.lstGameNamePreference.SelectedIndex;
            ComboItem tempItem = (ComboItem)this.lstGameNamePreference.Items[index];
            this.lstGameNamePreference.Items[index] = this.lstGameNamePreference.Items[index + neighbourRange];
            this.lstGameNamePreference.Items[index + neighbourRange] = tempItem;
            this.lstGameNamePreference.SelectedIndex = index + neighbourRange;
        }

        private void lstGameNamePreference_SelectedIndexChanged(object sender, EventArgs e)
        {
            int index = this.lstGameNamePreference.SelectedIndex;

            if (index == 0)
            {
                btnUp.Enabled = false;
                btnDown.Enabled = true;
            }
            else if (index == (this.lstGameNamePreference.Items.Count - 1))
            {
                btnDown.Enabled = false;
                btnUp.Enabled = true;
            }
            else
            {
                btnDown.Enabled = true;
                btnUp.Enabled = true;
            }
        }


        private void NandLoaderConfigLoad() 
        {
            string selectedNandLoader = null;
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

        private void ProxyPreferencesLoad()
        {
            string useProxy = ConfigurationSettings.AppSettings["UseProxy"];

            this.txtProxyServer.Text = ConfigurationSettings.AppSettings["ProxyServer"];
            this.txtProxyServer.Enabled = true;
            this.txtProxyUser.Text = ConfigurationSettings.AppSettings["ProxyUser"];
            this.txtProxyUser.Enabled = true;
            this.txtProxyPass.Text = ConfigurationSettings.AppSettings["ProxyPass"];
            this.txtProxyPass.Enabled = true;

            if (useProxy == "on")
            {
                this.chkUseProxy.Checked = true;
                this.txtProxyServer.Enabled = true;
                this.txtProxyUser.Enabled = true;
                this.txtProxyPass.Enabled = true;
            }
            else
            {
                this.chkUseProxy.Checked = false;
                this.txtProxyServer.Enabled = false;
                this.txtProxyUser.Enabled = false;
                this.txtProxyPass.Enabled = false;
            }
        }

        private void GameBlockageTypeConfigLoad()
        {
            for (int i = (int)BlockedGamesManager.BlockedGameType.None; i <= (int)BlockedGamesManager.BlockedGameType.Local; i++)
            {
                ComboItem item = new ComboItem(i, guiLang.Translate(gameBlockageTypeKeys[i]));
                this.cmbBlockageType.Items.Add(item);
            }

            string selectedBlockageType = ConfigurationSettings.AppSettings["GameBlockageType"];

            this.cmbBlockageType.SelectedIndex = Convert.ToInt32(selectedBlockageType);
        }

        private void LanguagePreferencesConfigLoad()
        {
            string bannerLangugagePreference = ConfigurationSettings.AppSettings["BannerLanguagePreference"];
            string[] listPreference = bannerLangugagePreference.Split(new char[] { ',' });
            int[] listPreferenceInt = Array.ConvertAll<string, int>(listPreference, delegate(string str) { return int.Parse(str); });

            for (int i = 0; i < listPreferenceInt.Length; i++)
            {
                int prefererredLang = listPreferenceInt[i];
                ComboItem item = new ComboItem(prefererredLang, this.mainLang.Translate(bannerLanguageKeys[prefererredLang]));
                this.lstGameNamePreference.Items.Add(item);
            }

            for (int i = 0; i <= (int)BannerLanguage.Korean; i++)
            {
                if (Array.IndexOf(listPreferenceInt, i) < 0)
                {
                    if ((i != (int)BannerLanguage.Unknown1) && (i != (int)BannerLanguage.Unknown2))
                    {
                        ComboItem item = new ComboItem(i, this.mainLang.Translate(bannerLanguageKeys[i]));
                        this.lstGameNamePreference.Items.Add(item);
                    }
                }
            }

        }

        private void labelNandLoader_Click(object sender, EventArgs e)
        {

        }

        private void label2_Click(object sender, EventArgs e)
        {

        }

        private void txtIpAddress_Leave(object sender, EventArgs e)
        {
            ValidateIpAddress();
        }

        private bool ValidateIpAddress()
        {
            string ipAddressRegex = @"\b(?:(?:25[0-5]|2[0-4][0-9]|[01]?[0-9][0-9]?)\.){3}(?:25[0-5]|2[0-4][0-9]|[01]?[0-9][0-9]?)\b";
            RegexStringValidator validator = new RegexStringValidator(ipAddressRegex);
            Regex regex = new Regex(ipAddressRegex);

            if (!regex.IsMatch(txtIpAddress.Text))
            {
                lblError.Visible = true;
                txtIpAddress.SelectAll();
                txtIpAddress.Focus();
                return false;
            }
            else
            {
                return true;
                lblError.Visible = false;
            }
        }

        private void button2_Click(object sender, EventArgs e)
        {

        }

        private void chkUseProxy_CheckedChanged(object sender, EventArgs e)
        {
            if (!chkUseProxy.Checked)
            {
                this.txtProxyServer.Enabled = false;
                this.txtProxyUser.Enabled = false;
                this.txtProxyPass.Enabled = false;
            }
            else
            {
                this.txtProxyServer.Enabled = true;
                this.txtProxyUser.Enabled = true;
                this.txtProxyPass.Enabled = true;
            }
        }

    }


    public class ComboItem
    {
        public int value;
        public string text;

        public ComboItem(int value, string text)
        {
            this.value = value;
            this.text = text;
        }

        public override string ToString()
        {
            return this.text;
        }
    }

}

