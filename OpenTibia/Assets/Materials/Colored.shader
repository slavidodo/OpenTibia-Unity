Shader "OpenTibiaUnity/Colored"
{
	Properties
	{
		[PerRendererData] _Color("Color", Color) = (1,1,1,1)
	}

		SubShader
	{
		Tags { "Queue" = "Transparent" "IgnoreProjector" = "True" "RenderType" = "Transparent" }
		Pass
		{
			Blend SrcAlpha OneMinusSrcAlpha
			Cull Off

			CGPROGRAM
			#pragma vertex vert
			#pragma fragment frag
			#pragma target 2.0
			#pragma multi_compile _ UNITY_SINGLE_PASS_STEREO STEREO_INSTANCING_ON STEREO_MULTIVIEW_ON
			#pragma multi_compile_instancing
			#include "UnityCG.cginc"

			struct appdata_t {
				float4 vertex : POSITION;
				UNITY_VERTEX_INPUT_INSTANCE_ID
			};
			struct v2f {
				float4 vertex : SV_POSITION;
				UNITY_VERTEX_INPUT_INSTANCE_ID
			};

			UNITY_INSTANCING_BUFFER_START(Props)
				UNITY_DEFINE_INSTANCED_PROP(float4, _Color)
			UNITY_INSTANCING_BUFFER_END(Props)

			v2f vert(appdata_t v)
			{
				v2f o;
				UNITY_SETUP_INSTANCE_ID(v);
				UNITY_TRANSFER_INSTANCE_ID(v, o);
				o.vertex = UnityObjectToClipPos(v.vertex);
				return o;
			}
			fixed4 frag(v2f i) : SV_Target
			{
				UNITY_SETUP_INSTANCE_ID(i);
				return UNITY_ACCESS_INSTANCED_PROP(Props, _Color);
			}
			ENDCG
		}
	}
}