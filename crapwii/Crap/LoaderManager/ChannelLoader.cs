// Copyright 2010 Nejat Dilek  <imruon@gmail.com>
// Licensed under the terms of the GNU GPL, version 2
// http://www.gnu.org/licenses/old-licenses/gpl-2.0.txt
using System;
using System.Collections.Generic;
using System.Text;

namespace Org.Irduco.LoaderManager
{
    [Serializable]
    public class ChannelLoader : Loader
    {
        private string titleIdPlaceHolder;

        public string TitleIdPlaceHolder
        {
            get
            {
                return this.titleIdPlaceHolder;
            }
            set
            {
                this.titleIdPlaceHolder = value;
            }
        }

        public override bool IsForwarder
        {
            get
            {
                return false;
            }
        }
    }
}
