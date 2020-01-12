Shader "Hidden/post_processing"
{
    Properties
    {
        _MainTex ("Texture", 2D) = "white" {}
        width ("mask width", Float) = 1
        height ("mask height", Float) = 1
    }
    SubShader
    {
        // No culling or depth
        Cull Off ZWrite Off ZTest Always

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

            v2f vert (appdata v)
            {
                v2f o;
                o.vertex = UnityObjectToClipPos(v.vertex);
                o.uv = v.uv;
                return o;
            }

            sampler2D _MainTex;
            float width;
            float height;

            fixed4 frag (v2f i) : SV_Target
            {
                fixed4 col = tex2D(_MainTex, i.uv);

                float2 newUv = i.uv - 0.5;
                newUv.x /= width;
                newUv.y /= height;
                float distance = length(newUv);
                float mask = smoothstep(distance, distance+0.01, 0.5);

                float rimMask = smoothstep(distance+0.012, distance+0.01, 0.5);
                float4 rimColor = float4(0.0,0.0,1.0,1.0);

                // float luminance = sqrt( 0.299*pow(col.r, 2.0) + 0.587*pow(col.g,2.0) + 0.114*pow(col.b,2.0) );
                // col = step(luminance, 0.6);

                return col * (mask-rimMask) + rimColor * (rimMask - (1.0-mask));
            }
            ENDCG
        }
    }
}
