#ifndef CUSTOM_GI_INCLUDE
#define CUSTOM_GI_INCLUDE

#include "Packages/com.unity.render-pipelines.core/ShaderLibrary/EntityLighting.hlsl"
#include "Packages/com.unity.render-pipelines.core/ShaderLibrary/ImageBasedLighting.hlsl" 
//Light Map IBL
TEXTURE2D(unity_Lightmap);
SAMPLER(samplerunity_Lightmap);
//ShadowMask
TEXTURE2D(unity_ShadowMask);
SAMPLER(samplerunity_ShadowMask);
//LPPVs
TEXTURE3D_FLOAT(unity_ProbeVolumeSH);
SAMPLER(samplerunity_ProbeVolumeSH);
//环境反射 间接高光部分
TEXTURECUBE(unity_SpecCube0);
SAMPLER(samplerunity_SpecCube0);

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
   //间接高光反射GI
   float3 specular;
   //ShadowMask存取GI的一部分
   ShadowMask shadowMask;
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
//采样Cubemap 环境反射部分
float3 SampleEnvironment(Surface surfaceWS,BRDF brdf)
{
    //3D 空间UV 反射方向
    float3 uvw=reflect(-surfaceWS.viewDirection,surfaceWS.normal);
	float mip=PerceptualRoughnessToMipmapLevel(brdf.perceptualRoughness);
	float4 environment=SAMPLE_TEXTURECUBE_LOD(unity_SpecCube0,samplerunity_SpecCube0,uvw,mip);
	return DecodeHDREnvironment(environment,unity_SpecCube0_HDR);
}
//Light Probe 球谐光部分
float3 SampleLightProbe(Surface surfaceWS)
{
    //开启 LIGHTMAP_ON 为开启就是 动态物体 受LightProbe影响 等于走两个分支 静态物体LightMap 动态物体球谐 LightProbe 
    #if  defined(LIGHTMAP_ON)
	   return 0.0;
	#else
	   if(unity_ProbeVolumeParams.x)
	   {
	      return SampleProbeVolumeSH4(TEXTURE3D_ARGS(unity_ProbeVolumeSH,samplerunity_ProbeVolumeSH),surfaceWS.position,surfaceWS.normal,
		          unity_ProbeVolumeWorldToObject,unity_ProbeVolumeParams.y,unity_ProbeVolumeParams.z,unity_ProbeVolumeMin.xyz,
				  unity_ProbeVolumeSizeInv.xyz);
	   }
	   else
	   {
	    float4 coefficients[7];
	   	coefficients[0] = unity_SHAr;
		coefficients[1] = unity_SHAg;
		coefficients[2] = unity_SHAb;
		coefficients[3] = unity_SHBr;
		coefficients[4] = unity_SHBg;
		coefficients[5] = unity_SHBb;
		coefficients[6] = unity_SHC;
		return max(0.0,SampleSH9(coefficients,surfaceWS.normal));
		}
	#endif
}
//采样ShadowMask 添加LPPvs支持
float4 SampleBakedShadows(float2 lightMapUV,Surface surfaceWS)
{
   #if defined(LIGHTMAP_ON)
       return SAMPLE_TEXTURE2D(unity_ShadowMask,samplerunity_ShadowMask,lightMapUV);
   #else
      if(unity_ProbeVolumeParams.x)
	  {
	     return SampleProbeOcclusion(TEXTURE3D_ARGS(unity_ProbeVolumeSH,samplerunity_ProbeVolumeSH),
		          surfaceWS.position,unity_ProbeVolumeWorldToObject,unity_ProbeVolumeParams.y,unity_ProbeVolumeParams.z,
				  unity_ProbeVolumeMin.xyz,unity_ProbeVolumeSizeInv.xyz);
	  }else
	  {
	     return unity_ProbesOcclusion;
	  }
   #endif
}

//全局光照部分
GI GetGI(float2 lightMapUV,Surface surfaceWS,BRDF brdf)
{
   GI gi;
   gi.diffuse = SampleLightMap(lightMapUV)+SampleLightProbe(surfaceWS);
   //间接高光部分
   gi.specular=SampleEnvironment(surfaceWS,brdf);
   //ShadowMask
   gi.shadowMask.always=false;
   gi.shadowMask.distance=false;
   gi.shadowMask.shadows=1.0;

   #if defined(_SHADOW_MASK_ALWAYS)
       gi.shadowMask.always=true;
	   gi.shadowMask.shadows=SampleBakedShadows(lightMapUV,surfaceWS);
   #elif defined(_SHADOW_MASK_DISTANCE)
       gi.shadowMask.distance=true;
       gi.shadowMask.shadows=SampleBakedShadows(lightMapUV,surfaceWS);
   #endif
   return gi;
}
#endif