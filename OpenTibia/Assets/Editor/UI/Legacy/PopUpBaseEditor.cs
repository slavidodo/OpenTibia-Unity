using UnityEditor;
using UnityEngine;

namespace OpenTibiaUnityEditor.UI.Legacy
{
    [CustomEditor(typeof(OpenTibiaUnity.UI.Legacy.PopUpBase), true)]
    [CanEditMultipleObjects]
    public class PopUpBaseEditor : Editor
    {
        SerializedProperty _title;
        SerializedProperty _content;
        SerializedProperty _footerSeparator;
        SerializedProperty _footer;

        protected virtual void OnEnable() {
            _title = serializedObject.FindProperty("_title");
            _content = serializedObject.FindProperty("_content");
            _footerSeparator = serializedObject.FindProperty("_footerSeparator");
            _footer = serializedObject.FindProperty("_footer");
        }

        public override void OnInspectorGUI() {
            serializedObject.Update();

            EditorGUILayout.LabelField("Pop Up Base", EditorStyles.boldLabel);
            EditorGUILayout.PropertyField(_title, new GUIContent("Title"));
            EditorGUILayout.PropertyField(_content, new GUIContent("Content"));
            EditorGUILayout.PropertyField(_footerSeparator, new GUIContent("Footer Separator"));
            EditorGUILayout.PropertyField(_footer, new GUIContent("Footer"));

            serializedObject.ApplyModifiedProperties();
        }
    }
}
