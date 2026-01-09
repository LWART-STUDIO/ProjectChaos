using System;
using PurrNet;
using UnityEngine;

namespace Game.Scripts.Client.Logic.Game
{
    public class LevelManager : NetworkBehaviour
    {
        [SerializeField] private int _expToLevel = 3;

        private SyncVar<int> _exp = new SyncVar<int>(0);
        private SyncVar<int> _level = new SyncVar<int>(0);
        
        public static Action<int> onExpChanged;
        public static Action<int> onLevelChanged;
        
        private int _expToNextLevel => _expToLevel*(_level +1);

        private void Awake()
        {
            InstanceHandler.RegisterInstance(this);
            _exp.onChanged += OnExpChanged;
            _level.onChanged += OnLevelChanged;
            
        }

        protected override void OnDestroy()
        {
            base.OnDestroy();
            InstanceHandler.UnregisterInstance<LevelManager>();
            _exp.onChanged -= OnExpChanged;
            _level.onChanged -= OnLevelChanged;
        }

        private void OnLevelChanged(int newLevel)
        {
            onLevelChanged?.Invoke(newLevel);
        }

        private void OnExpChanged(int newExp)
        {
            onExpChanged?.Invoke(newExp);
        }

        public void AddExp(int amount)
        {
            if(!isServer)
                return;
            _exp.value+=amount;
            CheckLevel();

        }

        private void CheckLevel()
        {
            if(_exp.value < _expToNextLevel)
                return;
           _exp.value -= _expToNextLevel;
           _level.value++;
        }
        
    }
}
