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
    public class U8
    {
        /// <summary>
        /// Checks if the given file is a U8 Archive
        /// </summary>
        /// <param name="file"></param>
        /// <returns></returns>
        public static bool CheckU8(byte[] file)
        {
            int length = 2500;
            if (file.Length < length) length = file.Length - 4;

            for (int i = 0; i < length; i++)
            {
                if (file[i] == 0x55 && file[i + 1] == 0xAA && file[i + 2] == 0x38 && file[i + 3] == 0x2D)
                    return true;
            }

            return false;
        }

        /// <summary>
        /// Checks if the given file is a U8 Archive
        /// </summary>
        /// <param name="file"></param>
        /// <returns></returns>
        public static bool CheckU8(string file)
        {
            byte[] buff = Tools.LoadFileToByteArray(file, 0, 2500);
            return CheckU8(buff);
        }

        /// <summary>
        /// Gets all contents of a folder (including (sub-)files and (sub-)folders)
        /// </summary>
        /// <param name="dir"></param>
        /// <param name="root"></param>
        /// <returns></returns>
        public static string[] GetDirContent(string dir, bool root)
        {
            string[] files = Directory.GetFiles(dir);
            string[] dirs = Directory.GetDirectories(dir);
            string all = "";

            if (root == false)
                all += dir + "\n";

            for (int i = 0; i < files.Length; i++)
                all += files[i] + "\n";

            foreach (string thisDir in dirs)
            {
                string[] temp = GetDirContent(thisDir, false);

                foreach (string thisTemp in temp)
                    all += thisTemp + "\n";
            }

            return all.Split(new char[] { '\n' }, StringSplitOptions.RemoveEmptyEntries);
        }

        /// <summary>
        /// Detecs if the U8 file has an IMD5 or IMET Header.
        /// Return: 0 = No Header, 1 = IMD5, 2 = IMET
        /// </summary>
        /// <param name="file"></param>
        public static int DetectHeader(string file)
        {
            byte[] temp = Tools.LoadFileToByteArray(file, 0, 400);
            return DetectHeader(temp);
        }

        /// <summary>
        /// Detecs if the U8 file has an IMD5 or IMET Header.
        /// Return: 0 = No Header, 1 = IMD5, 2 = IMET
        /// </summary>
        /// <param name="file"></param>
        public static int DetectHeader(byte[] file)
        {
            for (int i = 0; i < 16; i++) //Just to be safe
            {
                if (Convert.ToChar(file[i]) == 'I')
                    if (Convert.ToChar(file[i + 1]) == 'M')
                        if (Convert.ToChar(file[i + 2]) == 'D')
                            if (Convert.ToChar(file[i + 3]) == '5')
                                return 1;
            }

            int length = 400;
            if (file.Length < 400) length = file.Length - 4;

            for (int z = 0; z < length; z++)
            {
                if (Convert.ToChar(file[z]) == 'I')
                    if (Convert.ToChar(file[z + 1]) == 'M')
                        if (Convert.ToChar(file[z + 2]) == 'E')
                            if (Convert.ToChar(file[z + 3]) == 'T')
                                return 2;
            }

            return 0;
        }

        /// <summary>
        /// Adds an IMD5 Header to the given U8 Archive
        /// </summary>
        /// <param name="u8archive"></param>
        /// <returns></returns>
        public static byte[] AddHeaderIMD5(byte[] u8archive)
        {
            MemoryStream ms = new MemoryStream();
            MD5 md5 = MD5.Create();

            byte[] imd5 = new byte[4];
            imd5[0] = (byte)'I';
            imd5[1] = (byte)'M';
            imd5[2] = (byte)'D';
            imd5[3] = (byte)'5';

            byte[] size = BitConverter.GetBytes(IPAddress.HostToNetworkOrder(u8archive.Length));
            byte[] hash = md5.ComputeHash(u8archive, 0, u8archive.Length);

            ms.Seek(0, SeekOrigin.Begin);
            ms.Write(imd5, 0, imd5.Length);
            ms.Write(size, 0, size.Length);

            ms.Seek(0x10, SeekOrigin.Begin);
            ms.Write(hash, 0, hash.Length);

            ms.Write(u8archive, 0, u8archive.Length);

            md5.Clear();
            return ms.ToArray();
        }

        /// <summary>
        /// Adds an IMET Header to the given 00.app
        /// </summary>
        /// <param name="u8archive"></param>
        /// <param name="channeltitles">Order: Jap, Eng, Ger, Fra, Spa, Ita, Dut</param>
        /// <param name="sizes">Order: Banner.bin, Icon.bin, Sound.bin</param>
        /// <returns></returns>
        public static byte[] AddHeaderIMET(byte[] nullapp, string[] channeltitles, int[] sizes)
        {
            if (channeltitles.Length < 7) return nullapp;
            for (int i = 0; i < channeltitles.Length; i++)
                if (channeltitles[i].Length > 20) return nullapp;

            MemoryStream ms = new MemoryStream();
            MD5 md5 = MD5.Create();

            byte[] imet = new byte[4];
            imet[0] = (byte)'I';
            imet[1] = (byte)'M';
            imet[2] = (byte)'E';
            imet[3] = (byte)'T';

            byte[] unknown = BitConverter.GetBytes(IPAddress.HostToNetworkOrder(0x0000060000000003));

            byte[] iconsize = BitConverter.GetBytes(IPAddress.HostToNetworkOrder(sizes[1]));
            byte[] bannersize = BitConverter.GetBytes(IPAddress.HostToNetworkOrder(sizes[0]));
            byte[] soundsize = BitConverter.GetBytes(IPAddress.HostToNetworkOrder(sizes[2]));

            byte[] japtitle = new byte[84];
            byte[] engtitle = new byte[84];
            byte[] gertitle = new byte[84];
            byte[] fratitle = new byte[84];
            byte[] spatitle = new byte[84];
            byte[] itatitle = new byte[84];
            byte[] duttitle = new byte[84];

            for (int i = 0; i < 20; i++)
            {
                if (channeltitles[0].Length > i)
                {
                    japtitle[i * 2] = BitConverter.GetBytes(channeltitles[0][i])[1];
                    japtitle[i * 2 + 1] = BitConverter.GetBytes(channeltitles[0][i])[0];
                }
                if (channeltitles[1].Length > i)
                {
                    engtitle[i * 2] = BitConverter.GetBytes(channeltitles[1][i])[1];
                    engtitle[i * 2 + 1] = BitConverter.GetBytes(channeltitles[1][i])[0];
                }
                if (channeltitles[2].Length > i)
                {
                    gertitle[i * 2] = BitConverter.GetBytes(channeltitles[2][i])[1];
                    gertitle[i * 2 + 1] = BitConverter.GetBytes(channeltitles[2][i])[0];
                }
                if (channeltitles[3].Length > i)
                {
                    fratitle[i * 2] = BitConverter.GetBytes(channeltitles[3][i])[1];
                    fratitle[i * 2 + 1] = BitConverter.GetBytes(channeltitles[3][i])[0];
                }
                if (channeltitles[4].Length > i)
                {
                    spatitle[i * 2] = BitConverter.GetBytes(channeltitles[4][i])[1];
                    spatitle[i * 2 + 1] = BitConverter.GetBytes(channeltitles[4][i])[0];
                }
                if (channeltitles[5].Length > i)
                {
                    itatitle[i * 2] = BitConverter.GetBytes(channeltitles[5][i])[1];
                    itatitle[i * 2 + 1] = BitConverter.GetBytes(channeltitles[5][i])[0];
                }
                if (channeltitles[6].Length > i)
                {
                    duttitle[i * 2] = BitConverter.GetBytes(channeltitles[6][i])[1];
                    duttitle[i * 2 + 1] = BitConverter.GetBytes(channeltitles[6][i])[0];
                }
            }

            byte[] crypto = new byte[16];

            ms.Seek(128, SeekOrigin.Begin);
            ms.Write(imet, 0, imet.Length);
            ms.Write(unknown, 0, unknown.Length);
            ms.Write(iconsize, 0, iconsize.Length);
            ms.Write(bannersize, 0, bannersize.Length);
            ms.Write(soundsize, 0, soundsize.Length);

            ms.Seek(4, SeekOrigin.Current);
            ms.Write(japtitle, 0, japtitle.Length);
            ms.Write(engtitle, 0, engtitle.Length);
            ms.Write(gertitle, 0, gertitle.Length);
            ms.Write(fratitle, 0, fratitle.Length);
            ms.Write(spatitle, 0, spatitle.Length);
            ms.Write(itatitle, 0, itatitle.Length);
            ms.Write(duttitle, 0, duttitle.Length);

            ms.Seek(0x348, SeekOrigin.Current);
            ms.Write(crypto, 0, crypto.Length);

            byte[] tohash = ms.ToArray();
            crypto = md5.ComputeHash(tohash, 0x40, 0x600);

            ms.Seek(-16, SeekOrigin.Current);
            ms.Write(crypto, 0, crypto.Length);
            ms.Write(nullapp, 0, nullapp.Length);

            md5.Clear();
            return ms.ToArray();
        }

        /// <summary>
        /// Packs a U8 Archive
        /// </summary>
        /// <param name="folder"></param>
        /// <param name="outfile"></param>
        public static void PackU8(string folder, string outfile)
        {
            byte[] u8 = PackU8(folder);

            using (FileStream fs = new FileStream(outfile, FileMode.Create))
                fs.Write(u8, 0, u8.Length);
        }

        /// <summary>
        /// Packs a U8 Archive
        /// </summary>
        /// <param name="folder"></param>
        /// <param name="outfile"></param>
        public static void PackU8(string folder, string outfile, bool addimd5header)
        {
            byte[] u8 = PackU8(folder);

            if (addimd5header == true)
                u8 = AddHeaderIMD5(u8);

            using (FileStream fs = new FileStream(outfile, FileMode.Create))
                fs.Write(u8, 0, u8.Length);
        }

        /// <summary>
        /// Packs a U8 Archive
        /// </summary>
        /// <param name="folder"></param>
        /// <param name="outfile"></param>
        public static byte[] PackU8(string folder)
        {
            int a = 0, b = 0, c = 0;
            return PackU8(folder, out a, out b, out c);
        }

        /// <summary>
        /// Packs a U8 Archive
        /// </summary>
        /// <param name="folder"></param>
        /// <param name="outfile"></param>
        public static byte[] PackU8(string folder, bool addimd5header)
        {
            byte[] u8 = PackU8(folder);

            if (addimd5header == true)
                u8 = AddHeaderIMD5(u8);

            return u8;
        }

        /// <summary>
        /// Packs a U8 Archive
        /// </summary>
        /// <param name="folder"></param>
        /// <param name="outfile"></param>
        public static byte[] PackU8(string folder, out int bannersize, out int iconsize, out int soundsize)
        {
            int datapad = 32, stringtablepad = 32; //Biggie seems to use these paddings, so let's do it, too ;)
            string rootpath = folder;
            if (rootpath[rootpath.Length - 1] != '\\') rootpath = rootpath + "\\";

            bannersize = 0; iconsize = 0; soundsize = 0;

            string[] files = GetDirContent(folder, true);
            int nodecount = files.Length + 1; //All files and dirs + rootnode
            int recursion = 0;
            int currentnodes = 0;
            string name = string.Empty;
            string stringtable = "\0";
            byte[] tempnode = new byte[12];

            MemoryStream nodes = new MemoryStream();
            MemoryStream data = new MemoryStream();
            BinaryWriter writedata = new BinaryWriter(data);

            tempnode[0] = 0x01;
            tempnode[1] = 0x00;
            tempnode[2] = 0x00;
            tempnode[3] = 0x00;
            tempnode[4] = 0x00;
            tempnode[5] = 0x00;
            tempnode[6] = 0x00;
            tempnode[7] = 0x00;

            byte[] temp = BitConverter.GetBytes((UInt32)files.Length + 1); Array.Reverse(temp);
            tempnode[8] = temp[0];
            tempnode[9] = temp[1];
            tempnode[10] = temp[2];
            tempnode[11] = temp[3];

            nodes.Write(tempnode, 0, tempnode.Length);

            for (int i = 0; i < files.Length; i++)
            {
                files[i] = files[i].Remove(0, rootpath.Length - 1);

                recursion = Tools.CountCharsInString(files[i], '\\') - 1;
                name = files[i].Remove(0, files[i].LastIndexOf('\\') + 1);

                byte[] temp1 = BitConverter.GetBytes((UInt16)stringtable.Length); Array.Reverse(temp1);
                tempnode[2] = temp1[0];
                tempnode[3] = temp1[1];

                stringtable += name + "\0";

                if (Directory.Exists(rootpath + files[i])) //It's a dir
                {
                    tempnode[0] = 0x01;
                    tempnode[1] = 0x00;

                    byte[] temp2 = BitConverter.GetBytes((UInt32)recursion); Array.Reverse(temp2);
                    tempnode[4] = temp2[0];
                    tempnode[5] = temp2[1];
                    tempnode[6] = temp2[2];
                    tempnode[7] = temp2[3];

                    int size = currentnodes + 1;

                    for (int j = i; j < files.Length; j++)
                        if (files[j].Contains(files[i])) size++;

                    byte[] temp3 = BitConverter.GetBytes((UInt32)size); Array.Reverse(temp3);
                    tempnode[8] = temp3[0];
                    tempnode[9] = temp3[1];
                    tempnode[10] = temp3[2];
                    tempnode[11] = temp3[3];
                }
                else //It's a file
                {
                    byte[] tempfile = new byte[0x40];
                    int lzoffset = -1;

                    if (files[i].EndsWith("banner.bin"))
                    {
                        tempfile = Wii.Tools.LoadFileToByteArray(rootpath + files[i], 0, tempfile.Length);

                        for (int x = 0; x < tempfile.Length; x++)
                        {
                            if (tempfile[x] == 'L')
                                if (tempfile[x + 1] == 'Z')
                                    if (tempfile[x + 2] == '7')
                                        if (tempfile[x + 3] == '7')
                                        {
                                            lzoffset = x;
                                            break;
                                        }
                        }

                        if (lzoffset != -1)
                        {
                            bannersize = BitConverter.ToInt32(new byte[] { tempfile[lzoffset + 5], tempfile[lzoffset + 6], tempfile[lzoffset + 7], tempfile[lzoffset + 8] }, 0);
                        }
                        else
                        {
                            FileInfo fibanner = new FileInfo(rootpath + files[i]);
                            bannersize = (int)fibanner.Length - 32;
                        }
                    }
                    else if (files[i].EndsWith("icon.bin"))
                    {
                        tempfile = Wii.Tools.LoadFileToByteArray(rootpath + files[i], 0, tempfile.Length);

                        for (int x = 0; x < tempfile.Length; x++)
                        {
                            if (tempfile[x] == 'L')
                                if (tempfile[x + 1] == 'Z')
                                    if (tempfile[x + 2] == '7')
                                        if (tempfile[x + 3] == '7')
                                        {
                                            lzoffset = x;
                                        }
                        }

                        if (lzoffset != -1)
                        {
                            iconsize = BitConverter.ToInt32(new byte[] { tempfile[lzoffset + 5], tempfile[lzoffset + 6], tempfile[lzoffset + 7], tempfile[lzoffset + 8] }, 0);
                        }
                        else
                        {
                            FileInfo fiicon = new FileInfo(rootpath + files[i]);
                            iconsize = (int)fiicon.Length - 32;
                        }
                    }
                    else if (files[i].EndsWith("sound.bin"))
                    {
                        tempfile = Wii.Tools.LoadFileToByteArray(rootpath + files[i], 0, tempfile.Length);

                        for (int x = 0; x < tempfile.Length; x++)
                        {
                            if (tempfile[x] == 'L')
                                if (tempfile[x + 1] == 'Z')
                                    if (tempfile[x + 2] == '7')
                                        if (tempfile[x + 3] == '7')
                                        {
                                            lzoffset = x;
                                            break;
                                        }
                        }

                        if (lzoffset != -1)
                        {
                            soundsize = BitConverter.ToInt32(new byte[] { tempfile[lzoffset + 5], tempfile[lzoffset + 6], tempfile[lzoffset + 7], tempfile[lzoffset + 8] }, 0);
                        }
                        else
                        {
                            FileInfo fisound = new FileInfo(rootpath + files[i]);
                            soundsize = (int)fisound.Length - 32;
                        }
                    }

                    tempnode[0] = 0x00;
                    tempnode[1] = 0x00;

                    byte[] temp2 = BitConverter.GetBytes((UInt32)data.Position); Array.Reverse(temp2);
                    tempnode[4] = temp2[0];
                    tempnode[5] = temp2[1];
                    tempnode[6] = temp2[2];
                    tempnode[7] = temp2[3];

                    FileInfo fi = new FileInfo(rootpath + files[i]);
                    byte[] temp3 = BitConverter.GetBytes((UInt32)fi.Length); Array.Reverse(temp3);
                    tempnode[8] = temp3[0];
                    tempnode[9] = temp3[1];
                    tempnode[10] = temp3[2];
                    tempnode[11] = temp3[3];

                    using (FileStream fs = new FileStream(rootpath + files[i], FileMode.Open))
                    using (BinaryReader br = new BinaryReader(fs))
                        writedata.Write(br.ReadBytes((int)br.BaseStream.Length));

                    writedata.Seek(Tools.AddPadding((int)data.Position, datapad), SeekOrigin.Begin);
                }

                nodes.Write(tempnode, 0, tempnode.Length);
                currentnodes++;
            }

            byte[] type = new byte[2];
            byte[] curpos = new byte[4];

            for (int x = 0; x < nodecount * 12; x += 12)
            {
                nodes.Seek(x, SeekOrigin.Begin);
                nodes.Read(type, 0, 2);

                if (type[0] == 0x00 && type[1] == 0x00)
                {
                    nodes.Seek(x + 4, SeekOrigin.Begin);
                    nodes.Read(curpos, 0, 4);
                    Array.Reverse(curpos);

                    UInt32 newpos = BitConverter.ToUInt32(curpos, 0) + (UInt32)(Tools.AddPadding(0x20 + ((12 * nodecount) + stringtable.Length), stringtablepad));

                    nodes.Seek(x + 4, SeekOrigin.Begin);
                    byte[] temp2 = BitConverter.GetBytes(newpos); Array.Reverse(temp2);
                    nodes.Write(temp2, 0, 4);
                }
            }

            writedata.Close();
            MemoryStream output = new MemoryStream();
            BinaryWriter writeout = new BinaryWriter(output);

            writeout.Write((UInt32)0x2d38aa55);
            writeout.Write(IPAddress.HostToNetworkOrder((ushort)0x20));
            writeout.Write(IPAddress.HostToNetworkOrder((ushort)((12 * nodecount) + stringtable.Length)));
            writeout.Write(IPAddress.HostToNetworkOrder((ushort)(Tools.AddPadding(0x20 + ((12 * nodecount) + stringtable.Length), stringtablepad))));

            writeout.Seek(0x10, SeekOrigin.Current);

            writeout.Write(nodes.ToArray());
            writeout.Write(ASCIIEncoding.ASCII.GetBytes(stringtable));

            writeout.Seek(Tools.AddPadding(0x20 + ((12 * nodecount) + stringtable.Length), stringtablepad), SeekOrigin.Begin);

            writeout.Write(data.ToArray());

            output.Seek(0, SeekOrigin.End);
            for (int i = (int)output.Position; i < Tools.AddPadding((int)output.Position, datapad); i++)
                output.WriteByte(0);

            writeout.Close();
            output.Close();

            return output.ToArray();
        }

        /// <summary>
        /// Unpacks the given U8 archive
        /// If the archive is Lz77 compressed, it will be decompressed first!
        /// </summary>
        /// <param name="u8archive"></param>
        /// <param name="unpackpath"></param>
        public static void UnpackU8(string u8archive, string unpackpath)
        {
            byte[] u8 = Wii.Tools.LoadFileToByteArray(u8archive);
            UnpackU8(u8, unpackpath);
        }

        /// <summary>
        /// Unpacks the given U8 archive
        /// If the archive is Lz77 compressed, it will be decompressed first!
        /// </summary>
        /// <param name="u8archive"></param>
        /// <param name="unpackpath"></param>
        public static void UnpackU8(byte[] u8archive, string unpackpath)
        {
            int lz77offset = Lz77.GetLz77Offset(u8archive);
            if (lz77offset != -1) { u8archive = Lz77.Decompress(u8archive, lz77offset); }

            if (unpackpath[unpackpath.Length - 1] != '\\') { unpackpath = unpackpath + "\\"; }
            if (!Directory.Exists(unpackpath)) Directory.CreateDirectory(unpackpath);

            int u8offset = -1;
            int length = 2500;
            if (u8archive.Length < length) length = u8archive.Length - 4;

            for (int i = 0; i < length; i++)
            {
                if (u8archive[i] == 0x55 && u8archive[i + 1] == 0xAA && u8archive[i + 2] == 0x38 && u8archive[i + 3] == 0x2D)
                {
                    u8offset = i;
                    break;
                }
            }

            if (u8offset == -1) throw new Exception("File is not a valid U8 Archive!");

            int nodecount = Tools.HexStringToInt(u8archive[u8offset + 0x28].ToString("x2") + u8archive[u8offset + 0x29].ToString("x2") + u8archive[u8offset + 0x2a].ToString("x2") + u8archive[u8offset + 0x2b].ToString("x2"));
            int nodeoffset = 0x20;

            string[,] nodes = new string[nodecount, 5];

            for (int j = 0; j < nodecount; j++)
            {
                nodes[j, 0] = u8archive[u8offset + nodeoffset].ToString("x2") + u8archive[u8offset + nodeoffset + 1].ToString("x2");
                nodes[j, 1] = u8archive[u8offset + nodeoffset + 2].ToString("x2") + u8archive[u8offset + nodeoffset + 3].ToString("x2");
                nodes[j, 2] = u8archive[u8offset + nodeoffset + 4].ToString("x2") + u8archive[u8offset + nodeoffset + 5].ToString("x2") + u8archive[u8offset + nodeoffset + 6].ToString("x2") + u8archive[u8offset + nodeoffset + 7].ToString("x2");
                nodes[j, 3] = u8archive[u8offset + nodeoffset + 8].ToString("x2") + u8archive[u8offset + nodeoffset + 9].ToString("x2") + u8archive[u8offset + nodeoffset + 10].ToString("x2") + u8archive[u8offset + nodeoffset + 11].ToString("x2");

                nodeoffset += 12;
            }

            int stringtablepos = u8offset + nodeoffset;

            for (int x = 0; x < nodecount; x++)
            {
                bool end = false;
                int nameoffset = Tools.HexStringToInt(nodes[x, 1]);
                string thisname = "";

                do
                {
                    if (u8archive[stringtablepos + nameoffset] != 0x00)
                    {
                        char tempchar = Convert.ToChar(u8archive[stringtablepos + nameoffset]);
                        thisname += tempchar.ToString();
                        nameoffset++;
                    }
                    else end = true;
                } while (end == false);

                nodes[x, 4] = thisname;
            }

            string[] dirs = new string[nodecount];
            dirs[0] = unpackpath;
            int[] dircount = new int[nodecount];
            int dirindex = 0;

            try
            {
                for (int y = 1; y < nodecount; y++)
                {
                    switch (nodes[y, 0])
                    {
                        case "0100":
                            if (dirs[dirindex][dirs[dirindex].Length - 1] != '\\') { dirs[dirindex] = dirs[dirindex] + "\\"; }
                            Directory.CreateDirectory(dirs[dirindex] + nodes[y, 4]);
                            dirs[dirindex + 1] = dirs[dirindex] + nodes[y, 4];
                            dirindex++;
                            dircount[dirindex] = Tools.HexStringToInt(nodes[y, 3]);
                            break;
                        default:
                            int filepos = u8offset + Tools.HexStringToInt(nodes[y, 2]);
                            int filesize = Tools.HexStringToInt(nodes[y, 3]);

                            using (FileStream fs = new FileStream(dirs[dirindex] + "\\" + nodes[y, 4], FileMode.Create))
                            {
                                fs.Write(u8archive, filepos, filesize);
                            }
                            break;
                    }

                    while (dirindex > 0 && dircount[dirindex] == (y + 1))
                    {
                        dirindex--;
                    }
                }
            }
            catch { }
        }

        /// <summary>
        /// Gets the Banner.bin out of the 00000000.app
        /// </summary>
        /// <param name="nullapp"></param>
        /// <returns></returns>
        public static byte[] GetBannerBin(byte[] nullapp)
        {
            int lz77offset = Lz77.GetLz77Offset(nullapp);
            if (lz77offset != -1) { nullapp = Lz77.Decompress(nullapp, lz77offset); }

            int u8offset = -1;
            for (int i = 0; i < 2500; i++)
            {
                if (nullapp[i] == 0x55 && nullapp[i + 1] == 0xAA && nullapp[i + 2] == 0x38 && nullapp[i + 3] == 0x2D)
                {
                    u8offset = i;
                    break;
                }
            }

            if (u8offset == -1) throw new Exception("File is not a valid U8 Archive!");

            int nodecount = Tools.HexStringToInt(nullapp[u8offset + 0x28].ToString("x2") + nullapp[u8offset + 0x29].ToString("x2") + nullapp[u8offset + 0x2a].ToString("x2") + nullapp[u8offset + 0x2b].ToString("x2"));
            int nodeoffset = 0x20;

            string[,] nodes = new string[nodecount, 5];

            for (int j = 0; j < nodecount; j++)
            {
                nodes[j, 0] = nullapp[u8offset + nodeoffset].ToString("x2") + nullapp[u8offset + nodeoffset + 1].ToString("x2");
                nodes[j, 1] = nullapp[u8offset + nodeoffset + 2].ToString("x2") + nullapp[u8offset + nodeoffset + 3].ToString("x2");
                nodes[j, 2] = nullapp[u8offset + nodeoffset + 4].ToString("x2") + nullapp[u8offset + nodeoffset + 5].ToString("x2") + nullapp[u8offset + nodeoffset + 6].ToString("x2") + nullapp[u8offset + nodeoffset + 7].ToString("x2");
                nodes[j, 3] = nullapp[u8offset + nodeoffset + 8].ToString("x2") + nullapp[u8offset + nodeoffset + 9].ToString("x2") + nullapp[u8offset + nodeoffset + 10].ToString("x2") + nullapp[u8offset + nodeoffset + 11].ToString("x2");

                nodeoffset += 12;
            }

            int stringtablepos = u8offset + nodeoffset;

            for (int x = 0; x < nodecount; x++)
            {
                bool end = false;
                int nameoffset = Tools.HexStringToInt(nodes[x, 1]);
                string thisname = "";

                while (end == false)
                {
                    if (nullapp[stringtablepos + nameoffset] != 0x00)
                    {
                        char tempchar = Convert.ToChar(nullapp[stringtablepos + nameoffset]);
                        thisname += tempchar.ToString();
                        nameoffset++;
                    }
                    else end = true;
                }

                nodes[x, 4] = thisname;
            }

            for (int y = 1; y < nodecount; y++)
            {
                if (nodes[y, 4] == "banner.bin")
                {
                    int filepos = u8offset + Tools.HexStringToInt(nodes[y, 2]);
                    int filesize = Tools.HexStringToInt(nodes[y, 3]);

                    MemoryStream ms = new MemoryStream(nullapp);
                    byte[] banner = new byte[filesize];
                    ms.Seek(filepos, SeekOrigin.Begin);
                    ms.Read(banner, 0, filesize);
                    ms.Close();

                    return banner;
                }
            }

            throw new Exception("This file doesn't contain banner.bin!");
        }

        /// <summary>
        /// Gets the Icon.bin out of the 00000000.app
        /// </summary>
        /// <param name="nullapp"></param>
        /// <returns></returns>
        public static byte[] GetIconBin(byte[] nullapp)
        {
            int lz77offset = Lz77.GetLz77Offset(nullapp);
            if (lz77offset != -1) { nullapp = Lz77.Decompress(nullapp, lz77offset); }

            int u8offset = -1;
            for (int i = 0; i < 2500; i++)
            {
                if (nullapp[i] == 0x55 && nullapp[i + 1] == 0xAA && nullapp[i + 2] == 0x38 && nullapp[i + 3] == 0x2D)
                {
                    u8offset = i;
                    break;
                }
            }

            if (u8offset == -1) throw new Exception("File is not a valid U8 Archive!");

            int nodecount = Tools.HexStringToInt(nullapp[u8offset + 0x28].ToString("x2") + nullapp[u8offset + 0x29].ToString("x2") + nullapp[u8offset + 0x2a].ToString("x2") + nullapp[u8offset + 0x2b].ToString("x2"));
            int nodeoffset = 0x20;

            string[,] nodes = new string[nodecount, 5];

            for (int j = 0; j < nodecount; j++)
            {
                nodes[j, 0] = nullapp[u8offset + nodeoffset].ToString("x2") + nullapp[u8offset + nodeoffset + 1].ToString("x2");
                nodes[j, 1] = nullapp[u8offset + nodeoffset + 2].ToString("x2") + nullapp[u8offset + nodeoffset + 3].ToString("x2");
                nodes[j, 2] = nullapp[u8offset + nodeoffset + 4].ToString("x2") + nullapp[u8offset + nodeoffset + 5].ToString("x2") + nullapp[u8offset + nodeoffset + 6].ToString("x2") + nullapp[u8offset + nodeoffset + 7].ToString("x2");
                nodes[j, 3] = nullapp[u8offset + nodeoffset + 8].ToString("x2") + nullapp[u8offset + nodeoffset + 9].ToString("x2") + nullapp[u8offset + nodeoffset + 10].ToString("x2") + nullapp[u8offset + nodeoffset + 11].ToString("x2");

                nodeoffset += 12;
            }

            int stringtablepos = u8offset + nodeoffset;

            for (int x = 0; x < nodecount; x++)
            {
                bool end = false;
                int nameoffset = Tools.HexStringToInt(nodes[x, 1]);
                string thisname = "";

                while (end == false)
                {
                    if (nullapp[stringtablepos + nameoffset] != 0x00)
                    {
                        char tempchar = Convert.ToChar(nullapp[stringtablepos + nameoffset]);
                        thisname += tempchar.ToString();
                        nameoffset++;
                    }
                    else end = true;
                }

                nodes[x, 4] = thisname;
            }

            for (int y = 1; y < nodecount; y++)
            {
                if (nodes[y, 4] == "icon.bin")
                {
                    int filepos = u8offset + Tools.HexStringToInt(nodes[y, 2]);
                    int filesize = Tools.HexStringToInt(nodes[y, 3]);

                    MemoryStream ms = new MemoryStream(nullapp);
                    byte[] icon = new byte[filesize];
                    ms.Seek(filepos, SeekOrigin.Begin);
                    ms.Read(icon, 0, filesize);
                    ms.Close();

                    return icon;
                }
            }

            throw new Exception("This file doesn't contain icon.bin!");
        }

        /// <summary>
        /// Extracts all Tpl's to the given path
        /// </summary>
        /// <param name="u8archive"></param>
        /// <param name="path"></param>
        public static void UnpackTpls(byte[] u8archive, string unpackpath)
        {
            int lz77offset = Lz77.GetLz77Offset(u8archive);
            if (lz77offset != -1) { u8archive = Lz77.Decompress(u8archive, lz77offset); }

            if (unpackpath[unpackpath.Length - 1] != '\\') { unpackpath = unpackpath + "\\"; }
            if (!Directory.Exists(unpackpath)) Directory.CreateDirectory(unpackpath);

            int u8offset = -1;
            int length = 2500;
            if (u8archive.Length < 2500) length = u8archive.Length - 4;

            for (int i = 0; i < 2500; i++)
            {
                if (u8archive[i] == 0x55 && u8archive[i + 1] == 0xAA && u8archive[i + 2] == 0x38 && u8archive[i + 3] == 0x2D)
                {
                    u8offset = i;
                    break;
                }
            }

            if (u8offset == -1) throw new Exception("File is not a valid U8 Archive!");

            int nodecount = Tools.HexStringToInt(u8archive[u8offset + 0x28].ToString("x2") + u8archive[u8offset + 0x29].ToString("x2") + u8archive[u8offset + 0x2a].ToString("x2") + u8archive[u8offset + 0x2b].ToString("x2"));
            int nodeoffset = 0x20;

            string[,] nodes = new string[nodecount, 5];

            for (int j = 0; j < nodecount; j++)
            {
                nodes[j, 0] = u8archive[u8offset + nodeoffset].ToString("x2") + u8archive[u8offset + nodeoffset + 1].ToString("x2");
                nodes[j, 1] = u8archive[u8offset + nodeoffset + 2].ToString("x2") + u8archive[u8offset + nodeoffset + 3].ToString("x2");
                nodes[j, 2] = u8archive[u8offset + nodeoffset + 4].ToString("x2") + u8archive[u8offset + nodeoffset + 5].ToString("x2") + u8archive[u8offset + nodeoffset + 6].ToString("x2") + u8archive[u8offset + nodeoffset + 7].ToString("x2");
                nodes[j, 3] = u8archive[u8offset + nodeoffset + 8].ToString("x2") + u8archive[u8offset + nodeoffset + 9].ToString("x2") + u8archive[u8offset + nodeoffset + 10].ToString("x2") + u8archive[u8offset + nodeoffset + 11].ToString("x2");

                nodeoffset += 12;
            }

            int stringtablepos = u8offset + nodeoffset;

            for (int x = 0; x < nodecount; x++)
            {
                bool end = false;
                int nameoffset = Tools.HexStringToInt(nodes[x, 1]);
                string thisname = "";

                while (end == false)
                {
                    if (u8archive[stringtablepos + nameoffset] != 0x00)
                    {
                        char tempchar = Convert.ToChar(u8archive[stringtablepos + nameoffset]);
                        thisname += tempchar.ToString();
                        nameoffset++;
                    }
                    else end = true;
                }

                nodes[x, 4] = thisname;
            }

            for (int y = 1; y < nodecount; y++)
            {
                if (nodes[y, 4].Contains("."))
                {
                    if (nodes[y, 4].Remove(0, nodes[y, 4].LastIndexOf('.')) == ".tpl")
                    {
                        int filepos = u8offset + Tools.HexStringToInt(nodes[y, 2]);
                        int filesize = Tools.HexStringToInt(nodes[y, 3]);

                        using (FileStream fs = new FileStream(unpackpath + nodes[y, 4], FileMode.Create))
                        {
                            fs.Write(u8archive, filepos, filesize);
                        }
                    }
                }
            }
        }
    }

    public class Lz77
    {
        private const int N = 4096;
        private const int F = 18;
        private const int threshold = 2;
        private static int[] lson = new int[N + 1];
        private static int[] rson = new int[N + 257];
        private static int[] dad = new int[N + 1];
        private static ushort[] text_buf = new ushort[N + 17];
        private static int match_position = 0, match_length = 0;
        private static int textsize = 0;
        private static int codesize = 0;

        /// <summary>
        /// Returns the Offset to the Lz77 Header
        /// -1 will be returned, if the file is not Lz77 compressed
        /// </summary>
        /// <param name="data"></param>
        /// <returns></returns>
        public static int GetLz77Offset(byte[] data)
        {
            int length = 5000;
            if (data.Length < 5000) length = data.Length - 4;

            for (int i = 0; i < length; i++)
            {
                if (data[i] == 0x55 && data[i + 1] == 0xAA && data[i + 2] == 0x38 && data[i + 3] == 0x2D)
                {
                    break;
                }

                UInt32 tmp = BitConverter.ToUInt32(data, i);
                if (tmp == 0x37375a4c) return i;
            }

            return -1;
        }

        /// <summary>
        /// Decompresses the given file
        /// </summary>
        /// <param name="infile"></param>
        /// <param name="outfile"></param>
        public static void Decompress(string infile, string outfile)
        {
            byte[] input = Tools.LoadFileToByteArray(infile);
            int offset = GetLz77Offset(input);
            if (offset == -1) throw new Exception("File is not Lz77 compressed!");
            Tools.SaveFileFromByteArray(Decompress(input, offset), outfile);
        }

        /// <summary>
        /// Decompresses the given data
        /// </summary>
        /// <param name="compressed"></param>
        /// <param name="offset"></param>
        /// <returns></returns>
        public static byte[] Decompress(byte[] compressed, int offset)
        {
            int i, j, k, r, c, z;
            uint flags;
            UInt32 decomp_size;
            UInt32 cur_size = 0;

            MemoryStream infile = new MemoryStream(compressed);
            MemoryStream outfile = new MemoryStream();

            UInt32 gbaheader = new UInt32();
            byte[] temp = new byte[4];
            infile.Seek(offset + 4, SeekOrigin.Begin);
            infile.Read(temp, 0, 4);
            gbaheader = BitConverter.ToUInt32(temp, 0);

            decomp_size = gbaheader >> 8;
            byte[] text_buf = new byte[N + 17];

            for (i = 0; i < N - F; i++) text_buf[i] = 0xdf;
            r = N - F; flags = 7; z = 7;

            while (true)
            {
                flags <<= 1;
                z++;
                if (z == 8)
                {
                    if ((c = (char)infile.ReadByte()) == -1) break;
                    flags = (uint)c;
                    z = 0;
                }
                if ((flags & 0x80) == 0)
                {
                    if ((c = infile.ReadByte()) == infile.Length - 1) break;
                    if (cur_size < decomp_size) outfile.WriteByte((byte)c);
                    text_buf[r++] = (byte)c;
                    r &= (N - 1);
                    cur_size++;
                }
                else
                {
                    if ((i = infile.ReadByte()) == -1) break;
                    if ((j = infile.ReadByte()) == -1) break;
                    j = j | ((i << 8) & 0xf00);
                    i = ((i >> 4) & 0x0f) + threshold;
                    for (k = 0; k <= i; k++)
                    {
                        c = text_buf[(r - j - 1) & (N - 1)];
                        if (cur_size < decomp_size) outfile.WriteByte((byte)c); text_buf[r++] = (byte)c; r &= (N - 1); cur_size++;
                    }
                }
            }

            return outfile.ToArray();
        }

        public static void InitTree()
        {
            int i;
            for (i = N + 1; i <= N + 256; i++) rson[i] = N;
            for (i = 0; i < N; i++) dad[i] = N;
        }

        public static void InsertNode(int r)
        {
            int i, p, cmp;
            cmp = 1;
            p = N + 1 + (text_buf[r] == 0xffff ? 0 : text_buf[r]); //text_buf[r];
            rson[r] = lson[r] = N; match_length = 0;
            for (; ; )
            {
                if (cmp >= 0)
                {
                    if (rson[p] != N) p = rson[p];
                    else { rson[p] = r; dad[r] = p; return; }
                }
                else
                {
                    if (lson[p] != N) p = lson[p];
                    else { lson[p] = r; dad[r] = p; return; }
                }
                for (i = 1; i < F; i++)
                    if ((cmp = text_buf[r + i] - text_buf[p + i]) != 0) break;
                if (i > match_length)
                {
                    match_position = p;
                    if ((match_length = i) >= F) break;
                }
            }
            dad[r] = dad[p]; lson[r] = lson[p]; rson[r] = rson[p];
            dad[lson[p]] = r; dad[rson[p]] = r;
            if (rson[dad[p]] == p) rson[dad[p]] = r;
            else lson[dad[p]] = r;
            dad[p] = N;
        }

        public static void DeleteNode(int p)
        {
            int q;

            if (dad[p] == N) return;  /* not in tree */
            if (rson[p] == N) q = lson[p];
            else if (lson[p] == N) q = rson[p];
            else
            {
                q = lson[p];
                if (rson[q] != N)
                {
                    do { q = rson[q]; } while (rson[q] != N);
                    rson[dad[q]] = lson[q]; dad[lson[q]] = dad[q];
                    lson[q] = lson[p]; dad[lson[p]] = q;
                }
                rson[q] = rson[p]; dad[rson[p]] = q;
            }
            dad[q] = dad[p];
            if (rson[dad[p]] == p) rson[dad[p]] = q; else lson[dad[p]] = q;
            dad[p] = N;
        }

        /// <summary>
        /// Lz77 compresses the given File
        /// </summary>
        /// <param name="file"></param>
        /// <returns></returns>
        public static void Compress(string infile, string outfile)
        {
            byte[] thisfile = Tools.LoadFileToByteArray(infile);
            thisfile = Compress(thisfile);
            Tools.SaveFileFromByteArray(thisfile, outfile);
        }

        /// <summary>
        /// Lz77 compresses the given and saves it to the given Path
        /// </summary>
        /// <param name="file"></param>
        /// <returns></returns>
        public static void Compress(byte[] file, string outfile)
        {
            byte[] temp = Compress(file);
            Tools.SaveFileFromByteArray(temp, outfile);
        }

        /// <summary>
        /// Lz77 compresses the given Byte Array
        /// </summary>
        /// <param name="file"></param>
        /// <returns></returns>
        public static byte[] Compress(byte[] file)
        {
            int i, c, len, r, s, last_match_length, code_buf_ptr;
            int[] code_buf = new int[17];
            int mask;
            UInt32 filesize = ((Convert.ToUInt32(file.Length)) << 8) + 0x10;
            byte[] filesizebytes = BitConverter.GetBytes(filesize);

            MemoryStream output = new MemoryStream();
            output.WriteByte((byte)'L'); output.WriteByte((byte)'Z'); output.WriteByte((byte)'7'); output.WriteByte((byte)'7');
            MemoryStream infile = new MemoryStream(file);

            output.Write(filesizebytes, 0, filesizebytes.Length);

            InitTree();
            code_buf[0] = 0;
            code_buf_ptr = 1;
            mask = 0x80;
            s = 0;
            r = N - F;

            for (i = s; i < r; i++) text_buf[i] = 0xffff;
            for (len = 0; len < F && (c = (int)infile.ReadByte()) != -1; len++)
                text_buf[r + len] = (ushort)c;

            if ((textsize = len) == 0) return file;

            for (i = 1; i <= F; i++) InsertNode(r - i);

            InsertNode(r);

            do
            {
                if (match_length > len) match_length = len;

                if (match_length <= threshold)
                {
                    match_length = 1;
                    code_buf[code_buf_ptr++] = text_buf[r];
                }
                else
                {
                    code_buf[0] |= mask;

                    code_buf[code_buf_ptr++] = (char)
                        (((r - match_position - 1) >> 8) & 0x0f) |
                        ((match_length - (threshold + 1)) << 4);

                    code_buf[code_buf_ptr++] = (char)((r - match_position - 1) & 0xff);
                }
                if ((mask >>= 1) == 0)
                {
                    for (i = 0; i < code_buf_ptr; i++)
                        output.WriteByte((byte)code_buf[i]);
                    codesize += code_buf_ptr;
                    code_buf[0] = 0; code_buf_ptr = 1;
                    mask = 0x80;
                }

                last_match_length = match_length;
                for (i = 0; i < last_match_length &&
                        (c = (int)infile.ReadByte()) != -1; i++)
                {
                    DeleteNode(s);
                    text_buf[s] = (ushort)c;
                    if (s < F - 1) text_buf[s + N] = (ushort)c;
                    s = (s + 1) & (N - 1); r = (r + 1) & (N - 1);
                    InsertNode(r);
                }

                while (i++ < last_match_length)
                {
                    DeleteNode(s);
                    s = (s + 1) & (N - 1); r = (r + 1) & (N - 1);
                    if (--len != 0) InsertNode(r);
                }
            } while (len > 0);


            if (code_buf_ptr > 1)
            {
                for (i = 0; i < code_buf_ptr; i++) output.WriteByte((byte)code_buf[i]);
                codesize += code_buf_ptr;
            }

            if (codesize % 4 != 0)
                for (i = 0; i < 4 - (codesize % 4); i++)
                    output.WriteByte(0x00);

            infile.Close();
            return output.ToArray();
        }
    }

}
