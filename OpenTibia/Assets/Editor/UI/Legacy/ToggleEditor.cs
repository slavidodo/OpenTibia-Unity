using UnityEditor;
using UnityEngine;

namespace OpenTibiaUnityEditor.Modules.Login
{
    [CustomEditor(typeof(OpenTibiaUnity.UI.Legacy.Toggle), true)]
    [CanEditMultipleObjects]
    public class ToggleEditor : UnityEditor.UI.ToggleEditor
    {
        SerializedProperty _label;

        protected override void OnEnable() {
            base.OnEnable();

            _label = serializedObject.FindProperty("_label");
        }

        public override void OnInspectorGUI() {
            base.OnInspectorGUI();

            EditorGUILayout.LabelField("", GUI.skin.horizontalSlider);
            EditorGUILayout.LabelField("UI.Toggle", EditorStyles.boldLabel);
            EditorGUILayout.PropertyField(_label, new GUIContent("Label"));

            serializedObject.ApplyModifiedProperties();
        }
    }
}
