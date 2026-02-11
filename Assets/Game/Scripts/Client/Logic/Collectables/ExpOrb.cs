using Game.Scripts.Client.Logic.Collectables;
using Game.Scripts.Client.Logic.Game;
using Game.Scripts.Client.Logic.Player;
using Game.Scripts.Services;
using Game.Scripts.Services.Audio;
using PurrNet;
using Sisus.Init;
using UnityEngine;

namespace Game.Scripts.Client.Logic.Colectables
{
    public enum ExpOrbState
    {
        Free,           // свободен
        MovingToTarget, // движется к цели (магнит или объединение)
        Combining       // в процессе объединения
    }
    public class ExpOrb : NetworkBehaviour
    {
        [SerializeField] private GameObject[] _expRender;
        [SerializeField] private Collider _expCollider;
        private GameObject _currentRender;

        private Transform _target;      // цель для магнита (игрок или главный орб)
        private float _magnetSpeed = 4f;
        private bool _isMagnetActive;
        public ExpOrbState State { get; private set; } = ExpOrbState.Free;
        private int _exp = 0;
        [SerializeField] private Rigidbody _rigidbody;

        protected override void OnSpawned(bool asServer)
        {
            base.OnSpawned(asServer);
            if (!asServer) return;
            ExpOrbManager.AllOrbs.Add(this);
        }

        [ServerRpc(requireOwnership:false)]
        public void SetUpExpServer(int exp)
        {
            _exp = exp;
            ChangeVisual(_exp);
        }

        [ObserversRpc(runLocally:true)]
        private void ChangeVisual(int exp)
        {
            _exp = exp;
            foreach (var render in _expRender)
                render.SetActive(false);

            if (_exp < 10)
                _currentRender = _expRender[0];
            else if (_exp < 50)
                _currentRender = _expRender[1];
            else if (_exp < 100)
                _currentRender = _expRender[2];
            else
                _currentRender = _expRender[3];

            _currentRender.SetActive(true);
        }
        
        [ServerRpc(requireOwnership: false)]
        public void SetMagnetTarget(Transform target)
        {
            // Не можем перезаписать, если уже в движении
            if (State != ExpOrbState.Free) return;

            _target = target;
            State = ExpOrbState.MovingToTarget; // Устанавливаем состояние
            //_expCollider.enabled = false;
        }

        private void Update()
        {
            if (!isServer) 
                return;

            if (State == ExpOrbState.MovingToTarget && _target != null)
            {
                var targetPosition = _target.position + Vector3.up;

                // Interpolate towards the target
                Vector3 newPosition = Vector3.Lerp(
                    transform.position,
                    targetPosition,
                    _magnetSpeed * Time.deltaTime
                );

                // Use MovePosition for proper physics integration (if using Rigidbody)
                _rigidbody.MovePosition(newPosition);

                if (Vector3.Distance(transform.position, targetPosition) < 0.1f)
                {
                    State = ExpOrbState.Free;
                }
            }
        }

        private void OnTriggerEnter(Collider other)
        {
            if (other.TryGetComponent(out PlayerHealth playerHealth))
            {
                AddExp();
            }
        }
        [ServerRpc(requireOwnership: false)]
        private void AddExp()
        {
            if (InstanceHandler.TryGetInstance(out LevelManager levelManager))
                levelManager.AddExp(_exp);
            AddExpVisual();
        }

        [ObserversRpc(runLocally: true)]
        public void AddExpVisual()
        {
            _expCollider.enabled = false;
            AudioService.instance.PlaySoundInPlace("ExpOrb",transform.position);
            _currentRender.SetActive(false);
            State = ExpOrbState.Combining; // помечаем как в процессе объединения
            DestroyOrbServer();
        }

        [ServerRpc(requireOwnership:false)]
        public void DestroyOrbServer()
        {
            if (!isSpawned) 
                return;
            if(!isServer)
                return;
            ExpOrbManager.AllOrbs.Remove(this);
            Destroy(gameObject);
        }

        public int GetExp() => _exp;

        protected override void OnDespawned(bool asServer)
        {
            base.OnDespawned(asServer);
            if (!asServer) return;
            ExpOrbManager.AllOrbs.Remove(this);
        }
    
    }
}
