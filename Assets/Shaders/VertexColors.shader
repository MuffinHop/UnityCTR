Shader "Vertex Colors" {
    Properties
    {
        
    }
SubShader {
 
        Blend SrcAlpha OneMinusSrcAlpha
Cull Off
    Pass {
        CGPROGRAM     
        #include "Lighting.cginc"
        #pragma vertex vert
        #pragma fragment frag
       #pragma multi_compile WATER
        #include "UnityCG.cginc"
        #include "AutoLight.cginc"
        struct appdata {
            float4 vertex : POSITION;
            fixed4 color : COLOR;
            float2 uv : TEXCOORD0;
            float4 normal : NORMAL;
            uint vertexId : SV_VertexID;
        };

        struct v2f {
            float4 pos : SV_POSITION;
            fixed4 color : COLOR;
            uint vID : TEXCOORD1;
        }; 
        
        v2f vert (appdata v) {
            v2f o;
            o.pos = UnityObjectToClipPos(v.vertex );
            o.color = v.color;
            o.vID = v.vertexId;
            return o;
        }
        fixed4 frag (v2f i) : SV_Target { 
            return fixed4(i.color.rgb,1.0);
        }
        ENDCG
    }
}
}