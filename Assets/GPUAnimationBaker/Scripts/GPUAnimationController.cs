using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

namespace GPUAnimationBaker
{
    [RequireComponent(typeof(MeshRenderer))]
    public class GPUAnimationController : MonoBehaviour
    {
        // ----------------------------------------------------------------------------------
        // serialize
        // ----------------------------------------------------------------------------------

        [SerializeField]
        private MeshRenderer _meshRenderer;

        [SerializeField]
        private MeshFilter _meshFilter;

        // [SerializeField]
        // NOTE:
        // - srpbatcherがinstancedPropsに対応していない可能性がある = オブジェクトごとにインスタンスを変えることができない？
        // - ので、一旦強制true
        private bool _enabledGPUInstancing = true;

        [SerializeField]
        private Color _instanceBaseColor = Color.white;

        [Space(13)]
        [ReadOnly, SerializeField]
        private GPUAnimationDataScriptableObject _gpuAnimationDataScriptableObject;

        [SerializeField, HideInInspector]
        private int _currentGPUAnimationFrameIndex = 0;

        // ----------------------------------------------------------------------------------
        // unity engine
        // ----------------------------------------------------------------------------------

        // TODO: bakerでaddcomponentした時も呼ばれるのでうまいこと回避したい
        void Awake()
        {
            // _materialInstance = _meshRenderer.sharedMaterial;
            if (_meshRenderer == null)
            {
                _meshRenderer = GetComponent<MeshRenderer>();
            }

            if (_meshFilter == null)
            {
                _meshFilter = GetComponent<MeshFilter>();
            }

            if (_enabledGPUInstancing)
            {
                _materialPropertyBlock = new MaterialPropertyBlock();
            }
            else
            {
                _materialInstance = _meshRenderer.material;
                // _meshRenderer.material = _materialInstance;
            }

            if (_gpuAnimationDataScriptableObject == null)
            {
                return;
            }

            UpdateFrameInfo(_gpuAnimationDataScriptableObject.GPUAnimationFrames[_currentGPUAnimationFrameIndex]);
            PlayAnimation(_currentGPUAnimationFrameIndex);

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

        /// <summary>
        /// 
        /// </summary>
        void Update()
        {
            if (!_isRuntime || _gpuAnimationDataScriptableObject == null)
            {
                return;
            }

            UpdateMaterial();
            UpdateLODMesh();
        }

        // ----------------------------------------------------------------------------------
        // public
        // ----------------------------------------------------------------------------------

        /// <summary>
        /// 
        /// </summary>
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

        // public void Setup()
        // {
        //     _meshRenderer = GetComponent<MeshRenderer>();
        // }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="name"></param>
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

        /// <summary>
        /// 
        /// </summary>
        /// <param name="index"></param>
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

        /// <summary>
        /// 
        /// </summary>
        /// <param name="speed"></param>
        public void SetAnimationSpeed(float speed)
        {
            _animationSpeed = speed;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="offset"></param>
        public void SetAnimationOffset(float offset)
        {
            _animationOffset = offset;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="flag"></param>
        public void SetIsRuntime(bool flag)
        {
            _isRuntime = flag;
        }


        /// <summary>
        /// 
        /// </summary>
        /// <param name="data"></param>
        public void SetAnimationData(GPUAnimationDataScriptableObject data)
        {
            _gpuAnimationDataScriptableObject = data;
        }

        // ----------------------------------------------------------------------------------
        // private 
        // ----------------------------------------------------------------------------------

        private MaterialPropertyBlock _materialPropertyBlock;
        private Material _materialInstance;

        private GPUAnimationFrame _currentGPUAnimationFrameInfo;

        private int _currentGPUAnimationInitialFrame;

        private float _animationSpeed = 1;

        private float _animationOffset = 0;

        private bool _isRuntime = true;

        /// <summary>
        /// 
        /// </summary>
        /// <param name="frameInfo"></param>
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

        /// <summary>
        /// 
        /// </summary>
        void UpdateMaterial()
        {
            if (_enabledGPUInstancing)
            {
                _meshRenderer.GetPropertyBlock(_materialPropertyBlock);
                _materialPropertyBlock.SetFloat("_BoneCount", _gpuAnimationDataScriptableObject.BoneCount);
                _materialPropertyBlock.SetFloat("_AnimationSpeed", _animationSpeed);
                _materialPropertyBlock.SetFloat("_BakedAnimationTimeOffset", _animationOffset);
                _materialPropertyBlock.SetFloat("_BakedAnimationFPS", (float)_gpuAnimationDataScriptableObject.FPS);
                _materialPropertyBlock.SetFloat("_BakedAnimationTotalDuration", _gpuAnimationDataScriptableObject.TotalDuration);
                _materialPropertyBlock.SetFloat("_BakedAnimationTotalFrames", _gpuAnimationDataScriptableObject.TotalFrames);
                _materialPropertyBlock.SetFloat("_BakedCurrentAnimationInitialFrame", _currentGPUAnimationInitialFrame);
                _materialPropertyBlock.SetFloat("_BakedCurrentAnimationFrames", _currentGPUAnimationFrameInfo.Frames);
                _materialPropertyBlock.SetFloat("_BakedTextureWidth", (float)_gpuAnimationDataScriptableObject.TextureWidth);
                _materialPropertyBlock.SetFloat("_BakedTextureHeight", (float)_gpuAnimationDataScriptableObject.TextureHeight);
                _materialPropertyBlock.SetColor("_CustomColor", _instanceBaseColor);
                _meshRenderer.SetPropertyBlock(_materialPropertyBlock);
            }
            else
            {
                // _materialInstance.SetFloat("_VertexCount", (float)_gpuAnimationDataScriptableObject.VertexCount);
                _materialInstance.SetFloat("_BoneCount", _gpuAnimationDataScriptableObject.BoneCount);
                _materialInstance.SetFloat("_AnimationSpeed", _animationSpeed);
                _materialInstance.SetFloat("_BakedAnimationTimeOffset", _animationOffset);
                _materialInstance.SetFloat("_BakedAnimationFPS", (float)_gpuAnimationDataScriptableObject.FPS);
                _materialInstance.SetFloat("_BakedAnimationTotalDuration", _gpuAnimationDataScriptableObject.TotalDuration);
                _materialInstance.SetFloat("_BakedAnimationTotalFrames", _gpuAnimationDataScriptableObject.TotalFrames);
                _materialInstance.SetFloat("_BakedCurrentAnimationInitialFrame", _currentGPUAnimationInitialFrame);
                _materialInstance.SetFloat("_BakedCurrentAnimationFrames", _currentGPUAnimationFrameInfo.Frames);
                _materialInstance.SetFloat("_BakedTextureWidth", (float)_gpuAnimationDataScriptableObject.TextureWidth);
                _materialInstance.SetFloat("_BakedTextureHeight", (float)_gpuAnimationDataScriptableObject.TextureHeight);
                _materialInstance.SetColor("_CustomColor", _instanceBaseColor);
                // Debug.Log(_gpuAnimationDataScriptableObject.TotalDuration);
            }
        }

        /// <summary>
        /// 
        /// </summary>
        void UpdateLODMesh()
        {
            if (!Camera.main)
            {
                return;
            }

            // LODなし
            if (_gpuAnimationDataScriptableObject.GPUAnimationMeshLODSettings.Count < 2)
            {
                return;
            }

            Mesh targetMesh = _gpuAnimationDataScriptableObject.GPUAnimationMeshLODSettings[0].LODMesh;

            var targetCamera = Camera.main.transform.position;
            
            var sqrDistanceToCamera = (targetCamera - transform.position).sqrMagnitude;
            for (int i = 1; i < _gpuAnimationDataScriptableObject.GPUAnimationMeshLODSettings.Count; i++)
            {
                var td = _gpuAnimationDataScriptableObject.GPUAnimationMeshLODSettings[i].ThresholdDistance;
                var sqrThreshold = td * td;
                if (sqrDistanceToCamera > sqrThreshold)
                {
                    targetMesh = _gpuAnimationDataScriptableObject.GPUAnimationMeshLODSettings[i].LODMesh;
                }
                else
                {
                    break;
                }
            }

            if (_meshFilter.sharedMesh != targetMesh)
            {
                _meshFilter.sharedMesh = targetMesh;
            }
        }
    }
}