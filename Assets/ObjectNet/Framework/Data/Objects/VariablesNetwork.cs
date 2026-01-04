using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace com.onlineobject.objectnet {

    /// <summary>
    /// Represents a network entity that manages a collection of variables and their synchronization over a network.
    /// </summary>
    public class VariablesNetwork : NetworkEntity<IVariable[], IDataStream> {

        // Dictionary to hold variables with their names as keys.
        private Dictionary<string, IVariable> variables = new Dictionary<string, IVariable>();

        // Dictionary to hold variables with their names as keys.
        private Dictionary<string, IVariable> cachedVariables = new Dictionary<string, IVariable>();

        // Dictionary to map variable types to their corresponding byte identifiers.
        private Dictionary<Type, byte> variableTypes = new Dictionary<Type, byte>();

        // List to hold variables names to be identified by his index instead using variable name
        private List<string> variablesName = new List<string>();

        // Target component for the variables.
        private MonoBehaviour componentTarget;

        // Constants representing the byte identifiers for each variable type.
        const byte NULL_VALUE               =  0;
        const byte INTEGER_TYPE             =  1;
        const byte UNSIGNED_INTEGER_TYPE    =  2;
        const byte LONG_TYPE                =  3;
        const byte UNSIGNED_LONG_TYPE       =  4;
        const byte SHORT_TYPE               =  5;
        const byte UNSIGNED_SHORT_TYPE      =  6;
        const byte FLOAT_TYPE               =  7;
        const byte DOUBLE_TYPE              =  8;
        const byte BYTE_TYPE                =  9;
        const byte BYTE_ARRAY_TYPE          = 10;
        const byte STRING_TYPE              = 11;
        const byte CHAR_TYPE                = 12;
        const byte CHAR_ARRAY_TYPE          = 13;
        const byte VECTOR3_TYPE             = 14;
        const byte VECTOR2_TYPE             = 15;
        const byte COLOR_TYPE               = 16;
        const byte BOOLEAN_TYPE             = 17;

        /// <summary>
        /// Default constructor that initializes the variable types.
        /// </summary>
        public VariablesNetwork() : base() {
            this.InitializeTypes();
        }

        /// <summary>
        /// Constructor that takes a network object and initializes the variable types.
        /// </summary>
        /// <param name="networkObject">The network element associated with this network.</param>
        public VariablesNetwork(INetworkElement networkObject) : base(networkObject) {
            this.InitializeTypes();
        }

        /// <summary>
        /// Initializes the mapping of variable types to their byte identifiers.
        /// </summary>
        private void InitializeTypes() {
            this.variableTypes.Add(typeof(int),     INTEGER_TYPE);
            this.variableTypes.Add(typeof(uint),    UNSIGNED_INTEGER_TYPE);
            this.variableTypes.Add(typeof(long),    LONG_TYPE);
            this.variableTypes.Add(typeof(ulong),   UNSIGNED_LONG_TYPE);
            this.variableTypes.Add(typeof(short),   SHORT_TYPE);
            this.variableTypes.Add(typeof(ushort),  UNSIGNED_SHORT_TYPE);
            this.variableTypes.Add(typeof(float),   FLOAT_TYPE);
            this.variableTypes.Add(typeof(double),  DOUBLE_TYPE);
            this.variableTypes.Add(typeof(byte),    BYTE_TYPE);
            this.variableTypes.Add(typeof(byte[]),  BYTE_ARRAY_TYPE);
            this.variableTypes.Add(typeof(string),  STRING_TYPE);
            this.variableTypes.Add(typeof(char),    CHAR_TYPE);
            this.variableTypes.Add(typeof(char[]),  CHAR_ARRAY_TYPE);
            this.variableTypes.Add(typeof(Vector3), VECTOR3_TYPE);
            this.variableTypes.Add(typeof(Vector2), VECTOR2_TYPE);
            this.variableTypes.Add(typeof(Color),   COLOR_TYPE);
            this.variableTypes.Add(typeof(bool),    BOOLEAN_TYPE);
        }

        /// <summary>
        /// Registers a variable with the network.
        /// </summary>
        /// <param name="name">The name of the variable.</param>
        /// <param name="variable">The variable to register.</param>
        public void RegisterVariable(string name, IVariable variable) {
            if (!this.variables.ContainsKey(name)) {
                this.variables.Add(name, variable);
                this.variablesName.Add(name);
                this.variablesName.Sort(StringComparer.Ordinal);
                if (variablesName.Count > byte.MaxValue) {
                    throw new Exception(string.Format("The maximum os allowed variables is {0}. The variable \"{1}\" has exceeded the allowed number of variables", byte.MaxValue, name));
                }
            } else {
                this.variables[name] = variable;
            }
            if (!this.cachedVariables.ContainsKey(name)) {
                this.cachedVariables.Add(name, variable);
            } else {
                this.cachedVariables[name] = variable;
            }
        }

        /// <summary>
        /// Return if this variable is already registered
        /// </summary>
        /// <param name="name">The name of the variable.</param>
        public bool HasRegisteredVariable(string name) {
            return this.variables.ContainsKey(name);
        }

        /// <summary>
        /// Sets the target component for the variables.
        /// </summary>
        /// <param name="target">The target MonoBehaviour component.</param>
        public void SetComponentTarget(MonoBehaviour target) {
            this.componentTarget = target;
        }

        /// <summary>
        /// Computes the active state of the network entity by updating variables.
        /// </summary>
        public override void ComputeActive() {
            foreach (var variable in this.variables) {
                IVariable variableTarget = variable.Value;
                if (variableTarget != null) {
                    this.FlagUpdated(variable.Value.GetVariableValue().Equals(variableTarget.GetVariableValue()) == false);
                    variable.Value.SetVariableValue(variableTarget.GetVariableValue());
                } else if (variable.Value != null) {
                    this.FlagUpdated(variable.Value.GetVariableValue() != null);
                    variable.Value.SetVariableValue(null);
                }
            }
        }

        /// <summary>
        /// Computes the passive state of the network entity by synchronizing variables.
        /// </summary>
        public override void ComputePassive() {
            foreach (var variable in this.variables) {
                IVariable variableTarget = variable.Value;
                if (variableTarget != null) {
                    variableTarget.SetVariableValue(variable.Value.GetVariableValue());
                    if (variableTarget.WasModified()) {
                        variableTarget.TriggerOnChange(variableTarget.GetPreviousValue(), variableTarget.GetVariableValue());
                        variableTarget.Validate();
                    }
                }
            }
        }

        /// <summary>
        /// Gets the arguments for the passive state computation.
        /// </summary>
        /// <returns>An array of IVariable representing the passive arguments.</returns>
        public override IVariable[] GetPassiveArguments() {
            return this.variables.Values.ToArray<IVariable>();
        }

        /// <summary>
        /// Synchronizes the passive state with the given data.
        /// </summary>
        /// <param name="data">The data to synchronize with.</param>
        public override void SynchonizePassive(IVariable[] data) {            
        }

        /// <summary>
        /// Synchronizes the active state by writing variable data to the provided IDataStream.
        /// </summary>
        /// <param name="writer">The IDataStream writer to write the variable data to.</param>
        public override void SynchonizeActive(IDataStream writer) {
            // Write if update is paused
            writer.Write(this.IsPaused());
            if (this.IsPaused() == false) {
                writer.Write(this.variables.Count); // How many variables i will send
                foreach (var variable in this.variables) {
                    byte variableIndexPosition = (byte)this.variablesName.IndexOf(variable.Key);
                    writer.Write(variableIndexPosition); // Variable index on list
                    if (variable.Value == null) {
                        writer.Write(NULL_VALUE);
                    } else {
                        writer.Write(this.variableTypes[variable.Value.GetVariableType()]); // Variable type ( i need to read correclty )
                    }
                    if (variable.Value != null) {
                        if (this.variableTypes[variable.Value.GetVariableType()] == INTEGER_TYPE) {
                            writer.Write<int>((int)variable.Value.GetVariableValue());
                        } else if (this.variableTypes[variable.Value.GetVariableType()] == UNSIGNED_INTEGER_TYPE) {
                            writer.Write<uint>((uint)variable.Value.GetVariableValue());
                        } else if (this.variableTypes[variable.Value.GetVariableType()] == LONG_TYPE) {
                            writer.Write<long>((int)variable.Value.GetVariableValue());
                        } else if (this.variableTypes[variable.Value.GetVariableType()] == UNSIGNED_LONG_TYPE) {
                            writer.Write<ulong>((ulong)variable.Value.GetVariableValue());
                        } else if (this.variableTypes[variable.Value.GetVariableType()] == SHORT_TYPE) {
                            writer.Write<short>((short)variable.Value.GetVariableValue());
                        } else if (this.variableTypes[variable.Value.GetVariableType()] == UNSIGNED_SHORT_TYPE) {
                            writer.Write<ushort>((ushort)variable.Value.GetVariableValue());
                        } else if (this.variableTypes[variable.Value.GetVariableType()] == FLOAT_TYPE) {
                            writer.Write<float>((float)variable.Value.GetVariableValue());
                        } else if (this.variableTypes[variable.Value.GetVariableType()] == DOUBLE_TYPE) {
                            writer.Write<double>((double)variable.Value.GetVariableValue());
                        } else if (this.variableTypes[variable.Value.GetVariableType()] == BYTE_TYPE) {
                            writer.Write<byte>((byte)variable.Value.GetVariableValue());
                        } else if (this.variableTypes[variable.Value.GetVariableType()] == BYTE_ARRAY_TYPE) {
                            writer.Write<byte[]>((byte[])variable.Value.GetVariableValue());
                        } else if (this.variableTypes[variable.Value.GetVariableType()] == STRING_TYPE) {
                            writer.Write<string>((string)variable.Value.GetVariableValue());
                        } else if (this.variableTypes[variable.Value.GetVariableType()] == CHAR_TYPE) {
                            writer.Write<char>((char)variable.Value.GetVariableValue());
                        } else if (this.variableTypes[variable.Value.GetVariableType()] == CHAR_ARRAY_TYPE) {
                            writer.Write<char[]>((char[])variable.Value.GetVariableValue());
                        } else if (this.variableTypes[variable.Value.GetVariableType()] == VECTOR3_TYPE) {
                            writer.Write<Vector3>((Vector3)variable.Value.GetVariableValue());
                        } else if (this.variableTypes[variable.Value.GetVariableType()] == VECTOR2_TYPE) {
                            writer.Write<Vector2>((Vector2)variable.Value.GetVariableValue());
                        } else if (this.variableTypes[variable.Value.GetVariableType()] == COLOR_TYPE) {
                            writer.Write<Color>((Color)variable.Value.GetVariableValue());
                        } else if (this.variableTypes[variable.Value.GetVariableType()] == BOOLEAN_TYPE) {
                            writer.Write<bool>((bool)variable.Value.GetVariableValue());
                        }
                    }
                }
            }
        }

        /// <summary>
        /// Extracts variable data from the provided IDataStream and updates the network variables.
        /// </summary>
        /// <param name="reader">The IDataStream reader to read the variable data from.</param>
        public override void Extract(IDataStream reader) {
            // First extract if position is paused by other side
            bool isSenderPaused = reader.Read<bool>();
            if (isSenderPaused == false) {
                int variablesCount = reader.Read<int>();
                while (variablesCount > 0) {
                    variablesCount--;
                    // Read variable position index
                    byte variableIndexPosition = reader.Read<byte>();
                    if ((variableIndexPosition >= byte.MinValue) && (variableIndexPosition <= byte.MaxValue)) {
                        string varName = variablesName[variableIndexPosition];
                        byte varType = reader.Read<byte>();
                        object varValue = default(object);
                        if (varType == NULL_VALUE) {
                            varValue = null;
                        } else if (varType == INTEGER_TYPE) {
                            varValue = reader.Read<int>();
                        } else if (varType == UNSIGNED_INTEGER_TYPE) {
                            varValue = reader.Read<uint>();
                        } else if (varType == LONG_TYPE) {
                            varValue = reader.Read<long>();
                        } else if (varType == UNSIGNED_LONG_TYPE) {
                            varValue = reader.Read<ulong>();
                        } else if (varType == SHORT_TYPE) {
                            varValue = reader.Read<short>();
                        } else if (varType == UNSIGNED_SHORT_TYPE) {
                            varValue = reader.Read<ushort>();
                        } else if (varType == FLOAT_TYPE) {
                            varValue = reader.Read<float>();
                        } else if (varType == DOUBLE_TYPE) {
                            varValue = reader.Read<double>();
                        } else if (varType == BYTE_TYPE) {
                            varValue = reader.Read<byte>();
                        } else if (varType == BYTE_ARRAY_TYPE) {
                            varValue = reader.Read<byte[]>();
                        } else if (varType == STRING_TYPE) {
                            varValue = reader.Read<string>();
                        } else if (varType == CHAR_TYPE) {
                            varValue = reader.Read<char>();
                        } else if (varType == CHAR_ARRAY_TYPE) {
                            varValue = reader.Read<char[]>();
                        } else if (varType == VECTOR3_TYPE) {
                            varValue = reader.Read<Vector3>();
                        } else if (varType == VECTOR2_TYPE) {
                            varValue = reader.Read<Vector2>();
                        } else if (varType == COLOR_TYPE) {
                            varValue = reader.Read<Color>();
                        } else if (varType == BOOLEAN_TYPE) {
                            varValue = reader.Read<bool>();
                        }
                        // Update variable
                        if (this.variables.ContainsKey(varName)) {
                            if (this.variables[varName] != null) {
                                if (varValue != null) {
                                    this.variables[varName].SetVariableValue(varValue);
                                } else if (this.variables[varName] != null) {
                                    this.variables[varName].ClearVariableValue();
                                }
                            }
                        } else {
                            NetworkDebugger.Log(string.Format("Network variable \"{0}\" doesn't exists", varName));
                        }
                    } else {
                        NetworkDebugger.Log(string.Format("Network variable index \"{0}\" is invalid index", variableIndexPosition));
                    }
                }
            }
        }
    }

}