#ifndef CUSTOM_LIT_PASS_INCLUDE
#define CUSTOM_LIT_PASS_INCLUDE

//#include "../ShaderLibrary/Common.hlsl"
#include "../ShaderLibrary/Surface.hlsl"
#include "../ShaderLibrary/Shadows.hlsl"
#include "../ShaderLibrary/Light.hlsl"
#include "../ShaderLibrary/BRDF.hlsl"
#include "../ShaderLibrary/GI.hlsl"
#include "../ShaderLibrary/Lighting.hlsl"
//支持SRP Batch
//CBUFFER_START(UnityPerMaterial)
//   //配置颜色
//   float4 _BaseColor;
//CBUFFER_END
//支持Instancing
struct Attributes
{
   float3 positionOS : POSITION;
   float3 normalOS : NORMAL;
   //切线空间
   float4 tangentOS :TANGENT;
   float2 baseUV : TEXCOORD0;
   //LightMap
   GI_ATTRIBUTE_DATA
   UNITY_VERTEX_INPUT_INSTANCE_ID
};

//Alpha Blend Texture
//TEXTURE2D(_BaseMap);
//SAMPLER(sampler_BaseMap);

//UNITY_INSTANCING_BUFFER_START(UnityPerMaterial)
//   UNITY_DEFINE_INSTANCED_PROP(float4,_BaseMap_ST)
//   UNITY_DEFINE_INSTANCED_PROP(float4,_BaseColor)
//   UNITY_DEFINE_INSTANCED_PROP(float,_Cutoff)
//   UNITY_DEFINE_INSTANCED_PROP(float,_Metallic)
//   UNITY_DEFINE_INSTANCED_PROP(float,_Smoothness)
//UNITY_INSTANCING_BUFFER_END(UnityPerMaterial)
//TSen  VAR_BASE_UV ???
//输出结构
struct Varyings
{
   float4 positionCS : SV_POSITION;
   float3 positionWS : VAR_POSITION;
   float2 baseUV : VAR_BASE_UV;
   #if defined(_DETAIL_MAP)
        //Detail
        float2 detailUV : VAR_DETAIL_UV;
   #endif
   //世界空间法线
   float3 normalWS : VAR_NORMAL;
   #if defined(_NORMAL_MAP)
     float4 tangentWS : VAR_TANGENT;
   #endif
   GI_VARYINGS_DATA
   UNITY_VERTEX_INPUT_INSTANCE_ID
};
//顶点着色器
Varyings LitPassVertex(Attributes input) 
{
    Varyings output;
    UNITY_SETUP_INSTANCE_ID(input);
    UNITY_TRANSFER_INSTANCE_ID(input,output);
    //转换
    TRANSFER_GI_DATA(input,output);
    output.positionWS=TransformObjectToWorld(input.positionOS.xyz);
    output.positionCS=TransformWorldToHClip(output.positionWS);
    //反向Z
    #if UNITY_REVERSED_Z
       output.positionCS.z=min(output.positionCS.z,output.positionCS.w * UNITY_NEAR_CLIP_VALUE);//近平面
    #else
       output.positionCS.z=max(output.positionCS.z,output.positionCS.w * UNITY_NEAR_CLIP_VALUE);//近平面
    #endif

    output.normalWS=TransformObjectToWorldNormal(input.normalOS);
    #if defined(_NORMAL_MAP)
        //切线转换到世界空间
        output.tangentWS=float4(TransformObjectToWorldDir(input.tangentOS.xyz),input.tangentOS.w);
    #endif
    //顶点 访问 _BaseMap_ST
    //float4 baseST=UNITY_ACCESS_INSTANCED_PROP(UnityPerMaterial,_BaseMap_ST);input.baseUV*baseST.xy+baseST.zw
    output.baseUV=TransformBaseUV(input.baseUV);
    #if defined(_DETAIL_MAP)
        output.detailUV=TransformDetailUV(input.baseUV);
    #endif
    return output;
}
//片元着色器
float4 LitPassFragment(Varyings input) :SV_TARGET
{
   UNITY_SETUP_INSTANCE_ID(input);
   //Debug LOD
   //#if defined(LOD_FADE_CROSSFADE)
   //   return -unity_LODFade.x;
   //#endif
   ClipLOD(input.positionCS.xy,unity_LODFade.x);
   //float4 baseMap=SAMPLE_TEXTURE2D(_BaseMap,sampler_BaseMap,input.baseUV); baseMap*baseColor
   //float4 baseColor= UNITY_ACCESS_INSTANCED_PROP(UnityPerMaterial,_BaseColor);
   //UNITY_ACCESS_INSTANCED_PROP(UnityPerMaterial,_Cutoff)
   InputConfig config=GetInputConfig(input.baseUV);
   #if defined(_MASK_MAP)
      config.useMask=true;
   #endif
   //细节纹理
   #if defined(_DETAIL_MAP)
      config.detailUV=input.detailUV;
      config.useDetail=true;
   #endif

   float4 base = GetBase(config);
   #if defined(_CLIPPING)
      clip(base.a-GetCutoff(config));
   #endif
   //可视化世界空间法线
   //base.rgb=normalize(input.normalWS);
   //输入数据
   Surface surface;
   //世界空间位置
   surface.position=input.positionWS;//normalize(input.normalWS)
   #if defined(_NORMAL_MAP)
      surface.normal=NormalTangentToWorld(GetNormalTS(config),input.normalWS,input.tangentWS);
   #else
      surface.normal=normalize(input.normalWS);
   #endif
   //世界法线
   surface.interpolatedNormal=input.normalWS;
   surface.viewDirection=normalize(_WorldSpaceCameraPos-input.positionWS);
   //TSen 取负为何？
   surface.depth=-TransformWorldToView(input.positionWS).z;
   surface.color=base.rgb;
   surface.alpha=base.a;
   surface.metallic=GetMetallic(config);
   surface.occlusion=GetOcclusion(config);
   surface.smoothness=GetSmoothness(config);
   //菲涅尔
   surface.fresnelStrength=GetFresnel(config);
   //抖动
   surface.dither=InterleavedGradientNoise(input.positionCS.xy,0);
   #if defined(_PREMULTIPLY_ALPHA)
     //Surface->BRDF
     BRDF brdf=GetBRDF(surface,true);
   #else
     BRDF brdf=GetBRDF(surface);
   #endif
   //GI 全局光照部分
   GI gi=GetGI(GI_FRAGMENT_DATA(input),surface,brdf);
   float3 color=GetLighting(surface,brdf,gi);
   //自发光
   color+=GetEmission(config);
   return float4(color,surface.alpha);
} 
#endif