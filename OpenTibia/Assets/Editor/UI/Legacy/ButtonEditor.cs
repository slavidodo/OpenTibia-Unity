using UnityEditor;
using UnityEngine;

namespace OpenTibiaUnityEditor.UI.Legacy
{
    [CustomEditor(typeof(OpenTibiaUnity.UI.Legacy.Button), true)]
    [CanEditMultipleObjects]
    public class ButtonEditor : UnityEditor.UI.ButtonEditor
    {
        SerializedProperty _label;

        protected override void OnEnable() {
            base.OnEnable();

            _label = serializedObject.FindProperty("_label");
        }

        public override void OnInspectorGUI() {
            base.OnInspectorGUI();

            EditorGUILayout.LabelField("", GUI.skin.horizontalSlider);
            EditorGUILayout.LabelField("UI.Button", EditorStyles.boldLabel);
            EditorGUILayout.PropertyField(_label, new GUIContent("Label"));

            serializedObject.ApplyModifiedProperties();
        }
    }
}
