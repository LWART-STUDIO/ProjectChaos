using System;
using System.Collections.Generic;
using UnityEngine;

namespace com.onlineobject.objectnet {
    /// <summary>
    /// A MonoBehaviour class that acts as a bridge for network-related behaviors.
    /// </summary>
    public class NetworkInternalExecutor : MonoBehaviour {

        // Reference to an object that implements INetworkBehaviorInternal interface.
        private INetworkBehaviorInternal networkBehavior;

        // Flag if executor was already initialized
        private bool executorInitialized = false;

        // Flag if awake event was already called
        private bool awakeExecuted = false;

        // List of all executors registered on thi executor
        private List<NetworkInternalExecutor> executors = new List<NetworkInternalExecutor>();

        /// <summary>
        /// Registers a GameObject with a network behavior to this executor.
        /// </summary>
        /// <param name="target">The GameObject to which the network behavior is attached.</param>
        /// <param name="behavior">The network behavior to be registered with the executor.</param>
        public static NetworkInternalExecutor Register(GameObject target, INetworkBehaviorInternal behavior, Action<NetworkBehaviour> childStartAction) {
            NetworkInternalExecutor result = null;
            // Check if the target GameObject already has a NetworkInternalExecutor component.
            if (target.GetComponent<NetworkInternalExecutor>() == null) {
                // Add executor and set as result componet
                result = target.AddComponent<NetworkInternalExecutor>();
                // If not, add the NetworkInternalExecutor component and assign the network behavior.
                result.networkBehavior = behavior;
                // By objects lifecycle, when object start i haven't BehaviorMode defined yet, so i need to wait to be flagged before execute start
                result.enabled = false;
                // Register all executor
                result.executors.Add(target.GetComponent<NetworkInternalExecutor>());
                // Will add on each NetworkBehaviour
                foreach (NetworkBehaviour child in target.GetComponentsInChildren<NetworkBehaviour>()) {
                    if (child.gameObject != target) {
                        child.gameObject.AddComponent<NetworkInternalExecutor>().networkBehavior = child;
                        // By objects lifecycle, when obejct start i haven't BehaviorMode defined yet, so i need to wait to be flagged before execute start
                        child.gameObject.GetComponent<NetworkInternalExecutor>().enabled = false;
                        // Register childs as well
                        result.executors.Add(child.gameObject.GetComponent<NetworkInternalExecutor>());
                        if (childStartAction != null) {
                            childStartAction.Invoke(child);
                        }
                    } else {
                        bool isExecutorRegistered = false;
                        foreach (NetworkInternalExecutor registeredExecutor in result.executors) {
                            isExecutorRegistered |= child.Equals(registeredExecutor.networkBehavior);
                        }
                        if (isExecutorRegistered == false) {
                            NetworkInternalExecutor duplicatedExecutor = target.gameObject.AddComponent<NetworkInternalExecutor>();
                            duplicatedExecutor.networkBehavior = child;
                            result.executors.Add(duplicatedExecutor);
                            child.CollectMethodsImplementations();
                            child.OnNetworkStarted();
                        }
                    }
                }
            } else {
                result = target.GetComponent<NetworkInternalExecutor>();
            }
            return result;
        }

        /// <summary>
        /// Return if this executor was alreaady initialized
        /// </summary>
        /// <returns>True is was initialized, otherwise false</returns>
        public bool WasInitialize() {
            return this.executorInitialized;
        }

        /// <summary>
        /// Initialize all executors registered on this executor
        /// </summary>
        public void InitializeExecutors() {
            if (this.executorInitialized == false) {
                this.executorInitialized = true;
                foreach (NetworkInternalExecutor executor in this.executors) {
                    executor.enabled = true;
                }
            }
        }

        /// <summary>
        /// Unity's OnEnable method, called when the script instance is being loaded.
        /// </summary>
        void OnEnable() {
            // If a network behavior is assigned, call its InternalNetworkAwake method.
            if (this.awakeExecuted == false) {
                if (this.networkBehavior != null) {
                    this.awakeExecuted = true;
                    // set networkBehavior on childs ( if haven't )
                    foreach (NetworkInternalExecutor childExecutor in this.executors) {
                        if (childExecutor.networkBehavior == null) {
                            childExecutor.networkBehavior = this.networkBehavior;
                        } else {
                            childExecutor.networkBehavior.SetBehaviorMode(this.networkBehavior.GetBehaviorMode());
                        }
                    }
                    // Execute internal awake
                    this.networkBehavior.InternalNetworkAwake();
                }
            }
            // If a network behavior is assigned, call its InternalNetworkOnEnable method.
            if (this.networkBehavior != null) {
                this.networkBehavior.InternalNetworkOnEnable();
            }
        }

        /// <summary>
        /// Unity's OnDisable method, called when the script instance is being loaded.
        /// </summary>
        void OnDisable() {
            // If a network behavior is assigned, call its InternalNetworkOnEnable method.
            if (this.networkBehavior != null) {
                this.networkBehavior.InternalNetworkOnDisable();
            }
        }

        /// <summary>
        /// Unity's Start method, called before the first frame update.
        /// </summary>
        void Start() {
            // If a network behavior is assigned, call its InternalNetworkStart method.
            if (this.networkBehavior != null) {
                this.networkBehavior.InternalNetworkStart();
            }
        }

        /// <summary>
        /// Unity's Update method, called once per frame.
        /// </summary>
        void Update() {
            // If a network behavior is assigned, execute network process and call its InternalNetworkUpdate method.
            if (this.networkBehavior != null) {
                this.networkBehavior.UpdateSynchonizedVariables();
                this.networkBehavior.InternalNetworkUpdate();
            }
        }

        /// <summary>
        /// Unity's LateUpdate method, called after all Update methods have been called.
        /// </summary>
        void LateUpdate() {
            // If a network behavior is assigned, call its InternalNetworkLateUpdate method.
            if (this.networkBehavior != null) {
                this.networkBehavior.InternalNetworkLateUpdate();
            }
        }

        /// <summary>
        /// Unity's FixedUpdate method, called every fixed framerate frame.
        /// </summary>
        void FixedUpdate() {
            // If a network behavior is assigned, call its InternalNetworkFixedUpdate method.
            if (this.networkBehavior != null) {
                this.networkBehavior.ExecuteNetworkProcess();
                this.networkBehavior.InternalNetworkFixedUpdate();
            }
        }
    }

}