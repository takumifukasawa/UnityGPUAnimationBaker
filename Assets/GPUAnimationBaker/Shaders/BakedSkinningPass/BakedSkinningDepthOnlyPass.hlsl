#ifndef UNIVERSAL_BAKED_SKINNING_DEPTH_ONLY_PASS_INCLUDED
#define UNIVERSAL_BAKED_SKINNING_DEPTH_ONLY_PASS_INCLUDED

// ---------------------------------------------------------------------------------
// ref:
// Library/PackageCache/com.unity.render-pipelines.universal@12.1.7/Shaders/DepthOnlyPass.hlsl
// ---------------------------------------------------------------------------------

#include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"

// ------------------------------------------------------------
// CUSTOM_LINE_BEGIN
// ------------------------------------------------------------

#include "BakedSkinningCommon.hlsl"

// ------------------------------------------------------------
// CUSTOM_LINE_END
// ------------------------------------------------------------

struct Attributes
{
    float4 position : POSITION;
    float2 texcoord : TEXCOORD0;
    // ----------------------------------------------------------------
    // CUSTOM_LINE_BEGIN
    // ----------------------------------------------------------------
    float2 texcoord2 : TEXCOORD1;
    float4 texcoord3 : TEXCOORD2;
    // ----------------------------------------------------------------
    // CUSTOM_LINE_END
    // ----------------------------------------------------------------   
    UNITY_VERTEX_INPUT_INSTANCE_ID
};

struct Varyings
{
    float2 uv : TEXCOORD0;
    float4 positionCS : SV_POSITION;
    UNITY_VERTEX_INPUT_INSTANCE_ID
    UNITY_VERTEX_OUTPUT_STEREO
};

Varyings DepthOnlyVertex(Attributes input)
{
    Varyings output = (Varyings)0;
    UNITY_SETUP_INSTANCE_ID(input);
    UNITY_INITIALIZE_VERTEX_OUTPUT_STEREO(output);


    // ----------------------------------------------------------------
    // CUSTOM_LINE_BEGIN
    // ----------------------------------------------------------------

    // ORIGINAL
    // output.uv = TRANSFORM_TEX(input.texcoord, _BaseMap);
    // output.positionCS = TransformObjectToHClip(input.position.xyz);

    BakedSkinningAnimationInput bakedSkinningAnimationInput = CreateBakedSkinningAnimationInput(
        input.texcoord3
    );
    float4x4 bakedSkinMatrix = GetBakedSkinMatrix(
        bakedSkinningAnimationInput.boneIndices,
        bakedSkinningAnimationInput.boneWeights
    );
    float3 bakedSkinningPositionOS = GetBakedAnimationPositionOS(input.position, bakedSkinMatrix);

    // BakedSkinningAnimationInfo bakedSkinningAnimationInfo = GetBakedSkinningAnimationInfo(input.texcoord2);
    output.uv = TRANSFORM_TEX(input.texcoord, _BaseMap);
    output.positionCS = TransformObjectToHClip(bakedSkinningPositionOS);

    // ----------------------------------------------------------------
    // CUSTOM_LINE_END
    // ----------------------------------------------------------------

    return output;
}

half4 DepthOnlyFragment(Varyings input) : SV_TARGET
{
    UNITY_SETUP_STEREO_EYE_INDEX_POST_VERTEX(input);

    Alpha(SampleAlbedoAlpha(input.uv, TEXTURE2D_ARGS(_BaseMap, sampler_BaseMap)).a, _BaseColor, _Cutoff);
    return 0;
}
#endif
