using CTRFramework.Shared;
using CTRFramework.Vram;
using System;
using System.Collections.Generic;

namespace CTRFramework
{
    public class CtrTex
    {
        public TextureLayout[] midlods = new TextureLayout[3];
        public TextureLayout[] hi = new TextureLayout[16];
        public List<TextureLayout> animframes = new List<TextureLayout>(); //this actually has several lods too

        public uint ptrHi;
        public uint highExtra;
        public uint testData;
        public bool isAnimated = false;

        public bool hasHighDetail;
        public UIntPtr address;


        public CtrTex(BinaryReaderEx br, PsxPtr ptr)
        {
            Read(br, ptr);
        }

        public static uint highestPtr = 0;
        public void Read(BinaryReaderEx br, PsxPtr ptr)
        {
            address = (UIntPtr)br.BaseStream.Position;

            if (ptr.ExtraBits == HiddenBits.Bit1)
            {
                Console.WriteLine("!!!");
                Console.ReadKey();
            }

            //this apparently defines animated texture, really
            if (ptr.ExtraBits == HiddenBits.Bit0)
            {
                isAnimated = true;

                uint texpos = br.ReadUInt32();
                int numFrames = br.ReadInt16();
                int whatsthat = br.ReadInt16();

                if (whatsthat != 0)
                    Helpers.Panic(this, PanicType.Assume, $"whatsthat is not null! {whatsthat}");

                if (br.ReadUInt32() != 0)
                    Helpers.Panic(this, PanicType.Assume, "not 0!");

                uint[] ptrs = br.ReadArrayUInt32(numFrames);

                foreach (uint ptrAnimFrame in ptrs)
                {
                    br.Jump(ptrAnimFrame);
                    animframes.Add(TextureLayout.FromReader(br));
                }

                br.Jump(texpos);
            }

            for (int i = 0; i < 3; i++)
                midlods[i] = TextureLayout.FromReader(br);

            //Console.WriteLine(br.BaseStream.Position.ToString("X8"));
            //Console.ReadKey();

            //if (mosaic != 0)
            {
                ptrHi = br.ReadUInt32();
                highExtra = highExtra >> 9;
                
                testData = br.ReadUInt32();

                if (Scene.ReadHiTex)
                {
                    //loosely assume we got a valid pointer
                    if (ptrHi > 0x20000 && ptrHi < 0xC0000)
                    {
                        br.Jump(ptrHi);

                        for (int i = 0; i < 16; i++)
                            hi[i] = TextureLayout.FromReader(br);
                        hasHighDetail = true;
                    }
                }
            }
        }
    }
}