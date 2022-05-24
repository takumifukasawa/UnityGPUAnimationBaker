#ifndef UNIVERSAL_BAKED_SKINNING_COMMON_INCLUDED
#define UNIVERSAL_BAKED_SKINNING_COMMON_INCLUDED

struct BakedSkinningAnimationInfo
{
    float3 positionOS;
    float4 normalOS;
    float4 tangentOS;
};

struct BakedSkinningAnimationInput
{
    float instancedTimeOffset;
    float4 animationUV;
};

BakedSkinningAnimationInput CreateBakedSkinningAnimationInput(float2 uv)
{
    BakedSkinningAnimationInput input;

    input.instancedTimeOffset = UNITY_ACCESS_INSTANCED_PROP(Props, _BakedAnimationTimeOffset);

    float instancedVertexCount = UNITY_ACCESS_INSTANCED_PROP(Props, _VertexCount);
    float instancedAnimationSpeed = UNITY_ACCESS_INSTANCED_PROP(Props, _AnimationSpeed);

    // float instancedTotalDuration = UNITY_ACCESS_INSTANCED_PROP(Props, _BakedAnimationTotalDuration);

    float instancedAnimationFPS = UNITY_ACCESS_INSTANCED_PROP(Props, _BakedAnimationFPS);
    float instancedTextureWidth = UNITY_ACCESS_INSTANCED_PROP(Props, _BakedTextureWidth);
    float instancedTextureHeight = UNITY_ACCESS_INSTANCED_PROP(Props, _BakedTextureHeight);

    float instancedCurrentAnimationInitialFrame = UNITY_ACCESS_INSTANCED_PROP(Props, _BakedCurrentAnimationInitialFrame);
    float instancedCurrentAnimationFrames = UNITY_ACCESS_INSTANCED_PROP(Props, _BakedCurrentAnimationFrames);
    float instancedAnimationTotalFrames = UNITY_ACCESS_INSTANCED_PROP(Props, _BakedAnimationTotalFrames);

    float vertexId = uv.x;
    float frameCellOffsetX = 0.5 / instancedTextureWidth;
    float frameCellOffsetY = 0.5 / instancedTextureHeight;


    // TODO: -1 の必要ある？
    // float frameIndex = instancedCurrentAnimationInitialFrame + floor(fmod(_Time.y * _BakedAnimationFPS, instancedCurrentAnimationFrames) - 1);
    // float frameCoord = frameIndex / instancedTextureHeight + frameCellOffset;

    // NOTE: 正しいやつ
    float frameIndex = instancedCurrentAnimationInitialFrame + floor(fmod(input.instancedTimeOffset + _Time.y * instancedAnimationFPS * instancedAnimationSpeed, instancedCurrentAnimationFrames));
    // for debug
    // float frameIndex = 0;

    float vertexFrameIndex = instancedVertexCount * frameIndex + vertexId;

    // NOTE: 正しいやつ
    float frameX = fmod(vertexFrameIndex, instancedTextureWidth) / instancedTextureWidth + frameCellOffsetX;
    float frameY = floor(vertexFrameIndex / instancedTextureWidth) / instancedTextureHeight + frameCellOffsetY;
    // for debug
    // float frameX = vertexId / instancedTextureWidth + frameCellOffsetX;
    // float frameY = 0 + frameCellOffsetY;

    // original
    // input.animationUV = float4(uv.x, (input.instancedTimeOffset + _Time.y) / instancedTotalDuration, 0, 0);

    input.animationUV = float4(frameX, frameY, 0, 0);

    return input;
}

float3 GetBakedAnimationPositionOS(BakedSkinningAnimationInput input)
{
    return (tex2Dlod(_BakedPositionMap, input.animationUV)).xyz;
}

float4 GetBakedAnimationNormalOS(BakedSkinningAnimationInput input)
{
    return tex2Dlod(_BakedNormalMap, input.animationUV);
}

float4 GetBakedAnimationTangentOS(BakedSkinningAnimationInput input)
{
    // NOTE: 一旦tangent消してみる
    return float4(1, 0, 0, 1);
    // return tex2Dlod(_BakedTangentMap, input.animationUV);
}

#endif

