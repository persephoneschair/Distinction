Shader "Custom/Bubbles"
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

#define RADIUS 0.2
#define SMOOTH_BOORDER 0.01
#define PI 3.1415925
#define PId2 1.5707963
#define PI2 6.2831855
#define NUMCIRCLES 86
#define TIME_SCALE 2.
            float2 random(float x, float y)
            {
                float2 K1 = float2(23.140692, 2.6651442);
                return abs(frac(float2(cos(x*K1.x), sin(y*K1.y+166.6))));
            }

            float2 generateRandomShift(float xSeed, float ySeed, float time, float2 ratio)
            {
                return (2.*random(ceil(time/PI2)+xSeed, ceil(time/PI2)+ySeed)-1.)*ratio;
            }

            float sinWave01(float time)
            {
                return 0.5+0.5*sin(time-PId2);
            }

            float calculateCircle(float2 uv, float r, float blur)
            {
                float dist = length(uv);
                return smoothstep(r+blur, r, dist);
            }

            float4 frag (v2f __vertex_output) : SV_Target
            {
                vertex_output = __vertex_output;
                float4 fragColor = 0;
                float2 fragCoord = vertex_output.uv * _Resolution;
                float2 uv = (2.*fragCoord.xy-iResolution.xy)/iResolution.y;
                float2 ratio = iResolution.xy/iResolution.y;
                float col;
                for (int i = 0;i<NUMCIRCLES; ++i)
                {
                    float timeShift = 2.*float(i);
                    float time = TIME_SCALE*(_Time.y+timeShift);
                    float2 shift = generateRandomShift(2.*float(i), float(i+56), time, ratio);
                    col += calculateCircle(uv-shift, RADIUS*sinWave01(time), SMOOTH_BOORDER)*(1.-sinWave01(time));
                }
                float3 sincol = 0.5+0.5*cos(_Time.y+uv.xyx+float3(0, 2, 4));
                fragColor = float4(sincol*clamp(col, 0., 1.), 1.);
                if (_GammaCorrect) fragColor.rgb = pow(fragColor.rgb, 2.2);
                return fragColor;
            }
            ENDCG
        }
    }
}
