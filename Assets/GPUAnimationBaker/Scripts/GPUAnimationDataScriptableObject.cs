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
        public List<GPUAnimationFrame> GPUAnimationFrames = new List<GPUAnimationFrame>();
    }
}