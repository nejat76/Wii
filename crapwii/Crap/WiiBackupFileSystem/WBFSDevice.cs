//-------------------------------------
// WBFSSync - WBFSSync.exe
//
// Copyright 2009 Caian (ÔmΣga Frøst) <frost.omega@hotmail.com> based on wbfs.c and libwbfs.c:
// Copyright 2009 Kwiirk
//
// WBFSSync is Licensed under the terms of the Microsoft Reciprocal License (Ms-RL)
//
// WBFSDevice.cs:
//
// Implementa o dispositivo WBFS
//
// NOTA:
//
//  Primeiro setor WBFS do disco:
//
//  -----------
// | Head      |  (hdSectorSize)
//  -----------
// |	       |
// | DiscInfo  |
// |	       |
//  -----------
// |	       |
// | DiscInfo  |
// |	       |
//  -----------
// |	       |
// | ...       |
// |	       |
//  -----------
// |	       |
// | DiscInfo  |
// |	       |
//  -----------
// |	       |
// | LBA       |
// |	       |
//  -----------
// 
//-------------------------------------

using System;
using System.Collections.Generic;
//using System.Linq;
using System.Text;
using System.Collections;
using System.IO;
using System.Net;

namespace WBFSSync
{
    public enum WBFSRet : int
    {
        RET_WBFS_OK = 0,
        RET_WBFS_FAIL = -1,
        RET_WBFS_ABORTED = -2,

        RET_WBFS_BADFSMAGIC = -3, //Código de verificação do sistema de arquivos inválido
        RET_WBFS_BADFSSECSZ = -4,
        RET_WBFS_BADFSTSECS = -5,

        RET_WBFS_ACCESSDENIED = -6,

        RET_WBFS_NOFREEBLOCKS = -7,
        RET_WBFS_NOTAWIIDISC = -8,
        RET_WBFS_DISCNOTFOUND = -9,
        RET_WBFS_DISCREPEATED = -10,

        RET_WBFS_CRITICALERROR = -11,
        RET_WBFS_NATIVEERROR = -12,
        
        RET_WBFS_INVALIDARG = -13,

        RET_WBFS_TRIMNAME = -14,
        RET_WBFS_TRIMCODE = -15,
        RET_WBFS_FILLCODE = -16,
    }

    public delegate void DeviceDiscEventDelegate(WBFSDevice sender, IDisc disc);
    public delegate void DeviceEventDelegate(WBFSDevice sender);
    public delegate void DeviceErrorDelegate(WBFSDevice sender, WBFSRet error, LogMessageType type);

    public class WBFSDevice : IDisposable
    {
        //----------------- Constantes

        public const int wbfsMagic = (('W' << 24) | ('B' << 16) | ('F' << 8) | ('S'));
        public const uint wiidiscMagic = 0x5D1C9EA3;

        //public const uint wbfsMagic = 0x53464257; //HTON(wbfsMagic)
        //public const uint wiidiscMagic = 0xA39E1C5D; //HTON(0x5D1C9EA3)

        public const ushort discHeaderCopySize = 256;

        public const byte wiiSectorSize_s = 15;
        public const uint wiiSectorSize = 1 << 15;//32768
        public const uint wiiSectorsPerDisc = 143432 * 2; //Suporte a Double Layer

        public const int wbfsHeadMagicPos = 0; // Posição do "Magic" dentro do cabeçalho do sistema de arquivos
        public const int wbfsHeadHdSecsPos = 4; // Posição do "Número de setores" dentro do cabeçalho do sistema de arquivos
        public const int wbfsHeadHdSecSz = 8; // Posição do "Tamanho do setor" dentro do cabeçalho do sistema de arquivos
        public const int wbfsHeadWbfsSecSz = 9; // Posição do "Tamanho do setor WBFS" dentro do cabeçalho do sistema de arquivos
        public const int wbfsHeadDiscTable = 12; // Posição da "Tabela de disco" dentro do cabeçalho do sistema de arquivos

        public const int wiidiscMagicPos = 24;

        public const int wbfsHeaderSize = 12;

        //------ Isos

        public const int IsoNameLen = 32;
        public const int IsoCodeLen = 6;

        public const int IsoNamePos = 32;
        public const int IsoCodePos = 0;

        public const int IsoRegionPos = 319488;

        public const long IsoDVD5Size = 4699979776;
        public const long IsoDVD9Size = 9399959552;

        //----------------- Eventos

        public static event DeviceEventDelegate DeviceOpened;
        public static event DeviceEventDelegate DeviceClosing;
        public static event DeviceEventDelegate DeviceClosed;
        public static event DeviceEventDelegate DeviceUpdated;

        public static event DeviceDiscEventDelegate DiscAdded;
        public static event DeviceDiscEventDelegate DiscRemoved;
        public static event DeviceDiscEventDelegate DiscDataChanged;

        //----------------- Variáveis

        //--------- Outras
        internal String name = "!";
        internal Boolean isVirtual = false; //Arquivos abertos como discos físicos

        internal int result = 0; //Último erro ocorrido
        internal Boolean didCriticalError = false; //Uma falha crítica ocorreu durante a execução e o disco foi fechado
                                          //para evitar maiores problemas, evita sincronização de dados com o
                                          //disco físico

        internal Boolean isOpen = false;
        internal Boolean requiresAdmin = false;

        long free = 0;
        long used = 0;
        long size = 0;

        //--------- Sistema de arquivos
        internal IIOContext device = null;

        internal Byte[] Head = null; //Cabeçalho do sistema de arquivos WBFS
        internal EasyArraySegment<Byte> DiscTable = new EasyArraySegment<Byte>(); //Tabela de discos contida em 'Head'

        internal BitArray WLBATable = null; //Cabeçalho de setores WBFS do disco, 'false' para usado e 'true' para livre
        internal uint WLBAPositionLBA = 0; //Posição LBATable dentro do disco em setores
        internal long WLBATableSize = 0; //Tamanho da tabela de setores WBFS em bytes

        internal long PartitionOffset = 0; //Posição inicial do sistema de arquivos em bytes
        internal uint PartitionOffsetLBA = 0; //Posição inicial do sistema de arquivos em setores, na maioria dos casos é 0

        internal int discInfoSize = 0; //Tamanho do cabeçalho dos discos de wii
        internal uint discInfoSizeInSecs = 0; //Tamanho do cabeçalho dos discos de wii em setores do hd
        internal ushort maxDiscs = 0; //Número máximo de discos que podem ser armazenados no drive

        internal uint hdSectorSize = 0; //Tamanho do setor do drive
        internal byte hdSectorSize_s = 0; //Tamanho do setor em rotações de bit
        internal uint hdTotalSectors = 0; //Total de setores do drive
        internal byte hdTotalSectors_s = 0; //Total de setores em rotações de bit

        internal long wiiTotalSectors = 0; //Total de setores de disco de Wii existem no drive
        internal uint wiiSectorsPerWBFSSector = 0; //Total de setores de disco de Wii existem dentro de um setor WBFS

        internal uint wbfsSectorSize = 0; //Tamanho do setor WBFS
        internal byte wbfsSectorSize_s = 0; //Tamanho do setor WBFS em rotações de bit
        internal ushort wbfsTotalSectors = 0; //Total de setores WBFS no dispositivo
        internal ushort wbfsSectorsPerDisc = 0; //Número de setores WBFS por disco de Wii
        internal uint wbfsFreeSectors = 0; //Total de setores livres

        internal List<Disc> Discs = new List<Disc>(); //Lista de discos de Wii

        //--------- Propriedades

        public String Name { get { return name; } }
        public String Label { get { return String.Empty; } }

        public Boolean Virtual { get { return isVirtual; } }

        public long Free { get { return free; } }
        public long Used { get { return used; } }
        public long Size { get { return size; } }

        public Boolean Ready { get { return isOpen; } }
        public Boolean RequiresAdmin { get { return requiresAdmin; } }

        public Int32 DiscCount { get { return Discs.Count; } }

        //----------------- Rotinas

        //----------------- Retorna o disco a partir do indice global
        public IDisc GetDiscByIndex(int i)
        {
            if ((i < 0) || (i >= Discs.Count)) return null;
            return Discs[i];
        }

        //----------------- Retorna o disco a partir do código
        public IDisc GetDiscByCode(string code)
        {
            for (int i = 0; i < Discs.Count; i++)
            {
                if (String.Compare(Discs[i].Code, code, true) == 0) return Discs[i];
            }

            return null;
        }

        //----------------- Retorna o disco a partir do nome
        public IDisc GetDiscByName(string name)
        {
            for (int i = 0; i < Discs.Count; i++)
            {
                if (String.Compare(Discs[i].Name, name, true) == 0) return Discs[i];
            }

            return null;
        }

        //----------------- Abre o dispositivo, cria buffers e o handle
        public int Open(String drive, bool force)
        {
            if (drive.Length == 0) return (int)WBFSRet.RET_WBFS_INVALIDARG;
            if (drive.EndsWith(":\\")) drive = drive.Remove(drive.Length - 2);

            String openstring = "";
            String contextname = "";

            if (drive.Length > 1) //É um arquivo
            {
                openstring = drive;
                isVirtual = true;
                name = IOManager.CountContexts("VDRIVE\\").ToString() + ":\\";
                contextname = "VDRIVE\\" + name;
            }
            else
            {
                openstring = @"\\.\" + drive[0] + ':'; //Acesso direto a escrita e leitura no disco
                name = drive.ToUpper() + ":\\";
                contextname = "LDRIVE\\" + name;
            }

            //Cria o Handle que permite ler e escrever no disco
            device = IOManager.CreateIOContext(contextname, openstring, FileAccess.ReadWrite, 
                FileShare.None, 0, FileMode.Open, EFileAttributes.NoBuffering);

            if (device.Result != 0) //Falha ao abrir o disco
            {
                //Verifica se a falha é "acesso negado"
                if (device.Result == (int)WinError.ERROR_ACCESS_DENIED)
                {
                    //Loga a informação
                    Log.SendMessage(this.name, (int)WBFSRet.RET_WBFS_ACCESSDENIED, null, LogMessageType.Error);

                    //Retorna o erro
                    requiresAdmin = true;
                    didCriticalError = true;
                    Close();
                    return (int)WBFSRet.RET_WBFS_ACCESSDENIED;
                }
                else
                {
                    //Loga a informação
                    Log.SendMessage(this.name, (int)WBFSRet.RET_WBFS_NATIVEERROR, device.Result, LogMessageType.Error);

                    //Retorna o erro
                    result = device.Result;
                    didCriticalError = true;
                    return (int)WBFSRet.RET_WBFS_NATIVEERROR;
                }
            }

            if (device.Lock() != IORet.RET_IO_OK)
            {
                //Loga a informação
                Log.SendMessage(this.name, (int)WBFSRet.RET_WBFS_FAIL, null, LogMessageType.Error);

                result = device.Result;
                didCriticalError = true;
                Close();
                return (int)WBFSRet.RET_WBFS_FAIL;
            }

            if (!isVirtual)
            {
                //Lê a geometria do disco e informação da partição
                DISK_GEOMETRY dg = device.GetDiskGeometry();
                PARTITION_INFORMATION pi = device.GetPartitionInformation();

                //Seta o tamanho do setor, total de setores e seus respectivos logs
                hdSectorSize = (dg.BytesPerSector == 0 ? 512 : dg.BytesPerSector);
                hdTotalSectors = (uint)(pi.PartitionLength / hdSectorSize);
            }
            else //É um drive virtual, o tamanho e quantidade dos setores depende do arquivo e não da partição
            {
                //Redimensionamento de disco virtual...
                //device.Seek((long)1 << 35);
                //device.SetEOF();

                //O tamanho do arquivo
                FileInfo file = new FileInfo(drive);

                hdSectorSize = 512;
                hdTotalSectors = (uint)(file.Length / hdSectorSize);

                file = null;
            }

            hdSectorSize_s = bitshift((uint)hdSectorSize);
            hdTotalSectors_s = bitshift((uint)hdTotalSectors);

            //Calcula o total de setores de wii dentro da partição
            wiiTotalSectors = (uint)((long)hdTotalSectors * hdSectorSize / wiiSectorSize);

            //Posição inicial do sistema de arquivos no disco/partição
            PartitionOffsetLBA = 0;
            PartitionOffset = 0;

            //Aloca o cabeçalho do sistema de arquivos
            Head = new Byte[hdSectorSize];

            //Lê o cabeçalho do disco
            if (ReadStream(PartitionOffsetLBA * (long)hdSectorSize, hdSectorSize, Head) != (int)IORet.RET_IO_OK)
            {
                //Loga erro aqui
                Log.SendMessage(this.name, (int)WBFSRet.RET_WBFS_FAIL, null, LogMessageType.Error);

                result = device.Result;
                didCriticalError = true;
                Close();
                return (int)WBFSRet.RET_WBFS_FAIL;
            }

            //--------- Lê e compara o código de verificação do sistema de arquivos (Magic) e informaçoes
            // da partição
            uint discWbfsMagic = BitConverter.ToUInt32(Head, wbfsHeadMagicPos);
            uint discTotalSectors = ntohi(BitConverter.ToUInt32(Head, wbfsHeadHdSecsPos));
            byte discSectorSize_s = Head[wbfsHeadHdSecSz];

            if (discWbfsMagic != htoni(wbfsMagic))
            {
                //Loga erro aqui
                if (!force)
                {
                    Log.SendMessage(this.name, (int)WBFSRet.RET_WBFS_BADFSMAGIC, discWbfsMagic, LogMessageType.Error);
                    return (int)WBFSRet.RET_WBFS_BADFSMAGIC;
                }
                else
                {
                    Log.SendMessage(this.name, (int)WBFSRet.RET_WBFS_BADFSMAGIC, discWbfsMagic, LogMessageType.Warning);
                }
            }

            if (discTotalSectors != hdTotalSectors)
            {
                //Loga erro aqui
                if (!force)
                {
                    Log.SendMessage(this.name, (int)WBFSRet.RET_WBFS_BADFSTSECS, discTotalSectors, LogMessageType.Error);
                    return (int)WBFSRet.RET_WBFS_BADFSTSECS;
                }
                else
                {
                    Log.SendMessage(this.name, (int)WBFSRet.RET_WBFS_BADFSTSECS, discTotalSectors, LogMessageType.Warning);
                }
            }

            if (discSectorSize_s != hdSectorSize_s)
            {
                //Loga erro aqui
                if (!force)
                {
                    Log.SendMessage(this.name, (int)WBFSRet.RET_WBFS_BADFSSECSZ, discSectorSize_s, LogMessageType.Error);
                    return (int)WBFSRet.RET_WBFS_BADFSSECSZ;
                }
                else
                {
                    Log.SendMessage(this.name, (int)WBFSRet.RET_WBFS_BADFSSECSZ, discSectorSize_s, LogMessageType.Warning);
                }
            }

            //--------- libera acesso ao handle
            device.Unlock();

            //--------- Agora sim o disco está pronto, termina de inicializar algumas variáveis

            //--------- Calculo de setores wii e wbfs

            wbfsSectorSize_s = Head[wbfsHeadWbfsSecSz];
            wbfsSectorSize = (uint)(1 << wbfsSectorSize_s);
            wbfsTotalSectors = (ushort)((int)wiiTotalSectors >> (int)(wbfsSectorSize_s - wiiSectorSize_s));
            wbfsSectorsPerDisc = (ushort)((int)wiiSectorsPerDisc >> (int)(wbfsSectorSize_s - wiiSectorSize_s));

            wiiSectorsPerWBFSSector = (uint)(1 << (int)(wbfsSectorSize_s - wiiSectorSize_s));

            discInfoSize = Align_LBA((int)(discHeaderCopySize + wbfsSectorsPerDisc * 2));
            discInfoSizeInSecs = (uint)(discInfoSize >> hdSectorSize_s);

            WLBAPositionLBA = (uint)(((int)wbfsSectorSize - (int)wbfsTotalSectors / 8) >> (int)hdSectorSize_s);
            WLBATableSize = (uint)Align_LBA(wbfsTotalSectors / 8);

            WLBATable = null; //Será alocado e lido se necessário

            maxDiscs = Math.Min((ushort)((WLBAPositionLBA - 1) / (discInfoSize >> hdSectorSize_s)),
                (ushort)(hdSectorSize - wbfsHeaderSize));

            //Cria uma sub-matriz para a tabela de discos
            DiscTable = new EasyArraySegment<byte>(Head, wbfsHeadDiscTable, maxDiscs);

            //--------- atualiza informações do disco
            GetDeviceInfo();
            EnumerateDiscs();

            isOpen = true;
            OnDeviceOpened();

            return 0;
        }

        //----------------- Formata um dispositivo WBFS já aberto
        public int Format()
        {
            if (device.Lock() != IORet.RET_IO_OK)
            {
                //Loga a informação
                Log.SendMessage(this.name, (int)WBFSRet.RET_WBFS_FAIL, null, LogMessageType.Error);
                device.Close();
                return (int)WBFSRet.RET_WBFS_FAIL;
            }

            if (!isVirtual)
            {

                //Lê a geometria do disco e informação da partição
                DISK_GEOMETRY dg = device.GetDiskGeometry();
                PARTITION_INFORMATION pi = device.GetPartitionInformation();

                //Seta o tamanho do setor, total de setores e seus respectivos logs
                hdSectorSize = (dg.BytesPerSector == 0 ? 512 : dg.BytesPerSector);
                hdTotalSectors = (uint)(pi.PartitionLength / hdSectorSize);
            }
            else //É um drive virtual, o tamanho e quantidade dos setores depende do arquivo e não da partição
            {
                //O tamanho do arquivo
                FileInfo file = new FileInfo(device.Path);

                hdSectorSize = 512;
                hdTotalSectors = (uint)(file.Length / hdSectorSize);

                file = null;
            }

            //Zera o cabeçalho (primeiro setor da partição)
            //Cria um cabeçalho vazio
            Head = new Byte[hdSectorSize];
            Array.Clear(Head, 0, (int)hdSectorSize);

            //Reescreve o identificador
            byte[] b = BitConverter.GetBytes(htoni(wbfsMagic));
            Array.Copy(b, 0, Head, wbfsHeadMagicPos, b.Length);

            //Reescreve o tamanho de setor do HD
            b = null;
            b = BitConverter.GetBytes(bitshift(hdSectorSize));
            Array.Copy(b, 0, Head, wbfsHeadHdSecSz, b.Length);

            //Reescreve o numero de setores da partição
            b = null;
            b = BitConverter.GetBytes(htoni(hdTotalSectors));
            Array.Copy(b, 0, Head, wbfsHeadHdSecsPos, b.Length);

            //Calcula tamanho mínimo do bloco wbfs
            Byte size_s;
            for (size_s = 6; size_s < 11; size_s++)
            {
                if (wiiTotalSectors < ((1U << 16) * (1 << size_s)))
                {
                    break;
                }
            }

            b = null;

            //atualiza tamanho do bloco
            Head[wbfsHeadWbfsSecSz] = (byte)(size_s + wiiSectorSize_s);

            //Zera a tabela de setores usados
            WLBATable.SetAll(true);

            //Escreve o cabeçalho no disco
            SynchronizeFileSystem();

            //--------- libera acesso ao handle
            device.Unlock();

            //--------- Atualiza informações do disco
            wiiTotalSectors = (uint)((long)hdTotalSectors * hdSectorSize / wiiSectorSize);

            wbfsSectorSize_s = Head[wbfsHeadWbfsSecSz];
            wbfsSectorSize = (uint)(1 << wbfsSectorSize_s);
            wbfsTotalSectors = (ushort)((int)wiiTotalSectors >> (int)(wbfsSectorSize_s - wiiSectorSize_s));
            wbfsSectorsPerDisc = (ushort)((int)wiiSectorsPerDisc >> (int)(wbfsSectorSize_s - wiiSectorSize_s));

            wiiSectorsPerWBFSSector = (uint)(1 << (int)(wbfsSectorSize_s - wiiSectorSize_s));

            discInfoSize = Align_LBA((int)(discHeaderCopySize + wbfsSectorsPerDisc * 2));
            discInfoSizeInSecs = (uint)(discInfoSize >> hdSectorSize_s);

            WLBAPositionLBA = (uint)(((int)wbfsSectorSize - (int)wbfsTotalSectors / 8) >> (int)hdSectorSize_s);
            WLBATableSize = (uint)Align_LBA(wbfsTotalSectors / 8);

            WLBATable = null; //Será alocado e lido se necessário

            maxDiscs = Math.Min((ushort)((WLBAPositionLBA - 1) / (discInfoSize >> hdSectorSize_s)),
                (ushort)(hdSectorSize - wbfsHeaderSize));

            //Cria uma sub-matriz para a tabela de discos
            DiscTable = new EasyArraySegment<byte>(Head, wbfsHeadDiscTable, maxDiscs);

            //Marca todos os setores como vazios

            //--------- atualiza informações do disco
            GetDeviceInfo();
            EnumerateDiscs();

            isOpen = true;
            OnDeviceOpened();

            return (int)WBFSRet.RET_WBFS_OK;
        }

        //----------------- Formata um dispositivo para WBFS
        public static int Format(String drive)
        {
            string openstring = "";
            bool isVirtual = false;

            Byte[] Head = null; //Cabeçalho do sistema de arquivos WBFS
            uint PartitionOffsetLBA = 0; //Posição inicial do sistema de arquivos em setores, na maioria dos casos é 0

            uint hdSectorSize = 0; //Tamanho do setor do drive
            byte hdSectorSize_s = 0;
            uint hdTotalSectors = 0; //Total de setores do drive

            uint wiiTotalSectors = 0; //Total de setores de disco de Wii existem no drive

            byte wbfsSectorSize_s = 0;
            uint wbfsSectorSize = 0;
            ushort wbfsTotalSectors = 0;
            uint WLBAPositionLBA = 0;

            if (drive.EndsWith(":\\")) //Drive
            {
                openstring = @"\\.\" + drive[0] + ':';
            }
            else //Arquivo
            {
                openstring = drive;
                isVirtual = true;
            }

            IIOContext device = IOManager.CreateIOContext("FORMAT", openstring, FileAccess.ReadWrite, FileShare.Read, 0,
                FileMode.Open, EFileAttributes.NoBuffering);

            if (device.Result != 0) //Falha ao abrir o disco
            {
                //Verifica se a falha é "acesso negado"
                if (device.Result == (int)WinError.ERROR_ACCESS_DENIED)
                {
                    //Loga a informação
                    Log.SendMessage(drive, (int)WBFSRet.RET_WBFS_ACCESSDENIED, null, LogMessageType.Error);
                    return (int)WBFSRet.RET_WBFS_ACCESSDENIED;
                }
                else
                {
                    //Loga a informação
                    Log.SendMessage(drive, (int)WBFSRet.RET_WBFS_NATIVEERROR, device.Result, LogMessageType.Error);
                    return (int)WBFSRet.RET_WBFS_NATIVEERROR;
                }
            }

            if (device.Lock() != IORet.RET_IO_OK)
            {
                //Loga a informação
                Log.SendMessage(drive, (int)WBFSRet.RET_WBFS_FAIL, null, LogMessageType.Error);
                device.Close();
                return (int)WBFSRet.RET_WBFS_FAIL;
            }

            if (!isVirtual)
            {

                //Lê a geometria do disco e informação da partição
                DISK_GEOMETRY dg = device.GetDiskGeometry();
                PARTITION_INFORMATION pi = device.GetPartitionInformation();

                //Seta o tamanho do setor, total de setores e seus respectivos logs
                hdSectorSize = (dg.BytesPerSector == 0 ? 512 : dg.BytesPerSector);
                hdTotalSectors = (uint)(pi.PartitionLength / hdSectorSize);
            }
            else //É um drive virtual, o tamanho e quantidade dos setores depende do arquivo e não da partição
            {
                //O tamanho do arquivo
                FileInfo file = new FileInfo(drive);

                hdSectorSize = 512;
                hdTotalSectors = (uint)(file.Length / hdSectorSize);

                file = null;
            }

            //Calcula tamanho mínimo do bloco wbfs
            Byte size_s;
            for (size_s = 6; size_s < 11; size_s++)
            {
                if (wiiTotalSectors < ((1U << 16) * (1 << size_s)))
                {
                    break;
                }
            }

            hdSectorSize_s = bitshift(hdSectorSize);

            wiiTotalSectors = (uint)((long)hdTotalSectors * hdSectorSize / wiiSectorSize);

            wbfsSectorSize_s = (byte)(size_s + wiiSectorSize_s);
            wbfsSectorSize = (uint)(1 << wbfsSectorSize_s);
            wbfsTotalSectors = (ushort)((int)wiiTotalSectors >> (int)(wbfsSectorSize_s - wiiSectorSize_s));

            //Calcula a posição da tabela de uso de setores
            WLBAPositionLBA = (uint)(((int)wbfsSectorSize - (int)wbfsTotalSectors / 8) >> (int)hdSectorSize_s);

            //Zera o cabeçalho (primeiro setor wbfs da partição)
            //Cria um cabeçalho vazio
            Head = new Byte[hdSectorSize];
            Array.Clear(Head, 0, Head.Length);

            //Reescreve o identificador
            byte[] b = BitConverter.GetBytes(htoni(wbfsMagic));
            Array.Copy(b, 0, Head, wbfsHeadMagicPos, b.Length);

            //Reescreve o tamanho de setor do HD
            b = null;
            b = BitConverter.GetBytes(bitshift(hdSectorSize));
            Array.Copy(b, 0, Head, wbfsHeadHdSecSz, b.Length);

            //Reescreve o numero de setores da partição
            b = null;
            b = BitConverter.GetBytes(htoni(hdTotalSectors));
            Array.Copy(b, 0, Head, wbfsHeadHdSecsPos, b.Length);

            b = null;

            //atualiza tamanho do bloco
            Head[wbfsHeadWbfsSecSz] = wbfsSectorSize_s;

            //Escreve o cabeçalho no disco
            if (device.Write(Head, PartitionOffsetLBA * hdSectorSize, (int)hdSectorSize) != IORet.RET_IO_OK)
            {
                device.Close();
                return (int)WBFSRet.RET_WBFS_FAIL;
            }

            //Escreve a tabela de uso de setores
            Byte[] WLBATable = new Byte[Align_LBA(wbfsTotalSectors / 8, hdSectorSize)];
            for (int i = 0; i < WLBATable.Length; i++) WLBATable[i] = 0xFF;

            if (device.Write(WLBATable, (PartitionOffsetLBA + WLBAPositionLBA) * (long)hdSectorSize, WLBATable.Length) 
                != IORet.RET_IO_OK)
            {
                WLBATable = null;
                device.Close();
                return (int)WBFSRet.RET_WBFS_FAIL;
            }

            WLBATable = null;

            device.Close();
            return (int)WBFSRet.RET_WBFS_OK;
        }

        //----------------- Atualiza o dispositivo
        public void Update()
        {
            GetDeviceInfo();
            EnumerateDiscs();

            OnDeviceUpdated();
        }

        //----------------- Fecha o dispositivo e libera buffers, o handle e sincroniza o sistema de arquivos
        public void Close()
        {
            OnDeviceClosing();

            if (!didCriticalError) //Impede que uma transferência mal feita comprometa o sistema de arquivos
            {
                if (device.Lock() == IORet.RET_IO_OK)
                {
                    SynchronizeFileSystem();
                    device.Unlock();
                }
                else
                {
                    //Loga erro
                    Log.SendMessage(this.name, (int)WBFSRet.RET_WBFS_FAIL, null, LogMessageType.Error);
                }
            }

            for (int i = 0; i < Discs.Count; i++)
            {
                Discs[i].Dispose();
            }

            Discs.Clear();

            if (device != null)
            {
                device.ForceClose();
                device = null;
            }

            Head = null;
            WLBATable = null;

            isOpen = false;
            OnDeviceClosed();
        }

        //----------------- Atualiza as informações do disco e se necessário carrega a LBATable
        public int GetDeviceInfo()
        {
            if (device.Lock() != IORet.RET_IO_OK) return (int)WBFSRet.RET_WBFS_FAIL;

            long r = CountFreeBlocks();
            if (r < 0) return (int)r;

            wbfsFreeSectors = (uint)r;

            free = (long)wbfsSectorSize * wbfsFreeSectors;
            size = (long)wbfsSectorSize * wbfsTotalSectors;
            used = size - free;

            device.Unlock();
            return 0;
        }

        //----------------- Conta quantos discos estão no drive
        public int CountDiscs()
        {
            int count = 0;
            for (int i = 0; i < maxDiscs; i++) if (DiscTable[i] != 0) count++;

            return count;
        }

        //----------------- Enumera os discos de wii no drive
        public int EnumerateDiscs()
        {
            //Garante que os discos deletados sejam removidos
            for (int i = 0; i < Discs.Count; i++)
            {
                if ((Discs[i] == null) || Discs[i].deleted) Discs.RemoveAt(i);
            }

            int discs = CountDiscs();
            if (discs <= 0) return discs;

            //Tranca o contexto do dispositivo
            if (device.Lock() != IORet.RET_IO_OK) return (int)WBFSRet.RET_WBFS_FAIL;

            for (int i = 0; i < discs; i++)
            {
                Disc disc;
                if (GetDiscInfo(i, out disc) >= 0)
                {
                    if (GetDiscByCode(disc.code) == null)
                        Discs.Add(disc);
                }
            }

            //Libera o contexto do dispositivo
            device.Unlock();

            //Reorganiza a lista do menor para o maior indice de disco
            Discs.Sort();

            return 0;
        }

        //----------------- Lê as informações de um disco de wii no drive
        internal int GetDiscInfo(int index, out Disc disc)
        {
            disc = null;
            uint count = 0;

            for (int i = 0; i < maxDiscs; i++)
            {
                if (DiscTable[i] != 0)
                {
                    if (count++ == index)
                    {
                        byte[] buffer = new byte[discInfoSize];

                        //Lê a cópia do cabeçalho da iso + tabela lba
                        int r = ReadSector((long)(PartitionOffsetLBA + 1 /*setor 0*/ + i * discInfoSizeInSecs),
                            discInfoSizeInSecs, buffer);
                        if (r < 0) return r;

                        //verifica a identificação do disco (magic)
                        if (BitConverter.ToUInt32(buffer, wiidiscMagicPos) != htoni(wiidiscMagic))
                        {
                            //Loga erro de disco inválido
                            Log.SendMessage(this.name, (int)WBFSRet.RET_WBFS_NOTAWIIDISC, null, LogMessageType.Error);

                            //Trunca entrada
                            DiscTable[i] = 0;

                            //Retorna
                            return (int)WBFSRet.RET_WBFS_NOTAWIIDISC;

                        }

                        //Cria o disco
                        disc = new Disc();

                        //Associa o disco ao dispositivo
                        disc.device = this;

                        //Seta o indice do disco em relação à DiscTable
                        disc.index = i;

                        //Seta a posição do cabeçalho do disco
                        disc.discInfoLbaPosition = (PartitionOffsetLBA + 1 /*setor 0*/ + i * discInfoSizeInSecs);

                        //Lê o código do jogo de 6 caracteres
                        int j = IsoCodePos;
                        byte b = 0;
                        for (j = IsoCodePos; j < IsoCodePos + IsoCodeLen; j++) disc.code += (char)buffer[j];

                        //Lê o nome do jogo
                        j = IsoNamePos;
                        while (((b = buffer[j++]) != 0) && (j < IsoNamePos + IsoNameLen)) disc.name += (char)b;

                        //Cria e carrega a tabela wlba
                        disc.wlbaTable = new ushort[wbfsSectorsPerDisc];

                        for (j = 0; j < wbfsSectorsPerDisc; j++)
                            disc.wlbaTable[j] = ntohs(BitConverter.ToUInt16(buffer, discHeaderCopySize + (2 * j)));

                        //Conta os setores usados e calcula o tamanho

                        int usedblocks = 0;
                        for (j = 0; j < wbfsSectorsPerDisc; j++) { if (disc.wlbaTable[j] != 0) usedblocks++; }

                        disc.size = (usedblocks > (wiiSectorsPerDisc / 2) ? IsoDVD9Size : IsoDVD5Size);
                        disc.scrubbedsize = usedblocks * wbfsSectorSize;
                        disc.usedwbfssectors = (uint)usedblocks;

                        //Lê a região do disco

                        long p; //índice do setor do disco
                        uint o; //índice em relação ao setor 'p' do disco

                        disc.GetLBAPosAndSectorOffset(IsoRegionPos, out p, out o);
                        ReadSector(p, 1, buffer);

                        disc.region = (int)ntohi(BitConverter.ToUInt32(buffer, (int)o));

                        //Libera a memória alocada no 'buffer'
                        buffer = null;

                        return i;
                    }
                }
            }

            return -1;
        }

        //----------------- Altera o cabeçalho do disco
        public int ChangeDiscHeader(IDisc disc, String name, String code)
        {
            //Verificação

            if (disc == null)
            {
                Log.SendMessage(this.name, (int)WBFSRet.RET_WBFS_INVALIDARG, "disc", LogMessageType.Error);
                return (int)WBFSRet.RET_WBFS_INVALIDARG;
            }

            if (code.Length == 0)
            {
                //Se for vazio, o original será usado
                code = disc.Code;
            }
            else if (code.Length != IsoCodeLen)
            {
                if (code.Length < IsoCodeLen)
                {
                    while (code.Length < IsoCodeLen) code += '0';

                    //Loga Aviso
                    Log.SendMessage(this.name, (int)WBFSRet.RET_WBFS_FILLCODE, '0', LogMessageType.Warning);
                }
                else
                {
                    code = code.Substring(0, IsoCodeLen); //Eu preciso fazer isso para o GetDiscByCode()
                 
                    //Loga Aviso
                    Log.SendMessage(this.name, (int)WBFSRet.RET_WBFS_TRIMCODE, code, LogMessageType.Warning);
                }
            }

            //Manda o código para uppercase
            code = code.ToUpper();

            if (name.Length == 0)
            {
                //Se for vazio, o original será usado
                name = disc.Name;
            }
            else if (name.Length > IsoNameLen)
            {
                name = name.Substring(0, IsoNameLen);

                //Loga Aviso
                Log.SendMessage(this.name, (int)WBFSRet.RET_WBFS_TRIMNAME, name, LogMessageType.Warning);
            }

            IDisc otherdisc = GetDiscByCode(code);
            if ((otherdisc != null) && (otherdisc != disc)) //Se existir OUTRO disco com o mesmo código
            {
                //Loga Aviso
                Log.SendMessage(this.name, (int)WBFSRet.RET_WBFS_DISCREPEATED, code, LogMessageType.Error);

                return (int)WBFSRet.RET_WBFS_DISCREPEATED;
            }

            otherdisc = GetDiscByName(name);
            if ((otherdisc != null) && (otherdisc != disc)) //Se existir OUTRO disco com o mesmo nome
            {
                //Loga Aviso
                Log.SendMessage(this.name, (int)WBFSRet.RET_WBFS_DISCREPEATED, name, LogMessageType.Error);

                return (int)WBFSRet.RET_WBFS_DISCREPEATED;
            }

            //Tranca o contexto do dispositivo
            if (device.Lock() != IORet.RET_IO_OK)
            {
                Log.SendMessage(this.name, (int)WBFSRet.RET_WBFS_FAIL, null, LogMessageType.Error);
                return (int)WBFSRet.RET_WBFS_FAIL;
            }

            //Lê o setor de informação do disco
            byte[] buffer = new byte[hdSectorSize];
            ReadSector(disc.DiscInfoLBAPosition, 1, buffer);

            //Altera o setor com os novos nome e código

            for (int i = 0; i < IsoCodeLen; i++)
            {
                buffer[IsoCodePos + i] = (byte)code[i];
            }

            for (int i = 0; i < Math.Min(name.Length, IsoNameLen); i++)
            {
                buffer[IsoNamePos + i] = (byte)name[i];
            }

            for (int i = name.Length; i < IsoNameLen; i++) //Preenche o restante do espaço do nome com zeros
            {
                buffer[IsoNamePos + i] = 0;
            }

            //Escreve as alterações
            WriteSector(disc.DiscInfoLBAPosition, 1, buffer);

            //Agora altera o nome e código do disco em si, eles estão no WLBA[0]

            long p; //índice do setor do disco
            uint o; //índice em relação ao setor 'p' do disco

            disc.GetLBAPosAndSectorOffset(0, out p, out o);

            //O buffer já contem uma cópia modificada, porém a cópia que fica no setor de informação só
            //contém 256 bytes, o resto é WLBA, eu preciso de um setor inteiro preenchido o conteúdo real do disco
            ReadSector(p, 1, buffer);

            //Altera o setor com os novos nome e código
            for (int i = 0; i < IsoCodeLen; i++)
            {
                buffer[IsoCodePos + i] = (byte)code[i];
            }

            for (int i = 0; i < Math.Min(name.Length, IsoNameLen); i++)
            {
                buffer[IsoNamePos + i] = (byte)name[i];
            }

            for (int i = name.Length; i < IsoNameLen; i++) //Preenche o restante do espaço do nome com zeros
            {
                buffer[IsoNamePos + i] = 0;
            }

            //Escreve as alterações
            WriteSector(p, 1, buffer);

            //Libera o contexto do dispositivo
            device.Unlock();

            (disc as Disc).name = name;
            (disc as Disc).code = code;

            return 0;
        }

        //----------------- Apaga um disco do sistema de arquivos pelo indice do jogo na Lista 'Discs'
        public int DeleteDisc(int index)
        {
            return DeleteDisc(GetDiscByIndex(index));
        }

        //----------------- Apaga um disco do sistema de arquivos pelo código
        public int DeleteDisc(String code)
        {
            return DeleteDisc(GetDiscByCode(code));
        }

        //----------------- Apaga um disco do sistema de arquivos
        public int DeleteDisc(IDisc disc)
        {
            //Verificação
            if (disc == null)
            {
                Log.SendMessage(this.name, (int)WBFSRet.RET_WBFS_INVALIDARG, "disc", LogMessageType.Error);
                return (int)WBFSRet.RET_WBFS_INVALIDARG;
            }

            if (disc.Deleted)
            {
                Log.SendMessage(this.name, (int)WBFSRet.RET_WBFS_DISCNOTFOUND, null, LogMessageType.Error);
                return (int)WBFSRet.RET_WBFS_DISCNOTFOUND;
            }

            //Tranca o contexto do dispositivo
            if (device.Lock() != IORet.RET_IO_OK)
            {
                Log.SendMessage(this.name, (int)WBFSRet.RET_WBFS_FAIL, null, LogMessageType.Error);
                return (int)WBFSRet.RET_WBFS_FAIL;
            }


            Byte[] DiscHeader = new Byte[discInfoSize];
            int r = WriteSector(PartitionOffsetLBA + 1 + disc.Index * discInfoSizeInSecs, discInfoSizeInSecs, DiscHeader);
            if (r != (int)IORet.RET_IO_OK)
            {
                Log.SendMessage(this.name, (int)WBFSRet.RET_WBFS_FAIL, null, LogMessageType.Error);
                return r;
            }

            //Apaga o disco da tabela
            DiscTable[disc.Index] = 0;

            //Marca todos os setores utilizados pelo jogo como espaço livre
            for (int i = 0; i < wbfsSectorsPerDisc; i++)
            { 
                if (disc.WLBATable[i] != 0) 
                    FreeBlock(disc.WLBATable[i]); 
            }

            //Sincroniza a tabela de discos e setores
            SynchronizeFileSystem();

            //Libera o contexto do dispositivo
            device.Unlock();

            DiscHeader = null;

            OnDiscRemoved(disc);

            //Remove o disco da lista
            (disc as Disc).deleted = true;
            Discs.Remove((disc as Disc));

            return 0;
        }

        //----------------- Adiciona um disco ao dispositivo a partir de um arquivo no PC
        public int AddDisc(String filename, ProgressIndicator progress, Boolean Copy_1_1)
        {
            if (filename.Length == 0)
            {
                Log.SendMessage(this.name, (int)WBFSRet.RET_WBFS_INVALIDARG, "filename", LogMessageType.Error);
                return (int)WBFSRet.RET_WBFS_INVALIDARG;
            }

            if (progress == null)
            {
                Log.SendMessage(this.name, (int)WBFSRet.RET_WBFS_INVALIDARG, "progress", LogMessageType.Error);
                return (int)WBFSRet.RET_WBFS_INVALIDARG;
            }

            //Seleciona um slot vazio na tabela de discos
            int i = 0;
            for (i = 0; i < maxDiscs; i++)
            {
                if (DiscTable[i] == 0)
                {
                    break;
                }
            }

            if (i == maxDiscs)
            {
                Log.SendMessage(this.name, (int)WBFSRet.RET_WBFS_NOFREEBLOCKS, null, LogMessageType.Error);
                return (int)WBFSRet.RET_WBFS_NOFREEBLOCKS;
            }

            int discn = i;

            int r = 0; //Para guardar o código de erro

            //Tranca o contexto do dispositivo
            if (device.Lock() != IORet.RET_IO_OK)
            {
                Log.SendMessage(this.name, (int)WBFSRet.RET_WBFS_FAIL, null, LogMessageType.Error);
                return (int)WBFSRet.RET_WBFS_FAIL;
            }

            device.PrepareForAsync();

            //Cria o contexto do arquivo
            IIOContext source = IOManager.CreateIOContext("ADD", filename, FileAccess.Read, FileShare.Read, 0, 
                FileMode.Open, EFileAttributes.None);

            if (source.Result != 0) //Falha ao criar o handle
            {
                result = source.Result;

                //Verifica se a falha é "acesso negado"
                if (source.Result == (int)WinError.ERROR_ACCESS_DENIED)
                {
                    r = (int)WBFSRet.RET_WBFS_ACCESSDENIED;
                }
                else
                {
                    r = (int)WBFSRet.RET_WBFS_NATIVEERROR;
                }

                Log.SendMessage(this.name, r, result, LogMessageType.Error);
                goto FREE;
            }

            //Abre e monta o disco para ter a tabela de setores
            if ((r = (int)source.Lock()) != (int)IORet.RET_IO_OK) { goto FREE; }
            source.PrepareForAsync();

            //Matriz que descreve o disco dentro do sistema de arquivos
            Byte[] info = new Byte[discInfoSize];

            //Tabela de setores
            Byte[] wiiUsageTable = new Byte[wiiSectorsPerDisc];

            //Setores Usados
            uint wiiused = wiiSectorsPerDisc;

            String name = "";
            String code = "";

            if (!Copy_1_1)
            {

                WiiDisc disc = new WiiDisc(source, false);
                disc.Open();
                disc.BuildDisc(PartitionSelection.OnlyGamePartition);

                name = disc.name;
                code = disc.code;

                disc.SectorUsageTable.CopyTo(wiiUsageTable, 0);
                disc.HeaderCopy.CopyTo(info, 0);

                wiiused = disc.UsedSectors;

                //Fecha o disco
                disc.Close();
                disc = null;
            }
            else
            {
                source.Seek(0);
                source.Read(info, 0, 256);

                for (i = IsoCodePos; i < IsoCodePos + IsoCodeLen; i++) { code += (char)info[i]; }
                for (i = IsoNamePos; i < IsoNamePos + IsoNameLen; i++) { name += (char)info[i]; }

                source.Seek(0);
            }

            //Verifica se algum jogo no disco já tenha o mesmo codigo e/ou nome
            if (GetDiscByCode(code) != null)
            {
                //Loga Aviso

                r = (int)WBFSRet.RET_WBFS_DISCREPEATED;
                Log.SendMessage(this.name, (int)WBFSRet.RET_WBFS_DISCREPEATED, code, LogMessageType.Error);
                goto FREE;
            }

            if (GetDiscByName(name) != null)
            {
                //Loga Aviso

                r = (int)WBFSRet.RET_WBFS_DISCREPEATED;
                Log.SendMessage(this.name, (int)WBFSRet.RET_WBFS_DISCREPEATED, name, LogMessageType.Error);
                goto FREE;
            }

            //Cria a tabela de setores WBFS
            Boolean[] wbfsUsageTable = new Boolean[wbfsSectorsPerDisc];
            uint wbfsused = 0;
            for (i = 0; i < wbfsSectorsPerDisc; i++)
            {
                if (Copy_1_1 || IsBlockUsed(wiiUsageTable, (uint)i, wiiSectorsPerWBFSSector))
                {
                    wbfsused++;
                    wbfsUsageTable[i] = true;
                }
            }

            if (CountFreeBlocks() < wbfsused) //Sem espaço no disco
            {
                r = (int)WBFSRet.RET_WBFS_NOFREEBLOCKS;
                Log.SendMessage(this.name, (int)WBFSRet.RET_WBFS_NOFREEBLOCKS, null, LogMessageType.Error);
                goto FREE;
            }

            //Cria os buffers para leitura/escrita assíncrona
            Byte[] buffer1 = new Byte[wiiSectorSize];
            Byte[] buffer2 = new Byte[wiiSectorSize];

            Byte[] readBuffer = buffer1;
            Byte[] writeBuffer = buffer2;

            byte readBufferState = 0; //0 - vazio, 1 - lendo, 2 - cheio
            byte writeBufferState = 0; //0 - vazio, 1 - escrevendo, 2 - cheio

            ushort wbfssector = 0; //Indice na WLBA do disco que representa a posição de um setor WBFS alocado
            int wiisector = 0;

            //Reseta o contador de progresso, seta o contexto na posição 0, marca o disco e começa a transferência
            DiscTable[discn] = 1;
            source.Seek(0);
            progress.Reset(wiiused, wiiSectorSize);

            i = 0;
            while (!progress.Cancel)
            {
                if (!device.CanWork) { progress.Cancel = true; break; }

                if (readBufferState == 0)
                {
                    source.ReadAsync(readBuffer, ((long)wbfsSectorSize * i) + ((long)wiiSectorSize * wiisector),
                        (int)wiiSectorSize);
                    readBufferState = 1; //Lendo...
                }

                if((readBufferState == 1)&&(!source.Working)) //Fim da leitura
                {
                    //Conserta a tabela de partição se necessário
                    if (((long)wbfsSectorSize * i) + ((long)wiiSectorSize * wiisector) == 0x10000)
                    {
                        WiiDisc.FixPartitionTable(PartitionSelection.OnlyGamePartition, readBuffer);
                    }

                    readBufferState = 2; //Pronto
                }

                if ((writeBufferState == 1) && (!device.Working)) //Fim da escrita
                {
                    writeBufferState = 0; //Livre
                    progress.Progress++;
                }

                if ((readBufferState == 2) && (writeBufferState == 0))
                {
                    //Troca os buffers
                    byte[] t = readBuffer;
                    readBuffer = writeBuffer;
                    writeBuffer = t;

                    //Aloca um bloco, se ainda não tenha sido
                    if (wbfssector == 0) wbfssector = (ushort)AllocateBlock();

                    device.WriteAsync(writeBuffer, ((long)PartitionOffsetLBA * hdSectorSize) + 
                        ((long)wbfssector * wbfsSectorSize) + 
                        ((long)wiisector * wiiSectorSize), (int)wiiSectorSize);

                    writeBufferState = 1; //Escrevendo...
                    readBufferState = 0; //Permite o buffering de mais um setor

                    if (++wiisector == wiiSectorsPerWBFSSector) //O setor wbfs atual acabou
                    {
                        //Copia o indice do último setor para a tabela de informação do disco
                        Byte[] v = BitConverter.GetBytes(htons(wbfssector));
                        info[discHeaderCopySize + 2 * i] = v[0];
                        info[discHeaderCopySize + 2 * i + 1] = v[1];

                        //
                        wiisector = 0;
                        wbfssector = 0;

                        while (++i < wbfsSectorsPerDisc) if (wbfsUsageTable[i]) break; //Procura proximo setor
                        if (i == wbfsSectorsPerDisc) break; //Acabou o disco
                    }
                }

                //Impede 100% de uso de CPU
                //System.Threading.Thread.Sleep(1);
            }

            //Garante que todas as operações assíncronas terminem
            while (device.Working) System.Threading.Thread.Sleep(1);
            while (source.Working) System.Threading.Thread.Sleep(1); //Só ocorrerá se a operação for cancelada

            if (progress.Cancel) //Desaloca os blocos e o disco
            {
                for (i = 0; i < wbfsSectorsPerDisc; i++)
                {
                    wiisector = ntohs(BitConverter.ToUInt16(info, discHeaderCopySize + 2 * i));
                    FreeBlock((uint)wiisector);
                }

                DiscTable[discn] = 0;

                r = (int)WBFSRet.RET_WBFS_ABORTED;
                Log.SendMessage(this.name, (int)WBFSRet.RET_WBFS_ABORTED, null, LogMessageType.Error);
            }
            else
            {
                WriteToDiscInfo(discn, info);
                SynchronizeFileSystem(); //Escreve as alterações no disco
            }

        FREE: //Caso algo dê errado... venha direto para cá...

            device.FreeAsync();
            device.Unlock(); //Libera o contexto do dispositivo

            source.FreeAsync();
            source.Close(); //Fecha o disco

            //Limpa as matrizes
            wiiUsageTable = null;
            wbfsUsageTable = null;
            readBuffer = null;
            writeBuffer = null;
            buffer1 = null;
            buffer2 = null;

            return r;
        }

        //----------------- Adiciona um disco ao dispositivo a partir de outro dispositivo WBFS
        public int AddDisc(IDisc disc, ProgressIndicator progress, Boolean Copy_1_1)
        {
            int r = 0; //Para guardar o código de erro

            if (disc == null)
            {
                Log.SendMessage(this.name, (int)WBFSRet.RET_WBFS_INVALIDARG, "disc", LogMessageType.Error);
                return (int)WBFSRet.RET_WBFS_INVALIDARG;
            }

            if (progress == null)
            {
                Log.SendMessage(this.name, (int)WBFSRet.RET_WBFS_INVALIDARG, "progress", LogMessageType.Error);
                return (int)WBFSRet.RET_WBFS_INVALIDARG;
            }

            //Verifica se algum jogo no disco já tenha o mesmo codigo e/ou nome
            if (GetDiscByCode(disc.Code) != null)
            {
                //Loga Aviso

                r = (int)WBFSRet.RET_WBFS_DISCREPEATED;
                Log.SendMessage(this.name, (int)WBFSRet.RET_WBFS_DISCREPEATED, disc.Code, LogMessageType.Error);
            }

            if (GetDiscByName(disc.Name) != null)
            {
                //Loga Aviso

                r = (int)WBFSRet.RET_WBFS_DISCREPEATED;
                Log.SendMessage(this.name, (int)WBFSRet.RET_WBFS_DISCREPEATED, disc.Name, LogMessageType.Error);
            }

            //Seleciona um slot vazio na tabela de discos
            int i = 0;
            for (i = 0; i < maxDiscs; i++)
            {
                if (DiscTable[i] == 0)
                {
                    break;
                }
            }

            if (i == maxDiscs)
            {
                Log.SendMessage(this.name, (int)WBFSRet.RET_WBFS_NOFREEBLOCKS, null, LogMessageType.Error);
                return (int)WBFSRet.RET_WBFS_NOFREEBLOCKS;
            }

            int discn = i;

            //Tranca o contexto do dispositivo
            if (device.Lock() != IORet.RET_IO_OK)
            {
                Log.SendMessage(this.name, (int)WBFSRet.RET_WBFS_FAIL, null, LogMessageType.Error);
                return (int)WBFSRet.RET_WBFS_FAIL;
            }

            device.PrepareForAsync();

            //Cria o contexto do disco
            IIOContext source = new DiscReader(disc);

            if (source.Result != 0) //Falha ao criar o handle
            {
                result = source.Result;

                //Verifica se a falha é "acesso negado"
                if (source.Result == (int)WinError.ERROR_ACCESS_DENIED)
                {
                    r = (int)WBFSRet.RET_WBFS_ACCESSDENIED;
                }
                else
                {
                    r = (int)WBFSRet.RET_WBFS_NATIVEERROR;
                }

                Log.SendMessage(this.name, r, result, LogMessageType.Error);
                goto FREE;
            }

            //Abre e monta o disco para ter a tabela de setores
            if ((r = (int)source.Lock()) != (int)IORet.RET_IO_OK) { goto FREE; }
            source.PrepareForAsync();

            //Matriz que descreve o disco dentro do sistema de arquivos
            Byte[] info = new Byte[discInfoSize];

            //Tabela de setores
            Byte[] wiiUsageTable = new Byte[wiiSectorsPerDisc];

            //Setores Usados
            uint wiiused = wiiSectorsPerDisc;

            String name = "";
            String code = "";

            if (!Copy_1_1)
            {

                WiiDisc wiidisc = new WiiDisc(source, false);
                wiidisc.Open();
                wiidisc.BuildDisc(PartitionSelection.OnlyGamePartition);

                name = wiidisc.name;
                code = wiidisc.code;

                wiidisc.SectorUsageTable.CopyTo(wiiUsageTable, 0);
                wiidisc.HeaderCopy.CopyTo(info, 0);

                wiiused = wiidisc.UsedSectors;

                //Fecha o disco
                wiidisc.Close();
                wiidisc = null;
            }
            else
            {
                source.Seek(0);
                source.Read(info, 0, (int)((Disc)disc).device.hdSectorSize);

                source.Seek(0);
            }

            int runto = (int)(source.Length > IsoDVD5Size ? wbfsSectorSize : wbfsSectorSize / 2);

            //Cria a tabela de setores WBFS
            Boolean[] wbfsUsageTable = new Boolean[wbfsSectorsPerDisc];
            uint wbfsused = 0;
            for (i = 0; i < wbfsSectorsPerDisc; i++)
            {
                if (Copy_1_1 || IsBlockUsed(wiiUsageTable, (uint)i, wiiSectorsPerWBFSSector))
                {
                    wbfsused++;
                    wbfsUsageTable[i] = true;
                }
            }

            if (CountFreeBlocks() < wbfsused) //Sem espaço no disco
            {
                r = (int)WBFSRet.RET_WBFS_NOFREEBLOCKS;
                Log.SendMessage(this.name, (int)WBFSRet.RET_WBFS_NOFREEBLOCKS, null, LogMessageType.Error);
                goto FREE;
            }

            //Cria os buffers para leitura/escrita assíncrona
            Byte[] buffer1 = new Byte[wiiSectorSize];
            Byte[] buffer2 = new Byte[wiiSectorSize];

            Byte[] readBuffer = buffer1;
            Byte[] writeBuffer = buffer2;

            byte readBufferState = 0; //0 - vazio, 1 - lendo, 2 - cheio
            byte writeBufferState = 0; //0 - vazio, 1 - escrevendo, 2 - cheio

            ushort wbfssector_dest = 0; //Indice na WLBA do disco de destino que representa a posição de um setor WBFS alocado
            ushort wbfssector_src = disc.WLBATable[0]; //Indice na WLBA do disco de origem que representa a posição de um
                                                     //setor WBFS alocado
            int wiisector_dest = 0;
            int wiisector_src = 0;

            uint wiiSectorsPerWBFSSector_src = ((Disc)disc).device.wiiSectorsPerWBFSSector;
            uint wbfsSectorSize_src = ((Disc)disc).device.wbfsSectorSize;

            //Reseta o contador de progresso, seta o contexto na posição 0, marca o disco e começa a transferência
            DiscTable[discn] = 1;
            source.Seek(0);
            progress.Reset(wiiused, wiiSectorSize);

            // O fato do 'source' ser DiscReader não influencia no processo de leitura daqui para frente,
            // tanto que ainda é necessário fazer os cálculos de posição dos setores WBFS

            i = 0;
            while (!progress.Cancel)
            {
                if (!device.CanWork) { progress.Cancel = true; break; }

                if (readBufferState == 0)
                {
                    if (wbfssector_src == 0)
                    {
                        readBufferState = 2; //Pronto
                    }
                    else
                    {
                        source.ReadAsync(readBuffer, ((long)wbfsSectorSize_src * wbfssector_src) + 
                            ((long)wiiSectorSize * wiisector_src), (int)wiiSectorSize);
                        readBufferState = 1; //Lendo...
                    }
                }

                if ((readBufferState == 1) && (!source.Working)) //Fim da leitura
                {
                    //Conserta a tabela de partição se necessário
                    if (((long)wbfsSectorSize_src * wbfssector_src) + ((long)wiiSectorSize * wiisector_src) == 0x10000)
                    {
                        WiiDisc.FixPartitionTable(PartitionSelection.OnlyGamePartition, readBuffer);
                    }

                    readBufferState = 2; //Pronto
                }

                if ((writeBufferState == 1) && (!device.Working)) //Fim da escrita
                {
                    writeBufferState = 0; //Livre
                    progress.Progress++;
                }

                if ((readBufferState == 2) && (writeBufferState == 0))
                {
                    //Troca os buffers
                    byte[] t = readBuffer;
                    readBuffer = writeBuffer;
                    writeBuffer = t;

                    //Aloca um bloco, se ainda não tenha sido
                    if (wbfssector_dest == 0) wbfssector_dest = (ushort)AllocateBlock();

                    device.WriteAsync(writeBuffer, ((long)PartitionOffsetLBA * hdSectorSize) + 
                        ((long)wbfssector_dest * wbfsSectorSize) + 
                        ((long)wiisector_dest * wiiSectorSize), (int)wiiSectorSize);

                    writeBufferState = 1; //Escrevendo...
                    readBufferState = 0; //Permite o buffering de mais um setor

                    ++i; //Avança um setor

                    wiisector_dest = i % (int)wiiSectorsPerWBFSSector; //Calcula os setor relativo de wii

                    if (wiisector_dest == 0) //Mudança de setor wbfs
                    {
                        int wbfssec = i / (int)wiiSectorsPerWBFSSector;
                        //Copia o indice do último setor para a tabela de informação do disco
                        Byte[] v = BitConverter.GetBytes(htons(wbfssector_dest));
                        info[discHeaderCopySize + 2 * (wbfssec - 1)] = v[0];
                        info[discHeaderCopySize + 2 * (wbfssec - 1) + 1] = v[1];

                        wbfssector_dest = 0;

                        //procura pelo proximo setor wbfs usado no destino
                        while ((wbfssec = i / (int)wiiSectorsPerWBFSSector) < wbfsSectorsPerDisc) //Procura proximo setor
                        {
                            if (wbfsUsageTable[wbfssec])
                                break;
                            else
                                i += (int)wiiSectorsPerWBFSSector;
                        }
                        if (wbfssec == wbfsSectorsPerDisc) break; //Acabou o disco

                    }

                    //Calcula os setores da origem
                    wiisector_src = i % (int)wiiSectorsPerWBFSSector_src; //Calcula os setor relativo de wii
                    wbfssector_src = disc.WLBATable[i / (int)wiiSectorsPerWBFSSector_src];

                    if (wbfssector_src == 0) //setor vazio, limpa o buffer aqui
                    {
                        for (int k = 0; k < wiiSectorSize; k++) readBuffer[k] = 0;
                    }
                }

                //Impede 100% de uso de CPU
                //System.Threading.Thread.Sleep(1);
            }

            //Garante que todas as operações assíncronas terminem
            while (device.Working) System.Threading.Thread.Sleep(1);
            while (source.Working) System.Threading.Thread.Sleep(1); //Só ocorrerá se a operação for cancelada

            if (progress.Cancel) //Desaloca os blocos e o disco
            {
                for (i = 0; i < wbfsSectorsPerDisc; i++)
                {
                    wiisector_dest = ntohs(BitConverter.ToUInt16(info, discHeaderCopySize + 2 * i));
                    FreeBlock((uint)wiisector_dest);
                }

                DiscTable[discn] = 0;

                r = (int)WBFSRet.RET_WBFS_ABORTED;
                Log.SendMessage(this.name, (int)WBFSRet.RET_WBFS_ABORTED, null, LogMessageType.Error);
            }
            else
            {
                WriteToDiscInfo(discn, info);
                SynchronizeFileSystem(); //Escreve as alterações no disco
            }

        FREE: //Caso algo dê errado... venha direto para cá...

            device.FreeAsync();
            device.Unlock(); //Libera o contexto do dispositivo

            source.FreeAsync();
            source.Close(); //Fecha o disco

            //Limpa as matrizes
            wiiUsageTable = null;
            wbfsUsageTable = null;
            readBuffer = null;
            writeBuffer = null;
            buffer1 = null;
            buffer2 = null;

            return r;
        }

        //----------------- Adiciona um disco ao dispositivo a partir de um disco scrub
        public int AddDisc(ScrubDisc disc, ProgressIndicator progress, Boolean Copy_1_1)
        {
            int r = 0; //Para guardar o código de erro

            if (disc == null)
            {
                Log.SendMessage(this.name, (int)WBFSRet.RET_WBFS_INVALIDARG, "disc", LogMessageType.Error);
                return (int)WBFSRet.RET_WBFS_INVALIDARG;
            }

            if (progress == null)
            {
                Log.SendMessage(this.name, (int)WBFSRet.RET_WBFS_INVALIDARG, "progress", LogMessageType.Error);
                return (int)WBFSRet.RET_WBFS_INVALIDARG;
            }

            //Verifica se algum jogo no disco já tenha o mesmo codigo e/ou nome
            if (GetDiscByCode(disc.Code) != null)
            {
                //Loga Aviso

                r = (int)WBFSRet.RET_WBFS_DISCREPEATED;
                Log.SendMessage(this.name, (int)WBFSRet.RET_WBFS_DISCREPEATED, disc.Code, LogMessageType.Error);
            }

            if (GetDiscByName(disc.Name) != null)
            {
                //Loga Aviso

                r = (int)WBFSRet.RET_WBFS_DISCREPEATED;
                Log.SendMessage(this.name, (int)WBFSRet.RET_WBFS_DISCREPEATED, disc.Name, LogMessageType.Error);
            }

            //Seleciona um slot vazio na tabela de discos
            int i = 0;
            for (i = 0; i < maxDiscs; i++)
            {
                if (DiscTable[i] == 0)
                {
                    break;
                }
            }

            if (i == maxDiscs)
            {
                Log.SendMessage(this.name, (int)WBFSRet.RET_WBFS_NOFREEBLOCKS, null, LogMessageType.Error);
                return (int)WBFSRet.RET_WBFS_NOFREEBLOCKS;
            }

            int discn = i;

            //Tranca o contexto do dispositivo
            if (device.Lock() != IORet.RET_IO_OK)
            {
                Log.SendMessage(this.name, (int)WBFSRet.RET_WBFS_FAIL, null, LogMessageType.Error);
                return (int)WBFSRet.RET_WBFS_FAIL;
            }

            device.PrepareForAsync();

            //Cria o contexto do disco
            IIOContext source = disc;

            if (source.Result != 0) //Falha ao criar o handle
            {
                result = source.Result;

                //Verifica se a falha é "acesso negado"
                if (source.Result == (int)WinError.ERROR_ACCESS_DENIED)
                {
                    r = (int)WBFSRet.RET_WBFS_ACCESSDENIED;
                }
                else
                {
                    r = (int)WBFSRet.RET_WBFS_NATIVEERROR;
                }

                Log.SendMessage(this.name, r, result, LogMessageType.Error);
                goto FREE;
            }

            //Abre e monta o disco para ter a tabela de setores
            if ((r = (int)source.Lock()) != (int)IORet.RET_IO_OK) { goto FREE; }
            source.PrepareForAsync();

            //Matriz que descreve o disco dentro do sistema de arquivos
            Byte[] info = new Byte[discInfoSize];

            //Tabela de setores
            Byte[] wiiUsageTable = new Byte[wiiSectorsPerDisc];

            //Setores Usados
            uint wiiused = wiiSectorsPerDisc;

            String name = "";
            String code = "";

            if (!Copy_1_1)
            {

                WiiDisc wiidisc = new WiiDisc(source, false);
                wiidisc.Open();
                wiidisc.BuildDisc(PartitionSelection.OnlyGamePartition);

                name = wiidisc.name;
                code = wiidisc.code;

                wiidisc.SectorUsageTable.CopyTo(wiiUsageTable, 0);
                wiidisc.HeaderCopy.CopyTo(info, 0);

                wiiused = wiidisc.UsedSectors;

                //Fecha o disco
                wiidisc.Close();
                wiidisc = null;
            }
            else
            {
                source.Seek(0);
                source.Read(info, 0, 256);

                source.Seek(0);
            }

            int runto = (int)(source.Length > IsoDVD5Size ? wbfsSectorSize : wbfsSectorSize / 2);

            //Cria a tabela de setores WBFS
            Boolean[] wbfsUsageTable = new Boolean[wbfsSectorsPerDisc];
            uint wbfsused = 0;
            for (i = 0; i < wbfsSectorsPerDisc; i++)
            {
                if (Copy_1_1 || IsBlockUsed(wiiUsageTable, (uint)i, wiiSectorsPerWBFSSector))
                {
                    wbfsused++;
                    wbfsUsageTable[i] = true;
                }
            }

            if (CountFreeBlocks() < wbfsused) //Sem espaço no disco
            {
                r = (int)WBFSRet.RET_WBFS_NOFREEBLOCKS;
                Log.SendMessage(this.name, (int)WBFSRet.RET_WBFS_NOFREEBLOCKS, null, LogMessageType.Error);
                goto FREE;
            }

            //Cria os buffers para leitura/escrita assíncrona
            Byte[] buffer1 = new Byte[wiiSectorSize];
            Byte[] buffer2 = new Byte[wiiSectorSize];

            Byte[] readBuffer = buffer1;
            Byte[] writeBuffer = buffer2;

            byte readBufferState = 0; //0 - vazio, 1 - lendo, 2 - cheio
            byte writeBufferState = 0; //0 - vazio, 1 - escrevendo, 2 - cheio

            ushort wbfssector_dest = 0; //Indice na WLBA do disco de destino que representa a posição de um setor WBFS alocado
            ushort wbfssector_src = disc.SectorPositionTable[0];
            //setor WBFS alocado
            int wiisector_dest = 0;
            int wiisector_src = 0;

            uint wiiSectorsPerWBFSSector_src = (uint)disc.WiiSectorsPerWbfsSectors;
            uint wbfsSectorSize_src = (uint)disc.WbfsSectorSize;

            //Reseta o contador de progresso, seta o contexto na posição 0, marca o disco e começa a transferência
            DiscTable[discn] = 1;
            source.Seek(0);
            progress.Reset(wiiused, wiiSectorSize);

            // O fato do 'source' ser DiscReader não influencia no processo de leitura daqui para frente,
            // tanto que ainda é necessário fazer os cálculos de posição dos setores WBFS

            i = 0;
            while (!progress.Cancel)
            {
                if (!device.CanWork) { progress.Cancel = true; break; }

                if (readBufferState == 0)
                {
                    if (wbfssector_src == 0)
                    {
                        readBufferState = 2; //Pronto
                    }
                    else
                    {
                        source.ReadAsync(readBuffer, ((long)wbfsSectorSize_src * wbfssector_src) +
                            ((long)wiiSectorSize * wiisector_src), (int)wiiSectorSize);
                        readBufferState = 1; //Lendo...
                    }
                }

                if ((readBufferState == 1) && (!source.Working)) //Fim da leitura
                {
                    //Conserta a tabela de partição se necessário
                    if (((long)wbfsSectorSize_src * wbfssector_src) + ((long)wiiSectorSize * wiisector_src) == 0x10000)
                    {
                        WiiDisc.FixPartitionTable(PartitionSelection.OnlyGamePartition, readBuffer);
                    }

                    readBufferState = 2; //Pronto
                }

                if ((writeBufferState == 1) && (!device.Working)) //Fim da escrita
                {
                    writeBufferState = 0; //Livre
                    progress.Progress++;
                }

                if ((readBufferState == 2) && (writeBufferState == 0))
                {
                    //Troca os buffers
                    byte[] t = readBuffer;
                    readBuffer = writeBuffer;
                    writeBuffer = t;

                    //Aloca um bloco, se ainda não tenha sido
                    if (wbfssector_dest == 0) wbfssector_dest = (ushort)AllocateBlock();

                    device.WriteAsync(writeBuffer, ((long)PartitionOffsetLBA * hdSectorSize) + 
                        ((long)wbfssector_dest * wbfsSectorSize) +
                        ((long)wiisector_dest * wiiSectorSize), (int)wiiSectorSize);

                    writeBufferState = 1; //Escrevendo...
                    readBufferState = 0; //Permite o buffering de mais um setor

                    ++i; //Avança um setor

                    wiisector_dest = i % (int)wiiSectorsPerWBFSSector; //Calcula os setor relativo de wii

                    if (wiisector_dest == 0) //Mudança de setor wbfs
                    {
                        int wbfssec = i / (int)wiiSectorsPerWBFSSector;
                        //Copia o indice do último setor para a tabela de informação do disco
                        Byte[] v = BitConverter.GetBytes(htons(wbfssector_dest));
                        info[discHeaderCopySize + 2 * (wbfssec - 1)] = v[0];
                        info[discHeaderCopySize + 2 * (wbfssec - 1) + 1] = v[1];

                        wbfssector_dest = 0;

                        //procura pelo proximo setor wbfs usado no destino
                        while ((wbfssec = i / (int)wiiSectorsPerWBFSSector) < wbfsSectorsPerDisc) //Procura proximo setor
                        {
                            if (wbfsUsageTable[wbfssec])
                                break;
                            else
                                i += (int)wiiSectorsPerWBFSSector;
                        }
                        if (wbfssec == wbfsSectorsPerDisc) break; //Acabou o disco

                    }

                    //Calcula os setores da origem
                    wiisector_src = i % (int)wiiSectorsPerWBFSSector_src; //Calcula os setor relativo de wii
                    wbfssector_src = disc.SectorPositionTable[i / (int)wiiSectorsPerWBFSSector_src];

                    if (wbfssector_src == 0) //setor vazio, limpa o buffer aqui
                    {
                        for (int k = 0; k < wiiSectorSize; k++) readBuffer[k] = 0;
                    }
                }

                //Impede 100% de uso de CPU
                //System.Threading.Thread.Sleep(1);
            }

            //Garante que todas as operações assíncronas terminem
            while (device.Working) System.Threading.Thread.Sleep(1);
            while (source.Working) System.Threading.Thread.Sleep(1); //Só ocorrerá se a operação for cancelada

            if (progress.Cancel) //Desaloca os blocos e o disco
            {
                for (i = 0; i < wbfsSectorsPerDisc; i++)
                {
                    wiisector_dest = ntohs(BitConverter.ToUInt16(info, discHeaderCopySize + 2 * i));
                    FreeBlock((uint)wiisector_dest);
                }

                DiscTable[discn] = 0;

                r = (int)WBFSRet.RET_WBFS_ABORTED;
                Log.SendMessage(this.name, (int)WBFSRet.RET_WBFS_ABORTED, null, LogMessageType.Error);
            }
            else
            {
                WriteToDiscInfo(discn, info);
                SynchronizeFileSystem(); //Escreve as alterações no disco
            }

        FREE: //Caso algo dê errado... venha direto para cá...

            device.FreeAsync();
            device.Unlock(); //Libera o contexto do dispositivo

            source.FreeAsync();
            source.Unlock(); //Libera o contexto do disco

            //Limpa as matrizes
            wiiUsageTable = null;
            wbfsUsageTable = null;
            readBuffer = null;
            writeBuffer = null;
            buffer1 = null;
            buffer2 = null;

            return r;
        }

        //----------------- Adiciona um disco ao dispositivo a partir de um disco no drive de DVD-ROM
        public int AddDisc(Char drive, ProgressIndicator progress, Boolean Copy_1_1)
        {
            if (progress == null)
            {
                Log.SendMessage(this.name, (int)WBFSRet.RET_WBFS_INVALIDARG, "progress", LogMessageType.Error);
                return (int)WBFSRet.RET_WBFS_INVALIDARG;
            }

            //Seleciona um slot vazio na tabela de discos
            int i = 0;
            for (i = 0; i < maxDiscs; i++)
            {
                if (DiscTable[i] == 0)
                {
                    break;
                }
            }

            if (i == maxDiscs)
            {
                Log.SendMessage(this.name, (int)WBFSRet.RET_WBFS_NOFREEBLOCKS, null, LogMessageType.Error);
                return (int)WBFSRet.RET_WBFS_NOFREEBLOCKS;
            }

            int discn = i;

            int r = 0; //Para guardar o código de erro

            //Tranca o contexto do dispositivo
            if (device.Lock() != IORet.RET_IO_OK)
            {
                Log.SendMessage(this.name, (int)WBFSRet.RET_WBFS_FAIL, null, LogMessageType.Error);
                return (int)WBFSRet.RET_WBFS_FAIL;
            }

            device.PrepareForAsync();

            //Cria o contexto do arquivo
            IIOContext source = new DVDRomReader(drive);

            if (source.Result != 0) //Falha ao criar o handle
            {
                result = source.Result;

                //Verifica se a falha é "acesso negado"
                if (source.Result == (int)WinError.ERROR_ACCESS_DENIED)
                {
                    r = (int)WBFSRet.RET_WBFS_ACCESSDENIED;
                }
                else
                {
                    r = (int)WBFSRet.RET_WBFS_NATIVEERROR;
                }

                Log.SendMessage(this.name, r, result, LogMessageType.Error);
                goto FREE;
            }

            //Abre e monta o disco para ter a tabela de setores
            if ((r = (int)source.Lock()) != (int)IORet.RET_IO_OK) { goto FREE; }
            source.PrepareForAsync();

            //Matriz que descreve o disco dentro do sistema de arquivos
            Byte[] info = new Byte[discInfoSize];

            //Tabela de setores
            Byte[] wiiUsageTable = new Byte[wiiSectorsPerDisc];

            //Setores Usados
            uint wiiused = wiiSectorsPerDisc;

            String name = "";
            String code = "";

            if (!Copy_1_1)
            {
                WiiDisc disc = new WiiDisc(source, false);
                disc.Open();
                disc.BuildDisc(PartitionSelection.OnlyGamePartition);

                name = disc.name;
                code = disc.code;

                disc.SectorUsageTable.CopyTo(wiiUsageTable, 0);
                disc.HeaderCopy.CopyTo(info, 0);

                wiiused = disc.UsedSectors;

                //Fecha o disco
                disc.Close();
                disc = null;
            }
            else
            {
                source.Seek(0);
                source.Read(info, 0, 256);

                for (i = IsoCodePos; i < IsoCodePos + IsoCodeLen; i++) { code += (char)info[i]; }
                for (i = IsoNamePos; i < IsoNamePos + IsoNameLen; i++) { name += (char)info[i]; }

                source.Seek(0);
            }

            //Verifica se algum jogo no disco já tenha o mesmo codigo e/ou nome
            if (GetDiscByCode(code) != null)
            {
                //Loga Aviso

                r = (int)WBFSRet.RET_WBFS_DISCREPEATED;
                Log.SendMessage(this.name, (int)WBFSRet.RET_WBFS_DISCREPEATED, code, LogMessageType.Error);
                goto FREE;
            }

            if (GetDiscByName(name) != null)
            {
                //Loga Aviso

                r = (int)WBFSRet.RET_WBFS_DISCREPEATED;
                Log.SendMessage(this.name, (int)WBFSRet.RET_WBFS_DISCREPEATED, name, LogMessageType.Error);
                goto FREE;
            }

            //Cria a tabela de setores WBFS
            Boolean[] wbfsUsageTable = new Boolean[wbfsSectorsPerDisc];
            uint wbfsused = 0;
            for (i = 0; i < wbfsSectorsPerDisc; i++)
            {
                if (Copy_1_1 || IsBlockUsed(wiiUsageTable, (uint)i, wiiSectorsPerWBFSSector))
                {
                    wbfsused++;
                    wbfsUsageTable[i] = true;
                }
            }

            if (CountFreeBlocks() < wbfsused) //Sem espaço no disco
            {
                r = (int)WBFSRet.RET_WBFS_NOFREEBLOCKS;
                Log.SendMessage(this.name, (int)WBFSRet.RET_WBFS_NOFREEBLOCKS, null, LogMessageType.Error);
                goto FREE;
            }

            //Cria os buffers para leitura/escrita assíncrona
            Byte[] buffer1 = new Byte[wiiSectorSize];
            Byte[] buffer2 = new Byte[wiiSectorSize];

            Byte[] readBuffer = buffer1;
            Byte[] writeBuffer = buffer2;

            byte readBufferState = 0; //0 - vazio, 1 - lendo, 2 - cheio
            byte writeBufferState = 0; //0 - vazio, 1 - escrevendo, 2 - cheio

            ushort wbfssector = 0; //Indice na WLBA do disco que representa a posição de um setor WBFS alocado
            int wiisector = 0;

            //Reseta o contador de progresso, seta o contexto na posição 0, marca o disco e começa a transferência
            DiscTable[discn] = 1;
            source.Seek(0);
            progress.Reset(wiiused, wiiSectorSize);

            i = 0;
            while (!progress.Cancel)
            {
                if (!device.CanWork) { progress.Cancel = true; break; }

                if (readBufferState == 0)
                {
                    source.ReadAsync(readBuffer, ((long)wbfsSectorSize * i) + ((long)wiiSectorSize * wiisector), 
                        (int)wiiSectorSize);
                    readBufferState = 1; //Lendo...
                }

                if ((readBufferState == 1) && (!source.Working)) //Fim da leitura
                {
                    //Conserta a tabela de partição se necessário
                    if (((long)wbfsSectorSize * i) + ((long)wiiSectorSize * wiisector) == 0x10000)
                    {
                        WiiDisc.FixPartitionTable(PartitionSelection.OnlyGamePartition, readBuffer);
                    }

                    readBufferState = 2; //Pronto
                }

                if ((writeBufferState == 1) && (!device.Working)) //Fim da escrita
                {
                    writeBufferState = 0; //Livre
                    progress.Progress++;
                }

                if ((readBufferState == 2) && (writeBufferState == 0))
                {
                    //Troca os buffers
                    byte[] t = readBuffer;
                    readBuffer = writeBuffer;
                    writeBuffer = t;

                    //Aloca um bloco, se ainda não tenha sido
                    if (wbfssector == 0) wbfssector = (ushort)AllocateBlock();

                    device.WriteAsync(writeBuffer, ((long)PartitionOffsetLBA * hdSectorSize) + 
                        ((long)wbfssector * wbfsSectorSize) + ((long)wiisector * wiiSectorSize), (int)wiiSectorSize);

                    writeBufferState = 1; //Escrevendo...
                    readBufferState = 0; //Permite o buffering de mais um setor

                    if (++wiisector == wiiSectorsPerWBFSSector) //O setor wbfs atual acabou
                    {
                        //Copia o indice do último setor para a tabela de informação do disco
                        Byte[] v = BitConverter.GetBytes(htons((ushort)wbfssector));
                        info[discHeaderCopySize + 2 * i] = v[0];
                        info[discHeaderCopySize + 2 * i + 1] = v[1];

                        //
                        wiisector = 0;
                        wbfssector = 0;

                        while (++i < wbfsSectorsPerDisc) if (wbfsUsageTable[i]) break; //Procura proximo setor
                        if (i == wbfsSectorsPerDisc) break; //Acabou o disco
                    }
                }

                //Impede 100% de uso de CPU
                //System.Threading.Thread.Sleep(1);
            }

            //Garante que todas as operações assíncronas terminem
            while (device.Working) System.Threading.Thread.Sleep(1);
            while (source.Working) System.Threading.Thread.Sleep(1); //Só ocorrerá se a operação for cancelada

            if (progress.Cancel) //Desaloca os blocos e o disco
            {
                for (i = 0; i < wbfsSectorsPerDisc; i++)
                {
                    wiisector = ntohs(BitConverter.ToUInt16(info, discHeaderCopySize + 2 * i));
                    FreeBlock((uint)wiisector);
                }

                DiscTable[discn] = 0;

                r = (int)WBFSRet.RET_WBFS_ABORTED;
                Log.SendMessage(this.name, (int)WBFSRet.RET_WBFS_ABORTED, null, LogMessageType.Error);
            }
            else
            {
                WriteToDiscInfo(discn, info);
                SynchronizeFileSystem(); //Escreve as alterações no disco
            }

        FREE: //Caso algo dê errado... venha direto para cá...

            device.FreeAsync();
            device.Unlock(); //Libera o contexto do dispositivo

            source.FreeAsync();
            source.Close(); //Fecha o disco

            //Limpa as matrizes
            wiiUsageTable = null;
            wbfsUsageTable = null;
            readBuffer = null;
            writeBuffer = null;
            buffer1 = null;
            buffer2 = null;

            return r;
        }

        //----------------- Extrai um disco do dispositivo para o PC
        public int ExtractDisc(String fileout, IDisc disc, ProgressIndicator progress)
        {
            //Variável para guardar o código de retorno
            int r = 0;
            
            //A mística, cosmica, poderosa e semi-fenomenal variável idolatrada pelos programadores... o 'i'
            int i = 0;

            //Não dá para extrair um disco já deletado
            if (disc.Deleted) return (int)WBFSRet.RET_WBFS_DISCNOTFOUND;

            //O Diretório precisa ser válido
            if (!Directory.Exists(Path.GetDirectoryName(fileout))) return (int)WBFSRet.RET_WBFS_INVALIDARG;

            //Tranca o contexto do dispositivo
            if (device.Lock() != IORet.RET_IO_OK) return (int)WBFSRet.RET_WBFS_FAIL;

            //Prepara o dispositivo para rodar de modo assíncrono
            device.PrepareForAsync();

            //Cria o contexto do arquivo de saída
            IIOContext destination = IOManager.CreateIOContext("EXTRACT", fileout, FileAccess.Write, FileShare.Read, 0,
                FileMode.OpenOrCreate, EFileAttributes.None);

            if (destination.Result != 0) //Falha ao criar o handle
            {
                result = destination.Result;

                //Verifica se a falha é "acesso negado"
                if (destination.Result == (int)WinError.ERROR_ACCESS_DENIED)
                {
                    r = (int)WBFSRet.RET_WBFS_ACCESSDENIED;
                }
                else
                {
                    r = (int)WBFSRet.RET_WBFS_NATIVEERROR;
                }

                Log.SendMessage(this.name, r, result, LogMessageType.Error);
                goto FREE;
            }

            //Abre e monta o disco para ter a tabela de setores
            if (destination.Lock() != IORet.RET_IO_OK)
            {
                Log.SendMessage(this.name, (int)WBFSRet.RET_WBFS_FAIL, null, LogMessageType.Error);
                r = (int)WBFSRet.RET_WBFS_FAIL;
                goto FREE;
            }

            //Prepara o destino para rodar de modo assíncrono
            destination.PrepareForAsync();

            //Cria os buffers para leitura/escrita assíncrona
            Byte[] buffer1 = new Byte[wbfsSectorSize]; //Buffers um pouco maiores do que em AddDisc(...)
            Byte[] buffer2 = new Byte[wbfsSectorSize];
            Byte[] zeroBuffer = new Byte[wbfsSectorSize]; //buffer vazio para setores vazios, é mais rápido do que um
                                                   //Seek() em um setor nao alocado do destino que força o sistema
                                                   //operacional a ir criando setores até lá, resolve também o "Travão"
                                                   //na barra de progresso que alguns programas têm ao extrair o disco

            Byte[] readBuffer = buffer1;
            Byte[] writeBuffer = buffer2;

            byte readBufferState = 0; //0 - vazio, 1 - lendo, 2 - cheio
            byte writeBufferState = 0; //0 - vazio, 1 - escrevendo, 2 - cheio

            ushort wbfssector = 0; //Indice na WLBA do disco que representa a posição de um setor WBFS alocado
            uint zeroSectors = 0; //Quantidade de setores vazios antes de um usado, assim o programa pode ir lendo um setor
                                  //enquanto escreve os vazios

            //Seta o tamanho final do arquivo para nao faltar espaço enquanto extrai
            destination.Seek(disc.Size);
            destination.SetEOF();

            //wbfsSectorsPerDisc é o total de setores de um DVD9
            int runto = disc.Size == IsoDVD5Size ? wbfsSectorsPerDisc / 2 : wbfsSectorsPerDisc;

            //Volta para o começo do arquivo e zera o contador de progresso
            destination.Seek(0);
            progress.Reset(runto, wiiSectorSize);

            i = 0;
            wbfssector = disc.WLBATable[i]; //Supondo que o setor 0 sempre será usado por causa da identificação do disco

            while (!progress.Cancel)
            {
                if (!device.CanWork) { progress.Cancel = true; break; }

                if (readBufferState == 0)
                {
                    device.ReadAsync(readBuffer, ((long)PartitionOffsetLBA * hdSectorSize) + 
                        ((long)wbfsSectorSize * wbfssector), (int)wbfsSectorSize);
                    readBufferState = 1; //Lendo...
                }

                if ((readBufferState == 1) && (!device.Working)) //Fim da leitura
                {
                    readBufferState = 2; //Pronto
                }

                if ((writeBufferState == 1) && (!destination.Working)) //Fim da escrita
                {
                    //Escreve os setores vazios, essa operação é síncrona
                    while (zeroSectors-- > 0)
                    {
                        destination.Write(zeroBuffer, (int)wbfsSectorSize);
                        progress.Progress++;
                    }

                    zeroSectors = 0;

                    writeBufferState = 0; //Livre
                    progress.Progress++;

                    if (i == runto) break; //Acabou o disco
                }

                if ((readBufferState == 2) && (writeBufferState == 0))
                {
                    //Troca os buffers
                    byte[] t = readBuffer;
                    readBuffer = writeBuffer;
                    writeBuffer = t;

                    destination.WriteAsync(writeBuffer, (int)wbfsSectorSize);

                    writeBufferState = 1; //Escrevendo...
                    readBufferState = 0; //Permite o buffering de mais um setor

                    while (++i < runto) //Procura proximo setor
                    {
                        if (disc.WLBATable[i] != 0)
                        {
                            wbfssector = disc.WLBATable[i];
                            break;
                        }
                        else
                        {
                            zeroSectors++;
                        }
                    }

                    if (i == runto)
                    {
                        readBufferState = 3; //Tranca o buffer de leitura
                    }
                }

                //Impede 100% de uso de CPU
                //System.Threading.Thread.Sleep(1);
            }

            //Garante que todas as operações assíncronas terminem
            while (device.Working) System.Threading.Thread.Sleep(1);
            while (destination.Working) System.Threading.Thread.Sleep(1); //Só ocorrerá se a operação for cancelada

        FREE: //Caso algo dê errado... venha direto para cá...

            device.FreeAsync();
            device.Unlock();

            destination.FreeAsync();
            destination.Close();

            if (progress.Cancel) //Apaga o arquivo de destino
            {
                r = (int)WBFSRet.RET_WBFS_ABORTED;
                Log.SendMessage(this.name, (int)WBFSRet.RET_WBFS_ABORTED, null, LogMessageType.Error);
                IOManager.Delete(fileout);
            }

            return r;
        }

        //----------------- Calcula o tamanho que uma iso irá ocupar no drive
        public long CalculateDiscSize(String filename)
        {
            if (filename.Length == 0)
            {
                Log.SendMessage(this.name, (int)WBFSRet.RET_WBFS_INVALIDARG, "filename", LogMessageType.Error);
                return (int)WBFSRet.RET_WBFS_INVALIDARG;
            }

            long r = 0; //Para guardar o código de erro ou retornar o tamanho do disco

            //Cria o contexto do arquivo
            IIOContext source = IOManager.CreateIOContext("ADD", filename, FileAccess.Read, FileShare.Read, 0,
                FileMode.Open, EFileAttributes.None);

            if (source.Result != 0) //Falha ao criar o handle
            {
                result = source.Result;

                //Verifica se a falha é "acesso negado"
                if (source.Result == (int)WinError.ERROR_ACCESS_DENIED)
                {
                    r = (int)WBFSRet.RET_WBFS_ACCESSDENIED;
                }
                else
                {
                    r = (int)WBFSRet.RET_WBFS_NATIVEERROR;
                }

                Log.SendMessage(this.name, (int)r, result, LogMessageType.Error);
                goto END;
            }

            //Abre e monta o disco para ter a tabela de setores
            if ((r = (int)source.Lock()) != (int)IORet.RET_IO_OK) { goto END; }

            //Tabela de setores
            Byte[] wiiUsageTable = new Byte[wiiSectorsPerDisc];

            WiiDisc disc = new WiiDisc(source, false);
            disc.Open();
            disc.BuildDisc(PartitionSelection.OnlyGamePartition);

            disc.SectorUsageTable.CopyTo(wiiUsageTable, 0);

            //Fecha o disco e o arquivo de origem
            disc.Close();
            disc = null;

            //Conta os setores usados
            uint wbfsused = 0;
            for (int i = 0; i < wbfsSectorsPerDisc; i++)
            {
                if (IsBlockUsed(wiiUsageTable, (uint)i, wiiSectorsPerWBFSSector))
                {
                    wbfsused++;
                }
            }

            r = wbfsused * (long)wbfsSectorSize;

        END:

            if (source != null) source.Close();
            return r;
        }

        //----------------- Calcula o tamanho que uma iso irá ocupar no drive
        public long CalculateDiscSize(Byte[] wiiSectorUsageTable)
        {
            if ((wiiSectorUsageTable == null) || (wiiSectorUsageTable.Length != wiiSectorsPerDisc))
            {
                Log.SendMessage(this.name, (int)WBFSRet.RET_WBFS_INVALIDARG, "wiiSectorUsageTable", LogMessageType.Error);
                return (int)WBFSRet.RET_WBFS_INVALIDARG;
            }

            //Conta os setores usados
            uint wbfsused = 0;
            for (int i = 0; i < wbfsSectorsPerDisc; i++)
            {
                if (IsBlockUsed(wiiSectorUsageTable, (uint)i, wiiSectorsPerWBFSSector))
                {
                    wbfsused++;
                }
            }

            return wbfsused * (long)wbfsSectorSize;
        }
        
        //----------------- Salva o cabeçalho do sistema de arquivos no disco
        private void SynchronizeFileSystem()
        {
            WriteSector(PartitionOffsetLBA, 1, Head);
            if (WLBATable != null)
            {
                int[] iWLBATable = new int[WLBATableSize / 4];
                WLBATable.CopyTo(iWLBATable, 0);
                for (int i = 0; i < iWLBATable.Length; i++) iWLBATable[i] = (int)htoni((uint)iWLBATable[i]);

                WriteStream(((long)(PartitionOffsetLBA + WLBAPositionLBA)) << hdSectorSize_s, (uint)iWLBATable.Length, 
                    iWLBATable);

                iWLBATable = null;
            }
        }

        //----------------- Verifica se o drive é WBFS
        public static int IsWBFSDrive(String drive, bool force)
        {
            Boolean isVirtual = false;
            IIOContext device = null;

            Byte[] Head = null; //Cabeçalho do sistema de arquivos WBFS

            uint hdSectorSize = 0; //Tamanho do setor do drive
            byte hdSectorSize_s = 0; //Tamanho do setor em rotações de bit
            uint hdTotalSectors = 0; //Total de setores do drive
            byte hdTotalSectors_s = 0; //Total de setores em rotações de bit

            if (drive.Length == 0)
            {
                Log.SendMessage(drive, (int)WBFSRet.RET_WBFS_INVALIDARG, "drive name", LogMessageType.Error);
                return (int)WBFSRet.RET_WBFS_INVALIDARG;
            }
            if (drive.EndsWith(":\\")) drive = drive.Remove(drive.Length - 2);

            String openstring = "";
            String contextname = "";

            if (drive.Length > 1) //É um arquivo
            {
                contextname = Path.GetFileName(drive);
                openstring = drive;
                isVirtual = true;
            }
            else
            {
                contextname = drive + ":\\";
                openstring = @"\\.\" + drive + ':'; //Acesso direto a escrita e leitura no disco
            }

            //Cria o Handle que permite ler e escrever no disco
            device = IOManager.CreateIOContext(contextname, openstring, FileAccess.ReadWrite,
                FileShare.None, 0, FileMode.Open, EFileAttributes.NoBuffering);

            if (device.Result != 0) //Falha ao abrir o disco
            {
                //Verifica se a falha é "acesso negado"
                if (device.Result == (int)WinError.ERROR_ACCESS_DENIED)
                {
                    //Loga a informação
                    //Para o IsWBFSDevice isso não seria um erro propriamente dito
                    Log.SendMessage(drive, (int)WBFSRet.RET_WBFS_ACCESSDENIED, null, LogMessageType.Warning);

                    //Retorna o erro
                    return (int)WBFSRet.RET_WBFS_ACCESSDENIED;
                }
                else
                {
                    //Loga a informação
                    Log.SendMessage(drive, (int)WBFSRet.RET_WBFS_NATIVEERROR, device.Result, LogMessageType.Warning);

                    //Retorna o erro
                    return (int)WBFSRet.RET_WBFS_NATIVEERROR;
                }
            }

            if (device.Lock() != IORet.RET_IO_OK)
            {
                //Loga a informação
                Log.SendMessage(drive, (int)WBFSRet.RET_WBFS_FAIL, null, LogMessageType.Warning);

                Head = null;
                device.Close();

                return (int)WBFSRet.RET_WBFS_FAIL;
            }

            if (!isVirtual)
            {

                //Lê a geometria do disco e informação da partição
                DISK_GEOMETRY dg = device.GetDiskGeometry();
                PARTITION_INFORMATION pi = device.GetPartitionInformation();

                //Seta o tamanho do setor, total de setores e seus respectivos logs
                hdSectorSize = (dg.BytesPerSector == 0 ? 512 : dg.BytesPerSector);
                hdTotalSectors = (uint)(pi.PartitionLength / hdSectorSize);
            }
            else //É um drive virtual, o tamanho e quantidade dos setores depende do arquivo e não da partição
            {
                //O tamanho do arquivo
                FileInfo file = new FileInfo(drive);

                hdSectorSize = 512;
                hdTotalSectors = (uint)(file.Length / hdSectorSize);

                file = null;
            }

            hdSectorSize_s = bitshift((uint)hdSectorSize);
            hdTotalSectors_s = bitshift((uint)hdTotalSectors);

            //Aloca o cabeçalho do sistema de arquivos
            Head = new Byte[hdSectorSize];

            //Lê o cabeçalho do disco
            if (device.Read(Head, 0, (int)hdSectorSize) != IORet.RET_IO_OK)
            {
                //Loga erro aqui
                Log.SendMessage(drive, (int)WBFSRet.RET_WBFS_FAIL, null, LogMessageType.Warning);

                Head = null;
                device.Close();

                return (int)WBFSRet.RET_WBFS_FAIL;
            }

            //--------- Lê e compara o código de verificação do sistema de arquivos (Magic) e informaçoes
            // da partição
            uint discWbfsMagic = BitConverter.ToUInt32(Head, wbfsHeadMagicPos);
            uint discTotalSectors = ntohi(BitConverter.ToUInt32(Head, wbfsHeadHdSecsPos));
            byte discSectorSize_s = Head[wbfsHeadHdSecSz];

            if (discWbfsMagic != htoni(wbfsMagic))
            {
                //Loga erro aqui
                if (!force)
                {
                    Head = null;
                    device.Close();

                    Log.SendMessage(drive, (int)WBFSRet.RET_WBFS_BADFSMAGIC, discWbfsMagic, LogMessageType.Warning);
                    return (int)WBFSRet.RET_WBFS_BADFSMAGIC;
                }
            }

            if (discTotalSectors != hdTotalSectors)
            {
                //Loga erro aqui

                if (!force)
                {
                    Head = null;
                    device.Close();

                    Log.SendMessage(drive, (int)WBFSRet.RET_WBFS_BADFSTSECS, discTotalSectors, LogMessageType.Warning);
                    return (int)WBFSRet.RET_WBFS_BADFSTSECS;
                }
            }

            if (discSectorSize_s != hdSectorSize_s)
            {
                if (!force)
                {
                    Head = null;
                    device.Close();

                    Log.SendMessage(drive, (int)WBFSRet.RET_WBFS_BADFSSECSZ, discSectorSize_s, LogMessageType.Warning);
                    return (int)WBFSRet.RET_WBFS_BADFSSECSZ;
                }
            }

            //--------- libera acesso ao handle
            Head = null;
            device.Close();

            return (int)WBFSRet.RET_WBFS_OK;
        }

        //----------------- Rotinas variadas

        //----------------- Network to Host
        public static uint ntohi(uint i) { return (uint)IPAddress.NetworkToHostOrder((int)i); }
        public static ushort ntohs(ushort i) { return (ushort)IPAddress.NetworkToHostOrder((short)i); }
        public static ulong ntohl(ulong i) { return (ulong)IPAddress.NetworkToHostOrder((long)i); }

        //----------------- Host to Network
        public static uint htoni(uint i) { return (uint)IPAddress.HostToNetworkOrder((int)i); }
        public static ushort htons(ushort i) { return (ushort)IPAddress.HostToNetworkOrder((short)i); }
        public static ulong htonl(ulong i) { return (ulong)IPAddress.HostToNetworkOrder((long)i); }

        //----------------- carrega a LBATable
        public int LoadWLBATable()
        {
            if (WLBATable != null) return 0;
            int[] iWLBATable = new int[WLBATableSize / 4];

            int r = ReadStream(((long)(PartitionOffsetLBA + WLBAPositionLBA)) << hdSectorSize_s, (uint)iWLBATable.Length, 
                iWLBATable);
            if (r < 0) return r;

            for (int i = 0; i < iWLBATable.Length; i++) iWLBATable[i] = (int)ntohi((uint)iWLBATable[i]);

            WLBATable = new BitArray(iWLBATable);
            
            iWLBATable = null;
            return 0;
        }
        
        //----------------- Conta o número de blocos livres
        private long CountFreeBlocks()
        {
            int r = LoadWLBATable();
            if (r < 0) return r;

            uint count = 0;
            for (int i = 0; i < wbfsTotalSectors; i++)
            {
                if (WLBATable[i]) count++;
            }

            return count;
        }

        //----------------- Reserva um setor WBFS
        private uint AllocateBlock()
        {
            for (int i = 0; i < WLBATable.Count; i++)
            {
                if (WLBATable[i])
                {
                    WLBATable[i] = false;
                    return (uint)i + 1; // +1 porque o wlba = 0 é reservado para blocos vazios
                }
            }

            return ~(uint)0; //Não achou nenhum
        }

        //----------------- Libera um setor WBFS
        private void FreeBlock(uint block)
        {
            if (block == 0) return;
            WLBATable[(int)block - 1] = true;
        }

        //----------------- Limpa a tabela de setores
        public void ClearWLBATable()
        {
            WLBATable.SetAll(true);
        }

        //----------------- Limpa a tabela de discos
        public void ClearDiscTable()
        {
            for (int i = 0; i < maxDiscs; i++)
            {
                DiscTable[i] = 0;
            }
        }

        //----------------- Lê um setor do disco de uma determinada posição
        private int ReadSector(long lba, uint count, Array buffer)
        {
            device.Read(buffer, lba * hdSectorSize, (int)(count * hdSectorSize));
            return device.Result;
        }

        //----------------- Lê uma sequencia de bytes do disco de uma determinada posição
        private int ReadStream(long pos, uint count, Array buffer)
        {
            device.Read(buffer, pos, (int)count);
            return device.Result;
        }

        //----------------- Escreve um setor do disco de uma determinada posição
        private int WriteSector(long lba, uint count, Array buffer)
        {
            device.Write(buffer, lba << hdSectorSize_s, (int)(count * hdSectorSize));
            return device.Result;
        }

        //----------------- Escreve uma sequencia de bytes do disco de uma determinada posição
        private int WriteStream(long pos, uint count, Array buffer)
        {
            device.Write(buffer, pos, (int)count);
            return device.Result;
        }

        //----------------- Escreve uma sequencia de bytes para o setor de informação do disco
        private int WriteToDiscInfo(int discn, Array info)
        {
            device.Write(info, (PartitionOffsetLBA + 1) * (long)hdSectorSize + discn * (long)discInfoSize, discInfoSize);
            return device.Result;
        }

        //----------------- Retorna o numero de rotações de bit para a esquerda de um número
        public static byte bitshift(uint size)
        {
            byte r = 0;
            
            do
            {
                size >>= 1;
                r++;

            } while (size > 1);

            return r;
        }

        //----------------- verifica se o setor WBFS será usado por conter 1 ou mais setores de wii usados pelo disco
        public static Boolean IsBlockUsed(Byte[] used, uint i, uint block_size)
        {
            uint k = 0;
            i *= block_size;

            for (k = 0; k < block_size; k++)
            {
                if ((k < wiiSectorsPerDisc) && (used[i + k] != 0))
                    return true;
            }

            return false;
        }

        //----------------- alinhamento da LBA (sim, eu não sei direito o que isso quer dizer)
        private int Align_LBA(int x)
        {
            return ((x) + (int)hdSectorSize - 1) & (~((int)hdSectorSize - 1));
        }

        //----------------- alinha a LBA (sim, eu não sei direito o que isso quer dizer)
        private static int Align_LBA(int x, uint hdSectorSize)
        {
            return ((x) + (int)hdSectorSize - 1) & (~((int)hdSectorSize - 1));
        }

        //----------------- Disparadores de eventos
        private void OnDeviceOpened() { if (DeviceOpened != null) DeviceOpened(this); }
        private void OnDeviceClosing() { if (DeviceClosing != null) DeviceClosing(this); }
        private void OnDeviceClosed() { if (DeviceClosed != null) DeviceClosed(this); }
        private void OnDeviceUpdated() { if (DeviceUpdated != null) DeviceUpdated(this); }

        private void OnDiscAdded(IDisc disc) { if (DiscAdded != null) DiscAdded(this, disc); }
        private void OnDiscRemoved(IDisc disc) { if (DiscRemoved != null) DiscRemoved(this, disc); }
        private void OnDiscDataChanged(IDisc disc) { if (DiscDataChanged != null) DiscDataChanged(this, disc); }

        //----------------- Parte do IDisposable, garante que o objeto seja destruido pelo GC quando nao usado
        public void Dispose()
        {
            Close();
        }
    }
}
