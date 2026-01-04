#pragma warning disable 0168
#pragma warning disable 0219
#pragma warning disable 0414

using System;
using System.Collections.Generic;
using System.Reflection;
using UnityEngine;

namespace com.onlineobject.objectnet {
    /// <summary>
    /// Abstract class that defines the behavior of networked objects.
    /// </summary>
    public abstract class NetworkBehaviour : NetworkObject, INetworkBehavior {

        // Flag to show or hide network parameters in the inspector.
        [HideInInspector]
        [SerializeField]
        private bool ShowNetworkParameters = false;

        // Store the internal ID of this script into component, this is used to control NetworkVariables
        [HideInInspector]
        [SerializeField]
        private ushort BehaviorlId = 0;

        // Dictionary to hold method names and their corresponding MethodInfo objects.
        private Dictionary<string, MethodInfo> MethodsImplementation = new Dictionary<string, MethodInfo>();

        // List to keep track of synchronized fields.
        private List<string> SynchronizedFields = new List<string>();

        /// <summary>
        /// Virtual method to be overridden by derived classes to perform actions when the network starts.
        /// </summary>
        public virtual void OnNetworkStarted() {
        }

        /// <summary>
        /// Return associated behavior ID of this Behaviour
        /// </summary>
        /// <returns>Behaviour ID</returns>
        public ushort GetBehaviorId() {
            return this.BehaviorlId;
        }

        /// <summary>
        /// Retrieves the MethodInfo for a given method name within the current type.
        /// </summary>
        /// <param name="methodName">The name of the method to retrieve.</param>
        /// <returns>The MethodInfo object for the specified method.</returns>
        private MethodInfo GetInternalMethod(String methodName) {
            return this.GetType().GetMethod(methodName, BindingFlags.Public |
                                                        BindingFlags.NonPublic |
                                                        BindingFlags.Instance |
                                                        BindingFlags.DeclaredOnly);
        }

        /// <summary>
        /// Collects the MethodInfo for a given method name and adds it to the MethodsImplementation dictionary.
        /// </summary>
        /// <param name="methodName">The name of the method to collect.</param>
        private void CollectMethod(String methodName) {
            MethodInfo method = this.GetInternalMethod(methodName);
            if (method != null) {
                this.MethodsImplementation.Add(methodName, method);
            }
        }

        /// <summary>
        /// Collects the MethodInfo for all relevant network behavior methods.
        /// </summary>
        internal void CollectMethodsImplementations() {
            this.CollectMethod(NetworkBehaviorConstants.ActiveAwakeMethod);
            this.CollectMethod(NetworkBehaviorConstants.PassiveAwakeMethod);
            this.CollectMethod(NetworkBehaviorConstants.ActiveOnEnableMethod);
            this.CollectMethod(NetworkBehaviorConstants.PassiveOnEnableMethod);
            this.CollectMethod(NetworkBehaviorConstants.ActiveOnDisableMethod);
            this.CollectMethod(NetworkBehaviorConstants.PassiveOnDisableMethod);
            this.CollectMethod(NetworkBehaviorConstants.ActiveStartMethod);
            this.CollectMethod(NetworkBehaviorConstants.PassiveStartMethod);
            this.CollectMethod(NetworkBehaviorConstants.ActiveUpdateMethod);
            this.CollectMethod(NetworkBehaviorConstants.PassiveUpdateMethod);
            this.CollectMethod(NetworkBehaviorConstants.ActiveFixedUpdateMethod);
            this.CollectMethod(NetworkBehaviorConstants.PassiveFixedUpdateMethod);
            this.CollectMethod(NetworkBehaviorConstants.ActiveLateUpdateMethod);
            this.CollectMethod(NetworkBehaviorConstants.PassiveLateUpdateMethod);
        }

        /// <summary>
        /// Starts the network behavior and collects method implementations.
        /// </summary>
        /// <param name="networkId">Optional network ID to start with.</param>
        public override void StartNetwork(int networkId = 0, Func<INetworkElement, INetworkElement> onNetworkStartedCallback = null) {
            base.StartNetwork(networkId, onNetworkStartedCallback);
            this.CollectMethodsImplementations();
            this.OnNetworkStarted();
        }

        /// </summary>
        // Executes the active awake method if it exists.
        /// </summary>
        private void ExecuteActiveAwake() {
            if (this.MethodsImplementation.ContainsKey(NetworkBehaviorConstants.ActiveAwakeMethod)) {
                this.MethodsImplementation[NetworkBehaviorConstants.ActiveAwakeMethod].Invoke(this, null);
            }
        }

        /// </summary>
        // Executes the active onenable method if it exists.
        /// </summary>
        private void ExecuteActiveOnEnable() {
            if (this.MethodsImplementation.ContainsKey(NetworkBehaviorConstants.ActiveOnEnableMethod)) {
                this.MethodsImplementation[NetworkBehaviorConstants.ActiveOnEnableMethod].Invoke(this, null);
            }
        }

        /// </summary>
        // Executes the active ondisable method if it exists.
        /// </summary>
        private void ExecuteActiveOnDisable() {
            if (this.MethodsImplementation.ContainsKey(NetworkBehaviorConstants.ActiveOnDisableMethod)) {
                this.MethodsImplementation[NetworkBehaviorConstants.ActiveOnDisableMethod].Invoke(this, null);
            }
        }

        /// </summary>
        // Executes the active start method if it exists.
        /// </summary>
        private void ExecuteActiveStart() {
            if (this.MethodsImplementation.ContainsKey(NetworkBehaviorConstants.ActiveStartMethod)) {
                this.MethodsImplementation[NetworkBehaviorConstants.ActiveStartMethod].Invoke(this, null);
            }
        }

        /// </summary>
        // Executes the active update method if it exists.
        /// </summary>
        private void ExecuteActiveUpdate() {
            if (this.MethodsImplementation.ContainsKey(NetworkBehaviorConstants.ActiveUpdateMethod)) {
                this.MethodsImplementation[NetworkBehaviorConstants.ActiveUpdateMethod].Invoke(this, null);
            }
        }

        /// </summary>
        // Executes the active fixed update method if it exists.
        /// </summary>
        private void ExecuteActiveFixedUpdate() {
            if (this.MethodsImplementation.ContainsKey(NetworkBehaviorConstants.ActiveFixedUpdateMethod)) {
                this.MethodsImplementation[NetworkBehaviorConstants.ActiveFixedUpdateMethod].Invoke(this, null);
            }
        }

        /// </summary>
        // Executes the active late update method if it exists.
        /// </summary>
        private void ExecuteActiveLateUpdate() {
            if (this.MethodsImplementation.ContainsKey(NetworkBehaviorConstants.ActiveLateUpdateMethod)) {
                this.MethodsImplementation[NetworkBehaviorConstants.ActiveLateUpdateMethod].Invoke(this, null);
            }
        }

        /// </summary>
        // Executes the passive awake method if it exists.
        /// </summary>
        private void ExecutePassiveAwake() {
            if (this.MethodsImplementation.ContainsKey(NetworkBehaviorConstants.PassiveAwakeMethod)) {
                this.MethodsImplementation[NetworkBehaviorConstants.PassiveAwakeMethod].Invoke(this, null);
            }
        }

        /// </summary>
        // Executes the passive onenable method if it exists.
        /// </summary>
        private void ExecutePassiveOnEnable() {
            if (this.MethodsImplementation.ContainsKey(NetworkBehaviorConstants.PassiveOnEnableMethod)) {
                this.MethodsImplementation[NetworkBehaviorConstants.PassiveOnEnableMethod].Invoke(this, null);
            }
        }

        /// </summary>
        // Executes the passive ondisable method if it exists.
        /// </summary>
        private void ExecutePassiveOnDisable() {
            if (this.MethodsImplementation.ContainsKey(NetworkBehaviorConstants.PassiveOnDisableMethod)) {
                this.MethodsImplementation[NetworkBehaviorConstants.PassiveOnDisableMethod].Invoke(this, null);
            }
        }

        /// </summary>
        // Executes the passive start method if it exists.
        /// </summary>
        private void ExecutePassiveStart() {
            if (this.MethodsImplementation.ContainsKey(NetworkBehaviorConstants.PassiveStartMethod)) {
                this.MethodsImplementation[NetworkBehaviorConstants.PassiveStartMethod].Invoke(this, null);
            }
        }

        /// </summary>
        // Executes the passive fixed update method if it exists.
        /// </summary>
        private void ExecutePassiveFixedUpdate() {
            if (this.MethodsImplementation.ContainsKey(NetworkBehaviorConstants.PassiveFixedUpdateMethod)) {
                this.MethodsImplementation[NetworkBehaviorConstants.PassiveFixedUpdateMethod].Invoke(this, null);
            }
        }

        /// </summary>
        // Executes the passive late update method if it exists.
        /// </summary>
        private void ExecutePassiveLateUpdate() {
            if (this.MethodsImplementation.ContainsKey(NetworkBehaviorConstants.PassiveLateUpdateMethod)) {
                this.MethodsImplementation[NetworkBehaviorConstants.PassiveLateUpdateMethod].Invoke(this, null);
            }
        }

        /// </summary>
        // Executes the passive update method if it exists.
        /// </summary>
        private void ExecutePassiveUpdate() {
            if (this.MethodsImplementation.ContainsKey(NetworkBehaviorConstants.PassiveUpdateMethod)) {
                this.MethodsImplementation[NetworkBehaviorConstants.PassiveUpdateMethod].Invoke(this, null);
            }
        }

        /// <summary>
        /// Called when the network object is awakened. Executes different initialization
        /// methods based on the current behavior mode (Active or Passive).
        /// </summary>
        public override void InternalNetworkAwake() {
            if (BehaviorMode.Active.Equals(this.GetBehaviorMode())) {
                this.ExecuteActiveAwake();
            } else if (BehaviorMode.Passive.Equals(this.GetBehaviorMode())) {
                this.ExecutePassiveAwake();
            }
        }

        /// <summary>
        /// Internal OnEnable method called when the network object is enabled network operations.
        /// This is similar to a OnEnable method but specific to network initialization.
        /// </summary>
        public override void InternalNetworkOnEnable() {
            if (BehaviorMode.Active.Equals(this.GetBehaviorMode())) {
                this.ExecuteActiveOnEnable();
            } else if (BehaviorMode.Passive.Equals(this.GetBehaviorMode())) {
                this.ExecutePassiveOnEnable();
            }
        }

        /// <summary>
        /// Internal OnDisable method called when the network object is disabled network operations.
        /// This is similar to a OnDisable method but specific to network initialization.
        /// </summary>
        public override void InternalNetworkOnDisable() {
            if (BehaviorMode.Active.Equals(this.GetBehaviorMode())) {
                this.ExecuteActiveOnDisable();
            } else if (BehaviorMode.Passive.Equals(this.GetBehaviorMode())) {
                this.ExecutePassiveOnDisable();
            }
        }

        /// <summary>
        /// Called when the network object is started. Executes different start methods
        /// based on the current behavior mode (Active or Passive).
        /// </summary>
        public override void InternalNetworkStart() {
            if (BehaviorMode.Active.Equals(this.GetBehaviorMode())) {
                this.ExecuteActiveStart();
            } else if (BehaviorMode.Passive.Equals(this.GetBehaviorMode())) {
                this.ExecutePassiveStart();
            }
        }

        /// <summary>
        /// Called every frame to update the network object. Executes different update
        /// methods based on the current behavior mode (Active or Passive).
        /// </summary>
        public override void InternalNetworkUpdate() {
            if (BehaviorMode.Active.Equals(this.GetBehaviorMode())) {
                this.ExecuteActiveUpdate();
            } else if (BehaviorMode.Passive.Equals(this.GetBehaviorMode())) {
                this.ExecutePassiveUpdate();
            }
        }

        /// <summary>
        /// Called after all Update functions have been called. Executes different late
        /// update methods based on the current behavior mode (Active or Passive).
        /// </summary>
        public override void InternalNetworkLateUpdate() { 
            if (BehaviorMode.Active.Equals(this.GetBehaviorMode())) {
                this.ExecuteActiveLateUpdate();
            } else if (BehaviorMode.Passive.Equals(this.GetBehaviorMode())) {
                this.ExecutePassiveLateUpdate();
            }
        }

        /// <summary>
        /// Called every fixed framerate frame. Executes different fixed update methods
        /// based on the current behavior mode (Active or Passive).
        /// </summary>
        public override void InternalNetworkFixedUpdate() {
            if (BehaviorMode.Active.Equals(this.GetBehaviorMode())) {
                this.ExecuteActiveFixedUpdate();
            } else if (BehaviorMode.Passive.Equals(this.GetBehaviorMode())) {
                this.ExecutePassiveFixedUpdate();
            }
        }

        /// <summary>
        /// Initializes a synchronized field of the specified type.
        /// </summary>
        /// <param name="field">The field information to be synchronized.</param>
        /// <typeparam name="T">The type of the field to be synchronized.</typeparam>
        private void InitializeSynchonizedField<T>(FieldInfo field) {
            NetworkVariable<T> pairedSync = new NetworkVariable<T>(default(T), this);
            pairedSync.OnSynchonize(() => { return (T)field.GetValue(this); },
                                    (T value) => { field.SetValue(this, value); });
            // Register variable to be Synchronized
            this.RegisterSynchronizedVariable(field.Name, pairedSync);
        }

        /// <summary>
        /// Registers a field to be synchronized if it is not already registered.
        /// </summary>
        /// <param name="field">The name of the field to register.</param>
        public void RegisterSynchonizedField(String field) {
            if (!this.SynchronizedFields.Contains(field)) {
                this.SynchronizedFields.Add(field);
            }
        }

        /// <summary>
        /// Initializes all synchronized fields based on their types.
        /// </summary>
        public override void InitializeSynchonizedFields() {
            foreach (String fieldName in this.SynchronizedFields) {
                FieldInfo field = this.GetType().GetField(fieldName, BindingFlags.Public | 
                                                                     BindingFlags.NonPublic | 
                                                                     BindingFlags.Instance | 
                                                                     BindingFlags.DeclaredOnly);
                if ( field != null ) {
                    if ( field.FieldType == typeof(int) )
                        this.InitializeSynchonizedField<int>(field);
                    else if ( field.FieldType == typeof(uint) )
                        this.InitializeSynchonizedField<uint>(field);
                    else if ( field.FieldType == typeof(long) )
                        this.InitializeSynchonizedField<long>(field);
                    else if ( field.FieldType == typeof(ulong) )
                        this.InitializeSynchonizedField<ulong>(field);
                    else if ( field.FieldType == typeof(short) )
                        this.InitializeSynchonizedField<short>(field);
                    else if ( field.FieldType == typeof(ushort) )
                        this.InitializeSynchonizedField<ushort>(field);
                    else if ( field.FieldType == typeof(float) )
                        this.InitializeSynchonizedField<float>(field);
                    else if ( field.FieldType == typeof(double) )
                        this.InitializeSynchonizedField<double>(field);
                    else if ( field.FieldType == typeof(byte) )
                        this.InitializeSynchonizedField<byte>(field);
                    else if ( field.FieldType == typeof(byte[]) )
                        this.InitializeSynchonizedField<byte[]>(field);
                    else if ( field.FieldType == typeof(string) )
                        this.InitializeSynchonizedField<string>(field);
                    else if ( field.FieldType == typeof(char) )
                        this.InitializeSynchonizedField<char>(field);
                    else if ( field.FieldType == typeof(char[]) )
                        this.InitializeSynchonizedField<char[]>(field);
                    else if ( field.FieldType == typeof(Vector3) )
                        this.InitializeSynchonizedField<Vector3>(field);
                    else if ( field.FieldType == typeof(Color) )
                        this.InitializeSynchonizedField<Color>(field);
                    else if ( field.FieldType == typeof(bool) )
                        this.InitializeSynchonizedField<bool>(field);

                }
            }
        }
    }
}