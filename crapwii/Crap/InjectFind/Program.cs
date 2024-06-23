// Copyright 2010 Nejat Dilek  <imruon@gmail.com>
// Licensed under the terms of the GNU GPL, version 2
// http://www.gnu.org/licenses/old-licenses/gpl-2.0.txt
using System;
using System.Collections.Generic;
using System.Text;
using System.IO;

namespace InjectFind
{
    class Program
    {
        static int findArrayInArray(byte[] content, byte[] searchValue)
        {
            int i = 0;
        Label_0008:
            if (i < (content.Length - searchValue.Length))
            {
                int j = 0;
                while ((j < searchValue.Length) && (content[i + j] == searchValue[j]))
                {
                    j++;
                }
                if (j != searchValue.Length)
                {
                    i++;
                    goto Label_0008;
                }
            }
            if (i < (content.Length - searchValue.Length))
            {
                return i;
            }
            return -1;
        }

        static void Main(string[] args)
        {
            string dolFile = args[0];
            string targetDeclaration = args[1];
            byte[] search = new byte[] { 65, 65, 65, 65, 65,65, 65, 65, 65, 65,65, 65, 65, 65, 65,65 };
            byte[] dolFileBytes = File.ReadAllBytes(dolFile);
            int location = findArrayInArray(dolFileBytes, search);

            if (location >= 0)
            {
                string constant = "namespace CrapInstaller\r\n{\r\n";
                constant = constant + "\tclass Constant {\r\n";
                constant = constant + String.Format("\t\tpublic const int InjectionPosition = {0};\r\n", location);
                constant = constant + "\t}\r\n";
                constant = constant + "}";
                File.WriteAllText(targetDeclaration, constant);
                Console.Out.WriteLine(String.Format("Stub location constant ({0}) in {1} declared in file {2}", location, dolFile, targetDeclaration));
            }
            else
            {
                throw new Exception("Target byte array not found in the dol file!");
            }
        }
    }
}
