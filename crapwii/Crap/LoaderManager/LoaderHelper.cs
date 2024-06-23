// Copyright 2010 Nejat Dilek  <imruon@gmail.com>
// Licensed under the terms of the GNU GPL, version 2
// http://www.gnu.org/licenses/old-licenses/gpl-2.0.txt
using System;
using System.Collections.Generic;
using System.Text;
using System.Xml;

namespace Org.Irduco.LoaderManager
{
    public class LoaderHelper
    {
        private XmlDocument loaderConfigDocument;

        //Nand Loaders
        private Dictionary<string, NandLoader> nandLoadersDict;

        //Nand Loaders
        private List<NandLoader> nandLoaders;

        //General collection of below lists
        private List<BaseLoader> allLoaders;

        //Standalone loader list (for wii disc based games)
        private List<Loader> discLoaders;

        //Standalone loader list (for wii disc based games)
        private List<Loader> loaders;

        //Forwarder list (for wii disc based games)
        private List<Forwarder> forwarders;

        private List<ChannelLoader> channelLoaders;

        //Standalone loader list (for wiiware/vc games)
        private List<ChannelLoader> channelStandaloneLoaders;

        private List<ChannelForwarder> channelForwarders;

        #region List Accessors
        public List<NandLoader> NandLoaders
        {
            get
            {
                return this.nandLoaders;
            }
        }

        public List<BaseLoader> AllLoaders
        {
            get
            {
                return this.allLoaders;
            }
        }

        public List<Loader> DiscLoaders
        {
            get
            {
                return this.discLoaders;
            }
        }

        public List<Loader> Loaders
        {
            get
            {
                return this.loaders;
            }
        }

        public List<Forwarder> Forwarders
        {
            get
            {
                return this.forwarders;
            }
        }

        public List<ChannelLoader> ChannelLoaders
        {
            get
            {
                return this.channelLoaders;
            }
        }

        public List<ChannelLoader> ChannelStandaloneLoaders
        {
            get
            {
                return this.channelStandaloneLoaders;
            }
        }

        public List<ChannelForwarder> ChannelForwarders
        {
            get
            {
                return this.channelForwarders;
            }
        }

        public Dictionary<string, NandLoader> NandLoadersDict
        {
            get
            {
                return this.nandLoadersDict;
            }
        }

        #endregion

        public LoaderHelper(string loaderConfigPath)
        {
            this.loaderConfigDocument = loadLoaderConfiguration(loaderConfigPath);
            fillLoaders();
        }


        private void parseNandLoader(XmlNode nandLoaderItem, NandLoader loader)
        {
            XmlNodeList items = nandLoaderItem.ChildNodes;
            foreach(XmlNode node in items) 
            {
                switch (node.Name)
                {
                    case "Title": loader.Title = node.InnerText; break;
                    case "Filename": loader.Filename = node.InnerText; break;
                    case "Author": loader.Author = node.InnerText; break;
                    case "Modder": loader.Modder = node.InnerText; break;
                    case "DolContentIndex": loader.DolContentIndex = Convert.ToInt32(node.InnerText); break;
                }
            }
        }

        private void parseBaseLoader(XmlNode loaderItem, BaseLoader loader)
        {
            loader.Version = loaderItem.Attributes["version"].InnerText;
            XmlNodeList items = loaderItem.ChildNodes;
            foreach (XmlNode node in items)
            {
                switch (node.Name)
                {
                    case "Title": loader.Title = node.InnerText; break;
                    case "Filename": loader.Filename = node.InnerText; break;
                    case "Author": loader.Author = node.InnerText; break;
                    case "Modder": loader.Modder = node.InnerText; break;
                    case "ConfigPlaceHolder": loader.ConfigPlaceHolder = node.InnerText;  break;
                }
            }
        }

        private void parseLoader(XmlNode loaderItem, Loader loader)
        {
            XmlNodeList items = loaderItem.ChildNodes;
            foreach (XmlNode node in items)
            {
                switch (node.Name)
                {
                    case "DiscIdPlaceHolder": loader.DiscIdPlaceHolder = node.InnerText; break;
                    case "RegionOverride": loader.RegionOverride = (node.InnerText != null && node.InnerText == "1"); break;
                    case "VerboseLogSupport": loader.VerboseLogSupport = (node.InnerText != null && node.InnerText == "1"); break;
                    case "ForcedVideoModeSelection": loader.ForcedVideoModeSelection = (node.InnerText != null && node.InnerText == "1"); break;
                    case "OcarinaConfiguration": loader.OcarinaConfiguration = (node.InnerText != null && node.InnerText == "1"); break;
                    case "LanguageSelection": loader.LanguageSelection = (node.InnerText != null && node.InnerText == "1"); break;
                    case "SupportsSdSdhcCard": loader.SupportsSdSdhcCard = (node.InnerText != null && node.InnerText == "1"); break;
                    case "SupportFixes": loader.SupportFixes = (node.InnerText != null && node.InnerText == "1"); break;
                    case "SupportsAltDols": loader.SupportsAltDols = (node.InnerText != null && node.InnerText == "1"); break;
                    case "SupportsPartitionSelection": loader.SupportsPartitionSelection = (node.InnerText != null && node.InnerText == "1"); break;
                }
            }
        }

        private void parseForwarder(XmlNode loaderItem, Forwarder loader)
        {
            XmlNodeList items = loaderItem.ChildNodes;
            foreach (XmlNode node in items)
            {
                switch (node.Name)
                {
                    case "SupportedParameters": loader.SupportedParameters = node.InnerText; break;
                }
            }
        }

        private void parseChannelLoader(XmlNode loaderItem, ChannelLoader loader)
        {
            XmlNodeList items = loaderItem.ChildNodes;
            foreach (XmlNode node in items)
            {
                switch (node.Name)
                {
                    case "TitleIdPlaceHolder": loader.TitleIdPlaceHolder = node.InnerText; break;
                }
            }
        }

        private void fillLoaders()
        {
            string baseNodePath = "//LoaderConfig/";
            XmlNodeList nodeList = loaderConfigDocument.SelectNodes(baseNodePath + "NandLoaders/NandLoader");

            nandLoaders = new List<NandLoader>();
            loaders = new List<Loader>();
            channelStandaloneLoaders = new List<ChannelLoader>();
            channelLoaders = new List<ChannelLoader>();
            discLoaders = new List<Loader>();
            allLoaders = new List<BaseLoader>();
            forwarders = new List<Forwarder>();
            channelLoaders = new List<ChannelLoader>();
            channelForwarders = new List<ChannelForwarder>();
            nandLoadersDict = new Dictionary<string, NandLoader>();

            foreach (XmlNode node in nodeList)
            {
                NandLoader nandLoader = new NandLoader();
                parseNandLoader(node, nandLoader);
                nandLoaders.Add(nandLoader);
                nandLoadersDict.Add(nandLoader.Filename, nandLoader);
            }

            nodeList = loaderConfigDocument.SelectNodes(baseNodePath + "StandaloneLoaders/Loader");

            foreach (XmlNode node in nodeList)
            {
                Loader loader = new Loader();
                parseBaseLoader(node, loader);
                parseLoader(node, loader);
                allLoaders.Add(loader);                
                discLoaders.Add(loader);
                loaders.Add(loader);
            }

            nodeList = loaderConfigDocument.SelectNodes(baseNodePath + "Forwarders/Forwarder");

            foreach (XmlNode node in nodeList)
            {
                Forwarder loader = new Forwarder();
                parseBaseLoader(node, loader);
                parseLoader(node, loader);
                parseForwarder(node, loader);
                allLoaders.Add(loader);                
                discLoaders.Add(loader);
                forwarders.Add(loader);
            }

            nodeList = loaderConfigDocument.SelectNodes(baseNodePath + "ChannelStandaloneLoaders/Loader");

            foreach (XmlNode node in nodeList)
            {
                ChannelLoader loader = new ChannelLoader();
                parseBaseLoader(node, loader);
                parseChannelLoader(node, loader);
                allLoaders.Add(loader);
                channelLoaders.Add(loader);
                channelStandaloneLoaders.Add(loader);                
            }

            nodeList = loaderConfigDocument.SelectNodes(baseNodePath + "ChannelForwarders/Forwarder");

            foreach (XmlNode node in nodeList)
            {
                ChannelForwarder loader = new ChannelForwarder();
                parseBaseLoader(node, loader);
                parseChannelLoader(node, loader);
                allLoaders.Add(loader);
                channelLoaders.Add(loader);
                channelForwarders.Add(loader);                
            }

        }

        private XmlDocument loadLoaderConfiguration(string loaderConfigPath)
        {
            XmlDocument xmlDoc = new XmlDocument();
            xmlDoc.Load(loaderConfigPath);
            return xmlDoc;
        }
    }
}
