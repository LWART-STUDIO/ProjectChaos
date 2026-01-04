using UnityEngine;

namespace Game.Scripts.Client.Logic.Enemy
{
    public class AgentSetDestination : MonoBehaviour
    {


        public Transform Target;
        public float Radius;
        public bool EveryFrame;
        
        private void Start()
        {
         
        }

        void Update()
        {
            if (!EveryFrame)
                return;
            Start();
        }
    }
}