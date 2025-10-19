using ProjectDawn.Navigation.Hybrid;
using UnityEngine;

namespace Game.Scripts.Client.Logic.Enemy
{
    [RequireComponent(typeof(AgentAuthoring))]
    public class AgentSetDestination : MonoBehaviour
    {
        [SerializeField] private AgentAuthoring _agentAuthoring;
        [SerializeField] private AgentCrowdPathingAuthoring _agentCrowdPathingAuthoring;

        public Transform Target;
        public float Radius;
        public bool EveryFrame;

        public void SetTarget(Transform target, CrowdGroupAuthoring crowdGroupAuthoring)
        {
            _agentCrowdPathingAuthoring.Group = crowdGroupAuthoring;
            Target = target;
        }
        private void Start()
        {
            if (Target == null)
                return;
            var body = _agentAuthoring.EntityBody;
            body.Destination = Target.position;
            body.IsStopped = false;
            _agentAuthoring.EntityBody = body;
        }

        void Update()
        {
            if (!EveryFrame)
                return;
            Start();
        }
    }
}