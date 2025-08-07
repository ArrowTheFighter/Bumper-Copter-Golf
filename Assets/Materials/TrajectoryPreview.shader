Shader "Custom/TrajectoryPreview"
{
    Properties
    {
        _MainTex ("Texture", 2D) = "white" {}
        _Color ("Color", Color) = (1,1,1,1)
        _LaunchAngle ("Launch Angle", Range(0, 90)) = 45.0
        _InitialSpeed ("Initial Speed", Float) = 10.0
        _StripLength ("Strip Length", Float) = 1.0
        _StripWidth ("Strip Width", Float) = 0.5
        _Alpha ("Alpha", Range(0, 1)) = 0.8
    }
    
    SubShader
    {
        Tags { "RenderType"="Transparent" "Queue"="Transparent" "RenderPipeline"="UniversalPipeline" }
        LOD 200
        
        Blend SrcAlpha OneMinusSrcAlpha
        ZWrite Off
        Cull Off
        
        Pass
        {
            Name "ForwardLit"
            Tags{"LightMode" = "UniversalForward"}
            
            HLSLPROGRAM
            #pragma vertex vert
            #pragma fragment frag
            #pragma multi_compile_fog
            
            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"
            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Lighting.hlsl"
            
            struct appdata
            {
                float4 vertex : POSITION;
                float2 uv : TEXCOORD0;
                float3 normal : NORMAL;
                UNITY_VERTEX_INPUT_INSTANCE_ID
            };
            
            struct v2f
            {
                float2 uv : TEXCOORD0;
                float fogCoord : TEXCOORD1;
                float4 vertex : SV_POSITION;
                float3 worldPos : TEXCOORD2;
                float3 normal : TEXCOORD3;
                UNITY_VERTEX_INPUT_INSTANCE_ID
                UNITY_VERTEX_OUTPUT_STEREO
            };
            
            TEXTURE2D(_MainTex);
            SAMPLER(sampler_MainTex);
            
            CBUFFER_START(UnityPerMaterial)
                float4 _MainTex_ST;
                float4 _Color;
                float _LaunchAngle;
                float _InitialSpeed;
                float _StripLength;
                float _StripWidth;
                float _Alpha;
            CBUFFER_END
            
            // Physics constants
            #define GRAVITY 9.81
            
            float CalculateTrajectoryHeight(float z, float speed, float angle)
            {
                // Convert angle to radians
                float angleRad = radians(angle);
                
                // Avoid division by zero for vertical shots
                if (angle >= 89.0)
                    return 0.0;
                
                // Calculate initial velocity components
                float vz = speed * cos(angleRad); // horizontal velocity (along z)
                float vy = -speed * sin(angleRad); // vertical velocity
                
                // Avoid division by zero
                if (vz <= 0.001)
                    return 0.0;
                
                // Calculate time at this z position
                float t = z / vz;
                
                // Ballistic trajectory equation: y = vy*t - (1/2)*g*t²
                float y = vy * t - 0.5 * GRAVITY * t * t;
                
                return y;
            }
            
            v2f vert (appdata v)
            {
                v2f o;
                UNITY_SETUP_INSTANCE_ID(v);
                UNITY_TRANSFER_INSTANCE_ID(v, o);
                UNITY_INITIALIZE_VERTEX_OUTPUT_STEREO(o);
                
                // Work in object space first
                float3 objectPos = v.vertex.xyz;
                
                // Get the Z position along the trajectory (strip extends along Z-axis)
                float z = objectPos.z;
                
                // Scale z by strip length
                float trajectoryZ = z * _StripLength;
                
                // Calculate height using ballistic trajectory with speed and angle
                float trajectoryY = CalculateTrajectoryHeight(trajectoryZ, _InitialSpeed, _LaunchAngle);
                
                // Modify the object space position
                objectPos.z = trajectoryZ;  // Extend along Z
                objectPos.y += trajectoryY; // Add trajectory height
                objectPos.x *= _StripWidth; // Scale width
                
                // Transform to world space
                float3 worldPos = TransformObjectToWorld(objectPos);
                
                o.worldPos = worldPos;
                o.vertex = TransformWorldToHClip(worldPos);
                o.uv = TRANSFORM_TEX(v.uv, _MainTex);
                o.normal = TransformObjectToWorldNormal(v.normal);
                o.fogCoord = ComputeFogFactor(o.vertex.z);
                
                return o;
            }
            
            half4 frag (v2f i) : SV_Target
            {
                UNITY_SETUP_INSTANCE_ID(i);
                
                half4 col = SAMPLE_TEXTURE2D(_MainTex, sampler_MainTex, i.uv) * _Color;
                
                // Add some gradient based on trajectory progress
                float gradient = 1.0 - i.uv.y; // Fade out towards the end (UV.y represents progress along Z)
                col.rgb *= gradient * 0.5 + 0.5; // Subtle gradient effect
                
                // Apply alpha
                col.a *= _Alpha;
                
                // Apply fog
                col.rgb = MixFog(col.rgb, i.fogCoord);
                
                return col;
            }
            ENDHLSL
        }
    }
    
    FallBack "Universal Render Pipeline/Unlit"
}
