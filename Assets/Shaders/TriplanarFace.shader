Shader "Custom/TriplanarFace"
{
    Properties
    {
        _BaseMap ("Albedo", 2D) = "white" {}
        _BaseColor ("Color Tint", Color) = (1,1,1,1)
        _NormalMap ("Normal Map", 2D) = "bump" {}
        _NormalStrength ("Normal Strength", Range(0,2)) = 1.0
        _OcclusionMap ("Occlusion Map", 2D) = "white" {}
        _OcclusionStrength ("Occlusion Strength", Range(0,1)) = 1.0
        _Smoothness ("Smoothness", Range(0,1)) = 0.2
        _Metallic ("Metallic", Range(0,1)) = 0.0
        _TriplanarScale ("Triplanar Scale", Float) = 0.01
        _BlendSharpness ("Blend Sharpness", Range(1,8)) = 4.0
    }

    SubShader
    {
        Tags 
        { 
            "RenderType" = "Opaque" 
            "RenderPipeline" = "UniversalPipeline"
            "Queue" = "Geometry"
        }

        Pass
        {
            Name "ForwardLit"
            Tags { "LightMode" = "UniversalForward" }

            HLSLPROGRAM
            #pragma vertex vert
            #pragma fragment frag
            #pragma multi_compile _ _MAIN_LIGHT_SHADOWS
            #pragma multi_compile _ _MAIN_LIGHT_SHADOWS_CASCADE
            #pragma multi_compile _ _SHADOWS_SOFT
            #pragma multi_compile_fog

            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"
            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Lighting.hlsl"

            TEXTURE2D(_BaseMap);        SAMPLER(sampler_BaseMap);
            TEXTURE2D(_NormalMap);      SAMPLER(sampler_NormalMap);
            TEXTURE2D(_OcclusionMap);   SAMPLER(sampler_OcclusionMap);

            CBUFFER_START(UnityPerMaterial)
                float4 _BaseColor;
                float  _NormalStrength;
                float  _OcclusionStrength;
                float  _Smoothness;
                float  _Metallic;
                float  _TriplanarScale;
                float  _BlendSharpness;
            CBUFFER_END

            struct Attributes
            {
                float4 positionOS : POSITION;
                float3 normalOS   : NORMAL;
                float4 tangentOS  : TANGENT;
            };

            struct Varyings
            {
                float4 positionHCS : SV_POSITION;
                float3 positionWS  : TEXCOORD0;
                float3 normalWS    : TEXCOORD1;
                float3 tangentWS   : TEXCOORD2;
                float3 bitangentWS : TEXCOORD3;
                float  fogFactor   : TEXCOORD4;
            };

            // Sample a texture triplanarly in world space
            float4 TriplanarSample(TEXTURE2D_PARAM(tex, samp), float3 worldPos, float3 blendWeights, float scale)
            {
                float4 xSample = SAMPLE_TEXTURE2D(tex, samp, worldPos.yz * scale);
                float4 ySample = SAMPLE_TEXTURE2D(tex, samp, worldPos.xz * scale);
                float4 zSample = SAMPLE_TEXTURE2D(tex, samp, worldPos.xy * scale);
                return xSample * blendWeights.x + ySample * blendWeights.y + zSample * blendWeights.z;
            }

            float3 TriplanarNormal(TEXTURE2D_PARAM(tex, samp), float3 worldPos, float3 blendWeights, float scale, float strength)
            {
                float3 xNorm = UnpackNormalScale(SAMPLE_TEXTURE2D(tex, samp, worldPos.yz * scale), strength);
                float3 yNorm = UnpackNormalScale(SAMPLE_TEXTURE2D(tex, samp, worldPos.xz * scale), strength);
                float3 zNorm = UnpackNormalScale(SAMPLE_TEXTURE2D(tex, samp, worldPos.xy * scale), strength);
                // reorient each normal into world space per axis
                xNorm = float3(xNorm.xy + float2(0,0), abs(xNorm.z));
                yNorm = float3(yNorm.xy + float2(0,0), abs(yNorm.z));
                zNorm = float3(zNorm.xy + float2(0,0), abs(zNorm.z));
                return normalize(
                    xNorm.zyx * blendWeights.x +
                    yNorm.xzy * blendWeights.y +
                    zNorm.xyz * blendWeights.z
                );
            }

            Varyings vert(Attributes IN)
            {
                Varyings OUT;
                VertexPositionInputs posInputs = GetVertexPositionInputs(IN.positionOS.xyz);
                VertexNormalInputs normalInputs = GetVertexNormalInputs(IN.normalOS, IN.tangentOS);

                OUT.positionHCS  = posInputs.positionCS;
                OUT.positionWS   = posInputs.positionWS;
                OUT.normalWS     = normalInputs.normalWS;
                OUT.tangentWS    = normalInputs.tangentWS;
                OUT.bitangentWS  = normalInputs.bitangentWS;
                OUT.fogFactor    = ComputeFogFactor(posInputs.positionCS.z);
                return OUT;
            }

            half4 frag(Varyings IN) : SV_Target
            {
                // blend weights from world normal
                float3 blend = pow(abs(normalize(IN.normalWS)), _BlendSharpness);
                blend /= (blend.x + blend.y + blend.z + 0.0001);

                // sample albedo triplanarly
                float4 albedo = TriplanarSample(TEXTURE2D_ARGS(_BaseMap, sampler_BaseMap),
                                                IN.positionWS, blend, _TriplanarScale);
                albedo *= _BaseColor;

                // sample occlusion
                float occ = TriplanarSample(TEXTURE2D_ARGS(_OcclusionMap, sampler_OcclusionMap),
                                            IN.positionWS, blend, _TriplanarScale).r;
                occ = LerpWhiteTo(occ, _OcclusionStrength);

                // sample normal
                float3 normalWS = TriplanarNormal(TEXTURE2D_ARGS(_NormalMap, sampler_NormalMap),
                                                  IN.positionWS, blend, _TriplanarScale, _NormalStrength);

                // lighting
                InputData lightingInput = (InputData)0;
                lightingInput.positionWS        = IN.positionWS;
                lightingInput.normalWS          = normalWS;
                lightingInput.viewDirectionWS    = GetWorldSpaceNormalizeViewDir(IN.positionWS);
                lightingInput.shadowCoord        = TransformWorldToShadowCoord(IN.positionWS);
                lightingInput.fogCoord           = IN.fogFactor;
                lightingInput.bakedGI            = SampleSH(normalWS);
                lightingInput.normalizedScreenSpaceUV = GetNormalizedScreenSpaceUV(IN.positionHCS);

                SurfaceData surfaceData = (SurfaceData)0;
                surfaceData.albedo       = albedo.rgb;
                surfaceData.alpha        = 1.0;
                surfaceData.metallic     = _Metallic;
                surfaceData.smoothness   = _Smoothness;
                surfaceData.occlusion    = occ;
                surfaceData.normalTS     = float3(0,0,1);

                half4 color = UniversalFragmentPBR(lightingInput, surfaceData);
                color.rgb = MixFog(color.rgb, IN.fogFactor);
                return color;
            }
            ENDHLSL
        }

        // Shadow caster pass
        Pass
        {
            Name "ShadowCaster"
            Tags { "LightMode" = "ShadowCaster" }

            ZWrite On
            ZTest LEqual
            ColorMask 0

            HLSLPROGRAM
            #pragma vertex ShadowPassVertex
            #pragma fragment ShadowPassFragment
            #include "Packages/com.unity.render-pipelines.universal/Shaders/ShadowCasterPass.hlsl"
            ENDHLSL
        }
    }
}
