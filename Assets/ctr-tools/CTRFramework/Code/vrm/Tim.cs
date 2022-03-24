using CTRFramework.Shared;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using TreeEditor;
using UnityEngine;
using UnityEngine.Experimental.Rendering;
using Color = UnityEngine.Color;

namespace CTRFramework.Vram
{
    /// <summary>
    /// Please note this is super hacky implementation of PSX Tim format hardcoded for 4 bits used in Crash Team Racing.
    /// You can load any tim files, but overall code will not work properly with anything other than 4 bits.
    /// </summary>
    public class Tim : IRead
    {

        public Dictionary<string, Texture> textures = new Dictionary<string, Texture>();

        public uint magic;
        public uint flags;

        public uint clutsize;
        public Rectangle clutregion;
        public ushort[] clutdata;

        public uint datasize;

        public Rectangle region;

        public ushort[] data;

        private uint packedFlags => (uint)((int)bpp | ((hasClut ? 1 : 0) << 3));
        public BitDepth bpp = BitDepth.Bit16;

        public bool hasClut
        {
            get
            {
                switch (bpp)
                {
                    case BitDepth.Bit4:
                    case BitDepth.Bit8: return true;
                    case BitDepth.Bit16:
                    case BitDepth.Bit24:
                    default: return false;
                }
            }
        }

        public Tim()
        {
        }

        public static Tim FromFile(string fn)
        {
            using (BinaryReaderEx br = new BinaryReaderEx(File.OpenRead(fn)))
            {
                return new Tim(br);
            }
        }

        public static Tim FromReader(BinaryReaderEx br)
        {
            return new Tim(br);
        }
        
        public Tim(BinaryReaderEx br)
        {
            Read(br);
        }


        public Tim(Rectangle rect)
        {
            magic = 0x10;
            region = rect;
            data = new ushort[rect.Width * rect.Height];
            datasize = (uint)(data.Length * 2 + 4 * 3);
            flags = 2; //(((uint)bpp / 8) << 3) | 8;
        }

        /// <summary>
        /// Reads TIM from file using BinaryReader.
        /// </summary>
        /// <param name="br">BinaryReader.</param>
        public void Read(BinaryReaderEx br)
        {
            if (br.ReadUInt32() != magic)
                Helpers.Panic(this, PanicType.Warning, "Houston! magic mismatch");

            uint flags = br.ReadUInt32();

            switch (flags & 3)
            {
                case 0: bpp = BitDepth.Bit4; break;
                case 1: bpp = BitDepth.Bit8; break;
                case 2: bpp = BitDepth.Bit16; break;
                case 3: bpp = BitDepth.Bit24; break;
            }

            Helpers.Panic(this, PanicType.Info, bpp + " " + hasClut);

            if ((((flags >> 3) & 1) == 1) != hasClut)
                Helpers.Panic(this, PanicType.Warning, "Houston! bpp and clut mismatch.");

            if (hasClut)
            {
                uint _clutsize = br.ReadUInt32();
                clutregion.X = br.ReadUInt16();
                clutregion.Y = br.ReadUInt16();
                clutregion.Width = br.ReadUInt16();
                clutregion.Height = br.ReadUInt16();
                clutdata = br.ReadArrayUInt16(clutregion.Width * clutregion.Height);

                if (_clutsize != clutsize)
                    Console.WriteLine("Houston! clutsize mismatch.");
            }

            uint _datasize = br.ReadUInt32();
            region.X = br.ReadUInt16();
            region.Y = br.ReadUInt16();
            region.Width = br.ReadUInt16();
            region.Height = br.ReadUInt16();
            data = br.ReadArrayUInt16(region.Width * region.Height);

            if (_datasize != datasize)
                Console.WriteLine($"Houston! datasize mismatch. {_datasize} {datasize}");
        }


        /// <summary>
        /// Writes current TIM to file.
        /// </summary>
        /// <param name="filename">Filename.</param>
        public void Save(string filename)
        {
            using (var bw = new BinaryWriterEx(File.OpenWrite(filename)))
            {
                Write(bw);
            }
        }

        public void Write(BinaryWriterEx bw, List<UIntPtr> patchTable = null)
        {
            bw.Write(magic);
            bw.Write(packedFlags);

            if (hasClut)
            {
                bw.Write(clutsize);
                bw.Write((short)clutregion.X);
                bw.Write((short)clutregion.Y);
                bw.Write((short)clutregion.Width);
                bw.Write((short)clutregion.Height);
                foreach (ushort u in clutdata)
                    bw.Write(u);
            }

            bw.Write(datasize);
            bw.Write((short)region.X);
            bw.Write((short)region.Y);
            bw.Write((short)region.Width);
            bw.Write((short)region.Height);
            foreach (ushort u in data)
                bw.Write(u);
        }


        public override string ToString()
        {
            return region.ToString();
        }

        /// <summary>
        /// Draws one TIM over another.
        /// Not a failproof implementation, ensure that target TIM is larger than original.
        /// In CTR context only used to draw 2 TIM regions in a single TIM.
        /// </summary>
        /// <param name="src">Source TIM to draw.</param>
        public void DrawTim(Tim src)
        {
            if (src.data == null)
            {
                Debug.LogWarning("missing tim data.");
                return;
            }

            //int dstptr = (this.region.Width * src.region.Y + src.region.X) * 2;


            int srcptr = 0;
            int dstptr = this.region.Width * src.region.Y * 2 + src.region.X * 2;

            for (int i = 0; i < src.region.Height; i++)

            {
                //Console.WriteLine(srcptr + "\t" + dstptr);
                //Console.ReadKey();

                Buffer.BlockCopy(
                    src.data, srcptr,
                    this.data, dstptr,
                    src.region.Width * 2);

                dstptr += this.region.Width * 2;
                srcptr += src.region.Width * 2;
            }

            if (src.clutdata == null)
            {
                Debug.LogWarning("clutdata is missing.");
                return;
            }

            Buffer.BlockCopy(
                src.clutdata, 0,
                this.data, (this.region.Width * src.clutregion.Y + src.clutregion.X) * 2,
                src.clutdata.Length * 2); //keep in mind there will be leftover garbage if palette is less than 16 colors.
        }

        /// <summary>
        /// Cuts a Tim subtexture from current Tim, based on TextureLayout data.
        /// </summary>
        /// <param name="tl">TextureLayout object.</param>
        /// <returns>Tim object.</returns>
        public Tim GetTimTexture(TextureLayout tl)
        {
            int bpp = 4;

            if (tl.f1 > 0 && tl.f2 > 0 && tl.f3 > 0)
                bpp = 8;

            //Directory.CreateDirectory(path);

            //int width = (tl.width / 4) * 2;
            int width = (int)(tl.width * (bpp / 8.0f));
            int height = tl.height;

            ushort[] buf = new ushort[(width / 2) * height];

            //Console.WriteLine(width + "x" + height);

            int ptr = tl.Position * 2; // tl.PageY * 1024 * (1024 * 2 / 16) + tl.frame.Y * 1024 + tl.PageX * (1024 * 2 / 16) + tl.frame.X;

            for (int i = 0; i < height; i++)
            {
                try
                {
                    Buffer.BlockCopy(
                        this.data, ptr,
                        buf, i * width,
                        width);
                }
                catch
                { }

                ptr += CtrVrm.Width * 2;
            }


            Tim x = new Tim(tl.frame);

            x.data = buf;

            x.region = new Rectangle(tl.RealX, tl.RealY, tl.width / 4, tl.height);

            x.clutregion = new Rectangle(tl.PalX * 16, tl.PalY, 16, 1);
            x.clutdata = GetCtrClut(tl);
            x.clutsize = (uint)(x.clutregion.Width * 2 + 12);
            x.flags = 8; //4 bit + pal = 8

            /*
            Rectangle r = x.clutregion;

            if (r.Width % 4 != 0)
                r.Width += 16;

            Tim x2 = new Tim(r);
            x2.DrawTim(x);
            */

            //Console.WriteLine(x.clutdata.Length);

            return x;
        }


        public Tim GetTrueColorTexture(Rectangle r)
        {
            return GetTrueColorTexture(r.X, r.Y, r.Width, r.Height);
        }

        public Tim GetTrueColorTexture(int x, int y, int w, int h)
        {
            ushort[] buf = new ushort[w * h];

            //Console.WriteLine(width + "x" + height);

            int ptr = 1024 * y + x; // tl.PageY * 1024 * (1024 * 2 / 16) + tl.frame.Y * 1024 + tl.PageX * (1024 * 2 / 16) + tl.frame.X;

            for (int i = 0; i < h; i++)
            {
                Buffer.BlockCopy(
                    this.data, ptr * 2,
                    buf, i * w * 2,
                    w * 2);

                ptr += 1024;
            }


            Tim tim = new Tim(new Rectangle(x, y, w, h));
            tim.data = buf;
            tim.flags = 2; //4 bit + pal = 8

            return tim;
        }
        public byte[] SaveBMPToStream(byte[] pal)
        {
            using (MemoryStream ms = new MemoryStream())
            {
                using (BinaryWriterEx bw = new BinaryWriterEx(ms))
                {
                    BMPHeader bh = new BMPHeader();
                    bh.Update(region.Width * 4, region.Height, 16, 4);
                    byte[] data8 = new byte[data.Length * sizeof(short)];
                    Buffer.BlockCopy(data, 0, data8, 0, data8.Length);
                    bh.UpdateData(pal, FixBitmapData(FixPixelOrder(data8), region.Width * 2, region.Height));
                    bh.Write(bw);

                    return ms.ToArray();
                }
            }
        }

        /// <summary>
        /// This function is necessary to fix pixel order in 4-bit BMPs.
        /// </summary>
        /// <param name="data">4-bit pixel array of bytes.</param>
        /// <returns></returns>
        public static byte[] FixPixelOrder(byte[] data)
        {
            byte[] x = data;

            for (int i = 0; i < x.Length; i++)
                data[i] = (byte)(((data[i] & 0x0F) << 4) | ((data[i] & 0xF0) >> 4));

            return x;
        }
        public byte[] FixBitmapData(byte[] b, int width, int height)
        {
            byte[] data;

            if (width % 4 != 0)
            {
                int newWidth = width + width % 4;
                data = new byte[newWidth * height];
                for (int i = 0; i < height; i++)
                {
                    Buffer.BlockCopy(
                    b, i * width,
                    data, i * newWidth,
                    width);
                }
                return data;
            }

            return b;
        }


        /// <summary>
        /// Returns PS1 palette (CLUT) for corresponding texture layout.
        /// </summary>
        /// <param name="tl">Texture layout data.</param>
        public ushort[] GetCtrClut(TextureLayout tl)
        {
            ushort[] buf = new ushort[16];

            int ptr = tl.PalPosition * 2;

            Buffer.BlockCopy(
                this.data, ptr,
                buf, 0,
                16 * 2);

            return buf;
        }

        /// <summary>
        /// Converts PS1 palette (CLUT) to 32 bit BMP palette.
        /// </summary>
        /// <param name="clut">Array of 32 bytes.</param>
        public byte[] CtrClutToBmpPalette(ushort[] clut)
        {
            byte[] pal = new byte[16 * 4];

            // pals++;


            for (int i = 0; i < 16; i++)
            {
                Color c = Convert16(clut[i], true);

                // palbmp.SetPixel(i, pals, c);

                pal[i * 4] = (byte)(c.b*255f);
                pal[i * 4 + 1] = (byte)(c.g*255f);
                pal[i * 4 + 2] = (byte)(c.r*255f);
                pal[i * 4 + 3] = (byte)(c.a*255f);
            }


            return pal;
        }

        /// <summary>
        /// Converts 5-5-5-1 16 bit color to 8-8-8-8 32 bit color.
        /// </summary>
        /// <param name="col">16 bit ushort color value.</param>
        /// <param name="useAlpha">Defines whether alpha value should be preserved.</param>
        /// <returns></returns>
        public static Color Convert16(ushort col, bool useAlpha)
        {
            byte r = (byte)(((col >> 0) & 0x1F) << 3);
            byte g = (byte)(((col >> 5) & 0x1F) << 3);
            byte b = (byte)(((col >> 10) & 0x1F) << 3);
            byte a = (byte)((col >> 15) * 255);

            //um...
            if (a != 255 && r == 0 && g == 0 & b == 0)
            {
                r = 255;
                g = 0;
                b = 255;
            }

            return new Color((useAlpha ? a : 255)/255f, r/255f, g/255f, b/255f);
        }

        public static ushort ConvertTo16(Color c)
        {
            return ConvertTo16((byte)(c.r*255f), (byte)(c.g*255f), (byte)(c.b*255f));
        }

        public static ushort ConvertTo16(byte r, byte g, byte b)
        {
            return (ushort)((r >> 3 << 10) | (g >> 3 << 5) | (b >> 3 << 0));
        }

        public static Color Convert16(byte[] b, bool useAlpha)
        {
            ushort val = BitConverter.ToUInt16(b, 0);
            return Convert16(val, useAlpha);
        }
    }

}