using CTRFramework;
using System.Collections.Generic;
using UnityEngine;

namespace OpenCTR.Level
{
    public class MGLevel
    {
        public TriList wire = new TriList();

        public Dictionary<string, TriList> normalq = new Dictionary<string, TriList>();
        public Dictionary<string, TriList> waterq = new Dictionary<string, TriList>();
        public Dictionary<string, TriList> alphaq = new Dictionary<string, TriList>();
        public Dictionary<string, TriList> animatedq = new Dictionary<string, TriList>();

        public TriList wireq = new TriList();

        public Dictionary<string, TriList> flagq = new Dictionary<string, TriList>();

        public MGLevel()
        {
        }

        public List<string> textureList
        {
            get
            {
                List<string> list = new List<string>();

                foreach (var n in normalq)
                    if (!list.Contains(n.Key))
                        list.Add(n.Key);

                foreach (var n in alphaq)
                    if (!list.Contains(n.Key))
                        list.Add(n.Key);

                foreach (var n in animatedq)
                    if (!list.Contains(n.Key))
                        list.Add(n.Key);

                foreach (var n in waterq)
                    if (!list.Contains(n.Key))
                        list.Add(n.Key);

                return list;
            }
        }

        public MGLevel(SkyBox sb)
        {
            TriList normal = new TriList();

            normal.textureEnabled = false;
            normal.ScrollingEnabled = false;

            for (int i = 0; i < sb.Faces.Count; i++)
            {
                List<VertexPositionColorTexture> tri = new List<VertexPositionColorTexture>();
                tri.Add(DataConverter.ToVptc(sb.Vertices[(int)sb.Faces[i].X], new CTRFramework.Shared.Vector2b(0, 0)));
                tri.Add(DataConverter.ToVptc(sb.Vertices[(int)sb.Faces[i].Y], new CTRFramework.Shared.Vector2b(0, 0)));
                tri.Add(DataConverter.ToVptc(sb.Vertices[(int)sb.Faces[i].Z], new CTRFramework.Shared.Vector2b(0, 0)));

                normal.PushTri(tri);
            }

            normal.Seal();

            normalq.Add("test", normal);

            wire = new TriList(normal);
            wire.SetColor(Color.red);
        }

        public void Push(Dictionary<string, TriList> dict, string name, List<VertexPositionColorTexture> monolist, string custTex = "")
        {
            if (!dict.ContainsKey(name))
            {
                TriList ql = new TriList(new List<VertexPositionColorTexture>() { }, true, (custTex != "" ? custTex : name));
                dict.Add(name, ql);
            }

            dict[name].PushQuad(monolist);
        }

        public void Seal()
        {
            foreach (var ql in normalq)
                ql.Value.Seal();

            foreach (var ql in waterq)
                ql.Value.Seal();

            foreach (var ql in alphaq)
                ql.Value.Seal();

            foreach (var ql in animatedq)
                ql.Value.Seal();

            foreach (var ql in flagq)
                ql.Value.Seal();
        }
    }
}