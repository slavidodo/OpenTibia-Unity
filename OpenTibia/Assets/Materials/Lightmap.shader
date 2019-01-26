Shader "OpenTibiaUnity/Lightmap" {
    Properties {
		[NoScaleOffset] _MainTex("MainTexture", 2D) = "white" {}
    }

    SubShader {
        Tags { "RenderType"="Transparent" }
		Pass {
			Blend Off
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

			float4 _MainTex_ST;

			VERT_OUT vert(VERT_IN v) {
				VERT_OUT o;
				o.vertex = UnityObjectToClipPos(v.vertex);
				o.uv0 = TRANSFORM_TEX(v.uv0, _MainTex);
				return o;
			}

			fixed4 frag(VERT_OUT i) : SV_Target {
				fixed4 _MainTex_RGBA = tex2D(_MainTex, i.uv0);
				return _MainTex_RGBA;
			}

			ENDCG
		}
    }
}
