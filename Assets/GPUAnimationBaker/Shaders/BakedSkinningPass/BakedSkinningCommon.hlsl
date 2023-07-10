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

// TODO:
// - 行列の分4pxのoffset必要
// - 行ごとに0.5pxのoffset必要
float2 CalcBoneUV(int boneIndex, int matrixColIndex = 0)
{
    float instancedTimeOffset = UNITY_ACCESS_INSTANCED_PROP(Props, _BakedAnimationTimeOffset);

    float instancedBoneCount = UNITY_ACCESS_INSTANCED_PROP(Props, _BoneCount);
    float instancedAnimationSpeed = UNITY_ACCESS_INSTANCED_PROP(Props, _AnimationSpeed);

    // float instancedTotalDuration = UNITY_ACCESS_INSTANCED_PROP(Props, _BakedAnimationTotalDuration);

    float instancedAnimationFPS = UNITY_ACCESS_INSTANCED_PROP(Props, _BakedAnimationFPS);
    float instancedTextureWidth = UNITY_ACCESS_INSTANCED_PROP(Props, _BakedTextureWidth);
    float instancedTextureHeight = UNITY_ACCESS_INSTANCED_PROP(Props, _BakedTextureHeight);

    // for debug
    // instancedTextureWidth = 128.;
    // instancedTextureHeight = 64.;

    float instancedCurrentAnimationInitialFrame =
        UNITY_ACCESS_INSTANCED_PROP(Props, _BakedCurrentAnimationInitialFrame);
    float instancedCurrentAnimationFrames = UNITY_ACCESS_INSTANCED_PROP(Props, _BakedCurrentAnimationFrames);
    float instancedAnimationTotalFrames = UNITY_ACCESS_INSTANCED_PROP(Props, _BakedAnimationTotalFrames);

    float frameCellOffsetX = 0.5 / instancedTextureWidth;
    float frameCellOffsetY = 0.5 / instancedTextureHeight;

    // TODO: -1 の必要ある？
    // float frameIndex = instancedCurrentAnimationInitialFrame + floor(fmod(_Time.y * _BakedAnimationFPS, instancedCurrentAnimationFrames) - 1);
    // float frameCoord = frameIndex / instancedTextureHeight + frameCellOffset;

    // NOTE: 正しいやつ
    // animation clip frame index
    float frameIndex =
        instancedCurrentAnimationInitialFrame +
        floor(
            fmod(
                instancedTimeOffset + _Time.y * instancedAnimationFPS * instancedAnimationSpeed,
                instancedCurrentAnimationFrames
            )
        );
    // for debug
    // frameIndex = 0;

    // TODO:
    // - ここで4かけつつ行列のベクトル分割用にoffsetするべき？
    // - matrix col index は普通に足してok??
    float boneFrameIndex =
        instancedBoneCount * frameIndex * 4 + // フレーム数分ボーンの数をオフセット
        boneIndex * 4 + // ボーンのindex分ずらす
        matrixColIndex; // ボーンの行列の行分ずらす
    // for debug: frame index = 0
    // float boneFrameIndex = boneIndex * 4 + matrixColIndex;

    // NOTE: 正しいやつ
    float frameX = fmod(boneFrameIndex, instancedTextureWidth) / instancedTextureWidth + frameCellOffsetX;
    float frameY = floor(boneFrameIndex / instancedTextureWidth) / instancedTextureHeight + frameCellOffsetY;
    // for debug
    // float frameX = vertexId / instancedTextureWidth + frameCellOffsetX;
    // frameY = 0 + frameCellOffsetY;

    // original
    // input.animationUV = float4(uv.x, (input.instancedTimeOffset + _Time.y) / instancedTotalDuration, 0, 0);

    return float2(frameX, frameY);
}

float3 GetBakedAnimationPositionOS(BakedSkinningAnimationInput input)
{
    float2 boneUV00 = CalcBoneUV(input.boneIndex0, 0);
    float2 boneUV01 = CalcBoneUV(input.boneIndex0, 1);
    float2 boneUV02 = CalcBoneUV(input.boneIndex0, 2);
    // float2 boneUV03 = CalcBoneUV(input.boneIndex0, 3);
    
    float2 boneUV10 = CalcBoneUV(input.boneIndex1, 0);
    float2 boneUV11 = CalcBoneUV(input.boneIndex1, 1);
    float2 boneUV12 = CalcBoneUV(input.boneIndex1, 2);
    // float2 boneUV13 = CalcBoneUV(input.boneIndex1, 3);
    
    float4 bone0Col0 = tex2Dlod(_BakedBonesMap, float4(boneUV00, 0, 0));
    float4 bone0Col1 = tex2Dlod(_BakedBonesMap, float4(boneUV01, 0, 0));
    float4 bone0Col2 = tex2Dlod(_BakedBonesMap, float4(boneUV02, 0, 0));
    // float4 bone0Col3 = tex2Dlod(_BakedBonesMap, boneUV03);
    float4 bone0Col3 = float4(0, 0, 0, 1);
    
    float4 bone1Col0 = tex2Dlod(_BakedBonesMap, float4(boneUV10, 0, 0));
    float4 bone1Col1 = tex2Dlod(_BakedBonesMap, float4(boneUV11, 0, 0));
    float4 bone1Col2 = tex2Dlod(_BakedBonesMap, float4(boneUV12, 0, 0));
    // float4 bone1Col3 = tex2Dlod(_BakedBonesMap, boneUV13);
    float4 bone1Col3 = float4(0, 0, 0, 1);
    
    // float4x4 bone0Mat = float4x4(
    //     bone0Col0.x, bone0Col0.y, bone0Col0.z, bone0Col0.w,
    //     bone0Col1.x, bone0Col1.y, bone0Col1.z, bone0Col1.w,
    //     bone0Col2.x, bone0Col2.y, bone0Col2.z, bone0Col2.w,
    //     0, 0, 0, 1
    // );
    float4x4 bone0Mat = float4x4(bone0Col0, bone0Col1, bone0Col2, bone0Col3);
    float4x4 bone1Mat = float4x4(bone1Col0, bone1Col1, bone1Col2, bone1Col3);
    // blend
    // float4x4 boneMat = bone0Mat * input.boneWeight0 + bone1Mat * input.boneWeight1;
    // float4x4 boneMat = bone0Mat * input.boneWeight0 + bone1Mat * 0;
    float4x4 boneMat = float4x4(
        lerp(bone0Col0, bone1Col0, input.boneWeight0 / 1),
        lerp(bone0Col1, bone1Col1, input.boneWeight0 / 1),
        lerp(bone0Col2, bone1Col2, input.boneWeight0 / 1),
        float4(0, 0, 0, 1)
    );
    
    // return mul(bone0Mat, float4(input.localPosition, 1.)).xyz;
    return mul(boneMat, float4(input.localPosition, 1.)).xyz;
    // debug
    // return input.localPosition;
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
