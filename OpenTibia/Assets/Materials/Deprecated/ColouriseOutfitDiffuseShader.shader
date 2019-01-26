Shader "OpenTibiaUnity/ColouriseOutfitDiffuseShader"
{
	Properties
	{
		[NoScaleOffset] _MainTex("Main Texture", 2D) = "white" {}
		[NoScaleOffset] _ChannelsTex("Channels Texture", 2D) = "white" {}
		_HeadColor("Head Color", Color) = (1,0.85,0.014,1)
		_TorsoColor("Torso Color", Color) = (0.34,0.14,0.33,1)
		_LegsColor("Legs Color", Color) = (1,1,1,1)
		_DetailColor("Detail Color", Color) = (1,1,1,1)
		_HighlightColor("Highlight Color", Color) = (1,1,1,1) // TODO: this should always show the highlight color and opacity;
	}

	SubShader
	{
		Tags {
			"Queue" = "Transparent"
			"IgnoreProjector" = "True"
			"RenderType" = "Transparent"
			"PreviewType" = "Plane"
			"CanUseSpriteAtlas" = "True"
		}

		Blend SrcAlpha OneMinusSrcAlpha
		Cull Back
		Lighting Off
		ZWrite Off

		CGPROGRAM
		#pragma surface surf Lambert vertex:vert nofog nolightmap nodynlightmap keepalpha noinstancing
		#include "UnitySprites.cginc"

		sampler2D _ChannelsTex;
		fixed4 _HeadColor;
		fixed4 _TorsoColor;
		fixed4 _LegsColor;
		fixed4 _DetailColor;
		fixed4 _HighlightColor;

		struct Input
		{
			float2 uv_MainTex;
			float2 uv_ChannelsTex;
		};

		void ColorMask(fixed3 _InColor, fixed3 _InMaskColor, float _InRange, float _InFuzziness, out fixed _OutMask)
		{
			_OutMask = saturate(1 - (distance(_InMaskColor, _InColor) - _InRange) / max(_InFuzziness, 1e-5));
		}

		void Blend_Mul_Fixed4(fixed4 _InBaseColor, fixed4 _InBlendColor, float _InOpacity, out fixed4 _OutColor)
		{
			_OutColor = lerp(_InBaseColor, _InBaseColor * _InBlendColor, _InOpacity);
		}

		void vert(inout appdata_full v, out Input o)
		{
			UNITY_INITIALIZE_OUTPUT(Input, o);
		}

		void surf(Input i, inout SurfaceOutput o) {
			fixed4 _MainTex_RGBA = tex2D(_MainTex, i.uv_MainTex);
			fixed4 _ChannelsTex_RGBA = tex2D(_ChannelsTex, i.uv_ChannelsTex);

			// color mask for yellow, red, green, blue
			fixed _ChannelsTex_ColorMask_Yellow;
			fixed _ChannelsTex_ColorMask_Red;
			fixed _ChannelsTex_ColorMask_Green;
			fixed _ChannelsTex_ColorMask_Blue;
			ColorMask(_ChannelsTex_RGBA.rgb, fixed3(1, 1, 0), 0, 0, _ChannelsTex_ColorMask_Yellow);
			ColorMask(_ChannelsTex_RGBA.rgb, fixed3(1, 0, 0), 0, 0, _ChannelsTex_ColorMask_Red);
			ColorMask(_ChannelsTex_RGBA.rgb, fixed3(0, 1, 0), 0, 0, _ChannelsTex_ColorMask_Green);
			ColorMask(_ChannelsTex_RGBA.rgb, fixed3(0, 0, 1), 0, 0, _ChannelsTex_ColorMask_Blue);

			// blend to our colors
			fixed4 _ChannelsTex_Blend_Yellow;
			fixed4 _ChannelsTex_Blend_Red;
			fixed4 _ChannelsTex_Blend_Green;
			fixed4 _ChannelsTex_Blend_Blue;
			Blend_Mul_Fixed4(_ChannelsTex_ColorMask_Yellow.xxxx, _HeadColor, 1, _ChannelsTex_Blend_Yellow);
			Blend_Mul_Fixed4(_ChannelsTex_ColorMask_Red.xxxx, _TorsoColor, 1, _ChannelsTex_Blend_Red);
			Blend_Mul_Fixed4(_ChannelsTex_ColorMask_Green.xxxx, _LegsColor, 1, _ChannelsTex_Blend_Green);
			Blend_Mul_Fixed4(_ChannelsTex_ColorMask_Blue.xxxx, _DetailColor, 1, _ChannelsTex_Blend_Blue);

			fixed4 _ChannelsTex_FinalAdditive = _ChannelsTex_Blend_Yellow + _ChannelsTex_Blend_Red + _ChannelsTex_Blend_Green + _ChannelsTex_Blend_Blue;
			
			o.Albedo = _ChannelsTex_FinalAdditive * _MainTex_RGBA;
			o.Alpha = _ChannelsTex_RGBA.a;
		}
		ENDCG
	}
}