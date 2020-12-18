#ifndef CUSTOM_UNLIT_PASS_INCLUDE
#define CUSTOM_UNLIT_PASS_INCLUDE

#include "../ShaderLibrary/Common.hlsl"
//支持SRP Batch
//CBUFFER_START(UnityPerMaterial)
//   //配置颜色
//   float4 _BaseColor;
//CBUFFER_END
//支持Instancing
struct Attributes
{
   float3 positionOS : POSITION;
   #if defined(_VERTEX_COLORS)
        float4 color : COLOR;
   #endif
   #if defined(_FLIPBOOK_BLENDING)
      float4 baseUV : TEXCOORD0;
      float flipbookBlend :TEXCOORD1;
   #else
      float2 baseUV : TEXCOORD0;
   #endif
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
   float4 positionCS_SS : SV_POSITION; //?屏幕空间？
   float2 baseUV : VAR_BASE_UV;
    #if defined(_FLIPBOOK_BLENDING)
       float3 flipbookUVB :VAR_FLIPBOOK;
    #endif
   UNITY_VERTEX_INPUT_INSTANCE_ID
};
//顶点着色器
Varyings UnlitPassVertex(Attributes input) 
{
    Varyings output;
    UNITY_SETUP_INSTANCE_ID(input);
    UNITY_TRANSFER_INSTANCE_ID(input,output);
    float3 positionWS=TransformObjectToWorld(input.positionOS.xyz);
    output.positionCS_SS=TransformWorldToHClip(positionWS);
    //顶点 访问 _BaseMap_ST
    float4 baseST=UNITY_ACCESS_INSTANCED_PROP(UnityPerMaterial,_BaseMap_ST);
    #if defined(_VERTEX_COLORS)
       output.color=input.color;
    #endif
    output.baseUV.xy=TransformBaseUV(input.baseUV.xy);
    #if defined(_FLIPBOOK_BLENDING)
       output.flipbookUVB.xy=TransformBaseUV(input.baseUV.zw);
       output.flipbookUVB.z=input.flipbookBlend;
    #endif
    return output;
}
//片元着色器
float4 UnlitPassFragment(Varyings input) :SV_TARGET
{
   UNITY_SETUP_INSTANCE_ID(input);
   InputConfig config=GetInputConfig(input.positionCS_SS,input.baseUV);
   //return float4(config.fragment.bufferDepth.xxx/20.0,1.0);
   //return GetBufferColor(config.fragment,0.05);
   #if defined(_VERTEX_COLORS)
     config.color=input.color
   #endif
   #if defined(_FLIPBOOK_BLENDING)
      config.flipbookUVB=input.flipbookUVB;
      config.flipbookBlending =true;
   #endif
   //是否开启过渡
   #if defined(_NEAR_FADE)
      config.nearFade=true;
   #endif
   //是否开启软粒子
   #if defined(_SOFT_PARTICLES)
      config.softParticles=true;
   #endif
   float4 base=GetBase(config);
   //float4 baseColor= UNITY_ACCESS_INSTANCED_PROP(UnityPerMaterial,_BaseColor);
  // float4 base =baseMap*baseColor;
   #if defined(_CLIPPING)
      clip(base.a-UNITY_ACCESS_INSTANCED_PROP(UnityPerMaterial,_Cutoff));
   #endif

   #if defined(_DISTORTION)
      float2 distortion=GetDistortion(config)*base.a;
      base.rgb=lerp(GetBufferColor(config.fragment,distortion).rgb,base.rgb,saturate(base.a-GetDistortionBlend(config)));
   #endif
   return float4(base.rgb,GetFinalAlpha(base.a));
} 
#endif