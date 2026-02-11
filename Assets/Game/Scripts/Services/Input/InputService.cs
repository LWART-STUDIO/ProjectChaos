using Sisus.Init;
using UnityEngine;

namespace Game.Scripts.Services.Input
{
    [Service]
    public class InputService : MonoBehaviour,IService
    {
        public bool InputBlocked = false;
        private bool _inputBlocked =false;

        public void BlockInput()
        {
            _inputBlocked = true;
        }

        public void UnblockInput()
        {
            _inputBlocked = false;
        }

        public void LocalAwake()
        {
            
        }

        public void LocalStart()
        {

        }

        public void LocalUpdate(float deltaTime)
        {
            
        }
        public void LocalUnscaledUpdate(float deltaTime)
        {
          
            
        }
    }
}
