#ifndef CUSTOM_LIGHTING_INCLUDE
#define CUSTOM_LIGHTING_INCLUDE

float3 IncomingLight(Surface surface,Light light)
{
   //Diffuse Color
   return saturate(dot(surface.normal,light.direction))*light.color;
}
float3 GetLighting(Surface surface,Light light)
{
   return IncomingLight(surface,light)*surface.color;
}
//Diffuse Color * Albedo
float3 GetLighting(Surface surface)
{
   //循环四盏光的信息 颜色叠加
   float3 color=0.0;
   for(int i=0;i<GetDirectionalLightCount();i++)
   {
      color+=GetLighting(surface,GetDirectionalLight(i));
   }
   return color;
}

#endif