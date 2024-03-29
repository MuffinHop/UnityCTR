﻿using System;
using System.Collections.Generic;
using System.Drawing;
using System.Text;
using CTRFramework.Shared;

namespace CTRFramework.Vram
{
    public enum BlendingMode
    {
        Translucent = 0,
        Additive = 1,
        Subtractive = 2,
        Standard = 3
    }

    public enum BitDepth
    {
        Bit4 = 0,
        Bit8 = 1,
        Bit16 = 2,
        Bit24 = 3
    }
    public class TextureLayout : IRead
    {
        public static readonly int SizeOf = 0x0C;

        public uint offset;

        public List<Vector2b> uv = new List<Vector2b>() { new Vector2b(0, 0), new Vector2b(0, 0), new Vector2b(0, 0), new Vector2b(0, 0) };
        public List<Vector2b> normuv = new List<Vector2b>();

        public ushort PalX;
        public ushort PalY;

        public ushort PageX;
        public ushort PageY;

        public byte check;
        public BlendingMode blendingMode;


        public byte f1;
        public byte f2;
        public byte f3;

        public Point min
        {
            get
            {
                return new Point(
                    Math.Min(Math.Min(uv[0].X, uv[1].X), Math.Min(uv[2].X, uv[3].X)),
                    Math.Min(Math.Min(uv[0].Y, uv[1].Y), Math.Min(uv[2].Y, uv[3].Y))
                    );
            }
        }
 
        public Point max
        {
            get
            {
                return new Point(
                    Math.Max(Math.Max(uv[0].X, uv[1].X), Math.Max(uv[2].X, uv[3].X)),
                    Math.Max(Math.Max(uv[0].Y, uv[1].Y), Math.Max(uv[2].Y, uv[3].Y))
                    );
            }
        }

        public int width
        {
            get
            {
                return max.X - min.X + 1; //uv[1].X - uv[0].X + 1; }
            }
        }

        public int height
        {
            get
            {
                return max.Y - min.Y + 1;// return uv[3].Y - uv[0].Y + 1; }
            }
        }

        public int Position
        {
            get
            {
                return PageY * (CtrVrm.Height * CtrVrm.Width / 2) + min.Y * CtrVrm.Width + PageX * 64 + min.X / 4;
            }
        }

        public int PalPosition
        {
            get
            {
                return CtrVrm.Width * PalY + PalX * 16;
            }
        }

        public int RealX
        {
            get
            {
                return PageX * 64 + min.X / 4;
            }
        }
        public int RealY
        {
            get
            {
                return PageY * 256 + min.Y;
            }
        }

        public Rectangle frame
        {
            get
            {
                return new Rectangle(RealX, RealY, width / 4, height);
            }
        }

        public TextureLayout(BinaryReaderEx br)
        {
            Read(br);
        }

        public void NormalizeUV()
        {
            normuv.Clear();

            foreach (Vector2b v in uv)
            {
                Vector2b n = new Vector2b(0, 0);
                n.X = (byte)(Helpers.Normalize(min.X, max.X, v.X) * 255);
                n.Y = (byte)(Helpers.Normalize(min.Y, max.Y, v.Y) * 255);
                normuv.Add(n);
            }
        }

        public TextureLayout()
        {
        }

        public static TextureLayout FromReader(BinaryReaderEx br, bool skipcheck = false)
        {
            int test = br.ReadInt32();

            if (test == 0 && !skipcheck)
            {
                Helpers.Panic("TextureLayout", PanicType.Assume, "test failed");
                return null;
            }

            br.BaseStream.Position -= 4;
            return new TextureLayout(br);
        }

        public void Read(BinaryReaderEx br)
        {
            uv.Clear();

            offset = (uint)br.BaseStream.Position;

            uv.Add(new Vector2b(br));

            ushort buf = br.ReadUInt16();

            PalX = (ushort)(buf & 0x3F);
            PalY = (ushort)(buf >> 6);

            uv.Add(new Vector2b(br));

            buf = br.ReadUInt16();
            
            PageX = (ushort)(buf & 0xF);
            PageY = (ushort)((buf >> 4) & 1);

            blendingMode = (BlendingMode)((buf >> 5) & 3);
            byte rest = (byte)((buf >> 9) & 0x7F);

            if (rest != 0)
                Helpers.Panic(this, PanicType.Assume, $"rest = {rest}");

            uv.Add(new Vector2b(br));
            uv.Add(new Vector2b(br));

            NormalizeUV();

            //Console.WriteLine($"{Tag()}: f1 f2 f3 {f1} {f2} {f3}");

            /*
            //apparently some textures uses 8 bit mode, or even 16 bit color. must be a flag somewhere.
            if (offset == 0x0004B5B8 || offset == 0x0004B5E8 || offset == 0x0004B6A8 || offset == 0x0004B3D8)
            {

                Console.WriteLine(ToString());
                //Console.ReadKey();
            }
            */

            if (br.BaseStream.Position - offset != SizeOf)
                throw new Exception("TextureLayout: size mismatch");
        }

        private string _tag = "";

        //meant to be unique
        public string Tag()
        {
            if (_tag == "")
                _tag = $"{RealX}_{RealY}_{PalX}_{PalY}_{width}_{height}";

            return _tag;
            // return offset.ToString("X8");

            /*
            return PageX.ToString("X2") + PageY.ToString("X2") + "_" +
                PalX.ToString("X4") + PalY.ToString("X4") + "_" + uv[0].X.ToString("X2") + uv[0].Y.ToString("X2");
                */
        }

        public override string ToString()
        {
            return $"offset: {offset.ToString("X8")}\r\n\tUV: ({uv[0].ToString()}, {uv[1].ToString()}, {uv[2].ToString()}, {uv[3].ToString()})\r\n\tpalette: ({PalX}, {PalY})\r\n\tpage: ({PageX}, {PageY})";
        }

        public string Dump()
        {
            return $"{PageX}\t{PageY}\t{min}\t{width}\t{height}\t{PalX}\t{PalY}\t{Tag()}";
        }

        public string ToObj()
        {
            StringBuilder sb = new StringBuilder();

            //this is to avoid negative UV and make it clamp friendly
            int[] inds = new int[4] { 2, 3, 0, 1 };

            for (int i = 0; i < 4; i++)
            {
                sb.AppendFormat(
                    "vt {0} {1}\r\n",
                    normuv[inds[i]].X,
                    normuv[inds[i]].Y
                );
            }

            /*
            foreach (Vector2b v in normuv)
                sb.AppendFormat(
                    "vt {0} {1}\r\n",

                    Math.Round(v.X * 1.0, 3).ToString(),
                    Math.Round(v.Y * 1.0, 3).ToString()
                );
                */
            sb.AppendFormat("\r\nusemtl {0}\r\n", Tag());

            return sb.ToString();
        }

        public void Write(BinaryWriterEx bw)
        {
            //filler, add valid value
            uv[0].Write(bw);
            bw.Write((ushort)0);
            uv[1].Write(bw);
            bw.Write((ushort)0);
            uv[2].Write(bw);
            uv[3].Write(bw);
        }
    }
}
