#ifndef CUSTOM_LIGHT_INCLUDE
#define CUSTOM_LIGHT_INCLUDE

struct Light
{
   float3 color;
   float3 direction;
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
CBUFFER_END

///获取灯光的数量
int GetDirectionalLightCount()
{
   return _DirectionalLightCount;
}

Light GetDirectionalLight(int index)
{
   Light light;
   light.color=_DirectionalLightColors[index].rgb;
   light.direction=_DirectionalLightDirections[index].xyz;
   return light;
}

#endif