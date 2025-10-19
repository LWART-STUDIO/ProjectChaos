using SFAbilitySystem.Core;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

namespace SFAbilitySystem.Editor
{
    [CustomEditor(typeof(AbilityContainer))]
    public class AbilityContainerEditor : UnityEditor.Editor
    {
        private SerializedProperty abilityTiersProp;
        private string[] abilityTypes;
        private int selectedTypeIndex = 0;

        private void OnEnable()
        {
            abilityTiersProp = serializedObject.FindProperty("abilityTiers");
            FindAbilityTypes();
        }

        public override void OnInspectorGUI()
        {
            serializedObject.Update();

            // Draw the list
            EditorGUILayout.PropertyField(abilityTiersProp);

            // Add new ability controls
            EditorGUILayout.Space();
            selectedTypeIndex = EditorGUILayout.Popup("Add Ability", selectedTypeIndex, abilityTypes);

            if (GUILayout.Button("Add Selected Ability"))
            {
                AddAbility(abilityTypes[selectedTypeIndex]);
            }

            serializedObject.ApplyModifiedProperties();
        }

        private void FindAbilityTypes()
        {
            var types = new List<string>();
            var baseType = typeof(AbilityBase);
            var assemblies = System.AppDomain.CurrentDomain.GetAssemblies();

            foreach (var assembly in assemblies)
            {
                foreach (var type in assembly.GetTypes())
                {
                    if (type.IsClass && !type.IsAbstract && baseType.IsAssignableFrom(type))
                    {
                        types.Add(type.Name);
                    }
                }
            }

            abilityTypes = types.ToArray();
        }

        private void AddAbility(string typeName)
        {
            var baseType = typeof(AbilityBase);
            var assemblies = System.AppDomain.CurrentDomain.GetAssemblies();

            foreach (var assembly in assemblies)
            {
                var type = assembly.GetType(typeName);
                if (type != null && baseType.IsAssignableFrom(type))
                {
                    var ability = System.Activator.CreateInstance(type) as AbilityBase;
                    abilityTiersProp.arraySize++;
                    var newElement = abilityTiersProp.GetArrayElementAtIndex(abilityTiersProp.arraySize - 1);
                    newElement.managedReferenceValue = ability;
                    serializedObject.ApplyModifiedProperties();
                    return;
                }
            }
        }
    }
}
