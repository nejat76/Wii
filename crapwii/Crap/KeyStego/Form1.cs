// Copyright 2010 Nejat Dilek  <imruon@gmail.com>
// Licensed under the terms of the GNU GPL, version 2
// http://www.gnu.org/licenses/old-licenses/gpl-2.0.txt
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Text;
using System.Windows.Forms;
using System.IO;
using Org.Irduco.Text;

namespace KeyStego
{
    public partial class Form1 : Form
    {
        private WordStegoLib wordStegoLib;
        public Form1()
        {
            InitializeComponent();
        }

        private void label4_Click(object sender, EventArgs e)
        {

        }

        private void listBox3_SelectedIndexChanged(object sender, EventArgs e)
        {

        }

        private void Form1_Load(object sender, EventArgs e)
        {
            InitStegoLib();
            FillLists();
        }

        protected void InitStegoLib()
        {
            string[] allLines = File.ReadAllLines("Words.txt");
            List<string> allLinesColl = new List<string>(allLines);
            for(int i=0;i<allLinesColl.Count;i++) 
            {
                allLinesColl[i] = allLinesColl[i].Trim().ToLowerInvariant(); ;
            }
            wordStegoLib = new WordStegoLib(allLinesColl);
        }

        protected void FillLists()
        {
            words1.Items.Clear();
            foreach(string key in wordStegoLib.Words1.Keys) 
            {
                words1.Items.Add(key);
            }

            words2.Items.Clear();
            foreach(string key in wordStegoLib.Words2.Keys) 
            {
                words2.Items.Add(key);
            }

            words4.Items.Clear();
            foreach(string key in wordStegoLib.Words4.Keys) 
            {
                words4.Items.Add(key);
            }

            words8.Items.Clear();
            foreach(string key in wordStegoLib.Words8.Keys) 
            {
                words8.Items.Add(key);
            }

        }

        private void carrier_TextChanged(object sender, EventArgs e)
        {
            List<string> used = new List<string>();
            List<byte> hiddenData = wordStegoLib.ParseText(carrier.Text, ref used);
            remarkedWords.Items.Clear();
            for (int i = used.Count; i > 0; i--)
            {
                remarkedWords.Items.Add(used[i-1]);
            }

            StringBuilder sb = new StringBuilder();

            for (int i = 0; i < hiddenData.Count;i++ )
            {
                sb.AppendFormat("{0:X}", hiddenData[i]);
                if (i % 2>0)
                {
                    sb.Append(" ");
                }
            }

            hiddenHex.Text = sb.ToString();

            String s = String.Empty;
            
            byte[] bytes;
            int cCount;
            cCount = hiddenData.Count/2;
            bool even = (hiddenData.Count%2==0);
            if (!even) cCount++;

            bytes = new byte[cCount];

            for (int i = 0; i < cCount-1; i++)
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

            hiddenText.Text = Encoding.ASCII.GetString(bytes);

        }

        private void button1_Click(object sender, EventArgs e)
        {
            DialogResult result = saveFileDialog1.ShowDialog();
            if (result == DialogResult.OK)
            {
                try
                {
                    List<string> used = new List<string>();
                    byte[] hiddenData = wordStegoLib.ParseTextAsData(carrier.Text, ref used);

                    string fileName = saveFileDialog1.FileName;
                    File.WriteAllBytes(fileName, hiddenData);
                }
                catch (Exception ex)
                {
                    MessageBox.Show("Oops, can't do it : " + ex.ToString(), "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                }
            }

        }

    }
}
