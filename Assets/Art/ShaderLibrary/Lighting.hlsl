﻿#ifndef CUSTOM_LIGHTING_INCLUDE
#define CUSTOM_LIGHTING_INCLUDE

bool RenderingLayersOverlap(Surface surface,Light light)
{
   return (surface.renderingLayerMask & light.renderingLayerMask ) != 0;
}

float3 IncomingLight(Surface surface,Light light)
{
   //Diffuse Color  法线和灯光方向  阴影是乘到漫反射上面
   return saturate(dot(surface.normal,light.direction)*light.attenuation)*light.color;
}

float3 GetLighting(Surface surface,BRDF brdf,Light light)
{
   return IncomingLight(surface,light)*DirectBRDF(surface,brdf,light);
}
//Diffuse Color * Albedo 添加GI Debug GI
float3 GetLighting(Surface surfaceWS,BRDF brdf,GI gi)
{
   ShadowData shadowData=GetShadowData(surfaceWS);
   //Debug ShadowMask
   shadowData.shadowMask=gi.shadowMask;
   //return gi.shadowMask.shadows.rgb;
   //循环四盏光的信息 颜色叠加 w物体本身brdf.diffuse  gi.diffuse* brdf.diffuse
   float3 color=IndirectBRDF(surfaceWS,brdf,gi.diffuse,gi.specular);
   for(int i=0;i<GetDirectionalLightCount();i++)
   {
      Light light=GetDirectionalLight(i,surfaceWS,shadowData);
      if(RenderingLayersOverlap(surfaceWS,light))
      {
         color+=GetLighting(surfaceWS,brdf,light);
      }
   }

   #if defined(_LIGHTS_PER_OBJECT)
       for(int j=0;j<min(unity_LightData.y,8);j++)
       {
          int lightIndex=unity_LightIndices[j/4][j%4];
          Light light=GetOtherLight(lightIndex,surfaceWS,shadowData);
          if(RenderingLayersOverlap(surfaceWS,light))
          {
            color+=GetLighting(surfaceWS,brdf,light);
          }
       }
   #else
        //其他灯光计算
       for(int j=0;j<GetOtherLightCount();j++)
       {
           Light light=GetOtherLight(j,surfaceWS,shadowData);
           if(RenderingLayersOverlap(surfaceWS,light))
           {
               color+=GetLighting(surfaceWS,brdf,light);
           }
       }
   #endif

  
   return color;
}

#endif