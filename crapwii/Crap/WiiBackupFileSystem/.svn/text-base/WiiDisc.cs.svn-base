//-------------------------------------
// WBFSSync - WBFSSync.exe
//
// Copyright 2009 Caian (ÔmΣga Frøst) <frost.omega@hotmail.com> based on wiidisc.c:
// Copyright 2009 Kwiirk based on negentig.c:
// Copyright 2007,2008  Segher Boessenkool  <segher@kernel.crashing.org>
//
// WBFSSync is Licensed under the terms of the Microsoft Reciprocal License (Ms-RL)
//
// WiiDisc.cs:
//
// Implementa funções de decodificação do conteúdo de um disco de Wii
//
//-------------------------------------

using System;
using System.Collections.Generic;
//using System.Linq;
using System.Text;
using System.IO;

namespace WBFSSync
{
    // Tipos de partições presentes no disco
    public enum PartitionSelection : uint
    {
        UpdatePartitionType = 0,
        GamePartitionType = 1,
        OtherPartitionType = 2,
        //Outros valores selecionam partições daqueles valores
        AllPartitions = 0xFFFFFFFF - 3,
        RemoveUpdatePartition = 0xFFFFFFFF - 2,
        OnlyGamePartition = 0xFFFFFFFF - 1
    }

    // Estrutura para mapear arquivos nas partições do disco
    public struct DiscFile
    {
        public string Name;
        public uint Offset;
        public uint Size;

        public int Partition;

        public override string ToString()
        {
            if (Name != null)
            {
                if (Name != "")
                {
                    return Name;
                }
            }

            return base.ToString();
        }
    }

    // Estrutura para mapear partições do disco
    public struct DiscPartition
    {
        public int Index;
        public uint RawOffset;
        public uint Block;

        public uint DataOffset;
        public uint DataSize;

        public uint Type;

        public DiscFile[] Files;
        internal int NumFiles { get { return Files == null ? 0 : Files.Length; } }
    }

    public class WiiDisc
    {
        //---------------------------- Constantes

        public const string ext_wiiiso = ".iso";
        public const string typefilter_wiiiso = "*.iso";

        //---------------------------- Variáveis
        IIOContext file = null;
        bool isfileopen = false;

        public String name = "";
        public String code = "";

        public byte[] SectorUsageTable = null;
        public uint UsedSectors = 0;

        public byte[] HeaderCopy = null;

        public uint PartitionRawOffset = 0;
        public uint PartitionDataOffset = 0;
        public uint PartitionDataSize = 0;
        public uint PartitionBlock = 0;

        internal byte[] TempBuffer = null;
        internal byte[] TempBuffer2 = null;

        public byte[] DiscKey = new byte[16];

        public PartitionSelection Partition = 0;

        public Boolean GenerateExtendedInfo = false;

        //---------- Informações extras

        public DiscPartition[] Partitions = null; //Partições do disco + Arquivos
        public int NumPartitions { get { return Partitions == null ? 0 : Partitions.Length; } }
        public Boolean HasUpdate = false;

        int fileIndex = 0; //Arquivo atual
        int partIndex = 0; //Partição atual

        //---------------------------- Rotinas

        //--------------------------- Construtor, inicializa a classe com um contexto de arquivo
        public WiiDisc(IIOContext context, bool generateExtendedInfo)
        {
            file = context;
            Partition = PartitionSelection.AllPartitions;
            GenerateExtendedInfo = generateExtendedInfo;
        }

        //--------------------------- Abre o disco e trava o Contexto
        public int Open()
        {
            //Verifica se o Contexto já está trancado, se não, abra
            isfileopen = file.Locked;
            if (!isfileopen)
            {
                int l = (int)file.Lock();
                if (l != (int)IORet.RET_IO_OK) return l;
            }

            TempBuffer = new byte[0x8000];
            TempBuffer2 = new byte[0x8000];

            return 0;
        }

        public int BuildDisc(PartitionSelection selector)
        {
            SectorUsageTable = new byte[WBFSDevice.wiiSectorsPerDisc];
            Partition = selector;

            int r = DoDisc();

            UsedSectors = 0;
            if (r == 0) for (int i = 0; i < SectorUsageTable.Length; i++) if (SectorUsageTable[i] != 0) UsedSectors++;

            Partition = PartitionSelection.AllPartitions;
            return r;
        }

        public int DoDisc()
        {
            byte[] b = new byte[256];
            uint nPartitions = 0;
            uint magic = 0;

            DiscRead(0, b, 256);
            magic = _be32(b, 24);
            if (magic != 0x5D1C9EA3)
            {
                return (int)WBFSRet.RET_WBFS_NOTAWIIDISC;
            }

            HeaderCopy = new byte[256];
            b.CopyTo(HeaderCopy, 0);

            //Lê o código do jogo de 6 caracteres
            int j = WBFSDevice.IsoCodePos;
            byte c = 0;
            for (j = WBFSDevice.IsoCodePos; j < WBFSDevice.IsoCodePos + WBFSDevice.IsoCodeLen; j++) 
                code += (char)b[j];

            //Lê o nome do jogo
            j = WBFSDevice.IsoNamePos;
            while (((c = b[j++]) != 0) && (j < WBFSDevice.IsoNamePos + WBFSDevice.IsoNameLen)) name += (char)c;

            DiscRead(0x40000 >> 2, b, 256);
            nPartitions = _be32(b, 0);
            DiscRead(_be32(b, 4), b, 256);

            if (GenerateExtendedInfo)
            {
                Partitions = new DiscPartition[nPartitions];
                //----------
                partIndex = 0;
                fileIndex = 0;
            }

            for (int i = 0; i < nPartitions; i++)
            {
                PartitionRawOffset = _be32(b, 8 * (uint)i);
                if (!TestParitionSkip(_be32(b, 8 * (uint)i + 4), Partition))
                {
                    DoPartition();
                }

                if (GenerateExtendedInfo)
                {
                    Partitions[partIndex].Type = (_be32(b, 8 * (uint)i + 4));
                    HasUpdate = HasUpdate || (Partitions[partIndex].Type == 1);

                    //----------
                    partIndex++;
                    fileIndex = 0;
                }

            }

            b = null;
            return (int)WBFSRet.RET_WBFS_OK;
        }

        public void DiscRead(uint offset, byte[] data, uint length)
        {
            if (data != null)
            {
                if (length == 0) return;
                if (file.Read(data, ((long)offset) << 2, (int)length) < 0)
                {
                    //Nada
                }
            }

            if (SectorUsageTable != null)
            {
                uint blockno = offset >> 13;
                do
                {
                    SectorUsageTable[blockno] = 1;
                    blockno++;

                    if (length > 0x8000)
                        length -= 0x8000;

                } while (length > 0x8000);
            }
        }

        public static Boolean TestParitionSkip(uint partitionType, PartitionSelection partitionSelector)
        {
            switch (partitionSelector)
            {
                case PartitionSelection.AllPartitions:
                    return false;
                case PartitionSelection.RemoveUpdatePartition:
                    return (partitionType == 1);
                case PartitionSelection.OnlyGamePartition:
                    return (partitionType != 0);
                default:
                    return (partitionType != (uint)partitionSelector);
            }
        }

        public void DoPartition()
        {
            byte[] ticket = new byte[676];
            byte[] b = new byte[28];

            PartitionRawRead(0, ticket, 676);
            PartitionRawRead(676 >> 2, b, 28);

            uint tmd_size = _be32(b, 0);
            uint tmd_offset = _be32(b, 4);

            uint cert_size = _be32(b, 8);
            uint cert_offset = _be32(b, 12);

            uint h3_offset = _be32(b, 16);

            PartitionDataOffset = _be32(b, 20);
            PartitionBlock = (PartitionRawOffset + PartitionDataOffset) >> 13;

            byte[] tmd = new byte[tmd_size];
            PartitionRawRead(tmd_offset, tmd, tmd_size);

            byte[] cert = new byte[cert_size];
            PartitionRawRead(cert_offset, cert, cert_size);

            DecryptTitleKey(ticket, DiscKey);

            PartitionRawRead(h3_offset, null, 0x18000);

            if (GenerateExtendedInfo)
            {
                Partitions[partIndex].RawOffset = PartitionRawOffset;
                Partitions[partIndex].DataOffset = PartitionDataOffset;
                Partitions[partIndex].DataSize = PartitionDataSize;
                Partitions[partIndex].Block = PartitionBlock;
            }

            b = null;
            ticket = null;
            cert = null;
            tmd = null;

            DoFiles();
        }

        public void PartitionRawRead(uint offset, byte[] data, uint length)
        {
            DiscRead(PartitionRawOffset + offset, data, length);
        }

        public void DecryptTitleKey(byte[] tiket, byte[] title)
        {
            byte[] common_key = new byte[] { 0xeb, 0xe4, 0x2a, 0x22, 0x5e, 0x85, 0x93, 0xe4, 0x48, 0xd9, 0xc5, 0x45,
                0x73, 0x81, 0xaa, 0xf7 };
            byte[] iv = new byte[16];

            Array.Copy(tiket, 476, iv, 0, 8);
            for (int i = 8; i < 16; i++) iv[i] = 0;

            byte[] tiketb = new byte[tiket.LongLength - 0x01bf];
            Array.Copy(tiket, 0x01bf, tiketb, 0, tiketb.LongLength);

            AESRijndael.aes_set_key(common_key);
            AESRijndael.aes_decrypt(iv, tiketb, title, 16);
        }

        public void DoFiles()
        {
            byte[] b = new byte[0x480];
            PartitionRead(0, b, 0x480, false);

            uint dol_offset = _be32(b, 0x0420);
            uint fst_offset = _be32(b, 0x0424);
            uint fst_size = _be32(b, 0x0428) << 2;

            uint apl_offset = 0x2440 >> 2;
            byte[] apl_header = new byte[0x20];
            PartitionRead(apl_offset, apl_header, 32, false);
            uint apl_size = 0x20 + _be32(apl_header, 0x14) + _be32(apl_header, 0x18);

            PartitionRead(apl_offset, null, apl_size, true);
            PartitionRead(dol_offset, null, (fst_offset - dol_offset) << 2, true);

            byte[] fst = new byte[fst_size];
            PartitionRead(fst_offset, fst, fst_size, false);

            uint nFiles = _be32(fst, 8);

            if (GenerateExtendedInfo)
            {
                Partitions[partIndex].Files = new DiscFile[nFiles];
            }

            if (nFiles > 1)
            {
                DoFst(fst, 12 * nFiles, 0);
            }

            if (GenerateExtendedInfo)
            {
                DiscFile[] trim = new DiscFile[fileIndex];
                Array.Copy(Partitions[partIndex].Files, trim, fileIndex);

                Partitions[partIndex].Files = null;
                Partitions[partIndex].Files = trim;
                trim = null;
            }

            b = null;
            apl_header = null;
            fst = null;

        }

        public void PartitionRead(uint offset, byte[] data, uint length, bool fake)
        {
            if (fake && (SectorUsageTable == null)) return;

            uint offset_in_block = 0;
            uint length_in_block = 0;

            uint p = 0;

            while (length != 0)
            {
                offset_in_block = offset % (31744 >> 2);
                length_in_block = 31744 - (offset_in_block << 2);

                if (length_in_block > length) length_in_block = length;

                if (!fake)
                {
                    PartitionReadBlock(offset / (31744 >> 2), TempBuffer2);
                    Array.Copy(TempBuffer2, offset_in_block << 2, data, p, length_in_block);
                }
                else
                {
                    SectorUsageTable[PartitionBlock + (offset / (31744 >> 2))] = 1;
                }

                p += length_in_block;
                offset += (length_in_block >> 2);
                length -= length_in_block;
            }
        }

        public void PartitionReadBlock(uint blockno, byte[] block)
        {
            if (SectorUsageTable != null)
            {
                SectorUsageTable[PartitionBlock + blockno] = 1;
            }

            uint offset = PartitionDataOffset + ((0x8000 >> 2) * blockno);
            PartitionRawRead(offset, TempBuffer, 0x8000);


            //Decodifica os dados
            byte[] iv = new byte[16];
            Array.Copy(TempBuffer, 0x3d0, iv, 0, 16);

            byte[] raw = new byte[0x7c00];
            Array.Copy(TempBuffer, 0x400, raw, 0, 0x7c00);

            AESRijndael.aes_set_key(DiscKey);
            AESRijndael.aes_decrypt(iv, raw, block, 0x7c00);
        }

        public uint DoFst(byte[] fst, uint namespos, uint i)
        {
            uint size = _be32(fst, 12 * i + 8);

            if (i == 0)
            {
                for (uint j = 1; j < size; )
                {
                    j = DoFst(fst, namespos, j);
                }

                return size;
            }

            if (fst[12 * i] != 0)
            {
                for (uint j = i + 1; j < size; )
                {
                    j = DoFst(fst, namespos, j);
                }
                return size;
            }
            else
            {
                uint offset = _be32(fst, 12 * i + 4);
                PartitionRead(offset, null, size, true);

                if (GenerateExtendedInfo)
                {

                    String name = "";
                    Char c = '\0';
                    int k = (int)(namespos + (_be32(fst, 12 * i) & 0x00ffffff));
                    while ((c = (char)fst[k]) != '\0') { name += c; k++; }

                    DiscFile f = new DiscFile();
                    f.Name = name;
                    f.Size = size;
                    f.Offset = offset;
                    f.Partition = partIndex;

                    Partitions[partIndex].Files[fileIndex++] = f;
                }

                return i + 1;
            }
        }

        public static int FixPartitionTable(PartitionSelection partsel, byte[] partTable)
        {
            if (partsel == PartitionSelection.AllPartitions) return 0;
            uint nPartitions = _be32(partTable, 0);

            if ((_be32(partTable, 4) - 0x10000) > 0x50)
            {
                return -1;
            }

            uint part_offset = 0;
            uint part_type = 0;
            uint b = (_be32(partTable, 4) - 0x10000) * 4;
            uint j = 0;
            byte[] v;

            for (uint i = 0; i < nPartitions; i++)
            {
                part_offset = _be32(partTable, (b + 8 * i));
                part_type = _be32(partTable, (b + 8 * i + 4));

                if (!TestParitionSkip(part_type, partsel))
                {
                    v = BitConverter.GetBytes((uint)System.Net.IPAddress.HostToNetworkOrder((int)part_offset));
                    partTable[b + 8 * j] = v[0];
                    partTable[b + 8 * j + 1] = v[1];
                    partTable[b + 8 * j + 2] = v[2];
                    partTable[b + 8 * j + 3] = v[3];

                    v = BitConverter.GetBytes((uint)System.Net.IPAddress.HostToNetworkOrder((int)part_type));
                    partTable[b + 8 * j + 4] = v[0];
                    partTable[b + 8 * j + 5] = v[1];
                    partTable[b + 8 * j + 6] = v[2];
                    partTable[b + 8 * j + 7] = v[3];

                    j++;
                }
            }

            v = BitConverter.GetBytes((uint)System.Net.IPAddress.HostToNetworkOrder((int)j));
            partTable[0] = v[0];
            partTable[1] = v[1];
            partTable[2] = v[2];
            partTable[3] = v[3];

            v = null;
            return 0;
        }

        //--------------------------- Rotinas de extração de arquivos

        public void ExtractFile(String outfile, int partition, int disc)
        {
            byte[] buffer;
            ExtractFile(out buffer, partition, disc);

            File.WriteAllBytes(outfile, buffer);
            buffer = null;
        }

        public void ExtractFile(String outfile, int partition, String name)
        {
            byte[] buffer;
            ExtractFile(out buffer, partition, name);

            File.WriteAllBytes(outfile, buffer);
            buffer = null;
        }

        public void ExtractFile(String outfile, String name)
        {
            byte[] buffer;
            ExtractFile(out buffer, name);

            File.WriteAllBytes(outfile, buffer);
            buffer = null;
        }

        public void ExtractFile(String outfile, DiscFile discfile)
        {
            byte[] buffer;
            ExtractFile(out buffer, discfile);

            File.WriteAllBytes(outfile, buffer);
            buffer = null;
        }

        public void ExtractFile(out byte[] buffer, int partition, int disc)
        {
            ExtractFile(out buffer, Partitions[partition].Files[disc]);
        }

        public void ExtractFile(out byte[] buffer, int partition, String name)
        {
            buffer = null;

            for (int i = 0; i < Partitions[partition].Files.Length; i++)
            {
                if (Partitions[partition].Files[i].Name == name)
                {
                    ExtractFile(out buffer, Partitions[partition].Files[i]);
                    return;
                }
            }
        }

        public List<DiscFile> ListDols(int partition)
        {
            List<DiscFile> files = new List<DiscFile>();
            for (int i = 0; i < Partitions[partition].Files.Length; i++)
            {
                if (Partitions[partition].Files[i].Name.EndsWith(".dol"))
                {
                    files.Add(Partitions[partition].Files[i]);
                }
            }

            return files;
        }

        public void ExtractFile(out byte[] buffer, String name)
        {
            buffer = null;

            for (int j = 0; j < Partitions.Length; j++)
            {
                for (int i = 0; i < Partitions[j].Files.Length; i++)
                {
                    if (Partitions[j].Files[i].Name == name)
                    {
                        ExtractFile(out buffer, Partitions[j].Files[i]);
                        return;
                    }
                }
            }
        }

        public void ExtractFile(out byte[] buffer, DiscFile discfile)
        {
            buffer = new byte[discfile.Size];

            //Salva as configurações antigas da partição
            uint pro = PartitionRawOffset;
            uint pdo = PartitionDataOffset;
            uint pds = PartitionDataSize;
            uint pbl = PartitionBlock;

            //Seta as configurações para a partição alvo

            PartitionRawOffset = Partitions[discfile.Partition].RawOffset;
            PartitionDataOffset = Partitions[discfile.Partition].DataOffset;
            PartitionDataSize = Partitions[discfile.Partition].DataSize;
            PartitionBlock = Partitions[discfile.Partition].Block;

            //Lê o arquivo
            PartitionRead(discfile.Offset, buffer, discfile.Size, false);

            //Restaura as configurações antigas de partição
            PartitionRawOffset = pro;
            PartitionDataOffset = pdo;
            PartitionDataSize = pds;
            PartitionBlock = pbl;
        }

        //--------------------------- Fecha o disco e destrava o Contexto
        public void Close()
        {
            //Se o contexto não estava trancado, o contexto deve ser fechado pela rotina que o criou
            if (!isfileopen)
            {
                file.Unlock();
            }

            file = null;
            TempBuffer = null;
            TempBuffer2 = null;
            SectorUsageTable = null;
            HeaderCopy = null;
            DiscKey = null;

            if (Partitions != null)
            {
                for (int i = 0; i < Partitions.Length; i++)
                {
                    Partitions[i].Files = null;
                }
            }

            Partitions = null;
        }

        //--------------------------- Verifica se o arquivo é uma iso de Wii
        public static Int32 IsWiiIso(String file)
        {
            IIOContext context = IOManager.CreateIOContext("ISWIIISO", file, FileAccess.Read, FileShare.ReadWrite,
                0, FileMode.Open, EFileAttributes.None);
            if (context.Result != (int)IORet.RET_IO_OK)
            {
                //Loga erro
                return context.Result;
            }
            else
            {
                int r = (int)context.Lock();
                if (r != (int)IORet.RET_IO_OK)
                { 
                    //Loga erro
                    context.Close(); 
                    return r;
                }

                Byte[] header = new Byte[256];
                if(context.Read(header, 256) != IORet.RET_IO_OK)
                {
                    //Loga erro

                    r = context.Result;

                    context.Unlock();
                    context.Close();
                    return r;
                }

                r = IsWiiIso(header);

                context.Unlock();
                context.Close();

                return r;
            }
        }

        public static int IsWiiIso(Byte[] header)
        {
            int r;

            uint magic = _be32(header, 24);
            if (magic != 0x5D1C9EA3)
            {
                r = (int)WBFSRet.RET_WBFS_NOTAWIIDISC;
            }
            else
            {
                r = (int)WBFSRet.RET_WBFS_OK;
            }
            return r;
        }

        //--------------------------- Retorna as informações da iso de Wii
        public static Int32 GetIsoInformation(String file, out string code, out string name, out int region, out long used)
        {
            code = "";
            name = "";
            region = -1;
            used = 0;

            IIOContext context = IOManager.CreateIOContext("ISWIIISO", file, FileAccess.Read, FileShare.ReadWrite,
                0, FileMode.Open, EFileAttributes.None);
            if (context.Result != (int)IORet.RET_IO_OK)
            {
                //Loga erro
                return context.Result;
            }

            int r = 0;

            WiiDisc d = new WiiDisc(context, false);
            d.Open();
            r = d.BuildDisc(PartitionSelection.OnlyGamePartition);
            if (r == 0)
            {

                name = d.name;
                code = d.code;
                used = d.UsedSectors;

                Byte[] bregion = new byte[4];
                if (context.Read(bregion, WBFSDevice.IsoRegionPos, 4) != IORet.RET_IO_OK)
                {
                    //Loga erro

                    region = -1;
                }
                else
                {
                    region = (int)System.Net.IPAddress.NetworkToHostOrder((int)BitConverter.ToUInt32(bregion, 0));
                }

                bregion = null;
                d.Close();
                context.Close();

                return 0;
            }
            else
            {
                d.Close();
                context.Close();

                return r;
            }
        }

        //--------------------------- Retorna as informações da iso de Wii
        public static Int32 GetIsoInformation(String file, out string code, out string name, out long used)
        {
            code = "";
            name = "";
            used = 0;

            IIOContext context = IOManager.CreateIOContext("ISWIIISO", file, FileAccess.Read, FileShare.ReadWrite,
                0, FileMode.Open, EFileAttributes.None);
            if (context.Result != (int)IORet.RET_IO_OK)
            {
                //Loga erro
                return context.Result;
            }

            int r = 0;

            WiiDisc d = new WiiDisc(context, false);
            d.Open();
            r = d.BuildDisc(PartitionSelection.OnlyGamePartition);
            if (r == 0)
            {

                name = d.name;
                code = d.code;
                used = d.UsedSectors;

                d.Close();
                context.Close();

                return 0;
            }
            else
            {
                d.Close();
                context.Close();

                return r;
            }
        }

        //--------------------------- Retorna as informações da iso de Wii
        public static Int32 GetIsoInformation(String file, out string code, out string name, out int region)
        {
            code = "";
            name = "";
            region = -1;

            IIOContext context = IOManager.CreateIOContext("ISWIIISO", file, FileAccess.Read, FileShare.ReadWrite,
                0, FileMode.Open, EFileAttributes.None);
            if (context.Result != (int)IORet.RET_IO_OK)
            {
                //Loga erro
                return context.Result;
            }
            else
            {
                int r = (int)context.Lock();
                if (r != (int)IORet.RET_IO_OK)
                {
                    //Loga erro
                    context.Close();
                    return r;
                }

                Byte[] header = new Byte[256];
                if (context.Read(header, 256) != IORet.RET_IO_OK)
                {
                    //Loga erro

                    r = context.Result;

                    context.Unlock();
                    context.Close();
                    header = null;
                    return r;
                }

                uint magic = _be32(header, 24);
                if (magic != 0x5D1C9EA3)
                {
                    context.Unlock();
                    context.Close();
                    header = null;
                    return (int)WBFSRet.RET_WBFS_NOTAWIIDISC;
                }

                //Lê o código do jogo de 6 caracteres
                int j = WBFSDevice.IsoCodePos;
                byte c = 0;
                for (j = WBFSDevice.IsoCodePos; j < WBFSDevice.IsoCodePos + WBFSDevice.IsoCodeLen; j++)
                    code += (char)header[j];

                //Lê o nome do jogo
                j = WBFSDevice.IsoNamePos;
                while (((c = header[j++]) != 0) && (j < WBFSDevice.IsoNamePos + WBFSDevice.IsoNameLen)) name += (char)c;

                if (context.Read(header, WBFSDevice.IsoRegionPos, 4) != IORet.RET_IO_OK)
                {
                    //Loga erro

                    r = context.Result;

                    context.Unlock();
                    context.Close();
                    header = null;
                    return r;
                }

                region = (int)System.Net.IPAddress.NetworkToHostOrder((int)BitConverter.ToUInt32(header, 0));

                context.Unlock();
                context.Close();

                header = null;

                return 0;
            }
        }

        //--------------------------- Retorna as informações da iso de Wii
        public static Int32 SetIsoInformation(String file, string code, string name)
        {
            IIOContext context = IOManager.CreateIOContext("ISWIIISO", file, FileAccess.ReadWrite, FileShare.Read,
                0, FileMode.Open, EFileAttributes.None);
            if (context.Result != (int)IORet.RET_IO_OK)
            {
                //Loga erro
                return context.Result;
            }
            else
            {
                int r = (int)context.Lock();
                if (r != (int)IORet.RET_IO_OK)
                {
                    //Loga erro
                    context.Close();
                    return r;
                }

                Byte[] header = new Byte[256];
                if (context.Read(header, 256) != IORet.RET_IO_OK)
                {
                    //Loga erro

                    r = context.Result;

                    context.Unlock();
                    context.Close();
                    return r;
                }

                uint magic = _be32(header, 24);
                if (magic != 0x5D1C9EA3)
                {
                    context.Unlock();
                    context.Close();
                    header = null;

                    return (int)WBFSRet.RET_WBFS_NOTAWIIDISC;
                }

                header = null;

                //


                if (code.Length != 0)
                {
                    if (code.Length != WBFSDevice.IsoCodeLen)
                    {
                        if (code.Length < WBFSDevice.IsoCodeLen) while (code.Length < WBFSDevice.IsoCodeLen) code += '0';
                        else code = code.Substring(0, WBFSDevice.IsoCodeLen); //Eu preciso fazer isso para o GetDiscByCode()
                    }

                    context.Seek(WBFSDevice.IsoCodePos);
                    context.Write(code, false);
                }

                if (name.Length != 0)
                {
                    if (name.Length > WBFSDevice.IsoNameLen)
                    {
                        name = name.Substring(0, WBFSDevice.IsoNameLen);
                    }

                    context.Seek(WBFSDevice.IsoNamePos);
                    context.Write(name, false);
                    for (int i = name.Length; i < WBFSDevice.IsoNameLen; i++) context.Write((byte)0);
                }

                //
                context.Unlock();
                context.Close();

                return 0;
            }
        }

        //--------------------------- Misc.
        public static uint _be32(byte[] p, uint i)
        {
            return ((uint)(p[i]) << 24) | ((uint)(p[i + 1]) << 16) | ((uint)(p[i + 2]) << 8) | (uint)(p[i + 3]);
        }
    }
}
