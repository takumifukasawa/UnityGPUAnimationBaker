using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace GPUAnimationBaker
{
    [System.Serializable]
    public class GPUAnimationFrame
    {
        public string AnimationName;
        public int Frames;
        // public float Duration;
        public GPUAnimationFrame(string animationName, int frames)
        {
            AnimationName = animationName;
            Frames = frames;
        }
    }
}