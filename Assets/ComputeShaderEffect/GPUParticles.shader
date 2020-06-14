Shader "Ren/GPU Particle/Test"
{
    Properties
    {
        [HDR]
        _ColorHigh ("Color High Speed", Color) = (1, 0, 0, 1)
    }
    SubShader
    {

		 Tags { "RenderType" = "Opaque" "RenderPipeline" = "LightweightPipeline" "IgnoreProjector" = "True"}
        Pass
        {
            Tags { "LightMode" = "LightweightForward" }
            //ZTest Off
            CGPROGRAM
            
            #pragma vertex vert
            #pragma fragment frag
            
            #pragma multi_compile_fwdbase nolightmap nodirlightmap nodynlightmap novertexlight
            #pragma target 4.5
            //#pragma target es3.1
            
            #include "UnityCG.cginc"
            #include "UnityLightingCommon.cginc"
            #include "AutoLight.cginc"
            
            
            fixed4 _ColorHigh;
            
            struct Particle
            {
                float3 position;
                float scale;
                float height;
                float4 color;
                float4 color2;
                float translate;
            };
            
            StructuredBuffer<Particle> Particles;
            
            struct v2f
            {
                float4 pos: SV_POSITION;
                float2 uv_MainTex: TEXCOORD0;
                float3 diffuse: TEXCOORD1;
                float4 color: TEXCOORD2;
            };
            
            v2f vert(appdata_full v, uint instanceID: SV_InstanceID)
            {
                Particle data = Particles[instanceID];
                
                float vv = 1 / max(data.translate, 1);
                float3 localPosition = v.vertex * data.scale * float3(vv, data.height, vv);
                float3 worldPosition = data.position + localPosition + float3(0, data.translate, 0);
                float3 worldNormal = v.normal;
                
                float3 ndotl = saturate(dot(worldNormal, _WorldSpaceLightPos0.xyz));
                float3 diffuse = /*(ndotl * _LightColor0.rgb)*/1 + _LightColor0.rgb * 0.2;

				//_LightColor0
				//平行光顏色

                v2f o;
                o.pos = mul(UNITY_MATRIX_VP, float4(worldPosition, 1.0f));
                o.uv_MainTex = v.texcoord;
                o.diffuse = diffuse;
                o.color = Particles[instanceID].color;
                return o;
            }
            
            fixed4 frag(v2f i): SV_Target
            {
                float3 lighting = i.diffuse;
                fixed4 output = fixed4(i.color * lighting, 1);

                return output;
            }
            
            ENDCG
            
        }
    }
}