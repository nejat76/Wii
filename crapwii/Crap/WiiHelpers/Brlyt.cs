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

    public class Brlyt
    {
        /// <summary>
        /// Checks, if the TPLs match the TPLs specified in the brlyt
        /// </summary>
        /// <param name="brlyt"></param>
        /// <param name="tpls"></param>
        /// <returns></returns>
        public static bool CheckBrlytTpls(string brlyt, string[] tpls)
        {
            byte[] brlytArray = Tools.LoadFileToByteArray(brlyt);
            return CheckBrlytTpls(brlytArray, tpls);
        }

        /// <summary>
        /// Checks, if the TPLs match the TPLs specified in the brlyt
        /// </summary>
        /// <param name="brlyt"></param>
        /// <param name="tpls"></param>
        /// <returns></returns>
        public static bool CheckBrlytTpls(byte[] brlyt, string[] tpls)
        {
            int texcount = Tools.HexStringToInt(brlyt[44].ToString("x2") + brlyt[45].ToString("x2"));
            if (tpls.Length != texcount) return false;

            int texnamepos = 48 + (texcount * 8);
            for (int i = 0; i < texcount; i++)
            {
                string thisTex = "";
                while (brlyt[texnamepos] != 0x00)
                {
                    thisTex += Convert.ToChar(brlyt[texnamepos]);
                    texnamepos++;
                }
                texnamepos++;

                bool exists = Array.Exists(tpls, tpl => tpl == thisTex);
                if (exists == false) return false;
            }

            return true;
        }

        /// <summary>
        /// Checks, if one or more Tpls specified in the brlyt are missing and returns 
        /// the names of the missing ones.
        /// </summary>
        /// <param name="brlyt"></param>
        /// <param name="tpls"></param>
        /// <param name="missingtpls"></param>
        /// <returns></returns>
        public static bool CheckForMissingTpls(string brlyt, string[] tpls, out string[] missingtpls)
        {
            byte[] brlytArray = Tools.LoadFileToByteArray(brlyt);
            return CheckForMissingTpls(brlytArray, tpls, out missingtpls);
        }

        /// <summary>
        /// Checks, if one or more Tpls specified in the brlyt are missing and returns 
        /// the names of the missing ones.
        /// </summary>
        /// <param name="brlyt"></param>
        /// <param name="tpls"></param>
        /// <param name="missingtpls"></param>
        /// <returns></returns>
        public static bool CheckForMissingTpls(byte[] brlyt, string[] tpls, out string[] missingtpls)
        {
            List<string> missings = new List<string>();
            string[] brlytTpls = GetBrlytTpls(brlyt);
            bool missing = false;

            for (int i = 0; i < brlytTpls.Length; i++)
            {
                if (Tools.StringExistsInStringArray(brlytTpls[i], tpls) == false)
                {
                    missings.Add(brlytTpls[i]);
                    missing = true;
                }
            }

            missingtpls = missings.ToArray();
            return missing;
        }

        /// <summary>
        /// Checks, if one or more Tpls are not specified in the brlyt and returns 
        /// the names of the missing ones.
        /// </summary>
        /// <param name="brly"></param>
        /// <param name="tpls"></param>
        /// <param name="unusedtpls"></param>
        /// <returns></returns>
        public static bool CheckForUnusedTpls(string brlyt, string[] tpls, out string[] unusedtpls)
        {
            byte[] brlytArray = Tools.LoadFileToByteArray(brlyt);
            return CheckForUnusedTpls(brlytArray, tpls, out unusedtpls);
        }

        /// <summary>
        /// Checks, if one or more Tpls are not specified in the brlyt and returns 
        /// the names of the missing ones.
        /// </summary>
        /// <param name="brly"></param>
        /// <param name="tpls"></param>
        /// <param name="unusedtpls"></param>
        /// <returns></returns>
        public static bool CheckForUnusedTpls(byte[] brlyt, string[] tpls, out string[] unusedtpls)
        {
            List<string> unuseds = new List<string>();
            string[] brlytTpls = GetBrlytTpls(brlyt);
            bool missing = false;

            for (int i = 0; i < tpls.Length; i++)
            {
                if (Tools.StringExistsInStringArray(tpls[i], brlytTpls) == false)
                {
                    string wonum = tpls[i].Remove(tpls[i].LastIndexOf('.') - 1) + "00.tpl";
                    string wonum2 = tpls[i].Remove(tpls[i].LastIndexOf('.') - 2) + "00.tpl";
                    string wonum3 = tpls[i].Remove(tpls[i].LastIndexOf('.') - 1) + "01.tpl";
                    string wonum4 = tpls[i].Remove(tpls[i].LastIndexOf('.') - 2) + "01.tpl";

                    if (Tools.StringExistsInStringArray(wonum, brlytTpls) == false &&
                        Tools.StringExistsInStringArray(wonum2, brlytTpls) == false &&
                        Tools.StringExistsInStringArray(wonum3, brlytTpls) == false &&
                        Tools.StringExistsInStringArray(wonum4, brlytTpls) == false)
                    {
                        unuseds.Add(tpls[i]);
                        missing = true;
                    }
                }
            }

            unusedtpls = unuseds.ToArray();
            return missing;
        }

        /// <summary>
        /// Returns the name of all Tpls specified in the brlyt
        /// </summary>
        /// <param name="brlyt"></param>
        /// <returns></returns>
        public static string[] GetBrlytTpls(string brlyt)
        {
            byte[] temp = Tools.LoadFileToByteArray(brlyt);
            return GetBrlytTpls(temp);
        }

        /// <summary>
        /// Returns the name of all Tpls specified in the brlyt
        /// </summary>
        /// <param name="brlyt"></param>
        /// <returns></returns>
        public static string[] GetBrlytTpls(byte[] brlyt)
        {
            int texcount = Tools.HexStringToInt(brlyt[44].ToString("x2") + brlyt[45].ToString("x2"));
            int texnamepos = 48 + (texcount * 8);
            List<string> Tpls = new List<string>();

            for (int i = 0; i < texcount; i++)
            {
                string thisTex = "";
                while (brlyt[texnamepos] != 0x00)
                {
                    thisTex += Convert.ToChar(brlyt[texnamepos]);
                    texnamepos++;
                }
                Tpls.Add(thisTex);
                texnamepos++;
            }

            return Tpls.ToArray();
        }

        /// <summary>
        /// Returns true, if the given Tpl is specified in the brlyt.
        /// TplName must end with ".tpl"!
        /// </summary>
        /// <param name="brlyt"></param>
        /// <param name="TplName"></param>
        /// <returns></returns>
        public static bool IsTplInBrlyt(byte[] brlyt, string TplName)
        {
            string[] brlytTpls = GetBrlytTpls(brlyt);
            bool exists = Array.Exists(brlytTpls, Tpl => Tpl == TplName);
            return exists;
        }
    }

}