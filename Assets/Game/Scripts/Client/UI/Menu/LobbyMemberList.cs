using System;
using System.Collections.Generic;
using System.Linq;
using Michsky.UI.Reach;
using PurrLobby;
using PurrNet.Logging;
using TMPro;
using UnityEngine;

namespace Game.Scripts.Client.UI.Menu
{
    public class LobbyMemberList : MonoBehaviour
    {

        [SerializeField] private List<LobbyMember> _members;
        [SerializeField] private GameObject _createLobbyButton;
        [SerializeField] private GameObject _playButton;
        [SerializeField] private ModeSelector _mapSelector;
        [SerializeField] private TMP_Text _lobbyCountText;
        private int _currenMembersCount;
        private string _localUserID;

        private void Awake()
        {
            DisableAllMembers();
        }

        public void LobbyDataUpdate(PurrLobby.Lobby room, string localUserID)
        {
            if (!room.IsValid)
                return;
            if (!room.IsOwner)
            {
                _createLobbyButton.SetActive(false);
                _playButton.SetActive(false);
                _mapSelector.isInteractable = false;
            }
            _lobbyCountText.text = $"{room.Members.Count}/{4}";
            _localUserID = localUserID;
            SortAllMembers(room);

        }

        public void DisableAllMembers()
        {
            for (var index = 0; index < _members.Count; index++)
            {
                _members[index].SetEmpty();
            }
            _lobbyCountText.text = $"{0}/{4}";

        }

        private void SortAllMembers(PurrLobby.Lobby room)
        {
            if (!room.IsValid)
            {
                PurrLogger.LogError("Can't toggle ready state, current lobby is invalid.");
                return;
            }

            var sortedMembers = room.Members
                // локальный пользователь всегда первый
                .OrderByDescending(m => m.Id == _localUserID)
                // остальные — стабильный порядок (например, по Id)
                .ThenBy(m => m.Id)
                .ToList();

            _currenMembersCount = sortedMembers.Count;

            // инициализируем существующих
            for (int i = 0; i < _members.Count; i++)
            {
                if (i < sortedMembers.Count)
                    _members[i].Init(sortedMembers[i]);
                else
                    _members[i].SetEmpty();
            }
        }
    }

}
    