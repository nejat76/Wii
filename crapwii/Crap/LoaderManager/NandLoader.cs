﻿// Copyright 2010 Nejat Dilek  <imruon@gmail.com>
// Licensed under the terms of the GNU GPL, version 2
// http://www.gnu.org/licenses/old-licenses/gpl-2.0.txt
using System;
using System.Collections.Generic;
using System.Text;

namespace Org.Irduco.LoaderManager
{
    public class NandLoader
    {
        protected string version;
        protected string title;
        protected string filename;
        protected string author;
        protected string modder;
        protected int dolContentIndex;

        public string Title
        {
            get
            {
                return this.title;
            }
            set
            {
                this.title = value;
            }
        }

        public string Filename
        {
            get
            {
                return this.filename;
            }
            set
            {
                this.filename = value;
            }
        }

        public string Author
        {
            get
            {
                return this.author;
            }
            set
            {
                this.author = value;
            }
        }

        public string Modder
        {
            get
            {
                return this.modder;
            }
            set
            {
                this.modder = value;
            }
        }

        public string Version
        {
            get
            {
                return this.version;
            }
            set
            {
                this.version = value;
            }
        }

        public int DolContentIndex
        {
            get
            {
                return this.dolContentIndex;
            }
            set
            {
                this.dolContentIndex = value;
            }
        }

    }
}
