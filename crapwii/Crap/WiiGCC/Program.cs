// Copyright 2010 Nejat Dilek  <imruon@gmail.com>
// Licensed under the terms of the GNU GPL, version 2
// http://www.gnu.org/licenses/old-licenses/gpl-2.0.txt
using System;
using System.Collections.Generic;
using System.Windows.Forms;
using System.Security.Policy;

namespace WiiGSC
{
    static class Program
    {
        /// <summary>
        /// The main entry point for the application.
        /// </summary>
        [STAThread]
        static void Main()
        {
	        // Create application domain setup information
            AppDomainSetup domaininfo = new AppDomainSetup();
            domaininfo.ApplicationBase = String.Format( "{0}", System.Environment.CurrentDirectory );
            //Create evidence for the new appdomain from evidence of the current application domain
            Evidence adevidence = AppDomain.CurrentDomain.Evidence;

            // Create appdomain
            AppDomain domain = AppDomain.CreateDomain("MyDomain", adevidence, domaininfo );

            //Write out application domain information
            Console.WriteLine( "Host domain: {0}", AppDomain.CurrentDomain.FriendlyName );
            Console.WriteLine( "child domain: {0}", domain.FriendlyName );
            Console.WriteLine();
            Console.WriteLine( "Application Base Directory is: {0}", domain.BaseDirectory );

            Application.EnableVisualStyles();
            Application.SetCompatibleTextRenderingDefault(false);
            Application.Run(new Form1(domain));
        }
    }
}
