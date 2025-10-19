using SFAbilitySystem.Demo.Abilities;
using SFAbilitySystem.Demo.Interfaces;
using SFAbilitySystem.Demo.Logic;
using SFAbilitySystem.Demo.Spawnables;
using System;
using UnityEngine;

namespace SFAbilitySystem.Demo.Cards
{
    /// <summary>
    /// Concrete implementation of fireball ability logic.
    /// Handles projectile spawning, damage application, and dependency injection.
    /// </summary>
    public class FireballLogic : ActiveLogicBase<FireballAbility>, IAbilityInject
    {
        [Header("Fireball Settings")]
        [Tooltip("Prefab reference for the fireball projectile")]
        [SerializeField] private FireballProjectile fireballPrefab;

        private Transform spawnPoint; // Injection point for projectile spawning

        /// <summary>
        /// Specifies the required dependency type for this ability
        /// </summary>
        /// <returns>Camera type used for aiming/spawning</returns>
        public Type GetDependencyType()
        {
            return typeof(Camera);
        }

        /// <summary>
        /// Receives and processes the injected dependency
        /// </summary>
        /// <param name="instance">Must be of Camera type</param>
        public void Inject(UnityEngine.Object instance)
        {
            spawnPoint = ((Camera)instance).transform;
        }

        /// <summary>
        /// Executes the core fireball ability logic:
        /// 1. Spawns projectile at camera position
        /// 2. Applies damage value from ability config
        /// 3. Launches projectile with configured speed
        /// </summary>
        protected override void ExecuteAbility()
        {
            // Validate required references
            if (fireballPrefab == null || spawnPoint == null)
            {
                Debug.LogError("Fireball prefab or spawn point not set!");
                return;
            }

            // Create and configure projectile
            FireballProjectile fireball = Instantiate(
                fireballPrefab,
                spawnPoint.position,
                spawnPoint.rotation
            );

            // Set damage from ability configuration
            fireball.SetDamage(_activeAbilityBase.damage);

            // Apply projectile physics
            Rigidbody rb = fireball.GetComponent<Rigidbody>();
            if (rb != null)
            {
                rb.linearVelocity = spawnPoint.forward * _activeAbilityBase.projectileSpeed;
            }
        }
    }
}