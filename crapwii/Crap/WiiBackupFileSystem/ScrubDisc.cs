//-------------------------------------
// WBFSSync - WBFSSync.exe
//
// Copyright 2009 Caian (ÔmΣga Frøst) <frost.omega@hotmail.com>
//
// WBFSSync is Licensed under the terms of the Microsoft Reciprocal License (Ms-RL)
//
// ScrubDisc.cs:
//
// Compressor de isos, funciona da mesma forma que o sistema de arquivos WBFS, apenas os setores usados
// são salvos e uma matriz contendo as quais setores foram ou não usados é salva no cabeçalho para facilitar a leitura,
// implementa a interface IIOContext para ser usado por outas classes
//
// Formato (1.0):
//
// Assumindo hardware que trabalha com o formato little-endian
//
// Offset  | Nome              | Tipo
// -------------------------------------------
//  0000h  | ID "SYNCSCRUB"    | Byte[9]
//  0009h  | VERSION           | ushort
//  000Ah  | ISO CODE          | Byte[6]
//  0010h  | ISO NAME          | Byte[32]                     *O espaço não utilizado será preenchido por zeros
//  0030h  | ISO REGION        | sbyte
//  0031h  | ISO SIZE          | long
//  0039h  | WBFS SECTOR SIZE  | byte                         *Tamanho em rotações de bit
//  003Ah  | WBFS USED SECTORS | uint
//  003Eh  | WBFS SECTOR TABLE | BitArray salvo como Byte[]
//  -----  | PARTITION INFO    | byte                         *Partições salvas no scrub
//  -----  | PADDING           | WbfsSectorSize - base header *Alinhamento para poder usar a 'SectorPositionTable' onde 0 é vazio
//  -----  | DATA              | Byte[]                       *Tamanho depende do número de setores usados pelo disco
//-------------------------------------

using System;
using System.Collections.Generic;
//using System.Linq;
using System.Text;
using System.Collections;
using System.IO;

namespace WBFSSync
{
    public enum ScrubberRet : int
    {
        RET_SCRUB_OK = 0,
        RET_SCRUB_BADMAGIC = -2001, // os primeiros 9 bytes do arquivo não são "SYNCSCRUB"
        RET_SCRUB_INCORRECTVERSION = -2002, //versão não suportada
        RET_SCRUB_INCONSISTENTDATA = -2003, //o tamanho do arquivo é diferente de: 
                                            // Cabeçalho + (Setores Usados * Tamanho do Setor de Wii)
        RET_SCRUB_INVALIDARG = -2004,
        RET_SCRUB_FAIL = -2005,
        RET_SCRUB_ABORTED = -2006,
    }

    public class ScrubDisc : IIOContext
    {
        //------------------ Constantes
        
        public const string ext_wiiscrub = ".syncscrub";
        public const string typefilter_wiiscrub = "*.syncscrub";

        public const ushort current_version = 1000;
        
        //Para a versão 1.0
        public const int base_header_size_1000 = 63;
        
        public const byte default_wbfsSectorSize_s_1000 = 21;
        public const uint default_wbfsSectorSize_1000 = 1 << 21;
        public const uint default_wiiSectorsPerWbfsSectors_1000 = (int)(default_wbfsSectorSize_1000 / WBFSDevice.wiiSectorSize);
        public const uint default_wbfsSectorsPerDisc_1000 = (int)(WBFSDevice.wiiSectorsPerDisc / default_wiiSectorsPerWbfsSectors_1000);
        public const uint default_wbfsUsageTableSize_1000 = default_wbfsSectorsPerDisc_1000 / 8;

        //------------------ Variáveis
        BitArray sectorUsageTable = null;
        ushort[] sectorPositionTable = null;

        String code = "";
        String name = "";
        DiscRegion region = DiscRegion.Unknown;
        PartitionSelection partition = PartitionSelection.OnlyGamePartition;

        uint usedWbfsSectors = 0;
        long scrubSize = 0;
        long isoSize = 0;

        int wbfsSectorSize = 0;
        int wbfsSectorSize_s = 0;
        int wbfsSectorsPerDisc = 0;
        int wiiSectorsPerWbfsSectors = 0;
        int wbfsUsageTableSize = 0;

        IIOContext context = null;
        long discPosition = 0;
        long scrubPosition = 0;
        uint scrubOffset = 0;

        public String Name { get { return name; } }
        public String Code { get { return code; } }
        public DiscRegion Region { get { return region; } }
        public PartitionSelection Partition { get { return partition; } }

        public int WbfsSectorSize { get { return wbfsSectorSize; } }
        public int WbfsSectorsPerDisc { get { return wbfsSectorsPerDisc; } }
        public int WiiSectorsPerWbfsSectors { get { return wiiSectorsPerWbfsSectors; } }

        public ushort[] SectorPositionTable { get { return sectorPositionTable; } }

        public long ScrubSize { get { return scrubSize; } }
        public uint UsedWbfsSectors { get { return usedWbfsSectors; } }

        //-------------- Implementação da interface IIOContext

        public int Handle { get { return context.Handle; } }
        public long Position { get { return discPosition; } }
        public long Length { get { return isoSize; } }
        public int Result { get { return context.Result; } }
        public bool Working { get { return context.Working; } }

        public String Path { get { return context.Path; } }
        public bool Closed { get { return context.Closed; } }
        public bool CanWork { get { return context.CanWork; } }
        public bool Locked { get { return context.Locked; } }

        //------------------ Rotinas

        //------------------ Disco Scrub

        //------------------ Abre um disco scrub
        public int Open(String file, bool open_unsafe)
        {
            int r = 0;

            context = IOManager.CreateIOContext("SCRUBDISC", file, System.IO.FileAccess.Read, System.IO.FileShare.Read,
                0, System.IO.FileMode.Open, EFileAttributes.None);
            if (context.Result != 0)
            {
                //Loga erro

                return context.Result;
            }
            if ((r = (int)context.Lock()) != (int)IORet.RET_IO_OK)
            {
                //Loga erro

                context.Close();
                return r;
            }

            //Verifica a consistência do cabeçalho

            if (context.Length < base_header_size_1000)
            {
                //Loga o erro
                return (int)ScrubberRet.RET_SCRUB_INCONSISTENTDATA;
            }

            Byte[] Magic = new Byte[9];
            context.Read(Magic, 9);

            if (!CompareMagic(Magic))
            {
                //Loga erro
                context.Close();
                Log.SendMessage(System.IO.Path.GetFileName(file), (int)ScrubberRet.RET_SCRUB_BADMAGIC, Magic, 
                    LogMessageType.Error);
                return (int)ScrubberRet.RET_SCRUB_BADMAGIC;
            }

            UInt16 version = context.ReadUInt16();
            if (version != current_version)
            {
                context.Close();
                Log.SendMessage(System.IO.Path.GetFileName(file), (int)ScrubberRet.RET_SCRUB_INCORRECTVERSION, version, 
                    LogMessageType.Error);
                return (int)ScrubberRet.RET_SCRUB_INCORRECTVERSION;
            }

            //Lê o cabeçalho do arquivo
            
            code = context.ReadString(false, 6);
            name = context.ReadString(false, 32);
            context.Seek(49);
            region = (DiscRegion)context.ReadByte();
            isoSize = (long)context.ReadUInt64();

            wbfsSectorSize_s = context.ReadByte();
            wbfsSectorSize = 1 << wbfsSectorSize_s;
            wiiSectorsPerWbfsSectors = (int)(wbfsSectorSize / WBFSDevice.wiiSectorSize);
            wbfsSectorsPerDisc = (int)(WBFSDevice.wiiSectorsPerDisc / wiiSectorsPerWbfsSectors);
            wbfsUsageTableSize = (wbfsSectorsPerDisc - 1) / 8 + 1;

            usedWbfsSectors = context.ReadUInt32();
            scrubSize = wbfsSectorSize * usedWbfsSectors;
            
            //Verifica a consistência do arquivo

            if (context.Length != ((usedWbfsSectors + 1) << wbfsSectorSize_s))
            {
                if (open_unsafe)
                {
                    //Loga o aviso
                }
                else
                {
                    //Loga o erro
                    context.Close();
                    return (int)ScrubberRet.RET_SCRUB_INCONSISTENTDATA;
                }
            }

            //Lê a tabela de uso e calcula a Posição dos setores

            Byte[] bUsageTable = new Byte[wbfsUsageTableSize];
            context.Read(bUsageTable, wbfsUsageTableSize);
            sectorUsageTable = new BitArray(bUsageTable);

            sectorPositionTable = new ushort[wbfsSectorsPerDisc];

            ushort p = 1; //Primeiro setor
            for (int i = 0; i < wbfsSectorsPerDisc; i++)
            {
                sectorPositionTable[i] = (ushort)(sectorUsageTable[i] ? p++ : 0);
            }

            byte bPs = context.ReadByte();
            switch (bPs)
            {
                default: //Por padrão só a partição do jogo vai no Scrub
                case 0: partition = PartitionSelection.OnlyGamePartition; break;
                case 1: partition = PartitionSelection.RemoveUpdatePartition; break;
                case 2: partition = PartitionSelection.AllPartitions; break;
            }

            context.Unlock();
            //Não fecha o contexto

            return (int)ScrubberRet.RET_SCRUB_OK;
        }

        //------------------ Extrai um disco scrub

        public int Extract(String fileout, ProgressIndicator progress)
        {
            //Variável para guardar o código de retorno
            int r = 0;

            //A mística, cosmica, poderosa e semi-fenomenal variável idolatrada pelos programadores... o 'i'
            int i = 0;

            //O Diretório precisa ser válido
            if (!Directory.Exists(System.IO.Path.GetDirectoryName(fileout))) return (int)ScrubberRet.RET_SCRUB_INVALIDARG;

            //Tranca o contexto do dispositivo
            if (context.Lock() != IORet.RET_IO_OK) return (int)ScrubberRet.RET_SCRUB_FAIL;

            //Prepara o dispositivo para rodar de modo assíncrono
            context.PrepareForAsync();

            //Cria o contexto do arquivo de saída
            IIOContext destination = IOManager.CreateIOContext("EXTRACT", fileout, FileAccess.Write, FileShare.Read, 0,
                FileMode.OpenOrCreate, EFileAttributes.None);

            if (destination.Result != 0) //Falha ao criar o handle
            {

                goto FREE;
            }

            //Abre e monta o disco para ter a tabela de setores
            if (destination.Lock() != IORet.RET_IO_OK)
            {
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

            uint zeroSectors = 0; //Quantidade de setores vazios antes de um usado, assim o programa pode ir lendo um setor
            //enquanto escreve os vazios

            //Seta o tamanho final do arquivo para nao faltar espaço enquanto extrai
            destination.Seek(isoSize);
            destination.SetEOF();

            //wbfsSectorsPerDisc é o total de setores de um DVD9
            int runto = isoSize == WBFSDevice.IsoDVD5Size ? wbfsSectorsPerDisc / 2 : wbfsSectorsPerDisc;

            //Volta para o começo do arquivo e zera o contador de progresso
            destination.Seek(0);
            progress.Reset(runto, wbfsSectorSize);

            i = 0;
            while (!progress.Cancel)
            {
                if (readBufferState == 0)
                {
                    context.ReadAsync(readBuffer, ((long)sectorPositionTable[i]) << wbfsSectorSize_s, wbfsSectorSize);
                    readBufferState = 1; //Lendo...
                }

                if ((readBufferState == 1) && (!context.Working)) //Fim da leitura
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
                        if (sectorUsageTable[i])
                        {
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
            while (context.Working) System.Threading.Thread.Sleep(1);
            while (destination.Working) System.Threading.Thread.Sleep(1); //Só ocorrerá se a operação for cancelada

        FREE: //Caso algo dê errado... venha direto para cá...

            context.FreeAsync();
            context.Unlock();

            destination.FreeAsync();
            destination.Close();

            if (progress.Cancel) //Apaga o arquivo de destino
            {
                r =  (int)ScrubberRet.RET_SCRUB_ABORTED;
                Log.SendMessage(this.name, (int)WBFSRet.RET_WBFS_ABORTED, null, LogMessageType.Error);
                IOManager.Delete(fileout);
            }

            return r;
        }

        public static int CreateScrub(String filename, String fileout, ProgressIndicator progress, PartitionSelection ps)
        {
            if (filename.Length == 0)
            {
                return (int)ScrubberRet.RET_SCRUB_INVALIDARG;
            }

            if (fileout.Length == 0)
            {
                return (int)ScrubberRet.RET_SCRUB_INVALIDARG;
            }

            if (progress == null)
            {
                return (int)ScrubberRet.RET_SCRUB_INVALIDARG;
            }

            int r = 0; //Para guardar o código de erro

            //Cria o contexto do arquivo de destino
            IIOContext destination = IOManager.CreateIOContext("SCRUB", fileout, FileAccess.Write, FileShare.Read, 0,
                FileMode.Create, EFileAttributes.None);

            //Tranca o contexto do dispositivo
            if (destination.Lock() != IORet.RET_IO_OK)
            {
                return (int)ScrubberRet.RET_SCRUB_FAIL;
            }

            destination.PrepareForAsync();

            //Cria o contexto do arquivo de origem
            IIOContext source = IOManager.CreateIOContext("SCRUB", filename, FileAccess.Read, FileShare.Read, 0,
                FileMode.Open, EFileAttributes.None);

            if (source.Result != 0) //Falha ao criar o handle
            {
                goto FREE;
            }

            //Abre e monta o disco para ter a tabela de setores
            if ((r = (int)source.Lock()) != (int)IORet.RET_IO_OK) { goto FREE; }
            source.PrepareForAsync();

            //Tabela de setores de wii
            Byte[] wiiUsageTable = new Byte[WBFSDevice.wiiSectorsPerDisc];

            //Setores Usados
            uint wiiused = WBFSDevice.wiiSectorsPerDisc;

            String name = "";
            String code = "";
            sbyte region = -1;

            WiiDisc disc = new WiiDisc(source, false);
            disc.Open();
            disc.BuildDisc(ps);

            name = disc.name;
            code = disc.code;
            disc.SectorUsageTable.CopyTo(wiiUsageTable, 0);
            wiiused = disc.UsedSectors;

            //Fecha o disco
            disc.Close();
            disc = null;

            //Lê a região
            Byte[] bregion = new byte[4];
            source.Read(bregion, WBFSDevice.IsoRegionPos, 4);
            region = (sbyte)System.Net.IPAddress.NetworkToHostOrder((int)BitConverter.ToUInt32(bregion, 0));
            bregion = null;

            //Cria a tabela de setores WBFS
            BitArray wbfsUsageTable = new BitArray((int)default_wbfsSectorsPerDisc_1000);
            
            uint wbfsused = 0;
            int i = 0;

            for (i = 0; i < default_wbfsSectorsPerDisc_1000; i++)
            {
                if (WBFSDevice.IsBlockUsed(wiiUsageTable, (uint)i, default_wiiSectorsPerWbfsSectors_1000))
                {
                    wbfsused++;
                    wbfsUsageTable[i] = true;
                }
            }

            Byte[] bWbfsUsageTable = new Byte[(wbfsUsageTable.Count - 1) / 8 + 1];
            wbfsUsageTable.CopyTo(bWbfsUsageTable, 0);

            //Seta o tamanho do destino e escreve o cabeçalho
            destination.Seek((wbfsused + 1) << default_wbfsSectorSize_s_1000);
            destination.SetEOF();
            destination.Seek(0);

            WriteScrubHeader1000(destination, name, code, region, source.Length, default_wbfsSectorSize_s_1000,
                wbfsused, bWbfsUsageTable, ps);

            //Cria os buffers para leitura/escrita assíncrona
            Byte[] buffer1 = new Byte[WBFSDevice.wiiSectorSize];
            Byte[] buffer2 = new Byte[WBFSDevice.wiiSectorSize];

            Byte[] readBuffer = buffer1;
            Byte[] writeBuffer = buffer2;

            byte readBufferState = 0; //0 - vazio, 1 - lendo, 2 - cheio
            byte writeBufferState = 0; //0 - vazio, 1 - escrevendo, 2 - cheio

            ushort wbfssector = 1; //Setor 0 é o cabeçalho
            int wiisector = 0;

            //Reseta o contador de progresso, seta o contexto na posição 0
            source.Seek(0);
            progress.Reset(wiiused, WBFSDevice.wiiSectorSize);

            i = 0;
            while (!progress.Cancel)
            {
                if (readBufferState == 0)
                {
                    source.ReadAsync(readBuffer, ((long)default_wbfsSectorSize_1000 * i) +
                        ((long)WBFSDevice.wiiSectorSize * wiisector), (int)WBFSDevice.wiiSectorSize);
                    readBufferState = 1; //Lendo...
                }

                if ((readBufferState == 1) && (!source.Working)) //Fim da leitura
                {
                    //Conserta a tabela de partição se necessário
                    if (((long)default_wbfsSectorSize_1000 * i) + ((long)WBFSDevice.wiiSectorSize * wiisector) == 0x10000)
                    {
                        WiiDisc.FixPartitionTable(PartitionSelection.OnlyGamePartition, readBuffer);
                    }

                    readBufferState = 2; //Pronto
                }

                if ((writeBufferState == 1) && (!destination.Working)) //Fim da escrita
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

                    destination.WriteAsync(writeBuffer, ((long)wbfssector * default_wbfsSectorSize_1000) +
                        ((long)wiisector * WBFSDevice.wiiSectorSize), (int)WBFSDevice.wiiSectorSize);

                    writeBufferState = 1; //Escrevendo...
                    readBufferState = 0; //Permite o buffering de mais um setor

                    if (++wiisector == default_wiiSectorsPerWbfsSectors_1000) //O setor wbfs atual acabou
                    {
                        wiisector = 0;
                        wbfssector++;

                        while (++i < default_wbfsSectorsPerDisc_1000) if (wbfsUsageTable[i]) break; //Procura proximo setor
                        if (i == default_wbfsSectorsPerDisc_1000) break; //Acabou o disco
                    }
                }

                //Impede 100% de uso de CPU
                //System.Threading.Thread.Sleep(1);
            }

            //Garante que todas as operações assíncronas terminem
            while (destination.Working) System.Threading.Thread.Sleep(1);
            while (source.Working) System.Threading.Thread.Sleep(1); //Só ocorrerá se a operação for cancelada

        FREE: //Caso algo dê errado... venha direto para cá...

            destination.FreeAsync();
            destination.Close(); //Fecha o contexto do destino

            source.FreeAsync();
            source.Close(); //Fecha o disco

            if (progress.Cancel) //Desaloca os blocos e o disco
            {
                IOManager.Delete(fileout);
            }

            //Limpa as matrizes
            wiiUsageTable = null;
            wbfsUsageTable = null;
            readBuffer = null;
            writeBuffer = null;
            buffer1 = null;
            buffer2 = null;

            return r;
        }

        public static int CreateScrub(Char drive, String fileout, ProgressIndicator progress, PartitionSelection ps)
        {
            if (fileout.Length == 0)
            {
                return (int)ScrubberRet.RET_SCRUB_INVALIDARG;
            }

            if (progress == null)
            {
                return (int)ScrubberRet.RET_SCRUB_INVALIDARG;
            }

            int r = 0; //Para guardar o código de erro

            //Cria o contexto do arquivo de destino
            IIOContext destination = IOManager.CreateIOContext("SCRUB", fileout, FileAccess.Write, FileShare.Read, 0,
                FileMode.Create, EFileAttributes.None);

            //Tranca o contexto do dispositivo
            if (destination.Lock() != IORet.RET_IO_OK)
            {
                return (int)ScrubberRet.RET_SCRUB_FAIL;
            }

            destination.PrepareForAsync();

            //Cria o contexto do arquivo de origem
            IIOContext source = new DVDRomReader(drive);

            if (source.Result != 0) //Falha ao criar o handle
            {
                goto FREE;
            }

            //Abre e monta o disco para ter a tabela de setores
            if ((r = (int)source.Lock()) != (int)IORet.RET_IO_OK) { goto FREE; }
            source.PrepareForAsync();

            //Tabela de setores de wii
            Byte[] wiiUsageTable = new Byte[WBFSDevice.wiiSectorsPerDisc];

            //Setores Usados
            uint wiiused = WBFSDevice.wiiSectorsPerDisc;

            String name = "";
            String code = "";
            sbyte region = -1;

            WiiDisc disc = new WiiDisc(source, false);
            disc.Open();
            disc.BuildDisc(ps);

            name = disc.name;
            code = disc.code;
            disc.SectorUsageTable.CopyTo(wiiUsageTable, 0);
            wiiused = disc.UsedSectors;

            //Fecha o disco
            disc.Close();
            disc = null;

            //Lê a região
            Byte[] bregion = new byte[4];
            source.Read(bregion, WBFSDevice.IsoRegionPos, 4);
            region = (sbyte)System.Net.IPAddress.NetworkToHostOrder((int)BitConverter.ToUInt32(bregion, 0));
            bregion = null;

            //Cria a tabela de setores WBFS
            BitArray wbfsUsageTable = new BitArray((int)default_wbfsSectorsPerDisc_1000);

            uint wbfsused = 0;
            int i = 0;

            for (i = 0; i < default_wbfsSectorsPerDisc_1000; i++)
            {
                if (WBFSDevice.IsBlockUsed(wiiUsageTable, (uint)i, default_wiiSectorsPerWbfsSectors_1000))
                {
                    wbfsused++;
                    wbfsUsageTable[i] = true;
                }
            }

            Byte[] bWbfsUsageTable = new Byte[(wbfsUsageTable.Count - 1) / 8 + 1];
            wbfsUsageTable.CopyTo(bWbfsUsageTable, 0);

            //Seta o tamanho do destino e escreve o cabeçalho
            destination.Seek((wbfsused + 1) << default_wbfsSectorSize_s_1000);
            destination.SetEOF();
            destination.Seek(0);

            WriteScrubHeader1000(destination, name, code, region, source.Length, default_wbfsSectorSize_s_1000,
                wbfsused, bWbfsUsageTable, ps);

            //Cria os buffers para leitura/escrita assíncrona
            Byte[] buffer1 = new Byte[WBFSDevice.wiiSectorSize];
            Byte[] buffer2 = new Byte[WBFSDevice.wiiSectorSize];

            Byte[] readBuffer = buffer1;
            Byte[] writeBuffer = buffer2;

            byte readBufferState = 0; //0 - vazio, 1 - lendo, 2 - cheio
            byte writeBufferState = 0; //0 - vazio, 1 - escrevendo, 2 - cheio

            ushort wbfssector = 1; //Setor 0 é o cabeçalho
            int wiisector = 0;

            //Reseta o contador de progresso, seta o contexto na posição 0
            source.Seek(0);
            progress.Reset(wiiused, WBFSDevice.wiiSectorSize);

            i = 0;
            while (!progress.Cancel)
            {
                if (readBufferState == 0)
                {
                    source.ReadAsync(readBuffer, ((long)default_wbfsSectorSize_1000 * i) + 
                        ((long)WBFSDevice.wiiSectorSize * wiisector), (int)WBFSDevice.wiiSectorSize);
                    readBufferState = 1; //Lendo...
                }

                if ((readBufferState == 1) && (!source.Working)) //Fim da leitura
                {
                    //Conserta a tabela de partição se necessário
                    if (((long)default_wbfsSectorSize_1000 * i) + ((long)WBFSDevice.wiiSectorSize * wiisector) == 0x10000)
                    {
                        WiiDisc.FixPartitionTable(PartitionSelection.OnlyGamePartition, readBuffer);
                    }

                    readBufferState = 2; //Pronto
                }

                if ((writeBufferState == 1) && (!destination.Working)) //Fim da escrita
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

                    destination.WriteAsync(writeBuffer, ((long)wbfssector * default_wbfsSectorSize_1000) +
                        ((long)wiisector * WBFSDevice.wiiSectorSize), (int)WBFSDevice.wiiSectorSize);

                    writeBufferState = 1; //Escrevendo...
                    readBufferState = 0; //Permite o buffering de mais um setor

                    if (++wiisector == default_wiiSectorsPerWbfsSectors_1000) //O setor wbfs atual acabou
                    {
                        wiisector = 0;
                        wbfssector++;

                        while (++i < default_wbfsSectorsPerDisc_1000) if (wbfsUsageTable[i]) break; //Procura proximo setor
                        if (i == default_wbfsSectorsPerDisc_1000) break; //Acabou o disco
                    }
                }

                //Impede 100% de uso de CPU
                //System.Threading.Thread.Sleep(1);
            }

            //Garante que todas as operações assíncronas terminem
            while (destination.Working) System.Threading.Thread.Sleep(1);
            while (source.Working) System.Threading.Thread.Sleep(1); //Só ocorrerá se a operação for cancelada

        FREE: //Caso algo dê errado... venha direto para cá...

            destination.FreeAsync();
            destination.Close(); //Fecha o contexto do destino

            source.FreeAsync();
            source.Close(); //Fecha o disco

            if (progress.Cancel) //Desaloca os blocos e o disco
            {
                IOManager.Delete(fileout);
            }

            //Limpa as matrizes
            wiiUsageTable = null;
            wbfsUsageTable = null;
            readBuffer = null;
            writeBuffer = null;
            buffer1 = null;
            buffer2 = null;

            return r;
        }

        public static int CreateScrub(IDisc disc, String fileout, ProgressIndicator progress)
        {
            int r = 0; //Para guardar o código de erro

            if (disc == null)
            {
                return (int)ScrubberRet.RET_SCRUB_INVALIDARG;
            }

            if (fileout.Length == 0)
            {
                return (int)ScrubberRet.RET_SCRUB_INVALIDARG;
            }

            if (progress == null)
            {
                return (int)ScrubberRet.RET_SCRUB_INVALIDARG;
            }

            //Cria o contexto do arquivo de destino
            IIOContext destination = IOManager.CreateIOContext("SCRUB", fileout, FileAccess.Write, FileShare.Read, 0,
                FileMode.Create, EFileAttributes.None);

            //Tranca o contexto do dispositivo
            if (destination.Lock() != IORet.RET_IO_OK)
            {
                return (int)ScrubberRet.RET_SCRUB_FAIL;
            }

            destination.PrepareForAsync();

            //Cria o contexto do disco
            IIOContext source = new DiscReader(disc);

            if (source.Result != 0) //Falha ao criar o handle
            {
                goto FREE;
            }

            //Abre e monta o disco para ter a tabela de setores
            if ((r = (int)source.Lock()) != (int)IORet.RET_IO_OK) { goto FREE; }
            source.PrepareForAsync();

            int i = 0;

            //Disco do dispositivo WBFS
            Disc wbfsdisc = (Disc)disc;

            Byte[] bWbfsUsageTable = new Byte[(wbfsdisc.device.wbfsSectorsPerDisc - 1) / 8 + 1];

            int j = 0;
            i = 0;
            while( i < wbfsdisc.device.wbfsSectorsPerDisc)
            {
                if (wbfsdisc.WLBATable[i] != 0) bWbfsUsageTable[j] += (byte)(1 << (i % 8));
                if ((++i % 8) == 0) j++;
            }

            //Seta o tamanho do destino e escreve o cabeçalho
            destination.Seek((wbfsdisc.usedwbfssectors + 1) << default_wbfsSectorSize_s_1000);
            destination.SetEOF();
            destination.Seek(0);

            WriteScrubHeader1000(destination, wbfsdisc.name, wbfsdisc.code, (sbyte)wbfsdisc.region, wbfsdisc.Size,
                wbfsdisc.device.wbfsSectorSize_s, wbfsdisc.usedwbfssectors, bWbfsUsageTable, 
                PartitionSelection.OnlyGamePartition);

            //Cria os buffers para leitura/escrita assíncrona
            Byte[] buffer1 = new Byte[wbfsdisc.device.wbfsSectorSize]; //Buffers um pouco maiores do que em AddDisc(...)
            Byte[] buffer2 = new Byte[wbfsdisc.device.wbfsSectorSize];

            Byte[] readBuffer = buffer1;
            Byte[] writeBuffer = buffer2;

            byte readBufferState = 0; //0 - vazio, 1 - lendo, 2 - cheio
            byte writeBufferState = 0; //0 - vazio, 1 - escrevendo, 2 - cheio

            ushort wbfssector = 0; //Indice na WLBA do disco que representa a posição de um setor WBFS alocado

            //Seta o tamanho final do arquivo para nao faltar espaço enquanto extrai
            destination.Seek((disc.UsedWBFSSectors + 1) << wbfsdisc.device.wbfsSectorSize_s);
            destination.SetEOF();

            //wbfsSectorsPerDisc é o total de setores de um DVD9
            int runto = disc.Size == WBFSDevice.IsoDVD5Size ? wbfsdisc.device.wbfsSectorsPerDisc / 2 : 
                wbfsdisc.device.wbfsSectorsPerDisc;

            //zera o contador de progresso e move o ponteiro do destino para o segundo setor wbfs do disco scrub
            destination.Seek(wbfsdisc.device.wbfsSectorSize);
            progress.Reset(wbfsdisc.usedwbfssectors, WBFSDevice.wiiSectorSize);

            i = 0;
            wbfssector = disc.WLBATable[i]; //Supondo que o setor 0 sempre será usado por causa da identificação do disco

            while (!progress.Cancel)
            {
                if (!source.CanWork) { progress.Cancel = true; break; }

                if (readBufferState == 0)
                {
                    source.ReadAsync(readBuffer, ((long)wbfsdisc.device.PartitionOffsetLBA * wbfsdisc.device.hdSectorSize) +
                        ((long)wbfsdisc.device.wbfsSectorSize * wbfssector), (int)wbfsdisc.device.wbfsSectorSize);
                    readBufferState = 1; //Lendo...
                }

                if ((readBufferState == 1) && (!source.Working)) //Fim da leitura
                {
                    readBufferState = 2; //Pronto
                }

                if ((writeBufferState == 1) && (!destination.Working)) //Fim da escrita
                {
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

                    destination.WriteAsync(writeBuffer, (int)wbfsdisc.device.wbfsSectorSize);

                    writeBufferState = 1; //Escrevendo...
                    readBufferState = 0; //Permite o buffering de mais um setor

                    while (++i < runto) //Procura proximo setor
                    {
                        if (disc.WLBATable[i] != 0)
                        {
                            wbfssector = disc.WLBATable[i];
                            break;
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
            while (source.Working) System.Threading.Thread.Sleep(1);
            while (destination.Working) System.Threading.Thread.Sleep(1); //Só ocorrerá se a operação for cancelada

        FREE: //Caso algo dê errado... venha direto para cá...

            source.FreeAsync();
            source.Unlock();

            destination.FreeAsync();
            destination.Close();

            if (progress.Cancel) //Apaga o arquivo de destino
            {
                r = (int)ScrubberRet.RET_SCRUB_ABORTED;
                IOManager.Delete(fileout);
            }

            return r;
        }

        static void WriteScrubHeader1000(IIOContext context, String name, String code, sbyte region, long size, 
            byte sectorsize_s, uint usedsectors, Byte[] usagetable, PartitionSelection ps)
        {
            context.Seek(0);

            context.Write("SYNCSCRUB", false);
            context.Write(current_version);
            context.Write(code, false);
            context.Write(name, false);
            for (int i = name.Length; i < WBFSDevice.IsoNameLen; i++)
                context.Write((byte)0);
            context.Write((byte)region);
            context.Write(size);
            context.Write((byte)sectorsize_s);
            context.Write(usedsectors);
            context.Write(usagetable, usagetable.Length);

            byte bPs;
            switch (ps)
            {
                default: //Por padrão só a partição do jogo vai no Scrub
                case PartitionSelection.OnlyGamePartition: bPs = 0; break;
                case PartitionSelection.RemoveUpdatePartition: bPs = 1; break;
                case PartitionSelection.AllPartitions: bPs = 2; break;
            }

            context.Write(bPs);

            Byte[] padding = new Byte[(1 << sectorsize_s) - (int)context.Position];
            Array.Clear(padding, 0, padding.Length);
            context.Write(padding, padding.Length);
            padding = null;
        }

        //----------------- Calcula o tamanho que uma iso irá ocupar como scrub
        public static long CalculateDiscSize(String filename)
        {
            if (filename.Length == 0)
            {
                return (int)ScrubberRet.RET_SCRUB_INVALIDARG;
            }

            long result = 0; //Para guardar o código de erro ou o tamanho do scrub

            //Cria o contexto do arquivo de origem
            IIOContext source = IOManager.CreateIOContext("SCRUB", filename, FileAccess.Read, FileShare.Read, 0,
                FileMode.Open, EFileAttributes.None);

            if (source.Result != 0) //Falha ao criar o handle
            {
                goto END;
            }

            //Abre e monta o disco para ter a tabela de setores
            if ((result = (int)source.Lock()) != (int)IORet.RET_IO_OK) { goto END; }

            //Tabela de setores de wii
            Byte[] wiiUsageTable = new Byte[WBFSDevice.wiiSectorsPerDisc];

            WiiDisc disc = new WiiDisc(source, false);
            disc.Open();
            disc.BuildDisc(PartitionSelection.OnlyGamePartition);

            disc.SectorUsageTable.CopyTo(wiiUsageTable, 0);

            //Fecha o disco
            disc.Close();
            disc = null;

            uint wbfsused = 1; //O scrub tem 1 setor a mais para alinhamento e cabeçalho
            int i = 0;

            for (i = 0; i < default_wbfsSectorsPerDisc_1000; i++)
            {
                if (WBFSDevice.IsBlockUsed(wiiUsageTable, (uint)i, default_wiiSectorsPerWbfsSectors_1000))
                {
                    wbfsused++;
                }
            }

            result = (long)wbfsused * default_wbfsSectorSize_1000;

        END:

            if (source != null)
            {
                source.Close();
                source = null;
            }

            return result;
        }

        //----------------- Calcula o tamanho que uma iso irá ocupar como scrub
        public static long CalculateDiscSize(Byte[] wiiSectorUsageTable)
        {
            if ((wiiSectorUsageTable == null) || (wiiSectorUsageTable.Length != WBFSDevice.wiiSectorsPerDisc))
            {
                Log.SendMessage("Scrub", (int)WBFSRet.RET_WBFS_INVALIDARG, "wiiSectorUsageTable", LogMessageType.Error);
                return (int)WBFSRet.RET_WBFS_INVALIDARG;
            }

            //Conta os setores usados
            uint wbfsused = 0;
            for (int i = 0; i < default_wbfsSectorsPerDisc_1000; i++)
            {
                if (WBFSDevice.IsBlockUsed(wiiSectorUsageTable, (uint)i, default_wiiSectorsPerWbfsSectors_1000))
                {
                    wbfsused++;
                }
            }

            return (long)wbfsused * default_wbfsSectorSize_1000;
        }

        //------------------ Contexto IO

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

            int sectorIndex = (int)(discPosition / wbfsSectorSize);
            scrubOffset = (uint)(discPosition % wbfsSectorSize);
 
            scrubPosition = (sectorPositionTable[sectorIndex] << wbfsSectorSize_s) + scrubOffset;

            return context.Seek(scrubPosition);
        }

        public IORet Seek(long pos)
        {
            discPosition = pos;

            int sectorIndex = (int)(discPosition / wbfsSectorSize);
            scrubOffset = (uint)(discPosition % wbfsSectorSize);

            scrubPosition = (sectorPositionTable[sectorIndex] << wbfsSectorSize_s) + scrubOffset;

            return context.Seek(scrubPosition);
        }

        public IORet SetEOF()
        {
            return IORet.RET_IO_ACCESS_DENIED; //Não seta o fim de um drive físico
        }

        public IORet Close()
        {
            sectorUsageTable = null;
            sectorPositionTable = null;

            code = "";
            name = "";
            region = DiscRegion.Unknown;

            usedWbfsSectors = 0;
            scrubSize = 0;
            isoSize = 0;

            wbfsSectorSize = 0;
            wbfsSectorSize_s = 0;
            wbfsSectorsPerDisc = 0;
            wiiSectorsPerWbfsSectors = 0;
            wbfsUsageTableSize = 0;

            discPosition = 0;
            scrubPosition = 0;
            scrubOffset = 0;

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
                int toRead = (int)Math.Min(count, wbfsSectorSize - scrubOffset);

                if (scrubPosition == 0)
                {
                    Array.Clear(buffer, read, toRead);
                }
                else
                {
                    Byte[] readBuffer = new byte[toRead];
                    if ((ret = context.Read(readBuffer, toRead)) != IORet.RET_IO_OK) { return ret; }
                    Array.Copy(readBuffer, 0, buffer, read, toRead);
                }

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

        //------------------ Verificação

        //------------------ Verifica se o arquivo é um disco scrub
        public static int IsScrubDisc(String file)
        {
            int r = 0;

            IIOContext context = IOManager.CreateIOContext("ISSCRUB", file, System.IO.FileAccess.Read, System.IO.FileShare.Read,
                0, System.IO.FileMode.Open, EFileAttributes.None);
            if (context.Result != 0)
            {
                return context.Result;
            }
            if ((r = (int)context.Lock()) != (int)IORet.RET_IO_OK)
            {
                context.Close();
                return r;
            }

            Byte[] Magic = new Byte[9];
            context.Read(Magic, 9);

            if (!CompareMagic(Magic))
            {
                //Loga erro
                Log.SendMessage(System.IO.Path.GetFileName(file), (int)ScrubberRet.RET_SCRUB_BADMAGIC, Magic, 
                    LogMessageType.Error);
                r = (int)ScrubberRet.RET_SCRUB_BADMAGIC;
            }

            context.Close();
            return r;
        }

        //------------------ Verifica e extrai informações do disco scrub, por padrão 'used' é o número de setores de wii
        //utilizados
        public static int GetScrubbedInfo(String file, out string code, out string name, out int region, out long fullsize, 
            out long used)
        {
            int r = 0;

            code = "";
            name = "";
            region = -1;
            used = 0;
            fullsize = 0;

            IIOContext context = IOManager.CreateIOContext("ISSCRUB", file, System.IO.FileAccess.Read, System.IO.FileShare.Read,
                0, System.IO.FileMode.Open, EFileAttributes.None);
            if (context.Result != 0)
            {
                //Loga erro

                return context.Result;
            }
            if ((r = (int)context.Lock()) != (int)IORet.RET_IO_OK)
            {
                //Loga erro

                context.Close();
                return r;
            }

            Byte[] Magic = new Byte[9];
            context.Read(Magic, 9);

            if (!CompareMagic(Magic))
            {
                //Loga erro
                context.Close();
                Log.SendMessage(System.IO.Path.GetFileName(file), (int)ScrubberRet.RET_SCRUB_BADMAGIC, Magic, 
                    LogMessageType.Error);
                return (int)ScrubberRet.RET_SCRUB_BADMAGIC;
            }

            UInt16 version = context.ReadUInt16();
            if (version != current_version)
            {
                context.Close();
                Log.SendMessage(System.IO.Path.GetFileName(file), (int)ScrubberRet.RET_SCRUB_INCORRECTVERSION, version, 
                    LogMessageType.Error);
                return (int)ScrubberRet.RET_SCRUB_INCORRECTVERSION;
            }

            code = context.ReadString(false, 6);
            name = context.ReadString(false, 32);
            context.Seek(49);
            region = (sbyte)context.ReadByte();
            fullsize = (long)context.ReadUInt64();

            byte wbfssecsize_s = context.ReadByte();
            uint usedwbfssectors = context.ReadUInt32();

            used = (usedwbfssectors * (1 << wbfssecsize_s)) / WBFSDevice.wiiSectorSize;

            context.Close();
            return (int)ScrubberRet.RET_SCRUB_OK;
        }

        //------------------ Compara o código de verificação
        static bool CompareMagic(Byte[] magic)
        {
            return (magic[0] == (byte)'S') && (magic[1] == (byte)'Y') && (magic[2] == (byte)'N') && 
                   (magic[3] == (byte)'C') && (magic[4] == (byte)'S') && (magic[5] == (byte)'C') &&
                   (magic[6] == (byte)'R') && (magic[7] == (byte)'U') && (magic[8] == (byte)'B');
        }

        //------------------ Muda o nome e código da ISO dentro do scrub
        public static int SetScrubInfo(string file, string code, string name)
        {
            int r = 0;

            IIOContext context = IOManager.CreateIOContext("SCRUBDISC", file, System.IO.FileAccess.Read, FileShare.Read,
                0, FileMode.Open, EFileAttributes.None);

            if (context.Result != 0)
            {
                //Loga erro

                return context.Result;
            }
            if ((r = (int)context.Lock()) != (int)IORet.RET_IO_OK)
            {
                //Loga erro

                context.Close();
                return r;
            }

            //Verifica a consistência do cabeçalho

            if (context.Length < base_header_size_1000)
            {
                //Loga o erro
                return (int)ScrubberRet.RET_SCRUB_INCONSISTENTDATA;
            }

            Byte[] Magic = new Byte[9];
            context.Read(Magic, 9);

            if (!CompareMagic(Magic))
            {
                //Loga erro
                context.Close();
                Log.SendMessage(System.IO.Path.GetFileName(file), (int)ScrubberRet.RET_SCRUB_BADMAGIC, Magic,
                    LogMessageType.Error);
                return (int)ScrubberRet.RET_SCRUB_BADMAGIC;
            }

            UInt16 version = context.ReadUInt16();
            if (version != current_version)
            {
                context.Close();
                Log.SendMessage(System.IO.Path.GetFileName(file), (int)ScrubberRet.RET_SCRUB_INCORRECTVERSION, version,
                    LogMessageType.Error);
                return (int)ScrubberRet.RET_SCRUB_INCORRECTVERSION;
            }

            context.Seek(0x39);
            byte wbfsSectorSize_s = context.ReadByte();
            int wbfsSectorSize = 1 << wbfsSectorSize_s;

            if (code.Length != 0)
            {
                if (code.Length != WBFSDevice.IsoCodeLen)
                {
                    if (code.Length < WBFSDevice.IsoCodeLen) while (code.Length < WBFSDevice.IsoCodeLen) code += '0';
                    else code = code.Substring(0, WBFSDevice.IsoCodeLen); //Eu preciso fazer isso para o GetDiscByCode()
                }

                context.Seek(0x0A);
                context.Write(code, false);

                context.Seek(wbfsSectorSize + WBFSDevice.IsoCodePos);
                context.Write(code, false);
            }

            if (name.Length != 0)
            {
                if (name.Length > WBFSDevice.IsoNameLen)
                {
                    name = name.Substring(0, WBFSDevice.IsoNameLen);
                }

                context.Seek(0x10);
                context.Write(name, false);
                for (int i = name.Length; i < WBFSDevice.IsoNameLen; i++) context.Write((byte)0);

                context.Seek(wbfsSectorSize + WBFSDevice.IsoNamePos);
                context.Write(name, false);
                for (int i = name.Length; i < WBFSDevice.IsoNameLen; i++) context.Write((byte)0);
            }

            context.Close();

            return (int)ScrubberRet.RET_SCRUB_OK;
        }
    }
}
