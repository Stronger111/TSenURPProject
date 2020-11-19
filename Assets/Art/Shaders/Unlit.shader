﻿Shader "TSen RP/Unlit"
{
    Properties
    {
        _BaseMap("Texture",2D)="white"{}
        _BaseColor("Color",Color)=(1.0,1.0,1.0,1.0)
        _Cutoff ("Alpha Cutoff",Range(0.0,1.0)) = 0.5
        //是否开启 Alpha Clipping
        [Toggle(_CLIPPING)]_Clipping ("Alpha Clipping",Float) =0
        [Enum(UnityEngine.Rendering.BlendMode)] _SrcBlend ("Src Blend",Float) = 1
        [Enum(UnityEngine.Rendering.BlendMode)] _DstBlend ("Dst Blend",Float) = 0
        [Enum(Off,0,On,1)] _ZWrite ("Z Write",Float) = 1
    }
    SubShader
    {
        Tags { "RenderType"="Opaque" }
        LOD 100

        Pass
        {
            Blend [_SrcBlend] [_DstBlend]
            ZWrite [_ZWrite]
            HLSLPROGRAM
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
    }
}
