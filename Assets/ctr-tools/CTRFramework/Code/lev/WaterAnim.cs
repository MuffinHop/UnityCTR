using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using CTRFramework.Shared;
using UnityEngine;

namespace CTRFramework
{
    public class WaterAnim
    {
        public PsxPtr ptrVertex;
        public PsxPtr ptrWaterAnim;
        public Color[] AlphaAnimation = new Color[28];

        public WaterAnim(BinaryReaderEx br)
        {
            Read(br);
        }

        public static WaterAnim FromReader(BinaryReaderEx br)
        {
            return new WaterAnim(br);
        }

        public void Read(BinaryReaderEx br)
        {
            ptrVertex = PsxPtr.FromReader(br);
            ptrWaterAnim = PsxPtr.FromReader(br);
            var currentPointer = br.BaseStream.Position;
            br.Jump(ptrWaterAnim.Address);
            for (int i = 0; i < 28; i++)
            {
                var color4b = br.ReadUInt16();
                var a = (color4b & 0x003f) / 63f;
                var b = ( (color4b & 0x0fc0) >> 6 ) / 63f;
                var c = ( (color4b & 0xf000) >> 12 ) / 15f;
                AlphaAnimation[i] = new Color(
                    b,
                    b,
                    Mathf.Max(b,a),
                    1f-c
                        );
            }
            br.Jump(currentPointer);
        }
    }
}