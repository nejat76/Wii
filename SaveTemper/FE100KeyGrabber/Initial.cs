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
using System.IO;
using System.Net;
using System.Text.RegularExpressions;
using TCPListener;
using System.Threading;

namespace FE100KeyGrabber
{
    public partial class Initial : Form
    {
        Thread thread;
        Thread threadWiiLoad;
        delegate void SetStatusCallback(String text, int stat);

        delegate void SetKeyCallback(KeyType keyType, String keyValue, int part);

        public enum KeyType
        {
            None,
            NGMac,
            NGId,
            NGKeyId,
            NGPriv,
            NGSig
        }

        public Initial()
        {
            InitializeComponent();
        }

        private enum FillMode
        {
            Clear = 1,
            Effect = 2
        }

        private void fillTextAreaWithBytes(byte[] content, int length, TextBox txtBox)
        {
            StringBuilder sb = new StringBuilder();
            sb.Append(String.Format("{0:X}", content[0]).PadLeft(2,'0'));
            for (int i = 1; i < length; i++) 
            {
                sb.Append(" ");
                sb.Append(String.Format("{0:X}", content[i]).PadLeft(2, '0'));
            }

            txtBox.Text = sb.ToString();
        }

        private void fillTextAreaWithBytes(FillMode mode, TextBox txtBox)
        {
            if (mode == FillMode.Clear)
            {
                txtBox.Text = "";
            }
        }

        private void Initial_Load(object sender, EventArgs e)
        {

            GetLocalIPAddress();
            string currentFolder = AppDomain.CurrentDomain.BaseDirectory;

            try
            {
                byte[] sdkey = File.ReadAllBytes(currentFolder + "\\shared\\sd-key");
                fillTextAreaWithBytes(sdkey, 16, txtSdKey);
            }
            catch (Exception ex)
            {
                fillTextAreaWithBytes(FillMode.Clear, txtSdKey);
            }

            try
            {
                byte[] sdiv = File.ReadAllBytes(currentFolder + "\\shared\\sd-iv");
                fillTextAreaWithBytes(sdiv, 16, txtSdIv);
            }
            catch (Exception ex)
            {
                fillTextAreaWithBytes(FillMode.Clear, txtSdIv);
            }

            try
            {
                byte[] md5Blanker = File.ReadAllBytes(currentFolder + "\\shared\\md5-blanker");
                fillTextAreaWithBytes(md5Blanker, 16, txtMd5Blanker);
            }
            catch (Exception ex)
            {
                fillTextAreaWithBytes(FillMode.Clear, txtMd5Blanker);
            }

            try
            {
                byte[] ngId = File.ReadAllBytes(currentFolder + "\\private\\NG-id");
                fillTextAreaWithBytes(ngId, 4, txtNgId);
            }
            catch (Exception ex)
            {
                fillTextAreaWithBytes(FillMode.Clear, txtNgId);
            }

            try
            {
                byte[] ngKeyId = File.ReadAllBytes(currentFolder + "\\private\\NG-key-id");
                fillTextAreaWithBytes(ngKeyId, 4, txtNGKeyId);
            }
            catch (Exception ex)
            {
                fillTextAreaWithBytes(FillMode.Clear, txtNGKeyId);
            }

            try
            {
                byte[] ngMac = File.ReadAllBytes(currentFolder + "\\private\\NG-mac");
                fillTextAreaWithBytes(ngMac, 6, txtNGMac);
            }
            catch (Exception ex)
            {
                fillTextAreaWithBytes(FillMode.Clear, txtNGMac);
            }

            try
            {
                byte[] ngPriv = File.ReadAllBytes(currentFolder + "\\private\\NG-priv");
                fillTextAreaWithBytes(ngPriv, 30, txtNGPriv);
            }
            catch (Exception ex)
            {
                fillTextAreaWithBytes(FillMode.Clear, txtNGPriv);
            }

            try
            {
                byte[] ngSig = File.ReadAllBytes(currentFolder + "\\private\\NG-sig");
                fillTextAreaWithBytes(ngSig, 60, txtNGSig);
            }
            catch (Exception ex)
            {
                fillTextAreaWithBytes(FillMode.Clear, txtNGSig);
            }



        }

        private string formatHexString(string hexString)
        {
            string hexStringUpper = hexString.ToUpper();
            StringBuilder sb = new StringBuilder();
            for (int i = 0; i < hexString.Length / 2; i++)
            {
                sb.Append(hexStringUpper.Substring(i * 2, 2) + " ");
            }

            return sb.ToString().TrimEnd();
        }

        private void button3_Click(object sender, EventArgs e)
        {
            WebRequest request;
            StreamReader reader = null;
            try
            {
                MessageBox.Show("Connecting to get the keys...");
                request = HttpWebRequest.Create("http://hackmii.com/2008/04/keys-keys-keys/");
                //request = HttpWebRequest.Create("http://wiicrazy.tepetaklak.com/index.php/2009/06/10/fe100-022-with-updated-keygrabber/");
            
                WebResponse response = request.GetResponse();
                reader = new StreamReader(response.GetResponseStream());                
            }
            catch (Exception ex)
            {
                MessageBox.Show("Sorry... error : " + ex.Message);
                return;
            }

            string responseString = reader.ReadToEnd();
            request = null;
            Match m = Regex.Match(responseString, "SD key .([0-9a-fA-F]{32}).",RegexOptions.RightToLeft);
            string sdKey;
            if (m.Success)
            {
                sdKey = m.Groups[1].Captures[0].Value;
                txtSdKey.Text = formatHexString(sdKey);
            }
            else
            {
                MessageBox.Show("Not at this time");
                return;
            }

            m = Regex.Match(responseString, "SD IV .([0-9a-fA-F]{32}).", RegexOptions.RightToLeft);
            string sdIv;
            if (m.Success)
            {
                sdIv = m.Groups[1].Captures[0].Value;
                txtSdIv.Text = formatHexString(sdIv);
            }
            else
            {
                MessageBox.Show("Not at this time");
                return;
            }

            m = Regex.Match(responseString, "MD5 blanker .([0-9a-fA-F]{32}).", RegexOptions.RightToLeft);
            string md5Blanker;
            if (m.Success)
            {
                md5Blanker = m.Groups[1].Captures[0].Value;
                txtMd5Blanker.Text = formatHexString(md5Blanker);
            }
            else
            {
                MessageBox.Show("Not at this time");
                return;
            }

        }

        private string byteArrayToHexString(byte[] bytes)
        {            
            StringBuilder sb = new StringBuilder();
            for (int i = 0; i < bytes.Length; i++)
            {
                sb.Append(String.Format("{0:X}",bytes[i]).PadLeft(2,'0') + " ");
            }

            return sb.ToString().TrimEnd();
        }

        private void button1_Click(object sender, EventArgs e)
        {
            MessageBox.Show("Now give me a savefile (data.bin) that you got from your wii");
            OpenFileDialog fileDlg = new OpenFileDialog();
            fileDlg.Title = "Save file";
            fileDlg.DefaultExt = "bin";
            DialogResult result = fileDlg.ShowDialog();

            if (result == DialogResult.OK)
            {
                try
                {
                    byte[] macAddress = new byte[6];
                    FileStream fs = File.OpenRead(fileDlg.FileName);
                    fs.Seek(0xF128, SeekOrigin.Begin);
                    fs.Read(macAddress, 0, 6);
                    txtNGMac.Text = byteArrayToHexString(macAddress);

                    fs.Seek(-768, SeekOrigin.End);

                    byte[] certificate = new byte[640];
                    fs.Read(certificate, 0, certificate.Length);
                    byte[] ngId = new byte[8];
                    Array.Copy(certificate, 198, ngId, 0, 8);
                    String ngIdStr = Encoding.ASCII.GetString(ngId);

                    txtNgId.Text = formatHexString(ngIdStr);

                    byte[] ngKeyId = new byte[4];
                    Array.Copy(certificate, 260, ngKeyId, 0, 4);
                    String ngKeyIdStr = Encoding.ASCII.GetString(ngKeyId);

                    fillTextAreaWithBytes(ngKeyId, 4, txtNGKeyId);

                    byte[] ngSig = new byte[60];
                    Array.Copy(certificate, 4, ngSig, 0, 60);
                    fillTextAreaWithBytes(ngSig, 60, txtNGSig);

                    fs.Close();

                } catch(Exception ex)
                {
                    MessageBox.Show("Arghhh : " + ex.Message);
                    return;
                }
            }
        }

        private void button2_Click(object sender, EventArgs e)
        {

            MessageBox.Show("Now finally, hand me your keys.txt you got by using xyzzy on your wii, so I can extract your wii's private ecc key... alternatively you can point your nand dump (nand.bin) obtained with bootmii.");
            OpenFileDialog fileDlg = new OpenFileDialog();
            fileDlg.Title = "Your key dump or nand.bin file..";
            fileDlg.Filter = "Xyzzy key dump (keys.txt)|*.txt|bootmii nand dump (nand.bin)|*.bin";
            DialogResult result = fileDlg.ShowDialog();

            if (result == DialogResult.OK)
            {
                StreamReader reader = null;
                try
                {
                    if (Path.GetExtension(fileDlg.FileName) == ".bin")
                    {
                        FileStream fs = new FileStream(fileDlg.FileName, FileMode.Open);
                        if (fs.Length == 0x21000400)
                        {
                            byte[] ngPriv = new byte[30];
                            fs.Seek(0x21000128, SeekOrigin.Begin);
                            fs.Read(ngPriv, 0, 30);
                            txtNGPriv.Text = byteArrayToHexString(ngPriv);
                        }
                        else
                        {
                            MessageBox.Show("Doesn't seem to be nand dump produced by bootmii, a correct nand dump should be 528MB (553,649,152 bytes)");
                            return;
                        }
                    }
                    else
                    {
                        FileStream fs = File.OpenRead(fileDlg.FileName);
                        reader = new StreamReader(fs);
                        string responseString = reader.ReadToEnd();

                        Match m = Regex.Match(responseString, "ECC Priv Key:\\s*([0-9a-fA-F]{4} [0-9a-fA-F]{4} [0-9a-fA-F]{4} [0-9a-fA-F]{4} [0-9a-fA-F]{4} [0-9a-fA-F]{4}\\s*[0-9a-fA-F]{4}\\s*[0-9a-fA-F]{4}\\s*[0-9a-fA-F]{4}\\s*[0-9a-fA-F]{4}\\s*[0-9a-fA-F]{4}\\s*[0-9a-fA-F]{4}\\s*[0-9a-fA-F]{4}\\s*[0-9a-fA-F]{4}\\s*[0-9a-fA-F]{4}).", RegexOptions.RightToLeft);
                        fs.Close();
                        string eccKey;
                        if (m.Success)
                        {
                            eccKey = m.Groups[1].Captures[0].Value;
                            txtNGPriv.Text = formatHexString(eccKey.Replace('\t', ' ').Replace('\n', ' ').Replace('\r', ' ').Replace(" ", ""));
                        }
                        else
                        {
                            MessageBox.Show("Not at this time");
                            return;
                        }
                    }
                }
                catch (Exception ex)
                {
                    MessageBox.Show("Arghhh : " + ex.Message);
                    return;
                }
            }



        }

        private byte getByteFromHexString(string hexString)
        {
            return Convert.ToByte(hexString, 16);
        }

        private bool getBytesFromTextArea(byte[] bytes, TextBox txtBox, string field)
        {
            string hexString = txtBox.Text.Replace(" ","");
            if (hexString.Length != bytes.Length * 2)
            {
                MessageBox.Show("There is fewer or more hex characters in the field " + field + " not writing out " + field + " file");
                return false;
            }

            for (int i = 0; i < bytes.Length; i++)
            {
                bytes[i] = getByteFromHexString(hexString.Substring(i*2,2));
            }

            return true;
        }

        private void button4_Click(object sender, EventArgs e)
        {
            int count = 0;
            string currentFolder = AppDomain.CurrentDomain.BaseDirectory;

            string currentKey;
            currentKey = "sd-key";
            try
            {
                byte[] sdKey = new byte[16];
                getBytesFromTextArea(sdKey, txtSdKey, currentKey);
                File.WriteAllBytes(currentFolder + "\\shared\\" + currentKey, sdKey);
                count++;
            }
            catch (Exception ex)
            {
                MessageBox.Show("Can't write "  + currentKey);
            }

            currentKey = "sd-iv";
            try
            {
                byte[] sdiv = new byte[16];
                getBytesFromTextArea(sdiv, txtSdIv, currentKey);
                File.WriteAllBytes(currentFolder + "\\shared\\" + currentKey, sdiv);
                count++;
            }
            catch (Exception ex)
            {
                MessageBox.Show("Can't write " + currentKey);
            }

            currentKey = "md5-blanker";
            try
            {
                byte[] md5Blanker = new byte[16];
                getBytesFromTextArea(md5Blanker, txtMd5Blanker, currentKey);
                File.WriteAllBytes(currentFolder + "\\shared\\" + currentKey, md5Blanker);
                count++;
            }
            catch (Exception ex)
            {
                MessageBox.Show("Can't write " + currentKey);
            }


            currentKey = "NG-id";
            try
            {
                byte[] ngId = new byte[4];
                getBytesFromTextArea(ngId, txtNgId, currentKey);
                File.WriteAllBytes(currentFolder + "\\private\\" + currentKey, ngId);
                count++;
            }
            catch (Exception ex)
            {
                MessageBox.Show("Can't write " + currentKey);
            }



            currentKey = "NG-key-id";
            try
            {
                byte[] ngKeyId = new byte[4];
                getBytesFromTextArea(ngKeyId, txtNGKeyId, currentKey);
                File.WriteAllBytes(currentFolder + "\\private\\" + currentKey, ngKeyId);
                count++;
            }
            catch (Exception ex)
            {
                MessageBox.Show("Can't write " + currentKey);
            }


            currentKey = "NG-mac";
            try
            {
                byte[] ngMac = new byte[6];
                getBytesFromTextArea(ngMac, txtNGMac, currentKey);
                File.WriteAllBytes(currentFolder + "\\private\\" + currentKey, ngMac);
                count++;
            }
            catch (Exception ex)
            {
                MessageBox.Show("Can't write " + currentKey);
            }


            currentKey = "NG-priv";
            try
            {
                byte[] ngPriv = new byte[30];
                getBytesFromTextArea(ngPriv, txtNGPriv, currentKey);
                File.WriteAllBytes(currentFolder + "\\private\\" + currentKey, ngPriv);
                count++;
            }
            catch (Exception ex)
            {
                MessageBox.Show("Can't write " + currentKey);
            }

            currentKey = "NG-sig";
            try
            {
                byte[] ngSig = new byte[60];
                getBytesFromTextArea(ngSig, txtNGSig, currentKey);
                File.WriteAllBytes(currentFolder + "\\private\\" + currentKey, ngSig);
                count++;
            }
            catch (Exception ex)
            {
                MessageBox.Show("Can't write " + currentKey);
            }


            if (count == 8)
            {
                MessageBox.Show("All files saved successfully");
            }
            else if ((count<8) && (count>0))
            {
                MessageBox.Show("Some of the files saved.");
            }
        }

        private string Dup(String duplicationString, int count)
        {
            StringBuilder sb = new StringBuilder();
            for (int i = 0; i < count; i++)
            {
                sb.Append(duplicationString);
            }
            return sb.ToString();
        }

        private void btnGetFromWii_Click(object sender, EventArgs e)
        {
            if (txtIpAddress.Text == String.Empty) 
            {
                MessageBox.Show("No playing with your wii's ip address unentered!", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return;
            }

            txtNGPriv.Text = Dup(" ", 32 * 3 + 1);
            txtNGSig.Text = Dup(" ", 60 * 3 + 1);
            thread = new Thread(ThreadWork);
            thread.IsBackground = true;
            thread.Start();

            Thread.Sleep(1);

            threadWiiLoad = new Thread(ThreadWiiLoad);
            threadWiiLoad.IsBackground = true;
            threadWiiLoad.Start();
            
        }

        private void ThreadWork() 
        {
            Listener.HandleResponse handler = handleResponse;

            try
            {
                using (Listener listener = new Listener())
                {
                    listener.Initialize(handler);
                    listener.Listen(handler);
                }
            }
            catch (Exception ex)
            {
                handler.Invoke("ERROR:" + ex.ToString(), 0, null);       
            }
            
        }

        public string GetLocalIPAddress()
        {
            IPHostEntry host;
            string localIP = "";
            host = Dns.GetHostEntry(Dns.GetHostName());
            foreach (IPAddress ip in host.AddressList)
            {
                if (ip.AddressFamily.ToString() == "InterNetwork")
                {
                    return ip.ToString();
                }
            }
            return localIP;
        }

        private void ThreadWiiLoad()
        {
            WiiLoad.WiiLoad.WiiLoadProgress wiiloadProgressHandler = handleWiiLoadProgress;
            WiiLoad.WiiLoad wiiload = new WiiLoad.WiiLoad();
            
            string keyGrabberFile = "keygrabber.dol";
            byte[] keyGrabber = File.ReadAllBytes(keyGrabberFile);
            string[] args = new string[] { keyGrabberFile, GetLocalIPAddress() };
            wiiload.SendFile(txtIpAddress.Text, keyGrabber, keyGrabber.Length, wiiloadProgressHandler, args);
        }

        private void handleWiiLoadProgress(int count, int completed, string status, string error)
        {
            if (error == String.Empty)
            {
                SetStatusCallback d = new SetStatusCallback(SetStatus);
                this.Invoke(d, new object[] { status, 0 });
            }
            else
            {
                SetStatusCallback d = new SetStatusCallback(SetStatus);
                this.Invoke(d, new object[] { error, 1 });
            }
        }

        private void handleResponse(string response, int length, byte[] data)
        {
            System.Diagnostics.Debug.WriteLine(response);
            if ((response!=String.Empty) && (response.StartsWith("ERROR:")) )
            {
                SetStatusCallback d = new SetStatusCallback(SetStatus);
                this.Invoke(d, new object[] { "Houston we got a problem! : " + response, 0 });
                if ((thread != null) && (thread.IsAlive))
                {
                    thread.Abort();
                }
                else if (thread != null)
                {
                    thread = null;
                }
            }


            if (response != String.Empty)
            {
                
                SetStatusCallback d = new SetStatusCallback(SetStatus);
                this.Invoke(d, new object[] { response, 0 });
                string keyValue; KeyType keyType = KeyType.None; int keyPart = 0;
                if (ParseResponse(response, "NG-mac: ", out keyValue))
                {
                    keyType = KeyType.NGMac; keyPart = 0;
                }
                else if (ParseResponse(response, "NG-id: ", out keyValue))
                {
                    keyType = KeyType.NGId; keyPart = 0;
                }
                else if (ParseResponse(response, "NG-key-id: ", out keyValue))
                {
                    keyType = KeyType.NGKeyId; keyPart = 0;
                }
                else if (ParseResponse(response, "NG-priv: ", out keyValue))
                {
                    keyType = KeyType.NGPriv; keyPart = 0;
                }
                else if (ParseResponse(response, "NG-sig: ", out keyValue))
                {
                    keyType = KeyType.NGSig; keyPart = 0;
                }

                if (keyValue != String.Empty)
                {
                    SetKeyCallback k = new SetKeyCallback(SetKey);
                    this.Invoke(k, new object[] { keyType, keyValue, keyPart });
                }

            }
        }


        private bool ParseResponse(string response, string keyPrefix, out string keyValue) 
        {
            if (response.Contains(keyPrefix))
            {
                int startIndex = response.IndexOf(keyPrefix);
                int left = startIndex + keyPrefix.Length;
                keyValue = response.Substring(left, response.Length - left);
                return true;
            }
            else
            {
                keyValue = String.Empty;
                return false;
            }
        }

        private void SetStatus(String text, int stat)
        {
            if (stat == 0)
            {
                statusLabel.Text = text;
            }
            else
            {
                MessageBox.Show(text, "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void SetKey(KeyType keyType, String keyValue, int keyPart)
        {
            if (keyType == KeyType.NGMac)
            {
                txtNGMac.Text = keyValue.Trim();
            }
            else if (keyType == KeyType.NGId)
            {
                txtNgId.Text = keyValue.Trim();
            }
            else if (keyType == KeyType.NGKeyId)
            {
                txtNGKeyId.Text = keyValue.Trim();
            }
            else if (keyType == KeyType.NGPriv)
            {
                txtNGPriv.Text = keyValue.Trim();
            }
            else if (keyType == KeyType.NGSig)
            {
                txtNGSig.Text = keyValue.Trim();
            }
        }

        private void Initial_FormClosing(object sender, FormClosingEventArgs e)
        {
            if ((thread != null) && (thread.IsAlive))
            {
                thread.Abort();
            }
        }

    }
}
