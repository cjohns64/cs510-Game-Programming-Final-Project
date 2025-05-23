Shader "Custom/ConstantWidthLine"
{
    Properties
    {
        _Color     ("Line Color", Color) = (1,1,1,1)
        _LineWidth ("Line Width (px)", Float) = 4.0
    }
    SubShader
    {
        Tags { "RenderType"="Opaque" }
        Pass
        {
            CGPROGRAM
            #pragma target 4.0
            #pragma vertex vert
            #pragma geometry geom
            #pragma fragment frag

            // Two variants: default (local), WORLDSPACE_ON (world)
            #pragma multi_compile __ WORLDSPACE_ON

            #include "UnityCG.cginc"

            struct appdata
            {
                float3 vertex : POSITION;
                float3 next   : TEXCOORD0;
            };

            struct v2g
            {
                float4 pos      : SV_POSITION;
                float4 posNext  : TEXCOORD0;
            };

            struct g2f
            {
                float4 pos      : SV_POSITION;
                float2 uv       : TEXCOORD0;
            };

            float4 _Color;
            float _LineWidth;

            v2g vert(appdata v)
            {
                v2g o;

                #ifdef WORLDSPACE_ON 
                    o.pos     = UnityObjectToClipPos(v.vertex);
                    o.posNext = UnityObjectToClipPos(v.next);
                #else
                    o.pos = mul(UNITY_MATRIX_VP, float4(v.vertex, 1));
                    o.posNext = mul(UNITY_MATRIX_VP, float4(v.next, 1));
                #endif

                return o;
            }

            [maxvertexcount(4)]
            void geom(line v2g input[2], inout TriangleStream<g2f> triStream)
            {
                float4 p0 = input[0].pos;
                float4 p1 = input[1].pos;

                // direction and perpendicular in screen space
                float2 dir  = normalize((p1.xy/p1.w)-(p0.xy/p0.w));
                float2 perp = float2(-dir.y, dir.x);

                // pixel to clip-space factor
                float2 pixel = (_LineWidth * 0.5) / _ScreenParams.xy;
                float2 off0  = perp * pixel * p0.w;
                float2 off1  = perp * pixel * p1.w;

                g2f o;
                // four corners
                o.pos = float4(p0.xy + off0, p0.z, p0.w); o.uv = float2(0,0); triStream.Append(o);
                o.pos = float4(p0.xy - off0, p0.z, p0.w); o.uv = float2(0,1); triStream.Append(o);
                o.pos = float4(p1.xy + off1, p1.z, p1.w); o.uv = float2(1,0); triStream.Append(o);
                o.pos = float4(p1.xy - off1, p1.z, p1.w); o.uv = float2(1,1); triStream.Append(o);
            }

            fixed4 frag(g2f i) : SV_Target
            {
                return _Color;
            }
            ENDCG
        }
    }
    FallBack Off
}
