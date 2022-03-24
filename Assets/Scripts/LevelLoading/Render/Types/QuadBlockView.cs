using System;
using CTRFramework;
using UnityEngine;
using CTRFramework.Shared;
using UnityEngine.Serialization;

namespace OpenCTR.Level
{
        public class QuadBlockView : MonoBehaviour
        {
                public QuadFlags quadFlags;
                public uint bitvalue;

                //these values are contained in bitvalue, mask is 8b5b5b5b5b4z where b is bit and z is empty. or is it?
                public byte drawOrderLow;
                public FaceFlags[] faceFlags = new FaceFlags[4];
                public uint extradata;

                public byte[] drawOrderHigh = new byte[4];

                public PsxPtr[] ptrTexMid = new PsxPtr[4]; //offsets to mid texture definition
                public String[] ptrHighTex = new String[4]; //high ptrs

                public TerrainFlags terrainFlag;
                public byte WeatherIntensity;
                public byte WeatherType;
                public byte TerrainFlagUnknown; //almost always 0, only found in tiger temple and sewer speedway

                public short id;
                public byte trackPos;
                public String midunk;


                public PsxPtr ptrTexLow; //offset to LOD texture definition
                public PsxPtr ptrAddVis; //pointes to 4 extra visData structs, to be renamed

                public HiddenBits mosaicPtr1;
                public HiddenBits mosaicPtr2;
                public HiddenBits mosaicPtr3;
                public HiddenBits mosaicPtr4;
                public String otherExtra;
                public string ptrhi;
                public string testData;
                public VisInstance VisPointer;
                public byte flag;
                public VisDataFlags visflag;
                public byte unk0vis;
                public uint unk1vis;
                public int highTexturesCount;
                public string fdata;
                public String midptr;
        }
}