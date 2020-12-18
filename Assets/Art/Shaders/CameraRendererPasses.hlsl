#ifndef CUSTOM_CAMERA_RENDER_PASSES_INCLUDE
#define CUSTOM_CAMERA_RENDER_PASSES_INCLUDE

//#include "../ShaderLibrary/Common.hlsl"
TEXTURE2D(_SourceTexture);

struct Varyings
{
   //裁剪空间位置
   float4 positionCS : SV_POSITION;
   //屏幕空间UV
   float2 screenUV : VAR_SCREEN_UV;
};

Varyings DefaultPassVertex(uint vertexID : SV_VertexID)
{
   Varyings output;
   output.positionCS=float4
   (
       vertexID <=1 ? -1.0 :3.0,
       vertexID == 1 ? 3.0 :-1.0,
       0.0,1.0
   );
   output.screenUV=float2
   (
      vertexID <=1 ? 0.0 : 2.0,
      vertexID ==1 ? 2.0 : 0.0
   );

   if(_ProjectionParams.x<0.0)
   {
      output.screenUV.y=1.0-output.screenUV.y;
   }
   return output;
}

float4 CopyPassFragment(Varyings input) : SV_TARGET
{
    return SAMPLE_TEXTURE2D(_SourceTexture,sampler_linear_clamp,input.screenUV);
}
 float CopyDepthPassFragment(Varyings input) :SV_DEPTH
 {
    return SAMPLE_DEPTH_TEXTURE(_SourceTexture,sampler_point_clamp,input.screenUV);
 }
#endif