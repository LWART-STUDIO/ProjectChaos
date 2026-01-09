using Game.Scripts.Services.UI;
using PurrNet.StateMachine;
using Sisus.Init;
using UnityEngine;

namespace Game.Scripts.Client.Logic.Game
{
    public class GameStateLose : StateNode
    {
        public override void Enter(bool asServer)
        {
            base.Enter(asServer);
            if(asServer)
                return;
            Debug.Log("You Lose");
            Time.timeScale = 0;
            Service<UIService>.Instance.GetEndGamePanel().OpenWindow();
        }
    }
}
