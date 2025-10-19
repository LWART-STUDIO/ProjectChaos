using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Serialization;

namespace Game.Prefabs.Skills.Active
{
    public class ProjectileVfxManager : MonoBehaviour
    {
        [SerializeField] private ParticleSystem _particlePrefab; 
        private List<Projectile> _trackedProjectiles = new();
        private List<ParticleSystem> _particleSystems = new();
        private ParticleSystem.Particle[] _cachedParticles = new ParticleSystem.Particle[0];

        void Awake()
        {
            if (_particlePrefab != null)
            {
                ParticleSystem psInstance = Instantiate(_particlePrefab, transform); // лучше в transform

                // Отключаем эмиссию у всех систем
                var allSystems = psInstance.GetComponentsInChildren<ParticleSystem>();
                foreach (var ps in allSystems)
                {
                    var emission = ps.emission;
                    emission.enabled = false;
                    ps.Simulate(0, true, true);
                }

                _particleSystems.AddRange(allSystems);
            }
        }
        public void TrackProjectile(Projectile proj)
        {
            if (!_trackedProjectiles.Contains(proj))
                _trackedProjectiles.Add(proj);
        }

        void FixedUpdate()
        {
            if (_trackedProjectiles.Count == 0 || _particleSystems.Count == 0)
                return;

            // Увеличиваем кэш при необходимости
            if (_cachedParticles.Length < _trackedProjectiles.Count)
                System.Array.Resize(ref _cachedParticles, _trackedProjectiles.Count);

            int activeCount = 0;
            foreach (var proj in _trackedProjectiles)
            {
                if (proj.gameObject.activeInHierarchy)
                {
                    var p = _cachedParticles[activeCount];
                    p.position = proj.transform.position;
                    p.startSize =Random.Range(Mathf.Max(0,proj.Size-0.5f), proj.Size+0.5f); 
                    p.rotation3D = Vector3.forward * Random.Range(0f, 360f);
                    p.startLifetime = float.MaxValue;
                    p.remainingLifetime = float.MaxValue;
                    p.velocity = Vector3.zero;
                    p.startColor = Color.white; // или настрой по желанию
                    _cachedParticles[activeCount] = p;
                    activeCount++;
                }
            }

            if (activeCount > 0)
            {
                foreach (var ps in _particleSystems)
                {
                    ps.SetParticles(_cachedParticles, activeCount);
                }
            }

            // Убираем неактивные снаряды
            _trackedProjectiles.RemoveAll(p => !p.gameObject.activeInHierarchy);
        }
    }
}
