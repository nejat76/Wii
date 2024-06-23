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
using System.Net.Sockets;
using System.IO;
using CrapInstaller;
using Org.Irduco.CommonHelpers;
using System.Threading;

namespace WiiGSC
{
    public partial class Wiiload : Form
    {
    // Fields
    public string appPath;
    public string defaultIp;
    public byte[] dolData;
    public int dolSize;
    public MultiLanguageModuleHelper guiLang;
    public string packedWadPath;

    // Methods
    public Wiiload()
    {
        InitializeComponent();
    }

    private void button1_Click(object sender, EventArgs e)
    {
        button1.Enabled = false;
        button2.Enabled = false;

        toolStripProgressBar1.Value = 0;
        toolStripStatusLabel1.Text = "";

        List<object> work = new List<object>();
        if (this.chkOldHBC.Checked)
        {
            work.Add("TESTCOMPRESSED");
            //this.sendFile(this.dolData, (uint) this.dolSize, 0);
        }
        else
        {
            work.Add("TESTRAW");
            //this.sendFileNewUncompressed(this.dolData, (uint) this.dolSize, 0);
        }

        work.Add(this.dolData);
        work.Add((uint)this.dolSize);
        work.Add((int)0);
        

        backgroundWorker1.RunWorkerAsync(work);

    }

    private void button2_Click(object sender, EventArgs e)
    {
        button1.Enabled = false;
        button2.Enabled = false;

        if (!this.chkDisclaimer.Checked)
        {
            MessageBox.Show(this.guiLang.Translate("DISCLAIMER_TEXT"), this.guiLang.Translate("DISCLAIMER_HEADER"), MessageBoxButtons.OK, MessageBoxIcon.Exclamation);
            button1.Enabled = true;
            button2.Enabled = true;
        }
        else
        {
            int iosToUse = this.getIosToUse();
            if (iosToUse < 0)
            {
                MessageBox.Show(this.guiLang.Translate("IOS_SELECTION_ERROR"), this.guiLang.Translate("ERROR_HEADER"), MessageBoxButtons.OK, MessageBoxIcon.Hand);
                button1.Enabled = true;
                button2.Enabled = true;
            }
            else if (!string.IsNullOrEmpty(this.packedWadPath))
            {
                MessageBox.Show(this.guiLang.Translate("INSTALLING"), this.guiLang.Translate("INFO_HEADER"), MessageBoxButtons.OK, MessageBoxIcon.Asterisk);

                toolStripProgressBar1.Value = 0;
                toolStripStatusLabel1.Text = "";

                List<object> work = new List<object>();
                work.Add("INSTALL");
                work.Add(iosToUse);
                backgroundWorker1.RunWorkerAsync(work);
            }
            else
            {
                MessageBox.Show(this.guiLang.Translate("CANTFINDWAD"), this.guiLang.Translate("ERROR_HEADER"), MessageBoxButtons.OK, MessageBoxIcon.Hand);
                button1.Enabled = true;
                button2.Enabled = true;
            }
        }
    }

    private string Install(int iosToUse)
    {
        //try
        //{
            byte[] loader = null;
            MemoryStream ms = null;
            backgroundWorker1.ReportProgress(0, "Creating Installer dol");
            Thread.Sleep(500);
            ms = InstallerHelper.CreateInstaller(this.packedWadPath, (byte)iosToUse);
            backgroundWorker1.ReportProgress(0, "Installer dol created");
            Thread.Sleep(500);
            uint uncompressedSize = (uint)((int)ms.Length);
            string result = "";
            if (!this.chkOldHBC.Checked)
            {
                backgroundWorker1.ReportProgress(0, "Making use of compression feature of HBC");
                Thread.Sleep(500);
                loader = InstallerHelper.CreateCompressedInstallerUsingZlib(ms);
                backgroundWorker1.ReportProgress(0, "Installer dol compressed, sending to Wii");
                Thread.Sleep(500);
                result = this.sendFileNew2(loader, uncompressedSize, 0);
            }
            else
            {
                loader = ms.ToArray();
                result = this.sendFile(loader, uncompressedSize, 0);
            }
        //}
        //catch (Exception ex)
        //{
        //    //this.lblStatus.Text = "";
        //    //MessageBox.Show(this.guiLang.Translate("INSTALLER_BUILD_ERROR") + ": " + ex.Message, this.guiLang.Translate("ERROR_HEADER"), MessageBoxButtons.OK, MessageBoxIcon.Hand);
        //}

            return result;
    }

    private void button3_Click(object sender, EventArgs e)
    {
        MemoryStream ms = null;
        byte[] loader = null;
        string wadNamePart = null;
        if (!this.chkDisclaimer.Checked)
        {
            MessageBox.Show(this.guiLang.Translate("DISCLAIMER_TEXT"), this.guiLang.Translate("DISCLAIMER_HEADER"), MessageBoxButtons.OK, MessageBoxIcon.Exclamation);
        }
        else
        {
            int iosToUse = this.getIosToUse();
            if (iosToUse < 0)
            {
                MessageBox.Show(this.guiLang.Translate("IOS_SELECTION_ERROR"), this.guiLang.Translate("ERROR_HEADER"), MessageBoxButtons.OK, MessageBoxIcon.Hand);
            }
            else if (!string.IsNullOrEmpty(this.packedWadPath))
            {                
                saveFileDialog1.DefaultExt = "exe";
                saveFileDialog1.Filter = this.guiLang.Translate("EXE_FILTER");
                DialogResult result = saveFileDialog1.ShowDialog();
                if (result == DialogResult.OK)
                {

                    try
                    {
                        ms = InstallerHelper.CreateInstaller(this.packedWadPath, (byte)iosToUse);
                        uint uncompressedSize = (uint)((int)ms.Length);
                        if (!this.chkOldHBC.Checked)
                        {
                            loader = InstallerHelper.CreateCompressedInstallerUsingZlib(ms);
                        }
                        else
                        {
                            loader = ms.ToArray();
                        }
                        wadNamePart = Path.GetFileName(this.packedWadPath);
                        //exeName = InstallerHelper.CreateSelfInstaller(this.appPath, this.txtIpAddress.Text, wadNamePart, loader, (int)uncompressedSize, iosToUse);
                        string exeName = saveFileDialog1.FileName;
                        InstallerHelper.CreateSelfInstallerWithName(this.appPath, this.txtIpAddress.Text, wadNamePart, loader, (int)uncompressedSize, iosToUse, exeName);                       
                        MessageBox.Show(string.Format(this.guiLang.Translate("INSTALLER_EXE_CREATED"), exeName), this.guiLang.Translate("INFO_HEADER"), MessageBoxButtons.OK, MessageBoxIcon.Asterisk);
                    }
                    catch (Exception ex)
                    {
                        toolStripStatusLabel1.Text = "";
                        MessageBox.Show(this.guiLang.Translate("INSTALLER_BUILD_ERROR") + ": " + ex.Message, this.guiLang.Translate("ERROR_HEADER"), MessageBoxButtons.OK, MessageBoxIcon.Hand);
                    }
                }
            }
            else
            {
                MessageBox.Show(this.guiLang.Translate("CANTFINDWAD"), this.guiLang.Translate("ERROR_HEADER"), MessageBoxButtons.OK, MessageBoxIcon.Hand);
            }
        }
    }

    private void button4_Click(object sender, EventArgs e)
    {
        string dolName = null;
        MemoryStream ms = null;
        byte[] loader = null;
        if (!this.chkDisclaimer.Checked)
        {
            MessageBox.Show(this.guiLang.Translate("DISCLAIMER_TEXT"), this.guiLang.Translate("DISCLAIMER_HEADER"), MessageBoxButtons.OK, MessageBoxIcon.Exclamation);
        }
        else
        {
            int iosToUse = this.getIosToUse();
            if (iosToUse < 0)
            {
                MessageBox.Show(this.guiLang.Translate("IOS_SELECTION_ERROR"), this.guiLang.Translate("ERROR_HEADER"), MessageBoxButtons.OK, MessageBoxIcon.Hand);
            }
            else if (!string.IsNullOrEmpty(this.packedWadPath))
            {
                saveFileDialog1.DefaultExt = "dol";
                saveFileDialog1.Filter = this.guiLang.Translate("DOL_FILTER");
                DialogResult result = saveFileDialog1.ShowDialog();
                if (result == DialogResult.OK)
                {
                    try
                    {
                        ms = InstallerHelper.CreateInstaller(this.packedWadPath, (byte)iosToUse);
                        uint uncompressedSize = (uint)((int)ms.Length);
                        loader = ms.ToArray();
                        //dolName = this.packedWadPath.Replace(".wad", ".dol");
                        dolName = saveFileDialog1.FileName;
                        File.WriteAllBytes(dolName, loader);
                        MessageBox.Show(string.Format(this.guiLang.Translate("INSTALLER_DOL_CREATED"), dolName), this.guiLang.Translate("INFO_HEADER"), MessageBoxButtons.OK, MessageBoxIcon.Asterisk);
                    }
                    catch (Exception ex)
                    {
                        toolStripStatusLabel1.Text = "";
                        MessageBox.Show(this.guiLang.Translate("INSTALLER_BUILD_ERROR") + ": " + ex.Message, this.guiLang.Translate("ERROR_HEADER"), MessageBoxButtons.OK, MessageBoxIcon.Hand);
                    }
                }
            }
            else
            {
                MessageBox.Show(this.guiLang.Translate("CANTFINDWAD"), this.guiLang.Translate("ERROR_HEADER"), MessageBoxButtons.OK, MessageBoxIcon.Hand);
            }
        }
    }

    private int getIosToUse()
    {
        int iosToUse;
        if (int.TryParse(this.txtIOS.Text, out iosToUse))
        {
            if ((iosToUse >= 2) && (iosToUse <= 0xff))
            {
                return iosToUse;
            }
            return -1;
        }
        return -1;
    }

    private void loadMLResources()
    {
        this.label1.Text = this.guiLang.Translate("IPLABEL") + " :";
        this.button1.Text = this.guiLang.Translate("TESTBUTTONTEXT");
        this.button2.Text = this.guiLang.Translate("INSTALLBUTTONTEXT");
        this.chkDisclaimer.Text = this.guiLang.Translate("CHKDISCLAIMERTEXT");
        this.label2.Text = this.guiLang.Translate("REMINDER");
        this.label3.Text = this.guiLang.Translate("LASTPACKED") + ": ";
        this.chkOldHBC.Text = this.guiLang.Translate("OLD_HBC");
        this.label8.Text = this.guiLang.Translate("IOS_FOR_INSTALLATION") + " :";
        this.button3.Text = this.guiLang.Translate("CREATE_INSTALLER_EXE");
        this.label5.Text = this.guiLang.Translate("CREATE_INSTALLER_EXE_INFO");
        this.label6.Text = this.guiLang.Translate("INSTALL_INFO");
        this.button4.Text = this.guiLang.Translate("CREATE_INSTALLER_DOL");
        this.label7.Text = this.guiLang.Translate("CREATE_INSTALLER_DOL_INFO");
    }

    /// <remarks>Converting to worker</remarks>
    private string sendFile(byte[] fileContent, uint fileSize, int stripCount)
    {
        byte[] buffer = null;
        TcpClient client = null;
        NetworkStream stream = null;
        string ipAddress = null;
        bool flag;
        string[] ipBytes = null;
        try
        {
            int blockSize = 0x4000;
            buffer = new byte[12];
            ipAddress = this.txtIpAddress.Text;
            char[] separator = new char[] { '.' };
            ipBytes = ipAddress.Split(separator);
            backgroundWorker1.ReportProgress(25, "Connecting to " + ipAddress + ":4299...");
            Thread.Sleep(100);
            client = new TcpClient();
            client.Connect(ipAddress, 0x10cb);
            backgroundWorker1.ReportProgress(50, "Connected to " + ipAddress + ":4299");
            Thread.Sleep(100);
            buffer[0] = 0x48;
            buffer[1] = 0x41;
            buffer[2] = 0x58;
            buffer[3] = 0x58;
            stream = client.GetStream();
            buffer[4] = 0x30;
            buffer[5] = 0x31;
            buffer[6] = 0;
            buffer[7] = 0;
            buffer[8] = (byte)((fileSize >> 0x18) & 0xff);
            buffer[9] = (byte)((fileSize >> 0x10) & 0xff);
            buffer[10] = (byte)((fileSize >> 8) & 0xff);
            buffer[11] = (byte)(fileSize & 0xff);
            stream.Write(buffer, 0, 12);

            backgroundWorker1.ReportProgress(99, "Magic, version info & file size sent... sending file");
            Thread.Sleep(100);

            int offset = 0;
            int current = 0;
            int count = (int)(fileSize / blockSize);
            int leftOver = (int)(fileSize % blockSize);
            while (true)
            {
                if (current >= count)
                {
                    break;
                }
                stream.Write(fileContent, offset + stripCount, blockSize);
                offset += blockSize;
                string report = "Sending file... " + (blockSize * (current+1)) + " / " + fileSize;
                backgroundWorker1.ReportProgress(((current + 1) * 100) / count, report);
                //this.lblStatus.Text = ("Sending file..." + ((current + 1)).ToString()) + " / " + count;
                current++;
            }
            if (leftOver > 0)
            {
                stream.Write(fileContent, offset + stripCount, ((int)fileSize) - offset);
                string report = "Sending file... " + fileSize + " / " + fileSize;
                backgroundWorker1.ReportProgress(100, report);
                Thread.Sleep(100);
            }

            flag = true;
        }
        catch (Exception ex)
        {
            flag = false;
            throw;
        }
        finally
        {
            if ((client != null) && client.Connected)
            {
                stream.Close();
                client.Close();
            }
        }

        return flag ? "FINISHED_SENDING" : "";
    }

    //private bool sendFileNew(byte[] fileContent, uint fileSize, int stripCount)
    //{
    //    byte[] buffer = null;
    //    byte[] argsBuffer = null;
    //    NetworkStream stream = null;
    //    TcpClient client = null;
    //    string ipAddress = null;
    //    byte[] compressedFileContent = null;
    //    bool flag;
    //    string[] ipBytes = null;
    //    MemoryStream ms = null;
    //    try
    //    {
    //        bool flag2;
    //        try
    //        {
    //            ms = new MemoryStream();
    //            try
    //            {
    //                compressedFileContent = ZlibHelper.Compress(fileContent);
    //            }
    //            catch (Exception ex)
    //            {
    //                MessageBox.Show(string.Format("Can't compress! Result from zlib {0}: ", ex.Message), "Error", MessageBoxButtons.OK, MessageBoxIcon.Hand);
    //                return false;
    //            }
    //            uint compressedFileSize = (uint) compressedFileContent.Length;
    //            int blockSize = 0x1000;
    //            buffer = new byte[0x10];
    //            argsBuffer = new byte[14];
    //            ipAddress = this.txtIpAddress.Text;
    //            char[] separator = new char[] { '.' };
    //            ipBytes = ipAddress.Split(separator);
    //            this.lblStatus.Text = "Connecting to " + ipAddress + ":4299...";
    //            client = new TcpClient();
    //            client.Connect(ipAddress, 0x10cb);
    //            this.lblStatus.Text = "Connected to " + ipAddress + ":4299";
    //            buffer[0] = 0x48;
    //            buffer[1] = 0x41;
    //            buffer[2] = 0x58;
    //            buffer[3] = 0x58;
    //            stream = client.GetStream();
    //            this.lblStatus.Text = "Magic sent...";
    //            buffer[4] = 0;
    //            buffer[5] = 5;
    //            buffer[6] = 0;
    //            buffer[7] = 14;
    //            argsBuffer[6] = 0x30;
    //            argsBuffer[5] = 0x30;
    //            argsBuffer[4] = 0x30;
    //            argsBuffer[3] = 0x30;
    //            argsBuffer[2] = 0x30;
    //            argsBuffer[1] = 0x30;
    //            argsBuffer[0] = 0x30;
    //            argsBuffer[7] = 0x31;
    //            argsBuffer[8] = 0x2e;
    //            argsBuffer[9] = 100;
    //            argsBuffer[10] = 0x6f;
    //            argsBuffer[11] = 0x6c;
    //            argsBuffer[12] = 0;
    //            argsBuffer[13] = 0;
    //            this.lblStatus.Text = "Sent version info...";
    //            buffer[8] = (byte) ((compressedFileSize >> 0x18) & 0xff);
    //            buffer[9] = (byte) ((compressedFileSize >> 0x10) & 0xff);
    //            buffer[10] = (byte) ((compressedFileSize >> 8) & 0xff);
    //            buffer[11] = (byte) (compressedFileSize & 0xff);
    //            buffer[12] = (byte) ((fileSize >> 0x18) & 0xff);
    //            buffer[13] = (byte) ((fileSize >> 0x10) & 0xff);
    //            buffer[14] = (byte) ((fileSize >> 8) & 0xff);
    //            buffer[15] = (byte) (fileSize & 0xff);
    //            stream.Write(buffer, 0, 0x10);
    //            this.lblStatus.Text = "Sending file...";
    //            int offset = 0;
    //            int current = 0;
    //            int count = (int) (compressedFileSize / blockSize);
    //            int leftOver = (int) (compressedFileSize % blockSize);
    //            while (true)
    //            {
    //                if (current >= count)
    //                {
    //                    break;
    //                }
    //                stream.Write(compressedFileContent, offset + stripCount, blockSize);
    //                offset += blockSize;
    //                this.lblStatus.Text = ("Sending file..." + ((current + 1)).ToString()) + " / " + count;
    //                current++;
    //            }
    //            if (leftOver > 0)
    //            {
    //                stream.Write(compressedFileContent, offset + stripCount, ((int) compressedFileSize) - offset);
    //            }
    //            stream.Write(argsBuffer, 0, 14);
    //            this.lblStatus.Text = "Finished Sending!";
    //            return true;
    //        }
    //        catch (Exception ex)
    //        {
    //            MessageBox.Show("Daglar ooy oy, yollar ooy oy....! : " + ex.Message, "Error", MessageBoxButtons.OK, MessageBoxIcon.Hand);
    //            return false;
    //        }
    //    }
    //    finally
    //    {
    //        if ((client != null) && client.Connected)
    //        {
    //            stream.Close();
    //            client.Close();
    //        }
    //    }
    //}

    private string sendFileNew2(byte[] fileContent, uint fileSize, int stripCount)
    {
        byte[] buffer = null;
        byte[] argsBuffer = null;
        NetworkStream stream = null;
        TcpClient client = null;
        string ipAddress = null;
        bool flag;
        string[] ipBytes = null;
        try
        {
            uint compressedFileSize = (uint) fileContent.Length;
            int blockSize = 0x2000;
            buffer = new byte[0x10];
            argsBuffer = new byte[14];
            ipAddress = this.txtIpAddress.Text;
            char[] separator = new char[] { '.' };
            ipBytes = ipAddress.Split(separator);
            backgroundWorker1.ReportProgress(25, "Connecting to " + ipAddress + ":4299...");
            Thread.Sleep(100);
            client = new TcpClient();
            client.Connect(ipAddress, 0x10cb);
            backgroundWorker1.ReportProgress(50, "Connected to " + ipAddress + ":4299");
            Thread.Sleep(100);
            buffer[0] = 0x48;
            buffer[1] = 0x41;
            buffer[2] = 0x58;
            buffer[3] = 0x58;
            stream = client.GetStream();
            buffer[4] = 0;
            buffer[5] = 5;
            buffer[6] = 0;
            buffer[7] = 14;
            argsBuffer[6] = 0x30;
            argsBuffer[5] = 0x30;
            argsBuffer[4] = 0x30;
            argsBuffer[3] = 0x30;
            argsBuffer[2] = 0x30;
            argsBuffer[1] = 0x30;
            argsBuffer[0] = 0x30;
            argsBuffer[7] = 0x31;
            argsBuffer[8] = 0x2e;
            argsBuffer[9] = 100;
            argsBuffer[10] = 0x6f;
            argsBuffer[11] = 0x6c;
            argsBuffer[12] = 0;
            argsBuffer[13] = 0;
            buffer[8] = (byte) ((compressedFileSize >> 0x18) & 0xff);
            buffer[9] = (byte) ((compressedFileSize >> 0x10) & 0xff);
            buffer[10] = (byte) ((compressedFileSize >> 8) & 0xff);
            buffer[11] = (byte) (compressedFileSize & 0xff);
            buffer[12] = (byte) ((fileSize >> 0x18) & 0xff);
            buffer[13] = (byte) ((fileSize >> 0x10) & 0xff);
            buffer[14] = (byte) ((fileSize >> 8) & 0xff);
            buffer[15] = (byte) (fileSize & 0xff);
            stream.Write(buffer, 0, 0x10);

            backgroundWorker1.ReportProgress(99, "Magic, version info & file size sent... sending file");
            Thread.Sleep(100);

            int offset = 0;
            int current = 0;
            int count = (int) (compressedFileSize / blockSize);
            int leftOver = (int) (compressedFileSize % blockSize);
            while (true)
            {
                if (current >= count)
                {
                    break;
                }
                stream.Write(fileContent, offset + stripCount, blockSize);
                offset += blockSize;

                string report = "Sending file..." + (blockSize * (current + 1)) + " / " + compressedFileSize; ;
                backgroundWorker1.ReportProgress(((current + 1) * 100) / count, report);

                current++;
            }
            if (leftOver > 0)
            {
                stream.Write(fileContent, offset + stripCount, ((int) compressedFileSize) - offset);
                string report = "Sending file..." + compressedFileSize + " / " + compressedFileSize; ;
                backgroundWorker1.ReportProgress(100, report);
                Thread.Sleep(100);
            }
            stream.Write(argsBuffer, 0, 14);
        }
        finally
        {
            if ((client != null) && client.Connected)
            {
                stream.Close();
                client.Close();
            }
        }
        return "FINISHED_SENDING";
    }

    //Converting to worker
    private string sendFileNewUncompressed(byte[] fileContent, uint fileSize, int stripCount)
    {
        byte[] buffer = null;
        byte[] argsBuffer = null;
        NetworkStream stream = null;
        TcpClient client = null;
        string ipAddress = null;

        string[] ipBytes = null;
        try
        {
            int blockSize = 0x1000;
            buffer = new byte[0x10];
            argsBuffer = new byte[14];
            ipAddress = this.txtIpAddress.Text;
            char[] separator = new char[] { '.' };
            ipBytes = ipAddress.Split(separator);
            //this.lblStatus.Text = "Connecting to " + ipAddress + ":4299...";
            backgroundWorker1.ReportProgress(25, "Connecting to " + ipAddress + ":4299...");
            Thread.Sleep(100);
            client = new TcpClient();
            client.Connect(ipAddress, 0x10cb);
            //this.lblStatus.Text = "Connected to " + ipAddress + ":4299";
            backgroundWorker1.ReportProgress(50, "Connected to " + ipAddress + ":4299");
            Thread.Sleep(100);

            buffer[0] = 0x48;
            buffer[1] = 0x41;
            buffer[2] = 0x58;
            buffer[3] = 0x58;
            stream = client.GetStream();
            buffer[4] = 0;
            buffer[5] = 5;
            buffer[6] = 0;
            buffer[7] = 14;
            argsBuffer[6] = 0x30;
            argsBuffer[5] = 0x30;
            argsBuffer[4] = 0x30;
            argsBuffer[3] = 0x30;
            argsBuffer[2] = 0x30;
            argsBuffer[1] = 0x30;
            argsBuffer[0] = 0x30;
            argsBuffer[7] = 0x31;
            argsBuffer[8] = 0x2e;
            argsBuffer[9] = 100;
            argsBuffer[10] = 0x6f;
            argsBuffer[11] = 0x6c;
            argsBuffer[12] = 0;
            argsBuffer[13] = 0;
            buffer[8] = (byte) ((fileSize >> 0x18) & 0xff);
            buffer[9] = (byte) ((fileSize >> 0x10) & 0xff);
            buffer[10] = (byte) ((fileSize >> 8) & 0xff);
            buffer[11] = (byte) (fileSize & 0xff);
            buffer[12] = 0;
            buffer[13] = 0;
            buffer[14] = 0;
            buffer[15] = 0;
            stream.Write(buffer, 0, 0x10);
            backgroundWorker1.ReportProgress(99, "Magic, version info & file size sent... sending file");
            Thread.Sleep(100);

            int offset = 0;
            int current = 0;
            int count = (int) (fileSize / blockSize);
            int leftOver = (int) (fileSize % blockSize);
            while (true)
            {
                if (current >= count)
                {
                    break;
                }
                stream.Write(fileContent, offset + stripCount, blockSize);
                offset += blockSize;
                string report = "Sending file..." + (blockSize * (current+1)) + " / " + fileSize;;
                backgroundWorker1.ReportProgress(((current + 1) * 100) / count, report);
                current++;
            }
            if (leftOver > 0)
            {
                stream.Write(fileContent, offset + stripCount, ((int) fileSize) - offset);
                string report = "Sending file..." + fileSize + " / " + fileSize; ;
                backgroundWorker1.ReportProgress(100, report);
                Thread.Sleep(100);
            }
            stream.Write(argsBuffer, 0, 14);
        }
        finally
        {
            if ((client != null) && client.Connected)
            {
                stream.Close();
                client.Close();
            }
        }
        return "FINISHED_SENDING";
    }

    private void backgroundWorker1_DoWork(object sender, DoWorkEventArgs e)
    {
        List<object> arguments = (List<object>)e.Argument;
        string workType = (string)arguments[0];

        List<string> result = new List<string>();
        result.Add(workType);

        switch (workType)
        {
            case "TESTCOMPRESSED": result.Add(this.sendFile((byte[])arguments[1], (uint) arguments[2], (int) arguments[3])); break;
            case "TESTRAW": result.Add(sendFileNewUncompressed((byte[])arguments[1], (uint)arguments[2], (int)arguments[3])); break;
            case "INSTALL": result.Add(Install((int)arguments[1])); break;
            //case "EXECREATE": int z = 1; break;
            //case "DOLCREATE": int w = 1; break;
        }

        e.Result = result;
    }

    private void backgroundWorker1_ProgressChanged(object sender, ProgressChangedEventArgs e)
    {
        int percentage = e.ProgressPercentage;
        string text = (string)e.UserState;

        toolStripProgressBar1.Value = percentage;
        toolStripStatusLabel1.Text = text;

        //if (percentage == 0)
        //{
        //    //Failure case
        //    //Duruma göre değişebilir... bakalım.
        //    MessageBox.Show(this.guiLang.Translate("ERROR_SENDING") + " :" + text, this.guiLang.Translate("ERROR_HEADER"), MessageBoxButtons.OK, MessageBoxIcon.Hand);
        //}
        //else if (percentage == 100)
        //{
        //    //Success message
        //    MessageBox.Show(this.guiLang.Translate(text), "", MessageBoxButtons.OK, MessageBoxIcon.Information);
        //    this.guiLang.Translate("FINISHED_SENDING");
        //}
        //else
        //{
        //    lblStatus.Text = text;
        //} 
    }


    private void Wiiload_Load(object sender, EventArgs e)
    {
        this.txtIpAddress.Text = this.defaultIp;
        this.lblLastWad.Text = this.packedWadPath;
        this.loadMLResources();
    }

    private void backgroundWorker1_RunWorkerCompleted(object sender, RunWorkerCompletedEventArgs e)
    {
        if (e.Error != null)
        {
            //if ((source == "TESTCOMPRESSED") || (source == "TESTRAW"))
            //{
                MessageBox.Show(e.Error.Message, this.guiLang.Translate("ERROR_HEADER"), MessageBoxButtons.OK, MessageBoxIcon.Hand);
                toolStripProgressBar1.Value = 0;
                toolStripStatusLabel1.Text = "";
            //}
        }
        else
        {
            List<string> result = (List<string>)e.Result;
            string source = result[0];
            string operationResult = result[1];
            toolStripStatusLabel1.Text = this.guiLang.Translate(operationResult);
            MessageBox.Show(this.guiLang.Translate(operationResult), "", MessageBoxButtons.OK, MessageBoxIcon.Information);
        }

        button1.Enabled = true;
        button2.Enabled = true;
    }

    }
}
