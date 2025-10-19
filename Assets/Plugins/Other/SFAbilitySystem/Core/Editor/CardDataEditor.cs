using SFAbilitySystem.Core;
using System;
using System.Linq;
using UnityEditor;
using UnityEngine;

namespace SFAbilitySystem.Editor
{
    [CustomEditor(typeof(CardData), true)]
    public class CardDataEditor : UnityEditor.Editor
    {
        private SerializedProperty abilityContainerProp;
        private SerializedProperty abilityTiersProp;
        private string[] abilityTypeNames;
        private int selectedTypeIndex = 0;
        private bool showAbilities = true;
        private bool showCopyButton = false;
        private void OnEnable()
        {
            abilityContainerProp = serializedObject.FindProperty("_abilityContainer");
            abilityTiersProp = abilityContainerProp.FindPropertyRelative("abilityTiers");
            CacheAbilityTypes();
        }

        public override void OnInspectorGUI()
        {
            serializedObject.Update();

            // Draw standard properties first
            DrawDefaultInspectorWithoutAbilities();

            // Abilities section
            EditorGUILayout.Space(15);
            showAbilities = EditorGUILayout.BeginFoldoutHeaderGroup(showAbilities, "Ability Tiers", EditorStyles.foldoutHeader);
            if (showAbilities)
            {
                DrawAbilitiesSection();
            }
            EditorGUILayout.EndFoldoutHeaderGroup();

            serializedObject.ApplyModifiedProperties();
        }

        private void DrawDefaultInspectorWithoutAbilities()
        {
            var iterator = serializedObject.GetIterator();
            bool enterChildren = true;

            while (iterator.NextVisible(enterChildren))
            {
                if (iterator.name != "_abilityContainer")
                {
                    EditorGUILayout.PropertyField(iterator, true);
                }
                enterChildren = false;
            }
        }

        private void DrawAbilitiesSection()
        {
            EditorGUILayout.BeginVertical(EditorStyles.helpBox);

            // Header with controls
            EditorGUILayout.BeginHorizontal();
            EditorGUILayout.LabelField($"Ability Tiers: {abilityTiersProp.arraySize}", EditorStyles.boldLabel);

            // Add/remove buttons
            //if (GUILayout.Button("+", EditorStyles.miniButtonLeft, GUILayout.Width(25)))
            //    abilityTiersProp.arraySize++;

            //EditorGUI.BeginDisabledGroup(abilityTiersProp.arraySize == 0);
            //if (GUILayout.Button("-", EditorStyles.miniButtonRight, GUILayout.Width(25)))
            //    abilityTiersProp.arraySize--;
            //EditorGUI.EndDisabledGroup();
            EditorGUILayout.EndHorizontal();

            // Type selection
            EditorGUILayout.BeginHorizontal();
            selectedTypeIndex = EditorGUILayout.Popup(selectedTypeIndex, abilityTypeNames);
            if (GUILayout.Button("📋", EditorStyles.miniButton, GUILayout.Width(20)))
            {
                showCopyButton = !showCopyButton;
            }
            if (GUILayout.Button("Add", EditorStyles.miniButton, GUILayout.Width(60)))
            {
                AddNewAbility(abilityTypeNames[selectedTypeIndex]);
            }
            EditorGUILayout.EndHorizontal();

            // Tiers list
            EditorGUI.indentLevel++;
            for (int i = 0; i < abilityTiersProp.arraySize; i++)
            {
                DrawAbilityTier(i);
            }
            EditorGUI.indentLevel--;

            EditorGUILayout.EndVertical();
        }
        private MonoScript FindScriptForType(Type type)
        {
            // Fallback to asset database search if direct approach fails
            string[] guids = AssetDatabase.FindAssets($"t:MonoScript {type.Name}");

            foreach (string guid in guids)
            {
                string path = AssetDatabase.GUIDToAssetPath(guid);
                var script = AssetDatabase.LoadAssetAtPath<MonoScript>(path);
                if (script != null && script.GetClass() == type)
                {
                    return script;
                }
            }

            return null;
        }
        private void DrawAbilityTier(int index)
        {
            // Get the element safely
            if (index >= abilityTiersProp.arraySize) return;

            var element = abilityTiersProp.GetArrayElementAtIndex(index);
            if (element == null) return;

            var ability = element.managedReferenceValue as AbilityBase;

            // Get colors
            Color tierColor = GetRainbowColor(index);
            Color abilityColor = new Color(0.8f, 0.8f, 0.8f, 0.5f);

            // Main box
            EditorGUILayout.BeginVertical(EditorStyles.helpBox);
            {
                // Header
                EditorGUILayout.BeginHorizontal();
                {
                    // Label
                    string label = ability != null
                        ? $"{ObjectNames.NicifyVariableName(ability.GetType().Name)} (Tier {index})"
                        : $"Empty Tier {index}";
                    EditorGUILayout.LabelField(label, EditorStyles.boldLabel);


                    // Rainbow action buttons
                    using (new ButtonColorScope(GetRainbowColor(index), Color.white))
                    {

                        if (GUILayout.Button("✎", EditorStyles.miniButtonLeft, GUILayout.Width(20)))
                        {
                            MonoScript script = FindScriptForType(ability.GetType());
                            if (script != null)
                            {
                                AssetDatabase.OpenAsset(script);
                            }
                            else
                            {
                                Debug.LogWarning($"Could not find script file for {ability.GetType().Name}");
                            }
                        }
                    }
                    using (new ButtonColorScope(GetRainbowColor(index) / 1.2f, Color.white))
                    {

                        if (GUILayout.Button("🗄", EditorStyles.miniButtonMid, GUILayout.Width(20)))
                        {
                            MonoScript script = FindScriptForType(ability.GetType());
                            if (script != null)
                            {
                                EditorGUIUtility.PingObject(script);
                            }
                            else
                            {
                                Debug.LogWarning($"Could not find script file for {ability.GetType().Name}");
                            }
                        }
                    }
                    using (new ButtonColorScope(GetRainbowColor(index) / 1.4f, Color.white))
                    {
                        if (GUILayout.Button("↑", EditorStyles.miniButtonMid, GUILayout.Width(20)) && index > 0)
                        {
                            abilityTiersProp.MoveArrayElement(index, index - 1);
                            return; // Exit early to prevent serialization issues
                        }
                    }

                    using (new ButtonColorScope(GetRainbowColor(index) / 1.6f, Color.white))
                    {
                        if (GUILayout.Button("↓", EditorStyles.miniButtonMid, GUILayout.Width(20)) && index < abilityTiersProp.arraySize - 1)
                        {
                            abilityTiersProp.MoveArrayElement(index, index + 1);
                            return; // Exit early to prevent serialization issues
                        }
                    }

                    using (new ButtonColorScope(GetRainbowColor(index) / 1.8f, Color.white))
                    {
                        if (GUILayout.Button("×", EditorStyles.miniButtonRight, GUILayout.Width(20)))
                        {
                            abilityTiersProp.DeleteArrayElementAtIndex(index);
                            return; // Exit early to prevent serialization issues
                        }
                    }
                }
                EditorGUILayout.EndHorizontal();

                // Content - only draw if we haven't modified the array
                if (ability != null)
                {
                    EditorGUI.indentLevel++;

                    // Draw the property field safely
                    var copy = element.Copy();
                    var end = element.GetEndProperty();
                    bool enterChildren = true;

                    while (copy.NextVisible(enterChildren) && !SerializedProperty.EqualContents(copy, end))
                    {

                        EditorGUILayout.BeginHorizontal();

                        if (showCopyButton)
                        {
                            using (new ButtonColorScope(Color.clear, Color.white))
                            {
                                if (GUILayout.Button("Copy", EditorStyles.miniButton, GUILayout.Width(50)))
                                {
                                    EditorGUIUtility.systemCopyBuffer = copy.name;
                                    return; // Exit early to prevent serialization issues
                                }
                            }

                        }

                        EditorGUILayout.PropertyField(copy, true);

                        EditorGUILayout.EndHorizontal();
                        enterChildren = false;
                    }

                    // Preview
                    using (new BackgroundColorScope(new Color(abilityColor.r, abilityColor.g, abilityColor.b, 0.3f)))

                        EditorGUI.indentLevel--;
                }
                else
                {
                    EditorGUILayout.HelpBox("Select a type above and click 'Add'", MessageType.Warning);
                }
            }
            EditorGUILayout.EndVertical();

            EditorGUILayout.Space(5);
        }

        private Color GetRainbowColor(int index)
        {
            float hue = (index * 0.15f) % 1f;
            return Color.HSVToRGB(hue, 0.7f, 1f);
        }

        private void CacheAbilityTypes()
        {
            var abilityBaseType = typeof(AbilityBase);
            abilityTypeNames = AppDomain.CurrentDomain.GetAssemblies()
                .SelectMany(a => a.GetTypes())
                .Where(t => t.IsClass && !t.IsAbstract && abilityBaseType.IsAssignableFrom(t))
                .Select(t => t.Name)
                .OrderBy(n => n)
                .ToArray();
        }

        private void AddNewAbility(string typeName)
        {
            if (TryGetType(typeName, out var type))
            {
                serializedObject.Update();
                try
                {
                    Undo.RecordObject(target, "Add Ability");
                    abilityTiersProp.arraySize++;
                    var newElement = abilityTiersProp.GetArrayElementAtIndex(abilityTiersProp.arraySize - 1);
                    newElement.managedReferenceValue = Activator.CreateInstance(type);
                    serializedObject.ApplyModifiedProperties();
                    EditorUtility.SetDirty(target);
                }
                catch (Exception e)
                {
                    Debug.LogError($"Failed to add ability: {e.Message}");
                    abilityTiersProp.arraySize--;
                    serializedObject.ApplyModifiedProperties();
                }
            }
        }

        private bool TryGetType(string typeName, out Type type)
        {
            type = AppDomain.CurrentDomain.GetAssemblies()
        .SelectMany(a => a.GetTypes())
        .FirstOrDefault(t => t.Name == typeName && typeof(AbilityBase).IsAssignableFrom(t));
            return type != null;
        }

        private struct ButtonColorScope : IDisposable
        {
            private readonly Color originalColor;
            private readonly Color originalContentColor;

            public ButtonColorScope(Color bgColor, Color textColor)
            {
                originalColor = GUI.backgroundColor;
                originalContentColor = GUI.contentColor;
                GUI.backgroundColor = bgColor;
                GUI.contentColor = textColor;
            }

            public void Dispose()
            {
                GUI.backgroundColor = originalColor;
                GUI.contentColor = originalContentColor;
            }
        }

        private struct BackgroundColorScope : IDisposable
        {
            private readonly Color originalColor;

            public BackgroundColorScope(Color color)
            {
                originalColor = GUI.backgroundColor;
                GUI.backgroundColor = color;
            }

            public void Dispose()
            {
                GUI.backgroundColor = originalColor;
            }
        }

        private struct TextColorScope : IDisposable
        {
            private readonly Color originalColor;

            public TextColorScope(Color color)
            {
                originalColor = GUI.contentColor;
                GUI.contentColor = color;
            }

            public void Dispose()
            {
                GUI.contentColor = originalColor;
            }
        }
    }
}