using Game.Scripts.Client.Logic.Game;
using Game.Scripts.Client.Logic.Player;
using PurrNet;
using UnityEngine;

namespace Game.Scripts.Client.Logic.Events
{
    public class PassiveSkillGiver : NetworkBehaviour
    {
        [Header("Visual")]
        [SerializeField] private Transform _shield;
        [SerializeField] private GameObject _beam;
        [SerializeField] private GameObject _mainSphere;

        [Header("Settings")]
        [SerializeField] private float _collapseSpeed = 1f;
        [SerializeField] private float _riseSpeed = 2f;
        [SerializeField] private Vector3 _maxSize = new(8, 8, 8);
        [SerializeField] private float _checkRadius = 16f;
        [SerializeField] private LayerMask _playerMask;

        private readonly SyncVar<float> _currentValue = new(0);
        private readonly SyncVar<bool> _hasPlayers = new(false);
        private SyncVar<bool> _completed = new SyncVar<bool>(false);

        #region Network

        protected override void OnSpawned(bool asServer)
        {
            base.OnSpawned(asServer);

            if (!asServer)
                return;

            _currentValue.value = 0f;
            _completed.value = false;
            _hasPlayers.value = false;
            UpdateVisuals();
        }

        private void Update()
        {
            if (!isServer)
                return;
            if (!_completed.value)
                CheckPlayers();
            ProcessValue();
            UpdateVisuals();
        }

        #endregion

        #region Logic

        private void CheckPlayers()
        {
            var hits = Physics.OverlapSphere(
                transform.position,
                _checkRadius,
                _playerMask,
                QueryTriggerInteraction.Ignore
            );

            bool foundPlayer = false;

            foreach (var hit in hits)
            {
                if (hit.TryGetComponent<PlayerHealth>(out _))
                {
                    foundPlayer = true;
                    break;
                }
            }
            _hasPlayers.value = foundPlayer;
        }

        private void ProcessValue()
        {
            float speed = _hasPlayers.value ? _riseSpeed : _collapseSpeed;
            float direction = _hasPlayers.value ? 1f : -1f;

            _currentValue.value = Mathf.Clamp(
                _currentValue.value + direction * speed * Time.deltaTime,
                0f,
                _maxSize.x+0.1f
            );
            if (_currentValue.value >= _maxSize.x&&!_completed.value)
            {
                if(!InstanceHandler.TryGetInstance(out LevelManager levelManager))
                    return;
                levelManager.PassiveGrant();
                _completed.value = true;
            }
        }

        #endregion

        #region Visuals
        
        private void UpdateVisuals()
        {
            float t = _currentValue.value / _maxSize.x;

            _shield.localScale = Vector3.Lerp(Vector3.zero, _maxSize, t);

            if (!_completed.value)
                ShowBase();
            else
                HideBase();

            if (t >= 0.01f)
                ShowShield();
            else
                HideShield();
        }

        [ObserversRpc(requireServer: false)]
        private void ShowShield()
        {
            _shield.gameObject.SetActive(true);
        }

        [ObserversRpc(requireServer: false)]
        private void HideShield()
        {
            _shield.gameObject.SetActive(false);
        }

        [ObserversRpc(requireServer: false)]
        private void ShowBase()
        {
            _mainSphere.SetActive(true);
            _beam.SetActive(true);
        }

        [ObserversRpc(requireServer: false)]
        private void HideBase()
        {
            _mainSphere.SetActive(false);
            _beam.SetActive(false);
            _shield.gameObject.SetActive(false);
        }

        #endregion

#if UNITY_EDITOR
        private void OnDrawGizmosSelected()
        {
            Gizmos.color = Color.cyan;
            Gizmos.DrawWireSphere(transform.position, _checkRadius);
        }
#endif
    }
}
