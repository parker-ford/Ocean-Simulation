Shader "Parker/OceanShader"
{
    Properties
    {
        _DisplacementTex ("Texture", 2D) = "white" {}
    }
    SubShader
    {
        Tags { "RenderType"="Opaque" }
        LOD 100

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

            sampler2D _DisplacementTex;
            float4 _DisplacementTex_ST;

            v2f vert (appdata v)
            {
                v2f o;
                float3 displacement = tex2Dlod(_DisplacementTex, float4(v.uv,0,0)).rgb;
                o.vertex = UnityObjectToClipPos(v.vertex);
                o.uv = v.uv;
                return o;
            }

            fixed4 frag (v2f i) : SV_Target
            {
                float4 col = float4(1,1,1,1);
                return col;
            }
            ENDCG
        }
    }
}
