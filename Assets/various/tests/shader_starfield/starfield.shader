// This is an attempt to make an infinite starfield using the vertex shader.
// The idea was: use the particle's screen coordinates to wrap around, with a modulo

Shader "Unlit/Particle_AdditiveSimple"
{
  Properties
  {
    _MainTex ("Texture", 2D) = "white" {}
    color1 ("foreground", Color) = (255,255,255,255)
    color2 ("background", Color) = (0,0,0,255)
    fogDensity ("fog density", Range (0,0.3)) = 1
  }
  SubShader
  {
    Tags { "RenderType"="Opaque" "Queue"="Transparent" }
    LOD 100
    ZWrite Off
    Blend SrcAlpha OneMinusSrcAlpha
    Pass
    {
      CGPROGRAM
      #pragma vertex vert
      #pragma fragment frag
      
      #include "UnityCG.cginc"
      
      struct appdata {
        float4 vertex : POSITION;
        float3 uv : TEXCOORD0;
      };

      struct v2f {
        float2 uv : TEXCOORD0;
        float4 vertex : SV_POSITION;
        float4 screen : TEXCOORD1;
      };

      sampler2D _MainTex;
      float4 _MainTex_ST;
      float4 color1;
      float4 color2;
      float fogDensity;
      
      v2f vert (appdata v) {
        v2f o;
        o.vertex = UnityObjectToClipPos(v.vertex);
        o.screen = ComputeScreenPos(o.vertex);

        o.uv = TRANSFORM_TEX(v.uv, _MainTex);
        return o;
      }
      
      fixed4 frag (v2f i) : SV_Target {
        float dist = i.vertex.z * _ProjectionParams.z;
        float fog = smoothstep(fogDensity, 0.0, dist);

        fixed4 col = tex2D(_MainTex, i.uv);
        col.rgb = color2.rgb * fog + col.rgb * (1-fog);

        return col;
      }
      ENDCG
    }
  }
}