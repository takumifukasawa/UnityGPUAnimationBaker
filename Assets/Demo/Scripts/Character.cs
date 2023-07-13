using GPUAnimationBaker;
using TMPro;
using UnityEngine;
using UnityEngine.UIElements;

namespace Demo
{
    public class Character : MonoBehaviour
    {
        [SerializeField]
        private GPUAnimationController _gpuAnimationController;

        [SerializeField]
        private Rigidbody _rigidbody;

        [SerializeField]
        private float _moveMinSpeed = 0.05f;

        [SerializeField]
        private float _moveMaxSpeed = 0.1f;

        [SerializeField]
        private float _toggleStateMinInterval = 1.2f;

        [SerializeField]
        private float _toggleStateMaxInterval = 1.8f;

        private float _currentToggleStateInterval;

        private bool _isMoving = false;
        private float _currentMoveSpeed;

        private float _lastToggleStateTime = -Mathf.Infinity;

        private Vector3 _forward;

        void Start()
        {
            UpdateInterval();
            if (Random.Range(0f, 1f) > 0.5f)
            {
                Idle();
            }
            else
            {
                Run();
            }
        }

        void Update()
        {
            if (Time.time - _lastToggleStateTime > _currentToggleStateInterval)
            {
                UpdateInterval();
                ToggleState();
            }

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
        }
    }
}