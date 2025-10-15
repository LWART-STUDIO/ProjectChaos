using System;
using Game.Scripts.Services.Steam;
using Steamworks;

namespace Game.Scripts.Services.Lobby
{
    public interface ILobbyService
    {
        
        CSteamID LobbyId { get; }
        void CreateLobby();
        void JoinLobby();
        event Action<LobbyPlayer> OnPlayerJoined;
        event Action<LobbyPlayer> OnPlayerLeft;
        void LocalAwake();
        void LocalStart();
        void LocalUpdate(float deltaTime);
        void LeaveLobby();
        void StartGameServer();
    }
}