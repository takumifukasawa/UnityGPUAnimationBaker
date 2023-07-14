using System.Collections.Generic;
using GPUAnimationBaker;
using TMPro;
using UnityEngine;
using UnityEngine.UIElements;

namespace Demo
{
    public class Character : MonoBehaviour
    {
        [SerializeField]
        private bool _initializeOnAwake = false;
        
        [SerializeField]
        private GPUAnimationController _gpuAnimationController;

        [SerializeField]
        private Rigidbody _rigidbody;

        [SerializeField]
        private Collider _collider;

        [SerializeField]
        private float _moveMinSpeed = 0.05f;

        [SerializeField]
        private float _moveMaxSpeed = 0.1f;

        [SerializeField]
        private float _toggleStateMinInterval = 1.2f;

        [SerializeField]
        private float _toggleStateMaxInterval = 1.8f;

        [Space(13)]
        [Header("case: moving by transform")]
        [SerializeField]
        private Vector3 _movableMinArea;

        [SerializeField]
        private Vector3 _movableMaxArea;

        private float _currentToggleStateInterval;

        private bool _isMoving = false;
        private float _currentMoveSpeed;

        private float _lastToggleStateTime = -Mathf.Infinity;

        private Vector3 _forward;

        private bool _isInitialized = false;

        void Awake()
        {
            if (_initializeOnAwake)
            {
                // dummy
                Initialize(false);
            }
        }
        
        /// <summary>
        /// 
        /// </summary>
        /// <param name="enabledPhysics"></param>
        public void Initialize(bool enabledPhysics)
        {
            _gpuAnimationController.Initialize();
            
            if (!enabledPhysics)
            {
                Destroy(_rigidbody);
                _collider.isTrigger = true;
            }

            // UpdateInterval();

            if (Random.Range(0f, 1f) > 0.5f)
            {
                Idle();
            }
            else
            {
                Run();
            }

            var colorPalette = new Vector4(
                    // .125f, .125f, .125f, .125f
                (float)Random.Range(0, 4) / 4f + 0.125f,
                (float)Random.Range(0, 4) / 4f + 0.125f,
                (float)Random.Range(0, 4) / 4f + 0.125f,
                (float)Random.Range(0, 4) / 4f + 0.125f
            );
            _gpuAnimationController.UpdateMaterialVector("_ColorPalette", colorPalette);

            _isInitialized = true;
        }

        /// <summary>
        /// 
        /// </summary>
        void Update()
        {
            if (!_isInitialized)
            {
                return;
            }

            if (Time.time - _lastToggleStateTime > _currentToggleStateInterval)
            {
                UpdateInterval();
                ToggleState();
            }

            if (_rigidbody)
            {
                if (_isMoving)
                {
                    _rigidbody.velocity = _forward * _currentMoveSpeed;
                    Quaternion rot = Quaternion.LookRotation(_forward, Vector3.up);
                    _rigidbody.MoveRotation(rot);
                }
                else
                {
                    _rigidbody.velocity = Vector3.zero;
                    _rigidbody.angularVelocity = Vector3.zero;
                }
            }
            else
            {
                if (_isMoving)
                {
                    var p = transform.position;
                    transform.position = p + _forward * _currentMoveSpeed * Time.deltaTime;
                    transform.position = new Vector3(
                        Mathf.Clamp(transform.position.x, _movableMinArea.x, _movableMaxArea.x),
                        Mathf.Clamp(transform.position.y, _movableMinArea.y, _movableMaxArea.y),
                        Mathf.Clamp(transform.position.z, _movableMinArea.z, _movableMaxArea.z)
                    );
                    Quaternion rot = Quaternion.LookRotation(_forward, Vector3.up);
                    transform.rotation = rot;
                }
            }
        }

        void UpdateInterval()
        {
            _lastToggleStateTime = Time.time;
            _currentToggleStateInterval = Random.Range(_toggleStateMinInterval, _toggleStateMaxInterval);
        }

        void ToggleState()
        {
            if (_isMoving)
            {
                Idle();
            }
            else
            {
                Run();
            }
        }

        void Run()
        {
            _isMoving = true;
            _gpuAnimationController.PlayAnimation("Run");
            // _gpuAnimationController.PlayAnimation(1);
            _forward = new Vector3(
                Random.Range(-1f, 1f),
                0f,
                Random.Range(-1f, 1f)
            ).normalized;
            _currentMoveSpeed = Random.Range(_moveMinSpeed, _moveMaxSpeed);
        }

        void Idle()
        {
            _isMoving = false;
            _gpuAnimationController.PlayAnimation("Idle");
            // _gpuAnimationController.PlayAnimation(0);
        }
    }
}