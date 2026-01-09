using UnityEngine;

namespace Game.Scripts.Client.Logic.Player
{
    [CreateAssetMenu(menuName = "Game/Player Class Config")]
    public class PlayerClassConfig : ScriptableObject
    {
        public PlayerClassType classType;
        public PlayerHealth prefab;
    }
}
