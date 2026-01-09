using Game.Scripts.Client.Logic.Player;
using Game.Scripts.Services.Scene;
using Sisus.Init;
using UnityEngine;

namespace Game.Scripts.Services.Save
{
    [Service(typeof(SaveService))]
    public class SaveService : MonoBehaviour
    {
        private const string Key = "PlayerClass";

        public void SavePlayerClassType(PlayerClassType type)
        {
            PlayerPrefs.SetInt(Key, (int)type);
            PlayerPrefs.Save();
        }

        public PlayerClassType LoadPlayerClassType()
        {
            return (PlayerClassType)PlayerPrefs.GetInt(Key, 0);
       
        }
    }
}
