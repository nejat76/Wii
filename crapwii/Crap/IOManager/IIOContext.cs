//-------------------------------------
// WBFSSync - WBFSSync.exe
//
// Copyright 2009 Caian (ÔmΣga Frøst) <frost.omega@hotmail.com>
//
// WBFSSync is Licensed under the terms of the Microsoft Reciprocal License (Ms-RL)
//
// IIOContext.cs:
//
// Interface que garante seek / leitura / escrita seguros de um handle
//
// Nota: a estrutura char é Unicode, portanto, 16bits
// 
// Nota: Alguns nome convencionados no programa:
//  VDRIVE - Disco virtual
//  LDRIVE - Partição Lógicas (ex. C:\)
//  PDRIVE - Disco físico aberto (ex. PhysicalDrive0)
//
//-------------------------------------

using System;
using System.Collections.Generic;
//using System.Linq;
using System.Text;
using System.IO;

namespace WBFSSync
{
    public interface IIOContext
    {
        string Name { get; } //Identifica o IIOContext
        string Path { get; } //O Arquivo associado ao Contexto
        int Handle { get; }
        long Position { get; }
        long Length { get; }
        int Result { get; }

        bool Working { get; } //Verdadeiro quando em uma operação assíncrona
        bool Closed { get; } //O IIOContext foi fechado
        bool CanWork { get; } //Verifica se o Handle está acessível (destrancado, aberto e válido)
        bool Locked { get; } //Verifica se o Contexto detém acesso exclusivo ao Handle

        IORet Seek(long pos, SeekOrigin origin); // Move a posição do ponteiro do arquivo para o local desejado
        IORet Seek(long pos);

        IORet SetEOF(); //Seta a posição atual como final do arquivo
        IORet Close(); // Garante que o contador de referência do Handle usado seja decrementado e a memória liberada
        IORet ForceClose(); //Fecha o handle, libera o contexto e trava todos os contextos que compartilham esse handle

        IORet PrepareForAsync(); //Cria a Thread para leitura/escrita assíncrona
        IORet FreeAsync(); //Fecha a Thread de leitura/escrita

        IORet Lock(); // Ganha acesso exclusivo ao handle, quaisquer chamadas de outro IIOContext retornam RET_IO_LOCKED
                      // chamar Lock(); mais de uma vez antes do Unlock(); retorna erro, previne que o mesmo IIOContext seja
                      // usado em mais de uma rotina

        IORet Unlock(); // Libera acesso para outras IIOContexts

        // Rotinas demoradas, possuem versão assíncrona
        IORet Read(Array buffer, int count);
        IORet Write(Array buffer, int count);

        IORet Read(Array buffer, long pos, int count);
        IORet Write(Array buffer, long pos, int count);

        IORet ReadAsync(Array buffer, int count);
        IORet WriteAsync(Array buffer, int count);

        IORet ReadAsync(Array buffer, long pos, int count);
        IORet WriteAsync(Array buffer, long pos, int count);

        // Rotinas rápidas, não possuem versão assíncrona
        // Como não podem retornar códigos IORet, em caso de erro IIOContext.Result != 0
        // Para erros do tipo IORet - Result < 0, para erros nativos Result > 0

        PARTITION_INFORMATION GetPartitionInformation();
        DISK_GEOMETRY GetDiskGeometry();

        byte    ReadByte();
        char    ReadChar();
        int     ReadInt32();
        uint    ReadUInt32();
        short   ReadInt16();
        ushort  ReadUInt16();
        long    ReadInt64();
        ulong   ReadUInt64();
        string  ReadString(bool unicode);
        string ReadString(bool unicode, int max);

        IORet Write(byte v);
        IORet Write(char v);
        IORet Write(short v);
        IORet Write(ushort v);
        IORet Write(int v);
        IORet Write(uint v);
        IORet Write(long v);
        IORet Write(ulong v);
        IORet Write(string s, bool unicode);
    }
}
