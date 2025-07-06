Shader "Universal Render Pipeline/VAT_UniversalLit"
{
    Properties
    {
        [MainTexture] _BaseMap("Base Map", 2D) = "white" {}
        [MainColor]   _BaseColor("Base Color", Color) = (1, 1, 1, 1)

        _VATPositions("VAT Position Texture", 2D) = "black" {}
        _VATNormals("VAT Normal Texture", 2D) = "bump" {}
        _VATParams("VAT Params (Width, Height, 0, 0)", Vector) = (1, 1, 0, 0)
        _AnimParams("Anim Params (StartFrame, FrameCount, Time, FPS)", Vector) = (0, 1, 0, 24)
    }

    SubShader
    {
        Tags
        {
            "RenderType" = "Opaque"
            "RenderPipeline" = "UniversalPipeline"
            "IgnoreProjector" = "True"
        }
        LOD 300

        Pass
        {
            Name "ForwardLit"
            Tags { "LightMode" = "UniversalForward" }

            ZWrite On
            Cull Back

            HLSLPROGRAM
            #pragma target 3.5

            #pragma vertex vert
            #pragma fragment frag

            #pragma multi_compile _ _MAIN_LIGHT_SHADOWS _MAIN_LIGHT_SHADOWS_CASCADE _MAIN_LIGHT_SHADOWS_SCREEN
            #pragma multi_compile _ _ADDITIONAL_LIGHTS
            #pragma multi_compile _ _SHADOWS_SOFT
            #pragma multi_compile_instancing
            #include_with_pragmas "Packages/com.unity.render-pipelines.universal/ShaderLibrary/DOTS.hlsl"

            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"
            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Lighting.hlsl"

            struct Attributes
            {
                float4 positionOS : POSITION;
                float3 normalOS   : NORMAL;
                float2 uv         : TEXCOORD0;
                uint vertexID     : SV_VertexID;
                UNITY_VERTEX_INPUT_INSTANCE_ID
            };

            struct Varyings
            {
                float4 positionCS : SV_POSITION;
                float2 uv         : TEXCOORD0;
                float3 positionWS : TEXCOORD1;
                float3 normalWS   : TEXCOORD2;
                UNITY_VERTEX_INPUT_INSTANCE_ID
            };

            TEXTURE2D(_BaseMap);
            SAMPLER(sampler_BaseMap);
            TEXTURE2D(_VATPositions);
            SAMPLER(sampler_VATPositions);
            TEXTURE2D(_VATNormals);
            SAMPLER(sampler_VATNormals);

            CBUFFER_START(UnityPerMaterial)
                float4 _BaseMap_ST;
                float4 _BaseColor;
                float4 _VATParams;
                float4 _AnimParams;
            CBUFFER_END

            #ifdef UNITY_DOTS_INSTANCING_ENABLED
            UNITY_DOTS_INSTANCING_START(MaterialPropertyMetadata)
                UNITY_DOTS_INSTANCED_PROP_OVERRIDE_SUPPORTED(float4, _AnimParams)
                UNITY_DOTS_INSTANCED_PROP_OVERRIDE_SUPPORTED(float4, _BaseColor)
            UNITY_DOTS_INSTANCING_END(MaterialPropertyMetadata)

            #define _AnimParams UNITY_ACCESS_DOTS_INSTANCED_PROP_WITH_DEFAULT(float4, _AnimParams)
            #define _BaseColor UNITY_ACCESS_DOTS_INSTANCED_PROP_WITH_DEFAULT(float4, _BaseColor)
            #endif

            Varyings vert(Attributes input)
            {
                Varyings output;
                UNITY_SETUP_INSTANCE_ID(input);
                UNITY_TRANSFER_INSTANCE_ID(input, output);

                float4 anim = _AnimParams;
                float startFrame = anim.x;
                float frameCount = max(anim.y, 1.0);
                float animTime = max(anim.z, 0.0);
                float fps = anim.w > 0.0 ? anim.w : 24.0;

                float currentFrame = startFrame + fmod(animTime * fps, frameCount);
                float u = (float(input.vertexID) + 0.5) / _VATParams.x;
                float v = (currentFrame + 0.5) / _VATParams.y;
                float2 vatUV = float2(u, v);

                float3 animatedPos = SAMPLE_TEXTURE2D_LOD(_VATPositions, sampler_VATPositions, vatUV, 0).xyz;
                float3 animatedNorm = SAMPLE_TEXTURE2D_LOD(_VATNormals, sampler_VATNormals, vatUV, 0).xyz;

                input.positionOS.xyz = animatedPos;
                input.normalOS = normalize(animatedNorm);

                VertexPositionInputs vertexInput = GetVertexPositionInputs(input.positionOS.xyz);
                VertexNormalInputs normalInput = GetVertexNormalInputs(input.normalOS);

                output.positionCS = vertexInput.positionCS;
                output.positionWS = vertexInput.positionWS;
                output.normalWS = normalInput.normalWS;
                output.uv = TRANSFORM_TEX(input.uv, _BaseMap);

                return output;
            }

            float4 frag(Varyings input) : SV_Target
            {
                UNITY_SETUP_INSTANCE_ID(input);

                float4 texColor = SAMPLE_TEXTURE2D(_BaseMap, sampler_BaseMap, input.uv);
                float4 albedo = texColor * _BaseColor;

                Light mainLight = GetMainLight();
                float3 normal = normalize(input.normalWS);
                float NdotL = saturate(dot(normal, mainLight.direction));
                float3 lightColor = mainLight.color * (NdotL * mainLight.shadowAttenuation);
                float3 ambient = SampleSH(normal);

                float3 finalColor = albedo.rgb * (lightColor + ambient);
                return float4(finalColor, albedo.a);
            }
            ENDHLSL
        }

        Pass
        {
            Name "ShadowCaster"
            Tags { "LightMode" = "ShadowCaster" }

            ZWrite On
            ZTest LEqual
            ColorMask 0
            Cull Back

            HLSLPROGRAM
            #pragma target 3.5

            #pragma vertex ShadowPassVertex
            #pragma fragment ShadowPassFragment

            #pragma multi_compile_instancing
            #include_with_pragmas "Packages/com.unity.render-pipelines.universal/ShaderLibrary/DOTS.hlsl"

            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"
            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Lighting.hlsl"

            struct Attributes
            {
                float4 positionOS : POSITION;
                float3 normalOS   : NORMAL;
                uint vertexID     : SV_VertexID;
                UNITY_VERTEX_INPUT_INSTANCE_ID
            };

            struct Varyings
            {
                float4 positionCS : SV_POSITION;
                UNITY_VERTEX_INPUT_INSTANCE_ID
            };

            TEXTURE2D(_VATPositions);
            SAMPLER(sampler_VATPositions);
            TEXTURE2D(_VATNormals);
            SAMPLER(sampler_VATNormals);

            CBUFFER_START(UnityPerMaterial)
                float4 _BaseMap_ST;
                float4 _BaseColor;
                float4 _VATParams;
                float4 _AnimParams;
            CBUFFER_END

            #ifdef UNITY_DOTS_INSTANCING_ENABLED
            UNITY_DOTS_INSTANCING_START(MaterialPropertyMetadata)
                UNITY_DOTS_INSTANCED_PROP_OVERRIDE_SUPPORTED(float4, _AnimParams)
                UNITY_DOTS_INSTANCED_PROP_OVERRIDE_SUPPORTED(float4, _BaseColor)
            UNITY_DOTS_INSTANCING_END(MaterialPropertyMetadata)

            #define _AnimParams UNITY_ACCESS_DOTS_INSTANCED_PROP_WITH_DEFAULT(float4, _AnimParams)
            #define _BaseColor UNITY_ACCESS_DOTS_INSTANCED_PROP_WITH_DEFAULT(float4, _BaseColor)
            #endif

            float3 _LightDirection;

            Varyings ShadowPassVertex(Attributes input)
            {
                Varyings output;
                UNITY_SETUP_INSTANCE_ID(input);
                UNITY_TRANSFER_INSTANCE_ID(input, output);

                float4 anim = _AnimParams;
                float startFrame = anim.x;
                float frameCount = max(anim.y, 1.0);
                float animTime = max(anim.z, 0.0);
                float fps = anim.w > 0.0 ? anim.w : 24.0;

                float currentFrame = startFrame + fmod(animTime * fps, frameCount);
                float u = (float(input.vertexID) + 0.5) / _VATParams.x;
                float v = (currentFrame + 0.5) / _VATParams.y;
                float2 vatUV = float2(u, v);

                float3 animatedPos = SAMPLE_TEXTURE2D_LOD(_VATPositions, sampler_VATPositions, vatUV, 0).xyz;
                float3 animatedNorm = SAMPLE_TEXTURE2D_LOD(_VATNormals, sampler_VATNormals, vatUV, 0).xyz;

                input.positionOS.xyz = animatedPos;
                input.normalOS = normalize(animatedNorm);

                float3 positionWS = TransformObjectToWorld(input.positionOS.xyz);
                float3 normalWS = TransformObjectToWorldNormal(input.normalOS);

                output.positionCS = TransformWorldToHClip(ApplyShadowBias(positionWS, normalWS, _LightDirection));

                #if UNITY_REVERSED_Z
                    output.positionCS.z = min(output.positionCS.z, UNITY_NEAR_CLIP_VALUE);
                #else
                    output.positionCS.z = max(output.positionCS.z, UNITY_NEAR_CLIP_VALUE);
                #endif

                return output;
            }

            float4 ShadowPassFragment(Varyings input) : SV_TARGET
            {
                return 0;
            }
            ENDHLSL
        }

        Pass
        {
            Name "DepthOnly"
            Tags { "LightMode" = "DepthOnly" }

            ZWrite On
            ColorMask R
            Cull Back

            HLSLPROGRAM
            #pragma target 3.5

            #pragma vertex DepthPassVertex
            #pragma fragment DepthPassFragment

            #pragma multi_compile_instancing
            #include_with_pragmas "Packages/com.unity.render-pipelines.universal/ShaderLibrary/DOTS.hlsl"

            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"

            struct Attributes
            {
                float4 positionOS : POSITION;
                uint vertexID     : SV_VertexID;
                UNITY_VERTEX_INPUT_INSTANCE_ID
            };

            struct Varyings
            {
                float4 positionCS : SV_POSITION;
                UNITY_VERTEX_INPUT_INSTANCE_ID
            };

            TEXTURE2D(_VATPositions);
            SAMPLER(sampler_VATPositions);

            CBUFFER_START(UnityPerMaterial)
                float4 _BaseMap_ST;
                float4 _BaseColor;
                float4 _VATParams;
                float4 _AnimParams;
            CBUFFER_END

            #ifdef UNITY_DOTS_INSTANCING_ENABLED
            UNITY_DOTS_INSTANCING_START(MaterialPropertyMetadata)
                UNITY_DOTS_INSTANCED_PROP_OVERRIDE_SUPPORTED(float4, _AnimParams)
                UNITY_DOTS_INSTANCED_PROP_OVERRIDE_SUPPORTED(float4, _BaseColor)
            UNITY_DOTS_INSTANCING_END(MaterialPropertyMetadata)

            #define _AnimParams UNITY_ACCESS_DOTS_INSTANCED_PROP_WITH_DEFAULT(float4, _AnimParams)
            #define _BaseColor UNITY_ACCESS_DOTS_INSTANCED_PROP_WITH_DEFAULT(float4, _BaseColor)
            #endif

            Varyings DepthPassVertex(Attributes input)
            {
                Varyings output;
                UNITY_SETUP_INSTANCE_ID(input);
                UNITY_TRANSFER_INSTANCE_ID(input, output);

                float4 anim = _AnimParams;
                float startFrame = anim.x;
                float frameCount = max(anim.y, 1.0);
                float animTime = max(anim.z, 0.0);
                float fps = anim.w > 0.0 ? anim.w : 24.0;

                float currentFrame = startFrame + fmod(animTime * fps, frameCount);
                float u = (float(input.vertexID) + 0.5) / _VATParams.x;
                float v = (currentFrame + 0.5) / _VATParams.y;
                float2 vatUV = float2(u, v);

                float3 animatedPos = SAMPLE_TEXTURE2D_LOD(_VATPositions, sampler_VATPositions, vatUV, 0).xyz;
                input.positionOS.xyz = animatedPos;

                VertexPositionInputs vertexInput = GetVertexPositionInputs(input.positionOS.xyz);
                output.positionCS = vertexInput.positionCS;

                return output;
            }

            float4 DepthPassFragment(Varyings input) : SV_TARGET
            {
                return 0;
            }
            ENDHLSL
        }
    }
}
