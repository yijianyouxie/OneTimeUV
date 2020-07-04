﻿Shader "TLStudio/Effect/MultiUVFlow-Add"
{
	Properties{
		[Header(Tips begain)]
		[Header(Add UVFlowController Componet to use.)]
		[Header(Tips end)]
		[Space(20)]
		_MainTex("MainTex", 2D) = "white" {}

		_GlossTex("相乘GlossTex", 2D) = "white" {}

		_AdjustTex("相加AdjustTex", 2D) = "black" {}

		[HideInInspector]_AlphaCutoff("Alpha cutoff", Range(0,1)) = 0
		_DissoveTex("溶解DissoveTex", 2D) = "white" {}

		_MaskTex("遮罩MaskTex", 2D) = "white" {}
	}
	SubShader{
		Tags{
			"IgnoreProjector" = "True"
			"Queue" = "Transparent"
			"RenderType" = "Transparent"
		}
		Pass{
			Name "FORWARD"
			Tags{
				"LightMode" = "ForwardBase"
			}
			Blend SrcAlpha One
			Cull Off
			Lighting Off
			ZWrite Off
			LOD 150

			CGPROGRAM
			#pragma vertex vert
			#pragma fragment frag
			#include "UnityCG.cginc"
			#pragma target 3.0
			uniform sampler2D _MainTex; uniform float4 _MainTex_ST;
			uniform sampler2D _GlossTex; uniform float4 _GlossTex_ST;
			uniform sampler2D _AdjustTex; uniform float4 _AdjustTex_ST;
			uniform sampler2D _MaskTex; uniform float4 _MaskTex_ST;
			uniform float _AlphaCutoff;
			uniform sampler2D _DissoveTex; uniform float4 _DissoveTex_ST;
			struct VertexInput {
				float4 vertex : POSITION;
				float2 texcoord0 : TEXCOORD0;
				float4 vertexColor : COLOR;
			};
			struct VertexOutput {
				float4 pos : SV_POSITION;
				float2 uv0 : TEXCOORD0;
				float4 vertexColor : COLOR;
			};
			VertexOutput vert(VertexInput v) {
				VertexOutput o = (VertexOutput)0;
				o.uv0 = v.texcoord0;
				o.vertexColor = v.vertexColor;
				o.pos = mul(UNITY_MATRIX_MVP, v.vertex);
				return o;
			}
			float4 frag(VertexOutput i) : COLOR{
				//MainTex
				float4 _MainTex_var = tex2D(_MainTex, TRANSFORM_TEX(i.uv0, _MainTex));
				float3 emissive = (_MainTex_var.rgb*i.vertexColor.rgb);
				float3 finalColor = emissive;
				fixed finalAlpha = _MainTex_var.a;

				//_GlossTex
				float4 _GlossTex_var = tex2D(_GlossTex, TRANSFORM_TEX(i.uv0, _GlossTex));
				float3 glossFinalCol = _GlossTex_var.a*_GlossTex_var.rgb;
				finalColor *= glossFinalCol;
				//finalAlpha = (finalAlpha + (1 - finalAlpha)*_GlossTex_var.a);

				//_AdjustTex
				float4 _AdjustTex_var = tex2D(_AdjustTex, TRANSFORM_TEX(i.uv0, _AdjustTex));
				float3 finalAdjustCol = _AdjustTex_var.rgb*_AdjustTex_var.a;
				finalColor += finalAdjustCol;
				//finalAlpha = (finalAlpha + (1 - finalAlpha)*_AdjustTex_var.a);

				//_MaskTex
				float4 _MaskTex_var = tex2D(_MaskTex, TRANSFORM_TEX(i.uv0, _MaskTex));
				finalAlpha = (finalAlpha * _MaskTex_var.r);

				//_DissoveTex
				float4 _DissoveTex_var = tex2D(_DissoveTex, TRANSFORM_TEX(i.uv0, _DissoveTex));
				clip(_DissoveTex_var.a - _AlphaCutoff);

				return fixed4(finalColor.rgb, finalAlpha);
			}
			ENDCG
		}
	}
	FallBack "Diffuse"
}