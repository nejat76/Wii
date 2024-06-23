//-------------------------------------
// WBFSSync - WBFSSync.exe
//
// Copyright 2009 Caian (ÔmΣga Frøst) <frost.omega@hotmail.com>
//
// WBFSSync is Licensed under the terms of the Microsoft Reciprocal License (Ms-RL)
//
// IOManager.cs:
//
// Implementa gerenciador de arquivos, escrita, leitura, buffers e Mutex
//
// Nota: A classe IOManager permite apenas uma instância de um arquivo aberta por vez,
// se o mesmo aquivo for requisitado 2 ou mais vezes o primeiro handle será retornado MESMO SE
// dwDesiredAccess, dwShareMode, lpSecurityAttributes, dwCreationDisposition ou dwFlagsAndAttributes
// FOREM DIFERENTES! Portanto se você sabe que o arquivo pode ser aberto muitas vezes pelo programa
// seja consistente quanto a esses parâmetros.
//
// Arquivos abertos com a flag NoBuffering DEVEM ser lidos e escritos com buffers de tamanho múltiplo
// ao tamanho do setor do disco
//
//-------------------------------------

using System;
using System.Collections.Generic;
//using System.Linq;
using System.Text;
using System.Runtime.InteropServices;
using System.IO;

namespace WBFSSync
{
    public enum IORet : int
    {
        RET_IO_OK = 0,
        RET_IO_FAIL = -1001, //Falha não listada abaixo

        RET_IO_ALREADY_WORKING = -1002, //Foi chamada alguma rotina IO enquanto o IIOContext executa uma operação assíncrona

        RET_IO_INVALID_HANDLE = -1003, //O Handle é inválido
        RET_IO_HANDLE_CLOSED = -1004, //O Handle foi fechado
        RET_IO_INVALIDARG = -1005,    //Algum argumento é inválido, pra casos de Read() foi passado um Array inválido
        RET_IO_CONTEXT_CLOSED = -1006, //O contexto foi fechado pelo IIOContext.Close() ou pelo IOManager
        RET_IO_LOCKED = -1007,         //Outro IIOContext requisitou acesso exclusivo ao handle
        RET_IO_ALREADY_LOCKED = -1008, //O IIOContext já trancou o handle
        RET_IO_UNLOCKED = -1009,       //O IIOContext não requisitou acesso usando Lock()
        RET_IO_ACCESS_DENIED = -1010,  //Acesso negado, o usuário não tem privilégios para acessar o arquivo

        RET_IO_NATIVEERROR = -1011, //Erro retornado por uma rotina nativa

        RET_IO_ERROR_SEEKING = -1012, //Um erro ocorreu durante uma operação de Seek
        RET_IO_ERROR_READING = -1013, //Um erro ocorreu durante uma operação de Leitura
        RET_IO_ERROR_WRITING = -1014, //Um erro ocorreu durante uma operação de Escrita
        RET_IO_ERROR_SET_EOF = -1015, //Um erro ocorreu durante uma operação de fixar o fim do arquivo
        RET_IO_ERROR_CLOSING = -1016, //Um erro ocorreu durante uma operação de fechar o handle

        RET_IO_ERROR_HANDLE_OPEN = -1017, //O handle do arquivo a ser deletado está aberto
    }

    //unmanaged size = 32
    public struct PARTITION_INFORMATION
    {
        public long StartingOffset;
        public long PartitionLength;
        public uint HiddenSectors;
        public uint PartitionNumber;
        public byte PartitionType;
        public byte BootIndicator;
        public byte RecognizedPartition;
        public byte RewritePartition;

        public PARTITION_INFORMATION(long so, long pl, uint hs, uint pn, byte pt, byte bi, byte rp, byte rwp)
        {
            StartingOffset = so;
            PartitionLength = pl;
            HiddenSectors = hs;
            PartitionNumber = pn;
            PartitionType = pt;
            BootIndicator = bi;
            RecognizedPartition = rp;
            RewritePartition = rwp;
        }
    }

    //unmanaged size = 24
    public struct DISK_GEOMETRY
    {
        public long Cylinders;
        public MEDIA_TYPE MediaType;
        public uint TracksPerCylinder;
        public uint SectorsPerTrack;
        public uint BytesPerSector;

        public DISK_GEOMETRY(long c, MEDIA_TYPE t, uint tpc, uint spt, uint bps)
        {
            Cylinders = c;
            MediaType = t;
            TracksPerCylinder = tpc;
            SectorsPerTrack = spt;
            BytesPerSector = bps;
        }
    }

    public enum MEDIA_TYPE
    {
        Unknown,                // Format is unknown
        F5_1Pt2_512,            // 5.25", 1.2MB,  512 bytes/sector
        F3_1Pt44_512,           // 3.5",  1.44MB, 512 bytes/sector
        F3_2Pt88_512,           // 3.5",  2.88MB, 512 bytes/sector
        F3_20Pt8_512,           // 3.5",  20.8MB, 512 bytes/sector
        F3_720_512,             // 3.5",  720KB,  512 bytes/sector
        F5_360_512,             // 5.25", 360KB,  512 bytes/sector
        F5_320_512,             // 5.25", 320KB,  512 bytes/sector
        F5_320_1024,            // 5.25", 320KB,  1024 bytes/sector
        F5_180_512,             // 5.25", 180KB,  512 bytes/sector
        F5_160_512,             // 5.25", 160KB,  512 bytes/sector
        RemovableMedia,         // Removable media other than floppy
        FixedMedia,             // Fixed hard disk media
        F3_120M_512,            // 3.5", 120M Floppy
        F3_640_512,             // 3.5" ,  640KB,  512 bytes/sector
        F5_640_512,             // 5.25",  640KB,  512 bytes/sector
        F5_720_512,             // 5.25",  720KB,  512 bytes/sector
        F3_1Pt2_512,            // 3.5" ,  1.2Mb,  512 bytes/sector
        F3_1Pt23_1024,          // 3.5" ,  1.23Mb, 1024 bytes/sector
        F5_1Pt23_1024,          // 5.25",  1.23MB, 1024 bytes/sector
        F3_128Mb_512,           // 3.5" MO 128Mb   512 bytes/sector
        F3_230Mb_512,           // 3.5" MO 230Mb   512 bytes/sector
        F8_256_128,             // 8",     256KB,  128 bytes/sector
        F3_200Mb_512,           // 3.5",   200M Floppy (HiFD)
        F3_240M_512,            // 3.5",   240Mb Floppy (HiFD)
        F3_32M_512              // 3.5",   32Mb Floppy
    }

    [Flags]
    public enum EFileAttributes : uint
    {
        None = 0,
        Readonly = 0x00000001,
        Hidden = 0x00000002,
        System = 0x00000004,
        Directory = 0x00000010,
        Archive = 0x00000020,
        Device = 0x00000040,
        Normal = 0x00000080,
        Temporary = 0x00000100,
        SparseFile = 0x00000200,
        ReparsePoint = 0x00000400,
        Compressed = 0x00000800,
        Offline = 0x00001000,
        NotContentIndexed = 0x00002000,
        Encrypted = 0x00004000,
        Write_Through = 0x80000000,
        Overlapped = 0x40000000,
        NoBuffering = 0x20000000,
        RandomAccess = 0x10000000,
        SequentialScan = 0x08000000,
        DeleteOnClose = 0x04000000,
        BackupSemantics = 0x02000000,
        PosixSemantics = 0x01000000,
        OpenReparsePoint = 0x00200000,
        OpenNoRecall = 0x00100000,
        FirstPipeInstance = 0x00080000
    }

    //Códigos de erro mais usados
    public enum WinError : int
    {
        ERROR_SUCCESS = 0,
        ERROR_INVALID_FUNCTION = 1,
        ERROR_FILE_NOT_FOUND = 2,
        ERROR_ACCESS_DENIED = 5,
        ERROR_INVALID_HANDLE = 6,
        ERROR_NOT_ENOUGH_MEMORY = 8,
        ERROR_WRITE_PROTECT = 19,
     
    }

    public static class IOManager
    {
        //---------------------------- Estruturas internas úteis

        //---------------------------- Constantes

        //Para IOCTL_DISK_GET_DRIVE_GEOMETRY e IOCTL_DISK_GET_PARTITION_INFO
        internal const int FILE_DEVICE_DISK = 0x00000007;
        internal const int IOCTL_DISK_BASE = FILE_DEVICE_DISK;
        internal const int METHOD_BUFFERED = 0;
        internal const int FILE_ANY_ACCESS = 0;
        internal const int FILE_READ_ACCESS = 0x0001; // file & pipe

        //Para DeviceIOCOntrol
        internal const int IOCTL_DISK_GET_DRIVE_GEOMETRY = (((IOCTL_DISK_BASE) << 16) | ((FILE_ANY_ACCESS) << 14) | ((0x0000) << 2) | (METHOD_BUFFERED));
        internal const int IOCTL_DISK_GET_PARTITION_INFO = (((IOCTL_DISK_BASE) << 16) | ((FILE_READ_ACCESS) << 14) | ((0x0001) << 2) | (METHOD_BUFFERED));

        //---------------------------- Variáveis

        static List<Handle> Handles = new List<Handle>();
        static List<IOContext> Contexts = new List<IOContext>();

        //---------------------------- Rotinas

        //--------------- Cria um Contexto IO para o arquivo
        public static IIOContext CreateIOContext(string ContextName, string FileName, FileAccess DesiredAccess, 
            FileShare ShareMode, uint SecurityAttributes, FileMode CreationDisposition, EFileAttributes FlagsAndAttributes)
        {
            int h = CreateFile(FileName, DesiredAccess, ShareMode, SecurityAttributes, CreationDisposition, FlagsAndAttributes);
            
            IOContext context = new IOContext();
            context.name = ContextName;

            if (h < 0)
            {
                context.handle = new Handle(); //inválido por padrão
                context.result = -h; //converte o erro de volta para o padrao
            }
            else
            {
                Handle handle = GetHandle(h);
                context.handle = handle;
                context.result = 0;
                Contexts.Add(context);
            }
            return context;
        }

        //--------------- Fecha um Contexto IO
        internal static void ReleaseContext(IOContext context, bool force)
        {
            Contexts.Remove(context);

            if (force)
                ForceCloseHandle(context.handle);
            else
                CloseHandle(context.handle);
        }

        //--------------- Cria/Abre um arquivo ou disco, se houver erro retorna o valor negativo de um erro nativo
        internal static int CreateFile(string FileName, FileAccess DesiredAccess, FileShare ShareMode, 
            uint SecurityAttributes, FileMode CreationDisposition, EFileAttributes FlagsAndAttributes)
        {
            Handle handle = GetHandle(FileName);
            if (handle != null)
            {
                handle.counter++;
                return handle.handle;
            }

            int h = CreateFile(FileName, DesiredAccess, ShareMode, SecurityAttributes, CreationDisposition, 
                FlagsAndAttributes, 0);

            if (h == -1)
            {
                int error = Marshal.GetLastWin32Error();
                string name = Path.GetFileName(FileName);
                if (name.Length == 0) name = FileName;

                Log.SendMessage(name, (int)IORet.RET_IO_NATIVEERROR, error, LogMessageType.Error);
                return -error;
            }

            handle = new Handle();
            handle.name = FileName;
            handle.handle = h;
            handle.counter++; //Primeira instância do handle
            handle.position = 0;

            int r = SetFilePointerEx(h, 0, ref handle.size, SeekOrigin.End);
            r = Marshal.GetLastWin32Error();

            long dummy = 0;
            SetFilePointerEx(h, 0, ref dummy, SeekOrigin.Begin);

            Handles.Add(handle);

            return handle.handle;
        }

        //--------------- Fecha o Handle e o remove da lista se seu contador for 0
        internal static int CloseHandle(Handle handle)
        {
            handle.counter--;
            
            if (handle.counter == 0)
            {
                int r = CloseHandle(handle.handle);
                
                handle.name = "";
                handle.position = 0;
                handle.locked = false;
                handle.handle = -1;
                handle.size = 0;

                Handles.Remove(handle);

                return r;
            }

            return 0;
        }

        //--------------- Força o fechamento do Handle, independente do contador de referencia
        internal static int ForceCloseHandle(Handle handle)
        {
            CloseHandle(handle);

            for (int i = Contexts.Count - 1; i >= 0; i--)
            {
                if (Contexts[i].handle == handle)
                {
                    Contexts[i].Close(); //Não force o fechamento
                }
            }

            if (Handles.Contains(handle)) //Não deveria acontecer
            {
                int r = CloseHandle(handle.handle);

                handle.name = "";
                handle.position = 0;
                handle.locked = false;
                handle.handle = -1;
                handle.size = 0;

                Handles.Remove(handle);

                return r;
            }

            return 0;
        }

        //--------------- Cria um handle falso representando um arquivo para impedir se uso no programa
        public static void CreateFakeHandle(String file)
        {
            if (GetHandle(file) == null)
            {
                Handle h = new Handle();
                h.name = file;
                h.handle = -1;
                h.locked = true;
                h.counter = 1;

                Handles.Add(h);
            }
        }

        //--------------- Libera o handle falso
        public static void CloseFakeHandle(String file)
        {
            Handle h = GetHandle(file);
            if (h != null) Handles.Remove(h);
        }

        //--------------- Verifica se o handle ja existe
        internal static Handle GetHandle(int handle)
        {
            for (int i = 0; i < Handles.Count; i++)
            {
                if (Handles[i].handle == handle)
                    return Handles[i];
            }

            return null;
        }

        //--------------- verifica se o arquivo está aberto
        internal static Handle GetHandle(string name)
        {
            name = name.ToLower();
            for (int i = 0; i < Handles.Count; i++)
            {
                if (String.Compare(Handles[i].name, name, true) == 0)
                    return Handles[i];
            }

            return null;
        }

        //--------------- verifica se o handle ja existe
        public static Boolean ContainsHandle(int handle)
        {
            for (int i = 0; i < Handles.Count; i++)
            {
                if (Handles[i].handle == handle)
                    return true;
            }

            return false;
        }

        //--------------- verifica se o arquivo está aberto
        public static Boolean ContainsHandle(string name)
        {
            for (int i = 0; i < Handles.Count; i++)
            {
                if (String.Compare(Handles[i].name, name, true) == 0)
                    return true;
            }

            return false;
        }

        //--------------- Conta os contextos cujo nome comece por 'Name'
        public static int CountContexts(String Name)
        {
            int r = 0;
            for (int i = 0; i < Contexts.Count; i++)
            {
                if (Contexts[i].name.StartsWith(Name, StringComparison.InvariantCultureIgnoreCase)) 
                    r++;
            }

            return r;
        }

        //--------------- Fecha todos os handles
        public static void Dispose()
        {
            //Fecha os contextos abertos
            for (int i = 0; i < Contexts.Count; i++)
            {
                ReleaseContext(Contexts[i], false);
            }

            //Fecha quaisquer handles que ainda não tenham sido fechados
            while(Handles.Count > 0)
            {
                int r = CloseHandle(Handles[0].handle);

                Handles[0].name = "";
                Handles[0].position = 0;
                Handles[0].locked = false;
                Handles[0].handle = -1;
                Handles[0].size = 0;
                Handles[0].counter = 0;

                Handles.RemoveAt(0);
            }
        }

        //--------------- Apaga um arquivo
        public static int Delete(String file)
        {
            Handle h = GetHandle(file);
            if (h != null)
            {
                Log.SendMessage(Path.GetFileName(file), (int)IORet.RET_IO_ERROR_HANDLE_OPEN, null, LogMessageType.Error);
                return (int)IORet.RET_IO_ERROR_HANDLE_OPEN;
            }

            if (DeleteFile(file) == 0)
            {
                int error = Marshal.GetLastWin32Error();
                Log.SendMessage(Path.GetFileName(file), (int)IORet.RET_IO_NATIVEERROR, error, LogMessageType.Error);
                return error;
            }

            return (int)IORet.RET_IO_OK;
        }

        //--------------- Move um arquivo
        public static int Move(String source, String dest)
        {
            Handle h = GetHandle(source);
            if (h != null)
            {
                Log.SendMessage(Path.GetFileName(source), (int)IORet.RET_IO_ERROR_HANDLE_OPEN, null, LogMessageType.Error);
                return (int)IORet.RET_IO_ERROR_HANDLE_OPEN;
            }

            h = GetHandle(dest);
            if (h != null)
            {
                Log.SendMessage(Path.GetFileName(dest), (int)IORet.RET_IO_ERROR_HANDLE_OPEN, null, LogMessageType.Error);
                return (int)IORet.RET_IO_ERROR_HANDLE_OPEN;
            }

            if (MoveFile(source, dest) == 0)
            {
                int error = Marshal.GetLastWin32Error();
                Log.SendMessage("Move File", (int)IORet.RET_IO_NATIVEERROR, error, LogMessageType.Error);
                return error;
            }

            return (int)IORet.RET_IO_OK;
        }

        //--------------- Move um arquivo, renomeando para dest(2, 3, 4, ..., n).ext e retorna o caminho do arquivo
        public static string Move2(String source, String dest)
        {
            int loopmax = 100;
            //

            Handle h = GetHandle(source); //Origem ocupada, não tem o que fazer.
            if (h != null)
            {
                Log.SendMessage(Path.GetFileName(source), (int)IORet.RET_IO_ERROR_HANDLE_OPEN, null, LogMessageType.Error);
                return "";
            }

            int k = 0;

            goto LOOP;

        RENAME:

            if (k == 0) k = 2;
            else k++;

            string p = Path.GetDirectoryName(source);
            string n = Path.GetFileNameWithoutExtension(source);
            string e = Path.GetExtension(source);
            dest = String.Format("{0}{1}({2}){3}", p, n, k, e);

        LOOP:

            h = GetHandle(dest);
            if (h != null)
            {
                if (--loopmax <= 0)
                {
                    Log.SendMessage(Path.GetFileName(dest), (int)IORet.RET_IO_ERROR_HANDLE_OPEN, null, LogMessageType.Error);
                    return "";
                }
                else
                {
                    goto RENAME;
                }
            }

            if (MoveFile(source, dest) == 0)
            {
                if (--loopmax <= 0)
                {
                    Log.SendMessage(Path.GetFileName(dest), (int)IORet.RET_IO_ERROR_HANDLE_OPEN, null, LogMessageType.Error);
                    return "";
                }
                else
                {
                    int error = Marshal.GetLastWin32Error();
                    Log.SendMessage("Move File", (int)IORet.RET_IO_NATIVEERROR, error, LogMessageType.Error);
                    return "";
                }
            }

            return dest;
        }

        //--------------- Nativas

        [DllImport("kernel32.dll", CharSet = CharSet.Auto, SetLastError = true)]
        private static extern int CreateFile(string lpFileName, FileAccess dwDesiredAccess, FileShare dwShareMode, 
            uint lpSecurityAttributes, FileMode dwCreationDisposition, EFileAttributes dwFlagsAndAttributes,  
            uint hTemplateFile);

        [DllImport("kernel32.dll")]
        static extern int MoveFile(string lpExistingFileName, string lpNewFileName);

        [DllImport("kernel32.dll", CharSet = CharSet.Auto, SetLastError = true)]
        private static extern int CloseHandle(int handle);

        [DllImport("kernel32.dll", CharSet = CharSet.Auto, SetLastError = true)]
        private static extern int DeleteFile(string lpFileName);

        [DllImport("kernel32.dll", CharSet = CharSet.Auto, SetLastError = true)]
        private static extern int SetFilePointerEx(int handle, long offset, ref long newPtr, SeekOrigin seekmode);
    }
}
