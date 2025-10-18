using SFAbilitySystem.Core;
using SFAbilitySystem.Demo.Cards;
using UnityEngine;

namespace SFAbilitySystem.Demo.Player
{
    /// <summary>
    /// First-person character controller with movement, camera look, and ability integration.
    /// Handles:
    /// - WASD movement with sprinting
    /// - Mouse look with vertical clamping
    /// - Jumping with gravity
    /// - Ability system integration for movement modifiers
    /// </summary>
    [RequireComponent(typeof(CharacterController))]
    public class SimpleCharacterController : MonoBehaviour, ICardsPoolUpdated
    {
        [Header("Movement Settings")]
        [Tooltip("Base movement speed in meters/second")]
        [SerializeField] private float moveSpeed = 5f;

        [Tooltip("Sprinting speed in meters/second")]
        [SerializeField] private float sprintSpeed = 8f;

        [Tooltip("Jump height in meters")]
        [SerializeField] private float jumpHeight = 2f;

        [Tooltip("Gravity force applied in m/s² (negative for downward force)")]
        [SerializeField] private float gravity = -9.81f;

        [Header("Camera Settings")]
        [Tooltip("Mouse sensitivity multiplier")]
        [SerializeField] private float mouseSensitivity = 2f;

        [Tooltip("Reference to the camera transform")]
        [SerializeField] private Transform cameraTransform;

        [Tooltip("Vertical look angle range in degrees")]
        [SerializeField] private float upDownRange = 80f;

        [Header("Dependencies")]
        [Tooltip("Card system reference for ability updates")]
        [SerializeField] private CardSystem cardSystem;

        private CharacterController characterController;
        private Vector3 velocity;                  // Current movement velocity
        private float verticalRotation = 0f;       // Current camera pitch angle
        private float currentSpeed;                 // Current movement speed
        private bool _enabled = true;              // Controller active state
        private float _movementSpeedMultiplier = 1f; // Ability-modified speed multiplier

        private void OnEnable() => cardSystem.abilityManager.AddCardsPoolUpdatedCallback(this);
        private void OnDisable() => cardSystem.abilityManager.RemoveCardsPoolUpdatedCallback(this);

        private void Awake()
        {
            characterController = GetComponent<CharacterController>();
            currentSpeed = moveSpeed;
            SetCharacterControllerEnabled(true);
        }

        private void Update()
        {
            if (!_enabled) return;

            HandleCameraLook();
            HandleMovement();
        }

        /// <summary>
        /// Enables/disables character control and cursor state
        /// </summary>
        /// <param name="enabled">When false, disables input and shows cursor</param>
        public void SetCharacterControllerEnabled(bool enabled)
        {
            _enabled = enabled;
            Cursor.visible = !enabled;
            Cursor.lockState = enabled ? CursorLockMode.Locked : CursorLockMode.None;
        }

        /// <summary>
        /// Handles mouse input for camera rotation with vertical clamping
        /// </summary>
        private void HandleCameraLook()
        {
            // Horizontal rotation (yaw)
            float mouseX = Input.GetAxis("Mouse X") * mouseSensitivity;
            transform.Rotate(Vector3.up, mouseX);

            // Vertical rotation (pitch) with clamping
            float mouseY = Input.GetAxis("Mouse Y") * mouseSensitivity;
            verticalRotation = Mathf.Clamp(verticalRotation - mouseY, -upDownRange, upDownRange);
            cameraTransform.localEulerAngles = Vector3.right * verticalRotation;
        }

        /// <summary>
        /// Handles all movement logic including:
        /// - Ground detection
        /// - WASD movement
        /// - Sprinting
        /// - Jumping
        /// - Gravity
        /// </summary>
        private void HandleMovement()
        {
            // Ground check and velocity reset
            bool isGrounded = characterController.isGrounded;
            if (isGrounded && velocity.y < 0)
            {
                velocity.y = -2f; // Small downward force to ensure grounding
            }

            // Speed calculation
            currentSpeed = Input.GetKey(KeyCode.LeftShift) ? sprintSpeed : moveSpeed;

            // Movement input
            Vector3 moveDirection = transform.TransformDirection(
                new Vector3(
                    Input.GetAxis("Horizontal"),
                    0,
                    Input.GetAxis("Vertical")
                ).normalized);

            characterController.Move(moveDirection * currentSpeed * Time.deltaTime * _movementSpeedMultiplier);

            // Jumping
            if (isGrounded && Input.GetButtonDown("Jump"))
            {
                velocity.y = Mathf.Sqrt(jumpHeight * -2f * gravity);
            }

            // Gravity application
            velocity.y += gravity * Time.deltaTime;
            characterController.Move(velocity * Time.deltaTime);
        }

        /// <summary>
        /// Applies movement speed modifiers from ability cards
        /// </summary>
        public void OnCardsPoolUpdated(AbilityManager abilityManager)
        {
            if (abilityManager.TryGetCard(out FastLegs speedAbility))
            {
                _movementSpeedMultiplier = speedAbility.speedMultiplier;
            }
        }
    }
}