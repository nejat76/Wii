// Copyright 2010 Nejat Dilek  <imruon@gmail.com>
// Licensed under the terms of the GNU GPL, version 2
// http://www.gnu.org/licenses/old-licenses/gpl-2.0.txt

using System;
using System.Collections.Generic;
using System.Text;
using System.Net.Sockets;
using System.IO;

namespace WiiLoad
{
    public class WiiLoad
    {
        public delegate void WiiLoadProgress(int totalSize, int completedSize, string status, string error);

        public bool SendFile(string txtIpAddress, byte[] fileContent, int fileSize, WiiLoadProgress progressHandler, string[] args)
        {
            return SendCompressedFile(txtIpAddress, ZlibHelper.Compress(fileContent), fileContent.Length, progressHandler,args);
        }

        public bool SendCompressedFile(string txtIpAddress, byte[] fileContent, int fileSize, WiiLoadProgress progressHandler, string[] args)
        {
            TcpClient client = new TcpClient();
            NetworkStream stream = null;
            int compressedFileSize;

            MemoryStream argStream = new MemoryStream();

            for (int i = 0; i < args.Length; i++)
            {
                argStream.Write(Encoding.ASCII.GetBytes(args[i]), 0, args[i].Length);
                argStream.Write(new byte[] { 0 }, 0, 1);
            }

            byte[] argsBuffer = argStream.ToArray();

            try
            {
                compressedFileSize = fileContent.Length;
                int blockSize = 4 * 1024;
                byte[] buffer = new byte[16];
                //byte[] argsBuffer = new byte[14];
                string ipAddress = txtIpAddress;
                string[] ipBytes = ipAddress.Split('.');


                progressHandler.Invoke(0, 0, "Connecting to " + ipAddress + ":4299...", "");
                client.Connect(ipAddress, 4299);
                progressHandler.Invoke(0, 0, "Connected to " + ipAddress + ":4299", "");
                
                //Send Magic
                buffer[0] = 0x48;
                buffer[1] = 0x41;
                buffer[2] = 0x58;
                buffer[3] = 0x58;

                stream = client.GetStream();

                progressHandler.Invoke(0, 0, "Magic sent...", "");
                //Send Version Info
                buffer[4] = 0;
                buffer[5] = 5;
                buffer[6] = (byte)(argsBuffer.Length >> 8);
                buffer[7] = (byte) (argsBuffer.Length & 255);
                //argsBuffer[0] = argsBuffer[1] = argsBuffer[2] = argsBuffer[3] = argsBuffer[4] = argsBuffer[5] = argsBuffer[6] = 0x30;
                //argsBuffer[7] = 0x31;
                //argsBuffer[8] = 0x2E;
                //argsBuffer[9] = 0x64;
                //argsBuffer[10] = 0x6F;
                //argsBuffer[11] = 0x6C;
                //argsBuffer[12] = 0x00;
                //argsBuffer[13] = 0x00;

                progressHandler.Invoke(0, 0, "Sent version info...", "");

                //Send File Size
                buffer[8] = (byte)((compressedFileSize >> 24) & 0xff);
                buffer[9] = (byte)((compressedFileSize >> 16) & 0xff);
                buffer[10] = (byte)((compressedFileSize >> 8) & 0xff);
                buffer[11] = (byte)(compressedFileSize & 0xff);

                //Send Uncompressed file size
                buffer[12] = (byte)((fileSize >> 24) & 0xff);
                buffer[13] = (byte)((fileSize >> 16) & 0xff);
                buffer[14] = (byte)((fileSize >> 8) & 0xff);
                buffer[15] = (byte)(fileSize & 0xff);

                stream.Write(buffer, 0, 16);

                progressHandler.Invoke(0, 0, "Sending file...", "");
                int offset = 0;
                int current = 0;
                int count = (int)(compressedFileSize / blockSize);
                int leftOver = (int)(compressedFileSize % blockSize);

                while (current < count)
                {
                    stream.Write(fileContent, offset, blockSize);
                    offset += blockSize;
                    progressHandler.Invoke(count, current, "Sending file..." + (current + 1).ToString() + " / " + count, "");
                    current++;
                }

                if (leftOver > 0)
                {
                    stream.Write(fileContent, offset, compressedFileSize - offset);
                }

                stream.Write(argsBuffer, 0, argsBuffer.Length);

                progressHandler.Invoke(0, 0, "Finished Sending!", "");
                return true;
            }
            catch (Exception ex)
            {
                progressHandler.Invoke(0, 0, "Failed!", ex.Message);
                return false;
            }
            finally
            {
                if ((client != null) && (client.Connected))
                {
                    if (stream != null)
                    {
                        stream.Close();
                    }
                    client.Close();
                }
            }

        }

    }
}
