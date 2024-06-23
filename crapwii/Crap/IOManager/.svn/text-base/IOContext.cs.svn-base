//-------------------------------------
// WBFSSync - WBFSSync.exe
//
// Copyright 2009 Caian (ÔmΣga Frøst) <frost.omega@hotmail.com>
//
// WBFSSync is Licensed under the terms of the Microsoft Reciprocal License (Ms-RL)
//
// IOContext.cs:
//
// Implementação da interface que garante seek / leitura / escrita seguros de um handle
//
// Nota: Informações adicionais em IIOContext.cs
//
//-------------------------------------

using System;
using System.Collections.Generic;
//using System.Linq;
using System.Text;
using System.Runtime.InteropServices;
using System.IO;
using System.Threading;

namespace WBFSSync
{
    internal class IOContext : IIOContext, IDisposable
    {
        public String name = "";
        public Handle handle = null;
        public long position = 0;
        public int result = 0;
        public bool working = false;
        public bool lockedByMe = false;
        public bool closed = false;

        THREADINFO threadinfo = new THREADINFO();
        Thread asyncThread = null;

        #region IIOContext Members

        public string   Name { get { return name; } }
        public string Path { get { return handle.name; } }
        public int      Handle { get { return handle.handle; } }
        public long     Position { get { return position; } }
        public long     Length { get { return handle.size; } }
        public int      Result { get { return result; } }
        public bool     Working { get { return working; } }
        public bool     Closed { get { return closed; } }
        public bool     CanWork { get { return !closed && !handle.closed && handle.valid; } }
        public bool     Locked { get { return lockedByMe; } }
        private bool    CantWorkInLock { get { return handle.locked && !lockedByMe; } }

        //--------------- Seta o ponteiro do dispositivo para uma determinada posição
        public IORet Seek(long pos, SeekOrigin origin)
        {
            if (!lockedByMe)
            {
                Log.SendMessage(name, (int)IORet.RET_IO_UNLOCKED, null, LogMessageType.Error);
                return IORet.RET_IO_UNLOCKED;
            }

            if (origin == SeekOrigin.Begin)
            {
                position = pos;
            }
            else if (origin == SeekOrigin.End)
            {
                position = handle.size + pos;
            }
            else if (origin == SeekOrigin.Current)
            {
                position += pos;
            }

            if (handle.position != position)
            {
                if (SetFilePointerEx(handle.handle, position, ref handle.position, SeekOrigin.Begin) == 0)
                {
                    result = Marshal.GetLastWin32Error();
                    Log.SendMessage(name, (int)IORet.RET_IO_ERROR_SEEKING, result, LogMessageType.Error);
                    return IORet.RET_IO_ERROR_SEEKING;
                }
            }

            result = 0;
            return IORet.RET_IO_OK;
        }

        //--------------- Seta o ponteiro do dispositivo para uma determinada posição
        public IORet Seek(long pos)
        {
            if (!lockedByMe)
            {
                Log.SendMessage(name, (int)IORet.RET_IO_UNLOCKED, null, LogMessageType.Error);
                return IORet.RET_IO_UNLOCKED;
            }

            position = pos;
            if (handle.position != position)
            {
                if (SetFilePointerEx(handle.handle, position, ref handle.position, SeekOrigin.Begin) == 0)
                {
                    result = Marshal.GetLastWin32Error();
                    Log.SendMessage(name, (int)IORet.RET_IO_ERROR_SEEKING, result, LogMessageType.Error);
                    return IORet.RET_IO_ERROR_SEEKING;
                }
            }

            result = 0;
            return IORet.RET_IO_OK;
        }

        //--------------- Seta a posição do ponteiro como final do arquivo
        public IORet SetEOF()
        {
            if (!lockedByMe)
            {
                Log.SendMessage(name, (int)IORet.RET_IO_UNLOCKED, null, LogMessageType.Error);
                return IORet.RET_IO_UNLOCKED;
            }
            
            Seek(position, SeekOrigin.Begin);

            if (SetEndOfFile(handle.handle) == 0)
            {
                result = Marshal.GetLastWin32Error();
                Log.SendMessage(name, (int)IORet.RET_IO_ERROR_SET_EOF, result, LogMessageType.Error);
                return IORet.RET_IO_ERROR_SET_EOF;
            }

            handle.size = handle.position;
            return IORet.RET_IO_OK;
        }

        //--------------- Fecha o IIOContext
        public IORet Close()
        {
            if (!closed)
            {
                if (working) asyncThread.Abort();
                if (lockedByMe) Unlock();
                IOManager.ReleaseContext(this, false);

                name = "";
                handle = null;
                position = 0;
                result = 0;
                working = false;
                lockedByMe = false;
                closed = true;
            }
            return IORet.RET_IO_OK;
        }

        //--------------- Fecha o IIOContext e o Handle
        public IORet ForceClose()
        {
            if (!closed)
            {
                if (working) asyncThread.Abort();
                if (lockedByMe) Unlock();
                IOManager.ReleaseContext(this, true);

                name = "";
                handle = null;
                position = 0;
                result = 0;
                working = false;
                lockedByMe = false;
                closed = true;
            }
            return IORet.RET_IO_OK;
        }

        //--------------- Prepara o Context para uso assíncrono
        public IORet PrepareForAsync()
        {
            threadinfo = new THREADINFO();
            threadinfo.operation = 0;
            threadinfo.kill_thread = false;

            asyncThread = new Thread(new ThreadStart(AsyncRoutine));
            asyncThread.Start();
            return IORet.RET_IO_OK;
        }

        //--------------- Desinicializa thread the leitura/escrita
        public IORet FreeAsync()
        {
            if (asyncThread != null)
            {
                threadinfo.kill_thread = true;
                while (!threadinfo.thread_returned) Thread.Sleep(1);
                asyncThread = null;
            }
            return IORet.RET_IO_OK;
        }

        //--------------- Adquire acesso exclusivo ao handle
        public IORet Lock()
        {
            if (!CanWork)
            {
                if (closed)
                {
                    Log.SendMessage(name, (int)IORet.RET_IO_CONTEXT_CLOSED, null, LogMessageType.Error);
                    return IORet.RET_IO_CONTEXT_CLOSED;
                }
                else if (handle.closed)
                {
                    Log.SendMessage(name, (int)IORet.RET_IO_HANDLE_CLOSED, null, LogMessageType.Error);
                    return IORet.RET_IO_HANDLE_CLOSED;
                }
                else
                {
                    Log.SendMessage(name, (int)IORet.RET_IO_INVALID_HANDLE, null, LogMessageType.Error);
                    return IORet.RET_IO_INVALID_HANDLE;
                }
            }

            if (CantWorkInLock)
            {
                Log.SendMessage(name, (int)IORet.RET_IO_LOCKED, null, LogMessageType.Error);
                return IORet.RET_IO_LOCKED;
            }

            if (lockedByMe)
            {
                Log.SendMessage(name, (int)IORet.RET_IO_ALREADY_LOCKED, null, LogMessageType.Error);
                return IORet.RET_IO_ALREADY_LOCKED;
            }

            //

            lockedByMe = true;
            handle.locked = true;
            return IORet.RET_IO_OK;
        }

        //--------------- Libera o acesso do handle para outros Context
        public IORet Unlock()
        {
            lockedByMe = false;
            handle.locked = false;
            return IORet.RET_IO_OK;
        }

        //--------------- Lê um bloco de dados do dispositivo para uma matriz
        public IORet Read(Array buffer, long pos, int count)
        {
            Seek(pos);
            return Read(buffer, count);
        }

        //--------------- Lê um bloco de dados do dispositivo para uma matriz
        public IORet Read(Array buffer, int count)
        {
            if (!lockedByMe)
            {
                Log.SendMessage(name, (int)IORet.RET_IO_UNLOCKED, null, LogMessageType.Error);
                return IORet.RET_IO_UNLOCKED;
            }

            uint read = 0;
            int r = 0;

            uint c = (uint)count;

            if (buffer is byte[]) r = ReadFile(handle.handle, (byte[])buffer, c, ref read, 0);
            else if (buffer is ushort[]) r = ReadFile(handle.handle, (ushort[])buffer, c * 2, ref read, 0);
            else if (buffer is uint[]) r = ReadFile(handle.handle, (uint[])buffer, c * 4, ref read, 0);
            else if (buffer is ulong[]) r = ReadFile(handle.handle, (ulong[])buffer, c * 8, ref read, 0);
            else if (buffer is short[]) r = ReadFile(handle.handle, (short[])buffer, c * 2, ref read, 0);
            else if (buffer is int[]) r = ReadFile(handle.handle, (int[])buffer, c * 4, ref read, 0);
            else if (buffer is long[]) r = ReadFile(handle.handle, (long[])buffer, c * 8, ref read, 0);
            else if (buffer is sbyte[]) r = ReadFile(handle.handle, (sbyte[])buffer, c, ref read, 0);
            else if (buffer is char[]) r = ReadFile(handle.handle, (char[])buffer, c * 2, ref read, 0);
            else
            {
                result = 0;
                Log.SendMessage(name, (int)IORet.RET_IO_INVALIDARG, "typeof(buffer)", LogMessageType.Error);
                return IORet.RET_IO_INVALIDARG;
            }

            handle.position += read;

            if (r == 0)
            {
                result = Marshal.GetLastWin32Error();
                Log.SendMessage(name, (int)IORet.RET_IO_ERROR_READING, result, LogMessageType.Error);
                return IORet.RET_IO_ERROR_READING;
            }
                
            result = 0;
            return IORet.RET_IO_OK;
        }

        //--------------- Escreve o conteudo de uma matriz no handle
        public IORet Write(Array buffer, long pos, int count)
        {
            Seek(pos);
            return Write(buffer, count);
        }

        //--------------- Escreve o conteudo de uma matriz no handle
        public IORet Write(Array buffer, int count)
        {
            if (!lockedByMe)
            {
                Log.SendMessage(name, (int)IORet.RET_IO_UNLOCKED, null, LogMessageType.Error);
                return IORet.RET_IO_UNLOCKED;
            }

            uint written = 0;
            int r = 0;

            uint c = (uint)count;

            if (buffer is byte[]) r = WriteFile(handle.handle, (byte[])buffer, c, ref written, 0);
            else if (buffer is ushort[]) r = WriteFile(handle.handle, (ushort[])buffer, 2 * c, ref written, 0);
            else if (buffer is uint[]) r = WriteFile(handle.handle, (uint[])buffer, 4 * c, ref written, 0);
            else if (buffer is ulong[]) r = WriteFile(handle.handle, (ulong[])buffer, 8 * c, ref written, 0);
            else if (buffer is short[]) r = WriteFile(handle.handle, (short[])buffer, 2 * c, ref written, 0);
            else if (buffer is int[]) r = WriteFile(handle.handle, (int[])buffer, 4 * c, ref written, 0);
            else if (buffer is long[]) r = WriteFile(handle.handle, (long[])buffer, 8 * c, ref written, 0);
            else if (buffer is sbyte[]) r = WriteFile(handle.handle, (sbyte[])buffer, c, ref written, 0);
            else if (buffer is char[]) r = WriteFile(handle.handle, (char[])buffer, 2 * c, ref written, 0);
            else
            {
                result = 0;
                Log.SendMessage(name, (int)IORet.RET_IO_INVALIDARG, "typeof(buffer)", LogMessageType.Error);
                return IORet.RET_IO_INVALIDARG;
            }

            handle.position += written;

            if (r == 0)
            {
                result = Marshal.GetLastWin32Error();
                Log.SendMessage(name, (int)IORet.RET_IO_ERROR_READING, result, LogMessageType.Error);
                return IORet.RET_IO_ERROR_READING;
            }

            result = 0;
            return IORet.RET_IO_OK;
        }

        //--------------- Lê um bloco de dados do dispositivo para uma matriz de modo assíncrono
        public IORet ReadAsync(Array buffer, long pos, int count)
        {
            Seek(pos);
            return ReadAsync(buffer, count);
        }

        //--------------- Lê um bloco de dados do dispositivo para uma matriz de modo assíncrono
        public IORet ReadAsync(Array buffer, int count)
        {
            if (!lockedByMe) return IORet.RET_IO_UNLOCKED;
            if (working) return IORet.RET_IO_ALREADY_WORKING;

            threadinfo.array = buffer;
            threadinfo.count = count;
            threadinfo.operation = 1;

            working = true;
            return IORet.RET_IO_OK;
        }

        //--------------- Escreve o conteudo de uma matriz no handle de modo assíncrono
        public IORet WriteAsync(Array buffer, long pos, int count)
        {
            Seek(pos);
            return WriteAsync(buffer, count);
        }

        //--------------- Escreve o conteudo de uma matriz no handle de modo assíncrono
        public IORet WriteAsync(Array buffer, int count)
        {
            if (!lockedByMe) return IORet.RET_IO_UNLOCKED;
            if (working) return IORet.RET_IO_ALREADY_WORKING;

            threadinfo.array = buffer;
            threadinfo.count = count;
            threadinfo.operation = 2;

            working = true;
            return IORet.RET_IO_OK;
        }

        //--------------- Lê as informações da partição
        public PARTITION_INFORMATION GetPartitionInformation()
        {
            PARTITION_INFORMATION pi = new PARTITION_INFORMATION();
            int bytes = 0;

            if (!lockedByMe)
            {
                result = (int)IORet.RET_IO_UNLOCKED;
                Log.SendMessage(name, (int)IORet.RET_IO_UNLOCKED, null, LogMessageType.Error);
                return pi;
            }

            //--------- Determina as informações da partição
            if (DeviceIoControl(handle.handle, IOManager.IOCTL_DISK_GET_PARTITION_INFO, IntPtr.Zero, 0, ref pi,
                Marshal.SizeOf(pi), ref bytes, 0) == 0)
            {
                //Loga erro
                result = Marshal.GetLastWin32Error();
                Log.SendMessage(name, (int)IORet.RET_IO_NATIVEERROR, result, LogMessageType.Error);
            }

            return pi;
        }

        //--------------- Lê a Geometria do disco
        public DISK_GEOMETRY GetDiskGeometry()
        {
            DISK_GEOMETRY dg = new DISK_GEOMETRY();
            int bytes = 0;

            if (!lockedByMe)
            {
                result = (int)IORet.RET_IO_UNLOCKED;
                Log.SendMessage(name, (int)IORet.RET_IO_UNLOCKED, null, LogMessageType.Error);
                return dg;
            }

            //--------- Determina a geometria do disco
            if (DeviceIoControl(handle.handle, IOManager.IOCTL_DISK_GET_DRIVE_GEOMETRY, IntPtr.Zero, 0, ref dg,
                Marshal.SizeOf(dg), ref bytes, 0) == 0)
            {
                //Loga erro
                result = Marshal.GetLastWin32Error();
                Log.SendMessage(name, (int)IORet.RET_IO_NATIVEERROR, result, LogMessageType.Error);
            }

            return dg;
        }

        //--------------- Essas funções de leitura escrita não funcionarão se o handle nao for criado com Buffering

        //--------------- Lê um byte
        public byte ReadByte()
        {
            if (!lockedByMe) { result = (int)IORet.RET_IO_FAIL; return 0; }

            byte b = 0;
            uint read = 0;

            if (ReadFile(handle.handle, ref b, 1, ref read, 0) == 0)
            {
                result = Marshal.GetLastWin32Error();
                Log.SendMessage(name, (int)IORet.RET_IO_NATIVEERROR, result, LogMessageType.Error);
            }
            else
            {
                result = 0;
                position += 1;
                handle.position += 1;
            }

            return b;
        }

        //--------------- Lê um caracteren Unicode
        public char ReadChar()
        {
            if (!lockedByMe) { result = (int)IORet.RET_IO_UNLOCKED; return '\0'; }

            char b = '\0';
            uint read = 0;

            if (ReadFile(handle.handle, ref b, 2, ref read, 0) == 0)
            {
                result = Marshal.GetLastWin32Error();
                Log.SendMessage(name, (int)IORet.RET_IO_NATIVEERROR, result, LogMessageType.Error);
            }
            else
            {
                result = 0;
                position += 2;
                handle.position += 2;
            }

            return b;
        }

        //--------------- Lê um inteiro de 32 bits
        public int ReadInt32()
        {
            if (!lockedByMe) { result = (int)IORet.RET_IO_UNLOCKED; return 0; }

            int b = 0;
            uint read = 0;

            if (ReadFile(handle.handle, ref b, 4, ref read, 0) == 0)
            {
                result = Marshal.GetLastWin32Error();
                Log.SendMessage(name, (int)IORet.RET_IO_NATIVEERROR, result, LogMessageType.Error);
            }
            else
            {
                result = 0;
                position += 4;
                handle.position += 4;
            }

            return b;
        }

        //--------------- Lê um inteiro de 32 bits sem sinal
        public uint ReadUInt32()
        {
            if (!lockedByMe) { result = (int)IORet.RET_IO_UNLOCKED; return 0; }

            uint b = 0;
            uint read = 0;

            if (ReadFile(handle.handle, ref b, 4, ref read, 0) == 0)
            {
                result = Marshal.GetLastWin32Error();
                Log.SendMessage(name, (int)IORet.RET_IO_NATIVEERROR, result, LogMessageType.Error);
            }
            else
            {
                result = 0;
                position += 4;
                handle.position += 4;
            }

            return b;
        }

        //--------------- Lê um inteiro de 16 bits
        public short ReadInt16()
        {
            if (!lockedByMe) { result = (int)IORet.RET_IO_UNLOCKED; return 0; }

            short b = 0;
            uint read = 0;

            if (ReadFile(handle.handle, ref b, 2, ref read, 0) == 0)
            {
                result = Marshal.GetLastWin32Error();
                Log.SendMessage(name, (int)IORet.RET_IO_NATIVEERROR, result, LogMessageType.Error);
            }
            else
            {
                result = 0;
                position += 2;
                handle.position += 2;
            }

            return b;
        }

        //--------------- Lê um inteiro de 16 bits sem sinal
        public ushort ReadUInt16()
        {
            if (!lockedByMe) { result = (int)IORet.RET_IO_UNLOCKED; return 0; }

            ushort b = 0;
            uint read = 0;

            if (ReadFile(handle.handle, ref b, 2, ref read, 0) == 0)
            {
                result = Marshal.GetLastWin32Error();
                Log.SendMessage(name, (int)IORet.RET_IO_NATIVEERROR, result, LogMessageType.Error);
            }
            else
            {
                result = 0;
                position += 2;
                handle.position += 2;
            }

            return b;
        }

        //--------------- Lê um inteiro de 64 bits
        public long ReadInt64()
        {
            if (!lockedByMe) { result = (int)IORet.RET_IO_UNLOCKED; return 0; }

            long b = 0;
            uint read = 0;

            if (ReadFile(handle.handle, ref b, 8, ref read, 0) == 0)
            {
                result = Marshal.GetLastWin32Error();
                Log.SendMessage(name, (int)IORet.RET_IO_NATIVEERROR, result, LogMessageType.Error);
            }
            else
            {
                result = 0;
                position += 8;
                handle.position += 8;
            }

            return b;
        }

        //--------------- Lê um inteiro de 64 bits sem sinal
        public ulong ReadUInt64()
        {
            if (!lockedByMe) { result = (int)IORet.RET_IO_UNLOCKED; return 0; }

            ulong b = 0;
            uint read = 0;

            if (ReadFile(handle.handle, ref b, 8, ref read, 0) == 0)
            {
                result = Marshal.GetLastWin32Error();
                Log.SendMessage(name, (int)IORet.RET_IO_NATIVEERROR, result, LogMessageType.Error);
            }
            else
            {
                result = 0;
                position += 8;
                handle.position += 8;
            }

            return b;
        }

        //--------------- Lê uma sequencia de caracteres até encontrar um zero
        public string ReadString(bool unicode)
        {
            if (!lockedByMe) { result = (int)IORet.RET_IO_UNLOCKED; return ""; }

            StringBuilder s = new StringBuilder();
            char b = '\0';
            uint read = 0;
            uint count = (uint)(unicode ? 2 : 1);

            while (true)
            {
                b = '\0';
                if (ReadFile(handle.handle, ref b, count, ref read, 0) == 0)
                {
                    result = Marshal.GetLastWin32Error();
                    Log.SendMessage(name, (int)IORet.RET_IO_NATIVEERROR, result, LogMessageType.Error);
                    return s.ToString();
                }

                position ++;
                handle.position ++;

                if (b == '\0') break;

                s.Append(b);
            }

            result = 0;
            return s.ToString();
        }

        //--------------- Lê uma sequencia de caracteres até encontrar um zero ou chegar em um valor máximo
        public string ReadString(bool unicode, int max)
        {
            if (!lockedByMe) { result = (int)IORet.RET_IO_UNLOCKED; return ""; }

            StringBuilder s = new StringBuilder();
            char b = '\0';
            uint read = 0;
            uint count = (uint)(unicode ? 2 : 1);
            int i = 0;

            while (true)
            {
                b = '\0';
                if (ReadFile(handle.handle, ref b, count, ref read, 0) == 0)
                {
                    result = Marshal.GetLastWin32Error();
                    Log.SendMessage(name, (int)IORet.RET_IO_NATIVEERROR, result, LogMessageType.Error);
                    return s.ToString();
                }

                position++;
                handle.position++;

                if (b == '\0') break;

                s.Append(b);
                if (++i == max) break;
            }

            result = 0;
            return s.ToString();
        }

        //--------------- Escreve um byte no handle
        public IORet Write(byte v)
        {
            if (!lockedByMe) return IORet.RET_IO_UNLOCKED;

            uint written = 0;
            if (WriteFile(handle.handle, ref v, 1, ref written, 0) == 0)
            {
                result = Marshal.GetLastWin32Error();
                Log.SendMessage(name, (int)IORet.RET_IO_NATIVEERROR, result, LogMessageType.Error);
                return IORet.RET_IO_ERROR_WRITING;
            }

            result = 0;
            position += 1;
            handle.position += 1;
            return IORet.RET_IO_OK;
        }

        //--------------- Escreve um caractere Unicode no handle
        public IORet Write(char v)
        {
            if (!lockedByMe) return IORet.RET_IO_UNLOCKED;

            uint written = 0;
            if (WriteFile(handle.handle, ref v, 2, ref written, 0) == 0)
            {
                result = Marshal.GetLastWin32Error();
                Log.SendMessage(name, (int)IORet.RET_IO_NATIVEERROR, result, LogMessageType.Error);
                return IORet.RET_IO_ERROR_WRITING;
            }

            result = 0;
            position += 2;
            handle.position += 2;
            return IORet.RET_IO_OK;
        }

        //--------------- Escreve um inteiro de 16 bits no handle
        public IORet Write(short v)
        {
            if (!lockedByMe) return IORet.RET_IO_UNLOCKED;

            uint written = 0;
            if (WriteFile(handle.handle, ref v, 2, ref written, 0) == 0)
            {
                result = Marshal.GetLastWin32Error();
                Log.SendMessage(name, (int)IORet.RET_IO_NATIVEERROR, result, LogMessageType.Error);
                return IORet.RET_IO_ERROR_WRITING;
            }

            result = 0;
            position += 2;
            handle.position += 2;
            return IORet.RET_IO_OK;
        }

        //--------------- Escreve um inteiro de 16 bits em sinal no handle
        public IORet Write(ushort v)
        {
            if (!lockedByMe) return IORet.RET_IO_UNLOCKED;

            uint written = 0;
            if (WriteFile(handle.handle, ref v, 2, ref written, 0) == 0)
            {
                result = Marshal.GetLastWin32Error();
                Log.SendMessage(name, (int)IORet.RET_IO_NATIVEERROR, result, LogMessageType.Error);
                return IORet.RET_IO_ERROR_WRITING;
            }

            result = 0;
            position += 2;
            handle.position += 2;
            return IORet.RET_IO_OK;
        }

        //--------------- Escreve um inteiro de 32 bits no handle
        public IORet Write(int v)
        {
            if (!lockedByMe) return IORet.RET_IO_UNLOCKED;

            uint written = 0;
            if (WriteFile(handle.handle, ref v, 4, ref written, 0) == 0)
            {
                result = Marshal.GetLastWin32Error();
                Log.SendMessage(name, (int)IORet.RET_IO_NATIVEERROR, result, LogMessageType.Error);
                return IORet.RET_IO_ERROR_WRITING;
            }

            result = 0;
            position += 4;
            handle.position += 4;
            return IORet.RET_IO_OK;
        }

        //--------------- Escreve um inteiro de 32 bits sem sinal no handle
        public IORet Write(uint v)
        {
            if (!lockedByMe) return IORet.RET_IO_UNLOCKED;

            uint written = 0;
            if (WriteFile(handle.handle, ref v, 4, ref written, 0) == 0)
            {
                result = Marshal.GetLastWin32Error();
                Log.SendMessage(name, (int)IORet.RET_IO_NATIVEERROR, result, LogMessageType.Error);
                return IORet.RET_IO_ERROR_WRITING;
            }

            result = 0;
            position += 4;
            handle.position += 4;
            return IORet.RET_IO_OK;
        }

        //--------------- Escreve um inteiro de 64 bits no handle
        public IORet Write(long v)
        {
            if (!lockedByMe) return IORet.RET_IO_UNLOCKED;

            uint written = 0;
            if (WriteFile(handle.handle, ref v, 8, ref written, 0) == 0)
            {
                result = Marshal.GetLastWin32Error();
                Log.SendMessage(name, (int)IORet.RET_IO_NATIVEERROR, result, LogMessageType.Error);
                return IORet.RET_IO_ERROR_WRITING;
            }

            result = 0;
            position += 8;
            handle.position += 8;
            return IORet.RET_IO_OK;
        }

        //--------------- Escreve um inteiro de 64 bits sem sinal no handle
        public IORet Write(ulong v)
        {
            if (!lockedByMe) return IORet.RET_IO_UNLOCKED;

            uint written = 0;
            if (WriteFile(handle.handle, ref v, 8, ref written, 0) == 0)
            {
                result = Marshal.GetLastWin32Error();
                Log.SendMessage(name, (int)IORet.RET_IO_NATIVEERROR, result, LogMessageType.Error);
                return IORet.RET_IO_ERROR_WRITING;
            }

            result = 0;
            position += 8;
            handle.position += 8;
            return IORet.RET_IO_OK;
        }

        //--------------- Escreve uma sequencia de caracteres no handle
        public IORet Write(string s, bool unicode)
        {
            if (!lockedByMe) return IORet.RET_IO_UNLOCKED;

            char b;
            uint count = (uint)(unicode ? 2 : 1);
            uint written = 0;

            for (int i = 0; i < s.Length; i++)
            {
                b = s[i];
                if (WriteFile(handle.handle, ref b, count, ref written, 0) == 0)
                {
                    result = Marshal.GetLastWin32Error();
                    Log.SendMessage(name, (int)IORet.RET_IO_NATIVEERROR, result, LogMessageType.Error);
                    return IORet.RET_IO_ERROR_WRITING;
                }
                position += count;
                handle.position += count;
            }

            result = 0;
            return IORet.RET_IO_OK;
        }

        #endregion

        #region IDisposable Members

        public void Dispose()
        {
            Close();
        }

        #endregion

        //------------------------------------------

        struct THREADINFO
        {
            public Array array;
            public int count;
            public byte operation; // 0 - nada, 1 - leitura, 2 - escrita

            public bool kill_thread;
            public bool thread_returned;
        }

        void AsyncRoutine()
        {
            while (!threadinfo.kill_thread)
            {
                if (threadinfo.operation == 1)
                {
                    Read(threadinfo.array, threadinfo.count);
                    threadinfo.operation = 0;
                    working = false;
                }
                else if (threadinfo.operation == 2)
                {
                    Write(threadinfo.array, threadinfo.count);
                    threadinfo.operation = 0;
                    working = false;
                }
                else
                {
                    Thread.Sleep(1);
                }
            }

            threadinfo.thread_returned = true;
        }

        //------------------------------------------

        [DllImport("kernel32.dll", CharSet = CharSet.Auto, SetLastError = true)]
        private static extern int SetFilePointerEx(int handle, long offset, ref long newPtr, SeekOrigin seekmode);

        [DllImport("kernel32.dll", CharSet = CharSet.Auto, SetLastError = true)]
        private static extern int SetEndOfFile(int handle);

        [DllImport("Kernel32.dll", CharSet = CharSet.Auto, SetLastError = true)]
        private static extern int DeviceIoControl(int hDevice, uint dwIoControlCode, IntPtr InBuffer, int nInBufferSize, 
            ref DISK_GEOMETRY OutBuffer, int nOutBufferSize, ref int pBytesReturned, int lpOverlapped);

        [DllImport("Kernel32.dll", CharSet = CharSet.Auto, SetLastError = true)]
        private static extern int DeviceIoControl(int hDevice, uint dwIoControlCode, IntPtr InBuffer, int nInBufferSize, 
            ref PARTITION_INFORMATION OutBuffer, int nOutBufferSize, ref int pBytesReturned, int lpOverlapped);

        #region ReadFiles

        [DllImport("kernel32.dll", CharSet = CharSet.Auto, SetLastError = true)]
        private static extern int ReadFile(int handle, byte[] buffer, uint count, ref uint bytesRead, int lpOverlapped);

        [DllImport("kernel32.dll", CharSet = CharSet.Auto, SetLastError = true)]
        private static extern int ReadFile(int handle, sbyte[] buffer, uint count, ref uint bytesRead, int lpOverlapped);

        [DllImport("kernel32.dll", CharSet = CharSet.Auto, SetLastError = true)]
        private static extern int ReadFile(int handle, char[] buffer, uint count, ref uint bytesRead, int lpOverlapped);

        [DllImport("kernel32.dll", CharSet = CharSet.Auto, SetLastError = true)]
        private static extern int ReadFile(int handle, short[] buffer, uint count, ref uint bytesRead, int lpOverlapped);

        [DllImport("kernel32.dll", CharSet = CharSet.Auto, SetLastError = true)]
        private static extern int ReadFile(int handle, ushort[] buffer, uint count, ref uint bytesRead, int lpOverlapped);

        [DllImport("kernel32.dll", CharSet = CharSet.Auto, SetLastError = true)]
        private static extern int ReadFile(int handle, int[] buffer, uint count, ref uint bytesRead, int lpOverlapped);

        [DllImport("kernel32.dll", CharSet = CharSet.Auto, SetLastError = true)]
        private static extern int ReadFile(int handle, uint[] buffer, uint count, ref uint bytesRead, int lpOverlapped);

        [DllImport("kernel32.dll", CharSet = CharSet.Auto, SetLastError = true)]
        private static extern int ReadFile(int handle, long[] buffer, uint count, ref uint bytesRead, int lpOverlapped);

        [DllImport("kernel32.dll", CharSet = CharSet.Auto, SetLastError = true)]
        private static extern int ReadFile(int handle, ulong[] buffer, uint count, ref uint bytesRead, int lpOverlapped);

        [DllImport("kernel32.dll", CharSet = CharSet.Auto, SetLastError = true)]
        private static extern int ReadFile(int handle, ref byte v, uint count, ref uint bytesRead, int lpOverlapped);

        [DllImport("kernel32.dll", CharSet = CharSet.Auto, SetLastError = true)]
        private static extern int ReadFile(int handle, ref sbyte v, uint count, ref uint bytesRead, int lpOverlapped);

        [DllImport("kernel32.dll", CharSet = CharSet.Auto, SetLastError = true)]
        private static extern int ReadFile(int handle, ref char v, uint count, ref uint bytesRead, int lpOverlapped);

        [DllImport("kernel32.dll", CharSet = CharSet.Auto, SetLastError = true)]
        private static extern int ReadFile(int handle, ref short v, uint count, ref uint bytesRead, int lpOverlapped);

        [DllImport("kernel32.dll", CharSet = CharSet.Auto, SetLastError = true)]
        private static extern int ReadFile(int handle, ref ushort v, uint count, ref uint bytesRead, int lpOverlapped);

        [DllImport("kernel32.dll", CharSet = CharSet.Auto, SetLastError = true)]
        private static extern int ReadFile(int handle, ref int v, uint count, ref uint bytesRead, int lpOverlapped);

        [DllImport("kernel32.dll", CharSet = CharSet.Auto, SetLastError = true)]
        private static extern int ReadFile(int handle, ref uint v, uint count, ref uint bytesRead, int lpOverlapped);

        [DllImport("kernel32.dll", CharSet = CharSet.Auto, SetLastError = true)]
        private static extern int ReadFile(int handle, ref long v, uint count, ref uint bytesRead, int lpOverlapped);

        [DllImport("kernel32.dll", CharSet = CharSet.Auto, SetLastError = true)]
        private static extern int ReadFile(int handle, ref ulong v, uint count, ref uint bytesRead, int lpOverlapped);

        #endregion

        #region WriteFiles

        [DllImport("kernel32.dll", CharSet = CharSet.Auto, SetLastError = true)]
        private static extern int WriteFile(int handle, byte[] buffer, uint count, ref uint bytesWritten, int lpOverlapped);

        [DllImport("kernel32.dll", CharSet = CharSet.Auto, SetLastError = true)]
        private static extern int WriteFile(int handle, sbyte[] buffer, uint count, ref uint bytesWritten, int lpOverlapped);

        [DllImport("kernel32.dll", CharSet = CharSet.Auto, SetLastError = true)]
        private static extern int WriteFile(int handle, char[] buffer, uint count, ref uint bytesWritten, int lpOverlapped);

        [DllImport("kernel32.dll", CharSet = CharSet.Auto, SetLastError = true)]
        private static extern int WriteFile(int handle, short[] buffer, uint count, ref uint bytesWritten, int lpOverlapped);

        [DllImport("kernel32.dll", CharSet = CharSet.Auto, SetLastError = true)]
        private static extern int WriteFile(int handle, ushort[] buffer, uint count, ref uint bytesWritten, int lpOverlapped);

        [DllImport("kernel32.dll", CharSet = CharSet.Auto, SetLastError = true)]
        private static extern int WriteFile(int handle, int[] buffer, uint count, ref uint bytesWritten, int lpOverlapped);

        [DllImport("kernel32.dll", CharSet = CharSet.Auto, SetLastError = true)]
        private static extern int WriteFile(int handle, uint[] buffer, uint count, ref uint bytesWritten, int lpOverlapped);

        [DllImport("kernel32.dll", CharSet = CharSet.Auto, SetLastError = true)]
        private static extern int WriteFile(int handle, long[] buffer, uint count, ref uint bytesWritten, int lpOverlapped);

        [DllImport("kernel32.dll", CharSet = CharSet.Auto, SetLastError = true)]
        private static extern int WriteFile(int handle, ulong[] buffer, uint count, ref uint bytesWritten, int lpOverlapped);

        [DllImport("kernel32.dll", CharSet = CharSet.Auto, SetLastError = true)]
        private static extern int WriteFile(int handle, ref byte v, uint count, ref uint bytesWritten, int lpOverlapped);

        [DllImport("kernel32.dll", CharSet = CharSet.Auto, SetLastError = true)]
        private static extern int WriteFile(int handle, ref sbyte v, uint count, ref uint bytesWritten, int lpOverlapped);

        [DllImport("kernel32.dll", CharSet = CharSet.Auto, SetLastError = true)]
        private static extern int WriteFile(int handle, ref char v, uint count, ref uint bytesWritten, int lpOverlapped);

        [DllImport("kernel32.dll", CharSet = CharSet.Auto, SetLastError = true)]
        private static extern int WriteFile(int handle, ref short v, uint count, ref uint bytesWritten, int lpOverlapped);

        [DllImport("kernel32.dll", CharSet = CharSet.Auto, SetLastError = true)]
        private static extern int WriteFile(int handle, ref ushort v, uint count, ref uint bytesWritten, int lpOverlapped);

        [DllImport("kernel32.dll", CharSet = CharSet.Auto, SetLastError = true)]
        private static extern int WriteFile(int handle, ref int v, uint count, ref uint bytesWritten, int lpOverlapped);

        [DllImport("kernel32.dll", CharSet = CharSet.Auto, SetLastError = true)]
        private static extern int WriteFile(int handle, ref uint v, uint count, ref uint bytesWritten, int lpOverlapped);

        [DllImport("kernel32.dll", CharSet = CharSet.Auto, SetLastError = true)]
        private static extern int WriteFile(int handle, ref long v, uint count, ref uint bytesWritten, int lpOverlapped);

        [DllImport("kernel32.dll", CharSet = CharSet.Auto, SetLastError = true)]
        private static extern int WriteFile(int handle, ref ulong v, uint count, ref uint bytesWritten, int lpOverlapped);

        #endregion
    }
}
