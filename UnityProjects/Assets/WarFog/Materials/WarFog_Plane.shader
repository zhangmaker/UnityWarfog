Shader "WarFog/WarFog_Plane" {
	Properties {
		_MainTex ("Main Texture", 2D) = "white" {}
		_TransparentColor("Transparent Color", Color) = (0,0,0,0)
		_FogColor("Fog Color", Color) = (0,0,0,1)
		_BlurDistance("Blur Distance", Range(0.00001, 0.2)) = 0.01
		_FadeTime("Fade time", Range(0, 1)) = 0
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
			uniform float4 _TransparentColor;
			uniform float4 _FogColor;
			uniform float _BlurDistance;
			uniform float _FadeTime = 0;

			static float GaussianKernel[9] = {
				0.0947416f, 0.118318f, 0.0947416f,
				0.118318f, 0.147761, 0.118318f,
				0.0947416f, 0.118318f, 0.0947416f
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
				float4 col1 = tex2D(_MainTex, float2(IN.texcoord.x - _BlurDistance, IN.texcoord.y + _BlurDistance));
				float4 col2 = tex2D(_MainTex, float2(IN.texcoord.x, IN.texcoord.y + _BlurDistance));
				float4 col3 = tex2D(_MainTex, float2(IN.texcoord.x + _BlurDistance, IN.texcoord.y + _BlurDistance));
				float4 col4 = tex2D(_MainTex, float2(IN.texcoord.x - _BlurDistance, IN.texcoord.y));
				float4 col5 = tex2D(_MainTex, IN.texcoord);
				float4 col6 = tex2D(_MainTex, float2(IN.texcoord.x + _BlurDistance, IN.texcoord.y));
				float4 col7 = tex2D(_MainTex, float2(IN.texcoord.x - _BlurDistance, IN.texcoord.y - _BlurDistance));
				float4 col8 = tex2D(_MainTex, float2(IN.texcoord.x, IN.texcoord.y - _BlurDistance));
				float4 col9 = tex2D(_MainTex, float2(IN.texcoord.x + _BlurDistance, IN.texcoord.y - _BlurDistance));

				float4 fogColor = (col1* GaussianKernel[0] + col2 * GaussianKernel[1] + col3 * GaussianKernel[2] +
						col4 * GaussianKernel[3] + col5 * GaussianKernel[4] + col6 * GaussianKernel[5] +
						col7 * GaussianKernel[6] + col8 * GaussianKernel[7] + col9 * GaussianKernel[8]);
				float4 finColorOld = lerp(_TransparentColor, _FogColor, fogColor.r);
				float4 finColorNew = lerp(_TransparentColor, _FogColor, fogColor.g);
				float4 finColor = lerp(finColorOld, finColorNew, _FadeTime);

				return finColor;
			}
			ENDCG
		}
	}
}
