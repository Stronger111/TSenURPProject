Shader "TSen RP/Lit"
{
    Properties
    {
        _BaseMap("Texture",2D)="white"{}
        _BaseColor("Color",Color)=(0.5,0.5,0.5,1.0) //灰度颜色
        _Cutoff ("Alpha Cutoff",Range(0.0,1.0)) = 0.5
        //是否开启 Alpha Clipping
        [Toggle(_CLIPPING)]_Clipping ("Alpha Clipping",Float) =0
        [Toggle(_RECEIVE_SHADOWS)] _ReceiveShadows ("Receive Shadows",Float) = 1
        //阴影的模式
        [KeywordEnum(On,Clip,Dither,Off)] _Shadows ("Shadows",Float) =0
        //Metallic
        _Metallic ("Metallic",Range(0,1)) = 0
        //光滑度
        _Smoothness("Smoothness",Range(0,1)) = 0.5

		[Toggle(_PREMULTIPLY_ALPHA)] _PremulAlpha("Premultiply Alpha",Float)=0
        [Enum(UnityEngine.Rendering.BlendMode)] _SrcBlend ("Src Blend",Float) = 1
        [Enum(UnityEngine.Rendering.BlendMode)] _DstBlend ("Dst Blend",Float) = 0
        [Enum(Off,0,On,1)] _ZWrite ("Z Write",Float) = 1
    }
    SubShader
    {
        Tags { "LightMode"="CustomLit" }

        Pass
        {
            Blend [_SrcBlend] [_DstBlend]
            ZWrite [_ZWrite]
            HLSLPROGRAM
            #pragma target 3.5
            #pragma shader_feature _CLIPPING
            //接收阴影
            #pragma shader_feature _RECEIVE_SHADOWS
            //#pragma shader_feature _ _SHADOWS_CLIP _SHADOWS_DITHER
			#pragma shader_feature _PREMULTIPLY_ALPHA
            //软阴影
            #pragma multi_compile _ _DIRECTIONAL_PCF3 _DIRECTIONAL_PCF5 _DIRECTIONAL_PCF7
            #pragma multi_compile _ _CASCADE_BLEND_SOFT _CASCADE_BLEND_DITHER
            //GPU Instancing 生成两个变体 有GPU Instancing 支持得和没有GPU Instancing支持的
            #pragma multi_compile_instancing
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
            #pragma vertex ShadowCasterPassVertex
            #pragma fragment ShadowCasterPassFragment
            #include "ShadowCasterPass.hlsl"
            ENDHLSL
        }
    }
	//自定义GUI
	CustomEditor "CustomShaderGUI"
}
