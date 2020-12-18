Shader "TSen RP/Particles/Unlit"
{
    Properties
    {
        _BaseMap("Texture",2D)="white"{}
        [HDR]_BaseColor("Color",Color)=(1.0,1.0,1.0,1.0)
        [Toggle(_VERTEX_COLORS)]_VertexColor("Vertex Colors",Float) = 0
        //是否支持广告牌过渡
        [Toggle(_FLIPBOOK_BLENDING)] _FlipbookBlending ("Flipbook Blending",Float) = 0
        //根据深度进行渐变过渡
        [Toggle(_NEAR_FADE)] _NearFade ("Near Fade",Float) = 0
        _NearFadeDistance ("Near Fade Distance",Range(0.0,10.0))=1
        _NearFadeRange ("Near Fade Range",Range(0.01,10.0)) =1
        //软粒子
        [Toggle(_SOFT_PARTICLES)] _SoftParticles ("Soft Particles",Float) = 0
        _SoftParticlesDistance ("Soft Partices Distance",Range(0.0,10.0)) = 0
        _SoftParticlesRange ("Soft Partices Range",Range(0.01,10.0)) = 1
        //开启扰动粒子
        [Toggle(_DISTORTION)] _Distortion ("Distortion",Float) = 0
        [NoScaleOffset] _DistortionMap ("Distortion Vectors",2D) = "bump" {}
        _DistortionStrength("Distortion Strength",Range(0.0,0.2)) = 0.1
        _DistortionBlend("Distortion Blend",Range(0.0,1.0)) = 1
        _Cutoff ("Alpha Cutoff",Range(0.0,1.0)) = 0.5
        //是否开启 Alpha Clipping
        [Toggle(_CLIPPING)]_Clipping ("Alpha Clipping",Float) =0
		[HDR] _BaseColor("Color", Color) = (1.0, 1.0, 1.0, 1.0)
        [Enum(UnityEngine.Rendering.BlendMode)] _SrcBlend ("Src Blend",Float) = 1
        [Enum(UnityEngine.Rendering.BlendMode)] _DstBlend ("Dst Blend",Float) = 0
        [Enum(Off,0,On,1)] _ZWrite ("Z Write",Float) = 1
    }
    SubShader
    {
	    HLSLINCLUDE
		#include "../ShaderLibrary/Common.hlsl"
		#include "UnlitInput.hlsl"
		ENDHLSL

        Tags { "RenderType"="Opaque" }
        LOD 100

        Pass
        {
            Blend [_SrcBlend] [_DstBlend] ,One OneMinusSrcAlpha
            ZWrite [_ZWrite]
            HLSLPROGRAM
            #pragma shader_feature _VERTEX_COLOR
            #pragma shader_feature _FLIPBOOK_BLENDING
            #pragma shader_feature _NEAR_FADE
            //软粒子
            #pragma shader_feature _SOFT_PARTICLES
            //扰动
            #pragma shader_feature _DISTORTION
            #pragma shader_feature _CLIPPING
            //GPU Instancing 生成两个变体 有GPU Instancing 支持得和没有GPU Instancing支持的
            #pragma multi_compile_instancing
            #pragma vertex UnlitPassVertex
            #pragma fragment UnlitPassFragment
            #include "UnlitPass.hlsl"

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
            #pragma vertex ShadowCasterPassVertex
            #pragma fragment ShadowCasterPassFragment
            #include "ShadowCasterPass.hlsl"
            ENDHLSL
        }
    }
	//CustomEditor "CustomShaderGUI"
}
