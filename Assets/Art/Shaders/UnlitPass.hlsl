﻿#ifndef CUSTOM_UNLIT_PASS_INCLUDE
#define CUSTOM_UNLIT_PASS_INCLUDE

#include "../ShaderLibrary/Common.hlsl"
//支持SRP Batch
//CBUFFER_START(UnityPerMaterial)
//   //配置颜色
//   float4 _BaseColor;
//CBUFFER_END
//支持Instancing
struct Attributes
{
   float3 positionOS : POSITION;
   float2 baseUV : TEXCOORD0;
   UNITY_VERTEX_INPUT_INSTANCE_ID
};

//Alpha Blend Texture
//TEXTURE2D(_BaseMap);
//SAMPLER(sampler_BaseMap);

//UNITY_INSTANCING_BUFFER_START(UnityPerMaterial)
//   UNITY_DEFINE_INSTANCED_PROP(float4,_BaseMap_ST)
//   UNITY_DEFINE_INSTANCED_PROP(float4,_BaseColor)
//   UNITY_DEFINE_INSTANCED_PROP(float,_Cutoff)
//   UNITY_DEFINE_INSTANCED_PROP(float,_Metallic)
//   UNITY_DEFINE_INSTANCED_PROP(float,_Smoothness)
//UNITY_INSTANCING_BUFFER_END(UnityPerMaterial)
//TSen  VAR_BASE_UV ???
//输出结构
struct Varyings
{
   float4 positionCS : SV_POSITION;
   float2 baseUV : VAR_BASE_UV;
   UNITY_VERTEX_INPUT_INSTANCE_ID
};
//顶点着色器
Varyings UnlitPassVertex(Attributes input) 
{
    Varyings output;
    UNITY_SETUP_INSTANCE_ID(input);
    UNITY_TRANSFER_INSTANCE_ID(input,output);
    float3 positionWS=TransformObjectToWorld(input.positionOS.xyz);
    output.positionCS=TransformWorldToHClip(positionWS);
    //顶点 访问 _BaseMap_ST
    float4 baseST=UNITY_ACCESS_INSTANCED_PROP(UnityPerMaterial,_BaseMap_ST);
    output.baseUV=input.baseUV*baseST.xy+baseST.zw;
    return output;
}
//片元着色器
float4 UnlitPassFragment(Varyings input) :SV_TARGET
{
   UNITY_SETUP_INSTANCE_ID(input);
   float4 baseMap=SAMPLE_TEXTURE2D(_BaseMap,sampler_BaseMap,input.baseUV);
   float4 baseColor= UNITY_ACCESS_INSTANCED_PROP(UnityPerMaterial,_BaseColor);
   float4 base =baseMap*baseColor;
   #if defined(_CLIPPING)
      clip(base.a-UNITY_ACCESS_INSTANCED_PROP(UnityPerMaterial,_Cutoff));
   #endif
   return float4(base.rgb,GetFinalAlpha(base.a)) ;
} 
#endif