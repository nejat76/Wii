// Copyright 2010 Nejat Dilek  <imruon@gmail.com>
// Licensed under the terms of the GNU GPL, version 2
// http://www.gnu.org/licenses/old-licenses/gpl-2.0.txt
using System;
using System.IO;
using System.IO.Compression;
using System.Security.Cryptography;
using System.Text;
using System.CodeDom.Compiler;
using System.Collections.Generic;
using Org.Irduco.CommonHelpers;


namespace CrapInstaller
{
    public class InstallerHelper
    {
        public static MemoryStream CreateInstaller(byte[] wadFileBytes, byte iosToUse)
        {
            //const int injectionPosition = 0x852F8;
            const int maxAllowedSizeForWads = 4 * 1024 * 1024 - 32; //(Max 4MB-32bytes )

            //0. Read length of the wad to ensure it has an allowed size
            uint wadLength = (uint)wadFileBytes.Length;

            if (wadLength > maxAllowedSizeForWads)
            {
                throw new ArgumentException(String.Format("The file is sized above the max allowed limit of {1} for network installation.", maxAllowedSizeForWads));
            }

            //1. Open the stub installer from resources
            MemoryStream compressedStubInstallerStream = LoadCompressedStubInstaller("CrapInstaller.dol.z");
            compressedStubInstallerStream.Seek(0, SeekOrigin.Begin);

            //2. Decompress compressed installer
            MemoryStream uncompressedStubInstallerStream = new MemoryStream();

            using (GZipStream gzipStream = new GZipStream(compressedStubInstallerStream, CompressionMode.Decompress))
            {
                byte[] decompressedBuff = new byte[1024];
                while (true)
                {
                    int length = gzipStream.Read(decompressedBuff, 0, 1024);

                    if (length == 0) break;

                    uncompressedStubInstallerStream.Write(decompressedBuff, 0, length);
                }

            }

            //3. Take SHA of the wad and store it in the stub installer along with the size of the wad

            byte[] shaHash;
            using (SHA1 shaGen = SHA1.Create())
            {
                shaHash = shaGen.ComputeHash(wadFileBytes);
            }

            //4. Inject the data into the installer

            //Write out the wad size
            uncompressedStubInstallerStream.Seek(Constant.InjectionPosition, SeekOrigin.Begin);
            uncompressedStubInstallerStream.WriteByte((byte)((wadLength >> 24) & 0xff));
            uncompressedStubInstallerStream.WriteByte((byte)((wadLength >> 16) & 0xff));
            uncompressedStubInstallerStream.WriteByte((byte)((wadLength >> 8) & 0xff));
            uncompressedStubInstallerStream.WriteByte((byte)(wadLength & 0xff));

            //Write out the SHA1 value (Against corruption of the file on the network, this value will be checked by the installer)
            uncompressedStubInstallerStream.Write(shaHash, 0, 20);

            //Write out the ios to be used... 
            uncompressedStubInstallerStream.WriteByte(iosToUse);

            //pad it with three zeroes (to align it into 32-bit)
            uncompressedStubInstallerStream.WriteByte(0); uncompressedStubInstallerStream.WriteByte(0); uncompressedStubInstallerStream.WriteByte(0);


            //Write out to be installed wad file's contents...
            uncompressedStubInstallerStream.Write(wadFileBytes, 0, (int)wadLength);

            return uncompressedStubInstallerStream;

        }

        public static MemoryStream CreateInstaller(string wadFile, byte iosToUse)
        {
            byte[] wadFileBytes = File.ReadAllBytes(wadFile);
            return CreateInstaller(wadFileBytes, iosToUse);
        }

        private static MemoryStream LoadCompressedStubInstaller(string installerResourceName)
        {
            using (BinaryReader resLoader = new BinaryReader(System.Reflection.Assembly.GetExecutingAssembly().GetManifestResourceStream("CrapInstaller.Resources." + installerResourceName)))
            {
                MemoryStream ms = new MemoryStream();
                byte[] temp = resLoader.ReadBytes((int)resLoader.BaseStream.Length);
                ms.Write(temp, 0, temp.Length);
                return ms;
            }
        }

        public static MemoryStream CreateCompressedInstaller(MemoryStream rawInstallerStream)
        {
            MemoryStream compressedStream = new MemoryStream();

            GZipStream gzipStream = new GZipStream(compressedStream, CompressionMode.Compress);
                           
            byte[] rawData = rawInstallerStream.ToArray();
            gzipStream.Write(rawData, 0, rawData.Length);

            return compressedStream;
        }

        public static byte[] CreateCompressedInstallerUsingZlib(MemoryStream rawInstallerStream)
        {
            return ZlibHelper.Compress(rawInstallerStream.ToArray());
        }

        public static MemoryStream CreateCompressedInstallerUsingManagedZlib(MemoryStream rawInstallerStream)
        {
            MemoryStream compressedStream = new MemoryStream();

            Ionic.Zlib.GZipStream gzipStream = new Ionic.Zlib.GZipStream(compressedStream, Ionic.Zlib.CompressionMode.Compress);

            byte[] rawData = rawInstallerStream.ToArray();
            gzipStream.Write(rawData, 0, rawData.Length);

            return compressedStream;
        }

        public static MemoryStream CreateCompressedInstallerUsingDeflate(MemoryStream rawInstallerStream)
        {
            MemoryStream compressedStream = new MemoryStream();

            DeflateStream gzipStream = new DeflateStream(compressedStream, CompressionMode.Compress);

            byte[] rawData = rawInstallerStream.ToArray();
            gzipStream.Write(rawData, 0, rawData.Length);

            return compressedStream;
        }

        public static string CreateSelfInstaller(string appPath, string defaultIp, string wadName, byte[] compressedInstallerDol, int uncompressedSize, int iosToUse)
        {
            return CreateSelfInstaller(appPath, defaultIp, wadName, compressedInstallerDol, uncompressedSize, iosToUse, "");
        }

        public static string CreateSelfInstaller(string appPath, string defaultIp, string wadName, byte[] compressedInstallerDol, int uncompressedSize, int iosToUse, string outputFilename)
        {
            if (appPath != String.Empty)
            {
                Directory.SetCurrentDirectory(appPath);
            }

            SpitInstallerMetaData(defaultIp, wadName, compressedInstallerDol, uncompressedSize, iosToUse);

            CompilerResults cr;

            CodeDomProvider prov = null;

            prov = CodeDomProvider.CreateProvider("cs");

            CompilerParameters parameters = new CompilerParameters();
            parameters.ReferencedAssemblies.Add("system.dll");
            parameters.ReferencedAssemblies.Add("system.data.dll");
            parameters.ReferencedAssemblies.Add("System.Drawing.dll");
            parameters.ReferencedAssemblies.Add("System.Windows.Forms.dll");
            parameters.ReferencedAssemblies.Add("System.Xml.dll");
            parameters.IncludeDebugInformation = false;
            parameters.EmbeddedResources.Add("InstallerStub\\Resources\\installer.z");
            parameters.GenerateExecutable = true;
            parameters.GenerateInMemory = false;
            parameters.CompilerOptions = "/target:winexe";

            string outputAssembly = wadName.Replace(".wad", ".exe");
            if (outputFilename == String.Empty)
            {
                parameters.OutputAssembly = "wad\\" + outputAssembly;
            }
            else
            {
                parameters.OutputAssembly = outputFilename;
            }
            List<string> filenames = new List<string>();
            filenames.Add("InstallerStub\\Info.cs");
            filenames.Add("InstallerStub\\Form1.Designer.cs");
            filenames.Add("InstallerStub\\Form1.cs");
            filenames.Add("InstallerStub\\Program.cs");

            cr = prov.CompileAssemblyFromFile(parameters, filenames.ToArray());

            if (cr.Errors.HasErrors)
            {
                StringBuilder error = new StringBuilder();
                error.Append("Error Compiling Expression: \n");
                foreach (CompilerError err in cr.Errors)
                {
                    error.AppendFormat("Line {0}: {1}\n", err.Line, err.ErrorText);
                }
                throw new Exception(error.ToString());
            }

            return outputAssembly;
        }

        public static void CreateSelfInstallerWithName(string appPath, string defaultIp, string wadName, byte[] compressedInstallerDol, int uncompressedSize, int iosToUse, string outputFilename)
        {
            if (appPath != String.Empty)
            {
                Directory.SetCurrentDirectory(appPath);
            }

            SpitInstallerMetaData(defaultIp, wadName, compressedInstallerDol, uncompressedSize, iosToUse);

            CompilerResults cr;

            CodeDomProvider prov = null;

            prov = CodeDomProvider.CreateProvider("cs");

            CompilerParameters parameters = new CompilerParameters();
            parameters.ReferencedAssemblies.Add("system.dll");
            parameters.ReferencedAssemblies.Add("system.data.dll");
            parameters.ReferencedAssemblies.Add("System.Drawing.dll");
            parameters.ReferencedAssemblies.Add("System.Windows.Forms.dll");
            parameters.ReferencedAssemblies.Add("System.Xml.dll");
            parameters.IncludeDebugInformation = false;
            parameters.EmbeddedResources.Add("InstallerStub\\Resources\\installer.z");
            parameters.GenerateExecutable = true;
            parameters.GenerateInMemory = false;
            parameters.CompilerOptions = "/target:winexe";


            parameters.OutputAssembly = outputFilename;

            List<string> filenames = new List<string>();
            filenames.Add("InstallerStub\\Info.cs");
            filenames.Add("InstallerStub\\Form1.Designer.cs");
            filenames.Add("InstallerStub\\Form1.cs");
            filenames.Add("InstallerStub\\Program.cs");

            cr = prov.CompileAssemblyFromFile(parameters, filenames.ToArray());

            if (cr.Errors.HasErrors)
            {
                StringBuilder error = new StringBuilder();
                error.Append("Error Compiling Expression: \n");
                foreach (CompilerError err in cr.Errors)
                {
                    error.AppendFormat("Line {0}: {1}\n", err.Line, err.ErrorText);
                }
                throw new Exception(error.ToString());
            }
        }

        private static void SpitInstallerMetaData(string defaultIp, string wadName, byte[] compressedInstallerDol, int uncompressedSize, int iosToUse)
        {
            StringBuilder sb = new StringBuilder();
            sb.Append("using System;\n");
            sb.Append("namespace InstallerStub\n");
            sb.Append("{\n");
            sb.Append("public class Info\n");
            sb.Append("{\n");
            sb.Append("public static string WadName = \""); sb.Append(wadName); sb.Append("\";\n");
            sb.Append("public static int UncompressedSize = ");sb.Append(uncompressedSize); sb.Append(";\n");
            sb.Append("public static int CompressedSize = ");sb.Append(compressedInstallerDol.Length); sb.Append(";\n");
            sb.Append("public static string DefaultIp = \""); sb.Append(defaultIp); sb.Append("\";\n");
            sb.Append("public static string DefaultIos = \""); sb.Append(iosToUse); sb.Append("\";\n");
            sb.Append("}\n");
            sb.Append("}\n");
            string metaDataClass = sb.ToString();

            File.WriteAllText("InstallerStub\\Info.cs", metaDataClass);
            File.WriteAllBytes("InstallerStub\\Resources\\installer.z", compressedInstallerDol);

        }

    }
}
