using System.Collections.Generic;
using Game.Prefabs.Skills.Active;
using UnityEngine;

namespace Game.Scripts.Client.Logic.Skills.Active
{
    public class ProjectileVfxManager : MonoBehaviour
    {
        public static ProjectileVfxManager Instance { get; private set; }
        [Tooltip("Список партикл-систем (например: след, искры, дым). Каждая будет отображать ВСЕ снаряды.")]
        [SerializeField] private List<ParticleSystem> _particlePrefabs;

        private List<ParticleSystem> _activeSystems = new();
        private List<Projectile> _trackedProjectiles = new();

        // Кэш частиц для каждой системы
        private ParticleSystem.Particle[][] _particlesCache;
        private int _maxParticles = 0;

        void Awake()
        {
            Instance = this;
            // Создаём по одной системе на каждый тип эффекта
            _activeSystems.Clear();
            foreach (var prefab in _particlePrefabs)
            {
                var ps = Instantiate(prefab, transform);
                ps.transform.localPosition = Vector3.zero;
                ps.transform.localRotation = Quaternion.identity;
                var emission = ps.emission;
                emission.enabled = false; // управляем вручную
                ps.Stop(true, ParticleSystemStopBehavior.StopEmittingAndClear);
                _activeSystems.Add(ps);
            }

            // Инициализируем кэш
            _particlesCache = new ParticleSystem.Particle[_activeSystems.Count][];
            for (int i = 0; i < _activeSystems.Count; i++)
            {
                _particlesCache[i] = new ParticleSystem.Particle[1000]; // начальный размер
            }
            _maxParticles = 1000;
        }
        public void RegisterProjectile(Projectile proj)
        {
            if (proj != null && !_trackedProjectiles.Contains(proj))
            {
                _trackedProjectiles.Add(proj);
            }
        }

        public void UnregisterProjectile(Projectile proj)
        {
            _trackedProjectiles.Remove(proj);
        }
        public void TrackProjectile(Projectile proj)
        {
            if (!_trackedProjectiles.Contains(proj))
                _trackedProjectiles.Add(proj);
        }

        void FixedUpdate()
        {
            var allProjectiles = FindObjectsByType<Projectile>(FindObjectsInactive.Include, FindObjectsSortMode.None);
            _trackedProjectiles.Clear();
            foreach (var proj in allProjectiles)
            {
                if (proj != null && proj.gameObject.activeInHierarchy)
                {
                    _trackedProjectiles.Add(proj);
                }
            }

            if (_trackedProjectiles.Count == 0 || _activeSystems.Count == 0)
            {
                ClearAllParticles();
                return;
            }

            int activeCount = 0;
            // Собираем активные снаряды
            for (int i = _trackedProjectiles.Count - 1; i >= 0; i--)
            {
                var proj = _trackedProjectiles[i];
                if (proj == null || !proj.gameObject.activeInHierarchy)
                {
                    _trackedProjectiles.RemoveAt(i);
                }
                else
                {
                    activeCount++;
                }
            }

            if (activeCount == 0)
            {
                ClearAllParticles();
                return;
            }

            // Увеличиваем кэш, если нужно
            if (activeCount > _maxParticles)
            {
                _maxParticles = Mathf.NextPowerOfTwo(activeCount * 2); // с запасом
                for (int i = 0; i < _particlesCache.Length; i++)
                {
                    System.Array.Resize(ref _particlesCache[i], _maxParticles);
                }
            }

            // Обновляем частицы для КАЖДОЙ системы
            for (int sysIndex = 0; sysIndex < _activeSystems.Count; sysIndex++)
            {
                var particles = _particlesCache[sysIndex];
                int written = 0;

                foreach (var proj in _trackedProjectiles)
                {
                    if (proj == null || !proj.gameObject.activeInHierarchy) continue;

                    var p = new ParticleSystem.Particle
                    {
                        position = proj.transform.position,
                        startSize = Random.Range(Mathf.Max(0, proj.Size - 0.5f), proj.Size + 0.5f),
                        rotation3D = Vector3.forward * Random.Range(0f, 360f),
                        startLifetime = float.MaxValue,
                        remainingLifetime = float.MaxValue,
                        velocity = Vector3.zero,
                        startColor = GetColorForSystem(sysIndex) // можно настроить
                    };
                    particles[written] = p;
                    written++;
                }

                // Применяем частицы к системе
                _activeSystems[sysIndex].SetParticles(particles, written);
            }
        }

        private Color GetColorForSystem(int index)
        {
            // Пример: разные цвета для разных систем
            switch (index % 3)
            {
                case 0: return Color.white;
                case 1: return Color.red;
                case 2: return Color.blue;
                default: return Color.white;
            }
        }

        private void ClearAllParticles()
        {
            foreach (var ps in _activeSystems)
            {
                ps.SetParticles(null, 0);
            }
        }

        // Опционально: вызывать при уничтожении
        void OnDestroy()
        {
            foreach (var ps in _activeSystems)
            {
                if (ps != null) Destroy(ps.gameObject);
            }
        }
    }
}