using UnityEditor;
using UnityEngine;

namespace LeTai.TrueShadow.Editor
{
    public class InlineToolbar : PropertyDrawer
    {
        protected static Texture[] textures;

        // LAZY-INITIALIZED — avoids early access to EditorStyles
        private static GUIStyle _labelStyle;
        protected static GUIStyle LABEL_STYLE
        {
            get
            {
                if (_labelStyle == null)
                {
                    // Fallback to default label if EditorStyles isn't ready
                    var sourceStyle = EditorStyles.label ?? GUI.skin.label;
                    _labelStyle = new GUIStyle(sourceStyle)
                    {
                        alignment = TextAnchor.MiddleLeft
                    };
                }
                return _labelStyle;
            }
        }

        public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
        {
            using (var propScope = new EditorGUI.PropertyScope(position, label, property))
            {
                int id = GUIUtility.GetControlID(FocusType.Keyboard, position);
                var labelRect = position;
                labelRect.y += (labelRect.height - EditorGUIUtility.singleLineHeight) / 2;
                labelRect.height = EditorGUIUtility.singleLineHeight;

                var toolbarRect = EditorGUI.PrefixLabel(labelRect, id, propScope.content, LABEL_STYLE);
                toolbarRect.width = EditorGUIUtility.singleLineHeight * 4f;
                toolbarRect.height = position.height;
                toolbarRect.y = position.y;

                using (var changeScope = new EditorGUI.ChangeCheckScope())
                {
                    var isOn = GUI.Toolbar(toolbarRect, property.boolValue ? 1 : 0, textures) == 1;
                    var changed = changeScope.changed;

                    if (Event.current.type == EventType.KeyDown &&
                        GUIUtility.keyboardControl == id)
                    {
                        if (Event.current.keyCode == KeyCode.Return ||
                            Event.current.keyCode == KeyCode.KeypadEnter ||
                            Event.current.keyCode == KeyCode.Space)
                        {
                            changed = GUI.changed = true;
                            isOn = !isOn;
                        }
                    }

                    if (changed)
                        property.boolValue = isOn;
                }
            }
        }
    }
}