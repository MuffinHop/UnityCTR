Shader "Font Upscale" {
   Properties {
      _MainTex ("Texture Image", 2D) = "white" {} 
      _MainTex2 ("Texture Image 2", 2D) = "white" {} 
      _ScaleBlur ("Scale Blur", Range(0,20)) = 0.0
      _Amplifier ("Amplifier", Range(0,3)) = 0.0
      _Outline ("Outline", Range(0,80)) = 0.0
      _Switch ("Switch", Range(0,1)) = 0.0
      [Toggle(TWOCOLOR)] TWOCOLOR ("TWOCOLOR", Float) = 0.0
   }
   SubShader {
      Pass {	
         CGPROGRAM
 
         #pragma vertex vert  
         #pragma fragment frag 
         #pragma multi_compile __ TWOCOLOR
         uniform sampler2D _MainTex;	
         uniform sampler2D _MainTex2;	
         uniform float4 _MainTex_TexelSize;
         uniform float _ScaleBlur;
         uniform float _Amplifier;
         uniform float _Outline;
         uniform float _Switch;
         struct vertexInput {
            float4 vertex : POSITION;
            float4 texcoord : TEXCOORD0;
         };
         struct vertexOutput {
            float4 pos : SV_POSITION;
            float4 tex : TEXCOORD0;
         };
 
         vertexOutput vert(vertexInput input) 
         {
            vertexOutput output;
 
            output.tex = input.texcoord;
            output.pos = UnityObjectToClipPos(input.vertex);
            return output;
         }
         float rand(half2 co) {
             return frac(sin(dot(co.xy ,half2(12.9898,78.233))) * 43758.5453);
         }
         half4 boxBlur(sampler2D tex, half2 uv, half delta){
            half4 color=half4(0.0,0.0,0.0,0.0);
	         float total=0.0;
	         for(float t=-60.0;t<=60.0;t++){
	            color += tex2D( tex, uv + half2( rand(uv+half2(t,t))-0.5, rand(uv+half2(1.0+t,1.0+t))-0.5) * delta);
	            total++;
	         }
             return color / total;
         }
         float4 frag(vertexOutput input) : COLOR
         {
            float val = boxBlur(_MainTex,input.tex.xy, _ScaleBlur * 0.001).r;
            half4 color = fixed4(0.0,0.0,0.0,1.0);
            float val2 = val;
            val = saturate(pow(val * _Amplifier, 1.0));
            #if TWOCOLOR
                  color.rgb = half3(val, val, val);
            #else
               val = saturate( pow(1.4 - (1.05 - val) * 1.05,2.0));
               float limit = saturate(pow(val * 6.8,2.0) - 0.4);
               half3 gradient = lerp(half3(1.0,0.5,0.0),half3(1.0,0.1,0.0), -0.5 + (input.tex.y*10.0 + 0.5) % 1.0);
               color.rgb = lerp( color.rgb, gradient, limit);
               //color.rgb = lerp( half3(0.0,0.0,0.0), color.rgb, 1.0 - val);
               float calc = saturate(saturate(1.0 - pow( _Outline * val2, 1))-0.9)*10.0;
               color.rgb = lerp(color.rgb,half3(0.3,0.5,0.5), calc);
            #endif
            
            return lerp(color,tex2D(_MainTex2,input.tex.xy),_Switch);
         }
 
         ENDCG
      }
   }
   Fallback "Unlit/Texture"
}