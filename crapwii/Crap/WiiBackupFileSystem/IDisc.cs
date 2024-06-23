//-------------------------------------
// WBFSSync - WBFSSync.exe
//
// Copyright 2009 Caian (ÔmΣga Frøst) <frost.omega@hotmail.com>
//
// WBFSSync is Licensed under the terms of the Microsoft Reciprocal License (Ms-RL)
//
// IDisc.cs:
//
// Interface básica de um disco de Wii salvo no dispositivo
//
//-------------------------------------

using System;
using System.Collections.Generic;
//using System.Linq;
using System.Text;
using System.IO;

namespace WBFSSync
{
    public enum DiscRegion : sbyte
    {
        Unknown = -1,
        NTSCJ = 0,
        NTSCU = 1,
        PAL = 2,
        KOR = 4
    }

    public interface IDisc
    {
        Boolean Deleted { get; } // Disco foi apagado mas ainda não removido da lista
        Boolean IsOpen { get; } // Disco aberto, ele usa um contador para abrir/fechar
        Int32 Counter { get; }

        Int32 Index { get; }
        String Name { get; set; }
        String Code { get; set; }
        DiscRegion Region { get; }

        Int64 Size { get; } //O tamanho real do disco, DVD5 ou DVD9
        Int64 WBFSSize { get; } //O tamanho dentro de uma partição WBFS

        UInt16[] WLBATable { get; } // Tabela de endereços dos setores WBFS do jogo no disco
        Int64 DiscInfoLBAPosition { get; } //A posição do cabeçalho de disco no dispositivo em setores de disco
        UInt32 UsedWBFSSectors { get; } //O número de setores WBFS usados pelo disco

        int Delete();

        void GetLBAPosAndSectorOffset(long position, //Converte a posição relativa ao disco de wii para o respectivo
            out long drivelba, out uint offset);     //indice do setor no drive + excedente não múltiplo de hdSectorSize

        void GetLBAPosAndSectorOffset(long position, //Converte a posição relativa ao disco de wii para o respectivo
            out long drivelba, out uint offset,      //indice do setor no drive + excedente não múltiplo de hdSectorSize
            out long wbfssecpos);                    //e indica a posição do setor WBFS no drive

        uint BuildDiscSectorUsage(out byte[] sectorUsage); //Cria uma tabela de uso de setores de wii e retorna os usados

        IIOContext GetDeviceContext();
    }
}
