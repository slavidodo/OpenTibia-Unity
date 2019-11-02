Shader "OpenTibiaUnity/OutfitType" {
	Properties {
		[NoScaleOffset] _MainTex("Main Texture", 2D) = "white" {}
		[NoScaleOffset] _ChannelsTex("Channels Texture", 2D) = "white" {}
		[PerRendererData] _HeadColor("Head Color", Color) = (1,0.85,0.014,1)
		[PerRendererData] _TorsoColor("Torso Color", Color) = (0.34,0.14,0.33,1)
		[PerRendererData] _LegsColor("Legs Color", Color) = (1,1,1,1)
		[PerRendererData] _DetailColor("Detail Color", Color) = (1,1,1,1)
		_HighlightColor("Highlight Color", Color) = (1,1,1,1)
		[PerRendererData] _HighlightOpacity("Highlight Opacity", Float) = 0
	}

	SubShader {
		Tags { "RenderType" = "Transparent" }
		Pass {
			Blend SrcAlpha OneMinusSrcAlpha
			Cull Off

			CGPROGRAM
			#pragma vertex vert
			#pragma fragment frag
			#pragma target 2.0
			#pragma multi_compile_instancing
			#include "UnityCG.cginc"

			struct VERT_IN {
				float4 vertex : POSITION;
				float2 texcoord0 : TEXCOORD0;
				float2 texcoord1 : TEXCOORD1;
				UNITY_VERTEX_INPUT_INSTANCE_ID
			};

			struct VERT_OUT {
				float2 texcoord0 : TEXCOORD0;
				float2 texcoord1 : TEXCOORD1;
				float4 vertex : SV_POSITION;
				UNITY_VERTEX_INPUT_INSTANCE_ID
			};

			sampler2D _MainTex;
			sampler2D _ChannelsTex;

			UNITY_INSTANCING_BUFFER_START(Props)
				UNITY_DEFINE_INSTANCED_PROP(float4, _MainTex_UV)
				UNITY_DEFINE_INSTANCED_PROP(float4, _ChannelsTex_UV)
				UNITY_DEFINE_INSTANCED_PROP(float4, _HeadColor)
				UNITY_DEFINE_INSTANCED_PROP(float4, _TorsoColor)
				UNITY_DEFINE_INSTANCED_PROP(float4, _LegsColor)
				UNITY_DEFINE_INSTANCED_PROP(float4, _DetailColor)
				UNITY_DEFINE_INSTANCED_PROP(float4, _HighlightColor)
				UNITY_DEFINE_INSTANCED_PROP(float, _HighlightOpacity)
			UNITY_INSTANCING_BUFFER_END(Props)

			VERT_OUT vert(VERT_IN v) {
				VERT_OUT o;
				UNITY_SETUP_INSTANCE_ID(v);
				UNITY_TRANSFER_INSTANCE_ID(v, o);
				o.vertex = UnityObjectToClipPos(v.vertex);
				o.texcoord0 = (v.texcoord0 * UNITY_ACCESS_INSTANCED_PROP(Props, _MainTex_UV).xy) + UNITY_ACCESS_INSTANCED_PROP(Props, _MainTex_UV).zw;
				o.texcoord1 = (v.texcoord1 * UNITY_ACCESS_INSTANCED_PROP(Props, _ChannelsTex_UV).xy) + UNITY_ACCESS_INSTANCED_PROP(Props, _ChannelsTex_UV).zw;
				return o;
			}

			fixed ColorMask(fixed3 _InColor, fixed3 _InMaskColor, float _InRange, float _InFuzziness) {
				return saturate(1 - (distance(_InMaskColor, _InColor) - _InRange) / max(_InFuzziness, 1e-5));
			}

			fixed4 Blend_Mul_Fixed4(fixed4 _InBaseColor, fixed4 _InBlendColor, float _InOpacity) {
				return lerp(_InBaseColor, _InBaseColor * _InBlendColor, _InOpacity);
			}

			fixed4 frag(VERT_OUT i) : SV_Target {
				UNITY_SETUP_INSTANCE_ID(i);
				fixed4 _MainTex_RGBA = tex2D(_MainTex, i.texcoord0);
				fixed4 _ChannelsTex_RGBA = tex2D(_ChannelsTex, i.texcoord1);

				// color mask for yellow, red, green, blue
				fixed _ChannelsTex_ColorMask_Yellow = ColorMask(_ChannelsTex_RGBA.rgb, fixed3(1, 1, 0), 0, 0);
				fixed _ChannelsTex_ColorMask_Red = ColorMask(_ChannelsTex_RGBA.rgb, fixed3(1, 0, 0), 0, 0);
				fixed _ChannelsTex_ColorMask_Green = ColorMask(_ChannelsTex_RGBA.rgb, fixed3(0, 1, 0), 0, 0);
				fixed _ChannelsTex_ColorMask_Blue = ColorMask(_ChannelsTex_RGBA.rgb, fixed3(0, 0, 1), 0, 0);

				// blend to our colors
				fixed4 _ChannelsTex_Blend_Yellow = Blend_Mul_Fixed4(_ChannelsTex_ColorMask_Yellow.xxxx, UNITY_ACCESS_INSTANCED_PROP(Props, _HeadColor) , 1);
				fixed4 _ChannelsTex_Blend_Red = Blend_Mul_Fixed4(_ChannelsTex_ColorMask_Red.xxxx, UNITY_ACCESS_INSTANCED_PROP(Props, _TorsoColor), 1);
				fixed4 _ChannelsTex_Blend_Green = Blend_Mul_Fixed4(_ChannelsTex_ColorMask_Green.xxxx, UNITY_ACCESS_INSTANCED_PROP(Props, _LegsColor), 1);
				fixed4 _ChannelsTex_Blend_Blue = Blend_Mul_Fixed4(_ChannelsTex_ColorMask_Blue.xxxx, UNITY_ACCESS_INSTANCED_PROP(Props, _DetailColor), 1);

				fixed4 _ChannelsTex_FinalAdditive = _ChannelsTex_Blend_Yellow + _ChannelsTex_Blend_Red + _ChannelsTex_Blend_Green + _ChannelsTex_Blend_Blue;
				fixed4 _PropertyOut_Mul_Main = _ChannelsTex_FinalAdditive * _MainTex_RGBA;

				fixed4 _RegularColor = fixed4(_PropertyOut_Mul_Main.rgb, _ChannelsTex_RGBA.a);
				fixed3 _Blended = lerp(_RegularColor.rgb, _HighlightColor.rgb, UNITY_ACCESS_INSTANCED_PROP(Props, _HighlightOpacity));
				fixed4 _BlendedAndMasked = fixed4(_Blended, 1.0) * _RegularColor.a;
				return _BlendedAndMasked;
			}
			ENDCG
		}
	}
}
