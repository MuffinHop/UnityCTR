using System;
using System.Collections;
using System.Collections.Generic;
using System.Data;
using System.IO;
using System.Linq;
using System.Numerics;
using System.Threading;
using CTRFramework;
using CTRFramework.Shared;
using CTRFramework.Vram;
using OGdata.MetaPhys;
using TMPro;
using UI;
using UnityEditor;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.SocialPlatforms.Impl;
using UnityEngine.VFX;
using Pose = CTRFramework.Shared.Pose;
using Scene = CTRFramework.Scene;
using Vector2 = UnityEngine.Vector2;
using Vector3 = UnityEngine.Vector3;


namespace OpenCTR.Level
{
    public class LevelHandler : MonoBehaviour
    {
        [SerializeField] private string filename;
        [SerializeField] private string name;
        [SerializeField] private bool sceneDebug = false;
        [SerializeField] private Transform quadsTransform;
        [SerializeField] private Transform bspTransform;
        [SerializeField] private bool _loadEntities;
        [SerializeField] private PhysicMaterial _physicsMaterial;
        [SerializeField] private LapUI _lapUI;
        
        
        public SceneHeader header;
        public MeshInfo mesh;
        public int MaxTrackPosition => _maxTrackPosition;
        public LapUI LapUI => _lapUI;
        public bool ReadHiTex = true;
        public List<Vertex> verts = new List<Vertex>();
        public List<QuadBlock> quads = new List<QuadBlock>();
        public List<Pose> restartPts = new List<Pose>();
        public List< Texture2D> megaTextures;
        public List< Texture2D> lows;
        public List< Texture2D> mids;
        public List< Texture2D> highs;
        private Dictionary<string, GameObject> _sharedLevelGameObjects;
        private List<PickupHeader> pickups = new List<PickupHeader>();
        private List<VisData> visdata = new List<VisData>();
        private List<CtrModel> Models = new List<CtrModel>();
        private List<VertexAnim> vertanims = new List<VertexAnim>();
        private List<WaterAnim> waterAnim = new List<WaterAnim>();
        private SkyBox skybox;
        private Nav nav;
        private SpawnGroup spawnGroups;
        private TrialData trial;
        private IconPack iconpack;
        private CtrVrm vram;
        private Tim ctrvram;
        private List<Vector3s> posu2 = new List<Vector3s>();
        private Dictionary<string, Texture2D> midTextures;
        private Dictionary<string, Texture2D> lowTextures;
        private Dictionary<string, Texture2D> highTextures;
        private int _maxTrackPosition;
        [SerializeField] private List<MetaPhys>[] characterMetaPhysics;
        public void Start()
        {
            characterMetaPhysics = GetMetaPhysData.LoadMetaPhys();
            Application.targetFrameRate = 60;
            _sharedLevelGameObjects = new Dictionary<string, GameObject>();
            name = Path.GetFileNameWithoutExtension(filename);
            using (BinaryReaderEx br = new BinaryReaderEx(File.OpenRead(filename)))
            {
                Read(br);
            }

            WeldVertices(0.03333f); // Welding a bit for collisions.
            
            if (vram != null)
            {
                ctrvram = vram.GetVram();
                return;
            }

            InitTextures();

            InitMeshs();
            
            foreach (var quad in quads)
            {
                GenerateQuadGB(quad);
            }
            
            GenerateVis();
            CreateSkyBox(skybox);
            if (_loadEntities)
            {
                foreach (var entity in Models)
                {
                    CreateEntity(entity);
                }
            }

            
            AddCollider(asphaltMesh, "asphalt", "Asphalt", "Ground");
            AddCollider(dirtMesh, "dirt", "Dirt", "Ground");
            AddCollider(grassMesh, "grass", "Grass", "Ground");
            AddCollider(iceMesh, "ice", "Ice", "Ground");
            AddCollider(mudMesh, "mud", "Mud", "NonSolid");
            AddCollider(metalMesh, "metal", "Metal", "Ground");
            AddCollider(snowMesh, "snow", "Snow", "Ground");
            AddCollider(stoneMesh, "stone", "Stone", "Ground");
            AddCollider(trackMesh, "track", "Track", "Ground");
            AddCollider(waterMesh, "water", "Water", "NonSolid");
            AddCollider(woodMesh, "wood", "Wood", "Ground");
            AddCollider(fastWaterMesh, "fastWater", "FastWater", "Ground");
            AddCollider(hardPackMesh, "hardPack", "HardPack", "Ground");
            AddCollider(icyRoadMesh, "icyRoad", "IcyRoad", "Ground");
            AddCollider(oceanAsphaltMesh, "oceanAsphalt", "OceanAsphalt", "Ground");
            AddCollider(riverAsphaltMesh, "riverAsphalt", "RiverAsphalt", "Ground");
            AddCollider(sideSlipMesh, "sideSlip", "SideSlip", "Ground");
            AddCollider(slowDirtMesh, "slowDirt", "SlowDirt", "Ground");
            AddCollider(steamAsphaltMesh, "steamAsphalt", "SteamAsphalt", "Ground");
            AddCollider(wallMesh, "wallMesh", "Wall", "Wall");
            AddCollider(killRacerMesh, "killRacerMesh", "Kill", "Ground");
            AddCollider(turboPadMesh, "turboPadMesh", "TurboPad", "NonSolid");
            LoadSceneOnTop("Scenes/InLevelUI");
        }

        private void OnDestroy()
        {
            UnLoadSceneOnTop("Scenes/InLevelUI");
        }

        public void Update()
        {
            if (_lapUI == null) // bad code.
            {
                _lapUI = FindObjectOfType<LapUI>();
                if (_lapUI != null) // bad code.
                {
                    _lapUI.Setup(7);
                }
            }
        }
        

        public void LoadSceneOnTop(String scene)
        {
            SceneManager.LoadScene(scene, LoadSceneMode.Additive);
        }
 
        public void UnLoadSceneOnTop(String scene)
        {
            int n = SceneManager.sceneCount;
            if (n > 1)
            {
                SceneManager.UnloadSceneAsync(scene);
            }
        }
        public void SetVram(CtrVrm c)
        {
            vram = c;
            ctrvram = c.GetVram();
        }
        public void Read(BinaryReaderEx br)
        {
            ReadScene(PatchedContainer.FromReader(br).GetReader());
        }
        
        public void ReadScene(BinaryReaderEx br)
        {
            header = Instance<SceneHeader>.FromReader(br, 0);

            if (header == null)
                throw new DataException("Scene header is null. Halt parsing.");

            if (header.ptrMeshInfo != PsxPtr.Zero)
            {
                mesh = new PtrWrap<MeshInfo>(header.ptrMeshInfo).Get(br);
                quads = mesh.QuadBlocks;
                verts = mesh.Vertices;
                visdata = mesh.VisData;
            } 

            restartPts = new PtrWrap<Pose>(header.ptrRestartPts).GetList(br, header.numRestartPts);
            vertanims = new PtrWrap<VertexAnim>(header.ptrVcolAnim).GetList(br, header.numVcolAnim);
            skybox = new PtrWrap<SkyBox>(header.ptrSkybox).Get(br);
            nav = new PtrWrap<Nav>(header.ptrAiNav).Get(br);
            iconpack = new PtrWrap<IconPack>(header.ptrIcons).Get(br);
            trial = new PtrWrap<TrialData>(header.ptrTrialData).Get(br);

            if (header.numSpawnGroups > 0)
            {
                br.Jump(header.ptrSpawnGroups);
                spawnGroups = new SpawnGroup(br, (int)header.numSpawnGroups);
            }


            if (header.cntu2 > 0)
            {
                br.Jump(header.ptru2);

                int cnt = br.ReadInt32();
                int ptr = br.ReadInt32();


                br.Jump(ptr);

                for (int i = 0; i < cnt; i++)
                    posu2.Add(new Vector3s(br));
            }


            //find all water quads in visdata
            foreach (var node in visdata)
            {
                if (node.IsLeaf)
                {
                    if (node.flag.HasFlag(VisDataFlags.Water))
                    {
                        int z = (int)((node.ptrQuadBlock - mesh.ptrQuadBlocks.ToUInt32()) / 0x5C);

                        for (int i = z; i < z + node.numQuadBlock; i++)
                            quads[i].isWater = true;
                    }
                    SetPointer(node);
                }
            }

            br.Jump(header.ptrWater);
            for (int i = 0; i < header.numWater; i++)
            {
                waterAnim.Add(WaterAnim.FromReader(br));
            }
            
            //assign anim color target to vertex
            foreach (var va in vertanims)
            {
                verts[(int)((va.ptrVertex - mesh.ptrVertices.ToUInt32()) / 16)].color_target = va.color;
            }


            /*
             //water texture
            br.BaseStream.Position = header.ptrWater;

            List<uint> vptr = new List<uint>();
            List<uint> wptr = new List<uint>();

            for (int i = 0; i < header.cntWater; i++)
            {
                vptr.Add(br.ReadUInt32());
                wptr.Add(br.ReadUInt32());
            }

            wptr.Sort();

            foreach(uint u in wptr)
            {
                Console.WriteLine(u.ToString("X8"));
            }

            Console.ReadKey();
            */

            //read pickups
            for (int i = 0; i < header.numInstances; i++)
            {
                br.Jump(header.ptrInstancesPtr.Address + 4 * i);
                br.Jump(br.ReadUInt32());

                pickups.Add(PickupHeader.FromReader(br));
            }


            br.Jump(header.ptrModelsPtr);

            List<uint> modelPtr = br.ReadListUInt32(header.numModels);

            foreach (var ptr in modelPtr)
            {
                br.Jump(ptr);

                try
                {
                    CtrModel ctr = CtrModel.FromReader(br);
                    if (ctr != null)
                        Models.Add(ctr);
                }
                catch
                {
                    Helpers.Panic(this, PanicType.Error, "Unexpected CtrModel crash.");
                }
            }

            foreach (VertexAnim va in vertanims)
            {
                Helpers.Panic(this, PanicType.Info, va.ToString());
            }

            foreach (var scrollTexture in header.animatedTextureAddresses)
            {
                Debug.Log(scrollTexture.ToUInt32().ToString("X"));
            }

            
            
            if (sceneDebug)
            {
                //SceneTests();
            }

        }

        private void InitTextures()
        {
            string vrmpath = Path.ChangeExtension(filename, ".vrm");

            if (File.Exists(vrmpath))
            {
                Debug.Log("VRAM found!");
                vram = CtrVrm.FromFile(vrmpath);

                SetVram(CtrVrm.FromFile(vrmpath));
                
                    bool vram_path_is = File.Exists(vrmpath);
                    if (vram_path_is)
                    {
                        Debug.Log("VRAM found!");
                        
                        Dictionary<string, TextureLayout> dictionary = GetTexturesList(Detail.Med);
                        midTextures = new Dictionary<string, Texture2D>();
                        foreach(KeyValuePair<string, TextureLayout> entry in dictionary)
                        {
                            Tim texture = ctrvram.GetTimTexture(entry.Value);
                            Texture2D tex2D = new Texture2D(texture.region.Width*4, texture.region.Height, TextureFormat.ARGB32, false);
                            for (int y = 0; y < texture.region.Height; y++)
                            {
                                for (int x = 0; x < texture.region.Width; x++)
                                {
                                    var clutValue = texture.data[x + texture.region.Width * y];
                                    for (int clut = 0; clut < 4*4; clut+=4)
                                    {
                                        var pixelValue = texture.clutdata[(clutValue>>clut)&15];
                                
                                        float pixelRed = pixelValue & 31;
                                        pixelValue = (ushort) (pixelValue >> 5);
                                        float pixelGreen = pixelValue & 31;
                                        pixelValue = (ushort) (pixelValue >> 5);
                                        float pixelBlue = pixelValue & 31;
                                        pixelValue = (ushort) (pixelValue >> 5);
                                        int pixelAlpha = pixelValue & 1;
                                        tex2D.SetPixel(x*4 + clut/4,y,new Color(pixelRed/31f,pixelGreen/31f,pixelBlue/31f, pixelAlpha == 0 ? 0f : 1.0f));
                                    }
                                }
                            }
                            tex2D.Apply();
                            tex2D.filterMode = FilterMode.Point;
                            midTextures.Add( entry.Key, tex2D);
                            tex2D.name = entry.Key;
                            mids.Add(tex2D);
                            var path = Application.dataPath + "/TextureCache/mid/" + tex2D.name + ".png";
                            File.WriteAllBytes(path, tex2D.EncodeToPNG()); 
                        }
                        dictionary = GetTexturesList(Detail.Low);
                        lowTextures = new Dictionary<string, Texture2D>();
                        foreach(KeyValuePair<string, TextureLayout> entry in dictionary)
                        {
                            Tim texture = ctrvram.GetTimTexture(entry.Value);
                            Texture2D tex2D = new Texture2D(texture.region.Width*4, texture.region.Height, TextureFormat.ARGB32, false);
                            for (int y = 0; y < texture.region.Height; y++)
                            {
                                for (int x = 0; x < texture.region.Width; x++)
                                {
                                    var clutValue = texture.data[x + texture.region.Width * y];
                                    for (int clut = 0; clut < 4*4; clut+=4)
                                    {
                                        var pixelValue = texture.clutdata[(clutValue>>clut)&15];
                                
                                        float pixelRed = pixelValue & 31;
                                        pixelValue = (ushort) (pixelValue >> 5);
                                        float pixelGreen = pixelValue & 31;
                                        pixelValue = (ushort) (pixelValue >> 5);
                                        float pixelBlue = pixelValue & 31;
                                        pixelValue = (ushort) (pixelValue >> 5);
                                        int pixelAlpha = pixelValue & 1;
                                        tex2D.SetPixel(x*4 + clut/4,y,new Color(pixelRed/31f,pixelGreen/31f,pixelBlue/31f, pixelAlpha == 0 ? 0f : 1.0f));
                                    }
                                }
                            }
                            tex2D.Apply();
                            tex2D.filterMode = FilterMode.Point;
                            lowTextures.Add( entry.Key, tex2D);
                            tex2D.name = entry.Key;
                            lows.Add(tex2D);
                            var path = Application.dataPath + "/TextureCache/low/" + tex2D.name + ".png";
                            File.WriteAllBytes(path, tex2D.EncodeToPNG()); 
                        }
                        dictionary = GetTexturesList(Detail.High);
                        highTextures = new Dictionary<string, Texture2D>();
                        foreach(KeyValuePair<string, TextureLayout> entry in dictionary)
                        {
                            Tim texture = ctrvram.GetTimTexture(entry.Value);
                            Texture2D tex2D = new Texture2D(texture.region.Width*4, texture.region.Height, TextureFormat.ARGB32, false);
                            for (int y = 0; y < texture.region.Height; y++)
                            {
                                for (int x = 0; x < texture.region.Width; x++)
                                {
                                    var clutValue = texture.data[x + texture.region.Width * y];
                                    for (int clut = 0; clut < 4*4; clut+=4)
                                    {
                                        var pixelValue = texture.clutdata[(clutValue>>clut)&15];
                                
                                        float pixelRed = pixelValue & 31;
                                        pixelValue = (ushort) (pixelValue >> 5);
                                        float pixelGreen = pixelValue & 31;
                                        pixelValue = (ushort) (pixelValue >> 5);
                                        float pixelBlue = pixelValue & 31;
                                        pixelValue = (ushort) (pixelValue >> 5);
                                        int pixelAlpha = pixelValue & 1;
                                        tex2D.SetPixel(x*4 + clut/4,y,new Color(pixelRed/31f,pixelGreen/31f,pixelBlue/31f, pixelAlpha == 0 ? 0f : 1.0f));
                                    }
                                }
                            }
                            tex2D.Apply();
                            tex2D.filterMode = FilterMode.Point;
                            highTextures.Add( entry.Key, tex2D);
                            tex2D.name = entry.Key;
                            highs.Add(tex2D);
                            var path = Application.dataPath + "/TextureCache/high/" + tex2D.name + ".png";
                            File.WriteAllBytes(path, tex2D.EncodeToPNG()); 
                        }
                    }
                    lows = lows.OrderBy(go=>go.name).ToList();
                    mids = mids.OrderBy(go=>go.name).ToList();
                    highs = highs.OrderBy(go=>go.name).ToList();
            }
        }
        private void CreateSkyBox(SkyBox sb)
        {
            var gb = new GameObject();
            gb.name = "LevelSkyBox";
            gb.transform.parent = transform;
            var filter = gb.AddComponent<MeshFilter>();
            var meshRenderer = gb.AddComponent<MeshRenderer>();
            meshRenderer.material = new Material(Shader.Find("Vertex Colors"));
            TriList skybox = new TriList();

            skybox.textureEnabled = false;
            skybox.ScrollingEnabled = false;
            List<Vector3> vertices = new List<Vector3>();
            List<Color> colors = new List<Color>();
            List<int> triangles = new List<int>();
            Vector3 skyboxOffset = new Vector3(0f, -32f, 0f);
            if (sb==null || sb.Faces.Count == 0) return;
            for (int i = 0; i < sb.Faces.Count; i++)
            {
                Color colora = new Color(
                    sb.Vertices[(int)sb.Faces[i].X].Color.X / 255f, 
                    sb.Vertices[(int)sb.Faces[i].X].Color.Y / 255f,
                    sb.Vertices[(int)sb.Faces[i].X].Color.Z / 255f,
                    sb.Vertices[(int)sb.Faces[i].X].Color.W / 255f
                );
                Color colorb = new Color(
                    sb.Vertices[(int)sb.Faces[i].Y].Color.X / 255f, 
                    sb.Vertices[(int)sb.Faces[i].Y].Color.Y / 255f,
                    sb.Vertices[(int)sb.Faces[i].Y].Color.Z / 255f,
                    sb.Vertices[(int)sb.Faces[i].Y].Color.W / 255f
                );
                Color colorc = new Color(
                    sb.Vertices[(int)sb.Faces[i].Z].Color.X / 255f, 
                    sb.Vertices[(int)sb.Faces[i].Z].Color.Y / 255f,
                    sb.Vertices[(int)sb.Faces[i].Z].Color.Z / 255f,
                    sb.Vertices[(int)sb.Faces[i].Z].Color.W / 255f
                );
                vertices.Add(sb.Vertices[(int)sb.Faces[i].X].Position * 32f + skyboxOffset);
                vertices.Add(sb.Vertices[(int)sb.Faces[i].Y].Position * 32f + skyboxOffset);
                vertices.Add(sb.Vertices[(int)sb.Faces[i].Z].Position * 32f + skyboxOffset);
                colors.Add(colora);
                colors.Add(colorb);
                colors.Add(colorc);
                triangles.Add(i*3 + 0);
                triangles.Add(i*3 + 1);
                triangles.Add(i*3 + 2);
            }

            filter.mesh = new Mesh
            {
                vertices = vertices.ToArray(),
                colors = colors.ToArray(),
                triangles = triangles.ToArray()
            };
        }

        private void CreateEntity(CtrModel model)
        {
            var gb = new GameObject();
            gb.name = model.Name;
            gb.transform.parent = transform;
            var filter = gb.AddComponent<MeshFilter>();
            var meshRenderer = gb.AddComponent<MeshRenderer>();
            meshRenderer.material = new Material(Shader.Find("Vertex Colors"));
            TriList skybox = new TriList();

            skybox.textureEnabled = false;
            skybox.ScrollingEnabled = false;
            List<Vector3> vertices = new List<Vector3>();
            List<Color> colors = new List<Color>();
            List<int> triangles = new List<int>();
            var srcVerts = model.Entries[0].verts;
            for (int i = 0; i < srcVerts.Count; i+=3)
            {
                Vertex srcVertexA = srcVerts[i + 0];
                Vertex srcVertexB = srcVerts[i + 1];
                Vertex srcVertexC = srcVerts[i + 2];
                Color colora = new Color(
                    srcVertexA.Color.X / 255f, 
                    srcVertexA.Color.Y / 255f,
                    srcVertexA.Color.Z / 255f,
                    srcVertexA.Color.W / 255f
                );
                Color colorb = new Color(
                    srcVertexB.Color.X / 255f, 
                    srcVertexB.Color.Y / 255f,
                    srcVertexB.Color.Z / 255f,
                    srcVertexB.Color.W / 255f
                );
                Color colorc = new Color(
                    srcVertexC.Color.X / 255f, 
                    srcVertexC.Color.Y / 255f,
                    srcVertexC.Color.Z / 255f,
                    srcVertexC.Color.W / 255f
                );
                vertices.Add(srcVertexA.Position / 50f);
                vertices.Add(srcVertexB.Position / 50f);
                vertices.Add(srcVertexC.Position / 50f);
                colors.Add(colora);
                colors.Add(colorb);
                colors.Add(colorc);
                triangles.Add(i + 0);
                triangles.Add(i + 1);
                triangles.Add(i + 2);
            }

            filter.mesh = new Mesh
            {
                vertices = vertices.ToArray(),
                colors = colors.ToArray(),
                triangles = triangles.ToArray()
            };
        }
        public Dictionary<string, TextureLayout> GetTexturesList(Detail lod)
        {
            Dictionary<string, TextureLayout> tex = new Dictionary<string, TextureLayout>();

            if (lod == Detail.Models)
            {
                foreach (var model in Models)
                    foreach (var entry in model.Entries)
                        foreach (TextureLayout tl in entry.tl)
                            if (!tex.ContainsKey(tl.Tag()))
                                tex.Add(tl.Tag(), tl);

                if (iconpack != null)
                    foreach (var i in iconpack.Icons.Values)
                        if (i.tl != null)
                        {
                            if (!tex.ContainsKey(i.tl.Tag()))
                                tex.Add(i.tl.Tag(), i.tl);
                        }
                        else
                        {
                            //hmm
                            Helpers.Panic(this, PanicType.Error, i.Name);
                        }
            }
            else
            {
                foreach (QuadBlock qb in quads)
                {
                    switch (lod)
                    {
                        case Detail.Low:
                            if (qb.ptrTexLow != UIntPtr.Zero)
                                if (!tex.ContainsKey(qb.texlow.Tag()))
                                    tex.Add(qb.texlow.Tag(), qb.texlow);
                            break;

                        case Detail.Med:
                            foreach (CtrTex t in qb.tex)
                                if (t != null)
                                    if (t.midlods[2].Position != 0)
                                        if (!tex.ContainsKey(t.midlods[2].Tag()))
                                            tex.Add(t.midlods[2].Tag(), t.midlods[2]);
                            break;

                        case Detail.High:
                            foreach (CtrTex t in qb.tex)
                                if (t != null)
                                    foreach (var x in t.hi)
                                        if (x != null)
                                            if (!tex.ContainsKey(x.Tag()))
                                                tex.Add(x.Tag(), x);
                            break;
                    }
                }
            }

            return tex;
        }


        public Dictionary<string, TextureLayout> GetTexturesList()
        {
            Dictionary<string, TextureLayout> tex = new Dictionary<string, TextureLayout>();

            foreach (var t in GetTexturesList(Detail.Low))
                if (!tex.ContainsKey(t.Key))
                    tex.Add(t.Key, t.Value);

            foreach (var t in GetTexturesList(Detail.Med))
                if (!tex.ContainsKey(t.Key))
                    tex.Add(t.Key, t.Value);

            foreach (var t in GetTexturesList(Detail.High))
                if (!tex.ContainsKey(t.Key))
                    tex.Add(t.Key, t.Value);

            foreach (var t in GetTexturesList(Detail.Models))
                if (!tex.ContainsKey(t.Key))
                    tex.Add(t.Key, t.Value);

            return tex;
        }

        /// <summary>
        /// Returns VisData children
        /// </summary>
        /// <param name="visData"></param>
        public List<VisData> GetVisDataChildren(VisData visData)
        {
            List<VisData> childVisData = new List<VisData>();

            if (visData.leftChild != 0 && !visData.IsLeaf) // in the future: handle leaves different. Draw them?
            {
                ushort uLeftChild = (ushort)(visData.leftChild & 0x3fff);
                VisData leftChild = visdata.Find(cc => cc.id == uLeftChild);
                childVisData.Add(leftChild);
            }

            if (visData.rightChild != 0 && !visData.IsLeaf) // in the future: handle leaves different. Draw them?
            {
                ushort uRightChild = (ushort)(visData.rightChild & 0x3fff);
                VisData rightChild = visdata.Find(cc => cc.id == uRightChild);
                childVisData.Add(rightChild);
            }

            return childVisData;
        }

        [SerializeField] private int levelShiftOffset = -52; // offset (found in Unity)
        [SerializeField] private int levelShiftDivide = 92; // one step width

        public int GetLevelShiftOffset()
        {
            return levelShiftOffset;
        }
        public int GetLevelShiftDivide()
        {
            return levelShiftDivide;
        }
        public void SetPointer(VisData leaf)
        {

            uint ptrQuadBlock = (uint)(((leaf.ptrQuadBlock) / levelShiftDivide) + levelShiftOffset);
            uint numQuadBlock = leaf.numQuadBlock;
            for (int i = 0; i < numQuadBlock; i++)
            {
                long index = ptrQuadBlock + i;
                QuadBlock quad = quads[(int)Math.Min(Math.Max(index, 0), quads.Count - 1)];
                leaf.QuadPointers.Add(quad);
            }
        }

        /// <summary>
        /// Return QuadBlocks associated with the leaf, make sure you pass a leaf and not a branch.
        /// </summary>
        /// <param name="leaf"></param>
        public List<QuadBlock> GetListOfLeafQuadBlocks(VisData leaf)
        {
            List<QuadBlock> leafQuadBlocks = new List<QuadBlock>();

            if (!leaf.IsLeaf)
                return leafQuadBlocks;

            uint ptrQuadBlock = (uint)(((leaf.ptrQuadBlock) / levelShiftDivide) + levelShiftOffset);
            uint numQuadBlock = leaf.numQuadBlock;
            for (int i = 0; i < numQuadBlock; i++)
            {
                long index = ptrQuadBlock + i;
                QuadBlock quad = quads[(int)Math.Min(Math.Max(index, 0), quads.Count - 1)];
                leafQuadBlocks.Add(quad);
            }

            return leafQuadBlocks;
        }

        public void Dispose()
        {
            header = null;
            mesh = null;
            verts.Clear();
            vertanims.Clear();
            quads.Clear();
            pickups.Clear();
            visdata.Clear();
            Models.Clear();
            skybox = null;
            nav = null;
            spawnGroups = null;
            restartPts.Clear();
            ctrvram = null;
        }

        private int meshCounter = 0;

        private Dictionary<String, Texture2D> fullResTexture = new Dictionary<string, Texture2D>();

        private Texture2D PushTextureToHighDetail(QuadBlock qb, int a)
        {
            var str = qb.tex[a].ptrHi.ToString();
            if (!fullResTexture.ContainsKey(str))
            {
                int w = qb.tex[a].hi[0].width;
                int h = qb.tex[a].hi[0].height;
                int target_w = 64;
                int target_h = 64;
                int len = qb.tex[a].hi.Length;
                int qbIndex = quads.IndexOf(qb);
                //var srcVisData = visdata.Find(v => v.QuadPointers.Contains(qb));
                var srcVisData = visdata.Find(v => (qbIndex >= ((v.ptrQuadBlock - mesh.ptrQuadBlocks.ToUInt32()) / 0x5C)) && (qbIndex < ((v.ptrQuadBlock - mesh.ptrQuadBlocks.ToUInt32()) / 0x5C) + v.numQuadBlock));
                var flag = srcVisData.flag;
                bool subdiv4x1 = flag.HasFlag(VisDataFlags.Subdiv4x1);
                bool subdiv4x2 = flag.HasFlag(VisDataFlags.Subdiv4x2);
                if (subdiv4x1)
                {
                    target_h = 128;
                    len = 8;
                } else if (subdiv4x2)
                {
                    target_h = 128;
                    len = 8;
                }
                Texture2D megaTexture = new Texture2D(256, 256  / (subdiv4x1 ? 2 : 1), mids[0].format, true);
                megaTexture.filterMode = FilterMode.Point;
                for (int hi = 0; hi < len / (subdiv4x1 ? 2 : 1); hi++)
                {
                    if (qb.tex[a].hi[hi] == null) continue; // could be null because the cell is black so what's the point drawing it.
                    float src_w = qb.tex[a].hi[hi].width;
                    float src_h = qb.tex[a].hi[hi].height;
                    var vector2Bs = qb.tex[a].hi[hi].normuv;
                    var vectors = new Vector2[4];
                    if (vector2Bs != null)
                    {
                        var uv0 = vector2Bs[0];
                        var uv1 = vector2Bs[1];
                        var uv2 = vector2Bs[2];
                        var uv3 = vector2Bs[3];
                        
                        vectors[0] = new Vector2((float)uv0.X / 255f, (float)uv0.Y / 255f);
                        vectors[1] = new Vector2((float)uv1.X / 255f, (float)uv1.Y / 255f);
                        vectors[2] = new Vector2((float)uv2.X / 255f, (float)uv2.Y / 255f);
                        vectors[3] = new Vector2((float)uv3.X / 255f, (float)uv3.Y / 255f);
                    }
                    for (int j = 0; j < target_h; j++)
                    {
                        for (int i = 0; i < target_w; i++)
                        {
                            float u = (float)i / (float)target_w;
                            float v = (float)j / (float)target_h;

                            Vector2 tuv = Vector2.Lerp(
                                Vector2.Lerp(vectors[0], vectors[1], u),
                                Vector2.Lerp(vectors[2], vectors[3], u),
                                v);
                            tuv.x = Mathf.Min(Mathf.Max(tuv.x,0f),1f);
                            tuv.y = Mathf.Min(Mathf.Max(tuv.y,0f),1f);
                            /*int src_x = (int)(Mathf.Lerp(ax, dx, (float)i / (float)target_w) * src_w);
                            int src_y = (int)(Mathf.Lerp(ay, dy, (float)j / (float)target_h) * src_h);*/
                            var t = highTextures[qb.tex[a].hi[hi].Tag()];
                            t.filterMode = FilterMode.Point;
                            t.wrapMode = TextureWrapMode.Mirror;
                            megaTexture.SetPixel((hi&3) * target_w + i, (int)(hi/4) * target_h + j,  t.GetPixel( Mathf.CeilToInt(tuv.x * src_w),Mathf.FloorToInt(tuv.y * src_h)) );
                        }
                    }
                    if (((int)(hi / 4) * h - qb.tex[a].hi[hi].height) > megaTexture.height) break;
                }

                megaTexture.name = str;
                megaTexture.Apply();
                fullResTexture.Add(str, megaTexture);
                return megaTexture;
            }
            return fullResTexture[str];
        }

        class WeldLimits
        {
            public int Start;
            public int End;
            public float MaxDelta;
        }

        public void ThreadWeld(object o)
        {
            WeldLimits weldLimits = (WeldLimits)o;
            for (int i = weldLimits.Start; i < weldLimits.End; i++)
            {
                for (int j = 0; j < verts.Count; j++)
                {
                    if (Vector3.Distance(verts[i].Position, verts[j].Position) < weldLimits.MaxDelta)
                    {
                        verts[i].Position = verts[j].Position;
                    }
                }
            }
        }

        private void WeldVertices(float aMaxDelta = 0.01f)
        {
            List<Thread> threads = new List<Thread>();
            int amount = verts.Count / Environment.ProcessorCount;
            for (int i = 0; i < Environment.ProcessorCount; i++)
            {
                WeldLimits limits = new WeldLimits();
                limits.Start = amount * i;
                limits.End = amount * (i + 1);
                limits.MaxDelta = aMaxDelta;
                Thread t = new Thread(ThreadWeld);
                threads.Add(t);
                t.Start(limits);
            }

            for (int i = 0; i < Environment.ProcessorCount; i++)
            {
                while(threads[i].IsAlive);
            }
            Debug.Log("All Threads done.");
        }
        
        private Mesh asphaltMesh;
        private Mesh dirtMesh;
        private Mesh grassMesh;
        private Mesh iceMesh;
        private Mesh mudMesh;
        private Mesh metalMesh;
        private Mesh snowMesh;
        private Mesh stoneMesh;
        private Mesh trackMesh;
        private Mesh waterMesh;
        private Mesh woodMesh;
        private Mesh fastWaterMesh;
        private Mesh hardPackMesh;
        private Mesh icyRoadMesh;
        private Mesh oceanAsphaltMesh;
        private Mesh riverAsphaltMesh;
        private Mesh sideSlipMesh;
        private Mesh slowDirtMesh;
        private Mesh steamAsphaltMesh;
        private Mesh wallMesh;
        private Mesh killRacerMesh;
        private Mesh turboPadMesh;

        private void AddCollider(Mesh mesh, String name, String tag, String layer)
        {
            GameObject go = new GameObject(name);
            go.name = name;
            go.transform.parent = transform;
            
            var collider = go.AddComponent<MeshCollider>();
            collider.sharedMesh = mesh;
            if (layer != "NonSolid")
            {
                collider.sharedMaterial = _physicsMaterial;
            }

            go.tag = tag;
            go.layer = LayerMask.NameToLayer(layer);
        }
        void InitMeshs()
        {
             asphaltMesh = new Mesh();
             dirtMesh =  new Mesh();
             grassMesh =  new Mesh();
             iceMesh =  new Mesh();
             mudMesh =  new Mesh();
             metalMesh =  new Mesh();
             snowMesh =  new Mesh();
             stoneMesh =  new Mesh();
             trackMesh =  new Mesh();
             waterMesh =  new Mesh();
             woodMesh =  new Mesh();
             fastWaterMesh =  new Mesh();
             hardPackMesh =  new Mesh();
             icyRoadMesh =  new Mesh();
             oceanAsphaltMesh =  new Mesh();
             riverAsphaltMesh =  new Mesh();
             sideSlipMesh =  new Mesh();
             slowDirtMesh =  new Mesh();
             steamAsphaltMesh =  new Mesh();
             wallMesh = new Mesh();
             killRacerMesh = new Mesh();
             turboPadMesh = new Mesh();
        }
        private Mesh CombineMeshes(Mesh A, Mesh B)
        {
            var combine = new CombineInstance[2];
            combine[0].mesh = A;
            combine[0].transform = transform.localToWorldMatrix;
            combine[1].mesh = B;
            combine[1].transform = transform.localToWorldMatrix;
            
            var mesh = new Mesh();
            mesh.CombineMeshes(combine);
            return mesh;
        }
        private void GenerateQuadGB(QuadBlock qb)
        {

            //for (int i = 0; i < 4; i++)
            {
                var uvs = new List<Vector2>();
                var uvs2 = new List<Vector2>();
                var vertices = new List<Vector3>();
                var colors = new List<Color>();
                List<Vertex> vertList = qb.GetVertexList(verts);
                var indices = new List<int>();
                for (int j = 0; j < vertList.Count; j++)
                {
                    Vertex vertA = vertList[j];
                    indices.Add(j);
                    vertices.Add(Vector3.Scale(vertA.Position, new Vector3(1f, 1f, -1f)));
                    uvs.Add(new Vector2(vertA.uv.x, vertA.uv.y));
                    uvs2.Add(new Vector2((float)qb.trackPos,0f));
                    colors.Add(new Color(vertA.Color.X / 255.0f, vertA.Color.Y / 255.0f, vertA.Color.Z / 255.0f, 1f - vertA.Color.W / 255.0f));
                }

                if (qb.trackPos != 255)
                {
                    _maxTrackPosition = Math.Max(_maxTrackPosition, qb.trackPos);
                }

                var gb = new GameObject();
                gb.name = "Mesh" + meshCounter++;
                gb.transform.parent = quadsTransform;
                if (
                    (qb.quadFlags & QuadFlags.InvisibleTriggers) == QuadFlags.InvisibleTriggers ||
                    (qb.quadFlags & QuadFlags.Invisible) == QuadFlags.Invisible)
                {
                    gb.layer = LayerMask.NameToLayer("Invisible");
                }
                else
                {
                    gb.layer = LayerMask.NameToLayer("Ground");
                }
                
                //MeshCollider meshCollider = gb.AddComponent<MeshCollider>();

                QuadBlockView quadBlockView = gb.AddComponent<QuadBlockView>();
                quadBlockView.bitvalue = qb.bitvalue;
                quadBlockView.drawOrderLow = qb.drawOrderLow;
                quadBlockView.faceFlags = qb.faceFlags;
                quadBlockView.extradata = qb.extradata;
                quadBlockView.otherExtra = "0x" + qb.otherExtra.ToString("X");
                quadBlockView.drawOrderHigh = qb.drawOrderHigh;
                quadBlockView.ptrTexMid = qb.ptrTexMid;
                quadBlockView.terrainFlag = qb.terrainFlag;
                quadBlockView.WeatherIntensity = qb.WeatherIntensity;
                quadBlockView.WeatherType = qb.WeatherType;
                quadBlockView.TerrainFlagUnknown = qb.TerrainFlagUnknown;
                quadBlockView.id = qb.id;
                quadBlockView.trackPos = qb.trackPos;
                quadBlockView.midunk = "0x" + qb.midunk.ToString("X");
                quadBlockView.ptrTexLow = qb.ptrTexLow;
                quadBlockView.ptrAddVis = qb.ptrAddVis;
                quadBlockView.ptrhi = qb.tex.Count > 0 ? qb.tex[0].ptrHi.ToString("X") : "";
                quadBlockView.testData = qb.tex.Count > 0 ? qb.tex[0].testData.ToString("X") : "";
                quadBlockView.highTexturesCount = qb.highTexturesCount;
                quadBlockView.quadFlags = qb.quadFlags;
                quadBlockView.midptr = qb.ptrTexMid[0].Address.ToUInt32().ToString("X");



                if (qb.ptrAddVis != UIntPtr.Zero)
                {
                    quadBlockView.mosaicPtr1 = qb.mosaicPtr1.ExtraBits;
                    quadBlockView.mosaicPtr2 = qb.mosaicPtr2.ExtraBits;
                    quadBlockView.mosaicPtr3 = qb.mosaicPtr3.ExtraBits;
                    quadBlockView.mosaicPtr4 = qb.mosaicPtr4.ExtraBits;
                }


                qb.view = quadBlockView;
                //Add Components
                var filter = gb.AddComponent<MeshFilter>();
                var meshRenderer = gb.AddComponent<MeshRenderer>();
                meshRenderer.material = new Material(Shader.Find("PS1"));
                bool subdivide = false;
                try
                {
                    for (int a = 0; a < qb.tex.Count; a++)
                    {
                        var pathMid = Application.dataPath + "/TextureCache/mid/" + qb.tex[a].midlods[2].Tag() + ".png";
                        var pathHigh = Application.dataPath + "/TextureCache/high/" + qb.tex[a].ptrHi.ToString() + ".png";
                        if (!qb.tex[a].hasHighDetail && File.Exists(pathMid)) //only mid exists
                        {
                            var midbytes = File.ReadAllBytes(pathMid);
                            var midTexture = new Texture2D(2, 2);
                            midTexture.LoadImage(midbytes);
                            meshRenderer.material.SetTexture("_Tex" + a,
                                midTexture != null ? midTexture : Texture2D.whiteTexture);
                            meshRenderer.material.DisableKeyword("_HIGH_DETAIL");
                            subdivide = false;
                        } if (File.Exists(pathHigh) && File.Exists(pathMid)) //high exists
                        {
                            var midbytes = File.ReadAllBytes(pathMid);
                            var midTexture = new Texture2D(2, 2);
                            midTexture.LoadImage(midbytes);
                            meshRenderer.material.SetTexture("_Tex" + (a + 4),
                                midTexture != null ? midTexture : Texture2D.whiteTexture);
                            
                            var highbytes = File.ReadAllBytes(pathHigh);
                            var fullTex = new Texture2D(2, 2);
                            fullTex.LoadImage(highbytes);
                            meshRenderer.material.SetTexture("_Tex" + a,
                                fullTex != null ? fullTex : Texture2D.whiteTexture);
                            
                            meshRenderer.material.EnableKeyword("_HIGH_DETAIL");
                            subdivide = true;
                        } else if (qb.tex[a].hasHighDetail)
                        {
                            int len = qb.tex[a].hi.Length;
                            for (int hi = 0; hi < len; hi++)
                            {
                                if (qb.tex[a].hi[hi] == null) continue;
                                Texture2D fullTex = PushTextureToHighDetail(qb, a);

                                meshRenderer.material.SetTexture("_Tex" + a,
                                    fullTex != null ? fullTex : Texture2D.whiteTexture);
                                megaTextures.Add(fullTex);
                                
                                var path = Application.dataPath + "/TextureCache/high/" + fullTex.name + ".png";
                                File.WriteAllBytes(path, fullTex.EncodeToPNG()); 
                            
                                var midTexture = midTextures[qb.tex[a].midlods[2].Tag()];
                                meshRenderer.material.SetTexture("_Tex" + (a + 4), midTexture != null ? midTexture : Texture2D.whiteTexture);
                            }
                            meshRenderer.material.EnableKeyword("_HIGH_DETAIL");
                            subdivide = true;
                        }
                        else
                        {
                            var midTexture = midTextures[qb.tex[a].midlods[2].Tag()];
                            meshRenderer.material.SetTexture("_Tex" + a,
                                midTexture != null ? midTexture : Texture2D.whiteTexture);
                            meshRenderer.material.DisableKeyword("_HIGH_DETAIL");
                        }
                    }
                }
                catch (Exception e)
                {
                    Debug.LogWarning("Broken Textures", gb);
                    Debug.LogWarning(e, gb);
                }

                var srcVisData = visdata.Find(v => v.QuadPointers.Contains(qb));
                bool unk3 = false, unk4 = false;
                if (srcVisData != null)
                {
                    unk3 = srcVisData.flag.HasFlag(VisDataFlags.Subdiv4x1);
                    unk4 = srcVisData.flag.HasFlag(VisDataFlags.Subdiv4x2);
                }
                meshRenderer.material.SetInt("_unk3", unk3 ? 1 : 0);
                meshRenderer.material.SetInt("_unk4", unk4 ? 1 : 0);
                meshRenderer.material.SetInt("_flipRotate0", (int)qb.faceFlags[0].Rotation);
                meshRenderer.material.SetInt("_flipRotate1", (int)qb.faceFlags[1].Rotation);
                meshRenderer.material.SetInt("_flipRotate2", (int)qb.faceFlags[2].Rotation);
                meshRenderer.material.SetInt("_flipRotate3", (int)qb.faceFlags[3].Rotation);
                
                if (qb.tex.Count > 0)
                {
                    if (qb.tex[0].midlods.Length > 0)
                    {
                        var blendmode = qb.tex[0].midlods[0].blendingMode;
                        meshRenderer.material.SetInt("_blendingMode",  (int)blendmode);
                        if (blendmode != BlendingMode.Standard)
                        {
                            meshRenderer.material.renderQueue += 100;
                        }
                    }
                    meshRenderer.material.SetFloat("_Scroll",  qb.tex[0].isAnimated ? 1f : 0f);
                }

                meshRenderer.material.SetInt("_invisibleTriggers", qb.quadFlags.HasFlag(QuadFlags.InvisibleTriggers) ? 1 : 0);
                if (qb.isWater)
                {
                    meshRenderer.material.SetFloat("_Water", 1.0f);
                    var vert_anim = new List<Color>();
                    foreach (var vert in vertList)
                    {
                        WaterAnim water = waterAnim.Find(water => water.ptrVertex.Address == vert.Address);
                        foreach (Color anim in water.AlphaAnimation)
                        {
                            vert_anim.Add(anim);
                        }
                    }
                    meshRenderer.material.SetColorArray("_VertexAnimation", vert_anim);
                    meshRenderer.material.renderQueue += 100;
                }
                if (qb.quadFlags.HasFlag(QuadFlags.Reflection) && qb.terrainFlag.HasFlag(TerrainFlags.Ice) )
                {
                    meshRenderer.material.SetFloat("_Transparency", 0.5f);
                    meshRenderer.material.renderQueue += 100;
                }

                var baseMesh = new Mesh
                {
                    vertices = vertices.ToArray(),
                    colors = colors.ToArray(),
                    uv = uvs.ToArray()
                };
                
                baseMesh.SetIndices(indices, MeshTopology.Triangles, 0);
                
                baseMesh.RecalculateNormals();
                baseMesh.RecalculateBounds();
                
                if (subdivide) {
                    MeshHelper.Subdivide(baseMesh);
                }
                filter.mesh = baseMesh;
                /*if (_sharedLevelGameObjects.ContainsKey(qb.SimilarityId()))
                {
                    var meshFilter = _sharedLevelGameObjects[qb.SimilarityId()].GetComponent<MeshFilter>();
                    meshFilter.mesh = CombineMeshes(meshFilter.mesh, baseMesh);
                    Destroy(gb); // We did not need this gameobject after all - clean this up
                }
                else
                {
                    _sharedLevelGameObjects.Add(qb.SimilarityId(),gb);
                }*/
                    
                Mesh colliderMesh = new Mesh
                {
                    vertices = vertices.ToArray(),
                    triangles = indices.ToArray(),
                    uv = uvs2.ToArray()
                };
                if ((qb.quadFlags & QuadFlags.KillRacer) > 0)
                {
                    killRacerMesh = CombineMeshes(killRacerMesh, colliderMesh);
                    return;
                } 
                if ((qb.quadFlags & QuadFlags.TriggerScript) > 0)
                {
                    turboPadMesh = CombineMeshes(turboPadMesh, colliderMesh);
                    return;
                }

                bool isInvisibleTrigger = (qb.quadFlags & QuadFlags.InvisibleTriggers) > 0;
                bool isTriggerScript = (qb.quadFlags & QuadFlags.TriggerScript) > 0;
                bool isWall = (qb.quadFlags & QuadFlags.Wall) > 0;
                bool isNoCollision = (qb.quadFlags & QuadFlags.NoCollision) > 0;
                if (isWall)
                {
                    wallMesh = CombineMeshes(wallMesh, colliderMesh);
                } else  if (isTriggerScript)
                {
                    turboPadMesh = CombineMeshes(turboPadMesh, colliderMesh);
                } else if ( (isInvisibleTrigger | isTriggerScript | isWall | isNoCollision) == false)
                {
                    switch (qb.terrainFlag)
                    {
                        case TerrainFlags.Asphalt:
                            asphaltMesh = CombineMeshes(asphaltMesh, colliderMesh);
                            break;
                        case TerrainFlags.Dirt:
                            dirtMesh = CombineMeshes(dirtMesh, colliderMesh);
                            break;
                        case TerrainFlags.Grass:
                            grassMesh = CombineMeshes(grassMesh, colliderMesh);
                            break;
                        case TerrainFlags.Ice:
                            iceMesh = CombineMeshes(iceMesh, colliderMesh);
                            break;
                        case TerrainFlags.Metal:
                            metalMesh = CombineMeshes(metalMesh, colliderMesh);
                            break;
                        case TerrainFlags.Mud:
                            mudMesh = CombineMeshes(mudMesh, colliderMesh);
                            break;
                        case TerrainFlags.Snow:
                            snowMesh = CombineMeshes(snowMesh, colliderMesh);
                            break;
                        case TerrainFlags.Stone:
                            stoneMesh = CombineMeshes(stoneMesh, colliderMesh);
                            break;
                        case TerrainFlags.Track:
                            trackMesh = CombineMeshes(trackMesh, colliderMesh);
                            break;
                        case TerrainFlags.Water:
                            waterMesh = CombineMeshes(waterMesh, colliderMesh);
                            break;
                        case TerrainFlags.Wood:
                            woodMesh = CombineMeshes(woodMesh, colliderMesh);
                            break;
                        case TerrainFlags.FastWater:
                            fastWaterMesh = CombineMeshes(fastWaterMesh, colliderMesh);
                            break;
                        case TerrainFlags.HardPack:
                            hardPackMesh = CombineMeshes(hardPackMesh, colliderMesh);
                            break;
                        case TerrainFlags.IcyRoad:
                            icyRoadMesh = CombineMeshes(icyRoadMesh, colliderMesh);
                            break;
                        case TerrainFlags.OceanAsphalt:
                            waterMesh = CombineMeshes(waterMesh, colliderMesh);
                            break;
                        case TerrainFlags.RiverAsphalt:
                            riverAsphaltMesh = CombineMeshes(riverAsphaltMesh, colliderMesh);
                            break;
                        case TerrainFlags.SideSlip:
                            sideSlipMesh = CombineMeshes(sideSlipMesh, colliderMesh);
                            break;
                        case TerrainFlags.SlowDirt:
                            slowDirtMesh = CombineMeshes(slowDirtMesh, colliderMesh);
                            break;
                        case TerrainFlags.SteamAsphalt:
                            steamAsphaltMesh = CombineMeshes(steamAsphaltMesh, colliderMesh);
                            break;
                    }
                }
                
            }
        }
        
        void VisStepBranchInto(VisData visData, Transform parent)
        {
            if (visData == null) return;
            GameObject rootObjToSpawn = new GameObject("vis" + visData.id);
            VisInstance iRootVis = rootObjToSpawn.AddComponent<VisInstance>();
            iRootVis.Visi = visData;
            iRootVis.SetSceneHandler(this);
            iRootVis.Set(visData);
            rootObjToSpawn.transform.parent = parent;

            if (visData.leftChild != 0)
            {
                ushort uLeftChild = (ushort) (visData.leftChild & 0x3fff);
                VisData leftChild = visdata.Find(cc => cc.id == uLeftChild);
                VisStepBranchInto(leftChild, rootObjToSpawn.transform);
            }
            if (visData.rightChild != 0)
            {
                ushort uRightChild = (ushort) (visData.rightChild & 0x3fff);
                VisData rightChild = visdata.Find(cc => cc.id == uRightChild);
                VisStepBranchInto(rightChild, rootObjToSpawn.transform);
            }

        }

        void GenerateVis()
        {
            // Draw a semitransparent blue cube at the transforms position
            Gizmos.color = new Color(1, 0, 0, 0.5f);
            int i = 0;
            VisData rootVis = visdata[0];
            VisStepBranchInto(rootVis, bspTransform);
        }
    }
}