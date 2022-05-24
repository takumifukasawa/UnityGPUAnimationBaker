#ifndef UNIVERSAL_BAKED_SKINNING_SHADOW_CASTER_PASS_INCLUDED
#define UNIVERSAL_BAKED_SKINNING_SHADOW_CASTER_PASS_INCLUDED

// ---------------------------------------------------------------------------------
// ref:
// https://github.com/Unity-Technologies/Graphics/10.8.1/release/com.unity.render-pipelines.universal/Shaders/ShadowCasterPass.hlsl
// ---------------------------------------------------------------------------------

#include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"
#include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Shadows.hlsl"

// ----------------------------------------------------------------
// CUSTOM_LINE_BEGIN
// ----------------------------------------------------------------
#include "BakedSkinningCommon.hlsl"
// ----------------------------------------------------------------
// CUSTOM_LINE_END
// ----------------------------------------------------------------

float3 _LightDirection;

struct Attributes
{
    float4 positionOS   : POSITION;
    float3 normalOS     : NORMAL;
    float2 texcoord     : TEXCOORD0;
    // ----------------------------------------------------------------
    // CUSTOM_LINE_BEGIN
    // ----------------------------------------------------------------
    float2 texcoord2      : TEXCOORD1;
    // ----------------------------------------------------------------
    // CUSTOM_LINE_END
    // ----------------------------------------------------------------
    UNITY_VERTEX_INPUT_INSTANCE_ID
};

struct Varyings
{
    float2 uv           : TEXCOORD0;
    float4 positionCS   : SV_POSITION;
};

float4 GetShadowPositionHClip(Attributes input)
{
    // ----------------------------------------------------------------
    // CUSTOM_LINE_BEGIN
    // ----------------------------------------------------------------

    // default
    // float3 positionWS = TransformObjectToWorld(input.positionOS.xyz);
    // float3 normalWS = TransformObjectToWorldNormal(input.normalOS);

    BakedSkinningAnimationInput bakedSkinningAnimationInput = CreateBakedSkinningAnimationInput(input.texcoord2);
    float3 bakedSkinningPositionOS = GetBakedAnimationPositionOS(bakedSkinningAnimationInput);
    float4 bakedSkinningNormalOS = GetBakedAnimationNormalOS(bakedSkinningAnimationInput);

    float3 positionWS = TransformObjectToWorld(bakedSkinningPositionOS);
    float3 normalWS = TransformObjectToWorldNormal(bakedSkinningNormalOS);

    // ----------------------------------------------------------------
    // CUSTOM_LINE_END
    // ----------------------------------------------------------------

    float4 positionCS = TransformWorldToHClip(ApplyShadowBias(positionWS, normalWS, _LightDirection));

#if UNITY_REVERSED_Z
    positionCS.z = min(positionCS.z, positionCS.w * UNITY_NEAR_CLIP_VALUE);
#else
    positionCS.z = max(positionCS.z, positionCS.w * UNITY_NEAR_CLIP_VALUE);
#endif

    return positionCS;
}

Varyings ShadowPassVertex(Attributes input)
{
    Varyings output;
    UNITY_SETUP_INSTANCE_ID(input);

    output.uv = TRANSFORM_TEX(input.texcoord, _BaseMap);
    output.positionCS = GetShadowPositionHClip(input);
    return output;
}

half4 ShadowPassFragment(Varyings input) : SV_TARGET
{
    Alpha(SampleAlbedoAlpha(input.uv, TEXTURE2D_ARGS(_BaseMap, sampler_BaseMap)).a, _BaseColor, _Cutoff);
    return 0;
}

#endif