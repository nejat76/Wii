//-------------------------------------
// WBFSSync - WBFSSync.exe
//
// Copyright 2009 Caian (ÔmΣga Frøst) <frost.omega@hotmail.com>
//
// WBFSSync is Licensed under the terms of the Microsoft Reciprocal License (Ms-RL)
//
// ProgressIndicator.cs:
//
// Classe que calcula progresso de uma operação
//
//-------------------------------------

using System;
using System.Collections.Generic;
//using System.Linq;
using System.Text;

namespace WBFSSync
{
    public class ProgressIndicator
    {
        public int cancel = 0; //Compatibilidade com funções nativas
        public Boolean Cancel { get { return cancel != 0; } set { cancel = value ? 1 : 0; } }

        public long Progress = 0;
        public long Total = 1; //Valor padrão 1 para evitar problemas de divisão por 0
        public long Stamp = 0; //Para calculo de progresso total de todas as operações

        long eta = 0;
        public TimeSpan ETA
        {
            get
            {
                float bpt = (DateTime.Now.Ticks - eta) / Progress;

                return new TimeSpan((long)(bpt * (Total - Progress)));
            }
        }

        public float Percentage
        {
            get
            {
                if (Total == 0) return 0.0f;
                else return 100.0f * Math.Min((float)Progress / (float)Total, 1.0f);
            }
        }

        public float Ratio
        {
            get
            {
                if (Total == 0) return 0.0f;
                else return Math.Min((float)Progress / (float)Total, 1.0f);
            }
        }

        public void Reset(long total, long stamp)
        {
            Progress = 0;
            this.Total = total;
            eta = DateTime.Now.Ticks;
        }
    }
}
