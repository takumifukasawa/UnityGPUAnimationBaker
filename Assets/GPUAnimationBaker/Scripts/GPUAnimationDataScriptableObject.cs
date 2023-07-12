using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace GPUAnimationBaker
{
    [System.Serializable]
    public class GPUAnimationMeshLODSetting
    {
        public Mesh LODMesh;
        public float ThresholdDistance;

        public GPUAnimationMeshLODSetting(Mesh mesh, float distance)
        {
            LODMesh = mesh;
            ThresholdDistance = distance;
        }
    }

    [CreateAssetMenu(fileName = "Data", menuName = "ScriptableObjects/GPUAnimationData")]
    public class GPUAnimationDataScriptableObject : ScriptableObject
    {
        [Header("Editable Settings")]
        public List<GPUAnimationMeshLODSetting> GPUAnimationMeshLODSettings = new List<GPUAnimationMeshLODSetting>();
        [Space(13)]
        [Header("Stats")]
        public float FPS;
        public float TotalDuration;
        public float TotalFrames;
        public int TextureWidth;
        public int TextureHeight;
        public int BoneCount;
        public List<GPUAnimationFrame> GPUAnimationFrames = new List<GPUAnimationFrame>();
        public Material RuntimeMaterial;
        public Texture BakedBonesMap;
        public List<Matrix4x4> BoneOffsetMatrices;

        public static GPUAnimationDataScriptableObject Create(
            float fps,
            float totalDuration,
            float totalFrames,
            int textureWidth,
            int textureHeight,
            int boneCount,
            List<GPUAnimationFrame> gpuAnimationFrames,
            List<GPUAnimationMeshLODSetting> gpuAnimationMeshLODSettings,
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
            gpuAnimationDataScriptableObject.BoneCount = boneCount;
            gpuAnimationDataScriptableObject.GPUAnimationFrames = gpuAnimationFrames;
            gpuAnimationDataScriptableObject.GPUAnimationMeshLODSettings = gpuAnimationMeshLODSettings;
            gpuAnimationDataScriptableObject.RuntimeMaterial = runtimeMaterial;
            gpuAnimationDataScriptableObject.BakedBonesMap = bakedBonesMap;
            gpuAnimationDataScriptableObject.BoneOffsetMatrices = boneOffsetMatrices;

            return gpuAnimationDataScriptableObject;
        }
    }
}