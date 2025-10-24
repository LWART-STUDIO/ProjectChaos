using Sisus.Init;
using Unity.Netcode;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace Game.Scripts.Services.GameFlow
{
    [Service]
    public class GameFlowService : MonoBehaviour
    {
        [ServerRpc(RequireOwnership = false)]
        public void StartGameServerRpc(ServerRpcParams rpcParams = default)
        {
            if (!NetworkManager.Singleton.IsHost) return;
        
            // Убеждаемся, что только хост может запускать
            if (NetworkManager.Singleton.LocalClientId != NetworkManager.ServerClientId)
                return;
        
            // Запускаем игру на сервере
            StartGameLocally();
            
            // Уведомляем всех клиентов
            StartGameClientRpc();
        }
        [ClientRpc]
        private void StartGameClientRpc(ClientRpcParams rpcParams = default)
        {
            if (NetworkManager.Singleton.IsHost) return; // сервер уже обработал
            StartGameLocally();
        }
        private void StartGameLocally()
        {
            // Загружаем игровую сцену
            NetworkManager.Singleton.SceneManager.LoadScene("Game", LoadSceneMode.Single);
        }
    }
}
