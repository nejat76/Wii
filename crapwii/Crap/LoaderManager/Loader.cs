// Copyright 2010 Nejat Dilek  <imruon@gmail.com>
// Licensed under the terms of the GNU GPL, version 2
// http://www.gnu.org/licenses/old-licenses/gpl-2.0.txt
using System;
using System.Collections.Generic;
using System.Text;

namespace Org.Irduco.LoaderManager
{
    [Serializable]
    public class Loader : BaseLoader
    {
        protected string discIdPlaceHolder;
        protected bool regionOverride = false;
        protected bool verboseLogSupport = false;
        protected bool forcedVideoModeSelection = false;
        protected bool ocarinaConfiguration = false; 
        protected bool languageSelection = false;
        protected bool supportsSdSdhcCard = false;
        protected bool supportFixes = false;
        protected bool supportsAltDols = false;
        protected bool supportsPartitionSelection = false;

        public string DiscIdPlaceHolder
        {
            get
            {
                return this.discIdPlaceHolder;
            }
            set
            {
                this.discIdPlaceHolder = value;
            }
        }

        public bool RegionOverride
        {
            get
            {
                return this.regionOverride;
            }
            set
            {
                this.regionOverride = value;
            }
        }

        public bool VerboseLogSupport
        {
            get
            {
                return this.verboseLogSupport;
            }
            set
            {
                this.verboseLogSupport = value;
            }
        }

        public bool ForcedVideoModeSelection
        {
            get
            {
                return this.forcedVideoModeSelection;
            }
            set
            {
                this.forcedVideoModeSelection = value;
            }
        }

        public bool OcarinaConfiguration
        {
            get
            {
                return this.ocarinaConfiguration;
            }
            set
            {
                this.ocarinaConfiguration = value;
            }
        }

        public bool LanguageSelection
        {
            get
            {
                return this.languageSelection;
            }
            set
            {
                this.languageSelection = value;
            }
        }

        public bool SupportsSdSdhcCard
        {
            get
            {
                return this.supportsSdSdhcCard;
            }
            set
            {
                this.supportsSdSdhcCard = value;
            }
        }

        public bool SupportFixes
        {
            get
            {
                return this.supportFixes;
            }
            set
            {
                this.supportFixes = value;
            }
        }

        public bool SupportsAltDols
        {
            get
            {
                return this.supportsAltDols;
            }
            set
            {
                this.supportsAltDols = value;
            }
        }

        public bool SupportsPartitionSelection
        {
            get
            {
                return this.supportsPartitionSelection;
            }
            set
            {
                this.supportsPartitionSelection = value;
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