Shader "MaidChan/Actor"
{
    Properties
    {
        _Color ("Main Color", Color) = (1, 1, 1, 1)
        _ShadowColor ("Shadow Color", Color) = (0.8, 0.8, 1, 1)
        
        _MainTex ("Diffuse", 2D) = "white" { }
        _FalloffSampler ("Falloff Control", 2D) = "white" { }
        _RimLightSampler ("RimLight Control", 2D) = "white" { }
    }
    
    SubShader
    {
		Tags { "RenderType" = "Opaque" "RenderPipeline" = "LightweightPipeline" "IgnoreProjector" = "True"}
        Pass
        {
			Tags { "RenderType" = "Opaque" "Queue" = "Geometry" "LightMode" = "LightweightForward" }

            Cull Back
            ZTest LEqual

            HLSLPROGRAM
            
            #pragma target 4.5
            #pragma vertex vert
            #pragma fragment frag
            #include "UnityCG.cginc"
            #include "AutoLight.cginc"
            
			// global
			float _threshold;
            int _cull; 

            // Material parameters
            float4 _Color;
            float4 _ShadowColor;
            float4 _LightColor0;
            float4 _MainTex_ST;
            
            sampler2D _MainTex;
            sampler2D _FalloffSampler;
            sampler2D _RimLightSampler;
            
            #define FALLOFF_POWER 1.0
            
            struct v2f
            {
                float4 pos: SV_POSITION;
                float3 normal: TEXCOORD0;
                float2 uv: TEXCOORD1;
                float3 eyeDir: TEXCOORD2;
                float3 lightDir: TEXCOORD3;
				float3 worldPos: TEXCOORD4;
            };

            v2f vert(appdata_base v)
            {
                v2f o;
                o.pos = UnityObjectToClipPos(v.vertex);
                o.uv = TRANSFORM_TEX(v.texcoord.xy, _MainTex);
                o.normal = normalize(mul(unity_ObjectToWorld, float4(v.normal, 0)).xyz);
                
                // Eye direction vector
                o.worldPos = mul(unity_ObjectToWorld, v.vertex);
                o.eyeDir = normalize(_WorldSpaceCameraPos - o.worldPos);
                
                o.lightDir = WorldSpaceLightDir(v.vertex);
                return o;
            }
            void ScreenDoorTransparency(float alpha, float4 screenPos, float2 scaleScreenSize = float2(1, 1))
            {
                const float4x4 thresholdMatrix =
                {
                    1, 9, 3, 11,
                    13, 5, 15, 7,
                    4, 12, 2, 10,
                    16, 8, 14, 6
                };
                float2 pixelPos = screenPos.xy * scaleScreenSize;
                float threshold = thresholdMatrix[pixelPos.x % 4][pixelPos.y % 4] / 17;
                clip(alpha - threshold);
            }
            
            float4 frag(v2f i): COLOR
            {

                if (_cull == 1)
                {
                    float a0 = i.worldPos.y - _threshold + 1;
                    float a1 = max(a0, 0);
                    float thresholdCull = lerp(1, 0,max(1 - min(a1, 1) * 1.5,0));
                    ScreenDoorTransparency(thresholdCull, i.pos);
                }

                float4 diffuseColor = tex2D(_MainTex, i.uv);
                
                // Falloff. Convert the angle between the normal and the camera direction into a lookup for the gradient
                float normalDotEye = dot(i.normal, i.eyeDir);
				float falloffU = clamp(1 - abs(normalDotEye), 0.02, 0.98);
				float4 falloffSamplerColor = FALLOFF_POWER * tex2D(_FalloffSampler, float2(falloffU, 0.25f));
				float3 combinedColor = lerp(diffuseColor.rgb, falloffSamplerColor.rgb * diffuseColor.rgb, falloffSamplerColor.a);
                
                // Rimlight
				float rimlightDot = saturate(0.5 * (dot(i.normal, i.lightDir) + 1.0));
                falloffU = saturate(rimlightDot * falloffU);
                falloffU = tex2D(_RimLightSampler, float2(falloffU, 0.25f)).r;
				float3 lightColor = diffuseColor.rgb * 0.5;
                combinedColor += falloffU * lightColor;
                
				float4 finalColor = float4(combinedColor, diffuseColor.a) * _Color;

                return finalColor;
            }
            ENDHLSL
        }
    }
}
