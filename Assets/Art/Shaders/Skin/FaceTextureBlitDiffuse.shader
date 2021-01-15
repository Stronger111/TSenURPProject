// Unity built-in shader source. Copyright (c) 2016 Unity Technologies. MIT license (see license.txt)

Shader "Hidden/X2M/BlitCopyDiffuse" {
    Properties
    {
        _MainTex ("Texture", any) = "" {}
		_SubTex ("Texture", any) = "" {}
        _Color("Multiplicative color", Color) = (1.0, 1.0, 1.0, 1.0)
		_LerpWeight("Lerp Weight", float) = 0
		_RowNumAndIdx("Row Num And Index", Vector) = (0, 0, 0, 0)
		_DestPosAndSize("Dest Pos And Size", Vector) = (0, 0, 0, 0)
		_RotAndScale("Rotation And Scale", Vector) = (0, 1, 0, 0)
		_SrcPieceSize("Source Piece Size", Vector) = (0, 0, 0, 0)
		_Mode("Mode", Int) = 0
		_HasSubTexture("Has Sub Texture", Int) = 1
		_NeedMirror("Need Mirror", Int) = 1
    }
    SubShader 
	{
        Pass 
		{
            ZTest Always Cull Back ZWrite Off
			Blend SrcAlpha OneMinusSrcAlpha 
			//Color MaskRGB for not distrubing original emissive , Should be related to _Mode.
			ColorMask RGB

            CGPROGRAM
            #pragma vertex vert
            #pragma fragment frag
            #include "UnityCG.cginc"

            //UNITY_DECLARE_SCREENSPACE_TEXTURE(_MainTex);
			sampler2D _MainTex;
			sampler2D _SubTex;
            uniform float4 _MainTex_ST;
            uniform float4 _Color;

            struct appdata_t 
			{
			    //uint vertexID : SV_VertexID;
                float4 vertex : POSITION;
                float2 texcoord : TEXCOORD0;
                UNITY_VERTEX_INPUT_INSTANCE_ID
            };

            struct v2f 
			{
                float4 vertex : SV_POSITION;
                float2 texcoord : TEXCOORD0;
                UNITY_VERTEX_OUTPUT_STEREO
            };


			fixed _LerpWeight;
			fixed4 _RowNumAndIdx;
			fixed4 _DestPosAndSize;
			fixed2 _RotAndScale;
			fixed4 _SrcPieceSize;
			int _Mode;
			int _HasSubTexture;
			int _NeedMirror;

            v2f vert (appdata_t v)
            {
                v2f o;
                UNITY_SETUP_INSTANCE_ID(v);
                UNITY_INITIALIZE_VERTEX_OUTPUT_STEREO(o);
				//o.vertex=float4
			 //  (
				//   v.vertexID <=1 ? -1.0 :3.0,
				//   v.vertexID == 1 ? 3.0 :-1.0,
				//   0.0,1.0
			 //  );
            //           o.texcoord=float2
			 //  (
				//  v.vertexID <=1 ? 0.0 : 2.0,
				//  v.vertexID ==1 ? 2.0 : 0.0
			 //  );
			 //    //UV ·´
			  //   if(_ProjectionParams.x<0.0)
     //            {
				 //   v.texcoord.y=1.0-v.texcoord.y;
				 //}
				float4 vtx = v.vertex;
				//scale
				float2 size = _SrcPieceSize.xy / _DestPosAndSize.zw;
				float2 scaleRate = size * _RotAndScale.y;
				float2 centerOffset = 0.5 * (scaleRate - size);
				vtx.xy *= scaleRate;
				////rotate
				vtx.xy -= 0.5 * scaleRate;
			    float radAngel = _RotAndScale.x * UNITY_PI/180.0f;
				float rotX = cos(radAngel) * vtx.x - sin(radAngel) * vtx.y;
				float rotY = sin(radAngel) * vtx.x + cos(radAngel) * vtx.y;
				vtx.x = rotX + 0.5 * scaleRate.x;
				vtx.y = rotY + 0.5 * scaleRate.y;
				////translate
				vtx.xy += _DestPosAndSize.xy;
				vtx.xy -= centerOffset;

				vtx.w = 1.0f;
                o.vertex = UnityObjectToClipPos(vtx);
				o.texcoord=v.texcoord.xy;
				//o.texcoord=TRANSFORM_TEX(v.texcoord.xy, _MainTex);;
				//if (_NeedMirror > 0)
				//{
					o.texcoord.x = 1 - o.texcoord.x;
				//}
                return o;
            }

            fixed4 frag (v2f i) : SV_Target
            {
                UNITY_SETUP_STEREO_EYE_INDEX_POST_VERTEX(i);
				
				float piece = i.texcoord.y / _RowNumAndIdx.x;
				float realIdx = _RowNumAndIdx.x - _RowNumAndIdx.y - 1;
				float v = piece + 1.0 / _RowNumAndIdx.x * realIdx;

				piece = i.texcoord.x / _RowNumAndIdx.z;
				realIdx = _RowNumAndIdx.w;
				float u = piece + 1.0 / _RowNumAndIdx.z * realIdx;

				float2 texCoord1 = float2(u*_SrcPieceSize.z, v*_SrcPieceSize.w);
				float2 texCoord2 = float2(u, v);
				fixed4 color;
				fixed4 color1;
				fixed4 color2;
				//return fixed4(texCoord2.y,0,0,1);
				return tex2D(_MainTex,texCoord2);

				//SingleA,
				//RGB,
				//RGBA,
				if (_Mode < 1)
				{
					fixed alpha1 = UNITY_SAMPLE_SCREENSPACE_TEXTURE(_MainTex, texCoord1).a;
					fixed alpha2 = UNITY_SAMPLE_SCREENSPACE_TEXTURE(_SubTex, texCoord2).a;
					fixed alpha = _HasSubTexture ? lerp(alpha1, alpha2, _LerpWeight) : alpha1 * _LerpWeight;
					color = fixed4(_Color.rgb, alpha);
				}
				else if(_Mode < 2)
				{
					color1 = UNITY_SAMPLE_SCREENSPACE_TEXTURE(_MainTex, texCoord1);
					color2 = UNITY_SAMPLE_SCREENSPACE_TEXTURE(_SubTex, texCoord2);
					color = _HasSubTexture ? lerp(color1, color2, _LerpWeight) : color1;
					color.rgb *= _Color.rgb;
					color.a = 1;
				}
				else
				{
					color1 = UNITY_SAMPLE_SCREENSPACE_TEXTURE(_MainTex, texCoord1);
					color2 = UNITY_SAMPLE_SCREENSPACE_TEXTURE(_SubTex, texCoord2);
					color = _HasSubTexture ? (color2 * (color2.a*_LerpWeight) + color1 * (1-(color2.a*_LerpWeight))) : fixed4(color1.r, color1.g, color1.b, color1.a*_LerpWeight);
					color.rgb *= _Color.rgb;
				}
                return color;
            }
            ENDCG
        }
    }
    Fallback Off
}
