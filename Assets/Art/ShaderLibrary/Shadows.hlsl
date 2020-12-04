﻿#ifndef CUSTOM_SHADOWS_INCLUDED
#define CUSTOM_SHADOWS_INCLUDED
#include "Packages/com.unity.render-pipelines.core/ShaderLibrary/Shadow/ShadowSamplingTent.hlsl"
//Soft Shadow
#if defined(_DIRECTIONAL_PCF3)
  #define DIRECTIONAL_FILTER_SAMPLES 4
  #define DIRECTIONAL_FILTER_SETUP SampleShadow_ComputeSamples_Tent_3x3
#elif defined(_DIRECTIONAL_PCF5)
	#define DIRECTIONAL_FILTER_SAMPLES 9
	#define DIRECTIONAL_FILTER_SETUP SampleShadow_ComputeSamples_Tent_5x5
#elif defined(_DIRECTIONAL_PCF7)
	#define DIRECTIONAL_FILTER_SAMPLES 16
	#define DIRECTIONAL_FILTER_SETUP SampleShadow_ComputeSamples_Tent_7x7
#endif 

//默认支持4盏光方向阴影
#define MAX_SHADOWED_DIRECTIONAL_LIGHT_COUNT 4
//最大的Cascade
#define MAX_CASCADE_COUNT 4
//灯光空间ShadowMap
TEXTURE2D_SHADOW(_DirectionalShadowAtlas);
#define SHADOW_SAMPLER sampler_linear_clamp_compare
SAMPLER_CMP(SHADOW_SAMPLER);
//ShadowMask
struct ShadowMask
{
   //总是开启ShadowMask
   bool always;
   //是否开启distance shadow mask
   bool distance;
   float4 shadows;
};

//索引是由每一个片段进行决定的
struct ShadowData
{
   int cascadeIndex;
   //cascade 过渡
   float cascadeBlend;
   //阴影强度 超过Casade 强度为0
   float strength;
   //ShadowMask
   ShadowMask shadowMask;
};

struct DirectionalShadowData
{
   float strength;
   int tileIndex;
   float normalBias;
   //Shadow Mask 通道
   int shadowMaskChannel;
};

CBUFFER_START(_CustomShadows)
   int _CascadeCount;
   float4 _CascadeCullingSpheres[MAX_CASCADE_COUNT];
   float4 _CascadeData[MAX_CASCADE_COUNT];
   float4x4 _DirectionalShadowMatrices[MAX_SHADOWED_DIRECTIONAL_LIGHT_COUNT*MAX_CASCADE_COUNT];
   //阴影过渡
   float4 _ShadowDistanceFade;
   //大小
   float4 _ShadowAtlasSize;
CBUFFER_END



//计算阴影过渡的衰减 distance View 空间深度
float FadedShadowStrength(float distance,float scale,float fade)
{
   return saturate((1.0-distance*scale)*fade);
}

//获得阴影数据
ShadowData GetShadowData(Surface surfaceWS)
{
   ShadowData data;
   //ShadowMask数据
   data.shadowMask.always= false;
   data.shadowMask.distance=false;
   data.shadowMask.shadows=1.0;
   //两个Cascade的过渡
   data.cascadeBlend=1.0;
   //外层控制深度<深度显示距离才有深度强度surfaceWS.depth<_ShadowDistance?1.0:0.0
   data.strength=FadedShadowStrength(surfaceWS.depth,_ShadowDistanceFade.x,_ShadowDistanceFade.y);
   int i;
   for(i=0;i<_CascadeCount;i++)
   {
      float4 sphere=_CascadeCullingSpheres[i];
      float distanceSqr=DistanceSquared(surfaceWS.position,sphere.xyz);
      //距离小于半径在球体内
      if(distanceSqr<sphere.w){
         float fade=FadedShadowStrength(distanceSqr,_CascadeData[i].x,_ShadowDistanceFade.z);
         //最后一级Cascade
         if(i==_CascadeCount-1){
            data.strength*=fade;
         }else{
             data.cascadeBlend=fade;
         }
         break;
      }
      //片段的世界坐标没有在任何的Cascade里面
      if(i==_CascadeCount)
      {
         data.strength=0.0;
      }
      //抖动过滤
      #if defined(_CASCADE_BLEND_DITHER)
         else if(data.cascadeBlend < surfaceWS.dither)
         {
            i+=1;
         }
      #endif
      //判断如果没有使用软阴影 不用Blend
      #if !defined(_CASCADE_BLEND_SOFT)
         data.cascadeBlend=1.0;
      #endif
      data.cascadeIndex=i;
   }

   return data;
}

float SampleDirectionalShadowAtlas(float3 positionSTS)
{
   //positionSTS 采样位置
   return SAMPLE_TEXTURE2D_SHADOW(_DirectionalShadowAtlas,SHADOW_SAMPLER,positionSTS);
}
///PCF 过滤
float FilterDirectionalShadow(float3 positionSTS)
{
   #if defined(DIRECTIONAL_FILTER_SETUP)
      float weights[DIRECTIONAL_FILTER_SAMPLES];
      float2 positions[DIRECTIONAL_FILTER_SAMPLES];
      float4 size=_ShadowAtlasSize.yyxx;
      DIRECTIONAL_FILTER_SETUP(size,positionSTS.xy,weights,positions);
      float shadow=0;
      for(int i=0;i<DIRECTIONAL_FILTER_SAMPLES;i++)
      {
         shadow+=weights[i]*SampleDirectionalShadowAtlas(float3(positions[i].xy,positionSTS.z));
      }
      return shadow;
   #else
      return SampleDirectionalShadowAtlas(positionSTS);
   #endif
}
//判断是否开启ShadowMask
float GetBakedShadow(ShadowMask mask,int channel)
{
   //默认无Shadow
   float shadow=1.0;
   if( mask.always ||mask.distance)
   {
      if(channel >=0)
      {
          shadow=mask.shadows[channel];
      }
   }
   return shadow;
}
//测试ShadowMask
float MixMaskedAndRealtimeShadows(ShadowData global,float shadow,int shadowMaskChannel,float strength)
{
   float baked=GetBakedShadow(global.shadowMask,shadowMaskChannel);
   //Always ShadowMaks
   if(global.shadowMask.always)
   {
       shadow=lerp(1.0,shadow,global.strength);
       shadow=min(baked,shadow);
       return lerp(1.0,shadow,strength);
   }

   if(global.shadowMask.distance)
   {
       //阴影过渡
       shadow=lerp(baked,shadow,global.strength);
       return lerp(1.0,shadow,strength);
   }
   return lerp(1.0,shadow,strength*global.strength); //传统流程
}
//仅仅支持Baked 阴影 
float GetBakedShadow(ShadowMask mask,int channel,float strength)
{
   if(mask.always || mask.distance)
   {
       return lerp(1.0,GetBakedShadow(mask,channel),strength);
   }
   return 1.0;
}
//重构 
float GetCascadedShadow(DirectionalShadowData directional,ShadowData global,Surface surfaceWS)
{
    //Normal bias 偏移一个纹素大小 在世界方向上
   float3 normalBias=surfaceWS.interpolatedNormal*(directional.normalBias*_CascadeData[global.cascadeIndex].y);
   float3 positionSTS=mul(_DirectionalShadowMatrices[directional.tileIndex],float4(surfaceWS.position+normalBias,1.0)).xyz;
   float shadow=FilterDirectionalShadow(positionSTS);
   //判断不完全在任何一级的Cascade里面
   if(global.cascadeBlend<1.0)
   {
       normalBias=surfaceWS.interpolatedNormal*(directional.normalBias*_CascadeData[global.cascadeIndex+1].y);
       positionSTS=mul(_DirectionalShadowMatrices[directional.tileIndex+1],float4(surfaceWS.position+normalBias,1.0)).xyz;
       shadow=lerp(FilterDirectionalShadow(positionSTS),shadow,global.cascadeBlend);
   }
   //根据阴影强度进行Lerp
   return shadow;
}

//计算阴影的衰减
float GetDirectionalShadowAttenuation(DirectionalShadowData directional,ShadowData global,Surface surfaceWS)
{
   //本物体不接受阴影 返回为1
   #if !defined(_RECEIVE_SHADOWS)
      return 1.0;
   #endif

   float shadow;
   //如果阴影强度是0 总是返回Bake阴影 
   if(directional.strength * global.strength <=0.0)
   {
      return shadow=GetBakedShadow(global.shadowMask,directional.shadowMaskChannel,abs(directional.strength));
   }else
   {
       shadow=GetCascadedShadow(directional,global,surfaceWS);
       //测试
       shadow=MixMaskedAndRealtimeShadows(global,shadow,directional.shadowMaskChannel,directional.strength);
       shadow=lerp(1.0,shadow,directional.strength);
   }
  
   //根据阴影强度进行Lerp
   return shadow;
}



#endif