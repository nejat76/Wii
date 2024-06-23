//-------------------------------------
// WBFSSync - WBFSSync.exe
//
// Copyright 2009 Caian (ÔmΣga Frøst) <frost.omega@hotmail.com> based on libwbfs.c:
// Copyright 2009 Kwiirk
//
// WBFSSync is Licensed under the terms of the Microsoft Reciprocal License (Ms-RL)
//
// DVDRomReader.cs:
//
// Implementa um leitor de disco na forma de contexto
//
//-------------------------------------

using System;
using System.Collections.Generic;
//using System.Linq;
using System.Text;
using System.IO;

namespace WBFSSync
{
    public class DVDRomReader : IIOContext
    {
        //-------------- Variáveis para a Interface IIOContext

        long discPosition = 0;
        long deviceLBA = 0;
        uint offset = 0;

        IIOContext context;
        Int32 sectorSize;
        long size;

        //-------------- Rotinas

        public DVDRomReader(Char drive)
        {
            context = IOManager.CreateIOContext("DVD-ROM " + drive, @"\\.\" + drive + ':', FileAccess.Read, FileShare.Read, 0,
                FileMode.Open, EFileAttributes.NoBuffering);

            context.Lock();

            DISK_GEOMETRY dg = context.GetDiskGeometry();
            sectorSize = (int)dg.BytesPerSector;

            PARTITION_INFORMATION pi = context.GetPartitionInformation();
            size = pi.PartitionLength;

            context.Unlock();

        }

        //-------------- Implementação da interface IIOContext

        public int Handle { get { return context.Handle; } }
        public long Position { get { return discPosition; } }
        public long Length { get { return size; } }
        public int Result { get { return context.Result; } }
        public bool Working { get { return context.Working; } }

        public String Name { get { return context.Name; } }
        public String Path { get { return context.Path; } }
        public bool Closed { get { return context.Closed; } }
        public bool CanWork { get { return context.CanWork; } }
        public bool Locked { get { return context.Locked; } }

        public IORet Seek(long pos, SeekOrigin origin)
        {
            switch (origin)
            {
                case SeekOrigin.Begin:
                    discPosition = pos;
                    break;
                case SeekOrigin.Current:
                    discPosition += pos;
                    break;
                case SeekOrigin.End:
                    discPosition = context.Length + pos;
                    break;
            }

            deviceLBA = discPosition / sectorSize; //Divisão de inteiros
            offset = (uint)(discPosition % sectorSize);

            return context.Seek(deviceLBA * sectorSize);
        }

        public IORet Seek(long pos)
        {
            discPosition = pos;

            deviceLBA = discPosition / sectorSize; //Divisão de inteiros
            offset = (uint)(discPosition % sectorSize);

            return context.Seek(deviceLBA * sectorSize);
        }

        public IORet SetEOF()
        {
            return IORet.RET_IO_ACCESS_DENIED; //Não seta o fim de um drive físico
        }

        public IORet Close()
        {
            return context.Close();
        }

        public IORet ForceClose()
        {
            return IORet.RET_IO_ACCESS_DENIED; //Só o WBFSDevice pode forçar o fechamento do handle
        }

        public IORet PrepareForAsync()
        {
            return context.PrepareForAsync();
        }

        public IORet FreeAsync()
        {
            return context.FreeAsync();
        }

        public IORet Lock()
        {
            return context.Lock();
        }

        public IORet Unlock()
        {
            return context.Unlock();
        }

        public IORet Read(Array buffer, int count) //Precisa ser uma matriz de bytes
        {
            if (count <= 0) return IORet.RET_IO_OK;
            int read = 0;
            IORet ret;
            while (count > 0)
            {
                int toRead = (int)Math.Min(count, sectorSize - offset);

                int absoluteRead = (int)((((offset + toRead - 1) / sectorSize) + 1) * sectorSize);

                Byte[] absoluteBuffer = new byte[absoluteRead];
                if ((ret = context.Read(absoluteBuffer, absoluteRead)) != IORet.RET_IO_OK) { return ret; }
                Array.Copy(absoluteBuffer, offset, buffer, read, toRead);

                read += toRead;
                count -= toRead;
                discPosition += toRead;

                Seek(discPosition);
            }

            return IORet.RET_IO_OK;
        }

        public IORet Write(Array buffer, int count)
        {
            return IORet.RET_IO_ACCESS_DENIED;
        }

        public IORet Read(Array buffer, long pos, int count)
        {
            Seek(pos);
            return Read(buffer, count);
        }

        public IORet Write(Array buffer, long pos, int count)
        {
            return IORet.RET_IO_ACCESS_DENIED;
        }

        // As funções assíncronas DEVEM possuir posição e tamanho multiplos inteiros do tamanho do setor do drive e
        // 'count' deve ser menor ou igual ao tamanho do setor WBFS

        public IORet ReadAsync(Array buffer, int count)
        {
            return context.ReadAsync(buffer, count);
        }

        public IORet WriteAsync(Array buffer, int count)
        {
            return IORet.RET_IO_ACCESS_DENIED;
        }

        public IORet ReadAsync(Array buffer, long pos, int count)
        {
            return context.ReadAsync(buffer, pos, count);
        }

        public IORet WriteAsync(Array buffer, long pos, int count)
        {
            return IORet.RET_IO_ACCESS_DENIED;
        }

        public PARTITION_INFORMATION GetPartitionInformation()
        {
            return context.GetPartitionInformation();
        }

        public DISK_GEOMETRY GetDiskGeometry()
        {
            return context.GetDiskGeometry();
        }

        public byte ReadByte()
        {
            return context.ReadByte();
        }

        public char ReadChar()
        {
            return context.ReadChar();
        }

        public int ReadInt32()
        {
            return context.ReadInt32();
        }

        public uint ReadUInt32()
        {
            return context.ReadUInt32();
        }

        public short ReadInt16()
        {
            return context.ReadInt16();
        }

        public ushort ReadUInt16()
        {
            return context.ReadUInt16();
        }

        public long ReadInt64()
        {
            return context.ReadInt64();
        }

        public ulong ReadUInt64()
        {
            return context.ReadUInt64();
        }

        public string ReadString(bool unicode)
        {
            return context.ReadString(unicode);
        }

        public string ReadString(bool unicode, int max)
        {
            return context.ReadString(unicode, max);
        }

        public IORet Write(byte v)
        {
            return IORet.RET_IO_ACCESS_DENIED;
        }

        public IORet Write(char v)
        {
            return IORet.RET_IO_ACCESS_DENIED;
        }

        public IORet Write(short v)
        {
            return IORet.RET_IO_ACCESS_DENIED;
        }

        public IORet Write(ushort v)
        {
            return IORet.RET_IO_ACCESS_DENIED;
        }

        public IORet Write(int v)
        {
            return IORet.RET_IO_ACCESS_DENIED;
        }

        public IORet Write(uint v)
        {
            return IORet.RET_IO_ACCESS_DENIED;
        }

        public IORet Write(long v)
        {
            return IORet.RET_IO_ACCESS_DENIED;
        }

        public IORet Write(ulong v)
        {
            return IORet.RET_IO_ACCESS_DENIED;
        }

        public IORet Write(string s, bool unicode)
        {
            return context.Write(s, unicode);
        }
    }
}
