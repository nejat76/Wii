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
    public class UID
    {
        /// <summary>
        /// Checks if the given Title ID exists in the uid.sys
        /// </summary>
        /// <param name="uidsys"></param>
        /// <param name="fulltitleid"></param>
        /// <returns></returns>
        public static bool CheckUID(byte[] uidsys, string fulltitleid)
        {
            for (int i = 0; i < uidsys.Length; i += 12)
            {
                string temp = "";

                for (int y = i; y < i + 8; y++)
                    temp += uidsys[y].ToString("x2");

                if (temp == fulltitleid) return true;
            }

            return false;
        }

        /// <summary>
        /// Gets a new UID
        /// </summary>
        /// <param name="uidsys"></param>
        /// <returns></returns>
        public static string GetNewUID(byte[] uidsys)
        {
            string lastuid = uidsys[uidsys.Length - 4].ToString("x2") +
                uidsys[uidsys.Length - 3].ToString("x2") +
                uidsys[uidsys.Length - 2].ToString("x2") +
                uidsys[uidsys.Length - 1].ToString("x2");

            string newuid = (int.Parse(lastuid, System.Globalization.NumberStyles.HexNumber) + 1).ToString("x8");
            return newuid;
        }

        /// <summary>
        /// Adds a Title ID to uid.sys
        /// </summary>
        /// <param name="uidsys"></param>
        /// <param name="fulltitleid"></param>
        /// <returns></returns>
        public static byte[] AddUID(byte[] uidsys, string fulltitleid)
        {
            if (uidsys.Length > 11)
            {
                MemoryStream uid = new MemoryStream();
                byte[] titleid = Tools.HexStringToByteArray(fulltitleid);
                byte[] newuid = Tools.HexStringToByteArray(GetNewUID(uidsys));

                uid.Write(uidsys, 0, uidsys.Length);
                uid.Write(titleid, 0, titleid.Length);
                uid.Write(newuid, 0, newuid.Length);

                return uid.ToArray();
            }
            else
            {
                MemoryStream uid = new MemoryStream();
                byte[] titleid = Tools.HexStringToByteArray(fulltitleid);
                byte[] newuid = new byte[] { 0x00, 0x00, 0x10, 0x00 };

                uid.Write(titleid, 0, titleid.Length);
                uid.Write(newuid, 0, newuid.Length);

                return uid.ToArray();
            }
        }
    }
}