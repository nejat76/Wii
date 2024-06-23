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
using Wii;
using CrapInstaller;

namespace CrazyInstaller
{
    public partial class Form1 : Form
    {
        public Form1()
        {
            InitializeComponent();
        }

        public enum InstallerError
        {
            None,
            NotAWadFile,
            NotAUserChannel
        }

        private InstallerError ensureSafeChannel(byte[] wadFile)
        {
            if (!WadInfo.IsThisWad(wadFile))
            {
                return InstallerError.NotAWadFile;
            } else 
            {
                return InstallerError.None;
                //string fullTitleId = WadInfo.GetFullTitleID(wadFile, 1);
                //if (!fullTitleId.StartsWith("00010001"))
                //{
                //    return InstallerError.NotAUserChannel;
                //}
            }

            return InstallerError.None;
        }

        private void btnBrowse_Click(object sender, EventArgs e)
        {
            DialogResult result = openFileDialog1.ShowDialog();

            if (result == DialogResult.OK)
            {
                txtWadFile.Text = openFileDialog1.FileName;
            }

        }

        private byte[] preprocessWadFile(string filePath)
        {
            int iosToUse;
            if (Int32.TryParse(txtIosToUse.Text, out iosToUse))
            {
                if ((iosToUse < 3) || (iosToUse > 255))
                {
                    MessageBox.Show("Oops, please input a valid ios to use (3-255)!", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                    return null;
                }
            }
            else
            {
                MessageBox.Show("Oops, please input ios to use!", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return null;
            }

            if (filePath == String.Empty)
            {
                MessageBox.Show("Oops, select a source channel wad first!", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return null;
            }

            if (!File.Exists(filePath))
            {
                MessageBox.Show("Can't find the file!", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return null;
            }

            //See if file is smaller than 4MB
            FileInfo fileInfo = new FileInfo(filePath);
            long fileSize = fileInfo.Length;

            if (fileSize > (4 * 1024 * 1024 - 32))
            {
                MessageBox.Show("Sorry, this tool can't be used for channels over 4MB of size", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return null;
            }

            FileStream fs = null;

            try
            {
                byte[] fileContent = new byte[fileSize];
                fs = new FileStream(filePath, FileMode.Open);
                int read = fs.Read(fileContent, 0, (int)fileSize);
                if (read != fileSize)
                {
                    MessageBox.Show("Oops, can't read the file!", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                    return null;
                }

                InstallerError error = ensureSafeChannel(fileContent);

                if (error == InstallerError.NotAUserChannel)
                {
                    MessageBox.Show("This doesn't seem to be a user channel. This program can't be used for system channels like IOSes and other Ninty stuff!", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                    return null;
                }
                else if (error == InstallerError.NotAWadFile)
                {
                    MessageBox.Show("The file you pointed doesn't seem to be a wad file!", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                    return null;
                }

                return fileContent;

            }
            catch (Exception ex)
            {
                MessageBox.Show("Error! : " + ex.ToString(), "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
            finally
            {
                if (fs!=null) 
                {
                    fs.Dispose();
                }
            }
            return null;
        }

        private void btnCreateInstallerDol_Click(object sender, EventArgs e)
        {
            string fileName = txtWadFile.Text;
            byte[] wadContent = preprocessWadFile(fileName);
            if (wadContent != null)
            {
                try 
                {
                    int iosToUse = Convert.ToInt32(txtIosToUse.Text);
                    MemoryStream installerStream = InstallerHelper.CreateInstaller(wadContent, (byte) iosToUse);
                    saveFileDialog1.DefaultExt = "dol";
                    saveFileDialog1.Filter = "Wii executable files|*.dol";
                    DialogResult result = saveFileDialog1.ShowDialog();
                    if (result == DialogResult.OK)
                    {
                        string outputFilePath = saveFileDialog1.FileName;

                        byte[] outputFileContents = installerStream.ToArray();
                        File.WriteAllBytes(outputFilePath, outputFileContents);
                        MessageBox.Show("Successfuly created installer dol");
                    }
                }
                catch(Exception ex)
                {
                    MessageBox.Show("Error creating installer dol! : " + ex.ToString(), "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                }
                
            }
        }

        private void saveFileDialog1_FileOk(object sender, CancelEventArgs e)
        {
            
        }

        private void btnCreateInstallerExe_Click(object sender, EventArgs e)
        {
            string fileName = txtWadFile.Text;
            byte[] wadContent = preprocessWadFile(fileName);
            if (wadContent != null)
            {
                try
                {
                    int iosToUse = Convert.ToInt32(txtIosToUse.Text);
                    MemoryStream installerStream = InstallerHelper.CreateInstaller(wadContent, (byte)iosToUse);
                    int uncompressedSize = (int)installerStream.Length;
                    byte[] loader = InstallerHelper.CreateCompressedInstallerUsingZlib(installerStream);

					String wadNamePart = Path.GetFileName(fileName);

                    saveFileDialog1.DefaultExt = "exe";
                    saveFileDialog1.Filter = "Windows executable files|*.exe";
                    DialogResult result = saveFileDialog1.ShowDialog();
                    if (result == DialogResult.OK)
                    {
                        string outputFilePath = saveFileDialog1.FileName;

                        String exeName = InstallerHelper.CreateSelfInstaller(AppDomain.CurrentDomain.BaseDirectory, "192.168.2.4", wadNamePart, loader, uncompressedSize, iosToUse, outputFilePath);
                        MessageBox.Show("Successfuly created installer exe");
                    }
                }
                catch (Exception ex)
                {
                    MessageBox.Show("Error creating installer exe! : " + ex.ToString(), "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                }

            }

        }
    }
}
