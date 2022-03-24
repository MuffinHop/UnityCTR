using System;
using System.Collections.Generic;
using UnityEngine;

namespace OpenCTR.Level
{
    public class TriList 
    {
        public bool Sealed = false;
        public List<VertexPositionColorTexture> verts = new List<VertexPositionColorTexture>();
        private VertexPositionColorTexture[] verts_sealed;
        public List<Color> beginColor = new List<Color>();
        public List<Color> endColor = new List<Color>();
        public Dictionary<int, List<Color>> animatedColors = new Dictionary<int, List<Color>>();
        public bool vColAnimEnabled = true;
        public bool textureEnabled = true;
        public bool ScrollingEnabled = false;
        public bool CullingEnabled = true;
        public string textureName = "";
        private short[] indices;

        float lerp = 0;

        public int numVerts => verts.Count;

        public int numFaces => indices.Length / 3;

        public void SetColor(Color c)
        {
            for (int i = 0; i < numVerts; i++)
            {
                VertexPositionColorTexture v = verts[i];
                v.Color = c;
                verts[i] = v;
            }
        }

        public TriList()
        {
        }

        public void Seal()
        {
            indices = GenerateIndices().ToArray();
            verts_sealed = verts.ToArray();
            Sealed = true;
            for (int i = 0; i < numVerts; i++)
            {
                beginColor.Add(verts[i].Color);
                endColor.Add(Color.white);
            }
        }

        public void PushTri(List<VertexPositionColorTexture> lv)
        {
            if (Sealed)
            {
                Console.WriteLine("Trying to update sealed list.");
                return;
            }

            if (lv != null)
                verts.AddRange(lv);
        }

        public void PushQuad(List<VertexPositionColorTexture> lv)
        {
            if (Sealed)
            {
                Console.WriteLine("Trying to update sealed list.");
                return;
            }

            if (lv != null)
                verts.AddRange(new List<VertexPositionColorTexture>() { lv[0], lv[1], lv[2], lv[2], lv[1], lv[3] });
        }

        bool forward = true;

        public void Update()
        {
            if (!vColAnimEnabled && !ScrollingEnabled)
                return;

            if (ScrollingEnabled)
            {
                for (int i = 0; i < numVerts; i++)
                {
                    verts_sealed[i].TextureCoordinate.y -= Time.time * 3; //this will potentially overflow
                }
            }

            if (vColAnimEnabled)
            {
                for (int i = 0; i < numVerts; i++)
                {
                    verts_sealed[i].Color = Color.Lerp(beginColor[i], endColor[i], forward ? lerp : 1 - lerp);
                }
            }

            lerp += Time.time;
            if (lerp > 1)
            {
                lerp -= 1;
                forward = !forward;
            }
        }

        public TriList(List<VertexPositionColorTexture> v, bool te, string name = "")
        {
            verts.AddRange(v);
            textureEnabled = te;
            textureName = name;
        }

        public TriList(TriList t)
        {
            verts.AddRange(t.verts);
            textureEnabled = t.textureEnabled;
            textureName = t.textureName;
            GenerateIndices();
        }

        public List<short> GenerateIndices()
        {
            List<short> indices = new List<short>();

            for (int i = 0; i < numVerts; i++)
                indices.Add((short)i);

            return indices;
        }
        
    }
}
