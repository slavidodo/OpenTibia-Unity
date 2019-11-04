Shader "OpenTibiaUnity/AppearanceType" {
    Properties {
		[NoScaleOffset] _MainTex("Main Texture", 2D) = "white" {}
		_HighlightColor("Highlight Color", Color) = (1.0,1.0,1.0,1.0)
    }

    SubShader {
        Tags { "RenderType"="Transparent" }
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
				float2 texcoord : TEXCOORD0;
				UNITY_VERTEX_INPUT_INSTANCE_ID
			};

			struct VERT_OUT {
				float2 texcoord : TEXCOORD0;
				float4 vertex : SV_POSITION;
				UNITY_VERTEX_INPUT_INSTANCE_ID
			};

			sampler2D _MainTex;

			float4 _HighlightColor;

			UNITY_INSTANCING_BUFFER_START(Props)
				UNITY_DEFINE_INSTANCED_PROP(float4, _MainTex_UV)
				UNITY_DEFINE_INSTANCED_PROP(float, _HighlightOpacity)
			UNITY_INSTANCING_BUFFER_END(Props)

			VERT_OUT vert(VERT_IN v) {
				VERT_OUT o;
				UNITY_SETUP_INSTANCE_ID(v);
				UNITY_TRANSFER_INSTANCE_ID(v, o);
				o.vertex = UnityObjectToClipPos(v.vertex);
				o.texcoord = (v.texcoord * UNITY_ACCESS_INSTANCED_PROP(Props, _MainTex_UV).xy) + UNITY_ACCESS_INSTANCED_PROP(Props, _MainTex_UV).zw;
				return o;
			}

			fixed4 frag(VERT_OUT i) : SV_Target {
				UNITY_SETUP_INSTANCE_ID(i);
				fixed4 _RegularColor = tex2D(_MainTex, i.texcoord);
				fixed3 _Blended = lerp(_RegularColor.rgb, _HighlightColor.rgb, UNITY_ACCESS_INSTANCED_PROP(Props, _HighlightOpacity));
				fixed4 _BlendedAndMasked = fixed4(_Blended, 1.0) * _RegularColor.a;
				return _BlendedAndMasked;
			}

			ENDCG
		}
    }
}
