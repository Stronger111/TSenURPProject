#ifndef CUSTOM_LIT_INPUT_INCLUDED
#define CUSTOM_LIT_INPUT_INCLUDED

//Alpha Blend Texture
TEXTURE2D(_BaseMap);
//自发光
TEXTURE2D(_EmissionMap);
SAMPLER(sampler_BaseMap);
//Per Material Property
UNITY_INSTANCING_BUFFER_START(UnityPerMaterial)
   UNITY_DEFINE_INSTANCED_PROP(float4,_BaseMap_ST)
   UNITY_DEFINE_INSTANCED_PROP(float4,_BaseColor)
   UNITY_DEFINE_INSTANCED_PROP(float,_Cutoff)
   UNITY_DEFINE_INSTANCED_PROP(float,_Metallic)
   UNITY_DEFINE_INSTANCED_PROP(float,_Smoothness)
   UNITY_DEFINE_INSTANCED_PROP(float4,_EmissionColor) //float4 
UNITY_INSTANCING_BUFFER_END(UnityPerMaterial)

float2 TransformBaseUV(float2 baseUV)
{
   float4 baseST=UNITY_ACCESS_INSTANCED_PROP(UnityPerMaterial,_BaseMap_ST);
   return baseUV*baseST.xy+baseST.zw;
}
//获取Albedo
float4 GetBase(float2 baseUV)
{
   float4 map=SAMPLE_TEXTURE2D(_BaseMap,sampler_BaseMap,baseUV);
   float4 color=UNITY_ACCESS_INSTANCED_PROP(UnityPerMaterial,_BaseColor);
   return map*color;
}
//获取Cutoff
float GetCutoff(float2 baseUV)
{
    return UNITY_ACCESS_INSTANCED_PROP(UnityPerMaterial,_Cutoff);
}
//金属度
float GetMetallic(float2 baseUV)
{
   return UNITY_ACCESS_INSTANCED_PROP(UnityPerMaterial,_Metallic);
}
//Instance Buffer 数据
float GetSmoothness(float2 baseUV)
{
   return UNITY_ACCESS_INSTANCED_PROP(UnityPerMaterial,_Smoothness);
}
//自发光
float3 GetEmission(float2 baseUV)
{
    float4 map=SAMPLE_TEXTURE2D(_EmissionMap,sampler_BaseMap,baseUV);
	float4 color=UNITY_ACCESS_INSTANCED_PROP(UnityPerMaterial,_EmissionColor);
	return map.rgb*color.rgb;
}
#endif