Shader "Hidden/PostProcessing/ColorTint"
{
    SubShader
    {
        Cull Off
        ZWrite Off
        ZTest Always

        Pass
        {
            HLSLPROGRAM
            #pragma vertex VertDefault
            #pragma fragment Frag
            #include "Packages/com.unity.postprocessing/PostProcessing/Shaders/StdLib.hlsl"
            TEXTURE2D_SAMPLER2D(_MainTex, sampler_MainTex);
            //struct appdata
            //{
            //    float4 vertex : POSITION;
            //    float2 uv : TEXCOORD0;
            //};

            //struct v2f
            //{
            //    float2 uv : TEXCOORD0;
            //    //UNITY_FOG_COORDS(1)
            //    float4 vertex : SV_POSITION;
            //};
            //sampler2D _MainTex;
            //float4 _MainTex_ST;
            float _BlendMultiply;
            float4 _Color;

            float4 Frag (VaryingsDefault i) : SV_Target
            {
                float4 color=SAMPLE_TEXTURE2D(_MainTex,sampler_MainTex,i.texcoord);
                color=lerp(color,color*_Color,_BlendMultiply);
                return color;
            }

            ENDHLSL
        }
    }
}
