using UnityEngine;

namespace Demo
{
    public class CameraController : MonoBehaviour
    {
        [SerializeField]
        private Transform _rotateAnchor;

        [SerializeField]
        private float _rotateSpeed = 1f;

        void Update()
        {
            _rotateAnchor.Rotate(new Vector3(
                0,
                _rotateSpeed * Time.deltaTime,
                0
            ));
        }
    }
}