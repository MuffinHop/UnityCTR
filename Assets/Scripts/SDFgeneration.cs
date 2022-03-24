using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Experimental.Rendering;

public class SDFgeneration : MonoBehaviour
{
    [SerializeField] private Texture2D _font;
    [SerializeField] private RenderTexture _out;
    [SerializeField] private RenderTexture _final;
    [SerializeField] private RenderTexture _2x;
    [SerializeField] private Texture2D _upscaled;
    [SerializeField] private Texture2D _pointSample;
    [SerializeField] private Material _sdfMaterial;
    [SerializeField] private Material _upscaleMaterial;
    [SerializeField] private Material _downscaleMaterial;

    private int Distance(float startX, float startY)
    {
        float closest = float.MaxValue;
        for (int y = 0; y < _out.height; y++)
        {
            for (int x = 0; x < _out.width; x++)
            {
                Color pixel = _upscaled.GetPixel(x, y);
                if (pixel == Color.magenta || pixel == Color.black )
                {
                    closest = Mathf.Min(closest, Vector2.Distance(new Vector2(startX, startY),new Vector2(x,y)));
                }
            }
        }
        return (int)(closest*4f);
    }
    Texture2D ToTexture2D(RenderTexture rTex)
    {
        Texture2D tex = new Texture2D(rTex.width, rTex.height, TextureFormat.RGB24, false);
        RenderTexture.active = rTex;
        tex.ReadPixels(new Rect(0, 0, rTex.width, rTex.height), 0, 0);
        tex.Apply();
        RenderTexture.active = null;
        return tex;
    }
    void Start()
    {
        _out = new RenderTexture(_font.width * 4, _font.height * 4,24, GraphicsFormat.R32G32_SFloat); 
        _final = new RenderTexture(_font.width * 4, _font.height * 8,24);
        _2x = new RenderTexture(_font.width * 2, _font.height * 4,24);
        _pointSample = new Texture2D(_font.width,_font.height);
        _pointSample.filterMode = FilterMode.Point;
        for (int y = 0; y < _font.height; y++) {
            for (int x = 0; x < _font.width; x++)
            {
                Color color = _font.GetPixel(x, y);
                _pointSample.SetPixel(x, y, color);
            }
        }
        _pointSample.Apply();
        _sdfMaterial.SetTexture("_MainTex2", _pointSample);
        Graphics.Blit(_font, _out, _sdfMaterial);
        Graphics.Blit(_out, _final, _upscaleMaterial);
        Texture2D tex = ToTexture2D(_final);
            
        byte[] bytes = tex.EncodeToPNG();
        System.IO.File.WriteAllBytes(Application.dataPath+"\\UpscaledFont.png", bytes);
        Graphics.Blit(_final, _2x, _downscaleMaterial);
        Texture2D timesTwo = ToTexture2D(_2x);
        byte[] bytes2 = timesTwo.EncodeToPNG();
        System.IO.File.WriteAllBytes(Application.dataPath+"\\2X.png", bytes2);
        
    }

    private void Update()
    {
        /*if ((Time.frameCount % 600) == 0)
        {
            Graphics.Blit(_font, _out, _sdfMaterial);
        }*/
        _upscaleMaterial.mainTexture = _out;
        _upscaleMaterial.SetTexture("_MainTex2", _font);
    }
}
