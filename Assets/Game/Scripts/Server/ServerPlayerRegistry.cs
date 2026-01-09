using System.Collections.Generic;
using Game.Scripts.Client.Logic.Player;
using PurrNet;
using UnityEngine;

namespace Game.Scripts.Server
{
    public class ServerPlayerRegistry : MonoBehaviour
    {
        private Dictionary<PlayerID, PlayerClassType> _playerClasses = new();

        private void Awake()
        {
            InstanceHandler.RegisterInstance(this);
        }

        public void SetPlayerClass(PlayerID playerId, PlayerClassType classType)
        {
            _playerClasses[playerId] = classType;
        }

        public PlayerClassType GetPlayerClass(PlayerID playerId)
        {
            return _playerClasses.TryGetValue(playerId, out var type)
                ? type
                : PlayerClassType.Mage; // дефолт
        }

        private void OnDestroy()
        {
            InstanceHandler.UnregisterInstance<ServerPlayerRegistry>();
        }
    }
}