/* This file is part of ShowMiiWads
 * Copyright (C) 2009 Leathl
 * 
 * ShowMiiWads is free software: you can redistribute it and/or
 * modify it under the terms of the GNU General Public License as published
 * by the Free Software Foundation, either version 3 of the License, or
 * (at your option) any later version.
 *
 * ShowMiiWads is distributed in the hope that it will be
 * useful, but WITHOUT ANY WARRANTY; without even the implied warranty
 * of MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
 * GNU General Public License for more details.
 *
 * You should have received a copy of the GNU General Public License
 * along with this program.  If not, see <http://www.gnu.org/licenses/>.
 */

//Wii.py by Xuzz, SquidMan, megazig, Matt_P, Omega and The Lemon Man was the base for TPL conversion
//Zetsubou by SquidMan was a reference for TPL conversion
//gbalzss by Andre Perrot was the base for LZ77 (de-)compression
//Thanks to the authors!

using System;
using System.Collections.Generic;
using System.IO;
using System.Net;
using System.Security.Cryptography;
using System.Text;
using System.Drawing;

namespace Wii
{

    public class WadPack
    {
        public static byte[] wadheader = new byte[8] { 0x00, 0x00, 0x00, 0x20, 0x49, 0x73, 0x00, 0x00 };

        /// <summary>
        /// Gets the estimated size, might be !WRONG! due to Lz77 compression
        /// </summary>
        /// <param name="contentFolder"></param>
        public static int GetEstimatedSize(string contentdirectory)
        {
            if (contentdirectory[contentdirectory.Length - 1] != '\\') { contentdirectory = contentdirectory + "\\"; }

            if (!Directory.Exists(contentdirectory)) throw new DirectoryNotFoundException("The directory doesn't exists:\r\n" + contentdirectory);
            if (Directory.GetFiles(contentdirectory, "*.app").Length < 1) throw new Exception("No *.app file was found");
            if (Directory.GetFiles(contentdirectory, "*.cert").Length < 1) throw new Exception("No *.cert file was found");
            if (Directory.GetFiles(contentdirectory, "*.tik").Length < 1) throw new Exception("No *.tik file was found");
            if (Directory.GetFiles(contentdirectory, "*.tmd").Length < 1) throw new Exception("No *.tmd file was found");

            int size = 64; //Wad Header

            string[] certfile = Directory.GetFiles(contentdirectory, "*.cert");
            string[] tikfile = Directory.GetFiles(contentdirectory, "*.tik");
            string[] tmdfile = Directory.GetFiles(contentdirectory, "*.tmd");
            string[,] contents = WadInfo.GetContentInfo(File.ReadAllBytes(tmdfile[0]));

            FileInfo fi = new FileInfo(certfile[0]);
            size += Tools.AddPadding((int)fi.Length);
            fi = new FileInfo(tikfile[0]);
            size += Tools.AddPadding((int)fi.Length);
            fi = new FileInfo(tmdfile[0]);
            size += Tools.AddPadding((int)fi.Length);

            for (int i = 0; i < contents.GetLength(0); i++)
                size += Tools.AddPadding(int.Parse(contents[i, 3]));

            return size + 16; //Footer Timestamp
        }


        /// <summary>
        /// Packs the contents in the given directory and creates the destination wad file 
        /// </summary>
        /// <param name="directory"></param>
        public static void PackWad(string contentdirectory, string destinationfile, byte[] newTitleId)
        {
            if (contentdirectory[contentdirectory.Length - 1] != '\\') { contentdirectory = contentdirectory + "\\"; }

            if (!Directory.Exists(contentdirectory)) throw new DirectoryNotFoundException("The directory doesn't exists:\r\n" + contentdirectory);
            if (Directory.GetFiles(contentdirectory, "*.app").Length < 1) throw new Exception("No *.app file was found");
            if (Directory.GetFiles(contentdirectory, "*.cert").Length < 1) throw new Exception("No *.cert file was found");
            if (Directory.GetFiles(contentdirectory, "*.tik").Length < 1) throw new Exception("No *.tik file was found");
            if (Directory.GetFiles(contentdirectory, "*.tmd").Length < 1) throw new Exception("No *.tmd file was found");
            //if (File.Exists(destinationfile)) throw new Exception("The destination file already exists!");

            string[] certfile = Directory.GetFiles(contentdirectory, "*.cert");
            string[] tikfile = Directory.GetFiles(contentdirectory, "*.tik");
            string[] tmdfile = Directory.GetFiles(contentdirectory, "*.tmd");

            byte[] cert = Tools.LoadFileToByteArray(certfile[0]);
            byte[] tik = Tools.LoadFileToByteArray(tikfile[0]);
            byte[] tmd = Tools.LoadFileToByteArray(tmdfile[0]);

            tik = WadEdit.ChangeTitleKey(tik);

            string[,] contents = WadInfo.GetContentInfo(tmd);

            FileStream wadstream = new FileStream(destinationfile, FileMode.Create);


            /***CHANGE TITLE ID****/
            //Change the ID in the ticket
            tik[0x1e0] = newTitleId[0];
            tik[0x1e1] = newTitleId[1];
            tik[0x1e2] = newTitleId[2];
            tik[0x1e3] = newTitleId[3];

            //Change the ID in the tmd
            tmd[0x190] = newTitleId[0];
            tmd[0x191] = newTitleId[1];
            tmd[0x192] = newTitleId[2];
            tmd[0x193] = newTitleId[3];

            /***CHANGE TITLE ID****/

            //Trucha-Sign Tik and Tmd, if they aren't already
            WadEdit.TruchaSign(tik, 0);
            WadEdit.TruchaSign(tmd, 1);

            //Write Cert
            wadstream.Seek(64, SeekOrigin.Begin);
            wadstream.Write(cert, 0, cert.Length);

            //Write Tik
            wadstream.Seek(64 + Tools.AddPadding(cert.Length), SeekOrigin.Begin);
            wadstream.Write(tik, 0, tik.Length);

            //Write Tmd
            wadstream.Seek(64 + Tools.AddPadding(cert.Length) + Tools.AddPadding(tik.Length), SeekOrigin.Begin);
            wadstream.Write(tmd, 0, tmd.Length);

            //Write Content
            int allcont = 0;
            int contpos = 64 + Tools.AddPadding(cert.Length) + Tools.AddPadding(tik.Length) + Tools.AddPadding(tmd.Length);
            int contcount = WadInfo.GetContentNum(tmd);

            Tools.ChangeProgress(0);
            byte[] titlekey = WadInfo.GetTitleKey(tik);

            for (int i = 0; i < contents.GetLength(0); i++)
            {
                Tools.ChangeProgress((i + 1) * 100 / contents.GetLength(0));
                byte[] thiscont = Tools.LoadFileToByteArray(contentdirectory + contents[i, 1] + ".app");

                //if (i == contents.GetLength(0) - 1) { thiscont = WadEdit.EncryptContent(thiscont, tmd, i, titlekey, false); }
                //else { thiscont = WadEdit.EncryptContent(thiscont, tmd, i, titlekey, true); }
                thiscont = WadEdit.EncryptContent(thiscont, tmd, i, titlekey, true);

                wadstream.Seek(contpos, SeekOrigin.Begin);
                wadstream.Write(thiscont, 0, thiscont.Length);
                contpos += thiscont.Length;
                allcont += thiscont.Length;
            }

            //Write Footer Timestamp
            byte[] footer = Tools.GetTimestamp();
            //byte[] footer = new byte[0];
            Array.Resize(ref footer, Tools.AddPadding(footer.Length));


            wadstream.Seek(Tools.AddPadding(contpos), SeekOrigin.Begin);
            wadstream.Write(footer, 0, footer.Length);

            //Write Header
            byte[] certsize = Tools.FileLengthToByteArray(cert.Length);
            byte[] tiksize = Tools.FileLengthToByteArray(tik.Length);
            byte[] tmdsize = Tools.FileLengthToByteArray(tmd.Length);
            byte[] allcontsize = Tools.FileLengthToByteArray(allcont);
            byte[] footersize = Tools.FileLengthToByteArray(footer.Length);

            wadstream.Seek(0x00, SeekOrigin.Begin);
            wadstream.Write(wadheader, 0, wadheader.Length);
            wadstream.Seek(0x08, SeekOrigin.Begin);
            wadstream.Write(certsize, 0, certsize.Length);
            wadstream.Seek(0x10, SeekOrigin.Begin);
            wadstream.Write(tiksize, 0, tiksize.Length);
            wadstream.Seek(0x14, SeekOrigin.Begin);
            wadstream.Write(tmdsize, 0, tmdsize.Length);
            wadstream.Seek(0x18, SeekOrigin.Begin);
            wadstream.Write(allcontsize, 0, allcontsize.Length);
            wadstream.Seek(0x1c, SeekOrigin.Begin);
            wadstream.Write(footersize, 0, footersize.Length);

            wadstream.Close();
        }

        /// <summary>
        /// Packs a Wad from a title installed on Nand
        /// Returns: 0 = OK, 1 = Files missing, 2 = Shared contents missing, 3 = Cert missing
        /// </summary>
        /// <param name="nandpath"></param>
        /// <param name="path">XXXXXXXX\XXXXXXXX</param>
        /// <param name="destinationfile"></param>
        /// <returns></returns>
        public static void PackWadFromNand(string nandpath, string path, string destinationfile)
        {
            if (nandpath[nandpath.Length - 1] != '\\') { nandpath = nandpath + "\\"; }
            string path1 = path.Remove(8);
            string path2 = path.Remove(0, 9);
            string ticketdir = nandpath + "ticket\\" + path1 + "\\";
            string contentdir = nandpath + "title\\" + path1 + "\\" + path2 + "\\content\\";
            string sharedir = nandpath + "shared1\\";
            string certdir = nandpath + "sys\\";

            if (!Directory.Exists(ticketdir) ||
                !Directory.Exists(contentdir)) throw new DirectoryNotFoundException("Directory doesn't exist:\r\n" + contentdir);
            if (!Directory.Exists(sharedir)) throw new DirectoryNotFoundException("Directory doesn't exist:\r\n" + sharedir);
            if (!File.Exists(certdir + "cert.sys")) throw new FileNotFoundException("File doesn't exist:\r\n" + certdir + "cert.sys");

            byte[] cert = Tools.LoadFileToByteArray(certdir + "cert.sys");
            byte[] tik = Tools.LoadFileToByteArray(ticketdir + path2 + ".tik");
            byte[] tmd = Tools.LoadFileToByteArray(contentdir + "title.tmd");

            tik = WadEdit.ChangeTitleKey(tik);

            string[,] contents = WadInfo.GetContentInfo(tmd);

            FileStream wadstream = new FileStream(destinationfile, FileMode.Create);

            //Trucha-Sign Tik and Tmd, if they aren't already
            WadEdit.TruchaSign(tik, 0);
            WadEdit.TruchaSign(tmd, 1);

            //Write Cert
            wadstream.Seek(64, SeekOrigin.Begin);
            wadstream.Write(cert, 0, cert.Length);

            //Write Tik
            wadstream.Seek(64 + Tools.AddPadding(cert.Length), SeekOrigin.Begin);
            wadstream.Write(tik, 0, tik.Length);

            //Write Tmd
            wadstream.Seek(64 + Tools.AddPadding(cert.Length) + Tools.AddPadding(tik.Length), SeekOrigin.Begin);
            wadstream.Write(tmd, 0, tmd.Length);

            //Write Content
            int allcont = 0;
            int contpos = 64 + Tools.AddPadding(cert.Length) + Tools.AddPadding(tik.Length) + Tools.AddPadding(tmd.Length);
            int contcount = WadInfo.GetContentNum(tmd);

            Tools.ChangeProgress(0);
            byte[] titlekey = WadInfo.GetTitleKey(tik);
            byte[] contentmap = Tools.LoadFileToByteArray(sharedir + "content.map");

            for (int i = 0; i < contents.GetLength(0); i++)
            {
                Tools.ChangeProgress((i + 1) * 100 / contents.GetLength(0));
                byte[] thiscont = new byte[1];

                if (contents[i, 2] == "8001")
                {
                    string contname = ContentMap.GetSharedContentName(contentmap, contents[i, 4]);

                    if (contname == "fail")
                    {
                        wadstream.Close();
                        File.Delete(destinationfile);
                        throw new FileNotFoundException("At least one shared content is missing!");
                    }

                    thiscont = Tools.LoadFileToByteArray(sharedir + contname + ".app");
                }
                else thiscont = Tools.LoadFileToByteArray(contentdir + contents[i, 0] + ".app");

                //if (i == contents.GetLength(0) - 1) { thiscont = WadEdit.EncryptContent(thiscont, tmd, i, titlekey, false); }
                //else { thiscont = WadEdit.EncryptContent(thiscont, tmd, i, titlekey, true); }
                thiscont = WadEdit.EncryptContent(thiscont, tmd, i, titlekey, true);

                wadstream.Seek(contpos, SeekOrigin.Begin);
                wadstream.Write(thiscont, 0, thiscont.Length);
                contpos += thiscont.Length;
                allcont += thiscont.Length;
            }

            //Write Footer Timestamp
            byte[] footer = Tools.GetTimestamp();
            Array.Resize(ref footer, Tools.AddPadding(footer.Length, 16));

            int footerLength = footer.Length;
            wadstream.Seek(Tools.AddPadding(contpos), SeekOrigin.Begin);
            wadstream.Write(footer, 0, footer.Length);

            //Write Header
            byte[] certsize = Tools.FileLengthToByteArray(cert.Length);
            byte[] tiksize = Tools.FileLengthToByteArray(tik.Length);
            byte[] tmdsize = Tools.FileLengthToByteArray(tmd.Length);
            byte[] allcontsize = Tools.FileLengthToByteArray(allcont);
            byte[] footersize = Tools.FileLengthToByteArray(footerLength);

            wadstream.Seek(0x00, SeekOrigin.Begin);
            wadstream.Write(wadheader, 0, wadheader.Length);
            wadstream.Seek(0x08, SeekOrigin.Begin);
            wadstream.Write(certsize, 0, certsize.Length);
            wadstream.Seek(0x10, SeekOrigin.Begin);
            wadstream.Write(tiksize, 0, tiksize.Length);
            wadstream.Seek(0x14, SeekOrigin.Begin);
            wadstream.Write(tmdsize, 0, tmdsize.Length);
            wadstream.Seek(0x18, SeekOrigin.Begin);
            wadstream.Write(allcontsize, 0, allcontsize.Length);
            wadstream.Seek(0x1c, SeekOrigin.Begin);
            wadstream.Write(footersize, 0, footersize.Length);

            wadstream.Close();
        }
    }

}