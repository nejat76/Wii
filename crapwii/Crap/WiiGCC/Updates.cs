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
using Org.Irduco.LoaderManager;
using System.IO;
using System.Xml;
using System.Configuration;
using System.Net;
using System.Globalization;
using System.Net.Cache;
using System.Threading;

namespace WiiGSC
{
    public partial class Updates : Form
    {
        ItemCheckEventHandler handler;
        private string baseDirectory;        
        private MultiLanguageModuleHelper guiLang;
        Dictionary<string , UpdateManifestChange> applicableChangesDict;
        List<UpdateManifestChange> applicableChanges;
        private Form1 parent;
            

        private enum ProgressState
        {
            PreCleanup = -6,
            Cleanup = -5,
            UpdateCompleted = -4,
            DownloadError = -3,
            DownloadCompleted = -2,
            DownloadProgress = -1,            
            BeforeProcess = 0
        }

        public Updates()
        {
            InitializeComponent();
        }

        public Updates(Form1 parent, string baseDirectory, MultiLanguageModuleHelper guiLang)
        {
            this.parent = parent;
            this.InitializeComponent();
            this.baseDirectory = baseDirectory;
            this.guiLang = guiLang;
            loadMLResources();
            handler = new ItemCheckEventHandler(ListView1_ItemCheck1);
        }

        private void btnCheckUpdates_Click(object sender, EventArgs e)
        {
            for (int i = updateList.Items.Count-1; i >= 0; i--)
            {
                updateList.Items[i].Remove();
            }
            Dictionary<string, UpdateManifestItem> existingItems = GetExistingItems();

            Dictionary<string, UpdateManifestItem> updatedItems = new Dictionary<string, UpdateManifestItem>();
            List<UpdateManifestChange> updateManifestChanges = GetUpdatedItems(updatedItems);
            applicableChanges = new List<UpdateManifestChange>();

            //Iterate applicableChanges and compare against existingItems
            //For each change that contains an updated item that has higher version agains it's counterpart in existingItems

            foreach (UpdateManifestChange change in updateManifestChanges)
            {
                bool added = false;
                foreach (UpdateManifestItem item in change.Items)
                {
                    if (existingItems.ContainsKey(item.ContentFile))
                    {
                        UpdateManifestItem existingItem = existingItems[item.ContentFile];
                        item.OldVersion = existingItem.Version;
                        if (item.IsNewer(existingItem.Version))
                        {
                            if (!added)
                            {
                                applicableChanges.Add(change);
                            }
                            added = true;
                            item.NeedsUpdate = true;
                        }
                        else
                        {
                            item.NeedsUpdate = false;
                        }
                    }
                    else
                    {
                        if (!added)
                        {
                            applicableChanges.Add(change);
                        }
                        added = true;
                        item.OldVersion = "";
                        item.NeedsUpdate = true;
                    }
                }
            }

            applicableChangesDict = new Dictionary<string, UpdateManifestChange>();
            for (int i = 0; i < applicableChanges.Count; i++)
            {
                applicableChangesDict.Add(applicableChanges[i].Id, applicableChanges[i]);
            }

            if (applicableChanges.Count == 0) MessageBox.Show(guiLang.Translate("NO_APPLICABLE_CHANGES"));

            updateList.CheckBoxes = true;
            for (int i=0;i<applicableChanges.Count;i++) 
            {
                UpdateManifestChange change = applicableChanges[i];
                ListViewGroup viewGroup = new ListViewGroup(change.Id, change.Description + " ( " + change.UpdateDate + " )" );
                updateList.Groups.Add(viewGroup);

                for (int j = 0; j < change.Items.Count; j++)
                {
                    ListViewItem item = new ListViewItem(viewGroup);
                    item.SubItems.Add(new ListViewItem.ListViewSubItem(item, change.Items[j].ContentPath + "/" + change.Items[j].ContentFile));
                    item.SubItems.Add(new ListViewItem.ListViewSubItem(item, change.Items[j].OldVersion));
                    item.SubItems.Add(new ListViewItem.ListViewSubItem(item, change.Items[j].Version));
                    string status = String.Empty;
                    if (change.Items[j].OldVersion == "")
                    {
                        status = guiLang.Translate("NEW");
                    }
                    else if (change.Items[j].OldVersion == change.Items[j].Version)
                    {
                        status = guiLang.Translate("LATEST");
                        item.ForeColor = Color.Gray;
                    }
                    else if (change.Items[j].NeedsUpdate)
                    {
                        status = guiLang.Translate("UPDATED");
                    }
                    else
                    {
                        status = guiLang.Translate("ALREADY_NEW");
                        item.ForeColor = Color.Gray;
                    }
                    item.SubItems.Add(new ListViewItem.ListViewSubItem(item, status));
                    item.SubItems.Add(new ListViewItem.ListViewSubItem(item, change.Items[j].Description));                    
                    updateList.Items.Add(item);

                }
                
            }

        }

        private void ListView1_ItemCheck1(object sender, System.Windows.Forms.ItemCheckEventArgs e)
        {
            updateList.ItemCheck -= handler;
            string status = this.updateList.Items[e.Index].SubItems[4].Text;

            if ((e.NewValue == CheckState.Checked) && ((status != guiLang.Translate("NEW") && (status != guiLang.Translate("UPDATED")))))
            {
                e.NewValue = CheckState.Unchecked;
            }

            string groupName = this.updateList.Items[e.Index].Group.Name;

            if ((status == guiLang.Translate("NEW")) || (status == guiLang.Translate("UPDATED")))
            {
                for (int i = 0; i < updateList.Items.Count; i++)
                {
                    string statusItem = this.updateList.Items[i].SubItems[4].Text;
                    if (this.updateList.Items[i].Group.Name == groupName && ((statusItem == guiLang.Translate("NEW")) || (statusItem == guiLang.Translate("UPDATED"))))
                    {
                        if (i != e.Index)
                        {
                            this.updateList.Items[i].Checked = (e.NewValue == CheckState.Checked);
                        }
                    }
                }
            }

            updateList.ItemCheck += handler;
        }

        private List<UpdateManifestChange> GetUpdatedItems(Dictionary<string, UpdateManifestItem> updatedItems)
        {            
            //Get update manifest
            XmlDocument updateManifestXmlDoc = GetUpdateManifest();            

            List<UpdateManifestChange> changeList = new List<UpdateManifestChange>();
            //Process update manifest only get the highest version of any item and fill the updatedItems

            XmlNodeList changeNodeList = GetUpdateList(updateManifestXmlDoc);

            foreach (XmlNode node in changeNodeList)
            {
                UpdateManifestChange changeItem = GetChangeDetails(node);

                XmlNodeList changes = node.SelectNodes("Content/Item");
                bool allOlder = true;
                foreach (XmlNode item in changes)
                {
                    string version = item.Attributes["version"].InnerText;
                    string description = item.Attributes["desc"].InnerText;
                    string fullPath = item.InnerText;
                    string[] pathAndFile = fullPath.Split(new char[] { '/' });
                    string path = pathAndFile[0];
                    string fileName = pathAndFile[1];
                    

                    UpdateManifestItem updateManifestItem = new UpdateManifestItem(path, fileName, version);
                    updateManifestItem.Description = description;

                    if (updatedItems.ContainsKey(fileName))
                    {
                        UpdateManifestItem existing = updatedItems[fileName];

                        if (!existing.IsNewer(version))
                        {
                            updatedItems.Remove(fileName);
                            updatedItems.Add(fileName, updateManifestItem);
                        }
                    } 
                    else 
                    {
                        allOlder = false;
                        updatedItems.Add(fileName, updateManifestItem);
                    }

                    changeItem.Items.Add(updateManifestItem);
                }

                if (!allOlder)
                {
                    changeList.Add(changeItem);
                }
            }

            return changeList;
        }

        private static UpdateManifestChange GetChangeDetails(XmlNode node)
        {
            string description = node.SelectSingleNode("Description").InnerText;
            string id = node.Attributes["id"].InnerText;
            DateTime updateDate = DateTime.ParseExact(node.Attributes["date"].InnerText, "yyyy-MM-dd", CultureInfo.InvariantCulture);
            UpdateManifestChange changeItem = new UpdateManifestChange(id, updateDate, description);
            return changeItem;
        }

        private XmlNodeList GetUpdateList(XmlDocument updateManifestXmlDoc)
        {
            XmlNodeList changesList = updateManifestXmlDoc.SelectNodes(@"//Manifest/Update");
            return changesList;
        }

        private XmlDocument GetUpdateManifest()
        {
            string updateManifestUrl = ConfigurationSettings.AppSettings["UpdateManifestUrl"];
            StreamReader reader = null;
            XmlDocument xmlDoc = new XmlDocument();
            HttpRequestCachePolicy policy = new HttpRequestCachePolicy(HttpRequestCacheLevel.NoCacheNoStore);
            WebRequest request = HttpWebRequest.Create(updateManifestUrl);
            request.CachePolicy = policy;
            WebProxy proxy = AppUtils.GetProxyIfRequired();
            request.Proxy = (proxy != null) ? proxy : request.Proxy;
            request.Credentials = CredentialCache.DefaultCredentials;

            WebResponse response = request.GetResponse();
            reader = new StreamReader(response.GetResponseStream());
            xmlDoc.Load(reader);
            return xmlDoc;
        }

        private Dictionary<string, UpdateManifestItem> GetExistingItems()
        {
            //Get loader versions
            LoaderHelper loaderHelper = new LoaderHelper(this.baseDirectory + @"\" + "LoaderConfig.xml");
            List<BaseLoader> allLoaders = loaderHelper.AllLoaders;
            Dictionary<string, UpdateManifestItem> existingItems = new Dictionary<string, UpdateManifestItem>();
            foreach (BaseLoader loader in allLoaders)
            {
                if (File.Exists(this.baseDirectory + @"\Loaders\" + loader.Filename))
                {
                    UpdateManifestItem updateManifestItem = new UpdateManifestItem("Loaders", loader.Filename, loader.Version);
                    existingItems.Add(loader.Filename, updateManifestItem);
                }
            }

            List<NandLoader> allNandLoaders = loaderHelper.NandLoaders;
            foreach (BaseLoader loader in allLoaders)
            {
                if (File.Exists(this.baseDirectory + @"\NandLoaders\" + loader.Filename))
                {
                    UpdateManifestItem updateManifestItem = new UpdateManifestItem("NandLoaders", loader.Filename, loader.Version);
                    existingItems.Add(loader.Filename, updateManifestItem);
                }
            }

            //Get language resources versions
            //Lang klasöründeki tüm xml dosyalarından versiyon bilgilerini oku

            string[] xmlFiles = Directory.GetFiles(baseDirectory + @"\Lang", "*.xml");
            foreach (string filepath in xmlFiles)
            {
                string langDoc = File.ReadAllText(filepath);
                XmlDocument xmlDoc = new XmlDocument();
                xmlDoc.LoadXml(langDoc);
                XmlNode xmlNode = xmlDoc.SelectSingleNode("//LanguageResources");
                string version = xmlNode.Attributes["version"].InnerText;
                string file = Path.GetFileName(filepath);
                UpdateManifestItem updateManifestItem = new UpdateManifestItem("Lang", file, version);
                existingItems.Add(file, updateManifestItem);
            }

            return existingItems;
        }


        protected void PackageLabelUpdate(string labelMsg)
        {
            this.packageProgressStatus.Text = labelMsg;
        }

        private void btnProcessUpdate_Click(object sender, EventArgs e)
        {
            this.btnProcessUpdate.Enabled = false;
            this.btnCheckUpdates.Enabled = false;
            this.updateList.Enabled = false;

            Dictionary<string, UpdateManifestChange> changesToApply = new Dictionary<string, UpdateManifestChange>();
            List<UpdateManifestChange> changeList = new List<UpdateManifestChange>();
            
            foreach (ListViewItem item in this.updateList.Items)
            {
                if (item.Checked)
                {
                    if (!changesToApply.ContainsKey(item.Group.Name)) 
                    {
                        changesToApply.Add(item.Group.Name, applicableChangesDict[item.Group.Name]);
                        changeList.Add(applicableChangesDict[item.Group.Name]);
                    }
                }
            }

            if (changeList.Count == 0)
            {
                MessageBox.Show(guiLang.Translate("NO_APPLICABLE_CHANGES"));
                this.btnCheckUpdates.Enabled = true;
                this.btnProcessUpdate.Enabled = true;
                this.updateList.Enabled = true;
            }
            else
            {
                changeList.Sort();
                //Let's sort this change list from old to newer
                backgroundWorker1.RunWorkerAsync(changeList);
            }
        }

        private void backgroundWorker1_DoWork(object sender, DoWorkEventArgs e)
        {
            this.btnProcessUpdate.Enabled = false;
            List<UpdateManifestChange> changeList = (List<UpdateManifestChange>)e.Argument;

            int changeCount = changeList.Count;

            backgroundWorker1.ReportProgress(0, guiLang.Translate("PROCESSING"));

            for (int i = 0; i < changeList.Count; i++)
            {
                UpdateManifestChange change = changeList[i];
                Thread.Sleep(500);
                backgroundWorker1.ReportProgress(((i + 1) * 100 / changeCount), new string[] { "", change.Id + " - " + (i + 1) + "/" + changeCount });

                int fileCount = change.Items.Count;
                for (int j = 0; j < change.Items.Count; j++)
                {
                    Thread.Sleep(500);
                    UpdateManifestItem item = change.Items[j];
                    backgroundWorker1.ReportProgress(0, null);
                    DownloadFile(change.Id, item.ContentPath, item.ContentFile);
                    backgroundWorker1.ReportProgress((j+1)*100 / fileCount, new string[] { item.ContentPath + @"\" + item.ContentFile, guiLang.Translate("DOWNLOADING") + " - " + (j+1) + "/" + fileCount });
                }

                Thread.Sleep(1000);

            }


            backgroundWorker1.ReportProgress((int)ProgressState.PreCleanup, 5); Thread.Sleep(1000);
            backgroundWorker1.ReportProgress((int)ProgressState.PreCleanup, 4); Thread.Sleep(1000);
            backgroundWorker1.ReportProgress((int)ProgressState.PreCleanup, 3); Thread.Sleep(1000);
            backgroundWorker1.ReportProgress((int)ProgressState.PreCleanup, 2); Thread.Sleep(1000);
            backgroundWorker1.ReportProgress((int)ProgressState.PreCleanup, 1); Thread.Sleep(1000);
            backgroundWorker1.ReportProgress((int)ProgressState.Cleanup, null);

            Thread.Sleep(1000);


            for (int i = 0; i < changeList.Count; i++)
            {
                UpdateManifestChange change = changeList[i];
                Thread.Sleep(500);
                backgroundWorker1.ReportProgress(((i + 1) * 100 / changeCount), new string[] { "", change.Id + " - " + (i + 1) + "/" + changeCount });

                int fileCount = change.Items.Count;

                for (int j = 0; j < change.Items.Count; j++)
                {
                    Thread.Sleep(500);
                    UpdateManifestItem item = change.Items[j];
                    backgroundWorker1.ReportProgress(0, null);
                    CopyFile(change.Id, item.ContentPath, item.ContentFile);
                    backgroundWorker1.ReportProgress((j + 1) * 100 / fileCount, new string[] { item.ContentPath + @"\" + item.ContentFile, guiLang.Translate("COPYING") + " - " + (j + 1) + " / " + fileCount });
                }

                Thread.Sleep(1000);
            }

            backgroundWorker1.ReportProgress((int)ProgressState.UpdateCompleted, null);

        }

        private void CopyFile(string changeId, string filePath, string fileName)
        {
            string sourcePath = String.Format(@"{0}\temp\update\{1}\{2}\{3}", baseDirectory, changeId, filePath, fileName);
            string targetFolder = String.Format(@"{0}\{1}", baseDirectory, filePath);

            if (!Directory.Exists(targetFolder))
            {
                Directory.CreateDirectory(targetFolder);
            }

            string targetPath = String.Format(@"{0}\{1}\{2}", baseDirectory, filePath, fileName);
            File.Copy(sourcePath, targetPath, true);
        }

        private void DownloadFile(string changeId, string path, string fileName)
        {
            string updateBasePath = ConfigurationSettings.AppSettings["UpdateBasePath"];
            DownloadThread dl = new DownloadThread();
            dl.DownloadUrl = updateBasePath + changeId + "/" + path + "/" + fileName;
            dl.CompleteCallback += new DownloadCompleteHandler(DownloadCompleteCallback);
            dl.ProgressCallback += new DownloadProgressHandler(DownloadProgressCallback);
            dl.ErrorCallback += new DownloadErrorHandler(DownloadErrorCallback);
            dl.Proxy = AppUtils.GetProxyIfRequired();

            string[] callbackArgs = new string[] { changeId, path, fileName};
            dl.Download(callbackArgs);
        }

        static void StartDownloadThread(string[] callbackArgs)
        {
        }

        private void backgroundWorker1_ProgressChanged(object sender, ProgressChangedEventArgs e)
        {
            if (e.ProgressPercentage == (int)ProgressState.PreCleanup)
            {
                CleanUpProgress();
                packageProgressStatus.Text = String.Format(guiLang.Translate("ALERT"), (int)e.UserState);
            }
            else if (e.ProgressPercentage == (int)ProgressState.Cleanup)
            {
                CleanUpProgress();
            }
            else if (e.ProgressPercentage == (int)ProgressState.UpdateCompleted)
            {
                MessageBox.Show(guiLang.Translate("SUCCESS_RESULT"));
                parent.RefreshLanguage();
                this.Close();
            }
            else if (e.ProgressPercentage == (int)ProgressState.DownloadError)
            {
                object[] parameters = (object[])e.UserState;
                DownloadErrorCallbackShadow((string)parameters[0]);
            }
            else if (e.ProgressPercentage == (int)ProgressState.DownloadProgress)
            {
                object[] parameters = (object[]) e.UserState;
                DownloadProgressCallbackShadow((int)parameters[0], (int)parameters[1]);
            }
            else if (e.ProgressPercentage == (int)ProgressState.DownloadCompleted)
            {
                object[] parameters = (object[])e.UserState;
                DownloadCompleteCallbackShadow((byte[])parameters[0], (string[])parameters[1]);
            }
            else if (e.UserState == null)
            {
                this.outputGroupBox.Enabled = true;
                this.bytesDownloadedTextBox.Text = "";
                this.totalBytesTextBox.Text = "";
                this.progressBar.Minimum = 0;
                this.progressBar.Maximum = 0;
                this.progressBar.Value = 0;
            }
            else
            {
                int percentage = e.ProgressPercentage;

                if (e.UserState is string) { packageProgressStatus.Text = (string)e.UserState; return; }

                string[] userState = (string[])e.UserState;

                if (userState[0] != String.Empty)
                {
                    fileProgress.Value = percentage;
                    fileProgressStatus.Text = String.Format(userState[1], userState[0]);
                }
                else if (userState[1] != String.Empty)
                {
                    packageProgress.Value = percentage;
                    packageProgressStatus.Text = String.Format(guiLang.Translate("PROCESSING_PACKAGE"), userState[1]);
                }
            }
        }

        private void DownloadErrorCallback(string exceptionMessage)
        {
            backgroundWorker1.ReportProgress((int)ProgressState.DownloadError, new object[] { exceptionMessage});
        }

        private void DownloadErrorCallbackShadow(string exceptionMessage)
        {
            MessageBox.Show(guiLang.Translate("DOWNLOAD_ERROR") + exceptionMessage);

            this.btnCheckUpdates.Enabled = true;
            this.btnProcessUpdate.Enabled = true;
            this.updateList.Enabled = true;
            CleanUpProgress();
        }

        private void CleanUpProgress()
        {
            this.outputGroupBox.Enabled = true;
            this.bytesDownloadedTextBox.Text = "";
            this.totalBytesTextBox.Text = "";
            this.progressBar.Minimum = 0;
            this.progressBar.Maximum = 0;
            this.progressBar.Value = 0;
            this.packageProgress.Value = 0;
            this.fileProgress.Value = 0;
            this.fileProgressStatus.Text = "";
            this.packageProgressStatus.Text = "";
        }

        private void DownloadProgressCallback(int bytesSoFar, int totalBytes)
        {
            backgroundWorker1.ReportProgress((int)ProgressState.DownloadProgress, new object[] { bytesSoFar, totalBytes });
        }

        private void DownloadProgressCallbackShadow(int bytesSoFar, int totalBytes)
        {
            bytesDownloadedTextBox.Text = bytesSoFar.ToString("#,##0");

            if (totalBytes != -1)
            {
                progressBar.Minimum = 0;
                progressBar.Maximum = totalBytes;
                progressBar.Value = bytesSoFar;
                totalBytesTextBox.Text = totalBytes.ToString("#,##0");
            }
            else
            {
                progressBar.Visible = false;
                totalBytesTextBox.Text = guiLang.Translate("SIZE_UNKNOWN");
            }
        }

        private void DownloadCompleteCallback(byte[] dataDownloaded, string[] callbackArgs)
        {
            backgroundWorker1.ReportProgress((int)ProgressState.DownloadCompleted, new object[] {dataDownloaded, callbackArgs });
        }

        private void DownloadCompleteCallbackShadow(byte[] dataDownloaded, string[] callbackArgs)
        {
            if (!progressBar.Visible)
            {
                progressBar.Visible = true;
                progressBar.Minimum = 0;
                progressBar.Value = progressBar.Maximum = 1;
                totalBytesTextBox.Text = bytesDownloadedTextBox.Text;
            }

            //Dosyayı kaydedelim
            string changeId = callbackArgs[0];
            string filePath = callbackArgs[1]; //Kullanmıyoruz
            string fileName = callbackArgs[2];
            if (!Directory.Exists(String.Format(@"{0}\temp\update\{1}", baseDirectory, changeId)))
            {
                Directory.CreateDirectory(String.Format(@"{0}\temp\update\{1}",baseDirectory,changeId));
            }
            if (!Directory.Exists(String.Format(@"{0}\temp\update\{1}\{2}", baseDirectory, changeId, filePath)))
            {
                Directory.CreateDirectory(String.Format(@"{0}\temp\update\{1}\{2}", baseDirectory, changeId, filePath));
            }
            File.WriteAllBytes(String.Format(@"{0}\temp\update\{1}\{2}\{3}", baseDirectory, changeId, filePath, fileName), dataDownloaded);
        }

        private void btnProcessUpdates_Load(object sender, EventArgs e)
        {
           
        }

        private void loadMLResources()
        {
            this.Text = this.guiLang.Translate("FORM_TITLE");
            this.btnCheckUpdates.Text = this.guiLang.Translate("CHECK_UPDATES");
            this.btnProcessUpdate.Text = this.guiLang.Translate("PROCESS_UPDATES");
            this.lblFileProgress.Text = this.guiLang.Translate("FILE_PROGRESS");
            this.lblPackageProgress.Text = this.guiLang.Translate("PACKAGE_PROGRESS");
            this.outputGroupBox.Text = this.guiLang.Translate("DOWNLOAD_STATUS");
            this.lblBytesDownloaded.Text = this.guiLang.Translate("BYTES_DOWNLOADED");
            this.lblTotalBytes.Text = this.guiLang.Translate("TOTAL_BYTES");
            this.lblDownloadProgress.Text = this.guiLang.Translate("DOWNLOAD_PROGRESS");

            this.updateList.Columns[0].Text = this.guiLang.Translate("LIST_ACTION");
            this.updateList.Columns[1].Text = this.guiLang.Translate("LIST_FILENAME");
            this.updateList.Columns[2].Text = this.guiLang.Translate("LIST_EXISTING_VERSION");
            this.updateList.Columns[3].Text = this.guiLang.Translate("LIST_NEW_VERSION");
            this.updateList.Columns[4].Text = this.guiLang.Translate("LIST_STATUS");
            this.updateList.Columns[5].Text = this.guiLang.Translate("LIST_DESCRIPTION");
        }


    }


    public class UpdateManifestChange : IComparable
    {
        private string id;
        private string description;
        private DateTime updateDate;
        private List<UpdateManifestItem> items;

        public UpdateManifestChange(string id, DateTime updateDate, string description)
        {
            this.id = id;
            this.description = description;
            this.updateDate = updateDate;
            items = new List<UpdateManifestItem>();
        }

        public string Id
        {
            get
            {
                return this.id;
            }
        }

        public DateTime UpdateDate
        {
            get
            {
                return this.updateDate;
            }
        }

        public string UpdateDateString
        {
            get
            {
                return this.updateDate.ToShortDateString();
            }
        }

        public string Description
        {
            get
            {
                return this.description;
            }
        }

        public List<UpdateManifestItem> Items
        {
            get 
            {
                return this.items;
            }
        }

        #region IComparable Members

        public int CompareTo(object obj)
        {
            if (obj is UpdateManifestChange)
            {
                return this.updateDate.CompareTo(((UpdateManifestChange)obj).updateDate);
            }
            else
            {
                throw new NotImplementedException("Sorry can't compare apple with oranges!");
            }
        }

        #endregion
    }

    public class UpdateManifestItem
    {
        private string contentPath;
        private string contentFile;
        private string version;
        private string oldVersion;
        private bool needsUpdate;
        private string description;

        public UpdateManifestItem(string contentPath,string contentFile, string version)
        {
            this.contentPath = contentPath;
            this.contentFile = contentFile;
            this.version = version;
        }


        public string ContentPath
        {
            get
            {
                return this.contentPath;
            }
        }

        public string ContentFile
        {
            get
            {
                return this.contentFile;
            }
        }

        public string Version
        {
            get
            {
                return this.version;
            }
        }

        public string OldVersion
        {
            get
            {
                return this.oldVersion;
            }
            set
            {
                this.oldVersion = value;
            }
        }

        public bool NeedsUpdate
        {
            get
            {
                return this.needsUpdate;
            }
            set
            {
                this.needsUpdate = value;
            }
        }

        public string Description
        {
            get
            {
                return this.description;
            }
            set
            {
                this.description = value;
            }
        }


        public bool IsNewer(string otherVersion)
        {
            if (String.IsNullOrEmpty(otherVersion)) return true;
            int otherVersionMajor = 0;
            string otherVersionMinor = String.Empty;
            int versionMajor = 0;
            string versionMinor = String.Empty;

            if (this.version.Contains("."))
            {
                string[] verList = this.version.Split(new char[] { '.' });
                versionMajor = Convert.ToInt32(verList[0]);
                versionMinor = verList[1];
            }

            if (otherVersion.Contains("."))
            {
                string[] verList = otherVersion.Split(new char[] { '.' });
                otherVersionMajor = Convert.ToInt32(verList[0]);
                otherVersionMinor = verList[1];
            }

            if (versionMajor > otherVersionMajor) return true;

            //betas are older than the full versions (1.0 > 1.01b)
            if ((versionMinor.Replace("0", "") == String.Empty) && (otherVersionMinor.Replace("0", "") != String.Empty))
            {
                return true;
            }

            int v1 = Convert.ToInt32(versionMinor.Replace("b", "").PadRight(3, '0'));
            int v2 = Convert.ToInt32(otherVersionMinor.Replace("b", "").PadRight(3, '0'));

            return v1 > v2;
        }

    }
}
