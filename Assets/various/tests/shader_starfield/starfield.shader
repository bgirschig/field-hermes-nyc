// This is an attempt to make an infinite starfield using the vertex shader.
// The idea was: use the particle's screen coordinates to wrap around, with a modulo

Shader "Unlit/Particle_AdditiveSimple"
{
  Properties
  {
    _MainTex ("Texture", 2D) = "white" {}
    offsetX ("offsetX", Float) = 1
    offsetY ("offsetY", Float) = 1
    offsetZ ("offsetZ", Float) = 1
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
      // make fog work
      #pragma multi_compile_fog
      
      #include "UnityCG.cginc"
      struct appdata
      {
        float4 vertex : POSITION;
        float3 uv : TEXCOORD0;
        float3 center : TEXCOORD1;
      };
      struct v2f
      {
        float2 uv : TEXCOORD0;
        float3 center : TEXCOORD1;
        UNITY_FOG_COORDS(1)
        float4 vertex : SV_POSITION;
      };
      sampler2D _MainTex;
      float4 _MainTex_ST;
      float offsetX;
      float offsetY;
      float offsetZ;
      
      v2f vert (appdata v)
      {
        v2f o;
        o.vertex = UnityObjectToClipPos(v.vertex);
        float4 screen = ComputeScreenPos(o.vertex);
        // o.vertex.x -= v.center.x;
        // o.vertex.y += v.center.y;

        o.uv = TRANSFORM_TEX(v.uv, _MainTex);
        o.center = v.center;
        return o;
      }
      
      fixed4 frag (v2f i) : SV_Target
      {
        // sample the texture
        fixed4 col = tex2D(_MainTex, i.uv);
        // apply fog
        UNITY_APPLY_FOG(i.fogCoord, col);
        float val = i.center.z/20.0 + 0.5;
        return col;
      }
      ENDCG
    }
  }
}