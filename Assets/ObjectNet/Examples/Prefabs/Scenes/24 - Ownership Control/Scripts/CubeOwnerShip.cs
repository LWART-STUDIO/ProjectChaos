using UnityEngine;

namespace com.onlineobject.objectnet {
    public class CubeOwnerShip : MonoBehaviour {
        public Color activeColor;

        public Color passiveColor;

        private MeshRenderer render;

        private NetworkObject networkObject;

        private Vector3 offset;

        private float coordinates;

        // Start is called before the first frame update
        void Start() {
            this.render = this.GetComponent<MeshRenderer>();
            this.networkObject = this.GetComponent<NetworkObject>();
        }

        private void OnGUI() {
            if (this.networkObject != null) {
                Vector3 labelCoordinate = Camera.main.WorldToScreenPoint(this.gameObject.transform.position);
                GUI.Label(new Rect(labelCoordinate.x, Screen.height - labelCoordinate.y, 100, 20), string.Format("ID : {0}", this.networkObject.GetNetworkId()));
            }
        }

        // Update is called once per frame
        void LateUpdate() {
            if (this.networkObject != null) {
                if (this.networkObject.IsActive()) {
                    this.render.material.SetColor("_BaseColor", this.activeColor);
                } else if (this.networkObject.IsPassive()) {
                    this.render.material.SetColor("_BaseColor", this.passiveColor);
                }
                /**
                if (Input.GetKeyDown(KeyCode.S)) {
                    this.networkObject.GetNetworkElement().GetBehavior<PositionNetwork>().Pause();
                } else if (Input.GetKeyDown(KeyCode.R)) {
                    this.networkObject.GetNetworkElement().GetBehavior<PositionNetwork>().Resume();
                }
                /**/
            } else {
                this.networkObject = this.GetComponent<NetworkObject>();
            }            
        }

        public void OnMouseDown() {
            this.coordinates = Camera.main.WorldToScreenPoint(this.gameObject.transform.position).z;
            this.offset = this.gameObject.transform.position - this.GetMouseWorldPos();
        }

        public void OnMouseDrag() {
            if (this.networkObject != null) {
                if (this.networkObject.IsActive()) {
                    this.transform.position = this.GetMouseWorldPos() + this.offset;
                }
            }
        }

        private Vector3 GetMouseWorldPos() {
            Vector3 mousePoint = Input.mousePosition;
            mousePoint.z = this.coordinates;
            return Camera.main.ScreenToWorldPoint(mousePoint);
        }
    }
}