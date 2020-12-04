#ifndef CUSTOM_LIT_INPUT_INCLUDED
#define CUSTOM_LIT_INPUT_INCLUDED

#define INPUT_PROP(name) UNITY_ACCESS_INSTANCED_PROP(UnityPerMaterial,name)

//Alpha Blend Texture
TEXTURE2D(_BaseMap);
//Mask 纹理
TEXTURE2D(_MaskMap);
//自发光
TEXTURE2D(_EmissionMap);
SAMPLER(sampler_BaseMap);
//Detail 纹理
TEXTURE2D(_DetailMap);
SAMPLER(sampler_DetailMap);
//Normal map
TEXTURE2D(_NormalMap);
//Detail Map
TEXTURE2D(_DetailNormalMap);
//Per Material Property
UNITY_INSTANCING_BUFFER_START(UnityPerMaterial)
   UNITY_DEFINE_INSTANCED_PROP(float4,_BaseMap_ST)
   //Detail Map
   UNITY_DEFINE_INSTANCED_PROP(float4,_DetailMap_ST)
   UNITY_DEFINE_INSTANCED_PROP(float4,_BaseColor)
   UNITY_DEFINE_INSTANCED_PROP(float,_Cutoff)
   UNITY_DEFINE_INSTANCED_PROP(float,_Metallic)
   UNITY_DEFINE_INSTANCED_PROP(float,_Occlusion)
   UNITY_DEFINE_INSTANCED_PROP(float,_Smoothness)
   UNITY_DEFINE_INSTANCED_PROP(float4,_EmissionColor) //float4 
   UNITY_DEFINE_INSTANCED_PROP(float,_Fresnel)
   UNITY_DEFINE_INSTANCED_PROP(float,_DetailAlbedo)
   UNITY_DEFINE_INSTANCED_PROP(float,_DetailSmoothness)
   UNITY_DEFINE_INSTANCED_PROP(float,_NormalScale)
   UNITY_DEFINE_INSTANCED_PROP(float,_DetailNormalScale)
UNITY_INSTANCING_BUFFER_END(UnityPerMaterial)

//输入数据配置
struct InputConfig
{
   float2 baseUV;
   float2 detailUV;
   //是否使用遮罩纹理
   bool useMask;
   //可选择细节纹理
   bool useDetail;
};
//获取输入配置
InputConfig GetInputConfig(float2 baseUV,float2 detailUV=0.0)
{
   InputConfig c;
   c.baseUV=baseUV;
   c.detailUV=detailUV;
   c.useMask=false;
   c.useDetail=false;
   return c;
};

//转换细节UV
float2 TransformDetailUV(float2 detailUV)
{
   float4 detailST=INPUT_PROP(_DetailMap_ST);
   return detailUV*detailST.xy+detailST.zw;
}

float2 TransformBaseUV(float2 baseUV)
{
   float4 baseST=INPUT_PROP(_BaseMap_ST);
   return baseUV*baseST.xy+baseST.zw;
}

float4 GetMask(InputConfig c)
{
   if(c.useMask)
   {
       return SAMPLE_TEXTURE2D(_MaskMap,sampler_BaseMap,c.baseUV);
   }
   return 1.0;
}

//获取detail
float4 GetDetail(InputConfig c)
{
   if(c.useDetail)
   {
     float4 map=SAMPLE_TEXTURE2D(_DetailMap,sampler_DetailMap,c.detailUV);
     return map*2.0-1.0;   //转换到【-1，1】
   }
   return 0.0;
}

//获取Albedo
float4 GetBase(InputConfig c)
{
   float4 map=SAMPLE_TEXTURE2D(_BaseMap,sampler_BaseMap,c.baseUV);
   float4 color=INPUT_PROP(_BaseColor);
   if(c.useDetail)
   {
        //细节纹理 _DetailAlbedo 强度参数
       float detail=GetDetail(c).r*INPUT_PROP(_DetailAlbedo);
       float mask=GetMask(c).b;
       //做颜色的加法
       //map+=detail;  Gammra
       map.rgb=lerp(sqrt(map.rgb),detail<0.0? 0.0:1.0,abs(detail)*mask);
       map.rgb*= map.rgb;
   }
   return map*color;
}

//获取Cutoff
float GetCutoff(InputConfig c)
{
    return UNITY_ACCESS_INSTANCED_PROP(UnityPerMaterial,_Cutoff);
}
//金属度
float GetMetallic(InputConfig c)
{
   float metallic=INPUT_PROP(_Metallic);
   metallic*=GetMask(c).r;
   return metallic;
}
//Instance Buffer 数据
float GetSmoothness(InputConfig c)
{
   float smoothness= INPUT_PROP(_Smoothness);
   smoothness*=GetMask(c).a;
   if(c.useDetail)
   {
     float detail=GetDetail(c).b*INPUT_PROP(_DetailSmoothness);
     float mask=GetMask(c).b;
     smoothness=lerp(smoothness,detail < 0.0 ? 0.0 : 1.0 , abs(detail)*mask);
   }
   return smoothness;
}
//自发光
float3 GetEmission(InputConfig c)
{
    float4 map=SAMPLE_TEXTURE2D(_EmissionMap,sampler_BaseMap,c.baseUV);
	float4 color=UNITY_ACCESS_INSTANCED_PROP(UnityPerMaterial,_EmissionColor);
	return map.rgb*color.rgb;
}
//菲涅尔
float GetFresnel(InputConfig c)
{
    return UNITY_ACCESS_INSTANCED_PROP(UnityPerMaterial,_Fresnel);
}

//遮挡
float GetOcclusion(InputConfig c)
{
   float strength=INPUT_PROP(_Occlusion);
   float occlusion=GetMask(c).g;
   occlusion=lerp(occlusion,1.0,strength);
   return occlusion;  //极端情况
}
//法线 纹理
float3 GetNormalTS(InputConfig c)
{
   float4 map=SAMPLE_TEXTURE2D(_NormalMap,sampler_BaseMap,c.baseUV);
   float scale=INPUT_PROP(_NormalScale);
   float3 normal=DecodeNormal(map,scale);
   if(c.useDetail)
   {
         //细节法线
       map=SAMPLE_TEXTURE2D(_DetailNormalMap,sampler_DetailMap,c.detailUV);
       scale=INPUT_PROP(_DetailNormalScale)*GetMask(c).b;
       float3 detail=DecodeNormal(map,scale);
       normal=BlendNormalRNM(normal,detail);
   }
   return normal;
}
#endif