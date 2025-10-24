using System;
using Game.Scripts.Services.Steam;
using Steamworks;
using Unity.Netcode;

namespace Game.Scripts.Services.Lobby
{
    public interface ILobbyService
    {
        
        ulong LobbyId { get; }
        void CreateLobby();
        void JoinLobby();
        event Action<LobbyPlayer> OnPlayerJoined;
        event Action<LobbyPlayer> OnPlayerLeft;
         event Action OnLobbyEntered;
        void LocalAwake();
        void LocalStart();
        void LocalUpdate(float deltaTime);
        void LeaveLobby();
        
        void StartGameServer();
    }
}