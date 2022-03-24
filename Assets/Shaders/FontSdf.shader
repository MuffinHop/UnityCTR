Shader "Font SDF" {
   Properties {
      _MainTex ("Texture Image", 2D) = "white" {} 
      _MainTex2 ("Texture Image 2", 2D) = "white" {} 
   }
   SubShader {
      Pass {	
         CGPROGRAM
 
         #pragma vertex vert  
         #pragma fragment frag 
 
         uniform sampler2D _MainTex;
         uniform sampler2D _MainTex2;
         uniform float4 _MainTexelSize;
 
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
         float when_eq(float x, float y) {
           return 1.0 - abs(sign(x - y));
         }

         float when_neq(float x, float y) {
           return abs(sign(x - y));
         }

         float when_gt(float x, float y) {
           return max(sign(x - y), 0.0);
         }
         float when_gtSoft(float x, float y) {
           return saturate(x - y);
         }

         float when_lt(float x, float y) {
           return max(sign(y - x), 0.0);
         }
         float when_ltSoft(float x, float y) {
           return saturate(y - x);
         }


         float when_ge(float x, float y) {
           return 1.0 - when_lt(x, y);
         }

         float when_le(float x, float y) {
           return 1.0 - when_gt(x, y);
         }

         float rand(half2 co) {
             return frac(sin(dot(co.xy ,half2(12.9898,78.233))) * 43758.5453);
         }
         half4 boxBlur(sampler2D tex, half2 uv, half delta){
            half4 color=half4(0.0,0.0,0.0,0.0);
	         float total=0.0;
	         for(float t=-4.0;t<=4.0;t++){
	            color += tex2D( tex, uv + half2( rand(uv+half2(t,t))-0.5, rand(uv+half2(1.0+t,1.0+t))-0.5) * delta);
	            total++;
	         }
             return color / total;
         }
         
         float4 frag(vertexOutput input) : COLOR
         {
            float val = 1.0;
            half3 baseColor = tex2D(_MainTex, input.tex.xy).rgb;
            half2 offset = half2(2.0/1280.0,2.0/640.0); 
            half3 baseColor2 =
                max(max(max(max(   tex2D(_MainTex, input.tex.xy).rgb,
                    tex2D(_MainTex, input.tex.xy + offset * half2(1,1)).rgb), 
                    tex2D(_MainTex, input.tex.xy + offset * half2(-1,1)).rgb), 
                    tex2D(_MainTex, input.tex.xy + offset * half2(1,-1)).rgb),
                    tex2D(_MainTex, input.tex.xy + offset * half2(-1,-1)).rgb) ;
            float cond3 = 1.0 - when_gt(baseColor.r, 0.9) * when_lt(baseColor.g, 0.1) * when_gt(baseColor.b, 0.9); // is it magenta, then skip.
            float cond2 = when_gtSoft(baseColor2.r, 0.5) * when_ltSoft(baseColor2.g, 0.5) * when_gtSoft(baseColor2.b, 0.5); // is it magenta, then skip.
            for(float y = -0.02; y < 0.02; y += 1.0/1280.0)
            {
               for(float x = -0.02; x < 0.02; x += 1.0/640.0)
               {
                  //if(tex2D(_MainTex, half2(x,y)).rgb < half3(0.9,0.0,0.9) || tex2D(_MainTex, half2(x,y)).rgb < half3(0.1,0.1,0.1) )
                  half2 sampleUV = half2(input.tex.x+x,input.tex.y+y);
                  half3 colorSample = tex2D(_MainTex, sampleUV);
                  float cond = when_lt(colorSample.r, 0.2) * when_lt(colorSample.g, 0.2) * when_lt(colorSample.b, 0.2);
                  cond *= cond3;
                  val = lerp(val, min( val, distance(input.tex.xy, sampleUV)), cond);
               }
            }
            val *= 64;
            val *= 1.0-saturate(cond2 * 100.0); 
            return fixed4(val,0.0,0.0,1.0);
         }
 
         ENDCG
      }
   }
   Fallback "Unlit/Texture"
}