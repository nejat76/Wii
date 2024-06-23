using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Text;
using System.Windows.Forms;
using System.IO;
using System.Net.Sockets;
using System.Threading;

namespace InstallerStub
{
    public partial class Form1 : Form
    {
        public Form1()
        {
            InitializeComponent();
        }

        private void Form1_Load(object sender, EventArgs e)
        {
            txtIosToUse.Text = Info.DefaultIos.ToString();
            txtIpAddress.Text = Info.DefaultIp;
            lblGameName.Text = Info.WadName;
        }

        private void btnInstall_Click(object sender, EventArgs e)
        {
            btnInstall.Enabled = false;
            backgroundWorker1.RunWorkerAsync();
        }


	    private bool sendFile(byte[] fileContent, int fileSize) 
		{
            TcpClient client = new TcpClient();
            NetworkStream stream = null;
            int compressedFileSize;

            try
            {
                compressedFileSize = fileContent.Length;
                int blockSize = 4 * 1024;
                byte[] buffer = new byte[16];
                byte[] argsBuffer = new byte[14];
                string ipAddress = txtIpAddress.Text;
                string[] ipBytes = ipAddress.Split('.');

                backgroundWorker1.ReportProgress(0, "Connecting to " + ipAddress + ":4299...");
                Thread.Sleep(200);
                client.Connect(ipAddress, 4299);
                backgroundWorker1.ReportProgress(0, "Connected to " + ipAddress + ":4299...");
                Thread.Sleep(200);


                //Send Magic
                buffer[0] = 0x48;
                buffer[1] = 0x41;
                buffer[2] = 0x58;
                buffer[3] = 0x58;

                stream = client.GetStream();

                //Send Version Info
                buffer[4] = 0;
                buffer[5] = 5;
                buffer[6] = 0;
                buffer[7] = 14;
                argsBuffer[0] = argsBuffer[1] = argsBuffer[2] = argsBuffer[3] = argsBuffer[4] = argsBuffer[5] = argsBuffer[6] = 0x30;
                argsBuffer[7] = 0x31;
                argsBuffer[8] = 0x2E;
                argsBuffer[9] = 0x64;
                argsBuffer[10] = 0x6F;
                argsBuffer[11] = 0x6C;
                argsBuffer[12] = 0x00;
                argsBuffer[13] = 0x00;

                //Send File Size
                buffer[8] = (byte)((compressedFileSize >> 24) & 0xff);
                buffer[9] = (byte)((compressedFileSize >> 16) & 0xff);
                buffer[10] = (byte)((compressedFileSize >> 8) & 0xff);
                buffer[11] = (byte)(compressedFileSize & 0xff);

                //Send Uncompressed file size
                buffer[12] = (byte)((fileSize >> 24) & 0xff);
                buffer[13] = (byte)((fileSize >> 16) & 0xff);
                buffer[14] = (byte)((fileSize >> 8) & 0xff);
                buffer[15] = (byte)(fileSize & 0xff);

                stream.Write(buffer, 0, 16);

                backgroundWorker1.ReportProgress(0, "Magic, version info & file size sent... sending file");
                Thread.Sleep(200);

                int offset = 0;
                int current = 0;
                int count = (int)(compressedFileSize / blockSize);
                int leftOver = (int)(compressedFileSize % blockSize);

                while (current < count)
                {
                    stream.Write(fileContent, offset, blockSize);
                    offset += blockSize;
                    string report = "Sending file... " + (blockSize * (current + 1)) + " / " + fileSize;
                    backgroundWorker1.ReportProgress(((current + 1) * 100) / count, report);
                    current++;
                }

                if (leftOver > 0)
                {
                    stream.Write(fileContent, offset, compressedFileSize - offset);
                }

                stream.Write(argsBuffer, 0, 14);                
            }
            catch (Exception)
            {
                throw;
            }
            finally 
            {
                if ((client!=null) && (client.Connected))
                {
                    if (stream != null)
                    {
                        stream.Close();
                    }
                    client.Close();
                }
            }

            return true;

        }

        private static MemoryStream LoadCompressedStubInstaller(string installerResourceName)
        {
            //using (BinaryReader resLoader = new BinaryReader(System.Reflection.Assembly.GetExecutingAssembly().GetManifestResourceStream("InstallerStub.Resources." + installerResourceName)))
            using (BinaryReader resLoader = new BinaryReader(System.Reflection.Assembly.GetExecutingAssembly().GetManifestResourceStream(installerResourceName)))
            {
                MemoryStream ms = new MemoryStream();
                byte[] temp = resLoader.ReadBytes((int)resLoader.BaseStream.Length);
                ms.Write(temp, 0, temp.Length);
                return ms;
            }
        }

        private void backgroundWorker1_DoWork(object sender, DoWorkEventArgs e)
        {
            backgroundWorker1.ReportProgress(0, "Loading & decompressing stub installer");
            Thread.Sleep(200);
            MemoryStream ms = LoadCompressedStubInstaller("installer.z");
            backgroundWorker1.ReportProgress(0, "Stub installer loaded, sending to Wii");
            Thread.Sleep(200);

            sendFile(ms.ToArray(), Info.UncompressedSize);
        }

        private void backgroundWorker1_ProgressChanged(object sender, ProgressChangedEventArgs e)
        {
            this.toolStripProgressBar1.Value = e.ProgressPercentage;
            this.toolStripStatusLabel1.Text = (string)e.UserState;
        }

        private void backgroundWorker1_RunWorkerCompleted(object sender, RunWorkerCompletedEventArgs e)
        {
            if (e.Error == null)
            {
                string toolTipText = "Installer successfully sent to HBC";
                this.toolStripStatusLabel1.Text = toolTipText;
                MessageBox.Show(toolTipText);
            }
            else
            {
                this.toolStripStatusLabel1.Text = "Failed!";
                MessageBox.Show("Error encountered : " + e.Error.ToString());
            }

            btnInstall.Enabled = true;
        }
    }
}
