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
    public class WadEdit
    {
        /// <summary>
        /// Changes the region of the wad file
        /// </summary>
        /// <param name="wadfile"></param>
        /// <param name="region">0 = JAP, 1 = USA, 2 = EUR, 3 = FREE</param>
        /// <returns></returns>
        public static byte[] ChangeRegion(byte[] wadfile, int region)
        {

            int tmdpos = WadInfo.GetTmdPos(wadfile);

            if (region == 0) wadfile[tmdpos + 0x19d] = 0x00;
            else if (region == 1) wadfile[tmdpos + 0x19d] = 0x01;
            else if (region == 2) wadfile[tmdpos + 0x19d] = 0x02;
            else wadfile[tmdpos + 0x19d] = 0x03;

            wadfile = TruchaSign(wadfile, 1);

            return wadfile;
        }

        /// <summary>
        /// Changes the region of the wad file
        /// </summary>
        /// <param name="wadfile"></param>
        /// <param name="region"></param>
        public static void ChangeRegion(string wadfile, int region)
        {
            byte[] wadarray = Tools.LoadFileToByteArray(wadfile);
            wadarray = ChangeRegion(wadarray, region);

            using (FileStream fs = new FileStream(wadfile, FileMode.Open, FileAccess.Write))
            {
                fs.Seek(0, SeekOrigin.Begin);
                fs.Write(wadarray, 0, wadarray.Length);
            }
        }

        /// <summary>
        /// Changes the Channel Title of the wad file
        /// All languages have the same title
        /// </summary>
        /// <param name="wadfile"></param>
        /// <param name="title"></param>
        /// <returns></returns>
        public static byte[] ChangeChannelTitle(byte[] wadfile, string title)
        {
            return ChangeChannelTitle(wadfile, title, title, title, title, title, title, title);
        }

        /// <summary>
        /// Changes the Channel Title of the wad file
        /// Each language has a specific title
        /// </summary>
        /// <param name="wadfile"></param>
        /// <param name="jap"></param>
        /// <param name="eng"></param>
        /// <param name="ger"></param>
        /// <param name="fra"></param>
        /// <param name="spa"></param>
        /// <param name="ita"></param>
        /// <param name="dut"></param>
        public static void ChangeChannelTitle(string wadfile, string jap, string eng, string ger, string fra, string spa, string ita, string dut)
        {
            byte[] wadarray = Tools.LoadFileToByteArray(wadfile);
            wadarray = ChangeChannelTitle(wadarray, jap, eng, ger, fra, spa, ita, dut);

            using (FileStream fs = new FileStream(wadfile, FileMode.Open, FileAccess.Write))
            {
                fs.Seek(0, SeekOrigin.Begin);
                fs.Write(wadarray, 0, wadarray.Length);
            }
        }

        /// <summary>
        /// Changes the Channel Title of the wad file
        /// Each language has a specific title
        /// </summary>
        /// <param name="wadfile"></param>
        /// <param name="jap">Japanese Title</param>
        /// <param name="eng">English Title</param>
        /// <param name="ger">German Title</param>
        /// <param name="fra">French Title</param>
        /// <param name="spa">Spanish Title</param>
        /// <param name="ita">Italian Title</param>
        /// <param name="dut">Dutch Title</param>
        /// <returns></returns>
        public static byte[] ChangeChannelTitle(byte[] wadfile, string jap, string eng, string ger, string fra, string spa, string ita, string dut)
        {
            Tools.ChangeProgress(0);

            char[] japchars = jap.ToCharArray();
            char[] engchars = eng.ToCharArray();
            char[] gerchars = ger.ToCharArray();
            char[] frachars = fra.ToCharArray();
            char[] spachars = spa.ToCharArray();
            char[] itachars = ita.ToCharArray();
            char[] dutchars = dut.ToCharArray();

            byte[] titlekey = WadInfo.GetTitleKey(wadfile);
            string[,] conts = WadInfo.GetContentInfo(wadfile);
            int tmdpos = WadInfo.GetTmdPos(wadfile);
            int tmdsize = WadInfo.GetTmdSize(wadfile);
            int nullapp = 0;
            int contentpos = 64 + Tools.AddPadding(WadInfo.GetCertSize(wadfile)) + Tools.AddPadding(WadInfo.GetTikSize(wadfile)) + Tools.AddPadding(WadInfo.GetTmdSize(wadfile));
            SHA1Managed sha1 = new SHA1Managed();

            Tools.ChangeProgress(10);

            for (int i = 0; i < conts.GetLength(0); i++)
            {
                if (conts[i, 1] == "00000000")
                {
                    nullapp = i;
                    break;
                }
                else
                    contentpos += Tools.AddPadding(Convert.ToInt32(conts[i, 3]));
            }

            byte[] contenthandle = DecryptContent(wadfile, nullapp, titlekey);

            Tools.ChangeProgress(25);

            int imetpos = 0;

            for (int z = 0; z < 400; z++)
            {
                if (Convert.ToChar(contenthandle[z]) == 'I')
                    if (Convert.ToChar(contenthandle[z + 1]) == 'M')
                        if (Convert.ToChar(contenthandle[z + 2]) == 'E')
                            if (Convert.ToChar(contenthandle[z + 3]) == 'T')
                            {
                                imetpos = z;
                                break;
                            }
            }

            Tools.ChangeProgress(40);

            int count = 0;

            for (int x = imetpos; x < imetpos + 40; x += 2)
            {
                if (japchars.Length > count)
                {
                    contenthandle[x + 29] = BitConverter.GetBytes(japchars[count])[0];
                    contenthandle[x + 28] = BitConverter.GetBytes(japchars[count])[1];
                }
                else { contenthandle[x + 29] = 0x00; contenthandle[x + 28] = 0x00; }
                if (engchars.Length > count)
                {
                    contenthandle[x + 29 + 84] = BitConverter.GetBytes(engchars[count])[0];
                    contenthandle[x + 29 + 84 - 1] = BitConverter.GetBytes(engchars[count])[1];
                }
                else { contenthandle[x + 29 + 84] = 0x00; contenthandle[x + 29 + 84 - 1] = 0x00; }
                if (gerchars.Length > count)
                {
                    contenthandle[x + 29 + 84 * 2] = BitConverter.GetBytes(gerchars[count])[0];
                    contenthandle[x + 29 + 84 * 2 - 1] = BitConverter.GetBytes(gerchars[count])[1];
                }
                else { contenthandle[x + 29 + 84 * 2] = 0x00; contenthandle[x + 29 + 84 * 2 - 1] = 0x00; }
                if (frachars.Length > count)
                {
                    contenthandle[x + 29 + 84 * 3] = BitConverter.GetBytes(frachars[count])[0];
                    contenthandle[x + 29 + 84 * 3 - 1] = BitConverter.GetBytes(frachars[count])[1];
                }
                else { contenthandle[x + 29 + 84 * 3] = 0x00; contenthandle[x + 29 + 84 * 3 - 1] = 0x00; }
                if (spachars.Length > count)
                {
                    contenthandle[x + 29 + 84 * 4] = BitConverter.GetBytes(spachars[count])[0];
                    contenthandle[x + 29 + 84 * 4 - 1] = BitConverter.GetBytes(spachars[count])[1];
                }
                else { contenthandle[x + 29 + 84 * 4] = 0x00; contenthandle[x + 29 + 84 * 4 - 1] = 0x00; }
                if (itachars.Length > count)
                {
                    contenthandle[x + 29 + 84 * 5] = BitConverter.GetBytes(itachars[count])[0];
                    contenthandle[x + 29 + 84 * 5 - 1] = BitConverter.GetBytes(itachars[count])[1];
                }
                else { contenthandle[x + 29 + 84 * 5] = 0x00; contenthandle[x + 29 + 84 * 5 - 1] = 0x00; }
                if (dutchars.Length > count)
                {
                    contenthandle[x + 29 + 84 * 6] = BitConverter.GetBytes(dutchars[count])[0];
                    contenthandle[x + 29 + 84 * 6 - 1] = BitConverter.GetBytes(dutchars[count])[1];
                }
                else { contenthandle[x + 29 + 84 * 6] = 0x00; contenthandle[x + 29 + 84 * 6 - 1] = 0x00; }

                count++;
            }

            Tools.ChangeProgress(50);

            byte[] newmd5 = new byte[16];
            contenthandle = FixMD5InImet(contenthandle, out newmd5);
            byte[] newsha = sha1.ComputeHash(contenthandle);

            contenthandle = EncryptContent(contenthandle, WadInfo.ReturnTmd(wadfile), nullapp, titlekey, false);

            Tools.ChangeProgress(70);

            for (int y = 0; y < contenthandle.Length; y++)
            {
                wadfile[contentpos + y] = contenthandle[y];
            }

            //SHA1 in TMD
            byte[] tmd = Tools.GetPartOfByteArray(wadfile, tmdpos, tmdsize);
            for (int i = 0; i < 20; i++)
                tmd[0x1f4 + (36 * nullapp) + i] = newsha[i];
            TruchaSign(tmd, 1);
            wadfile = Tools.InsertByteArray(wadfile, tmd, tmdpos);

            int footer = WadInfo.GetFooterSize(wadfile);

            Tools.ChangeProgress(80);

            if (footer > 0)
            {
                int footerpos = wadfile.Length - footer;
                int imetposfoot = 0;

                for (int z = 0; z < 200; z++)
                {
                    if (Convert.ToChar(wadfile[footerpos + z]) == 'I')
                        if (Convert.ToChar(wadfile[footerpos + z + 1]) == 'M')
                            if (Convert.ToChar(wadfile[footerpos + z + 2]) == 'E')
                                if (Convert.ToChar(wadfile[footerpos + z + 3]) == 'T')
                                {
                                    imetposfoot = footerpos + z;
                                    break;
                                }
                }

                Tools.ChangeProgress(90);

                int count2 = 0;

                for (int x = imetposfoot; x < imetposfoot + 40; x += 2)
                {
                    if (japchars.Length > count2) { wadfile[x + 29] = Convert.ToByte(japchars[count2]); }
                    else { wadfile[x + 29] = 0x00; }
                    if (engchars.Length > count2) { wadfile[x + 29 + 84] = Convert.ToByte(engchars[count2]); }
                    else { wadfile[x + 29 + 84] = 0x00; }
                    if (gerchars.Length > count2) { wadfile[x + 29 + 84 * 2] = Convert.ToByte(gerchars[count2]); }
                    else { wadfile[x + 29 + 84 * 2] = 0x00; }
                    if (frachars.Length > count2) { wadfile[x + 29 + 84 * 3] = Convert.ToByte(frachars[count2]); }
                    else { wadfile[x + 29 + 84 * 3] = 0x00; }
                    if (spachars.Length > count2) { wadfile[x + 29 + 84 * 4] = Convert.ToByte(spachars[count2]); }
                    else { wadfile[x + 29 + 84 * 4] = 0x00; }
                    if (itachars.Length > count2) { wadfile[x + 29 + 84 * 5] = Convert.ToByte(itachars[count2]); }
                    else { wadfile[x + 29 + 84 * 5] = 0x00; }
                    if (dutchars.Length > count2) { wadfile[x + 29 + 84 * 6] = Convert.ToByte(dutchars[count2]); }
                    else { wadfile[x + 29 + 84 * 6] = 0x00; }

                    count2++;
                }

                for (int i = 0; i < 16; i++)
                    wadfile[imetposfoot + 1456 + i] = newmd5[i];
            }

            Tools.ChangeProgress(100);
            return wadfile;
        }

        /// <summary>
        /// Changes the Title ID in the Tik or Tmd file
        /// </summary>
        /// <param name="tiktmd"></param>
        /// <param name="type">0 = Tik, 1 = Tmd</param>
        /// <returns></returns>
        public static void ChangeTitleID(string tiktmdfile, int type, string titleid)
        {
            byte[] temp = Tools.LoadFileToByteArray(tiktmdfile);
            temp = ChangeTitleID(temp, type, titleid);
            Tools.SaveFileFromByteArray(temp, tiktmdfile);
        }

        /// <summary>
        /// Changes the Title ID in the Tik or Tmd file
        /// </summary>
        /// <param name="tiktmd"></param>
        /// <param name="type">0 = Tik, 1 = Tmd</param>
        /// <returns></returns>
        public static byte[] ChangeTitleID(byte[] tiktmd, int type, string titleid)
        {
            int offset = 0x1e0;
            if (type == 1) offset = 0x190;
            char[] id = titleid.ToCharArray();

            tiktmd[offset] = (byte)id[0];
            tiktmd[offset + 1] = (byte)id[1];
            tiktmd[offset + 2] = (byte)id[2];
            tiktmd[offset + 3] = (byte)id[3];

            tiktmd = TruchaSign(tiktmd, type);

            return tiktmd;
        }

        /// <summary>
        /// Changes the title ID of the wad file
        /// </summary>
        /// <param name="wadfile"></param>
        /// <param name="titleid"></param>
        /// <returns></returns>
        public static byte[] ChangeTitleID(byte[] wadfile, string titleid)
        {
            Tools.ChangeProgress(0);

            int tikpos = WadInfo.GetTikPos(wadfile);
            int tmdpos = WadInfo.GetTmdPos(wadfile);
            char[] id = titleid.ToCharArray();

            byte[] oldtitlekey = WadInfo.GetTitleKey(wadfile);

            Tools.ChangeProgress(20);

            //Change the ID in the ticket
            wadfile[tikpos + 0x1e0] = (byte)id[0];
            wadfile[tikpos + 0x1e1] = (byte)id[1];
            wadfile[tikpos + 0x1e2] = (byte)id[2];
            wadfile[tikpos + 0x1e3] = (byte)id[3];

            //Change the ID in the tmd
            wadfile[tmdpos + 0x190] = (byte)id[0];
            wadfile[tmdpos + 0x191] = (byte)id[1];
            wadfile[tmdpos + 0x192] = (byte)id[2];
            wadfile[tmdpos + 0x193] = (byte)id[3];

            Tools.ChangeProgress(40);

            //Trucha-Sign both
            wadfile = TruchaSign(wadfile, 0);

            Tools.ChangeProgress(50);

            wadfile = TruchaSign(wadfile, 1);

            Tools.ChangeProgress(60);

            byte[] newtitlekey = WadInfo.GetTitleKey(wadfile);
            byte[] tmd = WadInfo.ReturnTmd(wadfile);

            int contentcount = WadInfo.GetContentNum(wadfile);

            wadfile = ReEncryptAllContents(wadfile, oldtitlekey, newtitlekey);

            Tools.ChangeProgress(100);
            return wadfile;
        }

        /// <summary>
        /// Changes the title ID of the wad file
        /// </summary>
        /// <param name="wadfile"></param>
        /// <param name="titleid"></param>
        public static void ChangeTitleID(string wadfile, string titleid)
        {
            byte[] wadarray = Tools.LoadFileToByteArray(wadfile);
            wadarray = ChangeTitleID(wadarray, titleid);

            using (FileStream fs = new FileStream(wadfile, FileMode.Open, FileAccess.Write))
            {
                fs.Seek(0, SeekOrigin.Begin);
                fs.Write(wadarray, 0, wadarray.Length);
            }
        }

        /// <summary>
        /// Clears the Signature of the Tik / Tmd to 0x00
        /// </summary>
        /// <param name="wadtiktmd">Wad, Tik or Tmd</param>
        /// <param name="type">0 = Tik, 1 = Tmd</param>
        /// <returns></returns>
        public static byte[] ClearSignature(byte[] wadtiktmd, int type)
        {
            int tmdtikpos = 0;
            int tmdtiksize = wadtiktmd.Length; ;

            if (WadInfo.IsThisWad(wadtiktmd) == true)
            {
                //It's a wad file, so get the tik or tmd position and length
                switch (type)
                {
                    case 1:
                        tmdtikpos = WadInfo.GetTmdPos(wadtiktmd);
                        tmdtiksize = WadInfo.GetTmdSize(wadtiktmd);
                        break;
                    default:
                        tmdtikpos = WadInfo.GetTikPos(wadtiktmd);
                        tmdtiksize = WadInfo.GetTikSize(wadtiktmd);
                        break;
                }
            }

            for (int i = 4; i < 260; i++)
            {
                wadtiktmd[tmdtikpos + i] = 0x00;
            }

            return wadtiktmd;
        }

        /// <summary>
        /// Trucha-Signs the Tik or Tmd
        /// </summary>
        /// <param name="file">Wad or Tik or Tmd</param>
        /// <param name="type">0 = Tik, 1 = Tmd</param>
        /// <returns></returns>
        public static void TruchaSign(string file, int type)
        {
            byte[] temp = Tools.LoadFileToByteArray(file);
            temp = TruchaSign(temp, type);
            Tools.SaveFileFromByteArray(temp, file);
        }

        /// <summary>
        /// Trucha-Signs the Tik or Tmd
        /// </summary>
        /// <param name="wadortmd">Wad or Tik or Tmd</param>
        /// <param name="type">0 = Tik, 1 = Tmd</param>
        /// <returns></returns>
        public static byte[] TruchaSign(byte[] wadtiktmd, int type)
        {
            SHA1Managed sha = new SHA1Managed();
            int[] position = new int[2] { 0x1f2, 0x1d4 };
            int[] tosign = new int[2] { 0x140, 0x140 }; //0x104 0x140	1790	  
            int tiktmdpos = 0;
            int tiktmdsize = wadtiktmd.Length;

            if (sha.ComputeHash(wadtiktmd, tiktmdpos + tosign[type], tiktmdsize - tosign[type])[0] != 0x00)
            {
                ClearSignature(wadtiktmd, type);

                if (WadInfo.IsThisWad(wadtiktmd) == true)
                {
                    //It's a wad file
                    if (type == 0) //Get Tik position and size
                    {
                        tiktmdpos = WadInfo.GetTikPos(wadtiktmd);
                        tiktmdsize = WadInfo.GetTikSize(wadtiktmd);
                    }
                    else //Get Tmd position and size
                    {
                        tiktmdpos = WadInfo.GetTmdPos(wadtiktmd);
                        tiktmdsize = WadInfo.GetTmdSize(wadtiktmd);
                    }
                }

                byte[] sha1 = new byte[20];

                for (UInt16 i = 0; i < 65535; i++)
                {
                    byte[] hex = BitConverter.GetBytes(i);
                    wadtiktmd[tiktmdpos + position[type]] = hex[0];
                    wadtiktmd[tiktmdpos + position[type] + 1] = hex[1];

                    sha1 = sha.ComputeHash(wadtiktmd, tiktmdpos + tosign[type], tiktmdsize - tosign[type]);
                    if (sha1[0] == 0x00) break;
                }

                return wadtiktmd;
            }
            else return wadtiktmd;
        }

        /// <summary>
        /// Decrypts the given content
        /// </summary>
        /// <param name="content"></param>
        /// <returns></returns>
        public static byte[] DecryptContent(byte[] wadfile, int contentcount, byte[] titlekey)
        {
            int tmdpos = WadInfo.GetTmdPos(wadfile);
            byte[] iv = new byte[16];
            string[,] continfo = WadInfo.GetContentInfo(wadfile);
            int contentsize = Convert.ToInt32(continfo[contentcount, 3]);
            int paddedsize = Tools.AddPadding(contentsize, 16);

            int contentpos = 64 + Tools.AddPadding(WadInfo.GetCertSize(wadfile)) + Tools.AddPadding(WadInfo.GetTikSize(wadfile)) + Tools.AddPadding(WadInfo.GetTmdSize(wadfile));

            for (int x = 0; x < contentcount; x++)
            {
                contentpos += Tools.AddPadding(Convert.ToInt32(continfo[x, 3]));
            }

            iv[0] = wadfile[tmdpos + 0x1e8 + (0x24 * contentcount)];
            iv[1] = wadfile[tmdpos + 0x1e9 + (0x24 * contentcount)];

            RijndaelManaged decrypt = new RijndaelManaged();
            decrypt.Mode = CipherMode.CBC;
            decrypt.Padding = PaddingMode.None;
            decrypt.KeySize = 128;
            decrypt.BlockSize = 128;
            decrypt.Key = titlekey;
            decrypt.IV = iv;

            ICryptoTransform cryptor = decrypt.CreateDecryptor();

            MemoryStream memory = new MemoryStream(wadfile, contentpos, paddedsize);
            CryptoStream crypto = new CryptoStream(memory, cryptor, CryptoStreamMode.Read);

            bool fullread = false;
            byte[] buffer = new byte[16384];
            byte[] cont = new byte[1];

            using (MemoryStream ms = new MemoryStream())
            {
                while (fullread == false)
                {
                    int len = 0;
                    if ((len = crypto.Read(buffer, 0, buffer.Length)) <= 0)
                    {
                        fullread = true;
                        cont = ms.ToArray();
                    }
                    ms.Write(buffer, 0, len);
                }
            }

            memory.Close();
            crypto.Close();

            Array.Resize(ref cont, contentsize);

            return cont;
        }

        /// <summary>
        /// Decrypts the given content
        /// </summary>
        /// <param name="content"></param>
        /// <param name="tmd"></param>
        /// <param name="contentcount"></param>
        /// <param name="titlekey"></param>
        /// <returns></returns>
        public static byte[] DecryptContent(byte[] content, byte[] tmd, int contentcount, byte[] titlekey)
        {
            byte[] iv = new byte[16];
            string[,] continfo = WadInfo.GetContentInfo(tmd);
            int contentsize = content.Length;
            int paddedsize = Tools.AddPadding(contentsize, 16);
            Array.Resize(ref content, paddedsize);

            iv[0] = tmd[0x1e8 + (0x24 * contentcount)];
            iv[1] = tmd[0x1e9 + (0x24 * contentcount)];

            RijndaelManaged decrypt = new RijndaelManaged();
            decrypt.Mode = CipherMode.CBC;
            decrypt.Padding = PaddingMode.None;
            decrypt.KeySize = 128;
            decrypt.BlockSize = 128;
            decrypt.Key = titlekey;
            decrypt.IV = iv;

            ICryptoTransform cryptor = decrypt.CreateDecryptor();

            MemoryStream memory = new MemoryStream(content, 0, paddedsize);
            CryptoStream crypto = new CryptoStream(memory, cryptor, CryptoStreamMode.Read);

            bool fullread = false;
            byte[] buffer = new byte[memory.Length];
            byte[] cont = new byte[1];

            using (MemoryStream ms = new MemoryStream())
            {
                while (fullread == false)
                {
                    int len = 0;
                    if ((len = crypto.Read(buffer, 0, buffer.Length)) <= 0)
                    {
                        fullread = true;
                        cont = ms.ToArray();
                    }
                    ms.Write(buffer, 0, len);
                }
            }

            memory.Close();
            crypto.Close();

            return cont;
        }

        /// <summary>
        /// Encrypts the given content and adds a padding to the next 64 bytes
        /// </summary>
        /// <param name="content"></param>
        /// <param name="tmd"></param>
        /// <param name="contentcount"></param>
        /// <param name="titlekey"></param>
        /// <returns></returns>
        public static byte[] EncryptContent(byte[] content, byte[] tmd, int contentcount, byte[] titlekey, bool addpadding)
        {
            byte[] iv = new byte[16];
            string[,] continfo = WadInfo.GetContentInfo(tmd);
            int contentsize = content.Length;
            int paddedsize = Tools.AddPadding(contentsize, 16);
            Array.Resize(ref content, paddedsize);

            iv[0] = tmd[0x1e8 + (0x24 * contentcount)];
            iv[1] = tmd[0x1e9 + (0x24 * contentcount)];

            RijndaelManaged encrypt = new RijndaelManaged();
            encrypt.Mode = CipherMode.CBC;
            encrypt.Padding = PaddingMode.None;
            encrypt.KeySize = 128;
            encrypt.BlockSize = 128;
            encrypt.Key = titlekey;
            encrypt.IV = iv;

            ICryptoTransform cryptor = encrypt.CreateEncryptor();

            MemoryStream memory = new MemoryStream(content, 0, paddedsize);
            CryptoStream crypto = new CryptoStream(memory, cryptor, CryptoStreamMode.Read);

            bool fullread = false;
            byte[] buffer = new byte[memory.Length];
            byte[] cont = new byte[1];

            using (MemoryStream ms = new MemoryStream())
            {
                while (fullread == false)
                {
                    int len = 0;
                    if ((len = crypto.Read(buffer, 0, buffer.Length)) <= 0)
                    {
                        fullread = true;
                        cont = ms.ToArray();
                    }
                    ms.Write(buffer, 0, len);
                }
            }

            memory.Close();
            crypto.Close();

            if (addpadding == true) { Array.Resize(ref cont, Tools.AddPadding(cont.Length)); }
            return cont;
        }

        /// <summary>
        /// Re-Encrypts the given content
        /// </summary>
        /// <param name="wadfile"></param>
        /// <returns></returns>
        public static byte[] ReEncryptAllContents(byte[] wadfile, byte[] oldtitlekey, byte[] newtitlekey)
        {
            int contentnum = WadInfo.GetContentNum(wadfile);
            int certsize = WadInfo.GetCertSize(wadfile);
            int tiksize = WadInfo.GetTikSize(wadfile);
            int tmdsize = WadInfo.GetTmdSize(wadfile);
            int contentpos = 64 + Tools.AddPadding(certsize) + Tools.AddPadding(tiksize) + Tools.AddPadding(tmdsize);

            for (int i = 0; i < contentnum; i++)
            {
                byte[] tmd = WadInfo.ReturnTmd(wadfile);
                byte[] decryptedcontent = DecryptContent(wadfile, i, oldtitlekey);
                byte[] encryptedcontent = EncryptContent(decryptedcontent, tmd, i, newtitlekey, false);

                for (int j = 0; j < encryptedcontent.Length; j++)
                {
                    wadfile[contentpos + j] = encryptedcontent[j];
                }
                contentpos += Tools.AddPadding(encryptedcontent.Length);
            }

            return wadfile;
        }

        /// <summary>
        /// Fixes the MD5 Sum in the IMET Header
        /// </summary>
        /// <param name="file"></param>
        /// <returns></returns>
        public static byte[] FixMD5InImet(byte[] file, out byte[] newmd5)
        {
            if (Convert.ToChar(file[128]) == 'I' &&
                Convert.ToChar(file[129]) == 'M' &&
                Convert.ToChar(file[130]) == 'E' &&
                Convert.ToChar(file[131]) == 'T')
            {
                byte[] buffer = new byte[1536];

                using (MemoryStream ms = new MemoryStream())
                {
                    ms.Write(file, 0x40, 1536);
                    buffer = ms.ToArray();
                }

                for (int i = 0; i < 16; i++)
                    buffer[1520 + i] = 0x00;

                MD5 md5 = new MD5CryptoServiceProvider();
                byte[] hash = md5.ComputeHash(buffer);

                for (int i = 0; i < 16; i++)
                    file[1584 + i] = hash[i];

                newmd5 = hash;
                return file;
            }
            else
            {
                byte[] oldmd5 = new byte[16];

                using (MemoryStream ms = new MemoryStream())
                {
                    ms.Write(file, 1584, 16);
                    oldmd5 = ms.ToArray();
                }

                newmd5 = oldmd5;
                return file;
            }
        }

        /// <summary>
        /// Fixes the MD5 Sum in the IMET Header.
        /// </summary>
        /// <param name="file"></param>
        /// <returns></returns>
        public static byte[] FixMD5InImet(byte[] file)
        {
            byte[] tmp = new byte[16];
            return FixMD5InImet(file, out tmp);
        }

        /// <summary>
        /// Updates the Content Info in the Tmd.
        /// Tmd and Contents must be in the same Directory
        /// </summary>
        /// <param name="tmdfile"></param>
        public static void UpdateTmdContents(string tmdfile)
        {
            FileStream tmd = new FileStream(tmdfile, FileMode.Open, FileAccess.ReadWrite);

            tmd.Seek(0x1de, SeekOrigin.Begin);
            int contentcount = Tools.HexStringToInt(tmd.ReadByte().ToString("x2") + tmd.ReadByte().ToString("x2"));

            for (int i = 0; i < contentcount; i++)
            {
                int oldsize = 0;
                int contentpos = 0x1e4 + (36 * i);

                tmd.Seek(contentpos, SeekOrigin.Begin);
                string id = tmd.ReadByte().ToString("x2") + tmd.ReadByte().ToString("x2") + tmd.ReadByte().ToString("x2") + tmd.ReadByte().ToString("x2");
                string index = "0000" + tmd.ReadByte().ToString("x2") + tmd.ReadByte().ToString("x2");
                string type = tmd.ReadByte().ToString("x2") + tmd.ReadByte().ToString("x2");

                if (type != "0001") continue;

                try
                {
                    oldsize = Tools.HexStringToInt(tmd.ReadByte().ToString("x2") +
                        tmd.ReadByte().ToString("x2") +
                        tmd.ReadByte().ToString("x2") +
                        tmd.ReadByte().ToString("x2") +
                        tmd.ReadByte().ToString("x2") +
                        tmd.ReadByte().ToString("x2") +
                        tmd.ReadByte().ToString("x2") +
                        tmd.ReadByte().ToString("x2"));
                }
                catch { }

                byte[] oldsha1 = new byte[20];
                tmd.Read(oldsha1, 0, oldsha1.Length);
                string fileName = id;

                if (!File.Exists(tmdfile.Remove(tmdfile.LastIndexOf('\\') + 1) + fileName + ".app"))
                    fileName = index;

                if (File.Exists(tmdfile.Remove(tmdfile.LastIndexOf('\\') + 1) + fileName + ".app"))
                {
                    byte[] content = Wii.Tools.LoadFileToByteArray(tmdfile.Remove(tmdfile.LastIndexOf('\\') + 1) + fileName + ".app");
                    int newsize = content.Length;

                    if (newsize != oldsize)
                    {
                        byte[] changedsize = Tools.FileLengthToByteArray(newsize);

                        tmd.Seek(contentpos + 8, SeekOrigin.Begin);
                        for (int x = 8; x > changedsize.Length; x--) tmd.WriteByte(0x00);
                        tmd.Write(changedsize, 0, changedsize.Length);
                    }

                    SHA1Managed sha1 = new SHA1Managed();
                    byte[] newsha1 = sha1.ComputeHash(content);
                    sha1.Clear();

                    if (Tools.CompareByteArrays(newsha1, oldsha1) == false)
                    {
                        tmd.Seek(contentpos + 16, SeekOrigin.Begin);
                        tmd.Write(newsha1, 0, newsha1.Length);
                    }
                }
                else
                {
                    throw new Exception("At least one content file wasn't found!");
                }
            }

            tmd.Close();
        }

        /// <summary>
        /// Changes the Boot Index in the Tmd to the given value
        /// </summary>
        /// <param name="wadtmd"></param>
        /// <returns></returns>
        public static byte[] ChangeTmdBootIndex(byte[] wadtmd, int newindex)
        {
            int tmdpos = 0;

            if (WadInfo.IsThisWad(wadtmd) == true)
                tmdpos = WadInfo.GetTmdPos(wadtmd);

            byte[] index = BitConverter.GetBytes((UInt16)newindex);
            wadtmd[tmdpos + 0x1e0] = index[1];
            wadtmd[tmdpos + 0x1e1] = index[0];

            return wadtmd;
        }

        /// <summary>
        /// Changes the Content Count in the Tmd
        /// </summary>
        /// <param name="wadtmd"></param>
        /// <param name="newcount"></param>
        /// <returns></returns>
        public static byte[] ChangeTmdContentCount(byte[] wadtmd, int newcount)
        {
            int tmdpos = 0;

            if (WadInfo.IsThisWad(wadtmd) == true)
                tmdpos = WadInfo.GetTmdPos(wadtmd);

            byte[] count = BitConverter.GetBytes((UInt16)newcount);
            wadtmd[tmdpos + 0x1de] = count[1];
            wadtmd[tmdpos + 0x1df] = count[0];

            return wadtmd;
        }

        /// <summary>
        /// Changes the Slot where the IOS Wad will be installed to
        /// </summary>
        /// <param name="wad"></param>
        /// <param name="newslot"></param>
        /// <returns></returns>
        public static byte[] ChangeIosSlot(byte[] wadfile, int newslot)
        {
            Tools.ChangeProgress(0);

            int tikpos = WadInfo.GetTikPos(wadfile);
            int tmdpos = WadInfo.GetTmdPos(wadfile);
            byte[] slot = BitConverter.GetBytes(IPAddress.HostToNetworkOrder(newslot));

            byte[] oldtitlekey = WadInfo.GetTitleKey(wadfile);

            Tools.ChangeProgress(20);

            //Change the ID in the ticket
            wadfile[tikpos + 0x1e0] = slot[0];
            wadfile[tikpos + 0x1e1] = slot[1];
            wadfile[tikpos + 0x1e2] = slot[2];
            wadfile[tikpos + 0x1e3] = slot[3];

            //Change the ID in the tmd
            wadfile[tmdpos + 0x190] = slot[0];
            wadfile[tmdpos + 0x191] = slot[1];
            wadfile[tmdpos + 0x192] = slot[2];
            wadfile[tmdpos + 0x193] = slot[3];

            Tools.ChangeProgress(40);

            //Trucha-Sign both
            wadfile = TruchaSign(wadfile, 0);

            Tools.ChangeProgress(50);

            wadfile = TruchaSign(wadfile, 1);

            Tools.ChangeProgress(60);

            byte[] newtitlekey = WadInfo.GetTitleKey(wadfile);
            byte[] tmd = WadInfo.ReturnTmd(wadfile);

            int contentcount = WadInfo.GetContentNum(wadfile);

            wadfile = ReEncryptAllContents(wadfile, oldtitlekey, newtitlekey);

            Tools.ChangeProgress(100);
            return wadfile;
        }

        /// <summary>
        /// Changes the Title Version of a Wad or Tmd
        /// </summary>
        /// <param name="wadtmd"></param>
        /// <param name="newversion"></param>
        /// <returns></returns>
        public static byte[] ChangeTitleVersion(byte[] wadtmd, int newversion)
        {
            if (newversion > 65535) throw new Exception("Version can be max. 65535");

            int offset = 0x1dc;
            int tmdpos = 0;

            if (WadInfo.IsThisWad(wadtmd))
                tmdpos = WadInfo.GetTmdPos(wadtmd);

            byte[] version = BitConverter.GetBytes((UInt16)newversion);
            Array.Reverse(version);

            wadtmd[tmdpos + offset] = version[0];
            wadtmd[tmdpos + offset + 1] = version[1];

            wadtmd = TruchaSign(wadtmd, 1);

            return wadtmd;
        }

        /// <summary>
        /// Changes the Title Key in the Tik
        /// </summary>
        /// <param name="tik"></param>
        /// <returns></returns>
        public static byte[] ChangeTitleKey(byte[] tik)
        {
            byte[] newKey = new byte[] { 0x47, 0x6f, 0x74, 0x74, 0x61, 0x47, 0x65, 0x74, 0x53, 0x6f, 0x6d, 0x65, 0x42, 0x65, 0x65, 0x72 };
            Tools.InsertByteArray(tik, newKey, 447);
            return tik;
        }
    }

}