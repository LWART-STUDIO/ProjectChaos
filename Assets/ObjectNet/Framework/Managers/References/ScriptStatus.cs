using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using UnityEngine;

namespace com.onlineobject.objectnet {

    [Serializable]
    public class ScriptStatus : System.Object {

        [SerializeField]
        public MonoBehaviour Script = null;

        [SerializeField]
        public Boolean Enabled = false;

        [SerializeField]
        public int Delay = 0;

        [SerializeField]
        private VariablesList Variables;

        /// <summary>
        /// Constructor for the ScriptStatus class.
        /// </summary>
        /// <param name="script">The MonoBehaviour script associated with this status.</param>
        /// <param name="scriptEnabled">Initial enabled state of the script. Defaults to false.</param>
        public ScriptStatus(MonoBehaviour script, bool scriptEnabled = false) {
            this.Script = script;
            this.Enabled = scriptEnabled;
            this.Variables = new VariablesList();
        }

        /// <summary>
        /// Retrieves the list of variables associated with the script.
        /// </summary>
        /// <returns>A VariablesList containing the variables.</returns>
        public VariablesList GetVariables() {
            return this.Variables;
        }

        /// <summary>
        /// Sets the list of variables associated with the script.
        /// </summary>
        /// <param name="variablesList">The VariablesList to associate with the script.</param>
        public void SetVariables(VariablesList variablesList) {
            this.Variables = variablesList;
        }

        /// <summary>
        /// Retrieves an array of variable names that are enabled.
        /// </summary>
        /// <returns>An array of strings containing the names of enabled variables.</returns>
        public String[] GetSynchronizedVariables() {
            List<String> result = new List<string>();
            foreach (VariableStatus variable in this.Variables.GetVariables()) {
                if (variable.Enabled) {
                    result.Add(variable.Variable);
                }
            }
            return result.ToArray<string>();
        }

        /// <summary>
        /// Refreshes the list of variables by synchronizing with the actual fields of the MonoBehaviour script.
        /// </summary>
        /// <returns>The current instance of ScriptStatus after updating the variables.</returns>
        public ScriptStatus RefreshVariables() {
            if (this.Script != null) {
                // First list all class variables
                List<FieldInfo> fields = this.Script.GetType().GetFields(BindingFlags.Public |
                                                                         BindingFlags.NonPublic |
                                                                         BindingFlags.Instance |
                                                                         BindingFlags.DeclaredOnly).ToList<FieldInfo>();
                // Register missing variables
                foreach (FieldInfo field in fields) {
                    if (this.IsAllowedVariableOverNetwork(field.FieldType)) {
                        if (!this.Variables.ContainsVariable(field.Name)) {
                            this.Variables.RegisterVariable(field.Name);
                        }
                    }
                }
                // Remove not existent variables
                List<VariableStatus> toRemove = new List<VariableStatus>();
                foreach (VariableStatus field in this.Variables.GetVariables()) {
                    if (!fields.Exists(fd => fd.Name.Equals(field.Variable))) {
                        toRemove.Add(field);
                    }
                }
                // Remove
                while (toRemove.Count > 0) {
                    VariableStatus variable = toRemove[0];
                    toRemove.RemoveAt(0);
                    this.Variables.UnRegisterVariable(variable);
                }
            }
            return this;
        }

        /// <summary>
        /// Determines if a given type is allowed to be synchronized over the network.
        /// </summary>
        /// <param name="type">The Type to check.</param>
        /// <returns>True if the type is allowed, false otherwise.</returns>
        private bool IsAllowedVariableOverNetwork(Type type) {
            return (type == typeof(int)) ||
                   (type == typeof(uint)) ||
                   (type == typeof(long)) ||
                   (type == typeof(ulong)) ||
                   (type == typeof(short)) ||
                   (type == typeof(ushort)) ||
                   (type == typeof(float)) ||
                   (type == typeof(double)) ||
                   (type == typeof(byte)) ||
                   (type == typeof(byte[])) ||
                   (type == typeof(string)) ||
                   (type == typeof(char)) ||
                   (type == typeof(char[])) ||
                   (type == typeof(Vector3)) ||
                   (type == typeof(Color)) ||
                   (type == typeof(bool));
        }

    }

}