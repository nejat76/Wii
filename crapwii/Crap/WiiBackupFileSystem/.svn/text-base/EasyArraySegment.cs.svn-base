//-------------------------------------
// WBFSSync - WBFSSync.exe
//
// Copyright 2009 Caian (ÔmΣga Frøst) <frost.omega@hotmail.com>
//
// WBFSSync is Licensed under the terms of the Microsoft Reciprocal License (Ms-RL)
//
// EasyArraySegment.cs:
//
// Estrutura similar ao ArraySegment mas com indexação automática
//
//-------------------------------------

using System;
using System.Collections.Generic;
//using System.Linq;
using System.Text;

namespace WBFSSync
{
    public struct EasyArraySegment<T>
    {
        T[] Array;
        Int32 Start;
        Int32 Count;

        public T this[int i]
        {
            get
            {
                return Array[i + Start];
            }

            set
            {
                Array[i + Start] = value;
            }
        }

        public EasyArraySegment(T[] array)
        {
            Array = array;
            Start = 0;
            Count = array.Length;
        }

        public EasyArraySegment(T[] array, int start, int count)
        {
            Array = array;
            Start = start;
            Count = count;
        }
    }
}
