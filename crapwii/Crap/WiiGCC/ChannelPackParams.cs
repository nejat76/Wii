// Copyright 2010 Nejat Dilek  <imruon@gmail.com>
// Licensed under the terms of the GNU GPL, version 2
// http://www.gnu.org/licenses/old-licenses/gpl-2.0.txt
using System;
using System.Collections.Generic;
using System.Text;

namespace WiiGSC
{
    public delegate void StatusUpdater(bool isFinal, int index, int type, String status);

    public class ChannelPackParams
    {
        // Fields
        public bool altDolEnabled;
        public string altDolFile;
        public uint altDolOffset;
        public int altDolType;
        public string[] banners;
        public int fixes;
        public bool forceLanguage;
        public bool forceLoader;
        public bool forceVideo;
        public string forwarderParameters;
        public bool isForwarder;
        public bool ocarinaEnabled;
        public bool regionOverrideEnabled;
        public int selectedAltDol;
        public byte selectedLanguage;
        public string selectedLoader;
        public int selectedPartition;
        public byte selectedRegion;
        public StatusUpdater updater;
        public bool verboseLog;
        public int wadNaming;

    public ChannelPackParams(string[] banners, bool regionOverrideEnabled, byte selectedRegion, bool forceVideo,  bool verboseLog,  bool ocarinaEnabled,  bool forceLanguage, byte selectedLanguage,  bool forceLoader, int fixes, string selectedLoader, int wadNaming, int altDolType, int selectedAltDol, uint altDolOffset, string altDolFile, int selectedPartition,  bool isForwarder, string forwarderParameters, StatusUpdater updater)
    {
        this.banners = banners;
        this.regionOverrideEnabled = regionOverrideEnabled;
        this.selectedRegion = selectedRegion;
        this.forceVideo = forceVideo;
        this.verboseLog = verboseLog;
        this.ocarinaEnabled = ocarinaEnabled;
        this.forceLanguage = forceLanguage;
        this.selectedLanguage = selectedLanguage;
        this.forceLoader = forceLoader;
        this.selectedLoader = selectedLoader;
        this.updater = updater;
        this.wadNaming = wadNaming;
        this.fixes = fixes;
        this.altDolType = altDolType;
        this.selectedAltDol = selectedAltDol;
        this.altDolOffset = altDolOffset;
        this.altDolFile = altDolFile;
        this.selectedPartition = selectedPartition;
        this.isForwarder = isForwarder;
        this.forwarderParameters = forwarderParameters;
    }

}

 

}
