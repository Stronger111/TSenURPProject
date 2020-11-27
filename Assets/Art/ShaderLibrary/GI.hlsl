#ifndef CUSTOM_GI_INCLUDE
#define CUSTOM_GI_INCLUDE

#include "Packages/com.unity.render-pipelines.core/ShaderLibrary/EntityLighting.hlsl"

TEXTURE2D(unity_Lightmap);
SAMPLER(samplerunity_Lightmap);

//开启LightMap
#if defined(LIGHTMAP_ON)
    #define GI_ATTRIBUTE_DATA float2 lightMapUV : TEXCOORD1;
	#define GI_VARYINGS_DATA float2 lightMapUV : VAR_LIGHT_MAP_UV;
	#define TRANSFER_GI_DATA(input,output) \
	        output.lightMapUV=input.lightMapUV * \
			unity_LightmapST.xy+unity_LightmapST.zw;
	#define GI_FRAGMENT_DATA(input) input.lightMapUV
#else
	#define GI_ATTRIBUTE_DATA
	#define GI_VARYINGS_DATA
	#define TRANSFER_GI_DATA(input,output)
	#define GI_FRAGMENT_DATA(input) 0.0
#endif

struct GI
{
   float3 diffuse;
};
//这些都是封装好的函数 还是要仔细看的  纹理和采样器状态 TEXTURE2D_ARGS 传递给
float3 SampleLightMap(float2 lightMapUV)
{
   //?? 暂时不明白 true 光是否被压缩
   //half4 decodeInstructions = half4(LIGHTMAP_HDR_MULTIPLIER, LIGHTMAP_HDR_EXPONENT, 0.0h, 0.0h);
   #if defined(LIGHTMAP_ON)
       return SampleSingleLightmap(TEXTURE2D_ARGS(unity_Lightmap,samplerunity_Lightmap),lightMapUV,float4(1.0,1.0,0.0,0.0),
	   #if defined(UNITY_LIGHTMAP_FULL_HDR)
				false,
	   #else
				true,
	   #endif
			float4(LIGHTMAP_HDR_MULTIPLIER, LIGHTMAP_HDR_EXPONENT, 0.0, 0.0));
   #else
      return 0.0;
   #endif
}
//Light Probe 球谐光部分
float3 SampleLightProbe(Surface surfaceWS)
{
    //开启 LIGHTMAP_ON 为开启就是 动态物体 受LightProbe影响 等于走两个分支 静态物体LightMap 动态物体球谐 LightProbe 
    #if  defined(LIGHTMAP_ON)
	   return 0.0;
	#else
	   float4 coefficients[7];
	   	coefficients[0] = unity_SHAr;
		coefficients[1] = unity_SHAg;
		coefficients[2] = unity_SHAb;
		coefficients[3] = unity_SHBr;
		coefficients[4] = unity_SHBg;
		coefficients[5] = unity_SHBb;
		coefficients[6] = unity_SHC;
		return max(0.0,SampleSH9(coefficients,surfaceWS.normal));
	#endif
}

//全局光照部分
GI GetGI(float2 lightMapUV,Surface surfaceWS)
{
   GI gi;
   gi.diffuse = SampleLightMap(lightMapUV)+SampleLightProbe(surfaceWS);
   return gi;
}
#endif