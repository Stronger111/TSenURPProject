#ifndef CUSTOM_UNLIT_PASS_INCLUDE
#define CUSTOM_UNLIT_PASS_INCLUDE

#include "../ShaderLibrary/Common.hlsl"
//支持SRP Batch
CBUFFER_START(UnityPerMaterial)
   //配置颜色
   float4 _BaseColor;
CBUFFER_END

//顶点着色器
float4 UnlitPassVertex(float3 positionOS:POSITION) : SV_POSITION
{
    float3 positionWS=TransformObjectToWorld(positionOS.xyz);
    return TransformWorldToHClip(positionWS);
}
//片元着色器
float4 UnlitPassFragment() :SV_TARGET
{
   return _BaseColor;
} 
#endif