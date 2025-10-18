using UnityEngine;

namespace SFAbilitySystem.Demo.Spawnables
{
    /// <summary>
    /// Controls the visual representation and behavior of a laser beam effect.
    /// Handles beam rendering, target updating, and smooth fade-out effects.
    /// </summary>
    public class LaserBeam : MonoBehaviour
    {
        [Header("Rendering Components")]
        [Tooltip("Reference to the LineRenderer component that draws the beam")]
        [SerializeField] private LineRenderer lineRenderer;

        [Header("Visual Settings")]
        [Tooltip("Speed at which the beam fades out (alpha per second)")]
        [SerializeField] private float fadeSpeed = 2f;
        [Tooltip("Vertical offset for beam start position (for visual alignment)")]
        [SerializeField] private float verticalOffset = 0.1f;

        private float currentAlpha = 1f;    // Current transparency value
        private bool isFading = false;      // Fade state flag

        /// <summary>
        /// Initializes the laser beam with damage and range parameters
        /// </summary>
        /// <param name="damage">Damage value (for potential future use)</param>
        /// <param name="range">Maximum beam range in world units</param>
        public void Initialize(float damage, float range)
        {
            // Set up line renderer
            lineRenderer.positionCount = 2;
            UpdateBeam(transform.position + transform.forward * range);

            // Initialize visual properties
            ResetBeamAppearance();
        }

        /// <summary>
        /// Updates the beam's end point position
        /// </summary>
        /// <param name="endPoint">World position where the beam should terminate</param>
        public void UpdateBeam(Vector3 endPoint)
        {
            lineRenderer.SetPosition(0, transform.position - transform.up * verticalOffset);
            lineRenderer.SetPosition(1, endPoint);
        }

        /// <summary>
        /// Begins the beam fade-out sequence
        /// </summary>
        /// <param name="duration">Time in seconds for complete fade-out</param>
        public void FadeOut(float duration)
        {
            if (!isFading)
            {
                fadeSpeed = 1f / Mathf.Max(0.01f, duration); // Prevent division by zero
                isFading = true;
            }
        }

        /// <summary>
        /// Resets the beam to full visibility
        /// </summary>
        private void ResetBeamAppearance()
        {
            var color = lineRenderer.material.color;
            color.a = currentAlpha = 1f;
            lineRenderer.material.color = color;
            isFading = false;
        }

        /// <summary>
        /// Handles the fade-out effect in LateUpdate for smooth visual transitions
        /// </summary>
        private void LateUpdate()
        {
            if (!isFading || currentAlpha <= 0) return;

            // Update transparency
            currentAlpha = Mathf.Clamp01(currentAlpha - fadeSpeed * Time.deltaTime);

            var color = lineRenderer.material.color;
            color.a = currentAlpha;
            lineRenderer.material.color = color;

            // Cleanup when fully faded
            if (currentAlpha <= 0)
            {
                Destroy(gameObject);
            }
        }

        /// <summary>
        /// Ensures proper cleanup when destroyed
        /// </summary>
        private void OnDestroy()
        {
            // Additional cleanup logic can be added here if needed
        }
    }
}