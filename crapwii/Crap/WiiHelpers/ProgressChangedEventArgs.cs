/* This file is part of ShowMiiWads
 * Copyright (C) 2009 Leathl
 * 
 * ShowMiiWads is free software: you can redistribute it and/or
 * modify it under the terms of the GNU General Public License as published
 * by the Free Software Foundation, either version 3 of the License, or
 * (at your option) any later version.
 *
 * ShowMiiWads is distributed in the hope that it will be
 * useful, but WITHOUT ANY WARRANTY; without even the implied warranty
 * of MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
 * GNU General Public License for more details.
 *
 * You should have received a copy of the GNU General Public License
 * along with this program.  If not, see <http://www.gnu.org/licenses/>.
 */

//Wii.py by Xuzz, SquidMan, megazig, Matt_P, Omega and The Lemon Man was the base for TPL conversion
//Zetsubou by SquidMan was a reference for TPL conversion
//gbalzss by Andre Perrot was the base for LZ77 (de-)compression
//Thanks to the authors!

using System;
using System.IO;
using System.Net;

namespace Wii
{
    public class ProgressChangedEventArgs : EventArgs
    {
        private readonly int p_Percent = 0;

        public int PercentProgress
        {
            get { return p_Percent; }
        }

        internal ProgressChangedEventArgs(int PercentProgress)
            : base()
        {
            this.p_Percent = PercentProgress;
        }
    }
}