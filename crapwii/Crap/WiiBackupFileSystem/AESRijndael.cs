//-------------------------------------
// WBFSSync - WBFSSync.exe
//
// Copyright 2009 Caian (ÔmΣga Frøst) <frost.omega@hotmail.com> based on rijndael.c:
//
//   Written by Mike Scott 21st April 1999
//   mike@compapp.dcu.ie
//
//   Permission for free direct or derivative use is granted subject 
//   to compliance with any conditions that the originators of the 
//   algorithm place on its exploitation.
//
//
// WBFSSync is Licensed under the terms of the Microsoft Reciprocal License (Ms-RL)
//
// AESRijndael.cs:
//
// Rotinas de encriptação/decriptação de dados pelo algorítmo AES-Rijndael
//
//-------------------------------------

using System;
using System.Collections.Generic;
using System.Text;
using System.Runtime.InteropServices;

namespace WBFSSync
{
    internal static class AESRijndael
    {
        static readonly byte[] InCo = new byte[] { 0xB, 0xD, 0x9, 0xE };

        static byte[] fbsub = new byte[256];
        static byte[] rbsub = new byte[256];
        static byte[] ptab = new byte[256];
        static byte[] ltab = new byte[256];
        static uint[] ftable = new uint[256];
        static uint[] rtable = new uint[256];
        static uint[] rco = new uint[30];

        static int Nk = 0;
        static int Nb = 0;
        static int Nr = 0;
        static byte[] fi = new byte[24];
        static byte[] ri = new byte[24];
        static uint[] fkey = new uint[120];
        static uint[] rkey = new uint[120];

        //--------------------- Funções Públicas

        public static void aes_set_key(byte[] key)
        {
            gentables();
            gkey(4, 4, key);
        }

        // CBC mode decryption
        public static void aes_decrypt(byte[] iv, byte[] inbuf, byte[] outbuf, ulong len)
        {
            byte[] block = new byte[16];
            byte[] ctext_ptr;
            uint blockno = 0, i;

            //printf("aes_decrypt(%p, %p, %p, %lld)\n", iv, inbuf, outbuf, len);

            for (blockno = 0; blockno <= (len / 16); blockno++)
            {
                uint fraction;
                if (blockno == (len / 16)) //!Opa!
                { // last block
                    fraction = (uint)(len % 16); //!Opa!
                    if (fraction == 0) break;
                    memset(block, 0, 16); //!Opa!
                }
                else fraction = 16;

                //    debug_printf("block %d: fraction = %d\n", blockno, fraction);
                //memcpy(block, inbuf + blockno * sizeof(block), fraction);
                Array.Copy(inbuf, blockno * 16, block, 0, fraction); //!Opa!
                decrypt(block);

                //------------------
                long off = 0;//!Opa!
                //------------------

                if (blockno == 0)
                {
                    ctext_ptr = iv;
                }
                else
                {
                    ctext_ptr = inbuf;
                    off = (blockno - 1) * 16; //!Opa!
                }

                for (i = 0; i < fraction; i++)
                    outbuf[blockno * 16 + i] = (byte)(ctext_ptr[off + i] ^ block[i]);  //!Opa!
                //    debug_printf("Block %d output: ", blockno);
                //    hexdump(outbuf + blockno*sizeof(block), 16);
            }
        }

        // CBC mode encryption      
        public static void aes_encrypt(byte[] iv, byte[] inbuf, byte[] outbuf, ulong len)
        {
            byte[] block = new byte[16];
            uint blockno = 0, i;

            //  debug_printf("aes_decrypt(%p, %p, %p, %lld)\n", iv, inbuf, outbuf, len);

            for (blockno = 0; blockno <= (len / 16); blockno++)
            {
                uint fraction;
                if (blockno == (len / 16)) //!Opa!
                { // last block
                    fraction = (uint)(len % 16); //!Opa!
                    if (fraction == 0) break;
                    memset(block, 0, 16); //!Opa!
                }
                else fraction = 16;

                //    debug_printf("block %d: fraction = %d\n", blockno, fraction);
                //memcpy(block, inbuf + blockno * sizeof(block), fraction);
                Array.Copy(inbuf, blockno * 16, block, 0, fraction); //!Opa!

                for (i = 0; i < fraction; i++)
                    block[i] = (byte)(inbuf[blockno * 16 + i] ^ iv[i]); //!Opa!

                encrypt(block); //!Opa!
                //memcpy(iv, block, sizeof(block)); //!Opa!
                //memcpy(outbuf + blockno * sizeof(block), block, sizeof(block)); //!Opa!

                Array.Copy(block, iv, 16); //!Opa!
                Array.Copy(block, 0, outbuf, blockno * 16, 16); //!Opa!

                //    debug_printf("Block %d output: ", blockno);
                //    hexdump(outbuf + blockno*sizeof(block), 16);
            }
        }
        
        //--------------------- Funções Privadas

        //Aproximação precária para uma função 'maravilhastica' em C
        static void memset(byte[] block, ulong off, byte v, ulong len)
        {
            for (ulong i = 0; i < len; i++) { block[off + i] = v; }
        }

        static void memset(byte[] block, byte v, ulong len)
        {
            for (ulong i = 0; i < len; i++) { block[i] = v; }
        }

        //-------------------

        static byte ROTL(byte x) { return (byte)(((x) >> 7) | ((x) << 1)); }

        static int ROTL(int x) { return (((x) >> 7) | ((x) << 1)); }
        static int ROTL8(int x) { return (((x) << 8) | ((x) >> 24)); }
        static int ROTL16(int x) { return (((x) << 16) | ((x) >> 16)); }
        static int ROTL24(int x) { return (((x) << 24) | ((x) >> 8)); }

        static long ROTL(long x) { return (((x) >> 7) | ((x) << 1)); }
        static long ROTL8(long x) { return (((x) << 8) | ((x) >> 24)); }
        static long ROTL16(long x) { return (((x) << 16) | ((x) >> 16)); }
        static long ROTL24(long x) { return (((x) << 24) | ((x) >> 8)); }

        static uint ROTL(uint x) { return (((x) >> 7) | ((x) << 1)); }
        static uint ROTL8(uint x) { return (((x) << 8) | ((x) >> 24)); }
        static uint ROTL16(uint x) { return (((x) << 16) | ((x) >> 16)); }
        static uint ROTL24(uint x) { return (((x) << 24) | ((x) >> 8)); }

        static ulong ROTL(ulong x) { return (((x) >> 7) | ((x) << 1)); }
        static ulong ROTL8(ulong x) { return (((x) << 8) | ((x) >> 24)); }
        static ulong ROTL16(ulong x) { return (((x) << 16) | ((x) >> 16)); }
        static ulong ROTL24(ulong x) { return (((x) << 24) | ((x) >> 8)); }

        //-------------------

        static uint pack(byte[] b)
        { 
            /* pack bytes into a 32-bit Word */
            return ((uint)b[3] << 24) | ((uint)b[2] << 16) | ((uint)b[1] << 8) | (uint)b[0];
        }

        static uint pack(byte[] b, long offset)
        {
            /* pack bytes into a 32-bit Word */
            return ((uint)b[offset + 3] << 24) | ((uint)b[offset + 2] << 16) | ((uint)b[offset + 1] << 8) | (uint)b[offset];
        }

        static void unpack(uint a, byte[] b)
        { 
            /* unpack bytes from a word */
            b[0] = (byte)(a & 0xFF);
            b[1] = (byte)(a >> 8);
            b[2] = (byte)(a >> 16);
            b[3] = (byte)(a >> 24);
        }

        static void unpack(uint a, byte[] b, long offset)
        {
            /* unpack bytes from a word */
            b[offset] = (byte)(a & 0xFF);
            b[offset + 1] = (byte)(a >> 8);
            b[offset + 2] = (byte)(a >> 16);
            b[offset + 3] = (byte)(a >> 24);
        }

        static byte xtime(byte a)
        {
            byte b;
            if ((a & 0x80) != 0) 
                b = 0x1B;
            else 
                b = 0;
            a <<= 1;
            a ^= b;
            return a;
        }

        static byte bmul(byte x, byte y)
        { 
            /* x.y= AntiLog(Log(x) + Log(y)) */
            if ((x != 0) && (y != 0)) 
                return ptab[(ltab[x] + ltab[y]) % 255];
            else 
                return 0;
        }

        static uint SubByte(uint a)
        {
            byte[] b = new byte[4];
            unpack(a, b);
            b[0] = fbsub[b[0]];
            b[1] = fbsub[b[1]];
            b[2] = fbsub[b[2]];
            b[3] = fbsub[b[3]];
            return pack(b);
        }

        static byte product(uint x, uint y)
        { 
            /* dot product of two 4-byte arrays */
            byte[] xb = new byte[4], yb = new byte[4];
            unpack(x, xb);
            unpack(y, yb);
            return (byte)(bmul(xb[0], yb[0]) ^ bmul(xb[1], yb[1]) ^ bmul(xb[2], yb[2]) ^ bmul(xb[3], yb[3]));
            //!Opa!
        }

        static uint InvMixCol(uint x)
        { 
            /* matrix Multiplication */
            uint y, m;
            byte[] b = new byte[4];

            m = pack(InCo);
            b[3] = product(m, x);
            m = ROTL24(m);
            b[2] = product(m, x);
            m = ROTL24(m);
            b[1] = product(m, x);
            m = ROTL24(m);
            b[0] = product(m, x);
            y = pack(b);
            return y;
        }

        static byte ByteSub(byte x)
        {
            byte y = ptab[255 - ltab[x]];  /* multiplicative inverse */
            x = y; x = ROTL(x);
            y ^= x; x = ROTL(x);
            y ^= x; x = ROTL(x);
            y ^= x; x = ROTL(x);
            y ^= x; y ^= 0x63;
            return y;
        }

        static void gentables()
        { 
            /* generate tables */
            int i;
            byte y;
            byte[] b = new byte[4];

            /* use 3 as primitive root to generate power and log tables */

            ltab[0] = 0;
            ptab[0] = 1; ltab[1] = 0;
            ptab[1] = 3; ltab[3] = 1;
            for (i = 2; i < 256; i++)
            {
                ptab[i] = (byte)(ptab[i - 1] ^ xtime(ptab[i - 1])); //!Opa!
                ltab[ptab[i]] = (byte)i;
            }

            /* affine transformation:- each bit is xored with itself shifted one bit */

            fbsub[0] = 0x63;
            rbsub[0x63] = 0;
            for (i = 1; i < 256; i++)
            {
                y = ByteSub((byte)i);
                fbsub[i] = y; rbsub[y] = (byte)i;
            }

            for (i = 0, y = 1; i < 30; i++)
            {
                rco[i] = y;
                y = xtime(y);
            }

            /* calculate forward and reverse tables */
            for (i = 0; i < 256; i++)
            {
                y = fbsub[i];
                b[3] = (byte)(y ^ xtime(y)); b[2] = y; //!Opa!
                b[1] = y; b[0] = xtime(y);
                ftable[i] = pack(b);

                y = rbsub[i];
                b[3] = bmul(InCo[0], y); b[2] = bmul(InCo[1], y);
                b[1] = bmul(InCo[2], y); b[0] = bmul(InCo[3], y);
                rtable[i] = pack(b);
            }
        }

        static void gkey(int nb, int nk, byte[] key)
        { 
            /* blocksize=32*nb bits. Key=32*nk bits */
            /* currently nb,bk = 4, 6 or 8          */
            /* key comes as 4*Nk bytes              */
            /* Key Scheduler. Create expanded encryption key */
            int i, j, k, m, N;
            int C1, C2, C3;
            uint[] CipherKey = new uint[8];

            Nb = nb; Nk = nk;

            /* Nr is number of rounds */
            if (Nb >= Nk) Nr = 6 + Nb;
            else Nr = 6 + Nk;

            C1 = 1;
            if (Nb < 8) { C2 = 2; C3 = 3; }
            else { C2 = 3; C3 = 4; }

            /* pre-calculate forward and reverse increments */
            for (m = j = 0; j < nb; j++, m += 3)
            {
                fi[m] = (byte)((j + C1) % nb); //!Opa!
                fi[m + 1] = (byte)((j + C2) % nb); //!Opa!
                fi[m + 2] = (byte)((j + C3) % nb); //!Opa!
                ri[m] = (byte)((nb + j - C1) % nb); //!Opa!
                ri[m + 1] = (byte)((nb + j - C2) % nb); //!Opa!
                ri[m + 2] = (byte)((nb + j - C3) % nb); //!Opa!
            }

            N = Nb * (Nr + 1);

            for (i = j = 0; i < Nk; i++, j += 4)
            {
                CipherKey[i] = pack(key, j); //!Opa!
            }
            for (i = 0; i < Nk; i++) fkey[i] = CipherKey[i];
            for (j = Nk, k = 0; j < N; j += Nk, k++)
            {
                fkey[j] = fkey[j - Nk] ^ SubByte(ROTL24(fkey[j - 1])) ^ rco[k];
                if (Nk <= 6)
                {
                    for (i = 1; i < Nk && (i + j) < N; i++)
                        fkey[i + j] = fkey[i + j - Nk] ^ fkey[i + j - 1];
                }
                else
                {
                    for (i = 1; i < 4 && (i + j) < N; i++)
                        fkey[i + j] = fkey[i + j - Nk] ^ fkey[i + j - 1];
                    if ((j + 4) < N) fkey[j + 4] = fkey[j + 4 - Nk] ^ SubByte(fkey[j + 3]);
                    for (i = 5; i < Nk && (i + j) < N; i++)
                        fkey[i + j] = fkey[i + j - Nk] ^ fkey[i + j - 1];
                }

            }

            /* now for the expanded decrypt key in reverse order */

            for (j = 0; j < Nb; j++) rkey[j + N - Nb] = fkey[j];
            for (i = Nb; i < N - Nb; i += Nb)
            {
                k = N - Nb - i;
                for (j = 0; j < Nb; j++) rkey[k + j] = InvMixCol(fkey[i + j]);
            }
            for (j = N - Nb; j < N; j++) rkey[j - N + Nb] = fkey[j];
        }

        static void encrypt(byte[] buff)
        {
            int i, j, k, m;
            uint[] a = new uint[8], b = new uint[8], x, y, t;

            for (i = j = 0; i < Nb; i++, j += 4)
            {
                a[i] = pack(buff, j);
                a[i] ^= fkey[i];
            }
            k = Nb;
            x = a; y = b;

            /* State alternates between a and b */
            for (i = 1; i < Nr; i++)
            { /* Nr is number of rounds. May be odd. */

                /* if Nb is fixed - unroll this next 
                   loop and hard-code in the values of fi[]  */

                for (m = j = 0; j < Nb; j++, m += 3)
                { /* deal with each 32-bit element of the State */
                    /* This is the time-critical bit */
                    y[j] = fkey[k++] ^ ftable[(byte)x[j]] ^
                         ROTL8(ftable[(byte)(x[fi[m]] >> 8)]) ^
                         ROTL16(ftable[(byte)(x[fi[m + 1]] >> 16)]) ^
                         ROTL24(ftable[x[fi[m + 2]] >> 24]);
                }
                t = x; x = y; y = t;      /* swap pointers */
            }

            /* Last Round - unroll if possible */
            for (m = j = 0; j < Nb; j++, m += 3)
            {
                y[j] = fkey[k++] ^ (uint)fbsub[(byte)x[j]] ^
                     ROTL8((uint)fbsub[(byte)(x[fi[m]] >> 8)]) ^
                     ROTL16((uint)fbsub[(byte)(x[fi[m + 1]] >> 16)]) ^
                     ROTL24((uint)fbsub[x[fi[m + 2]] >> 24]);
            }
            for (i = j = 0; i < Nb; i++, j += 4)
            {
                unpack(y[i], buff, j);
                x[i] = y[i] = 0;   /* clean up stack */
            }
            return;
        }

        static void decrypt(byte[] buff)
        {
            int i, j, k, m;
            uint[] a = new uint[8], b = new uint[8], x, y, t;

            for (i = j = 0; i < Nb; i++, j += 4)
            {
                a[i] = pack(buff, j);
                a[i] ^= rkey[i];
            }
            k = Nb;
            x = a; y = b;

            /* State alternates between a and b */
            for (i = 1; i < Nr; i++)
            { /* Nr is number of rounds. May be odd. */

                /* if Nb is fixed - unroll this next 
                   loop and hard-code in the values of ri[]  */

                for (m = j = 0; j < Nb; j++, m += 3)
                { /* This is the time-critical bit */
                    y[j] = rkey[k++] ^ rtable[(byte)x[j]] ^
                         ROTL8(rtable[(byte)(x[ri[m]] >> 8)]) ^
                         ROTL16(rtable[(byte)(x[ri[m + 1]] >> 16)]) ^
                         ROTL24(rtable[x[ri[m + 2]] >> 24]);
                }
                t = x; x = y; y = t;      /* swap pointers */
            }

            /* Last Round - unroll if possible */
            for (m = j = 0; j < Nb; j++, m += 3)
            {
                y[j] = rkey[k++] ^ (uint)rbsub[(byte)x[j]] ^
                     ROTL8((uint)rbsub[(byte)(x[ri[m]] >> 8)]) ^
                     ROTL16((uint)rbsub[(byte)(x[ri[m + 1]] >> 16)]) ^
                     ROTL24((uint)rbsub[x[ri[m + 2]] >> 24]);
            }
            for (i = j = 0; i < Nb; i++, j += 4)
            {
                unpack(y[i], buff, j);
                x[i] = y[i] = 0;   /* clean up stack */
            }
            return;
        }



        //-----
    }
}
