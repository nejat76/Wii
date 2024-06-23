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
using Org.Irduco.UpdateManager;
using Org.Irduco.MultiLanguage;
using Org.Irduco.LoaderManager;
using System.Threading;
using System.Configuration;
using System.IO;
using Wii;
using System.Diagnostics;
using Org.Irduco.Text;
using System.Text.RegularExpressions;
using WBFSSync;

namespace WiiGSC
{
    public partial class Form1 : Form
    {
        private int altDolCount;
        private AppDomain appDomain;
        private string baseDirectory;
        private BlockedGamesManager blockedGamesManager;
        private string bootingDol;
        private string defaultIpAddressOfWii;
        private string discId;
        private static int failed = 0;
        private InfoForm frm;
        private MultiLanguageHelper mlHelper;
        private MultiLanguageModuleHelper guiLang;
        private MultiLanguageModuleHelper interfaceLang;
        private MultiLanguageModuleHelper settingsLang;
        private Wiiload wiiload;
        private MultiLanguageModuleHelper wiiloadLang;
        private MultiLanguageModuleHelper updatesLang;
        private string lastPackedWad;
        private LoaderHelper loaderHelper;
        private string openFileName;
        private bool programModeChannel;
        private string selectedLanguage;
        private static int successful = 0;
        private string titleIdN;

        private Thread trd;

        // Methods
        public Form1(AppDomain appDomain)
        {
            this.appDomain = appDomain;
            this.selectedLanguage = ConfigurationManager.AppSettings["language"];
            this.baseDirectory = this.appDomain.BaseDirectory;
            string[] args = Environment.GetCommandLineArgs();
            if (args.Length>1)
            {
                this.discId = args[1];
                this.discId = this.discId.Trim();
            }
            else
            {
                this.discId = "";
            }
            this.InitializeComponent();
            this.changeLanguage(this.selectedLanguage, false);
            this.loaderHelper = new LoaderHelper(this.baseDirectory + @"\" + "LoaderConfig.xml");
            this.addLoaders();
            this.loadMLResources();
        }

        //TODO: Convert thread killing stuff
        //private void ~Form1()
        //{
        //    if (this.components != null)
        //    {
        //        IDisposable components = this.components;
        //        if (components != null)
        //        {
        //            components.Dispose();
        //        }
        //    }
        //    if ((this.trd != null) && this.trd.IsAlive)
        //    {
        //        this.trd.Abort();
        //    }
        //}

        private void addChannelLoaders()
        {
            List<ChannelLoader> channelLoaders = null;
            channelLoaders = this.loaderHelper.ChannelLoaders;
            for (int i = 0; i < channelLoaders.Count; i++)
            {
                this.cmbLoaders.Items.Add(channelLoaders[i].Title);
            }
        }

        private void addDiscLoaders()
        {
            List<Loader> loaders = null;
            loaders = this.loaderHelper.DiscLoaders;
            for (int i = 0; i < loaders.Count; i++)
            {
                this.cmbLoaders.Items.Add(loaders[i].Title);
            }
        }

        private void addLoaders()
        {
            this.cmbLoaders.Items.Clear();
            this.hideOptionPanels();
            this.addDiscLoaders();
            this.addChannelLoaders();
        }

        private void bannerSelect()
        {
            if (this.openSaveFileDialog.ShowDialog() == DialogResult.OK)
            {
                if (this.altDolCount < 0)
                {
                    this.altDolCount = 0;
                }
                if (!this.handleBannerChange(this.openSaveFileDialog.FileName, string.Empty))
                {
                    MessageBox.Show(this.interfaceLang.Translate("INVALIDBANNERHEADER"), this.interfaceLang.Translate("ERROR"), MessageBoxButtons.OK, MessageBoxIcon.Hand);
                }
            }
            base.Invalidate();
        }
        
        private bool BatchCreate()
        {
            ChannelPackParams parameters = null;
            string[] banners = null;
            ParameterizedThreadStart myThreadDelegate = null;
            StatusUpdater updater = null;
            updater = new StatusUpdater(this.UpdateStats);
            banners = new string[this.listBox1.Items.Count];
            for (int i = 0; i < this.listBox1.Items.Count; i++)
            {
                banners[i] = (string) this.listBox1.Items[i];
            }
            parameters = this.SetLoaderOptions(true, banners, updater);
            if (parameters != null)
            {
                this.toolStripProgressBar1.Visible = true;
                this.toolStripProgressBar1.ProgressBar.Minimum = 0;
                this.toolStripProgressBar1.ProgressBar.Maximum = this.listBox1.Items.Count - 1;
                myThreadDelegate = new ParameterizedThreadStart(this.MyTask);
                this.trd = new Thread(myThreadDelegate);
                this.trd.IsBackground = false;
                this.trd.Start(parameters);
                return true;
            }
            return false;
        }

        private void BatchModePrepare()
        {
            this.panelBatch.Visible = true;
            this.listBox1.Visible = true;
            this.btnBatchCreate.Visible = true;
            this.txtTitleId.Visible = false;
            this.txtDiscId.Visible = false;
            this.btnCreate.Visible = false;
            this.btnTest.Visible = false;
        }

        private void btnBatchCreate_Click(object sender, EventArgs e)
        {
            this.toolStripStatusLabel1.Text = "";
            successful = 0;
            failed = 0;
            if (this.BatchCreate())
            {
                this.btnBatchCreate.Visible = false;
                this.btnDismiss.Visible = true;
            }
        }

        private void btnBrowse_Click(object sender, EventArgs e)
        {
            this.bannerSelect();
        }

        private void btnChannelSelect_Click(object sender, EventArgs e)
        {
            this.handleChannel();
        }

        private void btnCreateSelected_Click_1(object sender, EventArgs e)
        {
            string selectedDrive = null;
            string fullPath = null;
            string selectedGameFullPath = null;
            string selectedDiscId = null;
            string[] banners = null;
            string selectedGame = null;
            if (this.listGames.SelectedItems.Count == 0)
            {
                MessageBox.Show(this.interfaceLang.Translate("SELECTGAME"), this.interfaceLang.Translate("ERROR"), MessageBoxButtons.OK, MessageBoxIcon.Hand);
            }
            else if (this.listGames.SelectedItems.Count == 1)
            {
                selectedGame = this.listGames.SelectedItems[0].SubItems[2].Text;
                selectedDiscId = this.listGames.SelectedItems[0].SubItems[1].Text;
                if (this.cmbDriveList.SelectedItem != null)
                {
                    selectedDrive = ((string) this.cmbDriveList.SelectedItem).Substring(0, 1);
                }
                else
                {
                    MessageBox.Show(this.interfaceLang.Translate("SELECTDRIVEERROR"), this.interfaceLang.Translate("ERROR"), MessageBoxButtons.OK, MessageBoxIcon.Hand);
                    return;
                }
                if (this.IsFatOrNtfsWithWbfsFolder(selectedDrive))
                {
                    selectedGameFullPath = this.listGames.SelectedItems[0].SubItems[4].Text;
                    this.parseWbfsFile(selectedGameFullPath, false);
                }
                else
                {
                    this.parseWbfsFileSystem(selectedDrive, selectedDiscId, false, true);
                }
            }
            else
            {
                this.listBox1.Items.Clear();
                banners = new string[this.listGames.SelectedItems.Count];
                for (int i = 0; i < this.listGames.SelectedItems.Count; i++)
                {
                    fullPath = this.listGames.SelectedItems[i].SubItems[4].Text;
                    this.listBox1.Items.Add(fullPath);
                }
                this.panelWBFS.Hide();
                this.BatchModePrepare();
            }
        }

        private void btnDismiss_Click(object sender, EventArgs e)
        {
            this.btnDismiss.Visible = false;
            this.listBox1.Items.Clear();
            this.listBox1.Visible = false;
            this.txtTitleId.Visible = true;
            this.txtDiscId.Visible = true;
            this.btnCreate.Visible = true;
            this.btnTest.Visible = true;
            this.panelBatch.Visible = false;
            successful = 0;
            failed = 0;
            this.toolStripStatusLabel1.Text = "";
            this.toolStripProgressBar1.ProgressBar.Value = 0;
            this.toolStripProgressBar1.Visible = false;
        }

        private void btnGetStuff_Click(object sender, EventArgs e)
        {
            this.parseIso();
        }

        private void btnRefresh_Click_1(object sender, EventArgs e)
        {
            this.refreshDrives();
        }

        private void btnSelectIso_Click(object sender, EventArgs e)
        {
            this.isoSelection();
        }

        private void btnTest_Click(object sender, EventArgs e)
        {
            byte[] fileContent = null;
            try
            {
                fileContent = File.ReadAllBytes(this.bootingDol);
                this.wiiload = new Wiiload();
                this.wiiload.dolData = fileContent;
                this.wiiload.dolSize = fileContent.Length;
                this.wiiload.packedWadPath = this.lastPackedWad;
                this.wiiload.defaultIp = ConfigurationSettings.AppSettings["IpAddressOfWii"];
                this.wiiload.appPath = this.appDomain.BaseDirectory;
                this.wiiload.guiLang = this.wiiloadLang;
                this.wiiload.ShowDialog(this);
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message, this.interfaceLang.Translate("ERROR"), MessageBoxButtons.OK, MessageBoxIcon.Hand);
            }
        }

        private void button1_Click_1(object sender, EventArgs e)
        {
            this.viewWBFSDrive();
        }

        private void button2_Click(object sender, EventArgs e)
        {
            this.showInfoWindow();
        }

        private void button2_Click_1(object sender, EventArgs e)
        {
            string fullPath = null;
            string[] banners = null;
            if (this.listGames.Items.Count == 0)
            {
                MessageBox.Show(this.interfaceLang.Translate("NOT_NOW"), this.interfaceLang.Translate("ERROR"), MessageBoxButtons.OK, MessageBoxIcon.Hand);
            }
            else
            {
                this.listBox1.Items.Clear();
                banners = new string[this.listGames.Items.Count];
                for (int i = 0; i < this.listGames.Items.Count; i++)
                {
                    fullPath = this.listGames.Items[i].SubItems[4].Text;
                    this.listBox1.Items.Add(fullPath);
                }
                this.panelWBFS.Hide();
                this.BatchModePrepare();
            }
        }

        private void button3_Click(object sender, EventArgs e)
        {
        }

        private void button4_Click(object sender, EventArgs e)
        {
        }

        private void button4_Click_1(object sender, EventArgs e)
        {
            this.panelWBFS.Visible = false;
        }

        private void button6_Click(object sender, EventArgs e)
        {
            ChannelPackParams packParams = null;
            string wadName = null;
            string selectedDiscId = null;
            string error = null;
            string selectedTitleId = null;
            StatusUpdater updater = null;
            string[] banners = null;
            string bannerFilename = null;
            bannerFilename = this.txtDataFile.Text;
            banners = new string[] { bannerFilename };
            updater = new StatusUpdater(this.NoUpdateStats);
            packParams = this.SetLoaderOptions(false, banners, updater);
            if (packParams != null)
            {
                selectedDiscId = this.txtDiscId.Text;
                selectedTitleId = this.txtTitleId.Text;
                if (packParams.wadNaming == 0)
                {
                    wadName = this.txtDiscId.Text;
                }
                else if (packParams.wadNaming == 1)
                {
                    wadName = this.lblGameName.Text + " - " + selectedDiscId;
                }
                else if (packParams.wadNaming == 2)
                {
                    wadName = (this.lblGameName.Text + " - " + selectedTitleId) + " - " + selectedDiscId;
                }
                else
                {
                    wadName = this.txtDiscId.Text;
                }
                //wadName = this.replaceUnicode(wadName);
                wadName = this.stripSpecialCharactersFromFilename(wadName);
                error = this.PackWad(0, 0, packParams.regionOverrideEnabled, packParams.selectedRegion, packParams.forceVideo, packParams.verboseLog, packParams.ocarinaEnabled, packParams.forceLanguage, packParams.selectedLanguage, packParams.forceLoader, packParams.fixes, packParams.selectedLoader, packParams.banners[0], selectedDiscId, selectedTitleId, wadName, updater, packParams.altDolType, packParams.altDolFile, packParams.altDolOffset, packParams.isForwarder, packParams.selectedPartition, packParams.forwarderParameters);
                if (!string.IsNullOrEmpty(error))
                {
                    MessageBox.Show(this.interfaceLang.Translate("WADPACKERROR") + " : " + error, this.interfaceLang.Translate("ERROR"), MessageBoxButtons.OK, MessageBoxIcon.Hand);
                }
                else
                {
                    btnTest.Enabled = true;
                }
            }
        }

        private void changeLanguage(string language)
        {
            this.changeLanguage(language, true);
        }

        public void RefreshLanguage()
        {
            changeLanguage(mlHelper.Language, false);
        }

        private void changeLanguage(string language,  bool saveConfigFile)
        {
            MultiLanguageHelper newHelper = null;
            newHelper = new MultiLanguageHelper(language, (this.baseDirectory + @"\Lang\") + language + ".xml");
            if (newHelper.IsLoaded)
            {
                this.mlHelper = newHelper;
                this.ReflectLanguageChanges();
                if (saveConfigFile)
                {
                    this.saveConfig(language);
                }
            }
            else
            {
                MessageBox.Show(this.interfaceLang.TranslateAndReplace("LANGNOTFOUND", @"\n", "\n"), this.interfaceLang.Translate("ERROR"), MessageBoxButtons.OK, MessageBoxIcon.Hand);
            }
        }

        //private bool checkBanner(string bannerFile, out string gameName)
        //{
        //    gameName = "Dr.Mario";
        //    return true;
        //}

		 bool checkBanner(string bannerFile, out string gameName) 
		 {
			 FileStream stream = null;
             gameName = String.Empty;

			 try 
			 {
				 stream = File.OpenRead(bannerFile);
				 
				 stream.Seek(64, SeekOrigin.Begin); //Seek to the header... we should find IMET there...				 
				 int h1 = stream.ReadByte();
				 int h2 = stream.ReadByte();
				 int h3 = stream.ReadByte();
				 int h4 = stream.ReadByte();

 				 stream.Seek(128, SeekOrigin.Begin); //Or we can find it here... we should find IMET there...
				 int h5 = stream.ReadByte();
				 int h6 = stream.ReadByte();
				 int h7 = stream.ReadByte();
				 int h8 = stream.ReadByte();

				 int nameOffset;
				 if ( ((h1=='I') && (h2 == 'M') && (h3 == 'E') && (h4=='T')) )
				 {
					 nameOffset = 0x5C;
				 } else 
				 {
					 if (((h5=='I') && (h6 == 'M') && (h7 == 'E') && (h8=='T')) ) 
					 {
						 nameOffset = 0x9C;
					 } else {
						stream.Close();
						return false;
					}
				 }
				
				//Read the name from banner...
				 stream.Seek(nameOffset, SeekOrigin.Begin);

				 //sr = gcnew ^StreamReader(stream);
				 byte[] name = new byte[84];
				 //array<unsigned char^> englishName[84];
				 string[] names = new string[10];
				 for (int j=0;j<10;j++) 
				 {
					 stream.Read(name, 0, 84); 
					 for (int i=0;i<42;i++) 
					 {
						 //Convert double nulls to spaces.
						 if ((name[i*2] == 0) && (name[i*2+1] == 0) ) 
						 {
							 name[i*2] = 0;
							 name[i*2+1] = 0x20; //space
						 }
					 }
					 names[j] = System.Text.Encoding.BigEndianUnicode.GetString(name).Trim('\0');
					 for (int k=0;k<6;k++) 
					 {
						names[j] = names[j].Replace("  ", " ");
					 }
				 }
				 stream.Close();

                 gameName = SetPreferredName(gameName, names);

				 return true;

			 } catch (Exception ex) 
			 {
				 if ((stream!=null) && (stream.CanRead)) 
				 {
					 stream.Close();
				 }

				 //MessageBox::Show(interfaceLang->Translate("BANNERCHECKERROR") + " : " + ex->Message, interfaceLang->Translate("ERROR"), MessageBoxButtons::OK, MessageBoxIcon::Error);
				 return false;

			 }
		 }

         private static string SetPreferredName(string gameName, string[] names)
         {
             ////First try to use the english name, if it's empty look for deutsch, if that's empty too
             ////Then revert to the japanese name
             ////MessageBox::Show(Convert::ToString(names[1]->Length), "Information", MessageBoxButtons::OK, MessageBoxIcon::Information);
             //if (names[1].Length != 0)
             //{
             //    gameName = names[1];
             //}
             //else if (!(String.IsNullOrEmpty(names[2].Trim())))
             //{
             //    gameName = names[2];
             //}
             //else
             //{
             //    gameName = names[0];
             //}
             //return gameName;             

             string bannerLangugagePreference = ConfigurationSettings.AppSettings["BannerLanguagePreference"];
             string[] listPreference = bannerLangugagePreference.Split(new char[] { ',' });
             int[] listPreferenceInt = Array.ConvertAll<string, int>(listPreference, delegate(string str) { return int.Parse(str); });

             for (int i = 0; i < listPreferenceInt.Length; i++)
             {
                 if (!String.IsNullOrEmpty(names[listPreferenceInt[i]].Trim()) )
                 {
                     return names[listPreferenceInt[i]].Trim();
                 }
             }

             return String.Empty;
         }


         //private bool checkBanner(string bannerFile, out string gameName)
         //{
         //    FileStream stream = null;
         //    string[] names = null;
         //    byte[] name = null;
         //    bool flag;
         //    try
         //    {
         //        int nameOffset;
         //        stream = File.OpenRead(bannerFile);
         //        stream.Seek(0x40L, SeekOrigin.Begin);
         //        int h1 = stream.ReadByte();
         //        int h2 = stream.ReadByte();
         //        int h3 = stream.ReadByte();
         //        int h4 = stream.ReadByte();
         //        stream.Seek(0x80L, SeekOrigin.Begin);
         //        int h5 = stream.ReadByte();
         //        int h6 = stream.ReadByte();
         //        int h7 = stream.ReadByte();
         //        int h8 = stream.ReadByte();
         //        if (((h1 == 0x49) && (h2 == 0x4d)) && ((h3 == 0x45) && (h4 == 0x54)))
         //        {
         //            nameOffset = 0x5c;
         //        }
         //        else if (((h5 == 0x49) && (h6 == 0x4d)) && ((h7 == 0x45) && (h8 == 0x54)))
         //        {
         //            nameOffset = 0x9c;
         //        }
         //        else
         //        {
         //            stream.Close();
         //            return false;
         //        }
         //        stream.Seek((long) nameOffset, SeekOrigin.Begin);
         //        name = new byte[0x54];
         //        names = new string[7];
         //        int i = 0;
         //        goto Label_00D8;
         //    Label_00D2:
         //        i++;
         //    Label_00D8:
         //        if (i >= 7)
         //        {
         //            goto Label_016A;
         //        }
         //        stream.Read(name, 0, 0x54);
         //        goto Label_00F3;
         //    Label_00EF:
         //        i++;
         //    Label_00F3:
         //        if (i < 0x2a)
         //        {
         //            if ((name[i * 2] == 0) && (name[(i * 2) + 1] == 0))
         //            {
         //                name[i * 2] = 0;
         //                name[(i * 2) + 1] = 0x20;
         //            }
         //            goto Label_00EF;
         //        }
         //        char[] trimChars = new char[] { '\0' };
         //        names[i] = Encoding.BigEndianUnicode.GetString(name).Trim(trimChars);
         //        int j = 0;
         //        goto Label_0147;
         //    Label_0141:
         //        j++;
         //    Label_0147:
         //        if (j >= 6)
         //        {
         //            goto Label_00D2;
         //        }
         //        names[i] = names[i].Replace("  ", " ");
         //        goto Label_0141;
         //    Label_016A:
         //        stream.Close();
         //        if (names[1].Length != 0)
         //        {
         //            gameName[0] = names[1];
         //        }
         //        else if (!string.IsNullOrEmpty(names[2].Trim()))
         //        {
         //            gameName[0] = names[2];
         //        }
         //        else
         //        {
         //            gameName[0] = names[0];
         //        }
         //        flag = true;
         //    }
         //    catch (Exception)
         //    {
         //        if ((stream != null) && stream.CanRead)
         //        {
         //            stream.Close();
         //        }
         //        return false;
         //    }
         //    return flag;
         //}

         private void cmbDriveList_SelectedIndexChanged(object sender, EventArgs e)
        {
            this.GetDriveContents();
        }

        private void cmbLoaders_SelectedIndexChanged(object sender, EventArgs e)
        {
            int count = this.loaderHelper.DiscLoaders.Count;
            for (int i = 0; i < count; i++)
            {
                this.SetSelectedLoader(true, i, this.loaderHelper.DiscLoaders[i]);
            }
            int channelloadercount = this.loaderHelper.ChannelLoaders.Count;
            for (int i = 0; i < channelloadercount; i++)
            {
                this.SetSelectedLoader(false, i, this.loaderHelper.ChannelLoaders[i]);
            }
        }

        private void cmbPartition_SelectedIndexChanged(object sender, EventArgs e)
        {
            string partition = null;
            if (this.cmbPartition.Text != String.Empty)
            {
                partition = this.cmbPartition.Text.ToString().Substring(4);
                this.txtExtraParameters.Text = "partition=" + partition;
            }
            else
            {
                this.txtExtraParameters.Text = "";
            }
        }

        private void CreateCommonKey()
        {
            List<string> allLinesColl = null;
            byte[] hiddenData = null;
            string carrier = null;
            WordStegoLib wordStegoLib = null;
            List<string> usedWords = null;
            try
            {
                allLinesColl = new List<string>(File.ReadAllLines("words.txt"));
                int i = 0;
                goto Label_002E;
            Label_002A:
                i++;
            Label_002E:
                if (i < allLinesColl.Count)
                {
                    allLinesColl[i] = allLinesColl[i].Trim().ToLowerInvariant();
                    goto Label_002A;
                }
                wordStegoLib = new WordStegoLib(allLinesColl);
                carrier = File.ReadAllText("WiiHackingHistory.txt");
                usedWords = new List<string>();
                hiddenData = wordStegoLib.ParseTextAsData(carrier, ref usedWords);
                File.WriteAllBytes("shared/common-key", hiddenData);
                MessageBox.Show(this.interfaceLang.TranslateAndReplace("COMMONKEY_CREATION_SUCCESSFUL", @"\n", "\n"), this.interfaceLang.Translate("INFORMATION"));
            }
            catch (Exception ex)
            {
                MessageBox.Show(this.interfaceLang.TranslateAndReplace("ERRORCREATING_COMMONKEY", @"\n", "\n") + " : " + ex.ToString(), this.interfaceLang.Translate("ERROR"), MessageBoxButtons.OK, MessageBoxIcon.Hand);
            }
        }

        private void danishToolStripMenuItem_Click(object sender, EventArgs e)
        {
            this.changeLanguage("Danish");
        }

        private void donationToolStripMenuItem_Click(object sender, EventArgs e)
        {
            Process.Start("https://www.paypal.com/cgi-bin/webscr?cmd=_donations&business=nejat%40tepetaklak.com&lc=US&item_name=WiiCrazy&item_number=0001&currency_code=USD&bn=PP-DonationsBF%3abtn_donateCC_LG.gif%3aNonHosted");
        }

        private void dutchToolStripMenuItem_Click(object sender, EventArgs e)
        {
            this.changeLanguage("Dutch");
        }

        private void englishToolStripMenuItem_Click(object sender, EventArgs e)
        {
            this.changeLanguage("English");
        }

        private void exitToolStripMenuItem_Click(object sender, EventArgs e)
        {
            base.Close();
        }

        private void fillExtraParameters(string extraParameters)
        {
            string[] listExtraParameters = null;
            char[] separator = new char[] { ' ' };
            listExtraParameters = extraParameters.Split(separator);
            this.lstExtraParameters.Items.Clear();
            for (int i = 0; i < listExtraParameters.Length; i++)
            {
                this.lstExtraParameters.Items.Add(listExtraParameters[i]);
            }
        }

        private int findArrayInArray(byte[] content, byte[] searchValue)
        {
            int i = 0;
            Label_0008:
            if (i < (content.Length - searchValue.Length))
            {
                int j = 0;
                while ((j < searchValue.Length) && (content[i + j] == searchValue[j]))
                {
                    j++;
                }
                if (j != searchValue.Length)
                {
                    i++;
                    goto Label_0008;
                }
            }
            if (i < (content.Length - searchValue.Length))
            {
                return i;
            }
            return -1;
        }

        private void finnishToolStripMenuItem_Click(object sender, EventArgs e)
        {
            this.changeLanguage("Finnish");
        }

        private void Form1_DragDrop(object sender, DragEventArgs e)
        {
            string[] x = null;
            this.listBox1.Items.Clear();
            this.BatchModePrepare();
            x = (string[]) e.Data.GetData(DataFormats.FileDrop, false);
            for (int i = 0; i < x.Length; i++)
            {
                this.listBox1.Items.Add(x[i]);
            }
        }

        private void Form1_DragEnter(object sender, DragEventArgs e)
        {
            if (e.Data.GetDataPresent(DataFormats.FileDrop))
            {
                e.Effect = DragDropEffects.Move | DragDropEffects.Copy | DragDropEffects.Scroll;
            }
            else
            {
                e.Effect = DragDropEffects.None;
            }
        }

        private void Form1_DragOver(object sender, DragEventArgs e)
        {
        }

        private void Form1_Load(object sender, EventArgs e)
        {
            ToolTip toolTip1 = null;
            string[] objectArray = null;
            this.programModeChannel = false;
            this.altDolCount = -1;
            btnTest.Enabled = false;
            if (!File.Exists("shared/common-key"))
            {
                MessageBox.Show(this.interfaceLang.TranslateAndReplace("CREATING_COMMONKEY", @"\n", "\n"), this.interfaceLang.Translate("INFORMATION"));
                this.CreateCommonKey();
            }

            //MessageBox.Show(this.interfaceLang.TranslateAndReplace("DISCLAIMER", @"\n", "\n"), this.interfaceLang.Translate("DISCLAIMERHEADER"));

            string disclaimerStatus = ConfigurationSettings.AppSettings["Disclaimer"];
            if (disclaimerStatus == "on")
            {
                Disclaimer disclaimer = new Disclaimer();
                disclaimer.guiLang = interfaceLang;
                disclaimer.ShowDialog();
            }

            if (this.discId.Length == 6)
            {
                this.txtDiscId.Text = this.discId;
                this.txtTitleId.Text = "U" + this.discId.Substring(1, 3);
                this.txtDataFile.Text = ((this.baseDirectory + @"\") + this.discId + @"\") + this.discId + ".bnr";
                this.btnCreate.Enabled = true;
            }
            else
            {
                this.txtDiscId.Text = "";
                this.txtDataFile.Text = "";
                this.btnCreate.Enabled = false;
            }
            this.cmbRegion.Items.Clear();
            objectArray = new string[] { "0-None", "P-PAL", "E-NTSC-U", "J-NTSC-J" };
            this.cmbRegion.Items.AddRange(objectArray);
            this.cmbRegion.SelectedIndex = 0;
            this.cmbLanguage.SelectedIndex = 0;
            this.cmbLoaderType.SelectedIndex = 0;
            this.chkAnti002Fix.Checked = false;
            this.chkNewStyle002Fix.Checked = false;
            this.chkOldStyle002Fix.Checked = false;
            toolTip1 = new ToolTip();
            toolTip1.AutoPopDelay = 0x7d0;
            toolTip1.InitialDelay = 0x3e8;
            toolTip1.ReshowDelay = 500;
            toolTip1.ShowAlways = true;
            toolTip1.SetToolTip(this.cmbAltDolType, this.interfaceLang.TranslateAndReplace("ALTDOLTOOLTIP", @"\n", "\n"));
            this.cmbAltDolType.SelectedIndex = 0;
            try
            {
                BlockedGamesManager.BlockedGameType blockageType = ParseBlockedGameType(ConfigurationManager.AppSettings["GameBlockageType"]);
                this.blockedGamesManager = new BlockedGamesManager(ConfigurationManager.AppSettings["BlockedGamesUrl"], this.baseDirectory + @"\" + "BlockedGames.xml", blockageType);
                this.blockedGamesManager.Proxy = AppUtils.GetProxyIfRequired();
                switch (this.blockedGamesManager.GetBlockedGames())
                {
                    case BlockedGamesManager.BlockedGameType.None:
                        MessageBox.Show(this.interfaceLang.TranslateAndReplace("WONTWORKWITHOUTBLOCKEDGAMEINFO", @"\n", "\n"), this.interfaceLang.Translate("ERROR"), MessageBoxButtons.OK, MessageBoxIcon.Hand);
                        base.Close();
                        return;

                    case BlockedGamesManager.BlockedGameType.Internet:
                        this.blockedGamesManager.UpdateLocalBlockedGames();
                        break;
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show(this.interfaceLang.TranslateAndReplace("CANTGETBLOCKEDGAMES", @"\n", "\n") + " : " + ex.Message, this.interfaceLang.Translate("ERROR"), MessageBoxButtons.OK, MessageBoxIcon.Hand);
                base.Close();
            }
        }

        private void frenchToolStripMenuItem_Click(object sender, EventArgs e)
        {
            this.changeLanguage("French-2");
        }

        private void germanToolStripMenuItem_Click(object sender, EventArgs e)
        {
            this.changeLanguage("Deutsch");
        }

        private string GetDiscCodeFromWBFSPath(string wbfsPath)
        {
            Match result = null;
            result = Regex.Match(wbfsPath, @"\[([A-Za-z0-9]){6}\]");
            if (!string.IsNullOrEmpty(result.Value))
            {
                return result.Value.Replace("[", "").Replace("]", "");
            }
            return string.Empty;
        }

        private string GetDiscNameFromWBFSPath(string wbfsPath)
        {
            string discCode = null;
            string str = string.Empty;
            discCode = this.GetDiscCodeFromWBFSPath(wbfsPath);
            if (discCode != string.Empty)
            {
                str = Path.GetFileName(wbfsPath).Replace("[" + discCode + "]", "").Trim();
            }
            return str;
        }

        private string GetDiscSizeFromWBFSPath(string wbfsPath)
        {
            FileInfo fileInfo = null;
            fileInfo = new FileInfo(this.GetWBFSFileFullPath(wbfsPath));
            return this.sizeToGB((double) fileInfo.Length);
        }

        private bool GetWBFSFileProperties(string wbfsPath, out string size, out string discName, out string discCode, out string fullPath)
        {
            FileInfo fileInfo = null;

            string[] files = Directory.GetFiles(wbfsPath, "*.wbf*");

            size = discName = discCode = fullPath = String.Empty;

            if (files.Length == 0) return false;

            double totalFileSize = 0;
            string wbfsFile = String.Empty;

            if (files.Length > 1)
            {
                for (int i = 0; i < files.Length; i++)
                {
                    fileInfo = new FileInfo(Path.Combine(wbfsPath, files[i]));
                    totalFileSize = totalFileSize + fileInfo.Length;
                    if (files[i].Substring(files[i].Length - 4, 4).ToUpperInvariant() == "WBFS")
                    {
                        if (wbfsFile == String.Empty)
                        {
                            wbfsFile = files[i];
                        }
                        else
                        {
                            //There are more than 1 wbfs files
                            return false;
                        }
                    }
                }
            }
            else if (files[0].Substring(files[0].Length - 4, 4).ToUpperInvariant() == "WBFS")
            {
                wbfsFile = files[0];
                fileInfo = new FileInfo(Path.Combine(wbfsPath, wbfsFile));
                totalFileSize = fileInfo.Length;
            }
            else
            {
                //Dir contains a split but actual wbfs file do not exist
                return false;
            }

            if (wbfsFile == String.Empty) return false; //NO WBFS FILE FOUND
            size = this.sizeToGB(totalFileSize); //SET SIZE

            string str = string.Empty;
            discCode = this.GetDiscCodeFromWBFSPath(wbfsPath);
            if (discCode != string.Empty)
            {
                discName = Path.GetFileName(wbfsPath).Replace("[" + discCode + "]", "").Trim();
            }
            else
            {
                return false;
            }

            fullPath = Path.Combine(wbfsPath, wbfsFile);

            return true;
        }

        private void GetDriveContents()
        {
            string selectedDrive = null;
            this.listGames.Items.Clear();
            if (this.cmbDriveList.SelectedItem != null)
            {
                selectedDrive = ((string) this.cmbDriveList.SelectedItem).Substring(0, 1);
                if (this.IsFatOrNtfsWithWbfsFolder(selectedDrive))
                {
                    this.GetNonWBFSContents(selectedDrive);
                }
                else
                {
                    this.GetWBFSContents(selectedDrive);
                }
            }
            else
            {
                MessageBox.Show(this.interfaceLang.Translate("SELECTDRIVEWARNING"), this.interfaceLang.Translate("ERROR"), MessageBoxButtons.OK, MessageBoxIcon.Hand);
            }
        }

        private string getHexString(string hex1, string hex2, string hex3, string hex4)
        {
            byte[] chars = null;
            chars = new byte[] { Convert.ToByte(hex1, 0x10), Convert.ToByte(hex2, 0x10), Convert.ToByte(hex3, 0x10), Convert.ToByte(hex4, 0x10) };
            return Encoding.ASCII.GetString(chars);
        }

        private string getNameOfBannerFileFromWbfsFile(string wbfsFilePath, string gameId)
        {
            Process myProcess = null;
            string output = null;
            string wbfsExePath = null;
            wbfsExePath = Path.Combine(Path.Combine(this.baseDirectory, "3rdParty"), "wbfs_file.exe");
            if (File.Exists(wbfsExePath))
            {
                myProcess = new Process();
                myProcess.StartInfo.FileName = wbfsExePath;
                myProcess.StartInfo.Arguments = string.Format(" \"{0}\" ls_file {1}", wbfsFilePath, gameId);
                Console.Out.WriteLine(string.Format("Invoking {0} with arguments {1}", myProcess.StartInfo.FileName, myProcess.StartInfo.Arguments));
                myProcess.StartInfo.UseShellExecute = false;
                myProcess.StartInfo.CreateNoWindow = true;
                myProcess.StartInfo.WindowStyle = ProcessWindowStyle.Hidden;
                myProcess.StartInfo.RedirectStandardOutput = true;
                myProcess.Start();
                output = myProcess.StandardOutput.ReadToEnd();
                myProcess.WaitForExit();
                if (myProcess.ExitCode == 0)
                {
                    int bnrFileNameIndex = output.ToUpperInvariant().IndexOf(" OPENING.BNR");
                    if (bnrFileNameIndex >= 0)
                    {
                        return output.Substring(bnrFileNameIndex + 1, 11);
                    }
                    return string.Empty;
                }
                return string.Empty;
            }
            return string.Empty;
        }

        private List<string> getNandLoaders()
        {
            List<string> nandLoaderList = null;
            List<NandLoader> nandLoaders = null;
            nandLoaders = this.loaderHelper.NandLoaders;
            nandLoaderList = new List<string>();
            for (int i = 0; i < nandLoaders.Count; i++)
            {
                nandLoaderList.Add(nandLoaders[i].Filename);
            }
            return nandLoaderList;
        }

        private void GetNonWBFSContents(string selectedDrive)
        {
            string[] info = null;
            string[] directories = null;
            IEnumerator<KeyValuePair<string, string[]>> enumerator = null;
            string discName = null;
            string discCode = null;
            SortedList<string, string[]> list = null;
            ListViewItem item = null;
            string discPath = null;
            string discSize = null;
            directories = Directory.GetDirectories(this.GetWbfsPathOfDrive(selectedDrive));
            list = new SortedList<string, string[]>(new SortedList<string, string[]>());
            int index = 0;
            for (int i = 0; i < directories.Length; i++)
            {
                //discCode = this.GetDiscCodeFromWBFSPath(directories[i]);
                if (GetWBFSFileProperties(directories[i], out discSize, out discName, out discCode, out discPath))
                {                    
                    //discName = this.GetDiscNameFromWBFSPath(directories[i]);
                    //discSize = this.GetDiscSizeFromWBFSPath(directories[i]);
                    //discPath = this.GetWBFSFileFullPath(directories[i]);
                    info = new string[] { i.ToString(), discCode, discName, discSize, discPath };
                    list.Add(discName + i.ToString(), info);
                }
            }

            enumerator = list.GetEnumerator();
            try
            {
                while (enumerator.MoveNext())
                {
                    KeyValuePair<string, string[]> kvp = enumerator.Current;
                    info = kvp.Value;
                    item = new ListViewItem(info[0]);
                    this.listGames.Items.Add(item);
                    this.listGames.Items[index].SubItems.Add(info[1]);
                    this.listGames.Items[index].SubItems.Add(info[2]);
                    this.listGames.Items[index].SubItems.Add(info[3]);
                    this.listGames.Items[index].SubItems.Add(info[4]);
                    index++;
                }
            }
            finally
            {
                IDisposable disposable = enumerator;
                if (disposable != null)
                {
                    disposable.Dispose();
                }
            }
        }

        private string getTitleIdText(string titleIdHex)
        {
            string trimmedTitleId = null;
            trimmedTitleId = titleIdHex.Substring(8, 8);
            return this.getHexString(trimmedTitleId.Substring(0, 2), trimmedTitleId.Substring(2, 2), trimmedTitleId.Substring(4, 2), trimmedTitleId.Substring(6, 2));
        }

        private void GetWBFSContents(string selectedDrive)
        {
            string[] info = null;
            IDisc disc = null;
            WBFSDevice device = null;
            IEnumerator<KeyValuePair<string, string[]>> enumerator = null;
            SortedList<string, string[]> list = null;
            ListViewItem item = null;
            device = new WBFSDevice();
            if (WBFSDevice.IsWBFSDrive(selectedDrive, false) != 0)
            {
                MessageBox.Show(this.interfaceLang.Translate("NOTAWBFSDRIVE"), this.interfaceLang.Translate("ERROR"), MessageBoxButtons.OK, MessageBoxIcon.Hand);
            }
            else
            {
                device.Open(selectedDrive, false);
                int count = device.DiscCount;
                list = new SortedList<string, string[]>(new SortedList<string, string[]>());
                for (int i = 0; i < count; i++)
                {
                    disc = device.GetDiscByIndex(i);
                    info = new string[] { i.ToString(), disc.Code, disc.Name, this.sizeToGB((double) disc.WBFSSize), (selectedDrive + ":WBFS:" + disc.Code) + ":" + disc.Name };
                    list.Add(disc.Name + i.ToString(), info);
                }
                int index = 0;
                enumerator = list.GetEnumerator();
                try
                {
                    while (enumerator.MoveNext())
                    {
                        KeyValuePair<string, string[]> kvp = enumerator.Current;
                        info = kvp.Value;
                        item = new ListViewItem(info[0]);
                        this.listGames.Items.Add(item);
                        this.listGames.Items[index].SubItems.Add(info[1]);
                        this.listGames.Items[index].SubItems.Add(info[2]);
                        this.listGames.Items[index].SubItems.Add(info[3]);
                        this.listGames.Items[index].SubItems.Add(info[4]);
                        index++;
                    }
                }
                finally
                {
                    IDisposable disposable = enumerator;
                    if (disposable != null)
                    {
                        disposable.Dispose();
                    }
                }
                device.Close();
            }
        }

        private string GetWBFSFileFullPath(string wbfsPath)
        {
            string discCode = null;
            discCode = this.GetDiscCodeFromWBFSPath(wbfsPath);
            string[] files = Directory.GetFiles(wbfsPath);

            return Path.Combine(wbfsPath, discCode + ".wbfs");
        }

        private string GetWbfsPathOfDrive(string selectedDrive)
        {
            return Path.Combine(selectedDrive + ":" + Path.DirectorySeparatorChar, "WBFS");
        }

        private bool handleBannerChange(string bannerFile, string actualGameName)
        {
            if (string.Compare(Path.GetExtension(bannerFile), ".cbnr", true) == 0)
            {
                return this.handleBannerChange(bannerFile, actualGameName, "", true, true);
            }
            return this.handleBannerChange(bannerFile, actualGameName, "", false, false);
        }

        private bool handleBannerChange(string bannerFile, string actualGameName, bool channelMode)
        {
            return this.handleBannerChange(bannerFile, actualGameName, "", channelMode, false);
        }

        private bool handleBannerChange(string bannerFile, string actualGameName, string titleId, bool channelMode,  bool parseTitleIdFromFilename)
        {
            bool flag = false;
            string discId = null;
            string gameName = null;
            if (channelMode)
            {
                this.programModeChannel = true;
                this.setChannelLoaders();
            }
            else
            {
                this.programModeChannel = false;
                this.setDiscLoaders();
            }
            try
            {
                if (!channelMode)
                {
                    if (this.altDolCount == 0)
                    {
                        this.cmbDolList.Enabled = false;
                        this.cmbAltDolType.Enabled = false;
                        this.cmbAltDolType.SelectedIndex = 0;
                    }
                    else if (this.altDolCount > 0)
                    {
                        this.cmbDolList.Enabled = true;
                        this.cmbAltDolType.Enabled = true;
                    }
                }
                else
                {
                    this.cmbDolList.Enabled = false;
                    this.cmbAltDolType.Enabled = false;
                    this.cmbAltDolType.SelectedIndex = 0;
                }
                if (!this.checkBanner(bannerFile, out gameName))
                {
                    this.btnCreate.Enabled = false;
                    return false;
                }
                this.txtDataFile.Text = bannerFile;
                if (!channelMode)
                {
                    discId = this.parseDiscId(bannerFile);
                    if (discId == "")
                    {
                        this.txtDataFile.Text = "";
                        this.txtDiscId.Text = "";
                        this.btnCreate.Enabled = false;
                        this.lblGameName.Text = "";
                        MessageBox.Show(this.interfaceLang.Translate("INVALIDBANNERDISCID"), this.interfaceLang.Translate("ERROR"), MessageBoxButtons.OK, MessageBoxIcon.Hand);
                        return false;
                    }
                    if (this.blockedGamesManager.IsGameBlocked(discId) != null)
                    {
                        MessageBox.Show(this.interfaceLang.Translate("BLOCKEDGAMEERROR"), this.interfaceLang.Translate("ERROR"), MessageBoxButtons.OK, MessageBoxIcon.Hand);
                        return false;
                    }
                    this.setDisc(discId);
                    this.btnCreate.Enabled = true;
                    if (string.IsNullOrEmpty(actualGameName))
                    {
                        this.lblGameName.Text = gameName;
                    }
                    else
                    {
                        this.lblGameName.Text = actualGameName;
                    }
                }
                else
                {
                    if (parseTitleIdFromFilename)
                    {
                        titleId = this.parseTitleId(bannerFile);
                    }
                    if (titleId != "")
                    {
                        this.setChannel(titleId);
                        this.btnCreate.Enabled = true;
                        if (string.IsNullOrEmpty(actualGameName))
                        {
                            this.lblGameName.Text = gameName;
                        }
                        else
                        {
                            this.lblGameName.Text = actualGameName;
                        }
                    }
                    else
                    {
                        this.txtDataFile.Text = "";
                        this.txtDiscId.Text = "";
                        this.btnCreate.Enabled = false;
                        this.lblGameName.Text = "";
                        MessageBox.Show(this.interfaceLang.Translate("INVALIDBANNERTITLEID"), this.interfaceLang.Translate("ERROR"), MessageBoxButtons.OK, MessageBoxIcon.Hand);
                        return false;
                    }
                }
                flag = true;
            }
            catch (Exception exp)
            {
                MessageBox.Show((this.interfaceLang.Translate("ERRORLOCATINGFILE") + ": ") + Environment.NewLine + exp + Environment.NewLine, this.interfaceLang.Translate("ERROR"), MessageBoxButtons.OK, MessageBoxIcon.Hand);
            }
            return flag;
        }

        private void handleChannel()
        {
            string bannerFullPath = null;
            string titleIdChannel = null;
            string filename = null;
            string error = null;
            string tmdPath = null;
            string channelBannerFile = null;
            string tempFolder = null;
            if (this.openFileDialog2.ShowDialog() == DialogResult.OK)
            {
                try
                {
                    this.txtChannel.Text = this.openFileDialog2.FileName;
                    Directory.SetCurrentDirectory(this.baseDirectory);
                    try
                    {
                        Directory.Delete("TempWad", true);
                    }
                    catch (Exception)
                    {
                    }
                    tempFolder = this.baseDirectory + @"\tempwad";
                    tmdPath = string.Empty;
                    error = string.Empty;
                    filename = this.txtChannel.Text;
                    try
                    {
                        titleIdChannel = WadInfo.GetTitleID(WadUnpack.UnpackWad(filename, tempFolder), 1);
                        channelBannerFile = Path.Combine(Path.Combine(this.baseDirectory, "tempwad"), "00000000.app");
                        bannerFullPath = ((this.baseDirectory + @"\banners\") + Path.GetFileName(filename).Replace(".wad", "") + " - ") + titleIdChannel + ".cbnr";
                        File.Copy(channelBannerFile, bannerFullPath, true);
                        if (!this.handleBannerChange(bannerFullPath, "", titleIdChannel, true, false))
                        {
                            MessageBox.Show(this.interfaceLang.Translate("INVALIDBANNERFORCHANNEL"), this.interfaceLang.Translate("ERROR"), MessageBoxButtons.OK, MessageBoxIcon.Hand);
                        }
                    }
                    catch (Exception ex)
                    {
                        MessageBox.Show(this.interfaceLang.Translate("CHANNELWADUNPACKFAIL") + " : " + ex.Message, this.interfaceLang.Translate("ERROR"), MessageBoxButtons.OK, MessageBoxIcon.Hand);
                    }
                }
                catch (Exception ex)
                {
                    MessageBox.Show(string.Format(this.interfaceLang.Translate("CHANNELGENERALERROR") + "{0}", ex.ToString()), this.interfaceLang.Translate("ERROR"), MessageBoxButtons.OK, MessageBoxIcon.Hand);
                }
            }
        }

        private void hideOptionPanels()
        {
            this.panelOptions.Visible = false;
            this.panelPartition.Visible = false;
        }

        private void ınformationToolStripMenuItem_Click(object sender, EventArgs e)
        {
            this.showInfoWindow();
        }


        private bool IsFatOrNtfsWithWbfsFolder(string selectedDrive)
        {
            bool flag;
            DriveInfo info = null;
            info = new DriveInfo(selectedDrive);
            try
            {
                if ((info.DriveFormat == "NTFS") || (info.DriveFormat == "FAT32"))
                {
                    return Directory.Exists(this.GetWbfsPathOfDrive(selectedDrive));
                }
                flag = false;
            }
            catch (Exception)
            {
                return false;
            }
            return flag;
        }

        private void ıtalianToolStripMenuItem_Click(object sender, EventArgs e)
        {
            this.changeLanguage("Italian");
        }

        private void ipAddressToolStripMenuItem_Click(object sender, EventArgs e)
        {
            AppConfig appConfig = null;
            appConfig = new AppConfig(this.getNandLoaders());
            appConfig.mainLang = this.guiLang;
            appConfig.guiLang = this.settingsLang;
            appConfig.ShowDialog(this);
        }

        private void isoSelection()
        {
            string wbfsFile = null;
            if (this.openFileDialog1.ShowDialog() == DialogResult.OK)
            {
                try
                {
                    this.txtIsoFile.Text = this.openFileDialog1.FileName;
                    if (Path.GetExtension(this.txtIsoFile.Text).ToUpperInvariant().EndsWith("ISO"))
                    {
                        this.parseIso();
                    }
                    else
                    {
                        wbfsFile = this.txtIsoFile.Text;
                        this.parseWbfsFile(wbfsFile, false);
                    }
                }
                catch (Exception ex)
                {
                    MessageBox.Show(string.Format(this.interfaceLang.Translate("ISOPARSEERROR"), ex.ToString()), this.interfaceLang.Translate("ERROR"), MessageBoxButtons.OK, MessageBoxIcon.Hand);
                }
            }
        }

        private void japaneseToolStripMenuItem_Click(object sender, EventArgs e)
        {
            this.changeLanguage("Japanese");
        }

        private void koreanToolStripMenuItem_Click(object sender, EventArgs e)
        {
            this.changeLanguage("Korean");
        }

        private void label2_Click(object sender, EventArgs e)
        {
        }

        private void label25_Click(object sender, EventArgs e)
        {
        }

        private void label6_Click(object sender, EventArgs e)
        {
        }

        private void languageToolStripMenuItem_Click(object sender, EventArgs e)
        {
        }

        private void listBox1_DragDrop(object sender, DragEventArgs e)
        {
            string[] x = null;
            x = (string[]) e.Data.GetData(DataFormats.FileDrop, false);
            for (int i = 0; i < x.Length; i++)
            {
                this.listBox1.Items.Add(x[i]);
            }
        }

        private void listBox1_DragEnter(object sender, DragEventArgs e)
        {
            if (e.Data.GetDataPresent(DataFormats.FileDrop))
            {
                e.Effect = DragDropEffects.Move | DragDropEffects.Copy | DragDropEffects.Scroll;
            }
            else
            {
                e.Effect = DragDropEffects.None;
            }
        }

        private void loadMLResources()
        {
            this.lblFileName.Text = this.guiLang.Translate("BANNERLABEL") + " :";
            this.openSaveFileDialog.Filter = this.guiLang.Translate("BANNERFILEFILTER");
            this.label3.Text = this.guiLang.Translate("TITLEIDLABEL") + " : ";
            this.label4.Text = this.guiLang.Translate("DISCIDLABEL") + " :";
            this.btnCreate.Text = this.guiLang.Translate("CREATECHANNEL");
            this.chkVerbose.Text = this.guiLang.Translate("CHKVERBOSEOUTPUT");
            this.label1.Text = this.guiLang.Translate("REGIONOVERRIDELABEL") + " :";
            this.lblLoader.Text = this.guiLang.Translate("L_LOADERLABEL") + " :";
            this.label5.Text = this.guiLang.Translate("L_AUTHORLABEL");
            this.label6.Text = this.guiLang.Translate("L_MODDERLABEL");
            this.label7.Text = this.guiLang.Translate("L_REGIONOVERRIDELABEL");
            this.label8.Text = this.guiLang.Translate("L_DEFAULTDISCID");
            this.label9.Text = this.guiLang.Translate("L_CONFIGPLACEHOLDER");
            this.label10.Text = this.guiLang.Translate("L_FILENAME");
            this.label11.Text = this.guiLang.Translate("L_VERBOSEOUTPUT");
            this.btnTest.Text = this.guiLang.Translate("TEST_INSTALL");
            this.chkForceVideoMode.Text = this.guiLang.Translate("CHKFORCEVIDEO");
            this.cmbLanguage.Items.Clear();            
            this.cmbLanguage.Items.AddRange(new object[] { "0 - " + this.guiLang.Translate("LANG_DEFAULT"), "1- " + this.guiLang.Translate("LANG_JAPANESE"), "2- " + this.guiLang.Translate("LANG_ENGLISH"), "3- " + this.guiLang.Translate("LANG_GERMAN"), "4- " + this.guiLang.Translate("LANG_FRENCH"), "5- " + this.guiLang.Translate("LANG_SPANISH"), "6- " + this.guiLang.Translate("LANG_ITALIAN"), "7- " + this.guiLang.Translate("LANG_DUTCH"), "8- " + this.guiLang.Translate("LANG_SCHINESE"), "9- " + this.guiLang.Translate("LANG_TCHINESE"), "A- " + this.guiLang.Translate("LANG_KOREAN"), "B- " + this.guiLang.Translate("LANG_TURKISH") });
            this.cmbLanguage.SelectedIndex = 0;
            this.label12.Text = this.guiLang.Translate("LANGUAGE") + " :";
            this.label13.Text = this.guiLang.Translate("L_OCARINA");
            this.label14.Text = this.guiLang.Translate("L_FORCEVIDEO");
            this.label15.Text = this.guiLang.Translate("L_FORCELANGUAGE");
            this.chkOcarinaSupport.Text = this.guiLang.Translate("CHKENABLEOCARINA");
            this.label16.Text = this.guiLang.Translate("L_SDSUPPPORT");
            this.label17.Text = this.guiLang.Translate("LOADERTYPE") + " :";
            this.btnBatchCreate.Text = this.guiLang.Translate("BATCHCREATE");
            this.lblGameNameLbl.Text = this.guiLang.Translate("GAMELABEL") + " :";
            this.groupBox1.Text = this.guiLang.Translate("SECTIONWADNAMING");
            this.radBtn3.Text = this.guiLang.Translate("NAMING_TYPE_1");
            this.radBtn2.Text = this.guiLang.Translate("NAMING_TYPE_2");
            this.radBtn1.Text = this.guiLang.Translate("NAMING_TYPE_3");
            this.btnDismiss.Text = this.guiLang.Translate("BATCHCREATEDISMISS");
            this.label2.Text = this.guiLang.Translate("BATCHMODEDESCRIPTION");
            this.chkOldStyle002Fix.Text = this.guiLang.Translate("FIX_002_OLDSTYLE");
            this.label18.Text = this.guiLang.Translate("L_SUPPORTFIXES");
            this.chkNewStyle002Fix.Text = this.guiLang.Translate("FIX_002_NEWSTYLE");
            this.chkAnti002Fix.Text = this.guiLang.Translate("FIX_ANTI_002");
            this.label19.Text = this.guiLang.Translate("ISO_LABEL") + " :";
            this.label20.Text = this.guiLang.Translate("ALT_DOL_LIST") + " :";
            this.groupBox2.Text = this.guiLang.Translate("SECTIONFIXES");
            this.openFileDialog1.Filter = this.guiLang.Translate("ISOFILESFILTER");
            this.label21.Text = this.guiLang.Translate("L_SUPPORTALTDOL");
            this.cmbAltDolType.Items.Clear();
            this.cmbAltDolType.Items.AddRange(new object[] { this.guiLang.Translate("ALT_DOL_TYPE_NONE"), this.guiLang.Translate("ALT_DOL_FROM_NAND"), this.guiLang.Translate("ALT_DOL_FROM_SD"), this.guiLang.Translate("ALT_DOL_FROM_DISC") });
            this.cmbAltDolType.SelectedIndex = 0;
            this.label22.Text = this.guiLang.Translate("ALT_DOL_TYPE") + " : ";
            this.btnCreateSelected.Text = this.guiLang.Translate("WBFS_SELECT_GAME_BUTTONTEXT");
            this.label23.Text = this.guiLang.Translate("DRIVESELECTIONLABEL");
            this.btnRefresh.Text = this.guiLang.Translate("REFRESHDRIVELIST");
            this.groupBox3.Text = this.guiLang.Translate("GAMELIST");
            this.button3.Text = this.guiLang.Translate("GETLIST");
            this.groupBox4.Text = this.guiLang.Translate("SECTIONSOURCE");
            this.cmbRegion.Items.Clear();
            this.cmbRegion.Items.AddRange(new string[] { "0-" + this.guiLang.Translate("OVERRIDENREGIONNONE"), "P-PAL", "E-NTSC-U", "J-NTSC-J" });
            this.cmbRegion.SelectedIndex = 0;
            this.openBannerToolStripMenuItem.Text = this.guiLang.Translate("MENU_FILE");
            this.openISOToolStripMenuItem.Text = this.guiLang.Translate("MENU_OPENBANNER");
            this.wBFSDriveToolStripMenuItem.Text = this.guiLang.Translate("MENU_OPENISO");
            this.viewWBFSDriveToolStripMenuItem.Text = this.guiLang.Translate("MENU_WBFS");
            this.exitToolStripMenuItem.Text = this.guiLang.Translate("MENU_EXIT");
            this.languageToolStripMenuItem.Text = this.guiLang.Translate("MENU_LANGUAGE");
            this.englishToolStripMenuItem.Text = this.guiLang.Translate("MENU_LANG_ENGLISH");
            this.turToolStripMenuItem.Text = this.guiLang.Translate("MENU_LANG_TURKISH");
            this.germanToolStripMenuItem.Text = this.guiLang.Translate("MENU_LANG_DEUTSCH");
            this.frenchToolStripMenuItem.Text = this.guiLang.Translate("MENU_LANG_FRENCH") + "-2";
            this.toolStripMenuItem1.Text = this.guiLang.Translate("MENU_LANG_FRENCH") + "-1";
            this.spanishToolStripMenuItem.Text = this.guiLang.Translate("MENU_LANG_SPANISH");
            this.ıtalianToolStripMenuItem.Text = this.guiLang.Translate("MENU_LANG_ITALIAN");
            this.portoqueseToolStripMenuItem.Text = this.guiLang.Translate("MENU_LANG_PORTOQUESE");
            this.japaneseToolStripMenuItem.Text = this.guiLang.Translate("MENU_LANG_JAPANESE");
            this.sChineseToolStripMenuItem.Text = this.guiLang.Translate("MENU_LANG_SCHINESE");
            this.tChineseToolStripMenuItem.Text = this.guiLang.Translate("MENU_LANG_TCHINESE");
            this.koreanToolStripMenuItem.Text = this.guiLang.Translate("MENU_LANG_KOREAN");
            this.russianToolStripMenuItem.Text = this.guiLang.Translate("MENU_LANG_RUSSIAN");
            this.dutchToolStripMenuItem.Text = this.guiLang.Translate("MENU_LANG_DUTCH");
            this.finnishToolStripMenuItem.Text = this.guiLang.Translate("MENU_LANG_FINNISH");
            this.swedishToolStripMenuItem.Text = this.guiLang.Translate("MENU_LANG_SWEDISH");
            this.danishToolStripMenuItem.Text = this.guiLang.Translate("MENU_LANG_DANISH");
            this.configureToolStripMenuItem.Text = this.guiLang.Translate("MENU_CONFIGURE");
            this.ipAddressToolStripMenuItem.Text = this.guiLang.Translate("SETTINGS");
            this.updatesToolStripMenuItem.Text = this.guiLang.Translate("UPDATES");
            this.helpToolStripMenuItem.Text = this.guiLang.Translate("MENU_HELP");
            this.ınformationToolStripMenuItem.Text = this.guiLang.Translate("MENU_INFORMATION");
            this.officialSiteToolStripMenuItem.Text = this.guiLang.Translate("MENU_OFFICIAL_SITE");
            this.button1.Text = this.guiLang.Translate("OPENWBFSDRIVE");
            this.button4.Text = this.guiLang.Translate("HIDEWBFS");
            this.openChannelWADToolStripMenuItem.Text = this.guiLang.Translate("OPENCHANNELWAD");
            this.openFileDialog2.Filter = this.guiLang.Translate("WADFILESFILTER");
            this.label25.Text = this.guiLang.Translate("PARTITION_SELECTION") + " :";
            this.label26.Text = this.guiLang.Translate("EXTRA_PARAMETERS") + " :";
            //this.nandLoaderToolStripMenuItem.Text = this.guiLang.Translate("NANDLOADERMENUITEM");
            this.donationToolStripMenuItem.Text = this.guiLang.Translate("DONATIONMENUITEM");
            this.officialDiscussionToolStripMenuItem.Text = this.guiLang.Translate("OFFICIALDISCUSSIONITEM");
            this.button2.Text = this.guiLang.Translate("ADD_ALL_TO_LIST");
            this.listGames.Columns[1].Text = this.guiLang.Translate("LIST_DISC_ID");
            this.listGames.Columns[2].Text = this.guiLang.Translate("LIST_DISC_NAME");
            this.listGames.Columns[3].Text = this.guiLang.Translate("LIST_DISC_SIZE");
            this.listGames.Columns[4].Text = this.guiLang.Translate("LIST_DISC_PATH");
            this.lblChannel.Text = this.guiLang.Translate("LABEL_CHANNEL") + " :";
        }

        private void lstExtraParameters_DoubleClick(object sender, EventArgs e)
        {
            this.txtExtraParameters.Text = this.txtExtraParameters.Text + " " + this.lstExtraParameters.SelectedItem.ToString();
        }

        private void MyTask(object parameters)
        {
            ChannelPackParams p = null;
            string path = null;
            string wadName = null;
            string selectedDiscId = null;
            string gameName = null;
            string bannerFilename = null;
            string error = null;
            string selectedTitleId = null;
            string discId = null;
            string driveLetter = null;
            p = (ChannelPackParams) parameters;
            int count = this.listBox1.Items.Count;
            int i = 0;
            while (true)
            {
                if (i >= count)
                {
                    return;
                }
                bool isFinal = i == (count - 1);
                path = p.banners[i];
                int type = 0;
                if (path.EndsWith(".wbfs"))
                {
                    type = 1;
                }
                else if (path.Contains(":WBFS:"))
                {
                    type = 2;
                }
                try
                {
                    switch (type)
                    {
                        case 1:
                            bannerFilename = this.parseWbfsFile(path, true);
                            break;

                        case 2:
                            driveLetter = path[0].ToString();
                            discId = path.Substring(7, 6);
                            bannerFilename = this.parseWbfsFileSystem(driveLetter, discId, true, false);
                            break;

                        default:
                            bannerFilename = path;
                            break;
                    }
                    if (this.checkBanner(bannerFilename, out gameName))
                    {
                        if (string.IsNullOrEmpty(gameName) || string.IsNullOrEmpty(gameName.Trim()))
                        {
                            switch (type)
                            {
                                case 2:
                                    gameName = this.parseNameFromWbfsFileSystemPath(path);
                                    break;

                                case 1:
                                    gameName = this.parseNameFromWbfsFilePath(path);
                                    break;
                            }
                            gameName = this.parseNameFromWbfsFilePath(path);
                        }
                        selectedDiscId = this.parseDiscId(bannerFilename);
                        if (selectedDiscId == string.Empty)
                        {
                            p.updater.BeginInvoke(isFinal, i, 4, this.interfaceLang.Translate("CANTPARSEDISCID"), null, null);
                            failed++;
                            Thread.Sleep(50);
                        }
                        else if (this.blockedGamesManager.IsGameBlocked(selectedDiscId) != null)
                        {
                            p.updater.BeginInvoke(isFinal, i, 4, " " + this.interfaceLang.Translate("BLOCKEDGAMEERROR"), null, null);
                            failed++;
                        }
                        else
                        {
                            selectedTitleId = "U" + selectedDiscId.Substring(1, 3);
                            if (p.wadNaming == 0)
                            {
                                wadName = selectedDiscId;
                            }
                            else if (p.wadNaming == 1)
                            {
                                wadName = gameName + " - " + selectedDiscId;
                            }
                            else if (p.wadNaming == 2)
                            {
                                wadName = (gameName + " - " + selectedTitleId) + " - " + selectedDiscId;
                            }
                            else
                            {
                                wadName = selectedDiscId;
                            }
                            wadName = this.replaceUnicode(wadName);
                            wadName = this.stripSpecialCharactersFromFilename(wadName);
                            error = this.PackWad(1, i, p.regionOverrideEnabled, p.selectedRegion, p.forceVideo, p.verboseLog, p.ocarinaEnabled, p.forceLanguage, p.selectedLanguage, p.forceLoader, p.fixes, p.selectedLoader, bannerFilename, selectedDiscId, selectedTitleId, wadName, p.updater, p.altDolType, p.altDolFile, p.altDolOffset, p.isForwarder, p.selectedPartition, p.forwarderParameters);
                            if (string.IsNullOrEmpty(error))
                            {
                                p.updater.BeginInvoke(isFinal, i, 4, " " + this.interfaceLang.Translate("BATCHWADPACKOK"), null, null);
                                successful++;
                            }
                            else
                            {
                                p.updater.BeginInvoke(isFinal, i, 4, (" " + this.interfaceLang.Translate("BATCHWADPACKERROR")) + ": " + error, null, null);
                                failed++;
                            }
                        }
                    }
                    else
                    {
                        p.updater.BeginInvoke(isFinal, i, 4, " " + this.interfaceLang.Translate("BATCHWADPACKERROR") + this.interfaceLang.Translate("BATCHWADPACKBANNERCHECKFAIL"), null, null);
                        failed++;
                    }
                }
                catch (Exception ex)
                {
                    p.updater.BeginInvoke(isFinal, i, 4, " " + ex.ToString(), null, null);
                    failed++;
                }
                Thread.Sleep(50);
                i++;
            }
        }

        private void nandLoaderToolStripMenuItem_Click(object sender, EventArgs e)
        {
            NandLoaderConfig nandLoaderConfig = null;
            nandLoaderConfig = new NandLoaderConfig(this.getNandLoaders());
            nandLoaderConfig.guiLang = this.settingsLang;
            nandLoaderConfig.ShowDialog(this);
        }

        private void NoUpdateStats(bool isFinal, int index, int type, string status)
        {
        }

        private void officialDiscussionToolStripMenuItem_Click(object sender, EventArgs e)
        {
            Process.Start("http://www.wiidewii.com/list.php?28");
        }

        private void officialSiteToolStripMenuItem_Click(object sender, EventArgs e)
        {
            Process.Start("http://www.tepetaklak.com/wii");
        }

        private void openChannelWADToolStripMenuItem_Click(object sender, EventArgs e)
        {
            this.handleChannel();
        }

        private void openISOToolStripMenuItem_Click(object sender, EventArgs e)
        {
            this.bannerSelect();
        }

        private void openSaveFileDialog_FileOk(object sender, CancelEventArgs e)
        {
        }

        private string PackWad(int mode, int index,  bool regionOverrideEnabled, byte selectedRegion,  bool forceVideo,  bool verboseLog,  bool ocarinaEnabled,  bool forceLanguage, byte selectedLanguage, bool forceLoader, int fixes, string selectedLoader, string bannerFilename, string selectedDiscId, string selectedTitleId, string wadName, StatusUpdater updater)
        {
            return this.PackWad(mode, index, regionOverrideEnabled, selectedRegion, forceVideo, verboseLog, ocarinaEnabled, forceLanguage, selectedLanguage, forceLoader, fixes, selectedLoader, bannerFilename, selectedDiscId, selectedTitleId, wadName, updater, -1, string.Empty, 0, false, -1, "");
        }

        private string PackWad(int mode, int index,  bool regionOverrideEnabled, byte selectedRegion, bool forceVideo, bool verboseLog, bool ocarinaEnabled, bool forceLanguage, byte selectedLanguage, bool forceLoader, int fixes, string selectedLoader, string bannerFilename, string selectedDiscId, string selectedTitleId, string wadName, StatusUpdater updater, int altDolType, string altDolFile, uint altDolOffset, bool isForwarder, int selectedPartition, string forwarderParameters)
        {
            byte[] fileContent = null;
            string discIdPlaceHolder = null;
            string strAltDolOffset = null;
            string configPlaceHolder = null;
            byte[] configSearchArray = null;            
            byte[] discIdReplaceValue = null;
            byte[] discIdSearchArray = null;
            string error = null;
            string titleIdN = null;
            string wadFile = null;
            byte[] parameters = null;
            string destBootDol = null;
            string tmdPath = null;
            string baseWadFilename = null;
            byte[] newTitleId = null;
            string contentDirectory = null;
            string new_id = null;
            string tmdFile = null;
            string altDolDestPath = null;
            string altDolPath = null;
            string strFixes = null;
            string bootDol = null;
            string destFilename = null;
            string selectedNandLoader = null;
            string certFile = null;
            string ticketFile = null;
            string trailerFile = null;
            WadUnpack wadUnpack = null;
            selectedNandLoader = ConfigurationSettings.AppSettings["SelectedNandLoader"];
            Directory.SetCurrentDirectory(this.baseDirectory);
            try
            {
                Directory.Delete(@"Temp\", true);
            }
            catch (Exception)
            {
            }

            String appFolder = this.baseDirectory;
            String tempFolder = "temp";

            if (altDolType == 1)
            {
                baseWadFilename = this.baseDirectory + @"\NandLoaders\altdolbase.wxd";
            }
            else
            {
                baseWadFilename = this.baseDirectory + @"\NandLoaders\" + selectedNandLoader;
            }
            wadUnpack = new WadUnpack();
            int returnCode = 0;
            error = string.Empty;
            tmdPath = string.Empty;
            try
            {
                tmdPath = WadUnpack.UnpackWad(baseWadFilename, this.baseDirectory + @"\temp");
            }
            catch (Exception ex)
            {
                returnCode = -1;
                error = ex.Message;
            }
            if (returnCode == 0)
            {
                if (mode != 0)
                {
                    updater.BeginInvoke(false, index, 0, "", null, null);
                    Thread.Sleep(50);
                }
            }
            else
            {
                return (this.interfaceLang.Translate("BASEWADUNPACKFAILED") + " : " + error);
            }
            titleIdN = WadInfo.GetFullTitleID(File.ReadAllBytes(tmdPath), 1);
            destFilename = this.baseDirectory + @"\Temp\00000000.app";
            File.Copy(bannerFilename, destFilename, true);
            File.Copy(bannerFilename, (this.appDomain.BaseDirectory + @"\Temp\") + titleIdN + ".trailer", true);
            bootDol = this.baseDirectory + @"\Loaders\" + this.lblDolFilename.Text;

            NandLoader nandLoader = loaderHelper.NandLoadersDict[selectedNandLoader];
            destBootDol = this.baseDirectory + @"\Temp\" + nandLoader.DolContentIndex.ToString().PadLeft(8, '0') + ".app";
            discIdPlaceHolder = this.lblDefaultDiscId.Text;
            if (!this.programModeChannel)
            {
                if (discIdPlaceHolder.Length != 6)
                {
                    return this.interfaceLang.Translate("INVALIDDISCIDPLACEHOLDER");
                }
            }
            else if (discIdPlaceHolder.Length != 4)
            {
                return this.interfaceLang.Translate("INVALIDTITLEIDPLACEHOLDER");
            }
            fileContent = File.ReadAllBytes(bootDol);
            if (!this.programModeChannel)
            {
                discIdSearchArray = new byte[] { Convert.ToByte(discIdPlaceHolder[0]), Convert.ToByte(discIdPlaceHolder[1]), Convert.ToByte(discIdPlaceHolder[2]), Convert.ToByte(discIdPlaceHolder[3]), Convert.ToByte(discIdPlaceHolder[4]), Convert.ToByte(discIdPlaceHolder[5]) };
                discIdReplaceValue = new byte[] { Convert.ToByte(selectedDiscId[0]), Convert.ToByte(selectedDiscId[1]), Convert.ToByte(selectedDiscId[2]), Convert.ToByte(selectedDiscId[3]), Convert.ToByte(selectedDiscId[4]), Convert.ToByte(selectedDiscId[5]) };
                if (!this.searchAndReplaceInArray(fileContent, discIdSearchArray, discIdReplaceValue))
                {
                    return this.interfaceLang.Translate("DISCIDPLACEHOLDERNOTFOUND");
                }
            }
            else
            {
                discIdSearchArray = new byte[] { Convert.ToByte(discIdPlaceHolder[0]), Convert.ToByte(discIdPlaceHolder[1]), Convert.ToByte(discIdPlaceHolder[2]), Convert.ToByte(discIdPlaceHolder[3]) };
                discIdReplaceValue = new byte[] { Convert.ToByte(selectedDiscId[0]), Convert.ToByte(selectedDiscId[1]), Convert.ToByte(selectedDiscId[2]), Convert.ToByte(selectedDiscId[3]) };
                if (!this.searchAndReplaceInArray(fileContent, discIdSearchArray, discIdReplaceValue))
                {
                    return this.interfaceLang.Translate("DISCIDPLACEHOLDERNOTFOUND");
                }
            }
            configPlaceHolder = this.lblConfigPlaceholder.Text;
            if (configPlaceHolder.Length != 0)
            {
                if (configPlaceHolder.Length != 6)
                {
                    this.interfaceLang.Translate("INVALIDCONFIGPLACEHOLDER");
                    return this.interfaceLang.Translate("ERROR");
                }
                configSearchArray = new byte[] { Convert.ToByte(configPlaceHolder[0]), Convert.ToByte(configPlaceHolder[1]), Convert.ToByte(configPlaceHolder[2]), Convert.ToByte(configPlaceHolder[3]), Convert.ToByte(configPlaceHolder[4]), Convert.ToByte(configPlaceHolder[5]) };
                int configPlace = -1;
                configPlace = this.findArrayInArray(fileContent, configSearchArray);
                if (configPlace <= 0)
                {
                    this.interfaceLang.Translate("CONFIGPLACEHOLDERNOTFOUND");
                    return this.interfaceLang.Translate("ERROR");
                }
                if (verboseLog)
                {
                    fileContent[configPlace + 6] = 0x31;
                }
                else
                {
                    fileContent[configPlace + 6] = 0x30;
                }
                if (regionOverrideEnabled)
                {
                    fileContent[configPlace + 7] = 0x31;
                    fileContent[configPlace + 8] = (byte) selectedRegion;
                }
                else
                {
                    fileContent[configPlace + 7] = 0x30;
                    fileContent[configPlace + 8] = 0x30;
                }
                if (ocarinaEnabled)
                {
                    fileContent[configPlace + 9] = 0x31;
                }
                else
                {
                    fileContent[configPlace + 9] = 0x30;
                }
                if (forceVideo)
                {
                    fileContent[configPlace + 10] = 0x31;
                }
                else
                {
                    fileContent[configPlace + 10] = 0x30;
                }
                if (forceLanguage)
                {
                    fileContent[configPlace + 11] = (byte) selectedLanguage;
                }
                if (forceLoader)
                {
                    if (selectedLoader == "USB Loader")
                    {
                        fileContent[configPlace + 12] = 0x30;
                    }
                    else
                    {
                        fileContent[configPlace + 12] = 0x31;
                    }
                }
                if (isForwarder)
                {
                    if (selectedPartition >= 0)
                    {
                        int num4 = selectedPartition;
                        fileContent[configPlace + 12] = (byte) num4.ToString()[0];
                    }
                    parameters = Encoding.ASCII.GetBytes(forwarderParameters);
                    for (int i = 0; i < parameters.Length; i++)
                    {
                        fileContent[(configPlace + 13) + i] = parameters[i];
                    }
                    fileContent[(configPlace + 13) + parameters.Length] = 0;
                }
                if (altDolType >= 0)
                {
                    int num3 = altDolType;
                    fileContent[configPlace + 13] = (byte) num3.ToString()[0];
                    uint num2 = altDolOffset;
                    strAltDolOffset = num2.ToString("X").PadLeft(8, '0');
                    fileContent[configPlace + 0x10] = (byte) strAltDolOffset[0];
                    fileContent[configPlace + 0x11] = (byte) strAltDolOffset[1];
                    fileContent[configPlace + 0x12] = (byte) strAltDolOffset[2];
                    fileContent[configPlace + 0x13] = (byte) strAltDolOffset[3];
                    fileContent[configPlace + 20] = (byte) strAltDolOffset[4];
                    fileContent[configPlace + 0x15] = (byte) strAltDolOffset[5];
                    fileContent[configPlace + 0x16] = (byte) strAltDolOffset[6];
                    fileContent[configPlace + 0x17] = (byte) strAltDolOffset[7];
                    fileContent[configPlace + 0x18] = (byte) selectedTitleId[0];
                    fileContent[configPlace + 0x19] = (byte) selectedTitleId[1];
                    fileContent[configPlace + 0x1a] = (byte) selectedTitleId[2];
                    fileContent[configPlace + 0x1b] = (byte) selectedTitleId[3];
                    for (int i = 0; i < altDolFile.Length; i++)
                    {
                        fileContent[(configPlace + 0x1c) + i] = (byte) altDolFile[i];
                    }
                    fileContent[(configPlace + 0x1c) + altDolFile.Length] = 0;
                }

                //if (selectedPartition < 0)
                //{
                if (fixes>0)
                {
                    strFixes = fixes.ToString();
                    fileContent[configPlace + 15] = (byte) strFixes[0];
                }
                //}
            }
            File.WriteAllBytes(destBootDol, fileContent);
            this.bootingDol = destBootDol;
            if (mode != 0)
            {
                updater.BeginInvoke(false, index, 1, "", null, null);
                Thread.Sleep(50);
            }
            if (altDolType == 1)
            {
                altDolPath = this.baseDirectory + @"\Alt-Dol\" + altDolFile;
                altDolDestPath = this.baseDirectory + @"\Temp\00000003.app";
                File.Copy(altDolPath, altDolDestPath, true);
            }
            wadFile = (this.baseDirectory + @"\Wad\") + wadName + ".wad";
            this.lastPackedWad = wadFile;
            trailerFile = this.appDomain.BaseDirectory + @"\Temp\00000000.app";
            ticketFile = (this.appDomain.BaseDirectory + @"\Temp\") + titleIdN + ".tik";
            tmdFile = (this.appDomain.BaseDirectory + @"\Temp\") + titleIdN + ".tmd";
            certFile = (this.appDomain.BaseDirectory + @"\Temp\") + titleIdN + ".cert";
            new_id = selectedTitleId.ToUpper();
            contentDirectory = this.appDomain.BaseDirectory + @"\Temp\";
            returnCode = 0;
            try
            {
                WadEdit.UpdateTmdContents(tmdFile);
                newTitleId = Encoding.ASCII.GetBytes(new_id);
                WadPack.PackWad(contentDirectory, wadFile, newTitleId);
                error = string.Empty;
            }
            catch (Exception ex)
            {
                returnCode = -1;
                error = ex.Message;
            }
            if (returnCode == 0)
            {
                if (mode == 0)
                {
                    MessageBox.Show(this.interfaceLang.Translate("PACKSUCCESSFUL") + " : " + wadFile, this.interfaceLang.Translate("INFORMATION"), MessageBoxButtons.OK, MessageBoxIcon.Asterisk);
                    return null;
                }
                updater.BeginInvoke(false, index, 2, "", null, null);
                Thread.Sleep(50);
                return null;
            }
            return (this.interfaceLang.Translate("WADPACKFAIL") + " : " + error);
        }

        private BlockedGamesManager.BlockedGameType ParseBlockedGameType(string value)
        {
            return (BlockedGamesManager.BlockedGameType) Enum.Parse(typeof(BlockedGamesManager.BlockedGameType), value, false);
        }

        private string parseDiscId(string fullPath)
        {
            int indexOfPoint = fullPath.LastIndexOf('.');
            if ((indexOfPoint > 0) && (fullPath.Length == (indexOfPoint + 4)))
            {
                return fullPath.Substring(indexOfPoint - 6, 6);
            }
            return "";
        }

        private void parseIso()
        {
            WiiDisc d = null;
            List<DiscFile> files = null;
            IIOContext context = null;
            string bannerFullPath = null;
            context = IOManager.CreateIOContext("ISWIIISO", this.txtIsoFile.Text, FileAccess.Read, FileShare.ReadWrite, 0, FileMode.Open, EFileAttributes.None);
            if (context.Result != 0)
            {
                int result = context.Result;
                MessageBox.Show(string.Format(this.interfaceLang.Translate("ERROROPENINGISO") + " : ", result.ToString()));
            }
            else
            {
                int r = 0;
                d = new WiiDisc(context, false);
                d.GenerateExtendedInfo = true;
                d.Open();
                r = d.BuildDisc(~(PartitionSelection.OtherPartitionType | PartitionSelection.GamePartitionType));
                int gamePartition = 1;
                if (r != 0)
                {
                    d.Close();
                    context.Close();
                    MessageBox.Show(string.Format(this.interfaceLang.Translate("ISODISCBUILDERROR"), r));
                }
                else
                {
                    for (int i = 0; i < d.NumPartitions; i++)
                    {
                        if (d.Partitions[i].Type == 0)
                        {
                            gamePartition = i;
                        }
                    }
                    bannerFullPath = ((this.baseDirectory + @"\banners\") + this.stripSpecialCharactersFromFilename(d.name) + " - ") + d.code + ".bnr";
                    d.ExtractFile(bannerFullPath, gamePartition, "opening.bnr");
                    files = new List<DiscFile>(d.ListDols(gamePartition));
                    this.cmbDolList.Items.Clear();
                    for (int i = 0; i < files.Count; i++)
                    {
                        DiscFile discfile = files[i];
                        DiscFile file5 = files[i];
                        d.ExtractFile(this.baseDirectory + @"\Alt-Dol\" + file5.Name, discfile);
                        DiscFile file4 = files[i];
                        DiscFile file3 = files[i];
                        Debug.WriteLine(string.Format("File : {0} - Offset {1}", file3.Name, file4.Offset));
                        DiscFile item = files[i];
                        this.cmbDolList.Items.Add(item);
                    }
                    this.altDolCount = files.Count;
                    if (!this.handleBannerChange(bannerFullPath, d.name))
                    {
                        MessageBox.Show(this.interfaceLang.Translate("INVALIDBANNERHEADER"), this.interfaceLang.Translate("ERROR"), MessageBoxButtons.OK, MessageBoxIcon.Hand);
                        d.Close();
                        context.Close();
                    }
                    else
                    {
                        d.Close();
                        context.Close();
                    }
                }
            }
        }

        private string parseNameFromWbfsFilePath(string path)
        {
            string pathTillBracket = null;
            pathTillBracket = path.Remove(path.Length - 20, 20);
            int lastPathIndex = pathTillBracket.LastIndexOf('\\');
            return pathTillBracket.Substring(lastPathIndex + 1, pathTillBracket.Length - (lastPathIndex + 1)).Trim();
        }

        private string parseNameFromWbfsFileSystemPath(string path)
        {
            return path.Substring(14, path.Length - 14);
        }

        private string parseTitleId(string fullPath)
        {
            int indexOfPoint = fullPath.LastIndexOf('.');
            if (indexOfPoint > 0)
            {
                Console.WriteLine(fullPath.Length);
                Console.WriteLine(indexOfPoint);
                if (fullPath.Length == (indexOfPoint + 5))
                {
                    return fullPath.Substring(indexOfPoint - 4, 4);
                }
            }
            return "";
        }

        private string parseWbfsFile(string wbfsFile,  bool silent)
        {
            string bannerFullPath = null;
            string gameId = null;
            string actualBannerFileName = null;
            try
            {
                //gameId = filename.Substring(0, 6);
                string filename = Path.GetFileName(wbfsFile);

                gameId = GetDiscCodeFromWBFSFile(filename);

                if (gameId == String.Empty) throw new Exception("Can't decide disc id of the game from given filename");
                bannerFullPath = (this.baseDirectory + @"\banners\") + gameId + ".bnr";
                actualBannerFileName = this.getNameOfBannerFileFromWbfsFile(wbfsFile, gameId);
                if (actualBannerFileName != string.Empty)
                {
                    if (!this.parseWbfsFileInternal(wbfsFile, gameId, actualBannerFileName, bannerFullPath))
                    {
                        if (silent)
                        {
                            throw new Exception(this.interfaceLang.Translate("WBFS_BANNER_EXTRACTERROR"));
                        }
                        MessageBox.Show(this.interfaceLang.Translate("WBFS_BANNER_EXTRACTERROR"), this.interfaceLang.Translate("ERROR"), MessageBoxButtons.OK, MessageBoxIcon.Hand);
                    }
                    else if (!File.Exists(bannerFullPath))
                    {
                        if (silent)
                        {
                            throw new Exception(this.interfaceLang.Translate("WBFS_BANNER_EXTRACTERROR"));
                        }
                        MessageBox.Show(this.interfaceLang.Translate("WBFS_BANNER_EXTRACTERROR"), this.interfaceLang.Translate("ERROR"), MessageBoxButtons.OK, MessageBoxIcon.Hand);
                    }
                    else
                    {
                        if (silent || this.handleBannerChange(bannerFullPath, ""))
                        {
                            return bannerFullPath;
                        }
                        MessageBox.Show(this.interfaceLang.Translate("INVALIDBANNERHEADER"), this.interfaceLang.Translate("ERROR"), MessageBoxButtons.OK, MessageBoxIcon.Hand);
                    }
                }
                else
                {
                    if (silent)
                    {
                        throw new Exception(this.interfaceLang.Translate("WBFS_BANNER_LISTERROR"));
                    }
                    MessageBox.Show(this.interfaceLang.Translate("WBFS_BANNER_LISTERROR"), this.interfaceLang.Translate("ERROR"), MessageBoxButtons.OK, MessageBoxIcon.Hand);
                }
            }
            catch (Exception e)
            {
                if (silent)
                {
                    throw e;
                }
                MessageBox.Show(string.Format(this.interfaceLang.TranslateAndReplace("WBFS_BANNER_EXTRACTSYSTEMERROR", @"\n", "\n"), e.ToString()), this.interfaceLang.Translate("ERROR"), MessageBoxButtons.OK, MessageBoxIcon.Hand);
            }
            return string.Empty;
        }

        private string GetDiscCodeFromWBFSFile(string wbfsFile)
        {
            if (wbfsFile.Contains("[") && wbfsFile.Contains("]"))
            {
                Regex regex = new Regex("\\[......\\]");
                Match match = regex.Match(wbfsFile);
                if ((match == null) || (match.Length != 8)) return String.Empty;
                return match.Value.Replace("[", "").Replace("]", "");
            }
            else if (wbfsFile.ToUpperInvariant().Replace(".WBFS", "").Length == 6)
            {
                return wbfsFile.ToUpperInvariant().Replace(".WBFS", "");
            }
            else
            {
                return String.Empty;
            }
        }

        private bool parseWbfsFileInternal(string wbfsFilePath, string gameId, string actualBannerFileName, string destinationBannerFilename)
        {
            Process myProcess = null;
            string output = null;
            myProcess = new Process();
            myProcess.StartInfo.FileName = Path.Combine(Path.Combine(this.baseDirectory, "3rdParty"), "wbfs_file.exe");
            object[] args = new object[] { wbfsFilePath, gameId, actualBannerFileName, destinationBannerFilename };
            myProcess.StartInfo.Arguments = string.Format(" \"{0}\" extract_file {1} {2} \"{3}\"", args);
            Console.Out.WriteLine(string.Format("Invoking {0} with arguments {1}", myProcess.StartInfo.FileName, myProcess.StartInfo.Arguments));
            myProcess.StartInfo.UseShellExecute = false;
            myProcess.StartInfo.CreateNoWindow = true;
            myProcess.StartInfo.WindowStyle = ProcessWindowStyle.Hidden;
            myProcess.StartInfo.RedirectStandardOutput = true;
            myProcess.Start();
            output = myProcess.StandardOutput.ReadToEnd();
            myProcess.WaitForExit();
            Console.Out.WriteLine("Output from wbfs_file.exe (for extraction) : " + output);
            return (myProcess.ExitCode == 0);
        }

        private string parseWbfsFileSystem(string selectedDrive, string selectedDiscId,  bool silent,  bool extractAltDols)
        {
            WiiDisc d = null;
            List<DiscFile> files = null;
            DiscReader context = null;
            string bannerFullPath = null;
            WBFSDevice device = null;
            string gameName = null;
            device = new WBFSDevice();
            if (WBFSDevice.IsWBFSDrive(selectedDrive, false) != 0)
            {
                if (silent)
                {
                    throw new Exception(this.interfaceLang.Translate("NOTAWBFSDRIVE"));
                }
                MessageBox.Show(this.interfaceLang.Translate("NOTAWBFSDRIVE"), this.interfaceLang.Translate("ERROR"), MessageBoxButtons.OK, MessageBoxIcon.Hand);
                return string.Empty;
            }
            device.Open(selectedDrive, true);
            device.EnumerateDiscs();
            device.GetDeviceInfo();
            context = new DiscReader(device.GetDiscByCode(selectedDiscId));
            if (context.Result != 0)
            {
                if (!silent)
                {
                    int result = context.Result;
                    MessageBox.Show(string.Format(this.interfaceLang.Translate("WBFSDISCREADERROR"), result.ToString()));
                }
                else
                {
                    int num = context.Result;
                    throw new Exception(string.Format(this.interfaceLang.Translate("WBFSDISCREADERROR"), num.ToString()));
                }
                return string.Empty;
            }
            d = new WiiDisc(context, false);
            d.GenerateExtendedInfo = true;
            d.Open();
            int r = 0;
            int gamePartition = 1;
            r = d.BuildDisc(~(PartitionSelection.OtherPartitionType | PartitionSelection.GamePartitionType));
            if (r != 0)
            {
                d.Close();
                context.Close();
                if (silent)
                {
                    throw new Exception(string.Format(this.interfaceLang.Translate("WBFSDISCBUILDERROR"), r));
                }
                MessageBox.Show(string.Format(this.interfaceLang.Translate("WBFSDISCBUILDERROR"), r));
                return string.Empty;
            }
            for (int i = 0; i < d.NumPartitions; i++)
            {
                if (d.Partitions[i].Type == 0)
                {
                    gamePartition = i;
                }
            }
            bannerFullPath = ((this.baseDirectory + @"\banners\") + this.stripSpecialCharactersFromFilename(d.name) + " - ") + d.code + ".bnr";
            d.ExtractFile(bannerFullPath, gamePartition, "opening.bnr");
            if (extractAltDols)
            {
                files = new List<DiscFile>(d.ListDols(gamePartition));
                this.cmbDolList.Items.Clear();
                for (int i = 0; i < files.Count; i++)
                {
                    DiscFile discfile = files[i];
                    DiscFile file5 = files[i];
                    d.ExtractFile(this.baseDirectory + @"\Alt-Dol\" + file5.Name, discfile);
                    DiscFile file4 = files[i];
                    DiscFile file3 = files[i];
                    Debug.WriteLine(string.Format("File : {0} - Offset {1}", file3.Name, file4.Offset));
                    DiscFile item = files[i];
                    this.cmbDolList.Items.Add(item);
                }
                this.altDolCount = files.Count;
            }
            gameName = d.name;
            d.Close();
            context.Close();
            if (!silent && !this.handleBannerChange(bannerFullPath, gameName))
            {
                MessageBox.Show(this.interfaceLang.Translate("BANNERCHECKERROR"), this.interfaceLang.Translate("ERROR"), MessageBoxButtons.OK, MessageBoxIcon.Hand);
                return string.Empty;
            }
            return bannerFullPath;
        }

        private void portoqueseToolStripMenuItem_Click(object sender, EventArgs e)
        {
            this.changeLanguage("Portoquese");
        }

        private void RealUpdater( bool isFinal, int index, int type, string status)
        {
            if (type == 0)
            {
                this.listBox1.Items[index] = this.listBox1.Items[index] + this.interfaceLang.Translate("BATCHREPORTUNPACK");
            }
            else if (type == 1)
            {
                this.listBox1.Items[index] = this.listBox1.Items[index] + this.interfaceLang.Translate("BATCHREPORTPATCH");
            }
            else if (type == 2)
            {
                this.listBox1.Items[index] = this.listBox1.Items[index] + this.interfaceLang.Translate("BATCHREPORTPACK");
            }
            else if (type == 4)
            {
                this.listBox1.Items[index] = ((string) this.listBox1.Items[index]) + " " + status;
            }
            this.toolStripStatusLabel1.Text = this.listBox1.Items[index].ToString();
            if (isFinal)
            {
                Thread.Sleep(200);
                this.toolStripStatusLabel1.Text = string.Format(this.interfaceLang.Translate("BATCH_CREATION_RESULT"), successful, failed);
            }
            this.toolStripProgressBar1.ProgressBar.Value = index;
        }

        public void ReflectLanguageChanges()
        {
            this.interfaceLang = new MultiLanguageModuleHelper(this.mlHelper, "GUIDynamic");
            this.guiLang = new MultiLanguageModuleHelper(this.mlHelper, "GUIStatic");
            this.wiiloadLang = new MultiLanguageModuleHelper(this.mlHelper, "Wiiload");
            this.settingsLang = new MultiLanguageModuleHelper(this.mlHelper, "Configuration");
            this.updatesLang = new MultiLanguageModuleHelper(this.mlHelper, "Update");
            this.loadMLResources();
        }

        private void refreshDrives()
        {
            DriveInfo[] drives = null;
            drives = DriveInfo.GetDrives();
            this.cmbDriveList.Items.Clear();
            for (int i = 0; i < drives.Length; i++)
            {
                this.cmbDriveList.Items.Add(drives[i].Name);
            }
        }

        private string replaceUnicode(string fileName)
        {
            StringBuilder sb = null;
            byte[] chars = null;
            sb = new StringBuilder();
            chars = Encoding.BigEndianUnicode.GetBytes(fileName);
            for (int i = 0; i < fileName.Length; i++)
            {
                if (chars[i * 2] > 0)
                {
                    sb.Append("!");
                }
                else
                {
                    sb.Append(fileName[i]);
                }
            }
            return sb.ToString();
        }

        private void report(int index, int status)
        {
            if (status == 0)
            {
                this.listBox1.Items[index] = ((string) this.listBox1.Items[index]) + " " + this.interfaceLang.Translate("BATCHREPORTUNPACK");
            }
            if (status == 1)
            {
                this.listBox1.Items[index] = ((string) this.listBox1.Items[index]) + " " + this.interfaceLang.Translate("BATCHREPORTPATCH");
            }
            if (status == 1)
            {
                this.listBox1.Items[index] = ((string) this.listBox1.Items[index]) + " " + this.interfaceLang.Translate("BATCHREPORTPACK");
            }
            Thread.Sleep(50);
        }

        private void russianToolStripMenuItem_Click(object sender, EventArgs e)
        {
            this.changeLanguage("Russian");
        }

        private void saveConfig(string selectedLanguage)
        {
            System.Configuration.Configuration config = null;
            try
            {
                config = ConfigurationManager.OpenExeConfiguration(ConfigurationUserLevel.None);
                config.AppSettings.Settings.Remove("language");
                config.AppSettings.Settings.Add("language", selectedLanguage);
                config.Save(ConfigurationSaveMode.Modified);
                ConfigurationManager.RefreshSection("appSettings");
            }
            catch (Exception ex)
            {
                MessageBox.Show(this.guiLang.TranslateAndReplace("CONFIGSAVEERROR", @"\n", "\n") + ": " + ex.Message, "Error", MessageBoxButtons.OK, MessageBoxIcon.Hand);
            }
        }

        private void sChineseToolStripMenuItem_Click(object sender, EventArgs e)
        {
            this.changeLanguage("S.Chinese");
        }

        private bool searchAndReplaceInArray(byte[] content, byte[] searchValue, byte[] replaceValue)
        {
            int i = this.findArrayInArray(content, searchValue);
            if (i <= 0)
            {
                return false;
            }
            for (int k = 0; k < replaceValue.Length; k++)
            {
                content[i + k] = replaceValue[k];
            }
            return true;
        }

        private void setChannel(string titleId)
        {
            this.txtDiscId.Text = titleId;
            this.txtTitleId.Text = "U" + this.txtDiscId.Text.Substring(1, 3);
            this.label4.Text = this.guiLang.Translate("TITLEIDLABEL") + " :";
        }

        private void setChannelLoaders()
        {
            this.cmbLoaders.Items.Clear();
            this.hideOptionPanels();
            this.addChannelLoaders();
        }

        private void setDisc(string discId)
        {
            this.txtDiscId.Text = discId;
            this.txtTitleId.Text = "U" + discId.Substring(1, 3);
            this.label4.Text = this.guiLang.Translate("DISCIDLABEL") + " :";
        }

        private void setDiscLoaders()
        {
            this.cmbLoaders.Items.Clear();
            this.hideOptionPanels();
            this.addDiscLoaders();
        }

        private ChannelPackParams SetLoaderOptions(bool forBatchMode, string[] banners, StatusUpdater updater)
        {
            int fixes;
            string altDolFile = null;
            int wadNaming;
            string forwarderParameters = null;
            string selectedLoader = null;
            uint altDolOffset;
            byte selectedLanguage = 0;
            int selectedAltDol = 0;
            bool isForwarder = this.panelPartition.Visible;
            bool altDolEnabled = this.cmbAltDolType.Enabled;
            bool regionOverrideEnabled = this.cmbRegion.Enabled;
            byte selectedRegion = (byte) ((string) this.cmbRegion.SelectedItem)[0];
            bool forceVideo = (this.chkForceVideoMode.Enabled && this.chkForceVideoMode.Checked);

            int selectedLoaderIndex = this.cmbLoaders.SelectedIndex;
            int altDolType = 0;
            int selectedPartition = -1;
            if (altDolEnabled)
            {
                if (this.cmbAltDolType.SelectedIndex > 0)
                {
                    if (forBatchMode)
                    {
                        MessageBox.Show(this.interfaceLang.Translate("CANTUSE_ALTDOL_INBATCHMODE"), this.interfaceLang.Translate("ERROR"), MessageBoxButtons.OK, MessageBoxIcon.Hand);
                        return null;
                    }
                    if (this.cmbDolList.SelectedItem == null)
                    {
                        MessageBox.Show(this.interfaceLang.Translate("SELECTDOL"), this.interfaceLang.Translate("ERROR"), MessageBoxButtons.OK, MessageBoxIcon.Hand);
                        return null;
                    }
                    altDolType = this.cmbAltDolType.SelectedIndex;
                    if ((altDolType == 2) && (((DiscFile) this.cmbDolList.SelectedItem).Name.Length > 12))
                    {
                        MessageBox.Show(this.interfaceLang.Translate("INSANEERROR"), this.interfaceLang.Translate("ERROR"), MessageBoxButtons.OK, MessageBoxIcon.Hand);
                        return null;
                    }
                }
            }
            else
            {
                altDolType = -1;
            }
            if ((regionOverrideEnabled && (selectedRegion != 0x30)) && forceVideo)
            {
                MessageBox.Show(this.interfaceLang.Translate("OVERRIDEANDFORCENOPEZ"), this.interfaceLang.Translate("ERROR"), MessageBoxButtons.OK, MessageBoxIcon.Hand);
                return null;
            }
            if (selectedLoaderIndex < 0)
            {
                MessageBox.Show(this.interfaceLang.Translate("SELECTLOADER"), this.interfaceLang.Translate("ERROR"), MessageBoxButtons.OK, MessageBoxIcon.Hand);
                return null;
            }
            if (!forBatchMode && (this.txtTitleId.Text.Length != 4))
            {
                MessageBox.Show(this.interfaceLang.Translate("ENTERTITLEID"), this.interfaceLang.Translate("ERROR"), MessageBoxButtons.OK, MessageBoxIcon.Hand);
                return null;
            }
            bool forceLanguage = false;
            if (this.cmbLanguage.Enabled)
            {
                forceLanguage = true;
                selectedLanguage = Convert.ToByte(this.cmbLanguage.SelectedItem.ToString()[0]);
            }
            else
            {
                forceLanguage = false;
                selectedLanguage = 0x30;
            }
            bool forceLoader = false;
            if (this.cmbLoaderType.Enabled)
            {
                forceLoader = true;
                selectedLoader = (string) this.cmbLoaderType.SelectedItem;
            }
            else
            {
                forceLoader = false;
                selectedLoader = "";
            }
            if (this.chkOldStyle002Fix.Enabled)
            {
                fixes = 0;
                if (this.chkOldStyle002Fix.Checked)
                {
                    fixes++;
                }
                if (this.chkNewStyle002Fix.Checked)
                {
                    fixes += 2;
                }
                if (this.chkAnti002Fix.Checked)
                {
                    fixes += 4;
                }
            }
            else
            {
                fixes = 0;
            }
            if (altDolType > 0)
            {
                altDolFile = ((DiscFile) this.cmbDolList.SelectedItem).Name;
                altDolOffset = ((DiscFile) this.cmbDolList.SelectedItem).Offset;
            }
            else
            {
                altDolFile = "--------.---";
                altDolOffset = 0;
            }
            if (this.radBtn1.Checked)
            {
                wadNaming = 0;
            }
            else if (this.radBtn2.Checked)
            {
                wadNaming = 1;
            }
            else if (this.radBtn3.Checked)
            {
                wadNaming = 2;
            }
            else
            {
                wadNaming = 0;
            }

			bool verboseLog = (chkVerbose.Enabled) && (chkVerbose.Checked);
			bool ocarinaEnabled = (chkOcarinaSupport.Enabled) && (chkOcarinaSupport.Checked);

            if (altDolType == 2)
            {
                MessageBox.Show(string.Format(this.interfaceLang.Translate("ALTDOLNOTIFY"), altDolFile), this.interfaceLang.Translate("ALTDOLNOTIFYHEADER"), MessageBoxButtons.OK, MessageBoxIcon.Asterisk);
            }
            forwarderParameters = string.Empty;
            if (isForwarder)
            {
                if (this.cmbPartition.Enabled && (this.cmbPartition.SelectedIndex < 0))
                {
                    MessageBox.Show(string.Format(this.interfaceLang.Translate("SELECT_PARTITION"), altDolFile), this.interfaceLang.Translate("ALTDOLNOTIFYHEADER"), MessageBoxButtons.OK, MessageBoxIcon.Asterisk);
                    return null;
                }
                selectedPartition = this.cmbPartition.SelectedIndex;
                if (this.txtExtraParameters.Text.Length > 0x7f)
                {
                    MessageBox.Show(string.Format(this.interfaceLang.Translate("EXTRA_PARAMETERS_TOO_LONG"), altDolFile), this.interfaceLang.Translate("ALTDOLNOTIFYHEADER"), MessageBoxButtons.OK, MessageBoxIcon.Asterisk);
                    return null;
                }
                forwarderParameters = this.txtExtraParameters.Text;
            }
            return new ChannelPackParams(banners, regionOverrideEnabled, selectedRegion, forceVideo, verboseLog, ocarinaEnabled, forceLanguage, selectedLanguage, forceLoader, fixes, selectedLoader, wadNaming, altDolType, selectedAltDol, altDolOffset, altDolFile, selectedPartition, isForwarder, forwarderParameters, updater);
        }

        private void SetSelectedLoader(bool isDiscLoader, int i, Loader loader)
        {
            string strSupportedParameters = null;
            if (this.cmbLoaders.SelectedItem.Equals(loader.Title))
            {
                this.lblAuthor.Text = loader.Author;
                this.lblDolFilename.Text = loader.Filename;
                this.lblModder.Text = loader.Filename;
                if (loader.RegionOverride)
                {
                    this.lblRegionOverride.Text = this.interfaceLang.Translate("FEATUREEXIST");
                    this.cmbRegion.Enabled = true;
                }
                else
                {
                    this.lblRegionOverride.Text = this.interfaceLang.Translate("FEATUREDONOTEXIST");
                    this.cmbRegion.Enabled = false;
                }
                if (loader.VerboseLogSupport)
                {
                    this.lblVerboseLog.Text = this.interfaceLang.Translate("FEATUREEXIST");
                    this.chkVerbose.Enabled = true;
                }
                else
                {
                    this.chkVerbose.Enabled = false;
                    this.lblVerboseLog.Text = this.interfaceLang.Translate("FEATUREDONOTEXIST");
                }
                if (loader.OcarinaConfiguration)
                {
                    this.lblOcarinaSupport.Text = this.interfaceLang.Translate("FEATUREEXIST");
                    this.chkOcarinaSupport.Enabled = true;
                }
                else
                {
                    this.chkOcarinaSupport.Enabled = false;
                    this.lblOcarinaSupport.Text = this.interfaceLang.Translate("FEATUREDONOTEXIST");
                }
                if (loader.ForcedVideoModeSelection)
                {
                    this.lblForceVideoModeSupport.Text = this.interfaceLang.Translate("FEATUREEXIST");
                    this.chkForceVideoMode.Enabled = true;
                }
                else
                {
                    this.chkForceVideoMode.Enabled = false;
                    this.lblForceVideoModeSupport.Text = this.interfaceLang.Translate("FEATUREDONOTEXIST");
                }
                if (loader.LanguageSelection)
                {
                    this.lblForceLanguageSupport.Text = this.interfaceLang.Translate("FEATUREEXIST");
                    this.cmbLanguage.Enabled = true;
                }
                else
                {
                    this.cmbLanguage.Enabled = false;
                    this.lblForceLanguageSupport.Text = this.interfaceLang.Translate("FEATUREDONOTEXIST");
                }
                if (isDiscLoader)
                {
                    this.lblDefaultDiscId.Text = loader.DiscIdPlaceHolder;
                    this.label8.Text = this.guiLang.Translate("L_DEFAULTDISCID");
                }
                else
                {
                    this.lblDefaultDiscId.Text = ((ChannelLoader) loader).TitleIdPlaceHolder;
                    this.label8.Text = this.guiLang.Translate("L_DEFAULTTITLEID");
                }
                this.lblConfigPlaceholder.Text = loader.ConfigPlaceHolder;
                if (loader.SupportsSdSdhcCard)
                {
                    this.lblSdCardSupport.Text = this.interfaceLang.Translate("FEATUREEXIST");
                    this.cmbLoaderType.Enabled = true;
                }
                else
                {
                    this.cmbLoaderType.Enabled = false;
                    this.lblSdCardSupport.Text = this.interfaceLang.Translate("FEATUREDONOTEXIST");
                }
                if (isDiscLoader)
                {
                    if (loader.SupportsAltDols)
                    {
                        this.lblAltDolSupport.Text = this.interfaceLang.Translate("FEATUREEXIST");
                        if (this.altDolCount != 0)
                        {
                            this.cmbAltDolType.Enabled = true;
                            this.cmbDolList.Enabled = true;
                        }
                    }
                    else
                    {
                        this.cmbAltDolType.Enabled = false;
                        this.cmbDolList.Enabled = false;
                        this.lblAltDolSupport.Text = this.interfaceLang.Translate("FEATUREDONOTEXIST");
                    }
                }
                else
                {
                    this.cmbAltDolType.Enabled = false;
                    this.cmbDolList.Enabled = false;
                }
                if (isDiscLoader)
                {
                    if (loader.SupportFixes)
                    {
                        this.lblError002Fix.Text = this.interfaceLang.Translate("FEATUREEXIST");
                        this.chkAnti002Fix.Enabled = true;
                        this.chkNewStyle002Fix.Enabled = true;
                        this.chkOldStyle002Fix.Enabled = true;
                    }
                    else
                    {
                        this.lblError002Fix.Text = this.interfaceLang.Translate("FEATUREDONOTEXIST");
                        this.chkAnti002Fix.Enabled = false;
                        this.chkNewStyle002Fix.Enabled = false;
                        this.chkOldStyle002Fix.Enabled = false;
                    }
                }
                else
                {
                    this.chkAnti002Fix.Enabled = false;
                    this.chkNewStyle002Fix.Enabled = false;
                    this.chkOldStyle002Fix.Enabled = false;
                }
                if (!isDiscLoader)
                {
                    this.lblAltDolSupport.Text = this.guiLang.Translate("FEATURENOTRELATED");
                    this.lblError002Fix.Text = this.guiLang.Translate("FEATURENOTRELATED");
                    this.panelOptions.Visible = true;
                    this.panelPartition.Visible = false;
                }
                else if (loader.SupportsPartitionSelection || loader.IsForwarder)
                {
                    this.panelPartition.Visible = true;
                    this.panelOptions.Visible = false;
                    if (loader.SupportsPartitionSelection)
                    {
                        this.cmbPartition.Enabled = true;
                    }
                    else
                    {
                        this.cmbPartition.Enabled = false;
                        this.cmbPartition.SelectedItem = null;
                    }
                    if (loader.IsForwarder)
                    {
                        strSupportedParameters = ((Forwarder) loader).SupportedParameters;
                        if (!string.IsNullOrEmpty(strSupportedParameters))
                        {
                            this.fillExtraParameters(strSupportedParameters);
                        }
                        else
                        {
                            this.lstExtraParameters.Items.Clear();
                        }
                    }
                }
                else
                {
                    this.panelOptions.Visible = true;
                    this.panelPartition.Visible = false;
                }
            }
        }

        private void showInfoWindow()
        {
            this.frm = new InfoForm(this.appDomain);
            this.frm.ShowDialog(this);
        }

        private string sizeToGB(double size)
        {
            double x = size / 1073741824.0;
            return (x.ToString("0.000") + " GB");
        }

        private void spanishToolStripMenuItem_Click(object sender, EventArgs e)
        {
            this.changeLanguage("Spanish");
        }

        private string stripSpecialCharactersFromFilename(string fileName)
        {
            string result = null;
            result = fileName;
            return result.Replace(":", "-").Replace(@"\", "").Replace("/", "").Replace("*", "_").Replace("?", "").Replace("<", "[").Replace(">", "]").Replace("|", "");
        }

        private void swedishToolStripMenuItem_Click(object sender, EventArgs e)
        {
            this.changeLanguage("Swedish");
        }

        private void tChineseToolStripMenuItem_Click(object sender, EventArgs e)
        {
            this.changeLanguage("T.Chinese");
        }

        private void toolStripMenuItem1_Click(object sender, EventArgs e)
        {
            this.changeLanguage("French-1");
        }

        private void turToolStripMenuItem_Click(object sender, EventArgs e)
        {
            this.changeLanguage("Turkish");
        }

        private string unpackSelectedWad(string titleIdChannel)
        {
            string error = null;
            string tmdPath = null;
            string tempFolder = null;
            this.txtChannel.Text = this.openFileDialog2.FileName;
            Directory.SetCurrentDirectory(this.baseDirectory);
            try
            {
                Directory.Delete("TempWad", true);
            }
            catch (Exception)
            {
            }
            tempFolder = this.baseDirectory + @"\tempwad";
            tmdPath = string.Empty;
            int returnCode = 0;
            error = string.Empty;
            try
            {
                tmdPath = WadUnpack.UnpackWad(this.txtChannel.Text, tempFolder);
            }
            catch (Exception ex)
            {
                returnCode = -1;
                error = ex.Message;
            }
            if (returnCode == 0)
            {
                titleIdChannel = WadInfo.GetTitleID(tmdPath, 1);
                return string.Empty;
            }
            return (this.interfaceLang.Translate("WADUNPACKFAILED") + " : " + error);
        }

        private void UpdateStats(bool isFinal, int index, int type, string status)
        {
            object[] parameters = null;
            StatusUpdater updater = null;
            updater = new StatusUpdater(this.RealUpdater);
            parameters = new object[] { isFinal, index, type, status };
            this.listBox1.Invoke(updater, parameters);
        }

        private void viewWBFSDrive()
        {
            this.panelWBFS.Visible = true;
            if (this.cmbDriveList.SelectedItem == null)
            {
                this.refreshDrives();
            }
        }

        private void viewWBFSDriveToolStripMenuItem_Click(object sender, EventArgs e)
        {
            this.viewWBFSDrive();
        }

        private void WBFSDrive_Load(object sender, EventArgs e)
        {
            this.refreshDrives();
        }

        private void wBFSDriveToolStripMenuItem_Click(object sender, EventArgs e)
        {
            this.isoSelection();
        }

        // Nested Types
        public delegate void DelegateThreadTask(object parameters);

        private void updatesToolStripMenuItem_Click(object sender, EventArgs e)
        {
            Updates updates = new Updates(this, baseDirectory, this.updatesLang);
            updates.Show();
        }

    }
}
