#ifndef CUSTOM_LIGHTING_INCLUDE
#define CUSTOM_LIGHTING_INCLUDE

float3 IncomingLight(Surface surface,Light light)
{
   //Diffuse Color  法线和灯光方向
   return saturate(dot(surface.normal,light.direction))*light.color;
}

float3 GetLighting(Surface surface,BRDF brdf,Light light)
{
   return IncomingLight(surface,light)*DirectBRDF(surface,brdf,light);
}
//Diffuse Color * Albedo
float3 GetLighting(Surface surface,BRDF brdf)
{
   //循环四盏光的信息 颜色叠加
   float3 color=0.0;
   for(int i=0;i<GetDirectionalLightCount();i++)
   {
      color+=GetLighting(surface,brdf,GetDirectionalLight(i));
   }
   return color;
}

#endif