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
    public class ContentMap
    {
        /// <summary>
        /// Gets the name of the shared content in /shared1/.
        /// Returns "fail", if the content doesn't exist
        /// </summary>
        /// <param name="contentmap"></param>
        /// <param name="sha1ofcontent"></param>
        /// <returns></returns>
        public static string GetSharedContentName(byte[] contentmap, string sha1ofcontent)
        {
            int contindex = 0;
            string result = "";

            for (int i = 0; i < contentmap.Length - 19; i++)
            {
                string tmp = "";
                for (int y = 0; y < 20; y++)
                {
                    tmp += contentmap[i + y].ToString("x2");
                }

                if (tmp == sha1ofcontent)
                {
                    contindex = i;
                    break;
                }
            }

            if (contindex == 0) return "fail";

            result += Convert.ToChar(contentmap[contindex - 8]);
            result += Convert.ToChar(contentmap[contindex - 7]);
            result += Convert.ToChar(contentmap[contindex - 6]);
            result += Convert.ToChar(contentmap[contindex - 5]);
            result += Convert.ToChar(contentmap[contindex - 4]);
            result += Convert.ToChar(contentmap[contindex - 3]);
            result += Convert.ToChar(contentmap[contindex - 2]);
            result += Convert.ToChar(contentmap[contindex - 1]);

            return result;
        }

        /// <summary>
        /// Checks, if the shared content exists
        /// </summary>
        /// <param name="contentmap"></param>
        /// <param name="sha1ofcontent"></param>
        /// <returns></returns>
        public static bool CheckSharedContent(byte[] contentmap, string sha1ofcontent)
        {
            for (int i = 0; i < contentmap.Length - 19; i++)
            {
                string tmp = "";
                for (int y = 0; y < 20; y++)
                {
                    tmp += contentmap[i + y].ToString("x2");
                }

                if (tmp == sha1ofcontent) return true;
            }

            return false;
        }

        public static string GetNewSharedContentName(byte[] contentmap)
        {
            string name = "";

            name += Convert.ToChar(contentmap[contentmap.Length - 28]);
            name += Convert.ToChar(contentmap[contentmap.Length - 27]);
            name += Convert.ToChar(contentmap[contentmap.Length - 26]);
            name += Convert.ToChar(contentmap[contentmap.Length - 25]);
            name += Convert.ToChar(contentmap[contentmap.Length - 24]);
            name += Convert.ToChar(contentmap[contentmap.Length - 23]);
            name += Convert.ToChar(contentmap[contentmap.Length - 22]);
            name += Convert.ToChar(contentmap[contentmap.Length - 21]);

            string newname = (int.Parse(name, System.Globalization.NumberStyles.HexNumber) + 1).ToString("x8");

            return newname;
        }

        public static void AddSharedContent(string contentmap, string contentname, string sha1ofcontent)
        {
            byte[] name = new byte[8];
            byte[] sha1 = new byte[20];

            for (int i = 0; i < 8; i++)
            {
                name[i] = (byte)contentname[i];
            }

            for (int i = 0; i < sha1ofcontent.Length; i += 2)
            {
                sha1[i / 2] = Convert.ToByte(sha1ofcontent.Substring(i, 2), 16);
            }

            using (FileStream map = new FileStream(contentmap, FileMode.OpenOrCreate, FileAccess.ReadWrite))
            {
                map.Seek(0, SeekOrigin.End);
                map.Write(name, 0, name.Length);
                map.Write(sha1, 0, sha1.Length);
            }
        }
    }

}