//-------------------------------------
// WBFSSync - WBFSSync.exe
//
// Copyright 2009 Caian (ÔmΣga Frøst) <frost.omega@hotmail.com> based on libwbfs.c:
// Copyright 2009 Kwiirk
//
// WBFSSync is Licensed under the terms of the Microsoft Reciprocal License (Ms-RL)
//
// Disc.cs:
//
// Implementa a classe básica de um disco de Wii salvo no dispositivo
//
//-------------------------------------

using System;
using System.Collections.Generic;
//using System.Linq;
using System.Text;
using System.Threading;

namespace WBFSSync
{
    internal class Disc : IDisc, IComparable<Disc>
    {
        //-------------- Variáveis para a classe Disc e interface IDisc

        public WBFSDevice device = null;

        public bool deleted = false;
        public bool isOpen = false;

        public int referenceCounter = 0;

        public int index = -1;
        public string name = "";
        public string code = "";
        public int region = -1;

        public long size = 0;
        public long scrubbedsize = 0;

        public ushort[] wlbaTable = null;
        public long discInfoLbaPosition = 0;
        public uint usedwbfssectors = 0;

        //-------------- Implementação da interface IDisc

        public bool Deleted { get { return deleted; } }
        public bool IsOpen { get { return isOpen; } }
        public int Counter { get { return referenceCounter; } }

        public int Index { get { return index; } }
        public string Name { get { return name; } set { device.ChangeDiscHeader(this, value, code); } }
        public string Code { get { return code; } set { device.ChangeDiscHeader(this, name, value); } }
        public DiscRegion Region { get { return (DiscRegion)region; } }

        public Int64 Size { get { return size; } }
        public Int64 WBFSSize { get { return scrubbedsize; } }
        
        public ushort[] WLBATable { get { return wlbaTable; } }
        public long DiscInfoLBAPosition { get { return discInfoLbaPosition; } }
        public uint UsedWBFSSectors { get { return usedwbfssectors; } }

        public void GetLBAPosAndSectorOffset(long position, out long drivelba, out uint offset)
        {
            if (WLBATable == null) { drivelba = 0; offset = 0; return; }

            ushort iwlba = (ushort)(position >> device.wbfsSectorSize_s);
            uint iwlbaShift = (uint)(device.wbfsSectorSize_s - device.hdSectorSize_s);
            uint lbaMask = (device.wbfsSectorSize - 1) >> device.hdSectorSize_s;
            uint lba = (uint)((position >> device.hdSectorSize_s) & lbaMask);

            offset = (uint)(position & (device.hdSectorSize - 1));
            drivelba = device.PartitionOffsetLBA + (((long)WLBATable[iwlba]) << (int)iwlbaShift) + lba;

        }

        public void GetLBAPosAndSectorOffset(long position, out long drivelba, out uint offset, out long wbfssecpos)
        {
            if (WLBATable == null) { drivelba = 0; offset = 0; wbfssecpos = 0; return; }

            ushort iwlba = (ushort)(position >> device.wbfsSectorSize_s);
            uint iwlbaShift = (uint)(device.wbfsSectorSize_s - device.hdSectorSize_s);
            uint lbaMask = (device.wbfsSectorSize - 1) >> device.hdSectorSize_s;
            uint lba = (uint)((position >> device.hdSectorSize_s) & lbaMask);

            offset = (uint)(position & (device.hdSectorSize - 1));
            wbfssecpos = ((long)WLBATable[iwlba]) << (int)iwlbaShift;
            drivelba = device.PartitionOffsetLBA + wbfssecpos + lba;

        }

        public uint BuildDiscSectorUsage(out byte[] sectorUsage)
        {
            sectorUsage = new byte[WBFSDevice.wiiSectorsPerDisc];

            int j = 0; //Setor de wii
            int i = 0; //Setor wbfs
            uint count = 0;

            for (i = 0; i < device.wbfsSectorsPerDisc; i++)
            {
                if (wlbaTable[i] != 0)
                {
                    int p = (int)(device.wiiSectorsPerWBFSSector * i);
                    for (j = p; j < p + device.wiiSectorsPerWBFSSector; j++)
                    {
                        sectorUsage[j] = 1;
                        count++;
                    }
                }
            }

            return count;
        }

        public int Delete()
        {
            return device.DeleteDisc(this);
        }

        public IIOContext GetDeviceContext()
        {
            return device.device;
        }

        internal void Dispose()
        {
            deleted = true;
            isOpen = false;

            referenceCounter = 0;

            index = -1;
            name = "";
            code = "";
            region = -1;

            size = 0;
            scrubbedsize = 0;

            wlbaTable = null;
            discInfoLbaPosition = 0;
        }

        //-------------- Implementação da interface IComparable

        public int CompareTo(Disc other)
        {
            return other == null ? 0 : this.index - other.index;
        }
    }
}
