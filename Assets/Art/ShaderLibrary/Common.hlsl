#ifndef CUSTOM_COMMON_INCLUDED
#define CUSTOM_COMMON_INCLUDED
#include "Packages/com.unity.render-pipelines.core/ShaderLibrary/Common.hlsl"
#include "Packages/com.unity.render-pipelines.core/ShaderLibrary/CommonMaterial.hlsl"
#include "UnityInput.hlsl"
//物体空间转换到世界空间中
//float3 TransformObjectToWorld(float3 positionOS)
//{
//    return mul(unity_ObjectToWorld,float4(positionOS,1.0)).xyz;
//}

//float4 TransformWorldToHClip(float3 positionWS)
//{
//   return mul(unity_MatrixVP,float4(positionWS,1.0));
//}
//物体到世界
#define UNITY_MATRIX_M unity_ObjectToWorld 
//世界到物体
#define UNITY_MATRIX_I_M unity_WorldToObject
#define UNITY_MATRIX_V unity_MatrixV
#define UNITY_MATRIX_VP unity_MatrixVP
#define UNITY_MATRIX_P glstate_matrix_projection

//Shadow Mask bake To LightProbe 不打断合批
#if defined(_SHADOW_MASK_ALWAYS) || defined(_SHADOW_MASK_DISTANCE)
   #defined SHADOWS_SHADOWMASK
#endif

#include "Packages/com.unity.render-pipelines.core/ShaderLibrary/UnityInstancing.hlsl"
#include "Packages/com.unity.render-pipelines.core/ShaderLibrary/SpaceTransforms.hlsl"

float Square(float3 v)
{
  return v*v;
}

float DistanceSquared(float3 pA,float3 pB)
{
    return dot(pA-pB,pA-pB);
}
//LOD Cross Fade
void ClipLOD(float2 positionCS,float fade)
{
    #if defined (LOD_FADE_CROSSFADE)
       float dither=InterleavedGradientNoise(positionCS.xy,0); //Test InterleavedGradientNoise 噪波函数
       clip(fade+(fade <0.0 ? dither : -dither));
    #endif
}
#endif