#if UNITY_EDITOR
using SFAbilitySystem.Core;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

namespace SFAbilitySystem.Editor
{
    [CustomPropertyDrawer(typeof(AbilityBase), true)]
    public class AbilityDrawer : PropertyDrawer
    {
        public override float GetPropertyHeight(SerializedProperty property, GUIContent label)
        {
            // Base height for the foldout
            float height = EditorGUIUtility.singleLineHeight;

            if (property.isExpanded)
            {
                // Add height for each child property
                var children = GetChildren(property);
                foreach (var child in children)
                {
                    height += EditorGUI.GetPropertyHeight(child, true);
                }
            }

            return height;
        }

        public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
        {
            EditorGUI.BeginProperty(position, label, property);

            // Create foldout
            property.isExpanded = EditorGUI.Foldout(
                new Rect(position.x, position.y, position.width, EditorGUIUtility.singleLineHeight),
                property.isExpanded,
                GetDisplayName(property),
                true);

            if (property.isExpanded)
            {
                EditorGUI.indentLevel++;

                float y = position.y + EditorGUIUtility.singleLineHeight;
                var children = GetChildren(property);

                foreach (var child in children)
                {
                    float childHeight = EditorGUI.GetPropertyHeight(child, true);
                    EditorGUI.PropertyField(
                        new Rect(position.x, y, position.width, childHeight),
                        child,
                        true);
                    y += childHeight;
                }

                EditorGUI.indentLevel--;
            }

            EditorGUI.EndProperty();
        }

        private IEnumerable<SerializedProperty> GetChildren(SerializedProperty property)
        {
            var iterator = property.Copy();
            var end = property.GetEndProperty();
            bool enterChildren = true;

            while (iterator.NextVisible(enterChildren) && !SerializedProperty.EqualContents(iterator, end))
            {
                yield return iterator;
                enterChildren = false;
            }
        }

        private string GetDisplayName(SerializedProperty property)
        {
            if (property.managedReferenceValue == null)
                return "Null Ability";

            return ObjectNames.NicifyVariableName(property.managedReferenceValue.GetType().Name);
        }
    }
}
#endif