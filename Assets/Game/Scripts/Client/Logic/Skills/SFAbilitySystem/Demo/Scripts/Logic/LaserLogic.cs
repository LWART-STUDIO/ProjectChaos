using SFAbilitySystem.Demo.Abilities;
using SFAbilitySystem.Demo.Interfaces;
using SFAbilitySystem.Demo.Logic;
using SFAbilitySystem.Demo.Spawnables;
using System;
using System.Collections;
using Game.Scripts.Client.Logic.Skills;
using UnityEngine;

namespace SFAbilitySystem.Demo.Cards
{
    /// <summary>
    /// Handles the execution logic for laser beam abilities, including:
    /// - Beam rendering and collision detection
    /// - Damage-over-time application
    /// - Visual effects management
    /// </summary>
    public class LaserLogic : ActiveLogicBase<LaserAbility>, IAbilityInject
    {
        [Header("Laser Settings")]
        [Tooltip("Prefab containing the laser beam visual and collision components")]
        [SerializeField] private LaserBeam laserPrefab;

        [Tooltip("Particle system played during laser charge-up")]
        [SerializeField] private ParticleSystem chargeParticles;

        [Tooltip("Particle system played at laser impact point")]
        [SerializeField] private ParticleSystem impactParticles;

        private Transform spawnPoint;          // Laser origin point (from injected camera)
        private LaserBeam activeLaser;         // Current active laser instance
        private Coroutine laserRoutine;        // Active laser coroutine
        private Camera playerCamera;           // Reference to player camera
        private float lastDamageTickTime;      // Last damage application time

        /// <summary>
        /// Specifies Camera as the required dependency for aiming
        /// </summary>
        public Type GetDependencyType() => typeof(Camera);

        /// <summary>
        /// Receives and stores the player camera reference
        /// </summary>
        public void Inject(UnityEngine.Object instance)
        {
            playerCamera = (Camera)instance;
            spawnPoint = playerCamera.transform;
        }

        /// <summary>
        /// Initiates the laser firing sequence:
        /// 1. Plays charge-up effect
        /// 2. Spawns and maintains laser beam
        /// 3. Handles impact detection and damage
        /// </summary>
        protected override void ExecuteAbility()
        {
            if (laserPrefab == null || spawnPoint == null)
            {
                Debug.LogError("Laser components not properly initialized!");
                return;
            }

            // Clean up any existing laser
            if (laserRoutine != null)
            {
                StopCoroutine(laserRoutine);
                CleanupLaser();
            }

            laserRoutine = StartCoroutine(LaserRoutine());
        }

        /// <summary>
        /// Coroutine that manages the full laser lifecycle
        /// </summary>
        private IEnumerator LaserRoutine()
        {
            lastDamageTickTime = 0f;

            // Charge-up phase
            if (chargeParticles != null)
            {
                chargeParticles.transform.position = spawnPoint.position;
                chargeParticles.Play();
                yield return new WaitForSeconds(0.2f); // Brief charge time
            }

            // Initialize laser beam
            activeLaser = Instantiate(laserPrefab, spawnPoint.position, spawnPoint.rotation);
            activeLaser.Initialize(_activeAbilityBase.damage, _activeAbilityBase.range);

            float duration = _activeAbilityBase.duration;
            float timer = 0f;

            // Active laser phase
            while (timer < duration)
            {
                if (activeLaser == null) yield break;

                UpdateLaserTransform();
                HandleBeamCollision();

                timer += Time.deltaTime;
                yield return null;
            }

            CleanupLaser();
        }

        /// <summary>
        /// Updates the laser's position and rotation to follow the aim point
        /// </summary>
        private void UpdateLaserTransform()
        {
            activeLaser.transform.position = spawnPoint.position;
            activeLaser.transform.rotation = spawnPoint.rotation;
        }

        /// <summary>
        /// Handles collision detection and impact effects
        /// </summary>
        private void HandleBeamCollision()
        {
            if (Physics.Raycast(spawnPoint.position, spawnPoint.forward, out RaycastHit hit, _activeAbilityBase.range))
            {
                activeLaser.UpdateBeam(hit.point);
                UpdateImpactEffects(hit);

                if (ShouldApplyDamageTick())
                {
                    ApplyTickDamage(hit.collider);
                    lastDamageTickTime = Time.time;
                }
            }
            else
            {
                activeLaser.UpdateBeam(spawnPoint.position + spawnPoint.forward * _activeAbilityBase.range);
                StopImpactEffects();
            }
        }

        /// <summary>
        /// Manages impact particle effects at collision point
        /// </summary>
        private void UpdateImpactEffects(RaycastHit hit)
        {
            if (impactParticles == null) return;

            if (!impactParticles.isPlaying)
                impactParticles.Play();

            impactParticles.transform.position = hit.point;
            impactParticles.transform.rotation = Quaternion.LookRotation(hit.normal);
        }

        /// <summary>
        /// Stops impact effects when beam isn't hitting anything
        /// </summary>
        private void StopImpactEffects()
        {
            if (impactParticles != null && impactParticles.isPlaying)
            {
                impactParticles.Stop();
            }
        }

        /// <summary>
        /// Determines if a damage tick should be applied based on tick interval
        /// </summary>
        private bool ShouldApplyDamageTick() =>
            Time.time - lastDamageTickTime >= _activeAbilityBase.damageTickInterval;

        /// <summary>
        /// Applies damage to the target (implementation pending)
        /// </summary>
        private void ApplyTickDamage(Collider targetCollider)
        {
            // TODO: Implement damage application logic
            Debug.Log($"Applying laser tick damage to {targetCollider.name}");
        }

        /// <summary>
        /// Cleans up laser effects and instances
        /// </summary>
        private void CleanupLaser()
        {
            if (activeLaser != null)
            {
                activeLaser.FadeOut(0.5f);
                Destroy(activeLaser.gameObject, 0.5f);
            }

            if (impactParticles != null) impactParticles.Stop();
            if (chargeParticles != null) chargeParticles.Stop();
        }

        private void OnDestroy()
        {
            if (laserRoutine != null)
            {
                StopCoroutine(laserRoutine);
            }
            CleanupLaser();
        }
    }
}