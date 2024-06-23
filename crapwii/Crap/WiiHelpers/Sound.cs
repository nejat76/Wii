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
    public class Sound
    {
        /// <summary>
        /// Checks if the given Wave is a proper PCM WAV file
        /// </summary>
        /// <param name="wavefile"></param>
        /// <returns></returns>
        public static bool CheckWave(string wavefile)
        {
            byte[] wave = Tools.LoadFileToByteArray(wavefile, 0, 256);
            return CheckWave(wave);
        }

        /// <summary>
        /// Checks if the given Wave is a proper PCM WAV file
        /// </summary>
        /// <param name="wavefile"></param>
        /// <returns></returns>
        public static bool CheckWave(byte[] wavefile)
        {
            if (wavefile[0] != 'R' ||
                wavefile[1] != 'I' ||
                wavefile[2] != 'F' ||
                wavefile[3] != 'F' ||

                wavefile[8] != 'W' ||
                wavefile[9] != 'A' ||
                wavefile[10] != 'V' ||
                wavefile[11] != 'E' ||

                wavefile[12] != 'f' ||
                wavefile[13] != 'm' ||
                wavefile[14] != 't' ||

                wavefile[20] != 0x01 || //Format = PCM
                wavefile[21] != 0x00 ||

                wavefile[34] != 0x10 || //Bitrate (16bit)
                wavefile[35] != 0x00
                ) return false;

            return true;
        }

        /// <summary>
        /// Returns the playlength of the Wave file in seconds
        /// </summary>
        /// <param name="wavefile"></param>
        /// <returns></returns>
        public static int GetWaveLength(string wavefile)
        {
            byte[] wave = Tools.LoadFileToByteArray(wavefile, 0, 256);
            return GetWaveLength(wave);
        }

        /// <summary>
        /// Returns the playlength of the Wave file in seconds
        /// </summary>
        /// <param name="wavefile"></param>
        /// <returns></returns>
        public static int GetWaveLength(byte[] wavefile)
        {
            if (CheckWave(wavefile) == true)
            {
                byte[] BytesPerSec = new byte[] { wavefile[28], wavefile[29], wavefile[30], wavefile[31] };
                int bps = BitConverter.ToInt32(BytesPerSec, 0);

                byte[] Chunksize = new byte[] { wavefile[4], wavefile[5], wavefile[6], wavefile[7] };
                int chunks = BitConverter.ToInt32(Chunksize, 0);

                return Math.Abs(chunks / bps);
            }
            else
                throw new Exception("This is not a supported PCM Wave file!");
        }

        /// <summary>
        /// Converts a wave file to a sound.bin
        /// </summary>
        /// <param name="wavefile"></param>
        /// <param name="soundbin"></param>
        public static void WaveToSoundBin(string wavefile, string soundbin, bool compress)
        {
            if (CheckWave(wavefile) == true)
            {
                byte[] sound = Tools.LoadFileToByteArray(wavefile);
                if (compress == true) sound = Lz77.Compress(sound);
                sound = U8.AddHeaderIMD5(sound);
                Wii.Tools.SaveFileFromByteArray(sound, soundbin);
            }
            else
                throw new Exception("This is not a supported 16bit PCM Wave file!");
        }

        /// <summary>
        /// Converts a sound.bin to a wave file
        /// </summary>
        /// <param name="soundbin"></param>
        /// <param name="wavefile"></param>
        public static void SoundBinToAudio(string soundbin, string audiofile)
        {
            FileStream fs = new FileStream(soundbin, FileMode.Open);
            byte[] audio = new byte[fs.Length - 32];
            int offset = 0;

            fs.Seek(32, SeekOrigin.Begin);
            fs.Read(audio, 0, audio.Length);
            fs.Close();

            if ((offset = Lz77.GetLz77Offset(audio)) != -1)
                audio = Lz77.Decompress(audio, offset);

            Tools.SaveFileFromByteArray(audio, audiofile);
        }

        /// <summary>
        /// Converts a BNS file to a sound.bin
        /// </summary>
        /// <param name="bnsFile"></param>
        /// <param name="soundBin"></param>
        public static void BnsToSoundBin(string bnsFile, string soundBin, bool compress)
        {
            byte[] bns = Tools.LoadFileToByteArray(bnsFile);

            if (bns[0] != 'B' || bns[1] != 'N' || bns[2] != 'S')
                throw new Exception("This is not a supported BNS file!");

            if (compress) bns = Lz77.Compress(bns);
            bns = U8.AddHeaderIMD5(bns);

            Tools.SaveFileFromByteArray(bns, soundBin);
        }

        /// <summary>
        /// Returns the length of the BNS audio file in seconds
        /// </summary>
        /// <param name="bnsFile"></param>
        /// <returns></returns>
        public static int GetBnsLength(string bnsFile)
        {
            byte[] temp = Tools.LoadFileToByteArray(bnsFile, 0, 100);
            return GetBnsLength(temp);
        }

        /// <summary>
        /// Returns the length of the BNS audio file in seconds
        /// </summary>
        /// <param name="bnsFile"></param>
        /// <returns></returns>
        public static int GetBnsLength(byte[] bnsFile)
        {
            byte[] temp = new byte[4];
            temp[0] = bnsFile[45];
            temp[1] = bnsFile[44];

            int sampleRate = BitConverter.ToInt16(temp, 0);

            temp[0] = bnsFile[55];
            temp[1] = bnsFile[54];
            temp[2] = bnsFile[53];
            temp[3] = bnsFile[52];

            int sampleCount = BitConverter.ToInt32(temp, 0);

            return sampleCount / sampleRate;
        }
    }

}