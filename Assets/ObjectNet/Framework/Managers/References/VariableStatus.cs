using System;
using UnityEngine;

namespace com.onlineobject.objectnet {

    [Serializable]
    public class VariableStatus : System.Object {

        // Field to hold the variable name.
        [SerializeField]
        public String Variable = null;

        // Field to indicate whether the variable is enabled or not.
        [SerializeField]
        public Boolean Enabled = false;

        // Field to store the delay associated with the variable.
        [SerializeField]
        public int Delay = 0;

        /// <summary>
        /// Constructor for creating a new VariableStatus instance.
        /// </summary>
        /// <param name="variable">The name of the variable.</param>
        /// <param name="syncEnabled">Optional parameter to set the variable as enabled or disabled. Default is false.</param>
        public VariableStatus(String variable, bool syncEnabled = false) {
            this.Variable = variable;   // Assign the variable name.
            this.Enabled = syncEnabled; // Set the enabled status.
        }
    }

}