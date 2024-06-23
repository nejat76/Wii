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
    public class WadInfo
    {
        public const int Headersize = 64;
        public static string[] RegionCode = new string[4] { "Japan", "USA", "Europe", "Region Free" };

        /// <summary>
        /// Returns the Header of a Wadfile
        /// </summary>
        /// <param name="wadfile"></param>
        /// <returns></returns>
        public static byte[] GetHeader(byte[] wadfile)
        {
            byte[] Header = new byte[0x20];

            for (int i = 0; i < Header.Length; i++)
            {
                Header[i] = wadfile[i];
            }

            return Header;
        }

        /// <summary>
        /// Returns the size of the Certificate
        /// </summary>
        /// <param name="wadfile"></param>
        /// <returns></returns>
        public static int GetCertSize(byte[] wadfile)
        {
            int size = int.Parse(wadfile[0x08].ToString("x2") + wadfile[0x09].ToString("x2") + wadfile[0x0a].ToString("x2") + wadfile[0x0b].ToString("x2"), System.Globalization.NumberStyles.HexNumber);
            return size;
        }

        /// <summary>
        /// Returns the size of the Ticket
        /// </summary>
        /// <param name="wadfile"></param>
        /// <returns></returns>
        public static int GetTikSize(byte[] wadfile)
        {
            int size = int.Parse(wadfile[0x10].ToString("x2") + wadfile[0x11].ToString("x2") + wadfile[0x12].ToString("x2") + wadfile[0x13].ToString("x2"), System.Globalization.NumberStyles.HexNumber);
            return size;
        }

        /// <summary>
        /// Returns the size of the TMD
        /// </summary>
        /// <param name="wadfile"></param>
        /// <returns></returns>
        public static int GetTmdSize(byte[] wadfile)
        {
            int size = int.Parse(wadfile[0x14].ToString("x2") + wadfile[0x15].ToString("x2") + wadfile[0x16].ToString("x2") + wadfile[0x17].ToString("x2"), System.Globalization.NumberStyles.HexNumber);
            return size;
        }

        /// <summary>
        /// Returns the size of all Contents
        /// </summary>
        /// <param name="wadfile"></param>
        /// <returns></returns>
        public static int GetContentSize(byte[] wadfile)
        {
            int size = int.Parse(wadfile[0x18].ToString("x2") + wadfile[0x19].ToString("x2") + wadfile[0x1a].ToString("x2") + wadfile[0x1b].ToString("x2"), System.Globalization.NumberStyles.HexNumber);
            return size;
        }

        /// <summary>
        /// Returns the size of the Footer
        /// </summary>
        /// <param name="wadfile"></param>
        /// <returns></returns>
        public static int GetFooterSize(byte[] wadfile)
        {
            int size = int.Parse(wadfile[0x1c].ToString("x2") + wadfile[0x1d].ToString("x2") + wadfile[0x1e].ToString("x2") + wadfile[0x1f].ToString("x2"), System.Globalization.NumberStyles.HexNumber);
            return size;
        }

        /// <summary>
        /// Returns the position of the tmd in the wad file
        /// </summary>
        /// <param name="wadfile"></param>
        /// <returns></returns>
        public static int GetTmdPos(byte[] wadfile)
        {
            return Headersize + Tools.AddPadding(GetCertSize(wadfile)) + Tools.AddPadding(GetTikSize(wadfile));
        }

        /// <summary>
        /// Returns the position of the ticket in the wad file, ticket or tmd
        /// </summary>
        /// <param name="wadfile"></param>
        /// <returns></returns>
        public static int GetTikPos(byte[] wadfile)
        {
            return Headersize + Tools.AddPadding(GetCertSize(wadfile));
        }

        /// <summary>
        /// Returns the title ID of the wad file.
        /// </summary>
        /// <param name="wadfile"></param>
        /// <param name="type">0 = Tik, 1 = Tmd</param>
        /// <returns></returns>
        public static string GetTitleID(string wadtiktmd, int type)
        {
            byte[] temp = Tools.LoadFileToByteArray(wadtiktmd);
            return GetTitleID(temp, type);
        }

        /// <summary>
        /// Returns the title ID of the wad file.
        /// </summary>
        /// <param name="wadfile"></param>
        /// <param name="type">0 = Tik, 1 = Tmd</param>
        /// <returns></returns>
        public static string GetTitleID(byte[] wadtiktmd, int type)
        {
            string channeltype = GetChannelType(wadtiktmd, type);
            int tikpos = 0;
            int tmdpos = 0;

            if (IsThisWad(wadtiktmd) == true)
            {
                //It's a wad
                tikpos = GetTikPos(wadtiktmd);
                tmdpos = GetTmdPos(wadtiktmd);
            }

            if (type == 1)
            {
                if (!channeltype.Contains("System:"))
                {
                    string tmdid = Convert.ToChar(wadtiktmd[tmdpos + 0x190]).ToString() + Convert.ToChar(wadtiktmd[tmdpos + 0x191]).ToString() + Convert.ToChar(wadtiktmd[tmdpos + 0x192]).ToString() + Convert.ToChar(wadtiktmd[tmdpos + 0x193]).ToString();
                    return tmdid;
                }
                else if (channeltype.Contains("IOS"))
                {
                    int tmdid = Tools.HexStringToInt(wadtiktmd[tmdpos + 0x190].ToString("x2") + wadtiktmd[tmdpos + 0x191].ToString("x2") + wadtiktmd[tmdpos + 0x192].ToString("x2") + wadtiktmd[tmdpos + 0x193].ToString("x2"));
                    return "IOS" + tmdid;
                }
                else if (channeltype.Contains("System")) return "SYSTEM";
                else return "";
            }
            else
            {
                if (!channeltype.Contains("System:"))
                {
                    string tikid = Convert.ToChar(wadtiktmd[tikpos + 0x1e0]).ToString() + Convert.ToChar(wadtiktmd[tikpos + 0x1e1]).ToString() + Convert.ToChar(wadtiktmd[tikpos + 0x1e2]).ToString() + Convert.ToChar(wadtiktmd[tikpos + 0x1e3]).ToString();
                    return tikid;
                }
                else if (channeltype.Contains("IOS"))
                {
                    int tikid = Tools.HexStringToInt(wadtiktmd[tikpos + 0x1e0].ToString("x2") + wadtiktmd[tikpos + 0x1e1].ToString("x2") + wadtiktmd[tikpos + 0x1e2].ToString("x2") + wadtiktmd[tikpos + 0x1e3].ToString("x2"));
                    return "IOS" + tikid;
                }
                else if (channeltype.Contains("System")) return "SYSTEM";
                else return "";
            }
        }

        /// <summary>
        /// Returns the full title ID of the wad file as a hex string.
        /// </summary>
        /// <param name="wadfile"></param>
        /// <param name="type">0 = Tik, 1 = Tmd</param>
        /// <returns></returns>
        public static string GetFullTitleID(byte[] wadtiktmd, int type)
        {
            int tikpos = 0;
            int tmdpos = 0;

            if (IsThisWad(wadtiktmd) == true)
            {
                //It's a wad
                tikpos = GetTikPos(wadtiktmd);
                tmdpos = GetTmdPos(wadtiktmd);
            }

            if (type == 1)
            {
                string tmdid = wadtiktmd[tmdpos + 0x18c].ToString("x2") +
                    wadtiktmd[tmdpos + 0x18d].ToString("x2") +
                    wadtiktmd[tmdpos + 0x18e].ToString("x2") +
                    wadtiktmd[tmdpos + 0x18f].ToString("x2") +
                    wadtiktmd[tmdpos + 0x190].ToString("x2") +
                    wadtiktmd[tmdpos + 0x191].ToString("x2") +
                    wadtiktmd[tmdpos + 0x192].ToString("x2") +
                    wadtiktmd[tmdpos + 0x193].ToString("x2");
                return tmdid;
            }
            else
            {
                string tikid = wadtiktmd[tikpos + 0x1dc].ToString() +
                    wadtiktmd[tikpos + 0x1dd].ToString() +
                    wadtiktmd[tikpos + 0x1de].ToString() +
                    wadtiktmd[tikpos + 0x1df].ToString() +
                    wadtiktmd[tikpos + 0x1e0].ToString() +
                    wadtiktmd[tikpos + 0x1e1].ToString() +
                    wadtiktmd[tikpos + 0x1e2].ToString() +
                    wadtiktmd[tikpos + 0x1e3].ToString();
                return tikid;
            }
        }

        /// <summary>
        /// Returns the title for each language of a wad file.
        /// Order: Jap, Eng, Ger, Fra, Spa, Ita, Dut
        /// </summary>
        /// <param name="wadfile"></param>
        /// <returns></returns>
        public static string[] GetChannelTitles(string wadfile)
        {
            byte[] wadarray = Tools.LoadFileToByteArray(wadfile);
            return GetChannelTitles(wadarray);
        }

        /// <summary>
        /// Returns the title for each language of a wad file.
        /// Order: Jap, Eng, Ger, Fra, Spa, Ita, Dut
        /// </summary>
        /// <param name="wadfile"></param>
        /// <returns></returns>
        public static string[] GetChannelTitles(byte[] wadfile)
        {
            if (File.Exists(System.Windows.Forms.Application.StartupPath + "\\common-key.bin") || File.Exists(System.Windows.Forms.Application.StartupPath + "\\key.bin"))
            {
                string channeltype = GetChannelType(wadfile, 0);

                if (!channeltype.Contains("System:"))
                {
                    if (!channeltype.Contains("Hidden"))
                    {
                        string[] titles = new string[7];

                        string[,] conts = GetContentInfo(wadfile);
                        byte[] titlekey = GetTitleKey(wadfile);
                        int nullapp = 0;

                        for (int i = 0; i < conts.GetLength(0); i++)
                        {
                            if (conts[i, 1] == "00000000")
                                nullapp = i;
                        }

                        byte[] contenthandle = WadEdit.DecryptContent(wadfile, nullapp, titlekey);
                        int imetpos = 0;

                        if (contenthandle.Length < 400) return new string[7];

                        if (!channeltype.Contains("Downloaded"))
                        {
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

                            int jappos = imetpos + 29;
                            int count = 0;

                            for (int i = jappos; i < jappos + 588; i += 84)
                            {
                                for (int j = 0; j < 40; j += 2)
                                {
                                    if (contenthandle[i + j] != 0x00)
                                    {
                                        char temp = BitConverter.ToChar(new byte[] { contenthandle[i + j], contenthandle[i + j - 1] }, 0);
                                        titles[count] += temp;
                                    }
                                }

                                count++;
                            }

                            return titles;
                        }
                        else
                        {
                            //DLC's
                            for (int j = 97; j < 97 + 40; j += 2)
                            {
                                if (contenthandle[j] != 0x00)
                                {
                                    char temp = BitConverter.ToChar(new byte[] { contenthandle[j], contenthandle[j - 1] }, 0);
                                    titles[0] += temp;
                                }
                            }

                            for (int i = 1; i < 7; i++)
                                titles[i] = titles[0];

                            return titles;
                        }
                    }
                    else return new string[7];
                }
                else return new string[7];
            }
            else return new string[7];
        }

        /// <summary>
        /// Returns the title for each language of a 00.app file
        /// Order: Jap, Eng, Ger, Fra, Spa, Ita, Dut
        /// </summary>
        /// <param name="app"></param>
        /// <returns></returns>
        public static string[] GetChannelTitlesFromApp(string app)
        {
            byte[] tmp = Tools.LoadFileToByteArray(app);
            return GetChannelTitlesFromApp(tmp);
        }

        /// <summary>
        /// Returns the title for each language of a 00.app file
        /// Order: Jap, Eng, Ger, Fra, Spa, Ita, Dut
        /// </summary>
        /// <param name="app"></param>
        /// <returns></returns>
        public static string[] GetChannelTitlesFromApp(byte[] app)
        {
            string[] titles = new string[7];

            int imetpos = 0;
            int length = 400;

            if (app.Length < 400) length = app.Length - 4;

            for (int z = 0; z < length; z++)
            {
                if (Convert.ToChar(app[z]) == 'I')
                    if (Convert.ToChar(app[z + 1]) == 'M')
                        if (Convert.ToChar(app[z + 2]) == 'E')
                            if (Convert.ToChar(app[z + 3]) == 'T')
                            {
                                imetpos = z;
                                break;
                            }
            }

            if (imetpos != 0)
            {
                int jappos = imetpos + 29;
                int count = 0;

                for (int i = jappos; i < jappos + 588; i += 84)
                {
                    for (int j = 0; j < 40; j += 2)
                    {
                        if (app[i + j] != 0x00)
                        {
                            char temp = BitConverter.ToChar(new byte[] { app[i + j], app[i + j - 1] }, 0);
                            titles[count] += temp;
                        }
                    }

                    count++;
                }
            }

            return titles;
        }

        /// <summary>
        /// Returns the Type of the Channel as a string
        /// Wad or Tik needed for WiiWare / VC detection!
        /// </summary>
        /// <param name="wadfile"></param>
        /// <returns></returns>
        public static string GetChannelType(byte[] wadtiktmd, int type)
        {
            int tikpos = 0;
            int tmdpos = 0;

            if (IsThisWad(wadtiktmd) == true)
            {
                //It's a wad
                tikpos = GetTikPos(wadtiktmd);
                tmdpos = GetTmdPos(wadtiktmd);
            }

            string thistype = "";

            if (type == 0)
            { thistype = wadtiktmd[tikpos + 0x1dc].ToString("x2") + wadtiktmd[tikpos + 0x1dd].ToString("x2") + wadtiktmd[tikpos + 0x1de].ToString("x2") + wadtiktmd[tikpos + 0x1df].ToString("x2"); }
            else { thistype = wadtiktmd[tmdpos + 0x18c].ToString("x2") + wadtiktmd[tmdpos + 0x18d].ToString("x2") + wadtiktmd[tmdpos + 0x18e].ToString("x2") + wadtiktmd[tmdpos + 0x18f].ToString("x2"); }
            string channeltype = "Unknown";

            if (thistype == "00010001")
            {
                channeltype = CheckWiiWareVC(wadtiktmd, type);
            }
            else if (thistype == "00010002") channeltype = "System Channel";
            else if (thistype == "00010004" || thistype == "00010000") channeltype = "Game Channel";
            else if (thistype == "00010005") channeltype = "Downloaded Content";
            else if (thistype == "00010008") channeltype = "Hidden Channel";
            else if (thistype == "00000001")
            {
                channeltype = "System: IOS";

                string thisid = "";
                if (type == 0) { thisid = wadtiktmd[tikpos + 0x1e0].ToString("x2") + wadtiktmd[tikpos + 0x1e1].ToString("x2") + wadtiktmd[tikpos + 0x1e2].ToString("x2") + wadtiktmd[tikpos + 0x1e3].ToString("x2"); }
                else { thisid = wadtiktmd[tmdpos + 0x190].ToString("x2") + wadtiktmd[tmdpos + 0x191].ToString("x2") + wadtiktmd[tmdpos + 0x192].ToString("x2") + wadtiktmd[tmdpos + 0x193].ToString("x2"); }

                if (thisid == "00000001") channeltype = "System: Boot2";
                else if (thisid == "00000002") channeltype = "System: Menu";
                else if (thisid == "00000100") channeltype = "System: BC";
                else if (thisid == "00000101") channeltype = "System: MIOS";
            }

            return channeltype;
        }

        /// <summary>
        /// Returns the amount of included Contents (app-files)
        /// </summary>
        /// <param name="wadfile"></param>
        /// <returns></returns>
        public static int GetContentNum(byte[] wadtmd)
        {
            int tmdpos = 0;

            if (IsThisWad(wadtmd) == true)
            {
                //It's a wad file, so get the tmd position
                tmdpos = GetTmdPos(wadtmd);
            }

            int contents = Tools.HexStringToInt(wadtmd[tmdpos + 0x1de].ToString("x2") + wadtmd[tmdpos + 0x1df].ToString("x2"));

            return contents;
        }

        /// <summary>
        /// Returns the boot index specified in the tmd
        /// </summary>
        /// <param name="wadfile"></param>
        /// <returns></returns>
        public static int GetBootIndex(byte[] wadtmd)
        {
            int tmdpos = 0;

            if (IsThisWad(wadtmd))
                tmdpos = GetTmdPos(wadtmd);

            int bootIndex = Tools.HexStringToInt(wadtmd[tmdpos + 0x1e0].ToString("x2") + wadtmd[tmdpos + 0x1e1].ToString("x2"));

            return bootIndex;
        }

        /// <summary>
        /// Returns the approx. destination size on the Wii
        /// </summary>
        /// <param name="wadfile"></param>
        /// <returns></returns>
        public static string GetNandSize(byte[] wadtmd, bool ConvertToMB)
        {
            int tmdpos = 0;
            int minsize = 0;
            int maxsize = 0;
            int numcont = GetContentNum(wadtmd);

            if (IsThisWad(wadtmd) == true)
            {
                //It's a wad
                tmdpos = GetTmdPos(wadtmd);
            }

            for (int i = 0; i < numcont; i++)
            {
                int cont = 36 * i;
                int contentsize = Tools.HexStringToInt(wadtmd[tmdpos + 0x1e4 + 8 + cont].ToString("x2") +
                    wadtmd[tmdpos + 0x1e5 + 8 + cont].ToString("x2") +
                    wadtmd[tmdpos + 0x1e6 + 8 + cont].ToString("x2") +
                    wadtmd[tmdpos + 0x1e7 + 8 + cont].ToString("x2") +
                    wadtmd[tmdpos + 0x1e8 + 8 + cont].ToString("x2") +
                    wadtmd[tmdpos + 0x1e9 + 8 + cont].ToString("x2") +
                    wadtmd[tmdpos + 0x1ea + 8 + cont].ToString("x2") +
                    wadtmd[tmdpos + 0x1eb + 8 + cont].ToString("x2"));

                string type = wadtmd[tmdpos + 0x1e4 + 6 + cont].ToString("x2") + wadtmd[tmdpos + 0x1e5 + 6 + cont].ToString("x2");

                if (type == "0001")
                {
                    minsize += contentsize;
                    maxsize += contentsize;
                }
                else if (type == "8001")
                    maxsize += contentsize;
            }

            string size = "";

            if (maxsize == minsize) size = maxsize.ToString();
            else size = minsize.ToString() + " - " + maxsize.ToString();

            if (ConvertToMB == true)
            {
                if (size.Contains("-"))
                {
                    string min = size.Remove(size.IndexOf(' '));
                    string max = size.Remove(0, size.IndexOf('-') + 2);

                    min = Convert.ToString(Math.Round(Convert.ToDouble(min) * 0.0009765625 * 0.0009765625, 2));
                    max = Convert.ToString(Math.Round(Convert.ToDouble(max) * 0.0009765625 * 0.0009765625, 2));
                    if (min.Length > 4) { min = min.Remove(4); }
                    if (max.Length > 4) { max = max.Remove(4); }
                    size = min + " - " + max + " MB";
                }
                else
                {
                    size = Convert.ToString(Math.Round(Convert.ToDouble(size) * 0.0009765625 * 0.0009765625, 2));
                    if (size.Length > 4) { size = size.Remove(4); }
                    size = size + " MB";
                }
            }

            return size.Replace(",", ".");
        }

        /// <summary>
        /// Returns the approx. destination block on the Wii
        /// </summary>
        /// <param name="wadfile"></param>
        /// <returns></returns>
        public static string GetNandBlocks(string wadtmd)
        {
            using (FileStream fs = new FileStream(wadtmd, FileMode.Open))
            {
                byte[] temp = new byte[fs.Length];
                fs.Read(temp, 0, temp.Length);
                return GetNandBlocks(temp);
            }
        }

        /// <summary>
        /// Returns the approx. destination block on the Wii
        /// </summary>
        /// <param name="wadfile"></param>
        /// <returns></returns>
        public static string GetNandBlocks(byte[] wadtmd)
        {
            string size = GetNandSize(wadtmd, false);

            if (size.Contains("-"))
            {
                string size1 = size.Remove(size.IndexOf(' '));
                string size2 = size.Remove(0, size.LastIndexOf(' ') + 1);

                double blocks1 = (double)((Convert.ToDouble(size1) / 1024) / 128);
                double blocks2 = (double)((Convert.ToDouble(size2) / 1024) / 128);

                return Math.Ceiling(blocks1) + " - " + Math.Ceiling(blocks2);
            }
            else
            {
                double blocks = (double)((Convert.ToDouble(size) / 1024) / 128);

                return Math.Ceiling(blocks).ToString();
            }
        }

        /// <summary>
        /// Returns the title version of the wad file
        /// </summary>
        /// <param name="wadfile"></param>
        /// <returns></returns>
        public static int GetTitleVersion(string wadtmd)
        {
            byte[] temp = Tools.LoadFileToByteArray(wadtmd, 0, 10000);
            return GetTitleVersion(temp);
        }

        /// <summary>
        /// Returns the title version of the wad file
        /// </summary>
        /// <param name="wadfile"></param>
        /// <returns></returns>
        public static int GetTitleVersion(byte[] wadtmd)
        {
            int tmdpos = 0;

            if (IsThisWad(wadtmd) == true) { tmdpos = GetTmdPos(wadtmd); }
            return Tools.HexStringToInt(wadtmd[tmdpos + 0x1dc].ToString("x2") + wadtmd[tmdpos + 0x1dd].ToString("x2"));
        }

        /// <summary>
        /// Returns the IOS that is needed by the wad file
        /// </summary>
        /// <param name="wadfile"></param>
        /// <returns></returns>
        public static string GetIosFlag(byte[] wadtmd)
        {
            string type = GetChannelType(wadtmd, 1);

            if (!type.Contains("IOS") && !type.Contains("BC"))
            {
                int tmdpos = 0;
                if (IsThisWad(wadtmd) == true) { tmdpos = GetTmdPos(wadtmd); }
                return "IOS" + Tools.HexStringToInt(wadtmd[tmdpos + 0x188].ToString("x2") + wadtmd[tmdpos + 0x189].ToString("x2") + wadtmd[tmdpos + 0x18a].ToString("x2") + wadtmd[tmdpos + 0x18b].ToString("x2"));
            }
            else return "";
        }

        /// <summary>
        /// Returns the region of the wad file
        /// </summary>
        /// <param name="wadfile"></param>
        /// <returns></returns>
        public static string GetRegionFlag(byte[] wadtmd)
        {
            int tmdpos = 0;
            string channeltype = GetChannelType(wadtmd, 1);

            if (IsThisWad(wadtmd) == true) { tmdpos = GetTmdPos(wadtmd); }

            if (!channeltype.Contains("System:"))
            {
                int region = Tools.HexStringToInt(wadtmd[tmdpos + 0x19d].ToString("x2"));
                return RegionCode[region];
            }
            else return "";
        }

        /// <summary>
        /// Returns the Path where the wad will be installed on the Wii
        /// </summary>
        /// <param name="wadfile"></param>
        /// <returns></returns>
        public static string GetNandPath(string wadfile)
        {
            byte[] wad = Tools.LoadFileToByteArray(wadfile);
            return GetNandPath(wad, 0);
        }

        /// <summary>
        /// Returns the Path where the wad will be installed on the Wii
        /// </summary>
        /// <param name="wadfile"></param>
        /// <param name="type">0 = Tik, 1 = Tmd</param>
        /// <returns></returns>
        public static string GetNandPath(byte[] wadtiktmd, int type)
        {
            int tikpos = 0;
            int tmdpos = 0;

            if (IsThisWad(wadtiktmd) == true)
            {
                tikpos = GetTikPos(wadtiktmd);
                tmdpos = GetTmdPos(wadtiktmd);
            }

            string thispath = "";

            if (type == 0)
            {
                thispath = wadtiktmd[tikpos + 0x1dc].ToString("x2") +
                    wadtiktmd[tikpos + 0x1dd].ToString("x2") +
                    wadtiktmd[tikpos + 0x1de].ToString("x2") +
                    wadtiktmd[tikpos + 0x1df].ToString("x2") +
                    wadtiktmd[tikpos + 0x1e0].ToString("x2") +
                    wadtiktmd[tikpos + 0x1e1].ToString("x2") +
                    wadtiktmd[tikpos + 0x1e2].ToString("x2") +
                    wadtiktmd[tikpos + 0x1e3].ToString("x2");
            }
            else
            {
                thispath = wadtiktmd[tmdpos + 0x18c].ToString("x2") +
                    wadtiktmd[tmdpos + 0x18d].ToString("x2") +
                    wadtiktmd[tmdpos + 0x18e].ToString("x2") +
                    wadtiktmd[tmdpos + 0x18f].ToString("x2") +
                    wadtiktmd[tmdpos + 0x190].ToString("x2") +
                    wadtiktmd[tmdpos + 0x191].ToString("x2") +
                    wadtiktmd[tmdpos + 0x192].ToString("x2") +
                    wadtiktmd[tmdpos + 0x193].ToString("x2");
            }

            thispath = thispath.Insert(8, "\\");
            return thispath;
        }

        /// <summary>
        /// Returns true, if the wad file is a WiiWare / VC title.
        /// </summary>
        /// <param name="wadtiktmd"></param>
        /// <param name="type">0 = Tik, 1 = Tmd</param>
        /// <returns></returns>
        public static string CheckWiiWareVC(byte[] wadtiktmd, int type)
        {
            int tiktmdpos = 0;
            int offset = 0x221;
            int idoffset = 0x1e0;

            if (type == 1) { offset = 0x197; idoffset = 0x190; }
            if (IsThisWad(wadtiktmd) == true)
            {
                if (type == 1) tiktmdpos = GetTmdPos(wadtiktmd);
                else tiktmdpos = GetTikPos(wadtiktmd);
            }

            if (wadtiktmd[tiktmdpos + offset] == 0x01)
            {
                char idchar = Convert.ToChar(wadtiktmd[tiktmdpos + idoffset]);
                char idchar2 = Convert.ToChar(wadtiktmd[tiktmdpos + idoffset + 1]);

                if (idchar == 'H') return "System Channel";
                else if (idchar == 'W') return "WiiWare";
                else
                {
                    if (idchar == 'C') return "C64";
                    else if (idchar == 'E' && idchar2 == 'A') return "NeoGeo";
                    else if (idchar == 'E') return "VC - Arcade";
                    else if (idchar == 'F') return "NES";
                    else if (idchar == 'J') return "SNES";
                    else if (idchar == 'L') return "Sega Master System";
                    else if (idchar == 'M') return "Sega Genesis";
                    else if (idchar == 'N') return "Nintendo 64";
                    else if (idchar == 'P') return "Turbografx";
                    else if (idchar == 'Q') return "Turbografx CD";
                    else return "Channel Title";
                }
            }
            else return "Channel Title";
        }

        /// <summary>
        /// Returns all information stored in the tmd for all contents in the wad file.
        /// [x, 0] = Content ID, [x, 1] = Index, [x, 2] = Type, [x, 3] = Size, [x, 4] = Sha1
        /// </summary>
        /// <param name="wadfile"></param>
        /// <returns></returns>
        public static string[,] GetContentInfo(byte[] wadtmd)
        {
            int tmdpos = 0;

            if (IsThisWad(wadtmd) == true) { tmdpos = GetTmdPos(wadtmd); }
            int contentcount = GetContentNum(wadtmd);
            string[,] contentinfo = new string[contentcount, 5];

            for (int i = 0; i < contentcount; i++)
            {
                contentinfo[i, 0] = wadtmd[tmdpos + 0x1e4 + (36 * i)].ToString("x2") +
                    wadtmd[tmdpos + 0x1e5 + (36 * i)].ToString("x2") +
                    wadtmd[tmdpos + 0x1e6 + (36 * i)].ToString("x2") +
                    wadtmd[tmdpos + 0x1e7 + (36 * i)].ToString("x2");
                contentinfo[i, 1] = "0000" +
                    wadtmd[tmdpos + 0x1e8 + (36 * i)].ToString("x2") +
                    wadtmd[tmdpos + 0x1e9 + (36 * i)].ToString("x2");
                contentinfo[i, 2] = wadtmd[tmdpos + 0x1ea + (36 * i)].ToString("x2") +
                    wadtmd[tmdpos + 0x1eb + (36 * i)].ToString("x2");
                contentinfo[i, 3] = Tools.HexStringToInt(
                    wadtmd[tmdpos + 0x1ec + (36 * i)].ToString("x2") +
                    wadtmd[tmdpos + 0x1ed + (36 * i)].ToString("x2") +
                    wadtmd[tmdpos + 0x1ee + (36 * i)].ToString("x2") +
                    wadtmd[tmdpos + 0x1ef + (36 * i)].ToString("x2") +
                    wadtmd[tmdpos + 0x1f0 + (36 * i)].ToString("x2") +
                    wadtmd[tmdpos + 0x1f1 + (36 * i)].ToString("x2") +
                    wadtmd[tmdpos + 0x1f2 + (36 * i)].ToString("x2") +
                    wadtmd[tmdpos + 0x1f3 + (36 * i)].ToString("x2")).ToString();

                for (int j = 0; j < 20; j++)
                {
                    contentinfo[i, 4] += wadtmd[tmdpos + 0x1f4 + (36 * i) + j].ToString("x2");
                }
            }

            return contentinfo;
        }

        /// <summary>
        /// Returns the Tik of the wad file as a Byte-Array
        /// </summary>
        /// <param name="wadfile"></param>
        /// <returns></returns>
        public static byte[] ReturnTik(byte[] wadfile)
        {
            int tikpos = GetTikPos(wadfile);
            int tiksize = GetTikSize(wadfile);

            byte[] tik = new byte[tiksize];

            for (int i = 0; i < tiksize; i++)
            {
                tik[i] = wadfile[tikpos + i];
            }

            return tik;
        }

        /// <summary>
        /// Returns the Tmd of the wad file as a Byte-Array
        /// </summary>
        /// <param name="wadfile"></param>
        /// <returns></returns>
        public static byte[] ReturnTmd(byte[] wadfile)
        {
            int tmdpos = GetTmdPos(wadfile);
            int tmdsize = GetTmdSize(wadfile);

            byte[] tmd = new byte[tmdsize];

            for (int i = 0; i < tmdsize; i++)
            {
                tmd[i] = wadfile[tmdpos + i];
            }

            return tmd;
        }

        /// <summary>
        /// Checks, if the given file is a wad
        /// </summary>
        /// <param name="wadtiktmd"></param>
        /// <returns></returns>
        public static bool IsThisWad(byte[] wadtiktmd)
        {
            if (wadtiktmd[0] == 0x00 &&
                wadtiktmd[1] == 0x00 &&
                wadtiktmd[2] == 0x00 &&
                wadtiktmd[3] == 0x20 &&
                wadtiktmd[4] == 0x49 &&
                wadtiktmd[5] == 0x73)
            { return true; }

            return false;
        }

        /// <summary>
        /// Returns the decrypted TitleKey
        /// </summary>
        /// <param name="wadtik"></param>
        /// <returns></returns>
        public static byte[] GetTitleKey(byte[] wadtik)
        {
            byte[] commonkey = new byte[16];

            if (File.Exists(System.Windows.Forms.Application.StartupPath + "\\shared\\common-key"))
            { commonkey = Tools.LoadFileToByteArray(System.Windows.Forms.Application.StartupPath + "\\shared\\common-key"); }
            else { throw new Exception("The common-key file must be in the shared folder of the application directory!"); }

            byte[] encryptedkey = new byte[16];
            byte[] iv = new byte[16];
            int tikpos = 0;

            if (IsThisWad(wadtik) == true)
            {
                //It's a wad file, so get the tik position
                tikpos = GetTikPos(wadtik);
            }

            for (int i = 0; i < 16; i++)
            {
                encryptedkey[i] = wadtik[tikpos + 0x1bf + i];
            }

            for (int j = 0; j < 8; j++)
            {
                iv[j] = wadtik[tikpos + 0x1dc + j];
                iv[j + 8] = 0x00;
            }

            RijndaelManaged decrypt = new RijndaelManaged();
            decrypt.Mode = CipherMode.CBC;
            decrypt.Padding = PaddingMode.None;
            decrypt.KeySize = 128;
            decrypt.BlockSize = 128;
            decrypt.Key = commonkey;
            decrypt.IV = iv;

            ICryptoTransform cryptor = decrypt.CreateDecryptor();

            MemoryStream memory = new MemoryStream(encryptedkey);
            CryptoStream crypto = new CryptoStream(memory, cryptor, CryptoStreamMode.Read);

            byte[] decryptedkey = new byte[16];
            crypto.Read(decryptedkey, 0, decryptedkey.Length);

            crypto.Close();
            memory.Close();

            return decryptedkey;
        }

        /// <summary>
        /// Decodes the Timestamp in the Trailer, if available.
        /// Returns null if no Timestamp was found.
        /// </summary>
        /// <param name="trailer"></param>
        /// <returns></returns>
        public static DateTime GetCreationTime(string trailer)
        {
            byte[] bTrailer = Tools.LoadFileToByteArray(trailer);
            return GetCreationTime(bTrailer);
        }

        /// <summary>
        /// Decodes the Timestamp in the Trailer, if available.
        /// Returns null if no Timestamp was found.
        /// </summary>
        /// <param name="trailer"></param>
        /// <returns></returns>
        public static DateTime GetCreationTime(byte[] trailer)
        {
            DateTime result = new DateTime(1970, 1, 1);

            if (trailer[0] == 'C' &&
                trailer[1] == 'M' &&
                trailer[2] == 'i' &&
                trailer[3] == 'i' &&
                trailer[4] == 'U' &&
                trailer[5] == 'T')
            {
                ASCIIEncoding enc = new ASCIIEncoding();
                string stringSeconds = enc.GetString(trailer, 6, 10);
                int seconds = 0;

                if (int.TryParse(stringSeconds, out seconds))
                {
                    result = result.AddSeconds((double)seconds);
                    return result;
                }
                else return result;
            }

            return result;
        }
    }

}