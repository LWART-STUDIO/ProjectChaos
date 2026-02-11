using Michsky.UI.Reach;
using PurrLobby;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace Game.Scripts.Client.UI.Menu
{
    public class LobbyMember : MonoBehaviour
    {
        [SerializeField] private LobbyPlayer _lobbyPlayer;

        private Color _defaultColor;
        private string _memberId;
        public string MemberId => _memberId;

        public void Init(LobbyUser user)
        {
            _lobbyPlayer.SetPlayerName(user.DisplayName);
            _lobbyPlayer.SetAdditionalText("");
            _lobbyPlayer.SetPlayerPicture(user.Avatar);
            _memberId = user.Id;
            SetReady(user.IsReady);
        }

        public void SetEmpty()
        {
            _lobbyPlayer.SetEmpty();
        }
        
        public void SetReady(bool isReady)
        {
            if (isReady)
                _lobbyPlayer.SetReady();
            else
                _lobbyPlayer.SetNotReady();
        }
    }
}
