using UnityEngine;

namespace com.onlineobject.objectnet.examples {
    public class PlayerSynchronizedVariables : NetworkBehaviour {

        [SerializeField]
        private Color playerColor = Color.white;

        private Renderer playerBodyRender;

        public void Start() {
            this.playerBodyRender = this.GetComponent<Renderer>();
        }

        public void Update() {
            this.playerBodyRender.material.SetColor("_BaseColor", this.playerColor);
        }
    }
}