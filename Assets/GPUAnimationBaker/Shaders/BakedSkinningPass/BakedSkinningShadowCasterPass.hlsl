#ifndef UNIVERSAL_BAKED_SKINNING_SHADOW_CASTER_PASS_INCLUDED
#define UNIVERSAL_BAKED_SKINNING_SHADOW_CASTER_PASS_INCLUDED

// ---------------------------------------------------------------------------------
// ref:
// Library/PackageCache/com.unity.render-pipelines.universal@12.1.7/Shaders/ShadowCasterPass.hlsl
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

// Shadow Casting Light geometric parameters. These variables are used when applying the shadow Normal Bias and are set by UnityEngine.Rendering.Universal.ShadowUtils.SetupShadowCasterConstantBuffer in com.unity.render-pipelines.universal/Runtime/ShadowUtils.cs
// For Directional lights, _LightDirection is used when applying shadow Normal Bias.
// For Spot lights and Point lights, _LightPosition is used to compute the actual light direction because it is different at each shadow caster geometry vertex.
float3 _LightDirection;
float3 _LightPosition;

struct Attributes
{
    float4 positionOS   : POSITION;
    float3 normalOS     : NORMAL;
    float2 texcoord     : TEXCOORD0;
    // ----------------------------------------------------------------
    // CUSTOM_LINE_BEGIN
    // ----------------------------------------------------------------
    float2 texcoord2      : TEXCOORD1;
    float4 texcoord3 : TEXCOORD2;
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

    // ORIGINAL
    // float3 positionWS = TransformObjectToWorld(input.positionOS.xyz);
    // float3 normalWS = TransformObjectToWorldNormal(input.normalOS);

    BakedSkinningAnimationInput bakedSkinningAnimationInput = CreateBakedSkinningAnimationInput(input.positionOS.xyz, input.texcoord3);
    float4x4 bakedSkinMatrix = GetBakedSkinMatrix(
        bakedSkinningAnimationInput.boneIndices,
        bakedSkinningAnimationInput.boneWeights
    );
    float3 bakedSkinningPositionOS = GetBakedAnimationPositionOS(bakedSkinningAnimationInput, bakedSkinMatrix);
    float4 bakedSkinningNormalOS = GetBakedAnimationNormalOS(bakedSkinningAnimationInput, bakedSkinMatrix);

    float3 positionWS = TransformObjectToWorld(bakedSkinningPositionOS);
    float3 normalWS = TransformObjectToWorldNormal(bakedSkinningNormalOS);

    // ----------------------------------------------------------------
    // CUSTOM_LINE_END
    // ----------------------------------------------------------------

#if _CASTING_PUNCTUAL_LIGHT_SHADOW
    float3 lightDirectionWS = normalize(_LightPosition - positionWS);
#else
    float3 lightDirectionWS = _LightDirection;
#endif

    float4 positionCS = TransformWorldToHClip(ApplyShadowBias(positionWS, normalWS, lightDirectionWS));

#if UNITY_REVERSED_Z
    positionCS.z = min(positionCS.z, UNITY_NEAR_CLIP_VALUE);
#else
    positionCS.z = max(positionCS.z, UNITY_NEAR_CLIP_VALUE);
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