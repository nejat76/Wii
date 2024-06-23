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
    public class NAND
    {
        /// <summary>
        /// Backups all Saves from a NAND Backup
        /// </summary>
        /// <param name="nandpath"></param>
        /// <param name="destinationpath"></param>
        public static void BackupSaves(string nandpath, string destinationpath)
        {
            string titlefolder = nandpath + "\\title";
            string[] lowerdirs = Directory.GetDirectories(titlefolder);
            Tools.ChangeProgress(0);

            for (int i = 0; i < lowerdirs.Length; i++)
            {
                Tools.ChangeProgress((i + 1) * 100 / lowerdirs.Length);
                string[] upperdirs = Directory.GetDirectories(lowerdirs[i]);

                for (int j = 0; j < upperdirs.Length; j++)
                {
                    if (Directory.Exists(upperdirs[j] + "\\data"))
                    {
                        if (Directory.GetFiles(upperdirs[j] + "\\data").Length > 0 ||
                            Directory.GetDirectories(upperdirs[j] + "\\data").Length > 0)
                        {
                            Tools.CopyDirectory(upperdirs[j] + "\\data", (upperdirs[j] + "\\data").Replace(nandpath, destinationpath).Replace("\\title", ""));
                        }
                    }
                }
            }
        }

        /// <summary>
        /// Restores all Saves for existing titles to a NAND Backup
        /// </summary>
        /// <param name="backupdir"></param>
        /// <param name="nandpath"></param>
        public static void RestoreSaves(string backuppath, string nandpath)
        {
            string titlefolder = nandpath + "\\title";
            string[] lowerdirs = Directory.GetDirectories(backuppath);
            Tools.ChangeProgress(0);

            for (int i = 0; i < lowerdirs.Length; i++)
            {
                Tools.ChangeProgress((i + 1) * 100 / lowerdirs.Length);
                string[] upperdirs = Directory.GetDirectories(lowerdirs[i]);

                for (int j = 0; j < upperdirs.Length; j++)
                {
                    string[] datafiles = Directory.GetFiles(upperdirs[j] + "\\data");
                    string upperdirnand = upperdirs[j].Replace(backuppath, titlefolder);

                    if (Directory.Exists(upperdirnand) &&
                        (Directory.GetFiles(upperdirs[j] + "\\data").Length > 0 ||
                         Directory.GetDirectories(upperdirs[j] + "\\data").Length > 0))
                    {
                        if (!Directory.Exists(upperdirnand + "\\data")) Directory.CreateDirectory(upperdirnand + "\\data");
                        Tools.CopyDirectory(upperdirs[j] + "\\data", (upperdirs[j] + "\\data").Replace(backuppath, titlefolder));
                    }
                }
            }

            Tools.ChangeProgress(100);
        }

        /// <summary>
        /// Backups a single Save
        /// </summary>
        /// <param name="nandpath"></param>
        /// <param name="titlepath">Format: XXXXXXXX\XXXXXXXX</param>
        /// <param name="destinationpath"></param>
        public static void BackupSingleSave(string nandpath, string titlepath, string destinationpath)
        {
            string datafolder = nandpath + "\\title\\" + titlepath + "\\data";

            if (Directory.GetFiles(datafolder).Length > 0 ||
                Directory.GetDirectories(datafolder).Length > 0)
            {
                string savefolder = datafolder.Replace(nandpath, destinationpath).Replace("\\title", "");
                if (!Directory.Exists(savefolder)) Directory.CreateDirectory(savefolder);

                Tools.CopyDirectory(datafolder, savefolder);
            }
            else
            {
                throw new Exception("No save data was found!");
            }
        }

        /// <summary>
        /// Restores a singe Save, if the title exists on NAND Backup
        /// </summary>
        /// <param name="backuppath"></param>
        /// <param name="titlepath">Format: XXXXXXXX\XXXXXXXX</param>
        /// <param name="nandpath"></param>
        public static void RestoreSingleSave(string backuppath, string titlepath, string nandpath)
        {
            string titlefoldernand = nandpath + "\\title\\" + titlepath;
            string titlefolder = titlefoldernand.Replace(nandpath, backuppath).Replace("\\title", "");

            if (Directory.Exists(titlefoldernand) &&
                (Directory.GetFiles(titlefolder + "\\data").Length > 0 ||
                 Directory.GetDirectories(titlefolder + "\\data").Length > 0))
            {
                if (!Directory.Exists(titlefoldernand + "\\data")) Directory.CreateDirectory(titlefoldernand + "\\data");
                Tools.CopyDirectory(titlefolder + "\\data", titlefoldernand + "\\data");
            }
            else
            {
                throw new Exception("Title not found in NAND Backup!");
            }
        }

        /// <summary>
        /// Checks, if save data exists in the given title folder
        /// </summary>
        /// <param name="nandpath"></param>
        /// <param name="titlepath">Format: XXXXXXXX\XXXXXXXX</param>
        public static bool CheckForSaveData(string nandpath, string titlepath)
        {
            string datafolder = nandpath + "\\title\\" + titlepath + "\\data";

            if (!Directory.Exists(datafolder)) return false;
            else
            {
                string[] datafiles = Directory.GetFiles(datafolder);

                if (datafiles.Length > 0) return true;
                else return false;
            }
        }

        /// <summary>
        /// Checks, if save data exists in the given title folder
        /// </summary>
        /// <param name="nandpath"></param>
        /// <param name="titlepath">Format: XXXXXXXX\XXXXXXXX</param>
        public static bool CheckForBackupData(string backuppath, string titlepath)
        {
            string datafolder = backuppath + "\\" + titlepath + "\\data";

            if (!Directory.Exists(datafolder)) return false;
            else
            {
                string[] datafiles = Directory.GetFiles(datafolder);

                if (datafiles.Length > 0) return true;
                else return false;
            }
        }
    }

}