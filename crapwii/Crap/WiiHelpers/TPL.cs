/* This file is part of ShowMiiWads
 * Copyright (C) 2009 Leathl
 * 
 * ShowMiiWads is free software: you can redistribute it and/or
 * modify it under the terms of the GNU General Public License as published
 * by the Free Software Foundation, either version 3 of the License, or
 * (at your option) any later version.
 *
 * ShowMiiWads is distributed in the hope that it will be
 * useful, but WITHOUT ANY WARRANTY; without even the implied warranty
 * of MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
 * GNU General Public License for more details.
 *
 * You should have received a copy of the GNU General Public License
 * along with this program.  If not, see <http://www.gnu.org/licenses/>.
 */

//Wii.py by Xuzz, SquidMan, megazig, Matt_P, Omega and The Lemon Man was the base for TPL conversion
//Zetsubou by SquidMan was a reference for TPL conversion
//gbalzss by Andre Perrot was the base for LZ77 (de-)compression
//Thanks to the authors!

using System;
using System.Collections.Generic;
using System.IO;
using System.Net;
using System.Security.Cryptography;
using System.Text;
using System.Drawing;

namespace Wii
{

    public class TPL
    {
        /// <summary>
        /// Fixes rough edges (artifacts), if necessary
        /// </summary>
        /// <param name="tplFile"></param>
        public static void FixFilter(string tplFile)
        {
            using (FileStream fs = new FileStream(tplFile, FileMode.Open))
            {
                fs.Seek(41, SeekOrigin.Begin);
                if (fs.ReadByte() == 0x01)
                {
                    fs.Seek(-1, SeekOrigin.Current);
                    fs.Write(new byte[] { 0x00, 0x00, 0x01 }, 0, 3);
                }

                fs.Seek(45, SeekOrigin.Begin);
                if (fs.ReadByte() == 0x01)
                {
                    fs.Seek(-1, SeekOrigin.Current);
                    fs.Write(new byte[] { 0x00, 0x00, 0x01 }, 0, 3);
                }
            }
        }

        /// <summary>
        /// Converts a Tpl to a Bitmap
        /// </summary>
        /// <param name="tpl"></param>
        /// <returns></returns>
        public static Bitmap ConvertFromTPL(string tpl)
        {
            byte[] tplarray = Wii.Tools.LoadFileToByteArray(tpl);
            return ConvertFromTPL(tplarray);
        }

        /// <summary>
        /// Converts a Tpl to a Bitmap
        /// </summary>
        /// <param name="tpl"></param>
        /// <returns></returns>
        public static Bitmap ConvertFromTPL(byte[] tpl)
        {
            if (GetTextureCount(tpl) > 1) throw new Exception("Tpl's containing more than one Texture are not supported!");

            int width = GetTextureWidth(tpl);
            int height = GetTextureHeight(tpl);
            int format = GetTextureFormat(tpl);
            if (format == -1) throw new Exception("The Texture has an unsupported format!");

            switch (format)
            {
                case 0:
                    byte[] temp0 = FromI4(tpl);
                    return ConvertPixelToBitmap(temp0, width, height);
                case 1:
                    byte[] temp1 = FromI8(tpl);
                    return ConvertPixelToBitmap(temp1, width, height);
                case 2:
                    byte[] temp2 = FromIA4(tpl);
                    return ConvertPixelToBitmap(temp2, width, height);
                case 3:
                    byte[] temp3 = FromIA8(tpl);
                    return ConvertPixelToBitmap(temp3, width, height);
                case 4:
                    byte[] temp4 = FromRGB565(tpl);
                    return ConvertPixelToBitmap(temp4, width, height);
                case 5:
                    byte[] temp5 = FromRGB5A3(tpl);
                    return ConvertPixelToBitmap(temp5, width, height);
                case 6:
                    byte[] temp6 = FromRGBA8(tpl);
                    return ConvertPixelToBitmap(temp6, width, height);
                case 14:
                    byte[] temp14 = FromCMP(tpl);
                    return ConvertPixelToBitmap(temp14, width, height);
                default:
                    throw new Exception("The Texture has an unsupported format!");
            }
        }

        /// <summary>
        /// Converts the Pixel Data into a Png Image
        /// </summary>
        /// <param name="data">Byte array with pixel data</param>
        public static System.Drawing.Bitmap ConvertPixelToBitmap(byte[] data, int width, int height)
        {
            if (width == 0) width = 1;
            if (height == 0) height = 1;

            System.Drawing.Bitmap bmp = new System.Drawing.Bitmap(width, height, System.Drawing.Imaging.PixelFormat.Format32bppArgb);
            System.Drawing.Imaging.BitmapData bmpData = bmp.LockBits(
                                 new System.Drawing.Rectangle(0, 0, bmp.Width, bmp.Height),
                                 System.Drawing.Imaging.ImageLockMode.WriteOnly, bmp.PixelFormat);

            System.Runtime.InteropServices.Marshal.Copy(data, 0, bmpData.Scan0, data.Length);
            bmp.UnlockBits(bmpData);
            return bmp;
        }

        /// <summary>
        /// Gets the Number of Textures in a Tpl
        /// </summary>
        /// <param name="tpl"></param>
        /// <returns></returns>
        public static int GetTextureCount(byte[] tpl)
        {
            byte[] tmp = new byte[4];
            tmp[3] = tpl[4];
            tmp[2] = tpl[5];
            tmp[1] = tpl[6];
            tmp[0] = tpl[7];
            UInt32 count = BitConverter.ToUInt32(tmp, 0);
            return (int)count;
        }

        /// <summary>
        /// Gets the Format of the Texture in the Tpl
        /// </summary>
        /// <param name="tpl"></param>
        /// <returns></returns>
        public static int GetTextureFormat(string tpl)
        {
            byte[] temp = Tools.LoadFileToByteArray(tpl, 0, 50);
            return GetTextureFormat(temp);
        }

        /// <summary>
        /// Gets the Format of the Texture in the Tpl
        /// </summary>
        /// <param name="tpl"></param>
        /// <returns></returns>
        public static int GetTextureFormat(byte[] tpl)
        {
            byte[] tmp = new byte[4];
            tmp[3] = tpl[24];
            tmp[2] = tpl[25];
            tmp[1] = tpl[26];
            tmp[0] = tpl[27];
            UInt32 format = BitConverter.ToUInt32(tmp, 0);

            if (format == 0 ||
                format == 1 ||
                format == 2 ||
                format == 3 ||
                format == 4 ||
                format == 5 ||
                format == 6 ||
                format == 14) return (int)format;

            else return -1; //Unsupported Format
        }

        /// <summary>
        /// Gets the Format Name of the Texture in the Tpl
        /// </summary>
        /// <param name="tpl"></param>
        /// <returns></returns>
        public static string GetTextureFormatName(byte[] tpl)
        {
            switch (GetTextureFormat(tpl))
            {
                case 0:
                    return "I4";
                case 1:
                    return "I8";
                case 2:
                    return "IA4";
                case 3:
                    return "IA8";
                case 4:
                    return "RGB565";
                case 5:
                    return "RGB5A3";
                case 6:
                    return "RGBA8";
                case 14:
                    return "CMP";
                default:
                    return "Unknown";
            }
        }

        public static int avg(int w0, int w1, int c0, int c1)
        {
            int a0 = c0 >> 11;
            int a1 = c1 >> 11;
            int a = (w0 * a0 + w1 * a1) / (w0 + w1);
            int c = (a << 11) & 0xffff;

            a0 = (c0 >> 5) & 63;
            a1 = (c1 >> 5) & 63;
            a = (w0 * a0 + w1 * a1) / (w0 + w1);
            c = c | ((a << 5) & 0xffff);

            a0 = c0 & 31;
            a1 = c1 & 31;
            a = (w0 * a0 + w1 * a1) / (w0 + w1);
            c = c | a;

            return c;
        }

        /// <summary>
        /// Gets the Width of the Texture in the Tpl
        /// </summary>
        /// <param name="tpl"></param>
        /// <returns></returns>
        public static int GetTextureWidth(byte[] tpl)
        {
            byte[] tmp = new byte[2];
            tmp[1] = tpl[22];
            tmp[0] = tpl[23];
            UInt16 width = BitConverter.ToUInt16(tmp, 0);
            return (int)width;
        }

        /// <summary>
        /// Gets the Height of the Texture in the Tpl
        /// </summary>
        /// <param name="tpl"></param>
        /// <returns></returns>
        public static int GetTextureHeight(byte[] tpl)
        {
            byte[] tmp = new byte[2];
            tmp[1] = tpl[20];
            tmp[0] = tpl[21];
            UInt16 height = BitConverter.ToUInt16(tmp, 0);
            return (int)height;
        }

        /// <summary>
        /// Gets the offset to the Texturedata in the Tpl
        /// </summary>
        /// <param name="tpl"></param>
        /// <returns></returns>
        public static int GetTextureOffset(byte[] tpl)
        {
            byte[] tmp = new byte[4];
            tmp[3] = tpl[28];
            tmp[2] = tpl[29];
            tmp[1] = tpl[30];
            tmp[0] = tpl[31];
            UInt32 offset = BitConverter.ToUInt32(tmp, 0);
            return (int)offset;
        }

        /// <summary>
        /// Converts RGBA8 Tpl Array to RGBA Byte Array
        /// </summary>
        /// <param name="tpl"></param>
        /// <returns></returns>
        public static byte[] FromRGBA8(byte[] tpl)
        {
            int width = GetTextureWidth(tpl);
            int height = GetTextureHeight(tpl);
            int offset = GetTextureOffset(tpl);
            UInt32[] output = new UInt32[width * height];
            int inp = 0;
            for (int y = 0; y < height; y += 4)
            {
                for (int x = 0; x < width; x += 4)
                {
                    for (int k = 0; k < 2; k++)
                    {
                        for (int y1 = y; y1 < y + 4; y1++)
                        {
                            for (int x1 = x; x1 < x + 4; x1++)
                            {
                                byte[] pixelbytes = new byte[2];
                                pixelbytes[1] = tpl[offset + inp * 2];
                                pixelbytes[0] = tpl[offset + inp * 2 + 1];
                                UInt16 pixel = BitConverter.ToUInt16(pixelbytes, 0);
                                inp++;

                                if ((x1 >= width) || (y1 >= height))
                                    continue;

                                if (k == 0)
                                {
                                    int a = (pixel >> 8) & 0xff;
                                    int r = (pixel >> 0) & 0xff;
                                    output[x1 + (y1 * width)] |= (UInt32)((r << 16) | (a << 24));
                                }
                                else
                                {
                                    int g = (pixel >> 8) & 0xff;
                                    int b = (pixel >> 0) & 0xff;
                                    output[x1 + (y1 * width)] |= (UInt32)((g << 8) | (b << 0));
                                }
                            }
                        }
                    }
                }
            }

            return Tools.UInt32ArrayToByteArray(output);
        }

        /// <summary>
        /// Converts RGB5A3 Tpl Array to RGBA Byte Array
        /// </summary>
        /// <param name="tpl"></param>
        /// <returns></returns>
        public static byte[] FromRGB5A3(byte[] tpl)
        {
            int width = GetTextureWidth(tpl);
            int height = GetTextureHeight(tpl);
            int offset = GetTextureOffset(tpl);
            UInt32[] output = new UInt32[width * height];
            int inp = 0;
            int r, g, b;
            int a = 0;
            for (int y = 0; y < height; y += 4)
            {
                for (int x = 0; x < width; x += 4)
                {
                    for (int y1 = y; y1 < y + 4; y1++)
                    {
                        for (int x1 = x; x1 < x + 4; x1++)
                        {
                            byte[] pixelbytes = new byte[2];
                            pixelbytes[1] = tpl[offset + inp * 2];
                            pixelbytes[0] = tpl[offset + inp * 2 + 1];
                            UInt16 pixel = BitConverter.ToUInt16(pixelbytes, 0);
                            inp++;

                            if (y1 >= height || x1 >= width)
                                continue;

                            if ((pixel & (1 << 15)) != 0)
                            {
                                b = (((pixel >> 10) & 0x1F) * 255) / 31;
                                g = (((pixel >> 5) & 0x1F) * 255) / 31;
                                r = (((pixel >> 0) & 0x1F) * 255) / 31;
                                a = 255;
                            }
                            else
                            {
                                a = (((pixel >> 12) & 0x07) * 255) / 7;
                                b = (((pixel >> 8) & 0x0F) * 255) / 15;
                                g = (((pixel >> 4) & 0x0F) * 255) / 15;
                                r = (((pixel >> 0) & 0x0F) * 255) / 15;
                            }

                            int rgba = (r << 0) | (g << 8) | (b << 16) | (a << 24);
                            output[(y1 * width) + x1] = (UInt32)rgba;
                        }
                    }
                }
            }

            return Tools.UInt32ArrayToByteArray(output);
        }

        /// <summary>
        /// Converts RGB565 Tpl Array to RGBA Byte Array
        /// </summary>
        /// <param name="tpl"></param>
        /// <returns></returns>
        public static byte[] FromRGB565(byte[] tpl)
        {
            int width = GetTextureWidth(tpl);
            int height = GetTextureHeight(tpl);
            int offset = GetTextureOffset(tpl);
            UInt32[] output = new UInt32[width * height];
            int inp = 0;
            for (int y = 0; y < height; y += 4)
            {
                for (int x = 0; x < width; x += 4)
                {
                    for (int y1 = y; y1 < y + 4; y1++)
                    {
                        for (int x1 = x; x1 < x + 4; x1++)
                        {
                            byte[] pixelbytes = new byte[2];
                            pixelbytes[1] = tpl[offset + inp * 2];
                            pixelbytes[0] = tpl[offset + inp * 2 + 1];
                            UInt16 pixel = BitConverter.ToUInt16(pixelbytes, 0);
                            inp++;

                            if (y1 >= height || x1 >= width)
                                continue;

                            int b = (((pixel >> 11) & 0x1F) << 3) & 0xff;
                            int g = (((pixel >> 5) & 0x3F) << 2) & 0xff;
                            int r = (((pixel >> 0) & 0x1F) << 3) & 0xff;
                            int a = 255;

                            int rgba = (r << 0) | (g << 8) | (b << 16) | (a << 24);
                            output[y1 * width + x1] = (UInt32)rgba;
                        }
                    }
                }
            }

            return Tools.UInt32ArrayToByteArray(output);
        }

        /// <summary>
        /// Converts I4 Tpl Array to RGBA Byte Array
        /// </summary>
        /// <param name="tpl"></param>
        /// <returns></returns>
        public static byte[] FromI4(byte[] tpl)
        {
            int width = GetTextureWidth(tpl);
            int height = GetTextureHeight(tpl);
            int offset = GetTextureOffset(tpl);
            UInt32[] output = new UInt32[width * height];
            int inp = 0;
            for (int y = 0; y < height; y += 8)
            {
                for (int x = 0; x < width; x += 8)
                {
                    for (int y1 = y; y1 < y + 8; y1++)
                    {
                        for (int x1 = x; x1 < x + 8; x1 += 2)
                        {
                            int pixel = tpl[offset + inp];

                            if (y1 >= height || x1 >= width)
                                continue;

                            int r = (pixel >> 4) * 255 / 15;
                            int g = (pixel >> 4) * 255 / 15;
                            int b = (pixel >> 4) * 255 / 15;
                            int a = (pixel >> 4) * 255 / 15;

                            int rgba = (r << 0) | (g << 8) | (b << 16) | (a << 24);
                            output[y1 * width + x1] = (UInt32)rgba;

                            pixel = tpl[offset + inp];
                            inp++;

                            if (y1 >= height || x1 >= width)
                                continue;

                            r = (pixel & 0x0F) * 255 / 15;
                            g = (pixel & 0x0F) * 255 / 15;
                            b = (pixel & 0x0F) * 255 / 15;
                            a = (pixel & 0x0F) * 255 / 15;

                            rgba = (r << 0) | (g << 8) | (b << 16) | (a << 24);
                            output[y1 * width + x1 + 1] = (UInt32)rgba;
                        }
                    }
                }
            }

            return Tools.UInt32ArrayToByteArray(output);
        }

        /// <summary>
        /// Converts IA4 Tpl Array to RGBA Byte Array
        /// </summary>
        /// <param name="tpl"></param>
        /// <returns></returns>
        public static byte[] FromIA4(byte[] tpl)
        {
            int width = GetTextureWidth(tpl);
            int height = GetTextureHeight(tpl);
            int offset = GetTextureOffset(tpl);
            UInt32[] output = new UInt32[width * height];
            int inp = 0;
            for (int y = 0; y < height; y += 4)
            {
                for (int x = 0; x < width; x += 8)
                {
                    for (int y1 = y; y1 < y + 4; y1++)
                    {
                        for (int x1 = x; x1 < x + 8; x1++)
                        {
                            int pixel = tpl[offset + inp];
                            inp++;

                            if (y1 >= height || x1 >= width)
                                continue;

                            int r = ((pixel & 0x0F) * 255 / 15) & 0xff;
                            int g = ((pixel & 0x0F) * 255 / 15) & 0xff;
                            int b = ((pixel & 0x0F) * 255 / 15) & 0xff;
                            int a = (((pixel >> 4) * 255) / 15) & 0xff;

                            int rgba = (r << 0) | (g << 8) | (b << 16) | (a << 24);
                            output[y1 * width + x1] = (UInt32)rgba;
                        }
                    }
                }
            }

            return Tools.UInt32ArrayToByteArray(output);
        }

        /// <summary>
        /// Converts I8 Tpl Array to RGBA Byte Array
        /// </summary>
        /// <param name="tpl"></param>
        /// <returns></returns>
        public static byte[] FromI8(byte[] tpl)
        {
            int width = GetTextureWidth(tpl);
            int height = GetTextureHeight(tpl);
            int offset = GetTextureOffset(tpl);
            UInt32[] output = new UInt32[width * height];
            int inp = 0;
            for (int y = 0; y < height; y += 4)
            {
                for (int x = 0; x < width; x += 8)
                {
                    for (int y1 = y; y1 < y + 4; y1++)
                    {
                        for (int x1 = x; x1 < x + 8; x1++)
                        {
                            int pixel = tpl[offset + inp];
                            inp++;

                            if (y1 >= height || x1 >= width)
                                continue;

                            int r = pixel;
                            int g = pixel;
                            int b = pixel;
                            int a = 255;

                            int rgba = (r << 0) | (g << 8) | (b << 16) | (a << 24);
                            output[y1 * width + x1] = (UInt32)rgba;
                        }
                    }
                }
            }

            return Tools.UInt32ArrayToByteArray(output);
        }

        /// <summary>
        /// Converts IA8 Tpl Array to RGBA Byte Array
        /// </summary>
        /// <param name="tpl"></param>
        /// <returns></returns>
        public static byte[] FromIA8(byte[] tpl)
        {
            int width = GetTextureWidth(tpl);
            int height = GetTextureHeight(tpl);
            int offset = GetTextureOffset(tpl);
            UInt32[] output = new UInt32[width * height];
            int inp = 0;
            for (int y = 0; y < height; y += 4)
            {
                for (int x = 0; x < width; x += 4)
                {
                    for (int y1 = y; y1 < y + 4; y1++)
                    {
                        for (int x1 = x; x1 < x + 4; x1++)
                        {
                            byte[] pixelbytes = new byte[2];
                            pixelbytes[1] = tpl[offset + inp * 2];
                            pixelbytes[0] = tpl[offset + inp * 2 + 1];
                            UInt16 pixel = BitConverter.ToUInt16(pixelbytes, 0);
                            inp++;

                            if (y1 >= height || x1 >= width)
                                continue;

                            int r = (pixel >> 8);// &0xff;
                            int g = (pixel >> 8);// &0xff;
                            int b = (pixel >> 8);// &0xff;
                            int a = (pixel >> 8) & 0xff;

                            int rgba = (r << 0) | (g << 8) | (b << 16) | (a << 24);
                            output[y1 * width + x1] = (UInt32)rgba;
                        }
                    }
                }
            }

            return Tools.UInt32ArrayToByteArray(output);
        }

        /// <summary>
        /// Converts CMP Tpl Array to RGBA Byte Array
        /// </summary>
        /// <param name="tpl"></param>
        /// <returns></returns>
        public static byte[] FromCMP(byte[] tpl)
        {
            int width = GetTextureWidth(tpl);
            int height = GetTextureHeight(tpl);
            int offset = GetTextureOffset(tpl);
            UInt32[] output = new UInt32[width * height];
            UInt16[] c = new UInt16[4];
            int[] pix = new int[3];
            int inp = 0;

            for (int y = 0; y < height; y++)
            {
                for (int x = 0; x < width; x++)
                {
                    int ww = Tools.AddPadding(width, 8);

                    int x0 = x & 0x03;
                    int x1 = (x >> 2) & 0x01;
                    int x2 = x >> 3;

                    int y0 = y & 0x03;
                    int y1 = (y >> 2) & 0x01;
                    int y2 = y >> 3;

                    int off = (8 * x1) + (16 * y1) + (32 * x2) + (4 * ww * y2);

                    byte[] tmp1 = new byte[2];
                    tmp1[1] = tpl[offset + off];
                    tmp1[0] = tpl[offset + off + 1];
                    c[0] = BitConverter.ToUInt16(tmp1, 0);
                    tmp1[1] = tpl[offset + off + 2];
                    tmp1[0] = tpl[offset + off + 3];
                    c[1] = BitConverter.ToUInt16(tmp1, 0);

                    if (c[0] > c[1])
                    {
                        c[2] = (UInt16)avg(2, 1, c[0], c[1]);
                        c[3] = (UInt16)avg(1, 2, c[0], c[1]);
                    }
                    else
                    {
                        c[2] = (UInt16)avg(1, 1, c[0], c[1]);
                        c[3] = 0;
                    }

                    byte[] pixeldata = new byte[4];
                    pixeldata[3] = tpl[offset + off + 4];
                    pixeldata[2] = tpl[offset + off + 5];
                    pixeldata[1] = tpl[offset + off + 6];
                    pixeldata[0] = tpl[offset + off + 7];
                    UInt32 pixel = BitConverter.ToUInt32(pixeldata, 0);

                    int ix = x0 + (4 * y0);
                    int raw = c[(pixel >> (30 - (2 * ix))) & 0x03];

                    pix[0] = (raw >> 8) & 0xf8;
                    pix[1] = (raw >> 3) & 0xf8;
                    pix[2] = (raw << 3) & 0xf8;

                    int intout = (pix[0] << 16) | (pix[1] << 8) | (pix[2] << 0) | (255 << 24);
                    output[inp] = (UInt32)intout;
                    inp++;
                }
            }

            return Tools.UInt32ArrayToByteArray(output);
        }

        /// <summary>
        /// Gets the pixel data of a Bitmap as an Byte Array
        /// </summary>
        /// <param name="img"></param>
        /// <returns></returns>
        public static uint[] BitmapToRGBA(Bitmap img)
        {
            int x = img.Width;
            int y = img.Height;
            UInt32[] rgba = new UInt32[x * y];

            for (int i = 0; i < y; i += 4)
            {
                for (int j = 0; j < x; j += 4)
                {
                    for (int y1 = i; y1 < i + 4; y1++)
                    {
                        for (int x1 = j; x1 < j + 4; x1++)
                        {
                            if (y1 >= y || x1 >= x)
                                continue;

                            Color color = img.GetPixel(x1, y1);
                            rgba[x1 + (y1 * x)] = (UInt32)color.ToArgb();
                        }
                    }
                }
            }

            return rgba;
        }

        /// <summary>
        /// Converts an Image to a Tpl
        /// </summary>
        /// <param name="img"></param>
        /// <param name="format">4 = RGB565, 5 = RGB5A3, 6 = RGBA8</param>
        /// <returns></returns>
        public static void ConvertToTPL(Bitmap img, string destination, int format)
        {
            byte[] tpl = ConvertToTPL(img, format);

            using (FileStream fs = new FileStream(destination, FileMode.Create))
            {
                fs.Write(tpl, 0, tpl.Length);
            }
        }

        /// <summary>
        /// Converts an Image to a Tpl
        /// </summary>
        /// <param name="img"></param>
        /// <param name="format">4 = RGB565, 5 = RGB5A3, 6 = RGBA8</param>
        /// <returns></returns>
        public static void ConvertToTPL(Image img, string destination, int format)
        {
            byte[] tpl = ConvertToTPL((Bitmap)img, format);

            using (FileStream fs = new FileStream(destination, FileMode.Create))
            {
                fs.Write(tpl, 0, tpl.Length);
            }
        }

        /// <summary>
        /// Converts an Image to a Tpl
        /// </summary>
        /// <param name="img"></param>
        /// <param name="format">4 = RGB565, 5 = RGB5A3, 6 = RGBA8</param>
        /// <returns></returns>
        public static byte[] ConvertToTPL(Image img, int format)
        {
            return ConvertToTPL((Bitmap)img, format);
        }

        /// <summary>
        /// Converts an Image to a Tpl
        /// </summary>
        /// <param name="img"></param>
        /// <param name="format">4 = RGB565, 5 = RGB5A3, 6 = RGBA8</param>
        /// <returns></returns>
        public static byte[] ConvertToTPL(Bitmap img, int format)
        {
            using (MemoryStream ms = new MemoryStream())
            {
                byte[] rgbaData;

                UInt32 tplmagic = 0x20af30;
                UInt32 ntextures = 0x1;
                UInt32 headersize = 0xc;
                UInt32 texheaderoff = 0x14;
                UInt32 texpaletteoff = 0x0;

                UInt16 texheight = (UInt16)img.Height;
                UInt16 texwidth = (UInt16)img.Width;
                UInt32 texformat;
                UInt32 texdataoffset = 0x40;
                byte[] rest = new byte[] { 00, 00, 00, 00, 00, 00, 00, 00, 00, 00, 00, 01, 00, 00, 00, 01, 00, 00, 00, 00, 00, 00, 00, 00, 00, 00, 00, 00, 00, 00, 00, 00 };
                //This should do it for our needs.. rest includes padding

                switch (format)
                {
                    case 4: //RGB565
                        texformat = 0x4;
                        rgbaData = ToRGB565(img);
                        break;
                    case 5: //RGB5A3
                        texformat = 0x5;
                        rgbaData = ToRGB5A3(img);
                        break;
                    default: //RGBA8 = 6
                        texformat = 0x6;
                        rgbaData = ToRGBA8(img);
                        break;
                }

                byte[] buffer = BitConverter.GetBytes(tplmagic); Array.Reverse(buffer);
                ms.Seek(0, SeekOrigin.Begin);
                ms.Write(buffer, 0, buffer.Length);

                buffer = BitConverter.GetBytes(ntextures); Array.Reverse(buffer);
                ms.Write(buffer, 0, buffer.Length);

                buffer = BitConverter.GetBytes(headersize); Array.Reverse(buffer);
                ms.Write(buffer, 0, buffer.Length);

                buffer = BitConverter.GetBytes(texheaderoff); Array.Reverse(buffer);
                ms.Write(buffer, 0, buffer.Length);

                buffer = BitConverter.GetBytes(texpaletteoff); Array.Reverse(buffer);
                ms.Write(buffer, 0, buffer.Length);

                buffer = BitConverter.GetBytes(texheight); Array.Reverse(buffer);
                ms.Write(buffer, 0, buffer.Length);

                buffer = BitConverter.GetBytes(texwidth); Array.Reverse(buffer);
                ms.Write(buffer, 0, buffer.Length);

                buffer = BitConverter.GetBytes(texformat); Array.Reverse(buffer);
                ms.Write(buffer, 0, buffer.Length);

                buffer = BitConverter.GetBytes(texdataoffset); Array.Reverse(buffer);
                ms.Write(buffer, 0, buffer.Length);

                ms.Write(rest, 0, rest.Length);

                ms.Write(rgbaData, 0, rgbaData.Length);

                return ms.ToArray();
            }
        }

        /// <summary>
        /// Converts an Image to RGBA8 Tpl data
        /// </summary>
        /// <param name="img"></param>
        /// <returns></returns>
        public static byte[] ToRGBA8(Bitmap img)
        {
            uint[] pixeldata = BitmapToRGBA(img);
            int w = img.Width;
            int h = img.Height;
            int z = 0, iv = 0;
            byte[] output = new byte[Tools.AddPadding(w, 4) * Tools.AddPadding(h, 4) * 4];
            uint[] lr = new uint[32], lg = new uint[32], lb = new uint[32], la = new uint[32];

            for (int y1 = 0; y1 < h; y1 += 4)
            {
                for (int x1 = 0; x1 < w; x1 += 4)
                {
                    for (int y = y1; y < (y1 + 4); y++)
                    {
                        for (int x = x1; x < (x1 + 4); x++)
                        {
                            UInt32 rgba;

                            if (y >= h || x >= w)
                            {
                                rgba = 0;
                            }
                            else
                            {
                                rgba = pixeldata[x + (y * w)];
                            }

                            lr[z] = (uint)(rgba >> 16) & 0xff;
                            lg[z] = (uint)(rgba >> 8) & 0xff;
                            lb[z] = (uint)(rgba >> 0) & 0xff;
                            la[z] = (uint)(rgba >> 24) & 0xff;

                            z++;
                        }
                    }

                    if (z == 16)
                    {
                        for (int i = 0; i < 16; i++)
                        {
                            output[iv++] = (byte)(la[i]);
                            output[iv++] = (byte)(lr[i]);
                        }
                        for (int i = 0; i < 16; i++)
                        {
                            output[iv++] = (byte)(lg[i]);
                            output[iv++] = (byte)(lb[i]);
                        }

                        z = 0;
                    }
                }
            }


            return output;
        }

        /// <summary>
        /// Converts an Image to RGBA565 Tpl data
        /// </summary>
        /// <param name="img"></param>
        /// <returns></returns>
        public static byte[] ToRGB565(Bitmap img)
        {
            uint[] pixeldata = BitmapToRGBA(img);
            int w = img.Width;
            int h = img.Height;
            int z = -1;
            byte[] output = new byte[Tools.AddPadding(w, 4) * Tools.AddPadding(h, 4) * 2];

            for (int y1 = 0; y1 < h; y1 += 4)
            {
                for (int x1 = 0; x1 < w; x1 += 4)
                {
                    for (int y = y1; y < y1 + 4; y++)
                    {
                        for (int x = x1; x < x1 + 4; x++)
                        {
                            UInt16 newpixel;

                            if (y >= h || x >= w)
                            {
                                newpixel = 0;
                            }
                            else
                            {
                                uint rgba = pixeldata[x + (y * w)];

                                uint b = (rgba >> 16) & 0xff;
                                uint g = (rgba >> 8) & 0xff;
                                uint r = (rgba >> 0) & 0xff;

                                newpixel = (UInt16)(((b >> 3) << 11) | ((g >> 2) << 5) | ((r >> 3) << 0));
                            }

                            byte[] temp = BitConverter.GetBytes(newpixel);
                            Array.Reverse(temp);

                            output[++z] = temp[0];
                            output[++z] = temp[1];
                        }
                    }
                }
            }

            return output;
        }

        /// <summary>
        /// Converts an Image to RGBA5A3 Tpl data
        /// </summary>
        /// <param name="img"></param>
        /// <returns></returns>
        public static byte[] ToRGB5A3(Bitmap img)
        {
            uint[] pixeldata = BitmapToRGBA(img);
            int w = img.Width;
            int h = img.Height;
            int z = -1;
            byte[] output = new byte[Tools.AddPadding(w, 4) * Tools.AddPadding(h, 4) * 2];

            for (int y1 = 0; y1 < h; y1 += 4)
            {
                for (int x1 = 0; x1 < w; x1 += 4)
                {
                    for (int y = y1; y < y1 + 4; y++)
                    {
                        for (int x = x1; x < x1 + 4; x++)
                        {
                            int newpixel;

                            if (y >= h || x >= w)
                            {
                                newpixel = 0;
                            }
                            else
                            {
                                int rgba = (int)pixeldata[x + (y * w)];
                                newpixel = 0;

                                int r = (rgba >> 16) & 0xff;
                                int g = (rgba >> 8) & 0xff;
                                int b = (rgba >> 0) & 0xff;
                                int a = (rgba >> 24) & 0xff;

                                if (a <= 0xda)
                                {
                                    //RGB4A3

                                    newpixel &= ~(1 << 15);

                                    r = ((r * 15) / 255) & 0xf;
                                    g = ((g * 15) / 255) & 0xf;
                                    b = ((b * 15) / 255) & 0xf;
                                    a = ((a * 7) / 255) & 0x7;

                                    newpixel |= a << 12;
                                    newpixel |= b << 0;
                                    newpixel |= g << 4;
                                    newpixel |= r << 8;
                                }
                                else
                                {
                                    //RGB5

                                    newpixel |= (1 << 15);

                                    r = ((r * 31) / 255) & 0x1f;
                                    g = ((g * 31) / 255) & 0x1f;
                                    b = ((b * 31) / 255) & 0x1f;

                                    newpixel |= b << 0;
                                    newpixel |= g << 5;
                                    newpixel |= r << 10;
                                }
                            }

                            byte[] temp = BitConverter.GetBytes((UInt16)newpixel);
                            Array.Reverse(temp);

                            output[++z] = temp[0];
                            output[++z] = temp[1];
                        }
                    }
                }
            }

            return output;
        }
    }

}