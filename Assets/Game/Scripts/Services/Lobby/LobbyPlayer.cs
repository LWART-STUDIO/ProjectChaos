using UnityEngine;

namespace Game.Scripts.Services.Steam
{
    public class LobbyPlayer
    {
        public ulong Id;           // SteamID или локальный ID
        public string Name;        // Ник
        public Texture2D Avatar;   // Аватарка

        public LobbyPlayer(ulong id, string name, Texture2D avatar)
        {
            Id = id;
            Name = name;
            Avatar = avatar;
        }
    }
}