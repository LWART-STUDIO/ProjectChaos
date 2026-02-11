using System;
using System.Collections.Generic;
using Game.Scripts.Client.Logic.Player;
using PurrNet;
using SaintsField.Playa;
using UnityEngine;

namespace Game.Scripts.Client.Logic.Game
{
    public class LevelManager : NetworkBehaviour
    {
        [Header("Leveling")]
        [SerializeField] private AnimationCurve _expCurve = AnimationCurve.Linear(0, 7, 1, 4000);
        [SerializeField] private int _maxLevel = 160;
        [SerializeField] private List<int> _expByLevel = new List<int>();

        private SyncVar<int> _exp = new SyncVar<int>(0);
        private SyncVar<int> _level = new SyncVar<int>(0);

        public static Action<int> onExpChanged;
        public static Action<int> onLevelChanged;
        public static Action onPassiveGrant;

        private void Awake()
        {
            InstanceHandler.RegisterInstance(this);
            _exp.onChanged += OnExpChanged;
            _level.onChanged += OnLevelChanged;

            ApplyExpCurve();
        }

        public void PassiveGrant()
        {
            onPassiveGrant?.Invoke();
        }

        protected override void OnDestroy()
        {
            InstanceHandler.UnregisterInstance<LevelManager>();
            _exp.onChanged -= OnExpChanged;
            _level.onChanged -= OnLevelChanged;
            base.OnDestroy();
        
        }

        private void OnLevelChanged(int newLevel) => onLevelChanged?.Invoke(newLevel);
        private void OnExpChanged(int newExp) => onExpChanged?.Invoke(newExp);

        /// <summary>
        /// Добавляем EXP (вызывается при смерти врага)
        /// </summary>
        public void AddExp(int baseExp)
        {
            if (!isServer)
                return;

            int playerCount = Mathf.Max(1, PlayerHealth.AllPlayers.Count);

            // ⭐ soft scaling для кооп: players ^ 1.25
            int scaledExp = Mathf.CeilToInt(baseExp / Mathf.Pow(playerCount, 1.25f));

            _exp.value += scaledExp;
            CheckLevelUp();
        }

        /// <summary>
        /// Проверка на левелап, сразу несколько уровней, если EXP накопилось
        /// </summary>
        private void CheckLevelUp()
        {
            while (_level.value < _maxLevel && _exp.value >= GetExpToNextLevel())
            {
                _exp.value -= GetExpToNextLevel();
                _level.value++;
            }
        }

        /// <summary>
        /// EXP для следующего уровня
        /// </summary>
        private int GetExpToNextLevel()
        {
            if (_level.value + 1 < _expByLevel.Count)
                return _expByLevel[_level.value + 1];
            else
                return _expByLevel[_expByLevel.Count - 1];
        }

        /// <summary>
        /// Генерация EXP по кривой
        /// </summary>
        [Button]
        public void ApplyExpCurve()
        {
            _expByLevel.Clear();

            for (int i = 0; i < _maxLevel; i++)
            {
                float t = i / (float)(_maxLevel - 1);
                int exp = Mathf.CeilToInt(_expCurve.Evaluate(t));
                _expByLevel.Add(exp);
            }
        }

        /// <summary>
        /// Текущий уровень
        /// </summary>
        public int CurrentLevel => _level.value;

        /// <summary>
        /// Текущий EXP
        /// </summary>
        public int CurrentExp => _exp.value;

        /// <summary>
        /// EXP для следующего уровня
        /// </summary>
        public int ExpToNextLevel => GetExpToNextLevel();
    }
}
