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
        public List<Mesh> Meshes;
        public Material RuntimeMaterial;
        public Texture BakedBonesMap;
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
            List<Mesh> meshes,
            Material runtimeMaterial,
            Texture bakedBonesMap,
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
            gpuAnimationDataScriptableObject.Meshes = meshes;
            gpuAnimationDataScriptableObject.RuntimeMaterial = runtimeMaterial;
            gpuAnimationDataScriptableObject.BakedBonesMap = bakedBonesMap;
            gpuAnimationDataScriptableObject.BoneOffsetMatrices = boneOffsetMatrices;

            return gpuAnimationDataScriptableObject;
        }
    }
}