Shader "Parker/OceanShader"
{
    Properties
    {
        _DisplacementTex ("Texture", 2D) = "white" {}
        _SlopeTex ("Texture", 2D) = "white" {}
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
            #include "UnityStandardBRDF.cginc"

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
            sampler2D _SlopeTex;
            float3 _LightDir;

            v2f vert (appdata v)
            {
                v2f o;

                float3 worldPos = mul(unity_ObjectToWorld, v.vertex);
                float4 worldUV = float4(worldPos.xz, 0, 0);
                // o.uv = worldUV.xy;
                o.uv = v.uv;

                float3 viewVector = _WorldSpaceCameraPos.xyz - mul(unity_ObjectToWorld, v.vertex).xyz;
                float viewDist = length(viewVector);
                float lod_c = min(7.0 * 15.0 / viewDist, 1);

                float3 displacement = tex2Dlod(_DisplacementTex, worldUV / 100.0).rgb;
                // float3 displacement = tex2Dlod(_DisplacementTex, float4(v.uv,0,0)).rgb;
                v.vertex.xyz += mul(unity_WorldToObject, displacement.xyz);
                o.vertex = UnityObjectToClipPos(v.vertex);
                return o;
            }

            fixed4 frag (v2f i) : SV_Target
            {
                float2 slope = tex2D(_SlopeTex, i.uv / 100.0);
                float3 normal = normalize(float3(-slope.x, 1.0, -slope.y));
                
                normal = normalize(UnityObjectToWorldNormal(normalize(normal)));

                // return float4(normal, 1.0);

                // return float4(i.uv, 0.0, 1.0);

                // float ndotl = DotClamped(_LightDir, normal);

                // float4 col = float4(0,0,0.7, 1.0);
                
                // return col * ndotl + col * 0.3;
            }
            ENDCG
        }
    }
}
