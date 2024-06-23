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
    public class Tools
    {
        public static event EventHandler<ProgressChangedEventArgs> ProgressChanged;

        public static void ChangeProgress(int ProgressPercent)
        {
            EventHandler<ProgressChangedEventArgs> progressChanged = ProgressChanged;
            if (progressChanged != null)
            {
                progressChanged(new object(), new ProgressChangedEventArgs(ProgressPercent));
            }
        }

        /// <summary>
        /// Writes the small Byte Array into the big one at the given offset
        /// </summary>
        /// <param name="big"></param>
        /// <param name="small"></param>
        /// <param name="offset"></param>
        /// <returns></returns>
        public static byte[] InsertByteArray(byte[] big, byte[] small, int offset)
        {
            for (int i = 0; i < small.Length; i++)
                big[offset + i] = small[i];
            return big;
        }

        /// <summary>
        /// Returns the current UTC Unix Timestamp as a Byte Array
        /// </summary>
        /// <returns></returns>
        public static byte[] GetTimestamp()
        {
            DateTime dtNow = DateTime.UtcNow;
            TimeSpan tsTimestamp = (dtNow - new DateTime(1970, 1, 1, 0, 0, 0));

            int timestamp = (int)tsTimestamp.TotalSeconds;
            ASCIIEncoding enc = new ASCIIEncoding();
            byte[] timestampBytes = enc.GetBytes("CMiiUT" + timestamp.ToString());
            return timestampBytes;
        }

        /// <summary>
        /// Creates a new Byte Array out of the given one
        /// from the given offset with the specified length
        /// </summary>
        /// <param name="array"></param>
        /// <param name="offset"></param>
        /// <param name="length"></param>
        /// <returns></returns>
        public static byte[] GetPartOfByteArray(byte[] array, int offset, int length)
        {
            byte[] ret = new byte[length];
            for (int i = 0; i < length; i++)
                ret[i] = array[offset + i];
            return ret;
        }

        /// <summary>
        /// Converts UInt32 Array into Byte Array
        /// </summary>
        /// <param name="array"></param>
        /// <returns></returns>
        public static byte[] UInt32ArrayToByteArray(UInt32[] array)
        {
            List<byte> results = new List<byte>();
            foreach (UInt32 value in array)
            {
                byte[] converted = BitConverter.GetBytes(value);
                results.AddRange(converted);
            }
            return results.ToArray();
        }

        /// <summary>
        /// Converts UInt16 Array into Byte Array
        /// </summary>
        /// <param name="array"></param>
        /// <returns></returns>
        public static byte[] UInt16ArrayToByteArray(UInt16[] array)
        {
            List<byte> results = new List<byte>();
            foreach (UInt16 value in array)
            {
                byte[] converted = BitConverter.GetBytes(value);
                results.AddRange(converted);
            }
            return results.ToArray();
        }

        /// <summary>
        /// Converts UInt16 Array into Byte Array
        /// </summary>
        /// <param name="array"></param>
        /// <returns></returns>
        public static byte[] UIntArrayToByteArray(uint[] array)
        {
            List<byte> results = new List<byte>();
            foreach (uint value in array)
            {
                byte[] converted = BitConverter.GetBytes(value);
                results.AddRange(converted);
            }
            return results.ToArray();
        }

        /// <summary>
        /// Converts Byte Array into UInt16 Array
        /// </summary>
        /// <param name="array"></param>
        /// <returns></returns>
        public static UInt32[] ByteArrayToUInt32Array(byte[] array)
        {
            UInt32[] converted = new UInt32[array.Length / 2];
            int j = 0;
            for (int i = 0; i < array.Length; i += 4)
            {
                converted[j] = BitConverter.ToUInt32(array, i);
                j++;
            }
            return converted;
        }

        /// <summary>
        /// Converts Byte Array into UInt16 Array
        /// </summary>
        /// <param name="array"></param>
        /// <returns></returns>
        public static UInt16[] ByteArrayToUInt16Array(byte[] array)
        {
            UInt16[] converted = new UInt16[array.Length / 2];
            int j = 0;
            for (int i = 0; i < array.Length; i += 2)
            {
                converted[j] = BitConverter.ToUInt16(array, i);
                j++;
            }
            return converted;
        }

        /// <summary>
        /// Returns the file length as a Byte Array
        /// </summary>
        /// <param name="filelength"></param>
        /// <returns></returns>
        public static byte[] FileLengthToByteArray(int filelength)
        {
            byte[] length = BitConverter.GetBytes(filelength);
            Array.Reverse(length);
            return length;
        }

        /// <summary>
        /// Adds a padding to the next 64 bytes, if necessary
        /// </summary>
        /// <param name="size"></param>
        /// <returns></returns>
        public static int AddPadding(int value)
        {
            return AddPadding(value, 64);
        }

        /// <summary>
        /// Adds a padding to the given value, if necessary
        /// </summary>
        /// <param name="value"></param>
        /// <param name="padding"></param>
        /// <returns></returns>
        public static int AddPadding(int value, int padding)
        {
            if (value % padding != 0)
            {
                value = value + (padding - (value % padding));
            }

            return value;
        }

        /// <summary>
        /// Converts a Hex-String to Int
        /// </summary>
        /// <param name="hexstring"></param>
        /// <returns></returns>
        public static int HexStringToInt(string hexstring)
        {
            try { return int.Parse(hexstring, System.Globalization.NumberStyles.HexNumber); }
            catch { throw new Exception("An Error occured, maybe the Wad file is corrupt!"); }
        }

        /// <summary>
        /// Converts a Hex-String to Long
        /// </summary>
        /// <param name="hexstring"></param>
        /// <returns></returns>
        public static long HexStringToLong(string hexstring)
        {
            try { return long.Parse(hexstring, System.Globalization.NumberStyles.HexNumber); }
            catch { throw new Exception("An Error occured, maybe the Wad file is corrupt!"); }
        }

        /// <summary>
        /// Writes a Byte Array to a file
        /// </summary>
        /// <param name="file"></param>
        public static void SaveFileFromByteArray(byte[] file, string destination)
        {
            using (FileStream fs = new FileStream(destination, FileMode.Create))
                fs.Write(file, 0, file.Length);
        }

        /// <summary>
        /// Loads a file into a Byte Array
        /// </summary>
        /// <param name="sourcefile"></param>
        /// <returns></returns>
        public static byte[] LoadFileToByteArray(string sourcefile)
        {
            if (File.Exists(sourcefile))
            {
                using (FileStream fs = new FileStream(sourcefile, FileMode.Open))
                {
                    byte[] filearray = new byte[fs.Length];
                    fs.Read(filearray, 0, filearray.Length);
                    return filearray;
                }
            }
            else throw new FileNotFoundException("File couldn't be found:\r\n" + sourcefile);
        }

        /// <summary>
        /// Loads a file into a Byte Array
        /// </summary>
        /// <param name="sourcefile"></param>
        /// <returns></returns>
        public static byte[] LoadFileToByteArray(string sourcefile, int offset, int length)
        {
            if (File.Exists(sourcefile))
            {
                using (FileStream fs = new FileStream(sourcefile, FileMode.Open))
                {
                    if (fs.Length < length) length = (int)fs.Length;
                    byte[] filearray = new byte[length];
                    fs.Seek(offset, SeekOrigin.Begin);
                    fs.Read(filearray, 0, length);
                    return filearray;
                }
            }
            else throw new FileNotFoundException("File couldn't be found:\r\n" + sourcefile);
        }

        /// <summary>
        /// Checks the SHA1 of the Common-Key
        /// </summary>
        /// <param name="pathtocommonkey"></param>
        /// <returns></returns>
        public static bool CheckCommonKey(string pathtocommonkey)
        {
            byte[] sum = new byte[] { 0xEB, 0xEA, 0xE6, 0xD2, 0x76, 0x2D, 0x4D, 0x3E, 0xA1, 0x60, 0xA6, 0xD8, 0x32, 0x7F, 0xAC, 0x9A, 0x25, 0xF8, 0x06, 0x2B };

            FileInfo fi = new FileInfo(pathtocommonkey);
            if (fi.Length != 16) return false;
            else
            {
                byte[] ckey = LoadFileToByteArray(pathtocommonkey);

                SHA1Managed sha1 = new SHA1Managed();
                byte[] newsum = sha1.ComputeHash(ckey);

                if (CompareByteArrays(sum, newsum) == true) return true;
                else return false;
            }
        }


        /// <summary>
        /// Counts the appearance of a specific character in a string
        /// </summary>
        /// <param name="theString"></param>
        /// <param name="theChar"></param>
        /// <returns></returns>
        public static int CountCharsInString(string theString, char theChar)
        {
            int count = 0;
            foreach (char thisChar in theString)
            {
                if (thisChar == theChar)
                    count++;
            }
            return count;
        }

        /// <summary>
        /// Compares two Byte Arrays and returns true, if they match
        /// </summary>
        /// <param name="first"></param>
        /// <param name="second"></param>
        /// <returns></returns>
        public static bool CompareByteArrays(byte[] first, byte[] second)
        {
            if (first.Length != second.Length) return false;
            else
            {
                for (int i = 0; i < first.Length; i++)
                    if (first[i] != second[i]) return false;

                return true;
            }
        }

        /// <summary>
        /// Converts a Hex String to a Byte Array
        /// </summary>
        /// <param name="hexstring"></param>
        /// <returns></returns>
        public static byte[] HexStringToByteArray(string hexstring)
        {
            byte[] ba = new byte[hexstring.Length / 2];

            for (int i = 0; i < hexstring.Length / 2; i++)
            {
                ba[i] = byte.Parse(hexstring.Substring(i * 2, 2), System.Globalization.NumberStyles.HexNumber);
            }

            return ba;
        }

        /// <summary>
        /// Checks, if the given string does exist in the string Array
        /// </summary>
        /// <param name="theString"></param>
        /// <param name="theStringArray"></param>
        /// <returns></returns>
        public static bool StringExistsInStringArray(string theString, string[] theStringArray)
        {
            return Array.Exists(theStringArray, thisString => thisString == theString);
        }

        /// <summary>
        /// Copies an entire Directoy
        /// </summary>
        /// <param name="source"></param>
        /// <param name="destination"></param>
        public static void CopyDirectory(string source, string destination)
        {
            string[] subdirs = Directory.GetDirectories(source);
            string[] files = Directory.GetFiles(source);

            foreach (string thisFile in files)
            {
                if (!Directory.Exists(destination)) Directory.CreateDirectory(destination);
                if (File.Exists(destination + "\\" + Path.GetFileName(thisFile))) File.Delete(destination + "\\" + Path.GetFileName(thisFile));
                File.Copy(thisFile, destination + "\\" + Path.GetFileName(thisFile));
            }

            foreach (string thisDir in subdirs)
            {
                CopyDirectory(thisDir, destination + "\\" + thisDir.Remove(0, thisDir.LastIndexOf('\\') + 1));
            }
        }
    }

}