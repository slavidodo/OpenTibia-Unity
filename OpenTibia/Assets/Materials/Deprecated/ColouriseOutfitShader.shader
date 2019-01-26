Shader "OpenTibiaUnity/ColouriseOutfitShader"
{
	Properties
	{
		[NoScaleOffset] _MainTex("MainTexture", 2D) = "white" {}
		[NoScaleOffset] _ChannelsTex("ChannelsTexture", 2D) = "white" {}
		_HeadColor("HeadColor", Color) = (1,0.85,0.014,1)
		_TorsoColor("TorsoColor", Color) = (0.34,0.14,0.33,1)
		_LegsColor("LegsColor", Color) = (1,1,1,1)
		_DetailColor("DetailColor", Color) = (1,1,1,1)
	}

		SubShader
		{
			Tags { "RenderType" = "Transparent" }

			Pass
			{
				Blend SrcAlpha OneMinusSrcAlpha
				Cull Back

				CGPROGRAM
				#pragma vertex vert
				#pragma fragment frag

				#include "UnityCG.cginc"

				struct VERT_IN
				{
					float4 vertex : POSITION;
					float2 uv0 : TEXCOORD0;
					float2 uv1 : TEXCOORD1;
				};

				struct VERT_OUT
				{
					float2 uv0 : TEXCOORD0;
					float2 uv1 : TEXCOORD1;
					float4 vertex : SV_POSITION;
				};

				sampler2D _MainTex;
				sampler2D _ChannelsTex;
				fixed4 _HeadColor;
				fixed4 _TorsoColor;
				fixed4 _LegsColor;
				fixed4 _DetailColor;

				float4 _MainTex_ST;
				float4 _ChannelsTex_ST;

				VERT_OUT vert(VERT_IN v)
				{
					VERT_OUT o;
					o.vertex = UnityObjectToClipPos(v.vertex);
					o.uv0 = TRANSFORM_TEX(v.uv0, _MainTex);
					o.uv1 = TRANSFORM_TEX(v.uv1, _ChannelsTex);
					return o;
				}

				fixed ColorMask(fixed3 _InColor, fixed3 _InMaskColor, float _InRange, float _InFuzziness)
				{
					return saturate(1 - (distance(_InMaskColor, _InColor) - _InRange) / max(_InFuzziness, 1e-5));
				}

				fixed4 Blend_Mul_Fixed4(fixed4 _InBaseColor, fixed4 _InBlendColor, float _InOpacity)
				{
					return lerp(_InBaseColor, _InBaseColor * _InBlendColor, _InOpacity);
				}

				// This is the same as above by masking each channel & blend_multiply it with the original texture.
				fixed4 frag(VERT_OUT i) : SV_Target
				{
					fixed4 _MainTex_RGBA = tex2D(_MainTex, i.uv0);
					fixed4 _ChannelsTex_RGBA = tex2D(_ChannelsTex, i.uv1);

					// color mask for yellow, red, green, blue
					fixed _ChannelsTex_ColorMask_Yellow = ColorMask(_ChannelsTex_RGBA.rgb, fixed3(1, 1, 0), 0, 0);
					fixed _ChannelsTex_ColorMask_Red = ColorMask(_ChannelsTex_RGBA.rgb, fixed3(1, 0, 0), 0, 0);
					fixed _ChannelsTex_ColorMask_Green = ColorMask(_ChannelsTex_RGBA.rgb, fixed3(0, 1, 0), 0, 0);
					fixed _ChannelsTex_ColorMask_Blue = ColorMask(_ChannelsTex_RGBA.rgb, fixed3(0, 0, 1), 0, 0);

					// blend to our colors
					fixed4 _ChannelsTex_Blend_Yellow = Blend_Mul_Fixed4(_ChannelsTex_ColorMask_Yellow.xxxx, _HeadColor, 1);
					fixed4 _ChannelsTex_Blend_Red = Blend_Mul_Fixed4(_ChannelsTex_ColorMask_Red.xxxx, _TorsoColor, 1);
					fixed4 _ChannelsTex_Blend_Green = Blend_Mul_Fixed4(_ChannelsTex_ColorMask_Green.xxxx, _LegsColor, 1);
					fixed4 _ChannelsTex_Blend_Blue = Blend_Mul_Fixed4(_ChannelsTex_ColorMask_Blue.xxxx, _DetailColor, 1);

					fixed4 _ChannelsTex_FinalAdditive = _ChannelsTex_Blend_Yellow + _ChannelsTex_Blend_Red + _ChannelsTex_Blend_Green + _ChannelsTex_Blend_Blue;
					fixed4 _PropertyOut_Mul_Main = _ChannelsTex_FinalAdditive * _MainTex_RGBA;

					return fixed4(_PropertyOut_Mul_Main.rgb, _ChannelsTex_RGBA.a);
				}
				ENDCG
			}
		}
}
