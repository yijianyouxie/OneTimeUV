Shader "TLStudio/Effect/Additive_UVFlow" {
    Properties {
        _MainTex ("MainTex", 2D) = "white" {}
        _MaskTex ("MaskTex", 2D) = "white" {}
        _AlphaAdjust ("AlphaAdjust", float ) = 3
		
		[Space(20)]
		[Header(Select U or V to flow.)]
		_UFlow ("UFlow", float ) = 0
		_VFlow ("VFlow", float ) = 0
		[Space(20)]
        _UVCurve ("UVCurve", float ) = 0
        _Cutoff ("Alpha cutoff", Range(0,1)) = 0.5
    }
    SubShader {
        Tags {
            "IgnoreProjector"="True"
            "Queue"="Transparent"
            "RenderType"="Transparent"
        }
        Pass {
            Name "FORWARD"
            Tags {
                "LightMode"="ForwardBase"
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
            uniform sampler2D _MaskTex; uniform float4 _MaskTex_ST;
            uniform float _AlphaAdjust;
            uniform float _UVCurve;
			uniform float _UFlow;
			uniform float _VFlow;
			uniform float _Cutoff;
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
            VertexOutput vert (VertexInput v) {
                VertexOutput o = (VertexOutput)0;
                o.uv0 = v.texcoord0;
                o.vertexColor = v.vertexColor;
                o.pos = mul(UNITY_MATRIX_MVP, v.vertex );
                return o;
            }
            float4 frag(VertexOutput i) : COLOR {
////// Lighting:
////// Emissive:
                float2 uvOff = (i.uv0+((_UVCurve*2.0)+(-1.0))*float2(-_UFlow,-_VFlow));
                float4 _MainTex_var = tex2D(_MainTex,TRANSFORM_TEX(uvOff, _MainTex));
                float3 emissive = (_MainTex_var.rgb*i.vertexColor.rgb);
                float3 finalColor = emissive;
                float4 _MaskTex_var = tex2D(_MaskTex,TRANSFORM_TEX(i.uv0, _MaskTex));
				fixed finalAlpha = ((_AlphaAdjust*_MainTex_var.a)*_MaskTex_var.r);
				clip(_MainTex_var.a - _Cutoff);
                return fixed4(finalColor, finalAlpha);
            }
            ENDCG
        }
    }
    FallBack "Diffuse"
}
