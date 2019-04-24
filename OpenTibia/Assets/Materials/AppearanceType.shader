Shader "OpenTibiaUnity/AppearanceType" {
    Properties {
		[NoScaleOffset] _MainTex("Main Texture", 2D) = "white" {}
		_HighlightColor("Highlight Color", Color) = (1.0,1.0,1.0,1.0)
		_HighlightOpacity("Highlight Opacity", Float) = 0
    }

    SubShader {
        Tags { "RenderType"="Transparent" }
		Pass {
			Blend SrcAlpha OneMinusSrcAlpha
			Cull Back

			CGPROGRAM
			#pragma vertex vert
			#pragma fragment frag

			#include "UnityCG.cginc"

			struct VERT_IN {
				float4 vertex : POSITION;
				float2 uv0 : TEXCOORD0;
			};

			struct VERT_OUT {
				float2 uv0 : TEXCOORD0;
				float4 vertex : SV_POSITION;
			};

			sampler2D _MainTex;
			fixed4 _HighlightColor;
			fixed _HighlightOpacity;

			float4 _MainTex_ST;

			VERT_OUT vert(VERT_IN v) {
				VERT_OUT o;
				o.vertex = UnityObjectToClipPos(v.vertex);
				o.uv0 = TRANSFORM_TEX(v.uv0, _MainTex);
				return o;
			}

			fixed4 frag(VERT_OUT i) : SV_Target {
				fixed4 _RegularColor = tex2D(_MainTex, i.uv0);
				fixed3 _Blended = lerp(_RegularColor.rgb, _HighlightColor.rgb, _HighlightOpacity);
				fixed4 _BlendedAndMasked = fixed4(_Blended, 1.0) * _RegularColor.a;
				return _BlendedAndMasked;
			}

			ENDCG
		}
    }
}
