Shader "Custom/Rainbow"
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

#define EPS float2(0.0001, 0.)
            static float time;
            float3 rotateX(float a, float3 v)
            {
                return float3(v.x, cos(a)*v.y+sin(a)*v.z, cos(a)*v.z-sin(a)*v.y);
            }

            float3 rotateY(float a, float3 v)
            {
                return float3(cos(a)*v.x+sin(a)*v.z, v.y, cos(a)*v.z-sin(a)*v.x);
            }

            float sphere(float3 p, float r)
            {
                return length(p)-r;
            }

            float plane(float3 p, float4 n)
            {
                return dot(p, n.xyz)-n.w;
            }

            float sceneDist(float3 p)
            {
                const int num_spheres = 32;
                float sd = 1000.;
                for (int i = 0;i<num_spheres; ++i)
                {
                    float r = 0.22*sqrt(float(i));
                    float3 p2 = rotateX(cos(time+float(i)*0.2)*0.15, p);
                    float cd = -sphere(p2+float3(0., -0.9, 0.), 1.3);
                    sd = min(sd, max(abs(sphere(p2, r)), cd)-0.001);
                }
                return sd;
            }

            float3 sceneNorm(float3 p)
            {
                float d = sceneDist(p);
                return normalize(float3(sceneDist(p+EPS.xyy)-d, sceneDist(p+EPS.yxy)-d, sceneDist(p+EPS.yyx)-d));
            }

            float3 col(float3 p)
            {
                float a = length(p)*20.;
                return ((float3)0.5)+0.5*cos(float3(a, a*1.1, a*1.2));
            }

            float ambientOcclusion(float3 p, float3 n)
            {
                const int steps = 4;
                const float delta = 0.5;
                float a = 0.;
                float weight = 3.;
                for (int i = 1;i<=steps; i++)
                {
                    float d = float(i)/float(steps)*delta;
                    a += weight*(d-sceneDist(p+n*d));
                    weight *= 0.5;
                }
                return clamp(1.-a, 0., 1.);
            }

            float4 frag (v2f __vertex_output) : SV_Target
            {
                vertex_output = __vertex_output;
                float4 fragColor = 0;
                float2 fragCoord = vertex_output.uv * _Resolution;
                float2 uv = fragCoord.xy/iResolution.xy;
                float2 t = uv*2.-((float2)1.);
                t.x *= iResolution.x/iResolution.y;
                time = _Time.y;
                float3 ro = float3(-0.4, sin(time*2.)*0.05, 0.7), rd = rotateX(1.1, rotateY(0.5, normalize(float3(t.xy, -0.8))));
                float f = 0.;
                float3 rp, n;
                for (int i = 0;i<100; ++i)
                {
                    rp = ro+rd*f;
                    float d = sceneDist(rp);
                    if (abs(d)<0.0001)
                        break;
                        
                    f += d;
                }
                n = sceneNorm(rp);
                float3 l = normalize(float3(1., 1., -1.));
                float ao = ambientOcclusion(rp, n);
                fragColor.rgb = ((float3)0.5+0.5*clamp(dot(n, l), 0., 1.))*col(rp)*lerp(0.1, 1., ao)*1.6;
                fragColor.a = 1.;
                if (_GammaCorrect) fragColor.rgb = pow(fragColor.rgb, 2.2);
                return fragColor;
            }
            ENDCG
        }
    }
}
