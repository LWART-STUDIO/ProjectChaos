using SFAbilitySystem.Core;
using SFAbilitySystem.Demo.Abilities;
using SFAbilitySystem.Demo.Core;
using System.Collections;
using UnityEngine;

namespace SFAbilitySystem.Demo.Logic
{
    /// <summary>
    /// Abstract base class for typed active ability logic implementations.
    /// Handles cooldown, casting time, and execution flow for active abilities.
    /// </summary>
    /// <typeparam name="T">The specific ActiveAbilityBase type this logic handles</typeparam>
    public abstract class ActiveLogicBase<T> : ActiveAbilityLogicBase where T : ActiveAbilityBase
    {
        [Header("Ability Settings")]
        [Tooltip("Current cooldown time from the ability configuration")]
        [SerializeField] protected float CooldownTime => _activeAbilityBase.cooldown;

        [Tooltip("Casting time from the ability configuration")]
        protected float _castTime => _activeAbilityBase.castTime;

        protected KeyCode _hotkey = KeyCode.None;  // Bound hotkey for this ability
        protected bool _isOnCooldown = false;     // Cooldown state flag
        protected bool _isCasting = false;        // Casting state flag
        protected AbilityManager _abilityManager; // Reference to ability system
        protected T _activeAbilityBase;           // Typed ability configuration

        /// <summary>
        /// Determines if the ability is ready to be used
        /// </summary>
        protected override bool IsReady => !_isOnCooldown && !_isCasting;

        /// <summary>
        /// Initializes the ability with manager reference and configuration
        /// </summary>
        /// <param name="abilityManager">The ability system manager</param>
        /// <param name="abilityBase">Configuration data (must match type T)</param>
        public override void Initialize(AbilityManager abilityManager, ActiveAbilityBase abilityBase)
        {
            _abilityManager = abilityManager;
            _activeAbilityBase = abilityBase as T;

            if (_activeAbilityBase == null)
            {
                Debug.LogError($"Type mismatch: Expected {typeof(T)} but got {abilityBase.GetType()}");
            }
        }

        /// <summary>
        /// Sets the hotkey for this ability
        /// </summary>
        /// <param name="newHotkey">The key to activate this ability</param>
        public override void Initialize(KeyCode newHotkey)
        {
            _hotkey = newHotkey;
        }

        /// <summary>
        /// Handles cooldown timing updates
        /// </summary>
        protected virtual void Update()
        {
            HandleCooldown();
        }

        private void HandleCooldown()
        {
            if (_isOnCooldown)
            {
                CurrentCooldown -= Time.deltaTime;
                if (CurrentCooldown <= 0f)
                {
                    _isOnCooldown = false;
                    CurrentCooldown = 0f;
                }
            }
        }

        /// <summary>
        /// Initiates the ability cast sequence
        /// </summary>
        public override void PerformAction()
        {
            if (!IsReady) return;

            StartCoroutine(CastAbility());
        }

        private IEnumerator CastAbility()
        {
            _isCasting = true;
            yield return new WaitForSeconds(_castTime);
            ExecuteAbility();
            _isOnCooldown = true;
            CurrentCooldown = CooldownTime;
            _isCasting = false;
        }

        /// <summary>
        /// Contains the ability-specific implementation logic.
        /// Implement in derived classes.
        /// </summary>
        protected abstract void ExecuteAbility();
    }
}