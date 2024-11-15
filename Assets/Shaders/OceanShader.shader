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
            float _ClipMap_Scale;
            // float3 _ClipMap_ViewPosition; // Search for where or how this is set? Maybe camera position
            float3 _CameraPosition;
            float _ClipMap_LevelHalfSize;

            float ModifiedManhattanDistance(float3 a, float3 b)
            {
                float3 v = a - b;
                return max(abs(v.x + v.z) + abs(v.x - v.z), abs(v.y)) * 0.5;
            }

            // float3 ClipMapVertex(float3 position, float2 uv){
            //     float3 morphOffset = float3(uv.x, 0, uv.y);
            //     position *= _ClipMap_Scale;
            //     float meshScale = positionOs.y; // ??? may need to be changed. Should be _ClipMap_Scale I think
            //     float step = meshScale * 4;
            //     float3 snappedViewerPos = float3(floor(ClipMap_ViewerPosition.x / step) * step, 0, floor(ClipMap_ViewerPosition.z / step) * step);
            //     float3 worldPos = float3(snappedViewerPos.x + position.x, 0, snappedViewerPos.z + position.z);
            //     float morphStart = ((_ClipMap_LevelHalfSize + 1) * 0.5 + 8) * meshScale;
	        //     float morphEnd = (_ClipMap_LevelHalfSize - 2) * meshScale;

            //     float t = saturate((ModifiedManhattanDistance(worldPos, _ClipMap_ViewerPosition) - morphStart) / (morphEnd - morphStart));
            //     worldPos += morphOffset * meshScale * t;
            //     return worldPos;
            // }

            float3 ClipMapVertex(float3 position, float2 uv){

                float3 morphOffset = float3(uv.x, 0, uv.y);
                position *= _ClipMap_Scale;
                float3 meshScale = position.y;
                float step = meshScale * 4;

                // Snapes mesh to grid points to avoid translation artifacts
                float3 snappedViewerPos = float3(floor(_CameraPosition.x / step) * step, 0, floor(_CameraPosition.z / step) * step);
                float3 worldPos = float3(snappedViewerPos.x + position.x, 0, snappedViewerPos.z + position.z);

                // Distance at which morphing begins. Can the 8 be changed? Look into this
                float morphStart = ((_ClipMap_LevelHalfSize + 1) * 0.5 + 8) * meshScale;
                // Distance at which morphing ends
	            float morphEnd = (_ClipMap_LevelHalfSize - 2) * meshScale;

                // Calcualtes the distance from the current world position to the camera. Accounts for height of camera
                float distance = ModifiedManhattanDistance(worldPos, _CameraPosition);

                // Value from 0 to 1 determing how much the world position will morph
                float t = saturate((distance - morphStart) / (morphEnd - morphStart));

                worldPos += morphOffset * meshScale * t;

                return worldPos;
            }

            v2f vert (appdata v)
            {
                v2f o;


                // float4 worldPos = mul(unity_ObjectToWorld, v.vertex);
                float4 worldPos = float4(ClipMapVertex(v.vertex.xyz, v.uv), 1.0);
                float4 worldUV = float4(worldPos.xz, 0, 0);
                o.uv = worldUV.xy;
                float height = tex2Dlod(_DisplacementTex, worldUV / 3000.0).r;
                // worldPos.y += (height * 100);



                float4 viewPos = mul(UNITY_MATRIX_V, worldPos);

                float4 clipPos = mul(UNITY_MATRIX_P, viewPos);
                o.vertex = clipPos;

                float3 viewVector = _WorldSpaceCameraPos.xyz - mul(unity_ObjectToWorld, v.vertex).xyz;
                float viewDist = length(viewVector);
                float lod_c = min(7.0 * 15.0 / viewDist, 1);


                // float3 displacement = tex2Dlod(_DisplacementTex, worldUV / 3000.0).rgb;
                // float3 displacement = tex2Dlod(_DisplacementTex, float4(v.uv,0,0)).rgb;
                // v.vertex.xyz += mul(unity_WorldToObject, displacement.xyz);
                // o.vertex = UnityObjectToClipPos(v.vertex);
                // o.vertex = ClipMapVertex()


                return o;
            }

            fixed4 frag (v2f i) : SV_Target
            {
                float2 slope = tex2D(_SlopeTex, i.uv / 100.0);
                float3 normal = normalize(float3(-slope.x, 1.0, -slope.y));
                
                normal = normalize(UnityObjectToWorldNormal(normalize(normal)));

                // return

                // return float4(normal, 1.0);

                // return float4(i.uv, 0.0, 1.0);
                return float4(tex2D(_DisplacementTex, i.uv / 3000.0));

                // float ndotl = DotClamped(_LightDir, normal);

                // float4 col = float4(0,0,0.7, 1.0);
                
                // return col * ndotl + col * 0.3;
            }
            ENDCG
        }
    }
}
