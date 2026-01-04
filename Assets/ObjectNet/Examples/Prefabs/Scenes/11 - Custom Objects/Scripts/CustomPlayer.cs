using UnityEngine;

namespace com.onlineobject.objectnet.examples {
    public class CustomPlayer : NetworkBehaviour {

        [SerializeField]
        private Color playerColor = Color.white;

        private Renderer playerBodyRender;

        public void Start() {
            this.playerBodyRender = this.GetComponent<Renderer>();
        }

        public void Update() {
            ////////////////////////////////////////////////////////////////////////////////
            //
            //  All MonoBehaviour methods will keep working, nonetheless NetworkBehaviour also
            //  provide a suite of method to handle on Active and Passive modes
            //
            ////////////////////////////////////////////////////////////////////////////////
            this.playerBodyRender.material.SetColor("_BaseColor", this.playerColor);
        }
    
        /// <summary>
        /// This event is called when Network features starts
        /// 
        /// Note : This event is controlled internally by framework and ensure that 
        ///        network is up and running
        /// </summary>
        public override void OnNetworkStarted() { 
            NetworkDebugger.Log("Network is started");
        }

        /// <summary>
        /// Active Awake is executed when objects on Active mode Awake
        /// </summary>
        public void ActiveAwake() {
        }

        /// <summary>
        /// Active Awake is executed when objects on Passive mode Awake
        /// </summary>
        public void PassiveAwake() {
        }

        /// <summary>
        /// Active Start is executed when objects on Active mode Start
        /// </summary>
        public void ActiveStart() {
        }

        /// <summary>
        /// Passive Start is executed when objects on Passive mode Start
        /// </summary>
        public void PassiveStart() {
        }

        /// <summary>
        /// Active Update is executed every frame for Active object
        /// </summary>
        public void ActiveUpdate() {
        }

        /// <summary>
        /// Passive Update is executed every frame for Passive object
        /// </summary>
        public void PassiveUpdate() {
        }

        /// <summary>
        /// Active FixedUpdate is executed every physics frame for Active object
        /// </summary>
        public void ActiveFixedUpdate() {
        }

        /// <summary>
        /// Passive FixedUpdate is executed every physics frame for Passive object
        /// </summary>
        public void PassiveFixedUpdate() {
        }

        /// <summary>
        /// Active LateUpdate is executed every frame after Update for Active object
        /// </summary>
        public void ActiveLateUpdate() {
        }

        /// <summary>
        /// Passive LateUpdate is executed every frame after Update for Passive object
        /// </summary>
        public void PassiveLateUpdate() {
        }

    }
}