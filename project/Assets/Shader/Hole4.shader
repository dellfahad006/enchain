Shader "Hole4" {
    Properties {
        _EffectPower ("EffectPower", Range(0.5, 10)) = 2.4
        _Speed ("Speed", Range(-2, 2)) = 1.2
        _Spin ("Spin", Range(-20, 20)) = 8.4
        _PowerOfLines ("PowerOfLines", Range(1, 7)) = 1.5
        _Strensh ("Strensh", Range(0, 2)) = 0.41
        _Color ("Color", Color) = (0,0,0,1)
        _MultiLines ("MultiLines", Float) = 0.5
        _SpeenPower ("SpeenPower", Range(0.1, 1)) = 0.1
    }
    SubShader {
        Tags { "RenderType"="Transparent" "Queue"="Transparent" }
        Blend SrcAlpha OneMinusSrcAlpha
        Cull Off
        ZWrite Off

        Pass {
            CGPROGRAM
            #pragma vertex vert
            #pragma fragment frag
            #include "UnityCG.cginc"

            float _EffectPower;
            float _Speed;
            float _Spin;
            float _PowerOfLines;
            float _Strensh;
            fixed4 _Color;
            float _MultiLines;
            float _SpeenPower;

            struct appdata {
                float4 vertex : POSITION;
                float2 uv : TEXCOORD0;
            };

            struct v2f {
                float4 pos : SV_POSITION;
                float2 uv : TEXCOORD0;
            };

            v2f vert(appdata v) {
                v2f o;
                o.pos = UnityObjectToClipPos(v.vertex);
                o.uv = v.uv * 2.0 - 1.0;
                return o;
            }

            fixed4 frag(v2f i) : SV_Target {
                float r = length(i.uv);
                float a = atan2(i.uv.y, i.uv.x);
                float spiral = sin(a * round(max(1.0, _PowerOfLines * 4.0)) + pow(max(0.001, r), _SpeenPower) * _Spin - _Time.y * _Speed * 4.0);
                float lines = smoothstep(0.0, 1.0, spiral * _MultiLines + 0.5);
                float mask = saturate(1.0 - pow(r, _EffectPower));
                fixed3 col = lerp(_Color.rgb, fixed3(1.0, 1.0, 1.0), lines * _Strensh * mask);
                return fixed4(col, mask);
            }
            ENDCG
        }
    }
    Fallback "Diffuse"
}
