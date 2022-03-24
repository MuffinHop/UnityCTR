using CTRFramework.Shared;
using System.Collections.Generic;
using CTRFramework;
using UnityEngine;

namespace OpenCTR.Level
{
    class DataConverter
    {
        public static Vector3 ToVector3(Vector3s vector, float scale = 1.0f)
        {
            return new Vector3(vector.X * scale, vector.Y * scale, vector.Z * scale);
        }

        public static Vector3 ToVector3(Color color)
        {
            return new Vector3(color.r , color.g, color.b);
        }

        public static Vector3 ToVector3(Vector3 vector, float scale = 1.0f)
        {
            return new Vector3(vector.x, vector.y, vector.z) * scale;
        }
        public static Vector3 ToVector3(Vector4s s, float scale = 1.0f)
        {
            return new Vector3(s.X * scale, s.Y * scale, s.Z * scale);
        }

        public static Color ToColor(Vector4b s)
        {
            return new Color(s.X, s.Y, s.Z, s.W);
        }

        public static VertexPositionColorTexture ToVptc(Vertex v, Vector2b uv, float scale = 1.0f)
        {
            VertexPositionColorTexture mono_v = new VertexPositionColorTexture();
            mono_v.Position = ToVector3(v.Position, scale);
            mono_v.Color = new Color(
                v.Color.X / 255f,
                v.Color.Y / 255f,
                v.Color.Z / 255f
                );
            mono_v.TextureCoordinate = new Vector2(uv.X / 255.0f, uv.Y / 255.0f);
            return mono_v;
        }

        public static TriList ToTriList(CTRFramework.CtrModel model)
        {
            List<VertexPositionColorTexture> li = new List<VertexPositionColorTexture>();

            foreach (var x in model.Entries[0].verts)
                li.Add(DataConverter.ToVptc(x, new Vector2b(0, 0), 0.01f));

            TriList t = new TriList();
            t.textureEnabled = false;
            t.textureName = "test";
            t.ScrollingEnabled = false;
            t.PushTri(li);
            t.Seal();

            return t;
        }
    }
}