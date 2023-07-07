#ifndef UNIVERSAL_BAKED_SKINNING_SIMPLE_LIT_INPUT_INCLUDED
#define UNIVERSAL_BAKED_SKINNING_SIMPLE_LIT_INPUT_INCLUDED

// ---------------------------------------------------------------------------------
// ref:
// Library/PackageCache/com.unity.render-pipelines.universal@12.1.7/Shaders/SimpleLitInput.hlsl
// 
// cbufferの上書きができないので、SimpleLitInputをコピペしている
// ---------------------------------------------------------------------------------

#include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"
#include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/SurfaceInput.hlsl"

CBUFFER_START(UnityPerMaterial)
    float4 _BaseMap_ST;
    half4 _BaseColor;
    half4 _SpecColor;
    half4 _EmissionColor;
    half _Cutoff;
    half _Surface;
    // ----------------------------------------------------------------
    // CUSTOM_LINE_BEGIN
    // ----------------------------------------------------------------
    sampler2D _BakedBonesMap;
    // NOTE: skinningパターンではいらない
    // sampler2D _BakedPositionMap;
    // sampler2D _BakedNormalMap;
    // sampler2D _BakedTangentMap;

    // float _BakedVertexRange;
    // float _BakedAnimationFPS;
    // ----------------------------------------------------------------
    // CUSTOM_LINE_END
    // ----------------------------------------------------------------
CBUFFER_END

// ----------------------------------------------------------------
// CUSTOM_LINE_BEGIN
// ----------------------------------------------------------------
UNITY_INSTANCING_BUFFER_START(Props)
    UNITY_DEFINE_INSTANCED_PROP(half4, _CustomColor)
    UNITY_DEFINE_INSTANCED_PROP(float, _BoneCount)
    UNITY_DEFINE_INSTANCED_PROP(float, _AnimationSpeed)
    UNITY_DEFINE_INSTANCED_PROP(float, _BakedTextureWidth)
    UNITY_DEFINE_INSTANCED_PROP(float, _BakedTextureHeight)
    UNITY_DEFINE_INSTANCED_PROP(float, _BakedAnimationFPS)
    UNITY_DEFINE_INSTANCED_PROP(float, _BakedAnimationTotalDuration)
    UNITY_DEFINE_INSTANCED_PROP(float, _BakedAnimationTotalFrames)
    UNITY_DEFINE_INSTANCED_PROP(float, _BakedCurrentAnimationInitialFrame)
    UNITY_DEFINE_INSTANCED_PROP(float, _BakedCurrentAnimationFrames)
    UNITY_DEFINE_INSTANCED_PROP(float, _BakedAnimationTimeOffset)
UNITY_INSTANCING_BUFFER_END(Props)

// ----------------------------------------------------------------
// CUSTOM_LINE_END
// ----------------------------------------------------------------

TEXTURE2D(_SpecGlossMap);       SAMPLER(sampler_SpecGlossMap);

half4 SampleSpecularSmoothness(half2 uv, half alpha, half4 specColor, TEXTURE2D_PARAM(specMap, sampler_specMap))
{
    half4 specularSmoothness = half4(0.0h, 0.0h, 0.0h, 1.0h);
#ifdef _SPECGLOSSMAP
    specularSmoothness = SAMPLE_TEXTURE2D(specMap, sampler_specMap, uv) * specColor;
#elif defined(_SPECULAR_COLOR)
    specularSmoothness = specColor;
#endif

#ifdef _GLOSSINESS_FROM_BASE_ALPHA
    specularSmoothness.a = exp2(10 * alpha + 1);
#else
    specularSmoothness.a = exp2(10 * specularSmoothness.a + 1);
#endif

    return specularSmoothness;
}

inline void InitializeSimpleLitSurfaceData(float2 uv, out SurfaceData outSurfaceData)
{
    outSurfaceData = (SurfaceData)0;

    half4 albedoAlpha = SampleAlbedoAlpha(uv, TEXTURE2D_ARGS(_BaseMap, sampler_BaseMap));
    outSurfaceData.alpha = albedoAlpha.a * _BaseColor.a;
    AlphaDiscard(outSurfaceData.alpha, _Cutoff);

    outSurfaceData.albedo = albedoAlpha.rgb * _BaseColor.rgb;
#ifdef _ALPHAPREMULTIPLY_ON
    outSurfaceData.albedo *= outSurfaceData.alpha;
#endif

    half4 specularSmoothness = SampleSpecularSmoothness(uv, outSurfaceData.alpha, _SpecColor, TEXTURE2D_ARGS(_SpecGlossMap, sampler_SpecGlossMap));
    outSurfaceData.metallic = 0.0; // unused
    outSurfaceData.specular = specularSmoothness.rgb;
    outSurfaceData.smoothness = specularSmoothness.a;
    outSurfaceData.normalTS = SampleNormal(uv, TEXTURE2D_ARGS(_BumpMap, sampler_BumpMap));
    outSurfaceData.occlusion = 1.0; // unused
    outSurfaceData.emission = SampleEmission(uv, _EmissionColor.rgb, TEXTURE2D_ARGS(_EmissionMap, sampler_EmissionMap));
}

#endif