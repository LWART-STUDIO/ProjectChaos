using Game.Scripts.Client.Logic.Enemy;
using Game.Scripts.Client.Logic.Player;
using PurrNet;
using UnityEngine;

namespace Game.Scripts.Client.Logic.Game
{
    public class GameStatisticCollector  : NetworkBehaviour
    {
        private static float _gameTime;
        private static int _enemiesWasKilled;
        public static float GameTime=>_gameTime;
        public static int EnemyWasKilled => _enemiesWasKilled;
        public static int PlayerTotalDamage => CalculatePlayerDamage();


        protected override void OnSpawned(bool asServer)
        {
            base.OnSpawned(asServer);
            EnemyHealth.onEnemyKilled -= OnEnemyWasKilled;
            EnemyHealth.onEnemyKilled += OnEnemyWasKilled;

        }
        [ObserversRpc(runLocally: true)]
        public static void UpdateTime(float time)
        {
            _gameTime =  time;
        }
        [ObserversRpc(runLocally: true)]
        private void OnEnemyWasKilled(EnemyHealth health)
        {
            _enemiesWasKilled++;
        }

        public static int CalculatePlayerDamage()
        {
           return InstanceHandler.TryGetInstance(out SkillsHandler h) && h != null
                ? h.GetALLDamage()
                : 0;
        }
        protected override void OnDespawned(bool asServer)
        {
            base.OnDespawned(asServer);
            EnemyHealth.onEnemyKilled -= OnEnemyWasKilled;

        }
        
    }
}
