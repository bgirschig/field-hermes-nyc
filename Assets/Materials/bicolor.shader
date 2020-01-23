﻿Shader "Unlit/bicolor"
{
    Properties
    {
        _MainTex ("Texture", 2D) = "white" {}
        color1 ("Color 1", Color) = (255,255,255,255)
        color2 ("Color 2", Color) = (0,0,0,255)
        fogDensity ("fog density", Range (0,5)) = 1
    }
    SubShader
    {
        Tags { "RenderType"="Opaque" "Queue"="Transparent"}
        LOD 100
        ZWrite Off
        Blend SrcAlpha OneMinusSrcAlpha

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
                float4 screen : TEXCOORD1;
            };

            sampler2D _MainTex;
            float4 _MainTex_ST;
            float4 color1;
            float4 color2;
            float fogDensity;

            v2f vert (appdata v)
            {
                v2f o;
                o.vertex = UnityObjectToClipPos(v.vertex);
                o.uv = TRANSFORM_TEX(v.uv, _MainTex);
                o.screen = ComputeScreenPos(o.vertex);
                return o;
            }

            fixed4 frag (v2f i) : SV_Target
            {
                // sample the texture
                fixed4 col = tex2D(_MainTex, i.uv);
                float a = col.a;

                float dist = i.vertex.z * _ProjectionParams.z;
                float fog = smoothstep(.05, 0.0, dist);

                float luminance = 0.299*col.r + 0.587*col.g + 0.114*col.b;
                col = luminance * color1 + (1.0-luminance) * color2;

                col = color2 * fog + col * (1-fog);
                // return float4(col.rgb, 1);

                col.a = a;
                return col;
            }
            ENDCG
        }
    }
}
