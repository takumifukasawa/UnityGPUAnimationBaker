using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace GPUAnimationBaker
{
    [RequireComponent(typeof(MeshRenderer))]
    public class GPUAnimationController : MonoBehaviour
    {
        [ReadOnly, SerializeField]
        private GPUAnimationDataScriptableObject _gpuAnimationDataScriptableObject;

        [SerializeField, HideInInspector]
        private int _currentGPUAnimationFrameIndex = 0;

        private MeshRenderer _meshRenderer;

        private MaterialPropertyBlock _materialPropertyBlock;

        private GPUAnimationFrame _currentGPUAnimationFrameInfo;

        private int _currentGPUAnimationInitialFrame;

        private float _animationSpeed = 1;

        private float _animationOffset = 0;

        public string[] AnimationNames
        {
            get
            {
                if (_gpuAnimationDataScriptableObject == null)
                {
                    return new string[] { };
                }
                List<string> acc = new List<string>();
                for (int i = 0; i < _gpuAnimationDataScriptableObject.GPUAnimationFrames.Count; i++)
                {
                    acc.Add(_gpuAnimationDataScriptableObject.GPUAnimationFrames[i].AnimationName);
                }
                return acc.ToArray();
            }
        }

        private bool _isRuntime = true;

        void Awake()
        {
            _meshRenderer = GetComponent<MeshRenderer>();
            _materialPropertyBlock = new MaterialPropertyBlock();

            if (_gpuAnimationDataScriptableObject == null)
            {
                return;
            }

            UpdateFrameInfo(_gpuAnimationDataScriptableObject.GPUAnimationFrames[_currentGPUAnimationFrameIndex]);

            // for debug
            // Debug.Log(string.Format(
            //     "[GPUAnimationController] current animation data - initial frame: {0}, duration: {1}, vertex count: {2}, texture width {3}, texture height {4}",
            //     _currentGPUAnimationInitialFrame,
            //     _currentGPUAnimationFrameInfo.Frames,
            //     _gpuAnimationDataScriptableObject.VertexCount,
            //     _gpuAnimationDataScriptableObject.TextureWidth,
            //     _gpuAnimationDataScriptableObject.TextureHeight
            // ));
        }

        void UpdateFrameInfo(GPUAnimationFrame frameInfo)
        {
            _currentGPUAnimationFrameInfo = frameInfo;
            _currentGPUAnimationInitialFrame = 0;
            if (_currentGPUAnimationFrameIndex != 0)
            {
                for (int i = 0; i < _currentGPUAnimationFrameIndex; i++)
                {
                    _currentGPUAnimationInitialFrame += _gpuAnimationDataScriptableObject.GPUAnimationFrames[i].Frames;
                }
                // TODO: -1 の必要ある？
                // _currentGPUAnimationInitialFrame -= 1;
            }

        }

        public void PlayAnimation(string name)
        {
            GPUAnimationFrame targetFrame = _gpuAnimationDataScriptableObject.GPUAnimationFrames.Find(elem => name == elem.AnimationName);
            if (targetFrame == null)
            {
                Debug.LogError(string.Format(
                    "[GPUAnimationController] animation is not found - obj name: {0}, animation name: {1}",
                    gameObject.name,
                    name
                ));
                return;
            }
            UpdateFrameInfo(targetFrame);
        }

        public void PlayAnimation(int index)
        {
            GPUAnimationFrame targetFrame = _gpuAnimationDataScriptableObject.GPUAnimationFrames[index];
            if (targetFrame == null)
            {
                Debug.LogError(string.Format(
                    "[GPUAnimationController] animation is not found - obj name: {0}, animation name: {1}",
                    gameObject.name,
                    name
                ));
                return;
            }
            UpdateFrameInfo(targetFrame);
        }

        public void SetAnimationSpeed(float speed)
        {
            _animationSpeed = speed;
        }

        public void SetAnimationOffset(float offset)
        {
            _animationOffset = offset;
        }

        public void SetIsRuntime(bool flag)
        {
            _isRuntime = flag;
        }

        void FixedUpdate()
        {
            if (!_isRuntime || _gpuAnimationDataScriptableObject == null)
            {
                return;
            }

            _meshRenderer.GetPropertyBlock(_materialPropertyBlock);
            _materialPropertyBlock.SetFloat("_VertexCount", (float)_gpuAnimationDataScriptableObject.VertexCount);
            _materialPropertyBlock.SetFloat("_AnimationSpeed", _animationSpeed);
            _materialPropertyBlock.SetFloat("_BakedAnimationTimeOffset", _animationOffset);
            _materialPropertyBlock.SetFloat("_BakedAnimationFPS", (float)_gpuAnimationDataScriptableObject.FPS);
            _materialPropertyBlock.SetFloat("_BakedAnimationTotalDuration", _gpuAnimationDataScriptableObject.TotalDuration);
            _materialPropertyBlock.SetFloat("_BakedAnimationTotalFrames", _gpuAnimationDataScriptableObject.TotalFrames);
            _materialPropertyBlock.SetFloat("_BakedCurrentAnimationInitialFrame", _currentGPUAnimationInitialFrame);
            _materialPropertyBlock.SetFloat("_BakedCurrentAnimationFrames", _currentGPUAnimationFrameInfo.Frames);
            _materialPropertyBlock.SetFloat("_BakedTextureWidth", (float)_gpuAnimationDataScriptableObject.TextureWidth);
            _materialPropertyBlock.SetFloat("_BakedTextureHeight", (float)_gpuAnimationDataScriptableObject.TextureHeight);
            // Debug.Log(_gpuAnimationDataScriptableObject.TotalDuration);
            _meshRenderer.SetPropertyBlock(_materialPropertyBlock);
        }

        public void SetAnimationData(GPUAnimationDataScriptableObject data)
        {
            _gpuAnimationDataScriptableObject = data;
        }
    }
}