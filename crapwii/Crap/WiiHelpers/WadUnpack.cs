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
    public class WadUnpack
    {
        /// <summary>
        /// Unpacks the 00000000.app of a wad
        /// </summary>
        /// <param name="wadfile"></param>
        /// <returns></returns>
        public static void UnpackNullApp(string wadfile, string destination)
        {
            if (!destination.EndsWith(".app")) destination += "\\00000000.app";

            byte[] wad = Tools.LoadFileToByteArray(wadfile);
            byte[] nullapp = UnpackNullApp(wad);
            Tools.SaveFileFromByteArray(nullapp, destination);
        }

        /// <summary>
        /// Unpacks the 00000000.app of a wad
        /// </summary>
        /// <param name="wadfile"></param>
        /// <returns></returns>
        public static byte[] UnpackNullApp(byte[] wadfile)
        {
            int certsize = WadInfo.GetCertSize(wadfile);
            int tiksize = WadInfo.GetTikSize(wadfile);
            int tmdpos = WadInfo.GetTmdPos(wadfile);
            int tmdsize = WadInfo.GetTmdSize(wadfile);
            int contentpos = 64 + Tools.AddPadding(certsize) + Tools.AddPadding(tiksize) + Tools.AddPadding(tmdsize);

            byte[] titlekey = WadInfo.GetTitleKey(wadfile);
            string[,] contents = WadInfo.GetContentInfo(wadfile);

            for (int i = 0; i < contents.GetLength(0); i++)
            {
                if (contents[i, 1] == "00000000")
                {
                    return WadEdit.DecryptContent(wadfile, i, titlekey);
                }
            }

            throw new Exception("00000000.app couldn't be found in the Wad");
        }

        /// <summary>
        /// Unpacks the the wad file
        /// </summary>
        public static string UnpackWad(string pathtowad, string destinationpath)
        {
            byte[] wadfile = Tools.LoadFileToByteArray(pathtowad);
            return UnpackWad(wadfile, destinationpath);
        }

        /// <summary>
        /// Unpacks the the wad file
        /// </summary>
        public static string UnpackWad(string pathtowad, string destinationpath, out bool hashesmatch)
        {
            byte[] wadfile = Tools.LoadFileToByteArray(pathtowad);
            return UnpackWad(wadfile, destinationpath, out hashesmatch);
        }

        /// <summary>
        /// Unpacks the wad file to *wadpath*\wadunpack\
        /// </summary>
        /// <param name="pathtowad"></param>
        public static string UnpackWad(string pathtowad)
        {
            string destinationpath = pathtowad.Remove(pathtowad.LastIndexOf('\\'));
            byte[] wadfile = Tools.LoadFileToByteArray(pathtowad);
            return UnpackWad(wadfile, destinationpath);
        }

        /// <summary>
        /// Unpacks the wad file
        /// </summary>
        public static string UnpackWad(byte[] wadfile, string destinationpath)
        {
            bool temp;
            return UnpackWad(wadfile, destinationpath, out temp);
        }

        /// <summary>
        /// Unpacks the wad file
        /// </summary>
        public static string UnpackWad(byte[] wadfile, string destinationpath, out bool hashesmatch)
        {
            if (destinationpath[destinationpath.Length - 1] != '\\')
            { destinationpath = destinationpath + "\\"; }

            hashesmatch = true;

            if (!Directory.Exists(destinationpath))
            { Directory.CreateDirectory(destinationpath); }
            //if (Directory.GetFiles(destinationpath, "*.app").Length > 0)
            //{
            //    throw new Exception("At least one of the files to unpack already exists!");
            //}

            int certpos = 0x40;
            int certsize = WadInfo.GetCertSize(wadfile);
            int tikpos = WadInfo.GetTikPos(wadfile);
            int tiksize = WadInfo.GetTikSize(wadfile);
            int tmdpos = WadInfo.GetTmdPos(wadfile);
            int tmdsize = WadInfo.GetTmdSize(wadfile);
            int contentlength = WadInfo.GetContentSize(wadfile);
            int footersize = WadInfo.GetFooterSize(wadfile);
            int footerpos = 64 + Tools.AddPadding(certsize) + Tools.AddPadding(tiksize) + Tools.AddPadding(tmdsize) + Tools.AddPadding(contentlength);
            string wadpath = WadInfo.GetNandPath(wadfile, 0).Remove(8, 1);
            string[,] contents = WadInfo.GetContentInfo(wadfile);
            byte[] titlekey = WadInfo.GetTitleKey(wadfile);
            int contentpos = 64 + Tools.AddPadding(certsize) + Tools.AddPadding(tiksize) + Tools.AddPadding(tmdsize);

            //unpack cert
            using (FileStream cert = new FileStream(destinationpath + wadpath + ".cert", FileMode.Create))
            {
                cert.Seek(0, SeekOrigin.Begin);
                cert.Write(wadfile, certpos, certsize);
            }

            //unpack ticket
            using (FileStream tik = new FileStream(destinationpath + wadpath + ".tik", FileMode.Create))
            {
                tik.Seek(0, SeekOrigin.Begin);
                tik.Write(wadfile, tikpos, tiksize);
            }

            string tmdPath = destinationpath + wadpath + ".tmd";
            //unpack tmd
            using (FileStream tmd = new FileStream(tmdPath, FileMode.Create))
            {
                tmd.Seek(0, SeekOrigin.Begin);
                tmd.Write(wadfile, tmdpos, tmdsize);
            }

            //unpack trailer
            try
            {
                if (footersize > 0)
                {
                    using (FileStream trailer = new FileStream(destinationpath + wadpath + ".trailer", FileMode.Create))
                    {
                        trailer.Seek(0, SeekOrigin.Begin);
                        trailer.Write(wadfile, footerpos, footersize);
                    }
                }
            }
            catch { } //who cares if the trailer doesn't extract properly?

            Tools.ChangeProgress(0);

            //unpack contents
            for (int i = 0; i < contents.GetLength(0); i++)
            {
                Tools.ChangeProgress((i + 1) * 100 / contents.GetLength(0));
                byte[] thiscontent = WadEdit.DecryptContent(wadfile, i, titlekey);
                FileStream content = new FileStream(destinationpath + contents[i, 1] + ".app", FileMode.Create);

                content.Write(thiscontent, 0, thiscontent.Length);
                content.Close();

                contentpos += Tools.AddPadding(thiscontent.Length);

                //sha1 comparison
                SHA1Managed sha1 = new SHA1Managed();
                byte[] thishash = sha1.ComputeHash(thiscontent);
                byte[] tmdhash = Tools.HexStringToByteArray(contents[i, 4]);

                if (Tools.CompareByteArrays(thishash, tmdhash) == false) hashesmatch = false;
                //    throw new Exception("At least one content's hash doesn't match the hash in the Tmd!");
            }

            return tmdPath;
        }

        /// <summary>
        /// Unpacks the wad file to the given directory.
        /// Shared contents will be unpacked to /shared1
        /// </summary>
        /// <param name="wadfile"></param>
        /// <param name="nandpath"></param>
        public static void UnpackToNand(string wadfile, string nandpath)
        {
            byte[] wadarray = Tools.LoadFileToByteArray(wadfile);
            UnpackToNand(wadarray, nandpath);
        }

        /// <summary>
        /// Unpacks the wad file to the given directory.
        /// Shared contents will be unpacked to /shared1
        /// </summary>
        /// <param name="wadfile"></param>
        /// <param name="nandpath"></param>
        public static void UnpackToNand(byte[] wadfile, string nandpath)
        {
            string path = WadInfo.GetNandPath(wadfile, 0);
            string path1 = path.Remove(path.IndexOf('\\'));
            string path2 = path.Remove(0, path.IndexOf('\\') + 1);

            if (nandpath[nandpath.Length - 1] != '\\') { nandpath = nandpath + "\\"; }

            if (!Directory.Exists(nandpath + "ticket")) { Directory.CreateDirectory(nandpath + "ticket"); }
            if (!Directory.Exists(nandpath + "title")) { Directory.CreateDirectory(nandpath + "title"); }
            if (!Directory.Exists(nandpath + "ticket\\" + path1)) { Directory.CreateDirectory(nandpath + "ticket\\" + path1); }
            if (!Directory.Exists(nandpath + "title\\" + path1)) { Directory.CreateDirectory(nandpath + "title\\" + path1); }
            if (!Directory.Exists(nandpath + "title\\" + path1 + "\\" + path2)) { Directory.CreateDirectory(nandpath + "title\\" + path1 + "\\" + path2); }
            if (!Directory.Exists(nandpath + "title\\" + path1 + "\\" + path2 + "\\content")) { Directory.CreateDirectory(nandpath + "title\\" + path1 + "\\" + path2 + "\\content"); }
            if (!Directory.Exists(nandpath + "title\\" + path1 + "\\" + path2 + "\\data")) { Directory.CreateDirectory(nandpath + "title\\" + path1 + "\\" + path2 + "\\data"); }
            if (!Directory.Exists(nandpath + "shared1")) Directory.CreateDirectory(nandpath + "shared1");

            int certsize = WadInfo.GetCertSize(wadfile);
            int tikpos = WadInfo.GetTikPos(wadfile);
            int tiksize = WadInfo.GetTikSize(wadfile);
            int tmdpos = WadInfo.GetTmdPos(wadfile);
            int tmdsize = WadInfo.GetTmdSize(wadfile);
            int contentlength = WadInfo.GetContentSize(wadfile);
            string[,] contents = WadInfo.GetContentInfo(wadfile);
            byte[] titlekey = WadInfo.GetTitleKey(wadfile);
            int contentpos = 64 + Tools.AddPadding(certsize) + Tools.AddPadding(tiksize) + Tools.AddPadding(tmdsize);

            //unpack ticket
            using (FileStream tik = new FileStream(nandpath + "ticket\\" + path1 + "\\" + path2 + ".tik", FileMode.Create))
            {
                tik.Seek(0, SeekOrigin.Begin);
                tik.Write(wadfile, tikpos, tiksize);
            }

            //unpack tmd
            using (FileStream tmd = new FileStream(nandpath + "title\\" + path1 + "\\" + path2 + "\\content\\title.tmd", FileMode.Create))
            {
                tmd.Seek(0, SeekOrigin.Begin);
                tmd.Write(wadfile, tmdpos, tmdsize);
            }

            Tools.ChangeProgress(0);

            //unpack contents
            for (int i = 0; i < contents.GetLength(0); i++)
            {
                Tools.ChangeProgress((i + 1) * 100 / contents.GetLength(0));
                byte[] thiscontent = WadEdit.DecryptContent(wadfile, i, titlekey);

                if (contents[i, 2] == "8001")
                {
                    if (File.Exists(nandpath + "shared1\\content.map"))
                    {
                        byte[] contmap = Tools.LoadFileToByteArray(nandpath + "shared1\\content.map");

                        if (ContentMap.CheckSharedContent(contmap, contents[i, 4]) == false)
                        {
                            string newname = ContentMap.GetNewSharedContentName(contmap);

                            FileStream content = new FileStream(nandpath + "shared1\\" + newname + ".app", FileMode.Create);
                            content.Write(thiscontent, 0, thiscontent.Length);
                            content.Close();
                            ContentMap.AddSharedContent(nandpath + "shared1\\content.map", newname, contents[i, 4]);
                        }
                    }
                    else
                    {
                        FileStream content = new FileStream(nandpath + "shared1\\00000000.app", FileMode.Create);
                        content.Write(thiscontent, 0, thiscontent.Length);
                        content.Close();
                        ContentMap.AddSharedContent(nandpath + "shared1\\content.map", "00000000", contents[i, 4]);
                    }
                }
                else
                {
                    FileStream content = new FileStream(nandpath + "title\\" + path1 + "\\" + path2 + "\\content\\" + contents[i, 0] + ".app", FileMode.Create);

                    content.Write(thiscontent, 0, thiscontent.Length);
                    content.Close();
                }

                contentpos += Tools.AddPadding(thiscontent.Length);
            }

            //add titleid to uid.sys, if it doesn't exist
            string titleid = WadInfo.GetFullTitleID(wadfile, 1);

            if (File.Exists(nandpath + "\\sys\\uid.sys"))
            {
                FileStream fs = new FileStream(nandpath + "\\sys\\uid.sys", FileMode.Open);
                byte[] uidsys = new byte[fs.Length];
                fs.Read(uidsys, 0, uidsys.Length);
                fs.Close();

                if (UID.CheckUID(uidsys, titleid) == false)
                {
                    uidsys = UID.AddUID(uidsys, titleid);
                    Tools.SaveFileFromByteArray(uidsys, nandpath + "\\sys\\uid.sys");
                }
            }
            else
            {
                if (!Directory.Exists(nandpath + "\\sys")) Directory.CreateDirectory(nandpath + "\\sys");
                byte[] uidsys = UID.AddUID(new byte[0], titleid);
                Tools.SaveFileFromByteArray(uidsys, nandpath + "\\sys\\uid.sys");
            }
        }
    }

}
