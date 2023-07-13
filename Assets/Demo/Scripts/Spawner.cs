using GPUAnimationBaker;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace Demo
{
    public class Spawner : MonoBehaviour
    {
        [SerializeField]
        private Transform _spawnAnchor;

        [SerializeField]
        private Slider _spawnNumSlider;

        [SerializeField]
        private TextMeshProUGUI _spawnNumText;

        [SerializeField]
        private Button _respawnButton;

        [Space(13)]
        [Header("Settings")]
        [SerializeField]
        private GPUAnimationController _gpuAnimationControllerPrefab;

        [SerializeField]
        private int _initalSpawnNum = 100;

        [SerializeField]
        private Vector3 _spawnMinPosition = Vector3.zero;

        [SerializeField]
        private Vector3 _spawnMaxPosition = Vector3.one;

        [SerializeField]
        private Vector3 _spawnMinScale = Vector3.one;

        [SerializeField]
        private Vector3 _spawnMaxScale = Vector3.one;

        private int _spawnNum;

        /// <summary>
        /// 
        /// </summary>
        void Start()
        {
            _spawnNum = _initalSpawnNum;

            _spawnNumSlider.value = _spawnNum;
            _spawnNumText.text = _spawnNum.ToString();

            _spawnNumSlider.onValueChanged.AddListener((value) =>
            {
                _spawnNum = (Mathf.FloorToInt(value));
                _spawnNumText.text = _spawnNum.ToString();
            });
            _respawnButton.onClick.AddListener(() => { Respawn(_spawnNum); });

            Respawn(_spawnNum);
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="spawnNum"></param>
        void Respawn(int spawnNum)
        {
            foreach (Transform child in _spawnAnchor)
            {
                Destroy(child.gameObject);
            }

            for (int i = 0; i < spawnNum; i++)
            {
                var obj = Instantiate(_gpuAnimationControllerPrefab);
                var p = new Vector3(
                    Random.Range(_spawnMinPosition.x, _spawnMaxPosition.x),
                    Random.Range(_spawnMinPosition.y, _spawnMaxPosition.y),
                    Random.Range(_spawnMinPosition.z, _spawnMaxPosition.z)
                );
                var s = new Vector3(
                    Random.Range(_spawnMinScale.x, _spawnMaxScale.x),
                    Random.Range(_spawnMinScale.y, _spawnMaxScale.y),
                    Random.Range(_spawnMinScale.z, _spawnMaxScale.z)
                );
                obj.transform.parent = _spawnAnchor;
                obj.transform.position = p;
                obj.transform.localScale = s;
            }
        }
    }
}