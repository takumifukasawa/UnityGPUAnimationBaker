using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace GPUAnimationBaker
{
    [RequireComponent(typeof(Animator))]
    public class AnimatorBaker : MonoBehaviour
    {
        // ----------------------------------------------------------------------------------
        // serialize
        // ----------------------------------------------------------------------------------

#if UNITY_EDITOR
        [SerializeField]
        private ComputeShader _bakerComputeShader;
#endif
        
        [SerializeField]
        private Shader _runtimeShader;

        [SerializeField]
        private int _animationFps = 20;
        
        [Header("LOD Order")]
        [SerializeField]
        private List<Mesh> _bakeLODSkinnedMeshes;

        [SerializeField]
        private float _lodDistanceStep = 10;

        [Space(13)]
        
        [SerializeField]
        private bool _dryRun;

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

            animator.speed = 0;

            int totalFrames = 0;
            float totalDuration = 0;

            VertexAttributesBaker _baker = new VertexAttributesBaker(
                skinnedMeshRenderer,
                _bakeLODSkinnedMeshes,
                _lodDistanceStep
            );

            foreach (AnimationClip animationClip in animationClips)
            {
                int currentAnimationClipFrames = (int)(animationClip.length / (1f / (float)_animationFps));

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
                    _baker.MemoryAllBoneAttributes();
                }
            } // end foreach

#if UNITY_EDITOR
            _baker.Bake(
                _bakerComputeShader,
                totalFrames
            );
            if (!_dryRun)
            {
                _baker.SaveAssets(gameObject.name, _runtimeShader, _animationFps, totalDuration, totalFrames);
            }

            _baker.Dispose();
#endif

            Debug.Log("===== AnimatorBaker End Generate !! =====");
        }
    }
}