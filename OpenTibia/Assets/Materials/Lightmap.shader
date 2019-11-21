Shader "OpenTibiaUnity/Lightmap"
{
	Properties
	{
	}

	SubShader
	{
		Tags { "Queue" = "Transparent" "IgnoreProjector" = "True" "RenderType" = "Transparent" }

		Pass
		{
			Blend DstColor Zero
			BlendOp Add
			Cull Off

			CGPROGRAM
			#pragma vertex vert
			#pragma fragment frag
			#pragma target 2.0
			#include "UnityCG.cginc"

			struct appdata_t {
				float4 vertex : POSITION;
				float4 color : COLOR;
			};
			struct v2f {
				float4 vertex : SV_POSITION;
				float4 color : COLOR;
			};

			v2f vert(appdata_t v)
			{
				v2f o;
				o.vertex = UnityObjectToClipPos(v.vertex);
				o.color = v.color;
				return o;
			}
			fixed4 frag(v2f i) : SV_Target {
				return i.color;
			}
			ENDCG
		}
	}
}