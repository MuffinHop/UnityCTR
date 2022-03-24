Shader "Flag" {
   Properties {
   }
   SubShader {
      Pass {	
         CGPROGRAM
 
         #pragma vertex vert  
         #pragma fragment frag 
         struct vertexInput {
            float4 vertex : POSITION;
            float4 texcoord : TEXCOORD0;
         };
         struct vertexOutput {
            float4 pos : SV_POSITION;
            float4 vert : TEXCOORD0;
         };
 
         vertexOutput vert(vertexInput input) 
         {
            vertexOutput output;
            float4 vert = input.vertex;
            output.vert = vert;
            vert.y += sin(vert.x - _Time.y * 5.0);
            output.pos = UnityObjectToClipPos(vert);
            return output;
         }
         float4 frag(vertexOutput input) : COLOR
         {
            float x = floor(2.0*input.vert.x/3.0 + 100.0);
            float y = floor(2.0*input.vert.z/2.0 + 100.0);
            float chessboard = (1.0 + x + (y % 2.0) % 2.0);
            float str = 1.0 + sin(input.vert.x - _Time.y * 5.0)/3.0;
            return lerp(half4(0.5*str,0.5*str,0.5*str,1.0), half4(1.0*str,1.0*str,1.0*str,1.0), floor(chessboard%2.0));
         }
 
         ENDCG
      }
   }
   Fallback "Unlit/Texture"
}