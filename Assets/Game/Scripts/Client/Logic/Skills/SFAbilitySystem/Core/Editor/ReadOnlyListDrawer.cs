using SFAbilitySystem.Attributes;
using UnityEditor;
using UnityEngine;

namespace SFAbilitySystem.Editor
{
    [CustomPropertyDrawer(typeof(ReadOnlyInspectorAttribute))]
    public class ReadOnlyListDrawer : PropertyDrawer
    {
        public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
        {
            EditorGUI.BeginDisabledGroup(true);

            // Check if this is an array/list
            if (property.isArray && property.propertyType != SerializedPropertyType.String)
            {
                // Show the foldout
                property.isExpanded = EditorGUI.Foldout(
                    new Rect(position.x, position.y, position.width, EditorGUIUtility.singleLineHeight),
                    property.isExpanded,
                    label);

                if (property.isExpanded)
                {
                    EditorGUI.indentLevel++;
                    // Show array size
                    var size = property.FindPropertyRelative("Array.size");
                    EditorGUI.PropertyField(
                        new Rect(position.x, position.y + EditorGUIUtility.singleLineHeight, position.width, EditorGUIUtility.singleLineHeight),
                        size, new GUIContent("Size"), true);

                    // Show array elements
                    for (int i = 0; i < property.arraySize; i++)
                    {
                        var element = property.GetArrayElementAtIndex(i);
                        EditorGUI.PropertyField(
                            new Rect(position.x, position.y + EditorGUIUtility.singleLineHeight * (i + 2), position.width, EditorGUIUtility.singleLineHeight),
                            element, new GUIContent($"Element {i}"), true);
                    }
                    EditorGUI.indentLevel--;
                }
            }
            else
            {
                // Regular property field for non-array types
                EditorGUI.PropertyField(position, property, label, true);
            }

            EditorGUI.EndDisabledGroup();
        }

        public override float GetPropertyHeight(SerializedProperty property, GUIContent label)
        {
            if (property.isArray && property.propertyType != SerializedPropertyType.String)
            {
                if (!property.isExpanded)
                    return EditorGUIUtility.singleLineHeight;

                // Height for foldout + size field + all elements
                return EditorGUIUtility.singleLineHeight * (property.arraySize + 2) + 2;
            }

            return EditorGUI.GetPropertyHeight(property, label);
        }
    }
}