Shader "Custom/CRT"
{
    Properties
    {
        _MainTex ("iChannel0", 2D) = "white" {}
        _SecondTex ("iChannel1", 2D) = "white" {}
        _ThirdTex ("iChannel2", 2D) = "white" {}
        _FourthTex ("iChannel3", 2D) = "white" {}
        _Mouse ("Mouse", Vector) = (0.5, 0.5, 0.5, 0.5)
        [ToggleUI] _GammaCorrect ("Gamma Correction", Float) = 1
        _Resolution ("Resolution (Change if AA is bad)", Range(1, 1024)) = 1
    }
    SubShader
    {
        Pass
        {
            CGPROGRAM
            #pragma vertex vert
            #pragma fragment frag

            #include "UnityCG.cginc"

            struct appdata
            {
                float4 vertex : POSITION;
                float2 uv : TEXCOORD0;
            };

            struct v2f
            {
                float2 uv : TEXCOORD0;
                float4 vertex : SV_POSITION;
            };

            // Built-in properties
            sampler2D _MainTex;   float4 _MainTex_TexelSize;
            sampler2D _SecondTex; float4 _SecondTex_TexelSize;
            sampler2D _ThirdTex;  float4 _ThirdTex_TexelSize;
            sampler2D _FourthTex; float4 _FourthTex_TexelSize;
            float4 _Mouse;
            float _GammaCorrect;
            float _Resolution;

            // GLSL Compatability macros
            #define glsl_mod(x,y) (((x)-(y)*floor((x)/(y))))
            #define texelFetch(ch, uv, lod) tex2Dlod(ch, float4((uv).xy * ch##_TexelSize.xy + ch##_TexelSize.xy * 0.5, 0, lod))
            #define textureLod(ch, uv, lod) tex2Dlod(ch, float4(uv, 0, lod))
            #define iResolution float3(_Resolution, _Resolution, _Resolution)
            #define iFrame (floor(_Time.y / 60))
            #define iChannelTime float4(_Time.y, _Time.y, _Time.y, _Time.y)
            #define iDate float4(2020, 6, 18, 30)
            #define iSampleRate (44100)
            #define iChannelResolution float4x4(                      \
                _MainTex_TexelSize.z,   _MainTex_TexelSize.w,   0, 0, \
                _SecondTex_TexelSize.z, _SecondTex_TexelSize.w, 0, 0, \
                _ThirdTex_TexelSize.z,  _ThirdTex_TexelSize.w,  0, 0, \
                _FourthTex_TexelSize.z, _FourthTex_TexelSize.w, 0, 0)

            // Global access to uv data
            static v2f vertex_output;

            v2f vert (appdata v)
            {
                v2f o;
                o.vertex = UnityObjectToClipPos(v.vertex);
                o.uv =  v.uv;
                return o;
            }

            float2 crt_coords(float2 uv, float bend)
            {
                uv -= 0.5;
                uv *= 2.;
                uv.x *= 1.+pow(abs(uv.y)/bend, 2.);
                uv.y *= 1.+pow(abs(uv.x)/bend, 2.);
                uv /= 2.5;
                return uv+0.5;
            }

            float vignette(float2 uv, float size, float smoothness, float edgeRounding)
            {
                uv -= 0.5;
                uv *= size;
                float amount = sqrt(pow(abs(uv.x), edgeRounding)+pow(abs(uv.y), edgeRounding));
                amount = 1.-amount;
                return smoothstep(0., smoothness, amount);
            }

            float scanline(float2 uv, float lines, float speed)
            {
                return sin(uv.y*lines+_Time.y*speed);
            }

            float random(float2 uv)
            {
                return frac(sin(dot(uv, float2(15.5151, 42.2561)))*12341.142*sin(_Time.y*0.03));
            }

            float noise(float2 uv)
            {
                float2 i = floor(uv);
                float2 f = frac(uv);
                float a = random(i);
                float b = random(i+float2(1., 0.));
                float c = random(i+float2(0., 1.));
                float d = random(i+((float2)1.));
                float2 u = smoothstep(0., 1., f);
                return lerp(a, b, u.x)+(c-a)*u.y*(1.-u.x)+(d-b)*u.x*u.y;
            }

            float4 frag (v2f __vertex_output) : SV_Target
            {
                vertex_output = __vertex_output;
                float4 fragColor = 0;
                float2 fragCoord = vertex_output.uv * _Resolution;
                float2 uv = fragCoord/iResolution.xy;
                float2 crt_uv = crt_coords(uv, 4.);
                float s1 = scanline(uv, 200., -10.);
                float s2 = scanline(uv, 20., -3.);
                float4 col;
                col.r = tex2D(_MainTex, crt_uv+float2(0., 0.01)).r;
                col.g = tex2D(_MainTex, crt_uv).r;
                col.b = tex2D(_MainTex, crt_uv+float2(0., -0.01)).b;
                col.a = tex2D(_MainTex, crt_uv).a;
                col = lerp(col, ((float4)s1+s2), 0.05);
                fragColor = lerp(col, ((float4)noise(uv*75.)), 0.05)*vignette(uv, 1.9, 0.6, 8.);
                ;
                if (_GammaCorrect) fragColor.rgb = pow(fragColor.rgb, 2.2);
                return fragColor;
            }
            ENDCG
        }
    }
}
