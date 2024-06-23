using System;
using System.Collections.Generic;
using System.Text;

namespace Org.Irduco.Text
{    
    public class WordStegoLib
    {
        List<string> wordList;
        Dictionary<int, Dictionary<string, string>> valueWordDict;

        public Dictionary<string,string> Words1
        {
            get
            {
                return this.valueWordDict[1];
            }
        }

        public Dictionary<string, string> Words2
        {
            get
            {
                return this.valueWordDict[2];
            }
        }

        public Dictionary<string, string> Words4
        {
            get
            {
                return this.valueWordDict[4];
            }
        }

        public Dictionary<string, string> Words8
        {
            get
            {
                return this.valueWordDict[8];
            }
        }

        public WordStegoLib(List<string> wordList)
        {
            this.wordList = wordList;
            initValueDict();
        }

        private void initValueDict() 
        {
            valueWordDict = new Dictionary<int, Dictionary<string,string>>();
            int wordCount = this.wordList.Count;
            int singleListCount = wordCount / 4;
            int i = 0;
            Dictionary<string, string> items1 = new Dictionary<string,string>();
            Dictionary<string, string> items2 = new Dictionary<string,string>();
            Dictionary<string, string> items4 = new Dictionary<string,string>();
            Dictionary<string, string> items8 = new Dictionary<string,string>();

            valueWordDict.Add(1, items1);
            valueWordDict.Add(2, items2);
            valueWordDict.Add(4, items4);
            valueWordDict.Add(8, items8);
            for (; i < singleListCount; i++) items1.Add(this.wordList[i], this.wordList[i]); 
            for (; i < singleListCount*2; i++) items2.Add(this.wordList[i], this.wordList[i]); 
            for (; i < singleListCount*3; i++) items4.Add(this.wordList[i], this.wordList[i]); 
            for (; i < singleListCount*4; i++) items8.Add(this.wordList[i], this.wordList[i]); 
        }

        public int ParseLine(string line, ref List<string> used)
        {
            string[] str = line.Split(' ');
            int value = 0;
            
            foreach (string s in str)
            {
                string sTrimmed = s.Replace(")","").Replace("(","").Replace(",", "").Trim().ToLowerInvariant();
                string sWithoutSuffix = sTrimmed.Replace("'s", "").Replace("'nt", "").Replace("'t", "");
                int oldValue = value;
   
                if (valueWordDict[1].ContainsKey(sTrimmed) )
                {
                    value = value + 1;
                    used.Add(sTrimmed + " = 1");
                }
                else if (valueWordDict[2].ContainsKey(sTrimmed))
                {
                    value = value + 2;
                    used.Add(sTrimmed + " = 2");
                }
                else if (valueWordDict[4].ContainsKey(sTrimmed))
                {
                    value = value + 4;
                    used.Add(sTrimmed + " = 4");
                }
                else if (valueWordDict[8].ContainsKey(sTrimmed))
                {
                    value = value + 8;
                    used.Add(sTrimmed + " = 8");
                }

                if (oldValue == value)
                {
                    if (valueWordDict[1].ContainsKey(sWithoutSuffix))
                    {
                        value = value + 1;
                        used.Add(sWithoutSuffix + " = 1");
                    }
                    else if (valueWordDict[2].ContainsKey(sWithoutSuffix))
                    {
                        value = value + 2;
                        used.Add(sWithoutSuffix + " = 2");
                    }
                    else if (valueWordDict[4].ContainsKey(sWithoutSuffix))
                    {
                        value = value + 4;
                        used.Add(sWithoutSuffix + " = 4");
                    }
                    else if (valueWordDict[8].ContainsKey(sWithoutSuffix))
                    {
                        value = value + 8;
                        used.Add(sWithoutSuffix + " = 8");
                    }
                }
            }

            return value%16;
        }

        public List<byte> ParseText(string text, ref List<string> used)
        {
            string[] lines = text.Split('.', '?','!');
            List<byte> values = new List<byte>();
            for (int i = 0; i < lines.Length; i++)
            {
                values.Add(Convert.ToByte(ParseLine(lines[i], ref used)));
            }

            return values;

        }

        public byte[] ParseTextAsData(string text, ref List<string> used)
        {
            byte[] bytes;
            int cCount;

            List<byte> hiddenData = ParseText(text, ref used);

            cCount = hiddenData.Count / 2;
            bool even = (hiddenData.Count % 2 == 0);
            if (!even) cCount++;

            bytes = new byte[cCount];

            for (int i = 0; i < cCount - 1; i++)
            {
                bytes[i] = Convert.ToByte(hiddenData[i * 2] * 16 + hiddenData[i * 2 + 1]);
            }

            if (even)
            {
                bytes[cCount - 1] = Convert.ToByte(hiddenData[hiddenData.Count - 2] * 16 + hiddenData[hiddenData.Count - 1]);
            }
            else
            {
                bytes[cCount - 1] = Convert.ToByte(hiddenData[hiddenData.Count - 1] * 16);
            }

            return bytes;
        }

    }
}
