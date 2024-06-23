//-------------------------------------
// WBFSSync - WBFSSync.exe
//
// Copyright 2009 Caian (ÔmΣga Frøst) <frost.omega@hotmail.com>
//
// WBFSSync is Licensed under the terms of the Microsoft Reciprocal License (Ms-RL)
//
// Handle.cs:
//
// Classe que concentra informações sobre um arquivo aberto
//
//-------------------------------------

using System;
using System.Collections.Generic;
//using System.Linq;
using System.Text;

namespace WBFSSync
{
    class Handle
    {
        public string name = "";
        public int handle = -1;

        public int counter = 0;

        public long position = 0;
        public long size = 0;

        public bool locked = false;
        public bool closed { get { return counter == 0; } }
        public bool valid { get { return handle != -1; } }
    }
}
