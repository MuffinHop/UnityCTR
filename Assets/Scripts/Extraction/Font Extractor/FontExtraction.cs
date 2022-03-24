using System.Collections.Generic;
using System.IO;
using CTRFramework;
using CTRFramework.Shared;
using CTRFramework.Vram;
using UnityEngine;

public class FontExtraction : MonoBehaviour
{
    [SerializeField] private List<Texture2D> _textures;
    void Start()
    {
        _textures = new List<Texture2D>();
        string pathVrm = Application.dataPath + "/Output/packs/shared.vrm";
        BinaryReaderEx vrmBr = new BinaryReaderEx(File.OpenRead(pathVrm));
        CtrVrm vrm = new CtrVrm(vrmBr);
        int index = 0;
        foreach (var tim in vrm.Tims)
        {
            Texture2D tex2D = new Texture2D(tim.region.Width, tim.region.Height, TextureFormat.ARGB32, false);
            for (int y = 0; y < tim.region.Height; y++)
            {
                for (int x = 0; x < tim.region.Width; x++)
                {
                    var pixelValue = tim.data[x + tim.region.Width * y];
                        
                    float pixelRed = pixelValue & 31;
                    pixelValue = (ushort) (pixelValue >> 5);
                    float pixelGreen = pixelValue & 31;
                    pixelValue = (ushort) (pixelValue >> 5);
                    float pixelBlue = pixelValue & 31;
                    pixelValue = (ushort) (pixelValue >> 5);
                    int pixelAlpha = pixelValue & 1;
                    tex2D.SetPixel(x,y,new Color(pixelRed/31f,pixelGreen/31f,pixelBlue/31f, 1.0f));
                    
                }
            }
            tex2D.Apply();
            tex2D.filterMode = FilterMode.Point;
            
            byte[] bytes = tex2D.EncodeToPNG();
            var dirPath = Application.dataPath + "/../SaveImages/";
            if(!Directory.Exists(dirPath)) {
                Directory.CreateDirectory(dirPath);
            }
            File.WriteAllBytes(dirPath + "Image" + index + ".png", bytes);
            _textures.Add(tex2D);
            index++;
        }
    }
}