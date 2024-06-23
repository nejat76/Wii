//-------------------------------------
// WBFSSync - WBFSSync.exe
//
// Copyright 2009 Caian (ÔmΣga Frøst) <frost.omega@hotmail.com>
//
// WBFSSync is Licensed under the terms of the Microsoft Reciprocal License (Ms-RL)
//
// MBRReader.cs:
//
// Classe que abre, lê e interpreta informações sobre um disco físico
//
//-------------------------------------

using System;
using System.Collections.Generic;
//using System.Linq;
using System.Text;
using System.IO;
using System.Net;

namespace WBFSSync
{
    public struct PARTITION
    {
        public bool Bootable;

        public byte StartingHead;
        public byte StartingSector;
        public ushort StartingCylinder;

        public byte System;

        public byte EndingHead;
        public byte EndingSector;
        public ushort EndingCylinder;

        public long FirstSector;
        public long NumberOfSectors;
    }

    public class MBRReader
    {
        //---------------------------- Constantes

        //---------------------------- Variáveis

        public int result = 0;

        PARTITION[] Partitions = new PARTITION[4];
        ushort Magic = 0;

        //---------------------------- Rotinas

        public MBRReader(string device)
        {
            IIOContext pdrive = IOManager.CreateIOContext("PDRIVE\\" + device, device, FileAccess.Read, FileShare.Read,
                0, FileMode.Open, EFileAttributes.NoBuffering);

            if (pdrive.Result != 0)
            {
                result = pdrive.Result;
                return;
            }

            if (pdrive.Lock() != IORet.RET_IO_OK)
            {
                //Loga a informação
                result = pdrive.Result;
                return;
            }

            Byte[] mbr = new byte[512];
            if (pdrive.Read(mbr, 512) != IORet.RET_IO_OK)
            {
                result = pdrive.Result;
                pdrive.Unlock();
                pdrive.Close();
                return;
            }

            //-----------------

            //MBRReader(byte[] mbr)

            //-----------------

            pdrive.Unlock();
            pdrive.Close();
        }

        public MBRReader(byte[] mbr)
        {
            for (int i = 0; i < 4; i++)
            {
                int off = 0x1BE + 16 * i;

                Partitions[i].Bootable = (mbr[off] == 0x80);

                Partitions[i].StartingHead = mbr[off + 1];
                Partitions[i].StartingSector = (byte)(mbr[off + 2] & 63);
                Partitions[i].StartingCylinder = (ushort)(((int)(mbr[off + 2] & 192) << 8) + mbr[off + 3]);

                Partitions[i].System = mbr[off + 4];

                Partitions[i].EndingHead = mbr[off + 5];
                Partitions[i].EndingSector = (byte)(mbr[off + 6] & 63);
                Partitions[i].EndingCylinder = (ushort)(((int)(mbr[off + 6] & 192) << 8) + mbr[off + 7]);

                Partitions[i].FirstSector = (mbr[off + 8]) + (mbr[off + 9] << 2) + (mbr[off + 10] << 4) +
                    (mbr[off + 11] << 6);

                Partitions[i].NumberOfSectors = (mbr[off + 12]) + (mbr[off + 13] << 2) + (mbr[off + 14] << 4) +
                    (mbr[off + 15] << 6);


            }
        }
    }
}
