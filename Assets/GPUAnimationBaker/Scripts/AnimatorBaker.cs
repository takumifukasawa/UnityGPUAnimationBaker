using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using System.Runtime.InteropServices;

#if UNITY_EDITOR
using UnityEditor;
using System.IO;
#endif

namespace GPUAnimationBaker
{
    [RequireComponent(typeof(Animator))]
    public class AnimatorBaker : MonoBehaviour
    {
        // ----------------------------------------------------------------------------------
        // serialize
        // ----------------------------------------------------------------------------------

        [SerializeField]
        private ComputeShader _bakerComputeShader;

        [SerializeField]
        private Shader _runtimeShader;

        [SerializeField]
        private int _animationFps = 20;

        [SerializeField]
        private int _uvChannel = 1;

        // ----------------------------------------------------------------------------------
        // public
        // ----------------------------------------------------------------------------------

        /// <summary>
        /// 
        /// </summary>
        /// <returns></returns>
        public IEnumerator Generate()
        {
            Debug.Log("===== AnimatorBaker Begin Generate ... =====");
            Animator animator = GetComponent<Animator>();
            AnimationClip[] animationClips = animator.runtimeAnimatorController.animationClips;
            SkinnedMeshRenderer skinnedMeshRenderer = GetComponentInChildren<SkinnedMeshRenderer>();

            int vertexCount = skinnedMeshRenderer.sharedMesh.vertexCount;
            int textureWidth = Mathf.NextPowerOfTwo(vertexCount);

            // Mesh mesh = new Mesh();

            animator.speed = 0;

            int totalFrames = 0;
            float totalDuration = 0;

            VertexAttributesBaker _baker = new VertexAttributesBaker(skinnedMeshRenderer);

            foreach (AnimationClip animationClip in animationClips)
            {
                // int frames = Mathf.NextPowerOfTwo((int)(animationClip.length / (1f / (float)_animationFps)));
                // int currentAnimationClipFrames = Mathf.NextPowerOfTwo((int)(animationClip.length / (1f / (float)_animationFps)));
                int currentAnimationClipFrames = (int)(animationClip.length / (1f / (float)_animationFps));
                // List<VertexAttributes> vertexAttributesList = new List<VertexAttributes>();

                totalFrames += currentAnimationClipFrames;
                totalDuration += animationClip.length;

                // totalFrames = Mathf.NextPowerOfTwo(currentAnimationClipFrames);

                Debug.Log(string.Format(
                    "[AnimatorBaker] Generate - animationClip - name: {0}, duration: {1}, frames: {2}",
                    animationClip.name,
                    animationClip.length,
                    currentAnimationClipFrames
                ));

                _baker.MemoryAnimationFrame(animationClip.name, currentAnimationClipFrames);

                animator.Play(animationClip.name);

                yield return 0;

                for (int i = 0; i < currentAnimationClipFrames; i++)
                {
                    animator.Play(animationClip.name, 0, (float)i / currentAnimationClipFrames);
                    yield return 0;
                    for (int j = 0; j < vertexCount; j++)
                    {
                        int vertexIndex = j;
                        _baker.MemoryVertexAttributes(vertexIndex);
                    }
                }
            } // end foreach

#if UNITY_EDITOR
            _baker.Bake(
                _bakerComputeShader,
                totalFrames,
                _uvChannel
            );
            _baker.SaveAssets(gameObject.name, _runtimeShader, _animationFps, totalDuration, totalFrames);
            _baker.Dispose();
#endif

            Debug.Log("===== AnimatorBaker End Generate !! =====");
        }
    }
}