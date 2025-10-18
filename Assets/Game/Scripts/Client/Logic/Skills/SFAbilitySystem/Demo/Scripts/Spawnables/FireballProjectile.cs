using UnityEngine;

namespace SFAbilitySystem.Demo.Spawnables
{
    /// <summary>
    /// Handles fireball projectile behavior including:
    /// - Damage application on impact
    /// - Visual effects management
    /// - Collision detection
    /// </summary>
    public class FireballProjectile : MonoBehaviour
    {
        [Header("Settings")]
        [Tooltip("Base damage value applied on impact")]
        private float damage;

        [Header("Visual Effects")]
        [Tooltip("Particle system played on impact")]
        [SerializeField] private ParticleSystem impactParticles;

        /// <summary>
        /// Initializes the projectile's damage value
        /// </summary>
        /// <param name="amount">Damage to apply on impact</param>
        public void SetDamage(float amount)
        {
            damage = Mathf.Max(0, amount); // Ensure non-negative damage
        }

        /// <summary>
        /// Handles collision events and triggers impact effects
        /// </summary>
        private void OnCollisionEnter(Collision collision)
        {
            // Detach particles to allow them to finish playing
            if (impactParticles != null)
            {
                impactParticles.transform.SetParent(null);
                impactParticles.Stop(true, ParticleSystemStopBehavior.StopEmitting);
                Destroy(impactParticles.gameObject, impactParticles.main.duration);
            }

            // Cleanup projectile
            Destroy(gameObject);

            // TODO: Add explosion effect and area damage
            // TODO: Implement object pooling for better performance
        }

        /// <summary>
        /// Ensures proper cleanup of detached particles if projectile is destroyed prematurely
        /// </summary>
        private void OnDestroy()
        {
            if (impactParticles != null && impactParticles.transform.parent == null)
            {
                Destroy(impactParticles.gameObject);
            }
        }
    }
}