// Copyright 2010 Nejat Dilek  <imruon@gmail.com>
// Licensed under the terms of the GNU GPL, version 2
// http://www.gnu.org/licenses/old-licenses/gpl-2.0.txt
using System;
using System.Collections.Generic;
using System.Text;
using System.Xml;

namespace Org.Irduco.MultiLanguage
{
    public class MultiLanguageHelper
    {
        protected string language;
        protected string languageConfigFile;
        protected bool isLoaded;
        protected Dictionary<string, Dictionary<string, string>> languageConfig;

        public string LanguageConfigFile
        {
            get
            {
                return this.languageConfigFile;
            }
        }

        public string Language
        {
            get
            {
                return this.language;
            }
        }

        public bool IsLoaded
        {
            get
            {
                return this.isLoaded;
            }
        }

        public MultiLanguageHelper(string language, string languageConfigFile)
        {
            this.language = language;
            this.languageConfigFile = languageConfigFile;
            try
            {
                loadLanguageFile();
                this.isLoaded = true;
            }
            catch (Exception ex)
            {
                this.isLoaded = false;
            }
        }

        private void fillModule(Dictionary<string, Dictionary<string, string>> config, XmlDocument xmlDoc, XmlNode xmlNode)
        {
            Dictionary<string, string> moduleConfig = new Dictionary<string, string>();
            config.Add(xmlNode.Attributes["key"].Value, moduleConfig);

            foreach (XmlNode node in xmlNode.ChildNodes)
            {
                moduleConfig.Add(node.Attributes["key"].Value, node.InnerText);
            }

        }

        private void fillModules(Dictionary<string, Dictionary<string, string>> config, XmlDocument xmlDoc)
        {
            XmlNodeList nodeList = xmlDoc.SelectNodes("//LanguageResources/Module");

            foreach (XmlNode node in nodeList)
            {
                fillModule(config, xmlDoc, node);
            }
        }

        private void loadLanguageFile()
        {
            languageConfig = new Dictionary<string, Dictionary<string, string>>();

            XmlDocument xmlDoc = new XmlDocument();
            xmlDoc.Load(this.languageConfigFile);
            fillModules(languageConfig, xmlDoc);
        }

        public string Translate(string moduleName, string key)
        {
            Dictionary<string, string> moduleConfig = languageConfig[moduleName];
            if (moduleConfig.ContainsKey(key))
            {
                return moduleConfig[key];
            }
            else
            {
                return "[" + key + "]";
            }
        }
    }
}
