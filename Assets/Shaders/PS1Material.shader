Shader "PS1" {
    Properties
    {
        _Tex0 ("Texture", 2D) = "white" {}
        _Tex1 ("Texture", 2D) = "white" {}
        _Tex2 ("Texture", 2D) = "white" {}
        _Tex3 ("Texture", 2D) = "white" {}
        _Tex4 ("Texture", 2D) = "white" {}
        _Tex5 ("Texture", 2D) = "white" {}
        _Tex6 ("Texture", 2D) = "white" {}
        _Tex7 ("Texture", 2D) = "white" {}
        /*_Tex8 ("Texture", 2D) = "white" {}
        _Tex9 ("Texture", 2D) = "white" {}
        _Tex10 ("Texture", 2D) = "white" {}
        _Tex11 ("Texture", 2D) = "white" {}
        _Tex12 ("Texture", 2D) = "white" {}
        _Tex13 ("Texture", 2D) = "white" {}
        _Tex14 ("Texture", 2D) = "white" {}
        _Tex15 ("Texture", 2D) = "white" {}*/
        _flipRotate0 ("FlipRotate0", Int) = 0 
        _flipRotate1 ("FlipRotate1", Int) = 0 
        _flipRotate2 ("FlipRotate2", Int) = 0 
        _flipRotate3 ("FlipRotate3", Int) = 0 
        _invisibleTriggers ("InvisibleTriggers", Int) = 0 
        _Water ("Water", Float) = 0 
        _Transparency ("Transparency", Float) = 0 
        _Scroll ("Scroll", Float) = 0 
        _unk3 ("unk3", Int) = 0 
        _unk4 ("unk4", Int) = 0 
        _blendingMode ("Blending Mode", Int) = 0 
		[Toggle(_HIGH_DETAIL)] HighDetail("HIGH_DETAIL", Float) = 0
        
    }
SubShader {
        Tags{ "Queue" = "AlphaTest" "RenderType" = "TransparentCutout" "IgnoreProjector" = "True" }
 
        Blend SrcAlpha OneMinusSrcAlpha
Cull Off
    Pass {
        CGPROGRAM     
        #include "Lighting.cginc"
        #pragma vertex vert
        #pragma fragment frag
        #pragma shader_feature _HIGH_DETAIL
        #include "UnityCG.cginc"
        #include "AutoLight.cginc"
        #include "Lighting.cginc"
        sampler2D  _Tex0, _Tex1, _Tex2, _Tex3, _Tex4, _Tex5, _Tex6, _Tex7;
        int _flipRotate0, _flipRotate1, _flipRotate2, _flipRotate3;
        float4 _Tex0_ST,_Tex1_ST,_Tex2_ST,_Tex3_ST;
        float3 _PointLightPos, _PointLightColor;
        float _PointLightSize;
        int _invisibleTriggers;
        float _Water, _Transparency, _Scroll;
        float4 _VertexAnimation[28*24];
        int _blendingMode;
        
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
            float2 uv : TEXCOORD0;
            uint vID : TEXCOORD1;
            float dist : TEXCOORD2;
        }; 
        
        v2f vert (appdata v) {
            float3 worldPos = mul (unity_ObjectToWorld, v.vertex).xyz;
            v2f o;
            o.pos = UnityObjectToClipPos(v.vertex );
            if(_Scroll>0.0)
            {
                o.pos.z += 0.001;
            }
            o.color = v.color;
            o.uv = TRANSFORM_TEX(v.uv, _Tex0);
            o.vID = v.vertexId;
            float3 lightDir = _WorldSpaceLightPos0;
            float NL = saturate(dot(v.normal, lightDir));
            float atten = LIGHT_ATTENUATION(i);
            float pointToPosDist = 10.0*lerp(0.0,1.0,saturate(1.0 - distance(_PointLightPos.xyz,worldPos) / _PointLightSize)); //100.0 * _PointLightSize / ( 1.0 + pow(distance(_PointLightPos.xyz,worldPos),2.0));
            //o.color *= lerp( half4(1.0,1.0,1.0,1.0), lerp(half4(0.5,0.7,1.0,0.5),half4(0.92,0.97,1.0,0.4),plasma), _Water);
            if(_Water>0.0)
            {
                uint frame0 = uint(_Time.z) % 28;
                uint frame1 = uint(_Time.z + 1) % 28;
                o.color *= lerp( half4(1.0,1.0,1.0,1.0), lerp(_VertexAnimation[(v.vertexId*28) + frame0], _VertexAnimation[(v.vertexId*28) + frame1], _Time.z % 1.0), _Water);
            }
            o.dist = distance(_WorldSpaceCameraPos, mul(unity_ObjectToWorld, v.vertex));
            //o.color *= max(NL*2.0,0.1) * (_LightColor0 * atten + float4(pointToPosDist * _PointLightColor.r,pointToPosDist * _PointLightColor.g,pointToPosDist * _PointLightColor.b,1.0));
            return o;
        }
        #define Translucent 0
        #define Additive 1
        #define Subtractive 2
        #define Standard 3
        #define None 0
        #define Rotate90 1
        #define Rotate180 2
        #define Rotate270 3
        #define FlipRotate270 4
        #define FlipRotate180 5
        #define FlipRotate90 6
        #define Flip 7
        float flipper;
        float2 flagrot(float2 uv, int flag) {
            uv.x = uv.x % 1.0;
            uv.y = uv.y % 1.0;
            float x = uv.x;
            float y = uv.y;
            switch(flag) {
                case None:
                    //uv = rotuv(uv, 90.0);
                    uv.x = x;
                    uv.y = -y;
                    break;
                case Rotate90:
                    //uv = rotuv(uv, 90.0);
                    uv.x = y;
                    uv.y = x;
                    break;
                case Rotate180:
                    uv.x = -x;
                    uv.y = y;
                    break; 
                case Rotate270:
                    uv.x = -y;
                    uv.y = -x;
                    break;
                case FlipRotate90:
                    uv.x = y;
                    uv.y = -x;
                    flipper = 1;
                    break;
                case FlipRotate180: 
                    uv.x = -x;
                    uv.y = -y;
                    flipper = 1;
                    break;
                case FlipRotate270:
                    uv.x = -y;
                    uv.y = x;
                    flipper = 1;
                    break;
                case Flip:
                    flipper = 1;
                    break;
            }   
            if(flipper==1.0){
                uv.y *= -1.0;
                uv.x *= -1.0;
            }
            return uv;
        }
        float2 fixedRotation(half2 uv) {
            if (uv.x < 0.5) {
                if (uv.y < 0.5) {
                    uv = float2(uv.y, uv.x);
                } else {
                    uv = float2(uv.y + 0.5, uv.x + 0.5);
                }
            } else {
                if (uv.y < 0.5) {
                    uv = float2(uv.y + 0.5, uv.x);
                } else {
                    uv = float2(uv.y, uv.x + 0.5);
                }
            }
            return uv;
        }
        float when_eq(float x, float y) {
          return 1.0 - abs(sign(x - y));
        }

        float when_neq(float x, float y) {
          return abs(sign(x - y));
        }

        float when_gt(float x, float y) {
          return max(sign(x - y), 0.0);
        }

        float when_lt(float x, float y) {
          return max(sign(y - x), 0.0);
        }

        float when_ge(float x, float y) {
          return 1.0 - when_lt(x, y);
        }

        float when_le(float x, float y) {
          return 1.0 - when_gt(x, y);
        }
        #define MIDRES_DIST 60.0
        float4 sampleTex0(float2 uv, float dist)
        {
            uv = flagrot( uv, _flipRotate0);
            uv.y += _Scroll * _Time.y;
            uv.y *= -1.0;
            #ifdef _HIGH_DETAIL
                if(dist<MIDRES_DIST)
                {
                    return tex2D(_Tex0, uv);
                } else
                {
                    return tex2D(_Tex4, uv);
                }
            #else
                return tex2D(_Tex0, uv);
            #endif
        }
        float4 sampleTex1(float2 uv, float dist)
        {
            uv = flagrot(uv, _flipRotate1);
            uv.y += _Scroll * _Time.y;
            uv.y *= -1.0;
            #ifdef _HIGH_DETAIL
                if(dist<MIDRES_DIST)
                {
                    return tex2D(_Tex1, uv);
                } else
                {
                    return tex2D(_Tex5, uv);
                }
            #else
                return tex2D(_Tex1, uv);
            #endif
        }
        float4 sampleTex2(float2 uv, float dist)
        {
            uv = flagrot( uv, _flipRotate2);
            uv.y += _Scroll * _Time.y;
            uv.y *= -1.0;
            #ifdef _HIGH_DETAIL
                if(dist<MIDRES_DIST)
                {
                    return tex2D(_Tex2, uv);
                } else
                {
                    return tex2D(_Tex6, uv);
                }
            #else
                return tex2D(_Tex2, uv);
            #endif
        }
        float4 sampleTex3(float2 uv, float dist)
        {
            uv = flagrot( uv, _flipRotate3);
            uv.y += _Scroll * _Time.y;
            uv.y *= -1.0;
            #ifdef _HIGH_DETAIL
                if(dist<MIDRES_DIST)
                {
                    return tex2D(_Tex3, uv);
                } else
                {
                    return tex2D(_Tex7, uv);
                }
            #else
                return tex2D(_Tex3, uv);
            #endif
        }
        fixed4 frag (v2f i, fixed facing : VFACE) : SV_Target { 
            //return fixed4(i.uv.x, i.uv.y, 0.0, 1.0);
            
            half4 col = half4(0.0,0.0,0.0,0.0);
                if(i.uv.y>1.0) {
                    if(i.uv.x<1.0) {
                        col += i.color * sampleTex2( i.uv, i.dist);
                    } else {
                        col += i.color * sampleTex3( i.uv, i.dist); 
                    }
                } else {
                    if(i.uv.x<1.0) {
                        col = i.color * sampleTex0( i.uv, i.dist); 
                    } else {
                        col = i.color * sampleTex1( i.uv, i.dist);
                    }
                }
            col.a = 1.0 - col.a;
            
            if(_invisibleTriggers==1.0 ){
                discard;
                col.a = 0.5;
                if( ( ((i.uv.x*12.0)%1.0)<0.5) ){
                    discard;
                }
            }
            if(col.a > 0.99) {
                discard;
            }
            col.a = 1.0 - col.a - _Transparency;
            if(_blendingMode == Additive)
            {
                col.a = 0.5;
            }
            return col;
        }
        ENDCG
    }
}
}