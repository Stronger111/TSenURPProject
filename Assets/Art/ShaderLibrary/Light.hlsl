#ifndef CUSTOM_LIGHT_INCLUDE
#define CUSTOM_LIGHT_INCLUDE

struct Light
{
   float3 color;
   float3 direction;
   float attenuation;
};

#define MAX_DIRECTIONAL_LIGHT_COUNT 4

//灯光数据
CBUFFER_START(_CustomLight)
  //方向光灯光颜色
  //float3 _DirectionalLightColor;
  //方向光灯光方向
  //float3 _DirectionalLightDirection;
  int _DirectionalLightCount;
  float4 _DirectionalLightColors[MAX_DIRECTIONAL_LIGHT_COUNT];
  float4 _DirectionalLightDirections[MAX_DIRECTIONAL_LIGHT_COUNT];
  //阴影
  float4 _DirectionalLightShadowData[MAX_DIRECTIONAL_LIGHT_COUNT];
CBUFFER_END

///获取灯光的数量
int GetDirectionalLightCount()
{
   return _DirectionalLightCount;
}
//获取方向光阴影数据
DirectionalShadowData GetDirectionalShadowData(int lightIndex,ShadowData shadowData)
{
   DirectionalShadowData data;
   //阴影强度 *shadowData.strength 后面进行实时阴影到Bake 阴影过渡
   data.strength=_DirectionalLightShadowData[lightIndex].x;
   //+Cascade Index索引
   data.tileIndex=_DirectionalLightShadowData[lightIndex].y+shadowData.cascadeIndex;
   //Noraml Bias
   data.normalBias=_DirectionalLightShadowData[lightIndex].z;
   //Shadow Mask
   data.shadowMaskChannel=_DirectionalLightShadowData[lightIndex].w;
   return data;
}

Light GetDirectionalLight(int index,Surface surfaceWS,ShadowData shadowData)
{
   Light light;
   light.color=_DirectionalLightColors[index].rgb;
   light.direction=_DirectionalLightDirections[index].xyz;
   //Shadow 部分
   DirectionalShadowData dirShadowData=GetDirectionalShadowData(index,shadowData);
   light.attenuation=GetDirectionalShadowAttenuation(dirShadowData,shadowData,surfaceWS);
   //light.attenuation=shadowData.cascadeIndex*0.25;
   return light;
}

#endif