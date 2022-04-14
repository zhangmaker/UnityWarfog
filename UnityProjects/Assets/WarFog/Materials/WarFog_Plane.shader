Shader "WarFog/WarFog_Plane" {
	Properties {
		_MainTex ("Main Texture", 2D) = "white" {}
		_FogColor("Fog Color", Color) = (0,0,0,1)
		_BlurDistance("Blur Distance", Range(0.00001, 0.2)) = 0.01
	}
	SubShader {
		Tags { 
			"RenderType"="Transparent" 
			"Queue" = "Overlay"
			"IgnoreProjector " = "True"
		}
		LOD 300

		ZWrite On
		ZTest Always
		Blend SrcAlpha OneMinusSrcAlpha

		Pass {
			CGPROGRAM
			#pragma vertex vert
			#pragma fragment frag
			#pragma enable_d3d11_debug_symbols  //需要断点调试的Shader加上的  
			
			#include "UnityCG.cginc"

			struct appdata {
				float4 vertex : POSITION;
				float2 texcoord : TEXCOORD0;
				float4 color    : COLOR;
			};

			struct v2f {
				float4 vertex : SV_POSITION;
				float2 texcoord : TEXCOORD0;
				float4 color    : COLOR;
			};

			uniform sampler2D _MainTex;
			uniform float4 _MainTex_ST;
			uniform float4 _FogColor;
			uniform float _BlurDistance;

			static float GaussianKernel[9] = {
				0.0947416f, 0.118318f, 0.0947416f,
				0.118318f, 0.147761, 0.118318f,
				0.0947416f, 0.118318f, 0.0947416f

				/*0.05854983, 0.09653235, 0.05854983,
				0.09653235, 0.15915494, 0.09653235,
				0.05854983, 0.09653235, 0.05854983*/
			};
			
			v2f vert (appdata v) {
				v2f o;
				o.vertex = UnityObjectToClipPos(v.vertex);
				o.texcoord = TRANSFORM_TEX(v.texcoord, _MainTex);
				o.color = v.color;

				return o;
			}
			
			fixed4 frag (v2f IN) : SV_Target {
				// sample texture an blur
				fixed4 col1 = tex2D(_MainTex, float2(IN.texcoord.x - _BlurDistance, IN.texcoord.y + _BlurDistance));
				fixed4 col2 = tex2D(_MainTex, float2(IN.texcoord.x, IN.texcoord.y + _BlurDistance));
				fixed4 col3 = tex2D(_MainTex, float2(IN.texcoord.x + _BlurDistance, IN.texcoord.y + _BlurDistance));
				fixed4 col4 = tex2D(_MainTex, float2(IN.texcoord.x - _BlurDistance, IN.texcoord.y));
				fixed4 col5 = tex2D(_MainTex, IN.texcoord);
				fixed4 col6 = tex2D(_MainTex, float2(IN.texcoord.x + _BlurDistance, IN.texcoord.y));
				fixed4 col7 = tex2D(_MainTex, float2(IN.texcoord.x - _BlurDistance, IN.texcoord.y - _BlurDistance));
				fixed4 col8 = tex2D(_MainTex, float2(IN.texcoord.x, IN.texcoord.y - _BlurDistance));
				fixed4 col9 = tex2D(_MainTex, float2(IN.texcoord.x + _BlurDistance, IN.texcoord.y - _BlurDistance));

				fixed fog1 = (col1.r + col1.g + col1.b + col1.a) * 0.25f;
				fixed fog2 = (col2.r + col2.g + col2.b + col2.a) * 0.25f;
				fixed fog3 = (col3.r + col3.g + col3.b + col3.a) * 0.25f;
				fixed fog4 = (col4.r + col4.g + col4.b + col4.a) * 0.25f;
				fixed fog5 = (col5.r + col5.g + col5.b + col5.a) * 0.25f;
				fixed fog6 = (col6.r + col6.g + col6.b + col6.a) * 0.25f;
				fixed fog7 = (col7.r + col7.g + col7.b + col7.a) * 0.25f;
				fixed fog8 = (col8.r + col8.g + col8.b + col8.a) * 0.25f;
				fixed fog9 = (col9.r + col9.g + col9.b + col9.a) * 0.25f;

				float colorAlpha = (fog1* GaussianKernel[0] + fog2 * GaussianKernel[1] + fog3 * GaussianKernel[2] +
					fog4 * GaussianKernel[3] + fog5 * GaussianKernel[4] + fog6 * GaussianKernel[5] +
					fog7 * GaussianKernel[6] + fog8 * GaussianKernel[7] + fog9 * GaussianKernel[8]);
				fixed4 finColor = float4(_FogColor.rgb, colorAlpha * 0.3f + 0.2f);

				return finColor;
			}
			ENDCG
		}
	}
}
