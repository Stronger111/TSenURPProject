﻿#ifndef CUSTOM_SHADOW_CASTER_PASS_INCLUDE
#define CUSTOM_SHADOW_CASTER_PASS_INCLUDE

//#include "../ShaderLibrary/Common.hlsl"

//Alpha Blend Texture
//TEXTURE2D(_BaseMap);
//SAMPLER(sampler_BaseMap);

//UNITY_INSTANCING_BUFFER_START(UnityPerMaterial)
//   UNITY_DEFINE_INSTANCED_PROP(float4,_BaseMap_ST)
//   UNITY_DEFINE_INSTANCED_PROP(float4,_BaseColor)
//   UNITY_DEFINE_INSTANCED_PROP(float,_Cutoff)
//UNITY_INSTANCING_BUFFER_END(UnityPerMaterial)

struct Attributes
{
   float3 positionOS : POSITION;
   float2 baseUV : TEXCOORD0;
   UNITY_VERTEX_INPUT_INSTANCE_ID
};

//TSen  VAR_BASE_UV ???
//输出结构
struct Varyings
{
   float4 positionCS_SS : SV_POSITION;
   float2 baseUV : VAR_BASE_UV;
   UNITY_VERTEX_INPUT_INSTANCE_ID
};

bool _ShadowPancaking;
//顶点着色器
Varyings ShadowCasterPassVertex(Attributes input) 
{
    Varyings output;
    UNITY_SETUP_INSTANCE_ID(input);
    UNITY_TRANSFER_INSTANCE_ID(input,output);
    float3 positionWS=TransformObjectToWorld(input.positionOS.xyz);
    output.positionCS_SS=TransformWorldToHClip(positionWS);
    if(_ShadowPancaking)
    {
       #if UNITY_REVERSED_Z
          output.positionCS_SS.z=min(output.positionCS_SS.z,output.positionCS_SS.w*UNITY_NEAR_CLIP_VALUE);
       #else
          output.positionCS_SS.z=max(output.positionCS_SS.z,output.positionCS_SS.w*UNITY_NEAR_CLIP_VALUE);
       #endif
    }
    //顶点 访问 _BaseMap_ST
    //float4 baseST=UNITY_ACCESS_INSTANCED_PROP(UnityPerMaterial,_BaseMap_ST);
    output.baseUV=TransformBaseUV(input.baseUV);
    return output;
}
//片元着色器
void ShadowCasterPassFragment(Varyings input)
{
   UNITY_SETUP_INSTANCE_ID(input);
   //float4 baseMap=SAMPLE_TEXTURE2D(_BaseMap,sampler_BaseMap,input.baseUV);
   //float4 baseColor= UNITY_ACCESS_INSTANCED_PROP(UnityPerMaterial,_BaseColor);
   InputConfig config = GetInputConfig(input.positionCS_SS,input.baseUV);
   ClipLOD(config.fragment, unity_LODFade.x);
   float4 base =GetBase(config);
   #if defined(_SHADOWS_CLIP)
      clip(base.a-GetCutoff(config));
   #elif defined(_SHADOWS_DITHER)
      float dither=InterleavedGradientNoise(input.positionCS_SS.xy,0);
      clip(base.a-dither);
   #endif
} 
#endif