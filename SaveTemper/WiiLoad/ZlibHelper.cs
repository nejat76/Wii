using System;
using System.IO;
using System.Runtime.InteropServices;
namespace WiiLoad
{
    public static class ZlibHelper
    {
        public enum ZLibError : int
        {
            Z_OK = 0,
            Z_STREAM_END = 1,
            Z_NEED_DICT = 2,
            Z_ERRNO = (-1),
            Z_STREAM_ERROR = (-2),
            Z_DATA_ERROR = (-3),
            Z_MEM_ERROR = (-4),
            Z_BUF_ERROR = (-5),
            Z_VERSION_ERROR = (-6)
        }

        [DllImport("zlib1.dll")]
        public static extern ZLibError compress2(byte[] dest, ref int destLength, byte[] source, int sourceLength, int level);

        public static byte[] Compress(byte[] inFile)
        {
            ZLibError err;
            byte[] outFile = new byte[inFile.Length];
            int outLength = -1;

            err = compress2(outFile, ref outLength, inFile, inFile.Length, 6);

            if (err == ZLibError.Z_OK && outLength > -1)
            {
                Array.Resize(ref outFile, outLength);
                return outFile;
            }
            else
            {
                throw new Exception("An error occured while compressing! Code: " + err.ToString());
            }
        }
    }
}
