using SFAbilitySystem.Core;
using System.Collections;
using UnityEngine;
using UnityEngine.UI;

namespace SFAbilitySystem.Demo.UI
{
    /// <summary>
    /// Handles the visual animation sequence for UI cards including:
    /// - Smooth scale up/down transitions
    /// - Continuous wobble effect
    /// - Timed animation sequence
    /// </summary>
    public class AnimatedUICard : MonoBehaviour
    {
        [Header("Animation Timing")]
        [Tooltip("Duration of the scale-up animation in seconds")]
        [SerializeField] private float _scaleUpDuration = 0.5f;

        [Tooltip("Duration of the wobble animation in seconds")]
        [SerializeField] private float _wobbleDuration = 3f;

        [Tooltip("Duration of the scale-down animation in seconds")]
        [SerializeField] private float _scaleDownDuration = 0.5f;

        [Header("Wobble Settings")]
        [Tooltip("Maximum rotation angle for wobble effect in degrees")]
        [SerializeField] private float _wobbleAngle = 5f;

        [Tooltip("Speed of the wobble oscillation")]
        [SerializeField] private float _wobbleSpeed = 8f;

        [Header("UI References")]
        [Tooltip("Image component that displays the card icon")]
        [SerializeField] private Image image;

        private RectTransform _rectTransform;
        private Vector3 _originalScale;
        private Quaternion _originalRotation;
        private float _animationTimer = 0f;

        /// <summary>
        /// Initializes the card with data and starts the animation sequence
        /// </summary>
        /// <param name="card">CardData containing the visual information</param>
        public void Init(CardData card)
        {
            if (image == null)
            {
                Debug.LogError("Image reference is not set in AnimatedUICard", this);
                return;
            }

            image.sprite = card.abilityIcon;

            _rectTransform = GetComponent<RectTransform>();
            if (_rectTransform == null)
            {
                Debug.LogError("No RectTransform found on this GameObject", this);
                return;
            }

            _originalScale = _rectTransform.localScale;
            _originalRotation = _rectTransform.localRotation;
            _rectTransform.localScale = Vector3.zero;

            StartCoroutine(PlayAnimationSequence());
        }

        /// <summary>
        /// Plays the complete animation sequence:
        /// 1. Scale up with easing
        /// 2. Wobble effect
        /// 3. Scale down with easing
        /// </summary>
        private IEnumerator PlayAnimationSequence()
        {
            _animationTimer = 0f;
            float totalDuration = GetTotalTime();

            while (_animationTimer < totalDuration)
            {
                _animationTimer += Time.deltaTime;

                UpdateScalePhase();
                UpdateWobbleEffect();

                yield return null;
            }

            Destroy(gameObject);
        }

        /// <summary>
        /// Handles the scale animation based on current phase
        /// </summary>
        private void UpdateScalePhase()
        {
            if (_animationTimer <= _scaleUpDuration)
            {
                // Scale up with easing
                float progress = EaseOutBack(Mathf.Clamp01(_animationTimer / _scaleUpDuration));
                _rectTransform.localScale = Vector3.Lerp(Vector3.zero, _originalScale, progress);
            }
            else if (_animationTimer > _scaleUpDuration + _wobbleDuration)
            {
                // Scale down with easing
                float scaleDownProgress = EaseOutBack(Mathf.Clamp01(
                    (_animationTimer - (_scaleUpDuration + _wobbleDuration)) / _scaleDownDuration));
                _rectTransform.localScale = Vector3.Lerp(_originalScale, Vector3.zero, scaleDownProgress);
            }
            else
            {
                // Maintain full scale during wobble phase
                _rectTransform.localScale = _originalScale;
            }
        }

        /// <summary>
        /// Updates the continuous wobble rotation effect
        /// </summary>
        private void UpdateWobbleEffect()
        {
            float rotation = Mathf.Sin(Time.time * _wobbleSpeed) * _wobbleAngle;
            _rectTransform.localRotation = _originalRotation * Quaternion.Euler(0, 0, rotation);
        }

        /// <summary>
        /// Easing function for more dynamic animation
        /// </summary>
        private float EaseOutBack(float t)
        {
            const float c1 = 1.70158f;
            const float c3 = c1 + 1f;
            return 1f + c3 * Mathf.Pow(t - 1f, 3f) + c1 * Mathf.Pow(t - 1f, 2f);
        }

        /// <summary>
        /// Calculates the total duration of the animation sequence
        /// </summary>
        /// <returns>Total animation time in seconds</returns>
        public float GetTotalTime()
        {
            return _scaleUpDuration + _wobbleDuration + _scaleDownDuration;
        }
    }
}