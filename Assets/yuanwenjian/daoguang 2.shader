// Shader created with Shader Forge v1.37 
// Shader Forge (c) Neat Corporation / Joachim Holmer - http://www.acegikmo.com/shaderforge/
// Note: Manually altering this data may prevent you from opening it in Shader Forge
/*SF_DATA;ver:1.37;sub:START;pass:START;ps:flbk:,iptp:0,cusa:False,bamd:0,cgin:,lico:1,lgpr:1,limd:1,spmd:1,trmd:0,grmd:0,uamb:True,mssp:True,bkdf:False,hqlp:False,rprd:False,enco:False,rmgx:True,imps:True,rpth:0,vtps:0,hqsc:True,nrmq:1,nrsp:0,vomd:0,spxs:False,tesm:0,olmd:1,culm:2,bsrc:3,bdst:7,dpts:2,wrdp:False,dith:0,atcv:False,rfrpo:True,rfrpn:Refraction,coma:15,ufog:True,aust:True,igpj:True,qofs:0,qpre:3,rntp:2,fgom:False,fgoc:False,fgod:False,fgor:False,fgmd:0,fgcr:0.5,fgcg:0.5,fgcb:0.5,fgca:1,fgde:0.01,fgrn:0,fgrf:300,stcl:False,stva:128,stmr:255,stmw:255,stcp:6,stps:0,stfa:0,stfz:0,ofsf:0,ofsu:0,f2p0:False,fnsp:False,fnfb:False,fsmp:False;n:type:ShaderForge.SFN_Final,id:1,x:34273,y:32687,varname:node_1,prsc:2|emission-107-OUT,alpha-14-OUT;n:type:ShaderForge.SFN_Tex2d,id:4,x:33675,y:32766,ptovrint:False,ptlb:diffuse,ptin:_diffuse,varname:_diffuse,prsc:2,glob:False,taghide:False,taghdr:False,tagprd:False,tagnsco:False,tagnrm:False,ntxv:0,isnm:False|UVIN-23-UVOUT;n:type:ShaderForge.SFN_Tex2d,id:5,x:33877,y:33033,ptovrint:False,ptlb:alpha,ptin:_alpha,varname:_alpha,prsc:2,glob:False,taghide:False,taghdr:False,tagprd:False,tagnsco:False,tagnrm:False,ntxv:0,isnm:False;n:type:ShaderForge.SFN_Multiply,id:14,x:34090,y:32963,varname:node_14,prsc:2|A-108-OUT,B-5-R;n:type:ShaderForge.SFN_Panner,id:23,x:33502,y:32771,varname:node_23,prsc:2,spu:0,spv:-1|UVIN-54-UVOUT,DIST-191-OUT;n:type:ShaderForge.SFN_TexCoord,id:54,x:33329,y:32771,varname:node_54,prsc:2,uv:0,uaff:False;n:type:ShaderForge.SFN_VertexColor,id:101,x:33124,y:32709,varname:node_101,prsc:2;n:type:ShaderForge.SFN_Multiply,id:107,x:33916,y:32684,varname:node_107,prsc:2|A-4-RGB,B-101-RGB;n:type:ShaderForge.SFN_Multiply,id:108,x:33874,y:32883,varname:node_108,prsc:2|A-112-OUT,B-4-A;n:type:ShaderForge.SFN_ValueProperty,id:112,x:33676,y:32993,ptovrint:False,ptlb:QD,ptin:_QD,varname:_QD,prsc:2,glob:False,taghide:False,taghdr:False,tagprd:False,tagnsco:False,tagnrm:False,v1:20;n:type:ShaderForge.SFN_Add,id:191,x:33331,y:32956,varname:node_191,prsc:2|A-258-OUT,B-192-OUT;n:type:ShaderForge.SFN_Vector1,id:192,x:33128,y:33136,varname:node_192,prsc:2,v1:-1;n:type:ShaderForge.SFN_Multiply,id:258,x:33126,y:32956,varname:node_258,prsc:2|A-9071-OUT,B-259-OUT;n:type:ShaderForge.SFN_Vector1,id:259,x:32959,y:33097,varname:node_259,prsc:2,v1:2;n:type:ShaderForge.SFN_ValueProperty,id:5188,x:33878,y:33259,ptovrint:False,ptlb:alpha2,ptin:_alpha2,varname:_alpha2,prsc:2,glob:False,taghide:False,taghdr:False,tagprd:False,tagnsco:False,tagnrm:False,v1:1;n:type:ShaderForge.SFN_ValueProperty,id:9071,x:32946,y:32925,ptovrint:False,ptlb:lifealpha,ptin:_lifealpha,varname:_lifealpha,prsc:2,glob:False,taghide:False,taghdr:False,tagprd:False,tagnsco:False,tagnrm:False,v1:0;proporder:4-5-112-5188-9071;pass:END;sub:END;*/

Shader "Shader Forge/daoguang" {
    Properties {
        _diffuse ("diffuse", 2D) = "white" {}
        _alpha ("alpha", 2D) = "white" {}
        _QD ("QD", Float ) = 20
        _alpha2 ("alpha2", Float ) = 1
        _lifealpha ("lifealpha", Float ) = 0
        [HideInInspector]_Cutoff ("Alpha cutoff", Range(0,1)) = 0.5
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
            Blend SrcAlpha OneMinusSrcAlpha
            Cull Off
            ZWrite Off
            
            CGPROGRAM
            #pragma vertex vert
            #pragma fragment frag
            #define UNITY_PASS_FORWARDBASE
            #include "UnityCG.cginc"
            #pragma multi_compile_fwdbase
            #pragma multi_compile_fog
            #pragma only_renderers d3d9 d3d11 glcore gles gles3 metal xboxone ps4 psp2 n3ds wiiu 
            #pragma target 3.0
            uniform sampler2D _diffuse; uniform float4 _diffuse_ST;
            uniform sampler2D _alpha; uniform float4 _alpha_ST;
            uniform float _QD;
            uniform float _lifealpha;
            struct VertexInput {
                float4 vertex : POSITION;
                float2 texcoord0 : TEXCOORD0;
                float4 vertexColor : COLOR;
            };
            struct VertexOutput {
                float4 pos : SV_POSITION;
                float2 uv0 : TEXCOORD0;
                float4 vertexColor : COLOR;
                UNITY_FOG_COORDS(1)
            };
            VertexOutput vert (VertexInput v) {
                VertexOutput o = (VertexOutput)0;
                o.uv0 = v.texcoord0;
                o.vertexColor = v.vertexColor;
                o.pos = mul(UNITY_MATRIX_MVP, v.vertex );
                UNITY_TRANSFER_FOG(o,o.pos);
                return o;
            }
            float4 frag(VertexOutput i, float facing : VFACE) : COLOR {
                float isFrontFace = ( facing >= 0 ? 1 : 0 );
                float faceSign = ( facing >= 0 ? 1 : -1 );
////// Lighting:
////// Emissive:
                float2 node_23 = (i.uv0+((_lifealpha*2.0)+(-1.0))*float2(0,-1));
                float4 _diffuse_var = tex2D(_diffuse,TRANSFORM_TEX(node_23, _diffuse));
                float3 emissive = (_diffuse_var.rgb*i.vertexColor.rgb);
                float3 finalColor = emissive;
                float4 _alpha_var = tex2D(_alpha,TRANSFORM_TEX(i.uv0, _alpha));
                fixed4 finalRGBA = fixed4(finalColor,((_QD*_diffuse_var.a)*_alpha_var.r));
                UNITY_APPLY_FOG(i.fogCoord, finalRGBA);
                return finalRGBA;
            }
            ENDCG
        }
        Pass {
            Name "ShadowCaster"
            Tags {
                "LightMode"="ShadowCaster"
            }
            Offset 1, 1
            Cull Off
            
            CGPROGRAM
            #pragma vertex vert
            #pragma fragment frag
            #define UNITY_PASS_SHADOWCASTER
            #include "UnityCG.cginc"
            #include "Lighting.cginc"
            #pragma fragmentoption ARB_precision_hint_fastest
            #pragma multi_compile_shadowcaster
            #pragma multi_compile_fog
            #pragma only_renderers d3d9 d3d11 glcore gles gles3 metal xboxone ps4 psp2 n3ds wiiu 
            #pragma target 3.0
            struct VertexInput {
                float4 vertex : POSITION;
            };
            struct VertexOutput {
                V2F_SHADOW_CASTER;
            };
            VertexOutput vert (VertexInput v) {
                VertexOutput o = (VertexOutput)0;
                o.pos = mul(UNITY_MATRIX_MVP, v.vertex );
                TRANSFER_SHADOW_CASTER(o)
                return o;
            }
            float4 frag(VertexOutput i, float facing : VFACE) : COLOR {
                float isFrontFace = ( facing >= 0 ? 1 : 0 );
                float faceSign = ( facing >= 0 ? 1 : -1 );
                SHADOW_CASTER_FRAGMENT(i)
            }
            ENDCG
        }
    }
    FallBack "Diffuse"
    CustomEditor "ShaderForgeMaterialInspector"
}
