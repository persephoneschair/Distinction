Shader "Custom/IdealBubbles"
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

#define TOTAL_LAYERS 12.
#define hue(h) clamp(abs(frac(h+float4(3, 2, 1, 0)/3.)*6.-3.)-1., 0., 1.)
            float hash12(float2 p)
            {
                float3 p3 = frac(((float3)p.xyx)*0.1031);
                p3 += dot(p3, p3.yzx+33.33);
                return frac((p3.x+p3.y)*p3.z);
            }

            float4 frag (v2f __vertex_output) : SV_Target
            {
                vertex_output = __vertex_output;
                float4 fragColor = 0;
                float2 fragCoord = vertex_output.uv * _Resolution;
                float2 uv = fragCoord.xy/iResolution.x+((float2)100.);
                float3 col = ((float3)0.);
                for (float layer = 1.;layer<=TOTAL_LAYERS; layer++)
                {
                    float SIZE = (17.-layer)*0.5;
                    float2 luv = uv*SIZE;
                    float2 id = floor(luv);
                    luv = frac(luv)-0.5;
                    for (float y = -1.;y<=1.; y++)
                    {
                        for (float x = -1.;x<=1.; x++)
                        {
                            float2 rid = id-float2(x, y);
                            float rFactor1 = hash12(rid+542.*layer);
                            float rFactor2 = hash12(rid+159.*layer);
                            float t = _Time.y*5.5/(10.+layer*5./rFactor1)+100.*rFactor2;
                            float2 ruv = luv+float2(x, y)+float2(sin(_Time.y*0.1+t+rFactor1), sin(_Time.y*0.2+t*0.9+rFactor2));
                            float l = length(ruv);
                            float ld = length(ruv-((float2)0.075));
                            float SF = 1./min(iResolution.x, iResolution.y)*SIZE*(layer*2.);
                            float d = smoothstep(SF, -SF, l-(0.125+hash12(rid+700.)*0.25));
                            d *= step(hash12(rid*75.4+100.), 0.5+layer/TOTAL_LAYERS);
                            float colFactor = hash12(rid+500.);
                            float3 iCol = hue(colFactor).rgb*d*(0.7+smoothstep(0.1, 0.5, ld)*1.);
                            col = col+iCol*(0.25+(1.-layer/TOTAL_LAYERS)*0.2);
                        }
                    }
                }
                fragColor = float4(col, 1.);
                if (_GammaCorrect) fragColor.rgb = pow(fragColor.rgb, 2.2);
                return fragColor;
            }
            ENDCG
        }
    }
}
