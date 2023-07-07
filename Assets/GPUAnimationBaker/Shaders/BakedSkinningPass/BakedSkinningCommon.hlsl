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
    // float instancedTimeOffset;
    // float4 animationUV;
    float3 localPosition;
    int boneIndex0;
    int boneWeight0;
    int boneIndex1;
    int boneWeight1;
};

BakedSkinningAnimationInput CreateBakedSkinningAnimationInput(float3 localPosition, float4 boneWeights)
{
    BakedSkinningAnimationInput input;

    input.localPosition = localPosition;
    input.boneIndex0 = boneWeights.x;
    input.boneWeight0 = boneWeights.y;
    input.boneIndex1 = boneWeights.z;
    input.boneWeight1 = boneWeights.w;

    return input;
}

// TODO: 行列の分4pxのoffset必要
float2 CalcBoneUV(int boneIndex)
{
        float instancedTimeOffset = UNITY_ACCESS_INSTANCED_PROP(Props, _BakedAnimationTimeOffset);
    
        float instancedBoneCount = UNITY_ACCESS_INSTANCED_PROP(Props, _BoneCount);
        float instancedAnimationSpeed = UNITY_ACCESS_INSTANCED_PROP(Props, _AnimationSpeed);
    
        // float instancedTotalDuration = UNITY_ACCESS_INSTANCED_PROP(Props, _BakedAnimationTotalDuration);
    
        float instancedAnimationFPS = UNITY_ACCESS_INSTANCED_PROP(Props, _BakedAnimationFPS);
        float instancedTextureWidth = UNITY_ACCESS_INSTANCED_PROP(Props, _BakedTextureWidth);
        float instancedTextureHeight = UNITY_ACCESS_INSTANCED_PROP(Props, _BakedTextureHeight);
    
        float instancedCurrentAnimationInitialFrame = UNITY_ACCESS_INSTANCED_PROP(Props, _BakedCurrentAnimationInitialFrame);
        float instancedCurrentAnimationFrames = UNITY_ACCESS_INSTANCED_PROP(Props, _BakedCurrentAnimationFrames);
        float instancedAnimationTotalFrames = UNITY_ACCESS_INSTANCED_PROP(Props, _BakedAnimationTotalFrames);
    
        float frameCellOffsetX = 0.5 / instancedTextureWidth;
        float frameCellOffsetY = 0.5 / instancedTextureHeight;
    
        // TODO: -1 の必要ある？
        // float frameIndex = instancedCurrentAnimationInitialFrame + floor(fmod(_Time.y * _BakedAnimationFPS, instancedCurrentAnimationFrames) - 1);
        // float frameCoord = frameIndex / instancedTextureHeight + frameCellOffset;
    
        // NOTE: 正しいやつ
        float frameIndex = instancedCurrentAnimationInitialFrame + floor(fmod(instancedTimeOffset + _Time.y * instancedAnimationFPS * instancedAnimationSpeed, instancedCurrentAnimationFrames));
        // for debug
        // float frameIndex = 0;
    
        float boneFrameIndex = instancedBoneCount * frameIndex + boneIndex;
    
        // NOTE: 正しいやつ
        float frameX = fmod(boneFrameIndex, instancedTextureWidth) / instancedTextureWidth + frameCellOffsetX;
        float frameY = floor(boneFrameIndex / instancedTextureWidth) / instancedTextureHeight + frameCellOffsetY;
        // for debug
        // float frameX = vertexId / instancedTextureWidth + frameCellOffsetX;
        // float frameY = 0 + frameCellOffsetY;
    
        // original
        // input.animationUV = float4(uv.x, (input.instancedTimeOffset + _Time.y) / instancedTotalDuration, 0, 0);
    
        return float2(frameX, frameY);
}

float3 GetBakedAnimationPositionOS(BakedSkinningAnimationInput input)
{
    float boneUV0 = CalcBoneUV(input.boneIndex0);
    float boneUV1 = CalcBoneUV(input.boneIndex1);
    return input.localPosition;
}

float4 GetBakedAnimationNormalOS(BakedSkinningAnimationInput input)
{
    // TODO: skinningに合わせて再計算
    return float4(0, 0, 1, 1);
    // return tex2Dlod(_BakedNormalMap, input.animationUV);
}

float4 GetBakedAnimationTangentOS(BakedSkinningAnimationInput input)
{
    // NOTE: 一旦tangent消してみる
    return float4(1, 0, 0, 1);
    // return tex2Dlod(_BakedTangentMap, input.animationUV);
}

#endif

