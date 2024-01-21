Shader "Custom/BubblesAlt"
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

            static const float density = 12.;
            static const int w = 2, h = 2;
            static const bool polar = false;
            static const bool accel = true;
            static const float wobble = 0.25;
            float nrand(float2 n)
            {
                return frac(sin(dot(n.xy, float2(12.9898, 78.233)))*43758.547);
            }

            float2 nrand2(float2 n)
            {
                return float2(nrand(n*float2(-3.2145, 1.2345)), nrand(n*float2(-5.4321, 3.4521)));
            }

            float nrand(int2 n)
            {
                return frac(sin(dot(((float2)n.xy), float2(12.9898, 78.233)))*43758.547);
            }

            float2 nrand2(int2 n)
            {
                return float2(nrand(((float2)n)*float2(-3.2145, 1.2345)), nrand(((float2)n)*float2(-5.4321, 3.4521)));
            }

            float4 point_(int3 c, float t)
            {
                const int period = 256;
                float speed = 0.2+1.*nrand(c.xz);
                float st = glsl_mod(speed*t, float(period));
                c.y -= int(floor(st));
                if (c.y<0)
                    c.y += period;
                    
                float4 p = float4(nrand2(c.xy), speed, nrand(c.xy+c.yz));
                p.y += frac(st)-0.5;
                return p;
            }

            float3 _pal(in float t, in float3 a, in float3 b, in float3 c, in float3 d)
            {
                return a+b*cos(6.28318*(c*t+d));
            }

            float4 pal(float i)
            {
                return float4(_pal(i, float3(0.5, 0.5, 0.5), float3(0.5, 0.5, 0.5), float3(1., 1., 1.), float3(0., 0.33, 0.67)), 1.);
            }

            float2 xfrm(float2 uv)
            {
                if (polar)
                {
                    uv -= 0.5;
                    uv = float2(atan2(uv.y, uv.x), distance(uv, ((float2)0.)));
                    uv.x /= 3.1415927;
                }
                
                if (accel)
                {
                    uv.y = pow(uv.y+0.03, 0.25);
                }
                
                return uv;
            }

            float2 ixfrm(float2 uv)
            {
                if (accel)
                {
                    uv.y = pow(uv.y, 1./0.25)-0.03;
                }
                
                if (polar)
                {
                    uv.x *= 3.1415927;
                    uv = float2(cos(uv.x), sin(uv.x))*uv.y;
                    uv += 0.5;
                }
                
                return uv;
            }

            float4 frag (v2f __vertex_output) : SV_Target
            {
                vertex_output = __vertex_output;
                float4 fragColor = 0;
                float2 fragCoord = vertex_output.uv * _Resolution;
                float2 uv = fragCoord/iResolution.y;
                fragColor = ((float4)0);
                float4 nearest = ((float4)1.);
                float nearest_d = 9999.;
                int2 iuv = ((int2)floor(xfrm(uv)*density));
                for (int y = -h;y<=h; ++y)
                {
                    if (iuv.y+y<0)
                        continue;
                        
                    for (int x = -w;x<=w; ++x)
                    {
                        for (int z = 0;z<2; ++z)
                        {
                            int3 ic = int3(iuv+int2(x, y), z);
                            float4 p = point_(ic, _Time.y);
                            p.xy += ((float2)ic);
                            p.x += sin((p.y*0.5+p.w)*2.*3.1415927)*wobble;
                            p.xy = ixfrm(p.xy/density);
                            p.z = lerp(p.z, 1., 0.25);
                            float d = distance(uv, p.xy);
#if 1
                            if (nearest_d*p.z>=d*nearest.z)
#else
                                
                            if (nearest_d>=d)
#endif
                                
                            {
                                nearest = p;
                                nearest_d = d;
                            }
                        }
                    }
                }
                if (_Mouse.z>0.)
                {
                    fragColor = pal(nearest.w);
                }
                else 
                {
                    fragColor = pal(nearest.w)*nearest.z/(nearest_d*150.);
                    fragColor.rgb = clamp(fragColor.rgb-0.1, 0., 1.);
                }
                if (_GammaCorrect) fragColor.rgb = pow(fragColor.rgb, 2.2);
                return fragColor;
            }
            ENDCG
        }
    }
}
