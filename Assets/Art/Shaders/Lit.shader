Shader "TSen RP/Lit"
{
    Properties
    {
        _BaseMap("Texture",2D)="white"{}
        _BaseColor("Color",Color)=(0.5,0.5,0.5,1.0) //灰度颜色
        [Toggle(_NORMAL_MAP)] _NormalMapToggle ("Normal Map",Float) = 0
        [NoScaleOffset] _NormalMap ("Normals",2D) = "bump" {}
        _NormalScale("Normal Scale",Range(0,1)) = 1
        _Cutoff ("Alpha Cutoff",Range(0.0,1.0)) = 0.5
        //是否开启 Alpha Clipping
        [Toggle(_CLIPPING)]_Clipping ("Alpha Clipping",Float) =0
        [Toggle(_RECEIVE_SHADOWS)] _ReceiveShadows ("Receive Shadows",Float) = 1
        //阴影的模式
        [KeywordEnum(On,Clip,Dither,Off)] _Shadows ("Shadows",Float) =0
        //遮罩纹理开关
        [Toggle(_MASK_MAP)] _MaskMapToggle ("Mask Map",Float) = 0
        //Mask纹理
        [NoScaleOffset] _MaskMap ("Mask (MODS)",2D) = "white" {} 
        //Metallic
        _Metallic ("Metallic",Range(0,1)) = 0
        //遮挡强度
        _Occlusion ("Occlusion",Range(0,1)) =1
        //光滑度
        _Smoothness("Smoothness",Range(0,1)) = 0.5
        //菲涅尔
        _Fresnel("Fresnel",Range(0,1))=1
		//自发光
		[NoScaleOffset] _EmissionMap ("Emission",2D) ="white" {}
		[HDR] _EmissionColor ("Emission",Color) = (0,0,0,0)
        [Toggle(_DETAIL_MAP)] _DetailMapToggle ("Detail Maps",Float) = 0
        _DetailMap ("Details",2D)= "linearGrey"{}
        //细节法线
        [NoScaleOffset] _DetailNormalMap ("Detail Normals",2D) = "bump" {}
        //细节太强 调整细节强度
        _DetailAlbedo("Detail Albedo",Range(0,1))=1
        //细节粗糙度
        _DetailSmoothness("Detail Smoothness",Range(0,1))=1
        _DetailNormalScale("Detail Normal Scale",Range(0,1)) = 1
		[HideInInspector] _MainTex ("Texture for Lightmap",2D) = "white" {}
		[HideInInspector] _Color ("Color for Lightmap",Color)=(0.5,0.5,0.5,1.0)
		[Toggle(_PREMULTIPLY_ALPHA)] _PremulAlpha("Premultiply Alpha",Float)=0
        [Enum(UnityEngine.Rendering.BlendMode)] _SrcBlend ("Src Blend",Float) = 1
        [Enum(UnityEngine.Rendering.BlendMode)] _DstBlend ("Dst Blend",Float) = 0
        [Enum(Off,0,On,1)] _ZWrite ("Z Write",Float) = 1
    }
    SubShader
    {
	    HLSLINCLUDE
		#include "../ShaderLibrary/Common.hlsl"
		#include "LitInput.hlsl"
		ENDHLSL

        Tags { "LightMode"="CustomLit" }

        Pass
        {
            Blend [_SrcBlend] [_DstBlend]
            ZWrite [_ZWrite]
            HLSLPROGRAM
            #pragma target 3.5
            #pragma shader_feature _NORMAL_MAP  //法线纹理
            #pragma shader_feature _MASK_MAP
            #pragma shader_feature _DETAIL_MAP
            #pragma shader_feature _CLIPPING
            //接收阴影
            #pragma shader_feature _RECEIVE_SHADOWS
            //#pragma shader_feature _ _SHADOWS_CLIP _SHADOWS_DITHER
			#pragma shader_feature _PREMULTIPLY_ALPHA
            //软阴影
            #pragma multi_compile _ _DIRECTIONAL_PCF3 _DIRECTIONAL_PCF5 _DIRECTIONAL_PCF7
            #pragma multi_compile _ _CASCADE_BLEND_SOFT _CASCADE_BLEND_DITHER
            //ShadowMask
            #pragma multi_compile _ _SHADOW_MASK_ALWAYS  _SHADOW_MASK_DISTANCE
            //开启LightMap
            #pragma multi_compile _ LIGHTMAP_ON
            //GPU Instancing 生成两个变体 有GPU Instancing 支持得和没有GPU Instancing支持的
            #pragma multi_compile_instancing
            //LOD Cross-Fade
            #pragma multi_compile _ LOD_FADE_CROSSFADE

            #pragma vertex LitPassVertex
            #pragma fragment LitPassFragment
            #include "LitPass.hlsl"

            struct appdata
            {
                float4 vertex : POSITION;
                float2 uv : TEXCOORD0;
            };

            struct v2f
            {
                float2 uv : TEXCOORD0;
                float4 vertex : SV_POSITION;
            };

            sampler2D _MainTex;
            float4 _MainTex_ST;

            ENDHLSL
        }

        Pass
        {
            //阴影投射Pass
            Tags {"LightMode"="ShadowCaster"}

            ColorMask 0
            HLSLPROGRAM
            #pragma target 3.5
            //#pragma shader_feature _CLIPPING
            #pragma shader_feature _ _SHADOWS_CLIP _SHADOWS_DITHER
			#pragma multi_compile_instancing
            //LOD Cross-Fade
            #pragma multi_compile _ LOD_FADE_CROSSFADE
            #pragma vertex ShadowCasterPassVertex
            #pragma fragment ShadowCasterPassFragment
            #include "ShadowCasterPass.hlsl"
            ENDHLSL
        }

		Pass
		{
		   Tags {"LightMode"="Meta"}  //金属流工作模式

		   Cull Off

		   HLSLPROGRAM
		   #pragma target 3.5
		   #pragma vertex MetaPassVertex
		   #pragma fragment MetaPassFragment
		   #include "MetaPass.hlsl"
		   ENDHLSL
		}
    }
	//自定义GUI
	CustomEditor "CustomShaderGUI"
}
