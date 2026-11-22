Shader "Hole4" {
	Properties {
		_EffectPower ("EffectPower", Range(0.5, 10)) = 2.4
		_Speed ("Speed", Range(-2, 2)) = 1.2
		_Spin ("Spin", Range(-20, 20)) = 8.4
		_PowerOfLines ("PowerOfLines", Range(1, 7)) = 1.5
		_Strensh ("Strensh", Range(0, 2)) = 0.41
		_Color ("Color", Vector) = (0,0,0,1)
		_MultiLines ("MultiLines", Float) = 0.5
		_SpeenPower ("SpeenPower", Range(0.1, 1)) = 0.1
	}
	//DummyShaderTextExporter
	SubShader{
		Tags { "RenderType"="Opaque" }
		LOD 200

		Pass
		{
			HLSLPROGRAM
			#pragma vertex vert
			#pragma fragment frag

			float4x4 unity_ObjectToWorld;
			float4x4 unity_MatrixVP;

			struct Vertex_Stage_Input
			{
				float4 pos : POSITION;
			};

			struct Vertex_Stage_Output
			{
				float4 pos : SV_POSITION;
			};

			Vertex_Stage_Output vert(Vertex_Stage_Input input)
			{
				Vertex_Stage_Output output;
				output.pos = mul(unity_MatrixVP, mul(unity_ObjectToWorld, input.pos));
				return output;
			}

			float4 _Color;

			float4 frag(Vertex_Stage_Output input) : SV_TARGET
			{
				return _Color; // RGBA
			}

			ENDHLSL
		}
	}
	Fallback "Diffuse"
}