﻿using CTRFramework.Shared;
using UnityEngine;
using Random = System.Random;

namespace CTRFramework.Vram
{
    public class BMPHeader
    {
        char[] magic = new char[2] { 'B', 'M' };
        uint filesize = 0;
        uint reserved = 0;
        uint ptrBitmap = 0; //total size - header size - palette size

        uint headerSize = 0x28;
        int width = 0;
        int height = 0;
        ushort numPlanes = 1;
        ushort bpp = 4;
        uint compression = 0;

        uint bitmapSize = 0;
        uint ppix = 0xEC4;
        uint ppiy = 0xEC4;
        uint numColors = 16;
        uint numColorsImportant = 16;

        byte[] palette = new byte[0];
        byte[] data = new byte[0];

        uint datasize;

        public void Update(int w, int h, uint numCols, ushort bits)
        {
            width = w;
            height = h;

            numColors = numCols;
            numColorsImportant = numCols;
            palette = new byte[numCols * 4];

            bpp = bits;
            datasize = (uint)(width * height * bpp / 8);
            data = new byte[datasize];

            int totalsize = 0x36 + palette.Length + data.Length;
            ptrBitmap = (uint)(0x36 + palette.Length);
        }

        public static byte[] GrayScalePalette(int numColors)
        {
            byte[] x = new byte[numColors * 4];

            for (int i = 0; i < numColors; i++)
            {
                x[i * 4] = (byte)(255.0 / numColors * i);
                x[i * 4 + 1] = (byte)(255.0 / numColors * i);
                x[i * 4 + 2] = (byte)(255.0 / numColors * i);
                x[i * 4 + 3] = 255;
            }

            return x;
        }

        public byte[] RandomBitmap()
        {
            byte[] x = new byte[datasize];

            Random r = new Random();

            for (int i = 0; i < datasize; i++)
            {
                x[i] = (byte)r.Next(255);
            }

            return x;
        }

        public void UpdateData(byte[] pal, byte[] d)
        {
            palette = pal;
            data = d;
        }
        /*
        public void UpdateData(byte[] pal, ushort[] d)
        {
            palette = pal;

            for (int i = 0; i < d.Length; i++)
            {
                data[i*2] = (byte)(d[i] & 0xFF);
                data[i * 2 + 1] = (byte)(d[i] >> 8);
            }
         }
         */
        public Color? GetColor(int x)
        {
            if (x < palette.Length / 4)
                return new Color(palette[x + 0]/255f, palette[x + 1]/255f, palette[x + 2]/255f);

            return null;
        }

        public void Write(BinaryWriterEx bw)
        {
            bw.Write(magic);
            bw.Write(filesize);
            bw.Write(reserved);
            bw.Write(ptrBitmap);
            bw.Write(headerSize);
            bw.Write(width);
            bw.Write(-height);
            bw.Write(numPlanes);
            bw.Write(bpp);
            bw.Write(compression);
            bw.Write(bitmapSize);
            bw.Write(ppix);
            bw.Write(ppiy);
            bw.Write(numColors);
            bw.Write(numColorsImportant);
            bw.Write(palette);
            bw.Write(data);
        }


    }
}