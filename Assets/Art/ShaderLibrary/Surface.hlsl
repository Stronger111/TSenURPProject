#ifndef CUSTOM_SURFACE_INCLUDE
#define CUSTOM_SURFACE_INCLUDE

//表面属性
struct Surface
{
   float3 position;
   float3 normal;
   //Shadow Bias
   float3 interpolatedNormal;
   float3 viewDirection;
   //视野空间深度
   float depth;
   float3 color;
   float alpha;
   float metallic;
   //遮挡
   float occlusion;
   float smoothness;
   //菲涅尔
   float fresnelStrength;
   //抖动值
   float dither;
   //
   uint renderingLayerMask;
};

#endif