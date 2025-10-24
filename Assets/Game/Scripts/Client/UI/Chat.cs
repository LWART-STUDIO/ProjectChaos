using System;
using Game.Scripts.Services;
using Game.Scripts.Services.Lobby;
using Game.Scripts.Services.Steam;
using Sisus.Init;
using Steamworks;
using UnityEngine;
using TankAndHealerStudioAssets;

namespace Game.Scripts.Client.UI
{
    public class Chat : MonoBehaviour
    {
        [SerializeField] private UltimateChatBox _chatBox;
        private string _userName="PlayerName";
        public Action<string> OnUserSendMessage;
        private ILobbyService _lobbyService=>Service<ServiceInitor>.Instance.LobbyService;
        private Callback<LobbyChatMsg_t> _onLChatCallback;
        public void SetUserName(string userName)
        {
            _userName = userName;
        }
        private void Awake()
        {
            
            _chatBox.OnInputFieldSubmitted += PlayerChat;
            _lobbyService.OnPlayerJoined += OnPlayerJoined;
            _lobbyService.OnPlayerLeft += OnPlayerDisconected;
            if (_lobbyService is SteamService)
            {
                _onLChatCallback = Callback<LobbyChatMsg_t>.Create(OnLChatCallback);
              
            }
        }

        private void OnPlayerJoined(LobbyPlayer callback)
        {
            _chatBox.RegisterChat("[System]",$"Игрок {callback.Name} присоеденился.",UltimateChatBoxStyles.noticeMessage);
        }
        private void OnPlayerDisconected(LobbyPlayer callback)
        {
            _chatBox.RegisterChat("[System]",$"Игрок {callback.Name} покинул нас.",UltimateChatBoxStyles.noticeMessage);
        }
        private void PlayerChat(string message)
        {
            
            OnUserSendMessage?.Invoke(message);
            if (_lobbyService is SteamService)
            {
                var lobbyID = _lobbyService.LobbyId;
                byte[] data = System.Text.Encoding.UTF8.GetBytes(message);
                SteamMatchmaking.SendLobbyChatMsg(new CSteamID(lobbyID), data, data.Length);
            }
            else
            {
                _chatBox.RegisterChat(_userName,message,UltimateChatBoxStyles.boldUsername);
            }
                
        }

        private void OnLChatCallback(LobbyChatMsg_t callback)
        {
            CSteamID userID;
            byte[] data = new byte[4096];
            EChatEntryType chatEntryType;
            int dataSize = SteamMatchmaking.GetLobbyChatEntry(
                (CSteamID)callback.m_ulSteamIDLobby,
                (int)callback.m_iChatID,
                out userID,
                data,
                data.Length,
                out chatEntryType
            );

            string message = System.Text.Encoding.UTF8.GetString(data, 0, dataSize);
            string senderName = SteamFriends.GetFriendPersonaName(userID);

            // Отображаем в UI
            SteamChat(senderName, message);
        }
        public void SteamChat(string userName,string message)
        {

            _chatBox.RegisterChat(userName,message,UltimateChatBoxStyles.boldUsername);
        }

        private void OnDestroy()
        {
            _chatBox.OnInputFieldSubmitted -= PlayerChat;
            _lobbyService.OnPlayerJoined -= OnPlayerJoined;
            _lobbyService.OnPlayerLeft -= OnPlayerDisconected;
            _onLChatCallback?.Unregister();
        }
    }
}
