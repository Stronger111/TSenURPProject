#ifndef CUSTOM_LIT_INPUT_INCLUDED
#define CUSTOM_LIT_INPUT_INCLUDED

#define INPUT_PROP(name) UNITY_ACCESS_INSTANCED_PROP(UnityPerMaterial,name)

//Alpha Blend Texture
TEXTURE2D(_BaseMap);
SAMPLER(sampler_DistortionMap);
SAMPLER(sampler_BaseMap);
//扰动
TEXTURE2D(_DistortionMap);
//Per Material Property
UNITY_INSTANCING_BUFFER_START(UnityPerMaterial)
   UNITY_DEFINE_INSTANCED_PROP(float4,_BaseMap_ST)
   UNITY_DEFINE_INSTANCED_PROP(float4,_BaseColor)
   UNITY_DEFINE_INSTANCED_PROP(float,_NearFadeDistance)
   UNITY_DEFINE_INSTANCED_PROP(float,_NearFadeRange)
   UNITY_DEFINE_INSTANCED_PROP(float,_SoftParticlesDistance)
   UNITY_DEFINE_INSTANCED_PROP(float,_SoftParticlesRange)
   UNITY_DEFINE_INSTANCED_PROP(float,_DistortionStrength)
   UNITY_DEFINE_INSTANCED_PROP(float,_DistortionBlend)
   UNITY_DEFINE_INSTANCED_PROP(float,_Cutoff)
   UNITY_DEFINE_INSTANCED_PROP(float,_ZWrite)
   //UNITY_DEFINE_INSTANCED_PROP(float,_Smoothness)
UNITY_INSTANCING_BUFFER_END(UnityPerMaterial)

//输入数据配置
struct InputConfig
{
   Fragment fragment;
   float4 color;
   float2 baseUV;
   float3 flipbookUVB;
   bool flipbookBlending;
   //是否过渡
   bool nearFade;
   bool softParticles;
};
//获取输入配置
InputConfig GetInputConfig(float4 positionSS,float2 baseUV)
{
   InputConfig c;
   c.fragment=GetFragment(positionSS);
   c.color=1.0;
   c.baseUV=baseUV;
   c.flipbookUVB=0.0;
   c.flipbookBlending=false;
   c.nearFade=false;
   c.softParticles=false;
   return c;
};

float GetDistortionBlend(InputConfig c)
{
   return INPUT_PROP(_DistortionBlend);
}

float2 TransformBaseUV(float2 baseUV)
{
   float4 baseST=UNITY_ACCESS_INSTANCED_PROP(UnityPerMaterial,_BaseMap_ST);
   return baseUV*baseST.xy+baseST.zw;
}
//获取Albedo
float4 GetBase(InputConfig c)
{
   float4 baseMap=SAMPLE_TEXTURE2D(_BaseMap,sampler_BaseMap,c.baseUV);
   //开启翻页过渡
   if(c.flipbookBlending)
   {
       //采用第二套UV采样和第一套进行混合
       baseMap=lerp(baseMap,SAMPLE_TEXTURE2D(_BaseMap,sampler_BaseMap,c.flipbookUVB.xy),c.flipbookUVB.z);
   }
   if(c.nearFade)
   {
      float nearAttenuation=(c.fragment.depth-INPUT_PROP(_NearFadeDistance))/
             INPUT_PROP(_NearFadeRange);
      baseMap.a*=saturate(nearAttenuation);
   }
   //软粒子
   if(c.softParticles)
   {
       float depthDelta=c.fragment.bufferDepth-c.fragment.depth;
       float nearAttenuation=(depthDelta-INPUT_PROP(_SoftParticlesDistance))/INPUT_PROP(_SoftParticlesRange);
       baseMap.a*=saturate(nearAttenuation);
   }
   float4 baseColor=UNITY_ACCESS_INSTANCED_PROP(UnityPerMaterial,_BaseColor);
   return baseMap*baseColor*c.color;
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
//_ZWrite 写入表面是不透明得表面
float GetFinalAlpha(float alpha)
{
   return INPUT_PROP(_ZWrite) ? 1.0 : alpha;
}

float2 GetDistortion(InputConfig c)
{
   float4 rawMap=SAMPLE_TEXTURE2D(_DistortionMap,sampler_DistortionMap,c.baseUV);
   if(c.flipbookBlending)
   {
       rawMap=lerp(rawMap,SAMPLE_TEXTURE2D(_DistortionMap,sampler_DistortionMap,c.flipbookUVB.xy),c.flipbookUVB.z);
   }
   return DecodeNormal(rawMap,INPUT_PROP(_DistortionStrength)).xy;
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