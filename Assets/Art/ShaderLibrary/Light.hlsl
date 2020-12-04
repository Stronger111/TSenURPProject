#ifndef CUSTOM_LIGHT_INCLUDE
#define CUSTOM_LIGHT_INCLUDE

struct Light
{
   float3 color;
   float3 direction;
   float attenuation;
};

#define MAX_DIRECTIONAL_LIGHT_COUNT 4
//其他灯光
#define MAX_OTHER_LIGHT_COUNT 64

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

  //其他灯光
  int _OtherLightCount;
  float4 _OtherLightColors[MAX_OTHER_LIGHT_COUNT];
  float4 _OtherLightPositions[MAX_OTHER_LIGHT_COUNT];
  //其他灯光方向
  float4 _OtherLightDirections[MAX_OTHER_LIGHT_COUNT];
  //聚光灯角度
  float4 _OtherLightSpotAngles[MAX_OTHER_LIGHT_COUNT];
CBUFFER_END

///获取灯光的数量
int GetDirectionalLightCount()
{
   return _DirectionalLightCount;
}
//其他灯光数量
int GetOtherLightCount()
{
   return _OtherLightCount;
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

//其他光源数据
Light GetOtherLight(int index,Surface surfaceWS,ShadowData shadowData)
{
    Light light;
    light.color=_OtherLightColors[index].rgb;
    float3 ray=_OtherLightPositions[index].xyz-surfaceWS.position;
    light.direction=normalize(ray);
    //点光源距离衰减
    float distanceSqr=max(dot(ray,ray),0.00001);
    //范围过渡
    float rangeAttenuation=Square(saturate(1.0-Square(distanceSqr*_OtherLightPositions[index].w)));
    //聚光灯衰减
    float4 spotAngles=_OtherLightSpotAngles[index];
    float spotAttenuation=saturate(dot(_OtherLightDirections[index].xyz,light.direction)*spotAngles.x+spotAngles.y);
    light.attenuation=spotAttenuation*rangeAttenuation/distanceSqr;
    return light;
}
#endif