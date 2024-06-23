using System;
using System.Collections.Generic;
using System.Text;

namespace Org.Irduco.MultiLanguage
{
    public class MultiLanguageModuleHelper
    {
        protected string moduleName;
        protected MultiLanguageHelper helper;

        public string ModuleName
        {
            get
            {
                return this.moduleName;
            }
        }

        public MultiLanguageModuleHelper(MultiLanguageHelper helper, string moduleName)
        {
            this.helper = helper;
            this.moduleName = moduleName;
        }

        public string Translate(string key)
        {
            return this.helper.Translate(this.moduleName, key);
        }

        public string TranslateAndReplace(string key, string searchString, string replaceString)
        {
            string translation = this.helper.Translate(this.moduleName, key);
            return translation.Replace(searchString, replaceString);
        }

    }
}
