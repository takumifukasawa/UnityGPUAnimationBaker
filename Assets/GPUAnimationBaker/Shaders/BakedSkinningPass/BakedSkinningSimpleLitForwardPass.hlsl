#ifndef UNIVERSAL_BAKED_SKINNING_SIMPLE_LIT_PASS_INCLUDED
#define UNIVERSAL_BAKED_SKINNING_SIMPLE_LIT_PASS_INCLUDED

// ---------------------------------------------------------------------------------
// ref:
// https://github.com/Unity-Technologies/Graphics/blob/10.8.1/com.unity.render-pipelines.universal/Shaders/SimpleLitForwardPass.hlsl
// ---------------------------------------------------------------------------------

#include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Lighting.hlsl"

#include "BakedSkinningCommon.hlsl"

struct Attributes
{
    float4 positionOS    : POSITION;
    float3 normalOS      : NORMAL;
    float4 tangentOS     : TANGENT;
    float2 texcoord      : TEXCOORD0;
    // CUSTOM_LINE_BEGIN
    float2 texcoord2      : TEXCOORD2;
    // CUSTOM_LINE_END
    float2 lightmapUV    : TEXCOORD1;
    UNITY_VERTEX_INPUT_INSTANCE_ID
};

struct Varyings
{
    float2 uv                       : TEXCOORD0;
    DECLARE_LIGHTMAP_OR_SH(lightmapUV, vertexSH, 1);

    float3 posWS                    : TEXCOORD2;    // xyz: posWS

// NOTE: 一旦tangent消してみる
// #ifdef _NORMALMAP
//     float4 normal                   : TEXCOORD3;    // xyz: normal, w: viewDir.x
//     float4 tangent                  : TEXCOORD4;    // xyz: tangent, w: viewDir.y
//     float4 bitangent                : TEXCOORD5;    // xyz: bitangent, w: viewDir.z
// #else
    float3  normal                  : TEXCOORD3;
    float3 viewDir                  : TEXCOORD4;
// #endif

    half4 fogFactorAndVertexLight   : TEXCOORD6; // x: fogFactor, yzw: vertex light

#if defined(REQUIRES_VERTEX_SHADOW_COORD_INTERPOLATOR)
    float4 shadowCoord              : TEXCOORD7;
#endif
    float4 animationUV              : TEXCOORD8;

    float4 positionCS               : SV_POSITION;
    UNITY_VERTEX_INPUT_INSTANCE_ID
    UNITY_VERTEX_OUTPUT_STEREO
};

void InitializeInputData(Varyings input, half3 normalTS, out InputData inputData)
{
    inputData.positionWS = input.posWS;

// NOTE: 一旦tangent消してみる
// #ifdef _NORMALMAP
//     half3 viewDirWS = half3(input.normal.w, input.tangent.w, input.bitangent.w);
//     inputData.normalWS = TransformTangentToWorld(normalTS,
//         half3x3(input.tangent.xyz, input.bitangent.xyz, input.normal.xyz));
// #else
    half3 viewDirWS = input.viewDir;
    inputData.normalWS = input.normal;
// #endif

    inputData.normalWS = NormalizeNormalPerPixel(inputData.normalWS);
    viewDirWS = SafeNormalize(viewDirWS);

    inputData.viewDirectionWS = viewDirWS;

#if defined(REQUIRES_VERTEX_SHADOW_COORD_INTERPOLATOR)
    inputData.shadowCoord = input.shadowCoord;
#elif defined(MAIN_LIGHT_CALCULATE_SHADOWS)
    inputData.shadowCoord = TransformWorldToShadowCoord(inputData.positionWS);
#else
    inputData.shadowCoord = float4(0, 0, 0, 0);
#endif

    inputData.fogCoord = input.fogFactorAndVertexLight.x;
    inputData.vertexLighting = input.fogFactorAndVertexLight.yzw;
    inputData.bakedGI = SAMPLE_GI(input.lightmapUV, input.vertexSH, inputData.normalWS);
    inputData.normalizedScreenSpaceUV = GetNormalizedScreenSpaceUV(input.positionCS);
    inputData.shadowMask = SAMPLE_SHADOWMASK(input.lightmapUV);
}

///////////////////////////////////////////////////////////////////////////////
//                  Vertex and Fragment functions                            //
///////////////////////////////////////////////////////////////////////////////

// Used in Standard (Simple Lighting) shader
Varyings LitPassVertexSimple(Attributes input)
{
    Varyings output = (Varyings)0;

    UNITY_SETUP_INSTANCE_ID(input);
    UNITY_TRANSFER_INSTANCE_ID(input, output);
    UNITY_INITIALIZE_VERTEX_OUTPUT_STEREO(output);

    // ----------------------------------------------------------------
    // CUSTOM_LINE_BEGIN
    // ----------------------------------------------------------------

    // default
    // VertexPositionInputs vertexInput = GetVertexPositionInputs(input.positionOS.xyz);
    // VertexNormalInputs normalInput = GetVertexNormalInputs(input.normalOS, input.tangentOS);

    BakedSkinningAnimationInput bakedSkinningAnimationInput = CreateBakedSkinningAnimationInput(input.texcoord2);
    float3 bakedSkinningPositionOS = GetBakedAnimationPositionOS(bakedSkinningAnimationInput);
    float4 bakedSkinningNormalOS = GetBakedAnimationNormalOS(bakedSkinningAnimationInput);
    float4 bakedSkinningTangentOS = GetBakedAnimationTangentOS(bakedSkinningAnimationInput);

    VertexPositionInputs vertexInput = GetVertexPositionInputs(bakedSkinningPositionOS);
    VertexNormalInputs normalInput = GetVertexNormalInputs(bakedSkinningNormalOS, bakedSkinningTangentOS);

    output.animationUV = bakedSkinningAnimationInput.animationUV;

    // ----------------------------------------------------------------
    // CUSTOM_LINE_END
    // ----------------------------------------------------------------

    half3 viewDirWS = GetWorldSpaceViewDir(vertexInput.positionWS);
    half3 vertexLight = VertexLighting(vertexInput.positionWS, normalInput.normalWS);
    half fogFactor = ComputeFogFactor(vertexInput.positionCS.z);

    output.uv = TRANSFORM_TEX(input.texcoord, _BaseMap);
    output.posWS.xyz = vertexInput.positionWS;
    output.positionCS = vertexInput.positionCS;

// #ifdef _NORMALMAP
//     output.normal = half4(normalInput.normalWS, viewDirWS.x);
//     output.tangent = half4(normalInput.tangentWS, viewDirWS.y);
//     output.bitangent = half4(normalInput.bitangentWS, viewDirWS.z);
// #else
    output.normal = NormalizeNormalPerVertex(normalInput.normalWS);
    output.viewDir = viewDirWS;
// #endif

    OUTPUT_LIGHTMAP_UV(input.lightmapUV, unity_LightmapST, output.lightmapUV);
    OUTPUT_SH(output.normal.xyz, output.vertexSH);

    output.fogFactorAndVertexLight = half4(fogFactor, vertexLight);

#if defined(REQUIRES_VERTEX_SHADOW_COORD_INTERPOLATOR)
    output.shadowCoord = GetShadowCoord(vertexInput);
#endif

    return output;
}

// Used for StandardSimpleLighting shader
half4 LitPassFragmentSimple(Varyings input) : SV_Target
{
    UNITY_SETUP_INSTANCE_ID(input);
    UNITY_SETUP_STEREO_EYE_INDEX_POST_VERTEX(input);

    // ----------------------------------------------------------------
    // CUSTOM_LINE_BEGIN
    // ----------------------------------------------------------------

    // default
    // float2 uv = input.uv;
    // half4 diffuseAlpha = SampleAlbedoAlpha(uv, TEXTURE2D_ARGS(_BaseMap, sampler_BaseMap));
    // half3 diffuse = diffuseAlpha.rgb * _BaseColor.rgb;

    half4 instancedCustomColor = UNITY_ACCESS_INSTANCED_PROP(Props, _CustomColor);
    instancedCustomColor = half4(1, 1, 1, 1);

    float2 uv = input.uv;
    half4 diffuseAlpha = SampleAlbedoAlpha(uv, TEXTURE2D_ARGS(_BaseMap, sampler_BaseMap));
    half3 diffuse = diffuseAlpha.rgb * _BaseColor.rgb * instancedCustomColor.rgb;
    // half3 diffuse = diffuseAlpha.rgb * _BaseColor.rgb;

    // ----------------------------------------------------------------
    // CUSTOM_LINE_END
    // ----------------------------------------------------------------

    half alpha = diffuseAlpha.a * _BaseColor.a;
    AlphaDiscard(alpha, _Cutoff);

    #ifdef _ALPHAPREMULTIPLY_ON
        diffuse *= alpha;
    #endif

    half3 normalTS = SampleNormal(uv, TEXTURE2D_ARGS(_BumpMap, sampler_BumpMap));
    half3 emission = SampleEmission(uv, _EmissionColor.rgb, TEXTURE2D_ARGS(_EmissionMap, sampler_EmissionMap));
    half4 specular = SampleSpecularSmoothness(uv, alpha, _SpecColor, TEXTURE2D_ARGS(_SpecGlossMap, sampler_SpecGlossMap));
    half smoothness = specular.a;

    InputData inputData;
    InitializeInputData(input, normalTS, inputData);

    half4 color = UniversalFragmentBlinnPhong(inputData, diffuse, specular, smoothness, emission, alpha);
    color.rgb = MixFog(color.rgb, inputData.fogCoord);
    color.a = OutputAlpha(color.a, _Surface);

    // color.rgb = half3(input.animationUV.w, 0, 0);
    // color.a = 1;
    // color.rgb = half3(1, 1, 1);
    // color.a = 1;

    return color;
}

#endif