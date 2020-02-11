Shader "Hidden/post_processing"
{
    Properties
    {
        _MainTex ("Texture", 2D) = "white" {}
        foreground ("foreground", Color) = (255,255,255,255)
        background ("background", Color) = (0,0,0,255)
        maskColor ("mask", Color) = (255,0,0,255)
        width ("mask width", Float) = 1
        height ("mask height", Float) = 1
        
        offsetX ("offset X", Range(-1, 1)) = 0
        offsetY ("offset Y", Range(-1, 1)) = 0
        scaleX ("scale X", Range(0, 2)) = 1
        scaleY ("scale Y", Range(0, 2)) = 1

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
            float offsetX;
            float offsetY;
            float scaleX;
            float scaleY;
            float4 foreground;
            float4 background;
            float4 maskColor;

            fixed4 frag (v2f i) : SV_Target {
                // apply mapping offsets
                i.uv.x = i.uv.x / scaleX - offsetX;
                i.uv.y = i.uv.y / scaleY - offsetY;

                // Figure out what to mask
                float2 maskUv = i.uv - 0.5;
                maskUv.x /= width;
                maskUv.y /= height;
                float distance = length(maskUv);
                float mask = smoothstep(distance, distance+0.01, 0.5);

                // Eureka test
                // i.uv.x += sin((newUv.y+_Time*2) * 20)*0.1f;
                
                fixed4 col = tex2D(_MainTex, i.uv);
                float alpha = col.a;
                col = col * foreground + (1.0f-col) * background;

                col = col * mask + (1-mask) * maskColor;
                col.a = 1;
                return col;
            }
            ENDCG
        }
    }
}
