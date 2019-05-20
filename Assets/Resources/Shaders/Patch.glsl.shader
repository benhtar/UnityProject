Shader "Custom/Patch" {
	Properties {
		_Color ("Color", Color) = (1,1,1,1)
		_MainTex ("Albedo (RGB)", 2D) = "white" {}
		_Glossiness ("Smoothness", Range(0,1)) = 0.5
		_Metallic ("Metallic", Range(0,1)) = 0.0
	}
	SubShader {
		Tags { "RenderType"="Opaque" }
		LOD 200
		Pass{
			CGPROGRAM
			#pragma vertex VertexProgram
			#pragma fragment FragmentProgram

			#include "UnityCG.cginc"
			float4 VertexProgram() : SV_POSITION {

			}

			float4 FragmentProgram( 
				float4 position : SV_POSITION
			) : SV_TARGET{

			}

			ENDCG
		}
	}
	FallBack "Diffuse"
}
