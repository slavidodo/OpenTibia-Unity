using UnityEditor;
using UnityEngine;

namespace OpenTibiaUnityEditor.UI.Legacy
{
    [CustomEditor(typeof(OpenTibiaUnity.UI.Legacy.SidebarWidgetContainer), true)]
    [CanEditMultipleObjects]
    public class SidebarWidgetContainerEditor : Editor
    {
        SerializedProperty _content;
        SerializedProperty _tempContent;

        protected virtual void OnEnable() {
            _content = serializedObject.FindProperty("_content");
            _tempContent = serializedObject.FindProperty("_tempContent");
        }

        public override void OnInspectorGUI() {
            serializedObject.Update();

            EditorGUILayout.LabelField("Sidebar Widget Container", EditorStyles.boldLabel);
            EditorGUILayout.PropertyField(_content, new GUIContent("Content"));
            EditorGUILayout.PropertyField(_tempContent, new GUIContent("Temporary Content"));

            serializedObject.ApplyModifiedProperties();
        }
    }
}
