#ifndef CUSTOM_LIT_INPUT_INCLUDED
#define CUSTOM_LIT_INPUT_INCLUDED

//Alpha Blend Texture
TEXTURE2D(_BaseMap);
SAMPLER(sampler_BaseMap);
//Per Material Property
UNITY_INSTANCING_BUFFER_START(UnityPerMaterial)
   UNITY_DEFINE_INSTANCED_PROP(float4,_BaseMap_ST)
   UNITY_DEFINE_INSTANCED_PROP(float4,_BaseColor)
   UNITY_DEFINE_INSTANCED_PROP(float,_Cutoff)
   //UNITY_DEFINE_INSTANCED_PROP(float,_Metallic)
   //UNITY_DEFINE_INSTANCED_PROP(float,_Smoothness)
UNITY_INSTANCING_BUFFER_END(UnityPerMaterial)

//输入数据配置
struct InputConfig
{
   float2 baseUV;
   float2 detailUV;
};
//获取输入配置
InputConfig GetInputConfig(float2 baseUV,float2 detailUV=0.0)
{
   InputConfig c;
   c.baseUV=baseUV;
   c.detailUV=detailUV;
   return c;
};

float2 TransformBaseUV(float2 baseUV)
{
   float4 baseST=UNITY_ACCESS_INSTANCED_PROP(UnityPerMaterial,_BaseMap_ST);
   return baseUV*baseST.xy+baseST.zw;
}
//获取Albedo
float4 GetBase(InputConfig c)
{
   float4 map=SAMPLE_TEXTURE2D(_BaseMap,sampler_BaseMap,c.baseUV);
   float4 color=UNITY_ACCESS_INSTANCED_PROP(UnityPerMaterial,_BaseColor);
   return map*color;
}
//获取Cutoff
float GetCutoff(float2 baseUV)
{
    return UNITY_ACCESS_INSTANCED_PROP(UnityPerMaterial,_Cutoff);
}

float3 GetEmission (InputConfig c) {
	return GetBase(c).rgb;
}

//菲尼尔
float GetFresnel (float2 baseUV) {
	return 0.0;
}
//金属度
//float GetMetallic(float2 baseUV)
//{
//   return UNITY_ACCESS_INSTANCED_PROP(UnityPerMaterial,_Metallic);
//}
//Instance Buffer 数据
//float GetSmoothness(float2 baseUV)
//{
//   return UNITY_ACCESS_INSTANCED_PROP(UnityPerMaterial,_Smoothness);
//}
#endif