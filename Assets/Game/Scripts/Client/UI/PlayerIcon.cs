using Game.Scripts.Services.Steam;
using Steamworks;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace Game.Scripts.Client.UI
{
    public class PlayerIcon : MonoBehaviour
    {
        public RawImage avatarImage; // RawImage для аватара
        public TMP_Text playerName; // Text для имени

        public void Setup(LobbyPlayer player)
        {

            // Устанавливаем имя
            if (playerName != null)
                playerName.text = player.Name;
            if (avatarImage != null)
                avatarImage.texture = player.Avatar;
        }

    }
}