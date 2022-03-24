Shader "Font Downscale" {
    Properties {
        _MainTex ("Source", 2D) = "white" {}
    }
    SubShader {
        Pass {
 
            CGPROGRAM
            #include "UnityCG.cginc"
            #pragma vertex vert_img
            #pragma fragment frag
 
            sampler2D _MainTex;
 
            half4 frag (v2f_img i) : SV_Target // not COLOR
            {
                return tex2D(_MainTex, i.uv);
            }
        ENDCG
        }
    }
}