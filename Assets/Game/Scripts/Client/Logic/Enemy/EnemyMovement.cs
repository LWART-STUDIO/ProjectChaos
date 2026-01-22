using Game.Scripts.Client.Logic.Player;
using ProjectDawn.Navigation.Hybrid;
using PurrNet;
using UnityEngine;

namespace Game.Scripts.Client.Logic.Enemy
{
    [RequireComponent(typeof(AgentAuthoring))]
    public class EnemyMovement : NetworkBehaviour,ITick
    {
        [SerializeField] private EnemyHealth _enemyHealth;
        [SerializeField]private AgentAuthoring _agent;
        [SerializeField] private AgentCylinderShapeAuthoring _agentShape;
        [SerializeField] private AgentAvoidAuthoring _sonar;
        [SerializeField] private AgentNavMeshAuthoring _spawnEffect;
        private PlayerHealth _targetPlayer;
        private float _lastPlayerCheck;
    
        protected override void OnSpawned()
        {
            base.OnSpawned();
            enabled =isServer;
            if (isServer)
            {
                _agent.enabled = true;
                _agentShape.enabled = true;
                _sonar.enabled = true;
            
            }
      
        }

        protected override void OnPoolReset()
        {
            if(!isServer)
                return;
            _targetPlayer = null;
            _lastPlayerCheck = 0;

        }

        private void Update()
        {
            if(!isServer)
                return;
            if(!_targetPlayer)
                return;
            if(!_enemyHealth.Spawned)
                return;
            if (_enemyHealth.Health <= 0)
            {
                _agent.enabled = false;
                return;
            }
            var body = _agent.EntityBody;
            body.Destination = _targetPlayer.transform.position;;
            body.IsStopped = false;
            _agent.EntityBody = body;

        }

        public void OnTick(float delta)
        {
            if(!isServer)
                return;
            if (Time.time < _lastPlayerCheck + 1f)
                return;
            _lastPlayerCheck = Time.time;
            var allPlayers = PlayerHealth.AllPlayers;
            if(allPlayers.Count <= 0)
                return;
            PlayerID closestPlayer = default;
            float closestDistance = float.MaxValue;
            foreach (var player in allPlayers)
            {
                if(player.Value.Health<=0)
                    continue;
                var playerPos = player.Value.transform.position;
                var distance = Vector3.Distance(transform.position, playerPos);
                if (distance < closestDistance)
                {
                    closestDistance = distance;
                    closestPlayer = player.Key;
                }
            }
            if(closestPlayer == default)
                return;
            PlayerHealth.AllPlayers.TryGetValue(closestPlayer, out _targetPlayer);
        }
    }
}