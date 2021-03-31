#ifndef CUSTOM_LIGHTING_INCLUDED
#define CUSTOM_LIGHTING_INCLUDED

void MainLight_float(float3 WorldPos,out float3 Dir,out float3 Color,out float DisAtten,out float ShadowAtten)
{
#ifdef SHADERGRAPH_PREVIEW
       Dir=half3(0.5,0.5,0);
       Color=1;
       DisAtten=1;
       ShadowAtten=1;
#else
    #if SHADOWS_SCREEN
       float4 clipPos=TransformWorldToHClip(WorldPos);
       float4 shadowCoord= ComputeScreenPos(clipPos);
    #else
       float4 shadowCoord=TransformWorldToShadowCoord(WorldPos);
    #endif
    Light mainLight=GetMainLight(shadowCoord);
    Dir=mainLight.direction;
    Color=mainLight.color;
    DisAtten=mainLight.distanceAttenuation;
    ShadowAtten=mainLight.shadowAttenuation;
#endif
}


void MainLight_half(float3 WorldPos,out half3 Dir,out half3 Color,out half DisAtten,out half ShadowAtten)
{
#ifdef SHADERGRAPH_PREVIEW
       Dir=half3(0.5,0.5,0);
       Color=1;
       DisAtten=1;
       ShadowAtten=1;
#else
    #if SHADOWS_SCREEN
       half4 clipPos=TransformWorldToHClip(WorldPos);
       half4 shadowCoord= ComputeScreenPos(clipPos);

    #else
       half4 shadowCoord=TransformWorldToShadowCoord(WorldPos);
    #endif
    Light mainLight=GetMainLight(shadowCoord);
    Dir=mainLight.direction;
    Color=mainLight.color;
    DisAtten=mainLight.distanceAttenuation;
    ShadowAtten=mainLight.shadowAttenuation;
#endif
}

//void Specular_float(out float3 Out)
//{
//}

void Specular_half(half3 Specular,half Smoothness,half3 Dir,half3 Color,half3 WorldNormal,half3 WorldView,out half3 Out)
{
    #ifdef SHADERGRAPH_PREVIEW
       Out=0;
    #else
       Smoothness=exp2(10*Smoothness+1);
       WorldNormal=normalize(WorldNormal);
       WorldView=SafeNormalize(WorldView);
       Out=LightingSpecular(Color,Dir,WorldNormal,WorldView,half4(Specular,0),Smoothness);
    #endif
}

void Specular_float(float3 Specular,float Smoothness,float3 Dir,float3 Color,float3 WorldNormal,float3 WorldView,out float3 Out)
{
    #ifdef SHADERGRAPH_PREVIEW
       Out=0;
    #else
       Smoothness=exp2(10*Smoothness+1);
       WorldNormal=normalize(WorldNormal);
       WorldView=SafeNormalize(WorldView);
       Out=LightingSpecular(Color,Dir,WorldNormal,WorldView,half4(Specular,0),Smoothness);
    #endif
}

//逐顶点 逐像素光源  Lambert 
void AddLight_half(half3 Spec,half3 Smoothness,half WorldPos,half3 WorldNormal,half3 WorldView,out half3 Diffuse , out half3 Specular)
{
#ifdef SHADERGRAPH_PREVIEW
    Diffuse=0;
    Specular=0;
#else
    Diffuse=0;
    Specular=0;
     Smoothness=exp2(10*Smoothness+1);
     WorldNormal=normalize(WorldNormal);
     WorldView=SafeNormalize(WorldView);
     //逐像素光的个数
     int pixelLightCount=GetAdditionalLightsCount();
     for(int i=0;i<pixelLightCount;i++)
     {
         Light light=GetAdditionalLight(i,WorldPos);
         half3 attenuatedLightColor=light.color*(light.distanceAttenuation*light.shadowAttenuation);
         Diffuse+=LightingLambert(attenuatedLightColor,light.direction,WorldNormal);
         Specular+=LightingSpecular(attenuatedLightColor,light.direction,WorldNormal,WorldView,half4(Specular,0),Smoothness);
     }
#endif
}


void AddLight_float(float3 Spec,float3 Smoothness,float WorldPos,float3 WorldNormal,float3 WorldView,out float3 Diffuse ,out float3 Specular)
{
#ifdef SHADERGRAPH_PREVIEW
    Diffuse=0;
    Specular=0;
#else
    Diffuse=0;
    Specular=0;
     Smoothness=exp2(10*Smoothness+1);
     WorldNormal=normalize(WorldNormal);
     WorldView=SafeNormalize(WorldView);
     //逐像素光的个数
     int pixelLightCount=GetAdditionalLightsCount();
     for(int i=0;i<pixelLightCount;i++)
     {
         Light light=GetAdditionalLight(i,WorldPos);
         float3 attenuatedLightColor=light.color*(light.distanceAttenuation*light.shadowAttenuation);
         Diffuse+=LightingLambert(attenuatedLightColor,light.direction,WorldNormal);
         Specular+=LightingSpecular(attenuatedLightColor,light.direction,WorldNormal,WorldView,float4(Specular,0),Smoothness);
     }
#endif
}
#endif