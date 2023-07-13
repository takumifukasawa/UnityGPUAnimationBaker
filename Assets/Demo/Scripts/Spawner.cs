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
        private Toggle _physicsToggle;

        [SerializeField]
        private Toggle _contactToggle;

        [SerializeField]
        private Button _respawnButton;

        [Space(13)]
        [Header("Settings")]
        [SerializeField]
        private Character _characterPrefab;

        [SerializeField]
        private int _initalSpawnNum = 100;

        [SerializeField]
        private Vector3 _spawnMinPosition = Vector3.zero;

        [SerializeField]
        private Vector3 _spawnMaxPosition = Vector3.one;

        [SerializeField]
        private float _spawnMinScale = 0.8f;

        [SerializeField]
        private float _spawnMaxScale = 1f;

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
            _respawnButton.onClick.AddListener(() => { Respawn(_spawnNum, _physicsToggle.isOn, _contactToggle.isOn); });

            Respawn(_spawnNum, _physicsToggle.isOn, _contactToggle.isOn);
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="spawnNum"></param>
        void Respawn(int spawnNum, bool enabledPhysics, bool enabledContact)
        {
            foreach (Transform child in _spawnAnchor)
            {
                Destroy(child.gameObject);
            }

            for (int i = 0; i < spawnNum; i++)
            {
                var obj = Instantiate(_characterPrefab);
                obj.Initialize(enabledPhysics);
                var p = new Vector3(
                    Random.Range(_spawnMinPosition.x, _spawnMaxPosition.x),
                    Random.Range(_spawnMinPosition.y, _spawnMaxPosition.y),
                    Random.Range(_spawnMinPosition.z, _spawnMaxPosition.z)
                );
                var ss = Random.Range(_spawnMinScale, _spawnMaxScale);
                var s = new Vector3(ss, ss, ss);
                obj.transform.parent = _spawnAnchor;
                obj.transform.position = p;
                obj.transform.localScale = s;
            }

            var layer = LayerMask.NameToLayer("Character");
            if (enabledContact)
            {
                Physics.IgnoreLayerCollision(layer, layer, false);
            }
            else
            {
                Physics.IgnoreLayerCollision(layer, layer, true);
            }
        }
    }
}