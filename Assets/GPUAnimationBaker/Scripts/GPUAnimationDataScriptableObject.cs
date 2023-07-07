using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace GPUAnimationBaker
{
    [CreateAssetMenu(fileName = "Data", menuName = "ScriptableObjects/GPUAnimationData")]
    public class GPUAnimationDataScriptableObject : ScriptableObject
    {
        public float FPS;
        public float TotalDuration;
        public float TotalFrames;
        public int TextureWidth;
        public int TextureHeight;
        public int VertexCount;
        public int BoneCount;
        public List<GPUAnimationFrame> GPUAnimationFrames = new List<GPUAnimationFrame>();
        public List<Matrix4x4> BoneOffsetMatrices;

        public static GPUAnimationDataScriptableObject Create(
            float fps,
            float totalDuration,
            float totalFrames,
            int textureWidth,
            int textureHeight,
            int vertexCount,
            int boneCount,
            List<GPUAnimationFrame> gpuAnimationFrames,
            List<Matrix4x4> boneOffsetMatrices
        )
        {
            var gpuAnimationDataScriptableObject = ScriptableObject.CreateInstance<GPUAnimationDataScriptableObject>();

            gpuAnimationDataScriptableObject.FPS = fps;
            gpuAnimationDataScriptableObject.TotalDuration = totalDuration;
            gpuAnimationDataScriptableObject.TotalFrames = totalFrames;
            gpuAnimationDataScriptableObject.TextureWidth = textureWidth;
            gpuAnimationDataScriptableObject.TextureHeight = textureHeight;
            gpuAnimationDataScriptableObject.VertexCount = vertexCount;
            gpuAnimationDataScriptableObject.BoneCount = boneCount;
            gpuAnimationDataScriptableObject.GPUAnimationFrames = gpuAnimationFrames;
            gpuAnimationDataScriptableObject.BoneOffsetMatrices = boneOffsetMatrices;

            return gpuAnimationDataScriptableObject;
        }
    }
}