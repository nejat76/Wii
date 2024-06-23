// Copyright 2010 Nejat Dilek  <imruon@gmail.com>
// Licensed under the terms of the GNU GPL, version 2
// http://www.gnu.org/licenses/old-licenses/gpl-2.0.txt
using System;
using System.Collections.Generic;
using System.Text;

namespace Org.Irduco.UpdateManager
{
    public class GameInfo
    {
        private string discId;
        private string titleId;
        private string name;

        public string DiscId
        {
            get
            {
                return discId;
            }
        }

        public string TitleId
        {
            get
            {
                return titleId;
            }
        }

        public string Name
        {
            get
            {
                return name;
            }
        }

        public GameInfo(string discId, string titleId, string name)
        {
            this.discId = discId;
            this.titleId = titleId;
            this.name = name;
        }

        public override string ToString()
        {
            return this.name + " - " + this.titleId + " - " + this.discId;
        }

    }
}