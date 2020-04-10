using UnityEditor;
using UnityEngine;

namespace OpenTibiaUnityEditor.UI.Legacy
{
    [CustomEditor(typeof(OpenTibiaUnity.UI.Legacy.SidebarWidget), true)]
    [CanEditMultipleObjects]
    public class SidebarWidgetEditor : Editor
    {
        SerializedProperty _closable;
        SerializedProperty _resizable;
        SerializedProperty _minimizedHeight;
        SerializedProperty _minContentHeight;
        SerializedProperty _maxContentHeight;
        SerializedProperty _preferredContentHeight;
        SerializedProperty _title;
        SerializedProperty _content;
        SerializedProperty _closeButton;
        SerializedProperty _minimizeButton;
        SerializedProperty _maximizeButton;

        protected virtual void OnEnable() {
            _closable = serializedObject.FindProperty("_closable");
            _resizable = serializedObject.FindProperty("_resizable");
            _minimizedHeight = serializedObject.FindProperty("_minimizedHeight");
            _minContentHeight = serializedObject.FindProperty("_minContentHeight");
            _maxContentHeight = serializedObject.FindProperty("_maxContentHeight");
            _preferredContentHeight = serializedObject.FindProperty("_preferredContentHeight");
            _title = serializedObject.FindProperty("_title");
            _content = serializedObject.FindProperty("_content");
            _closeButton = serializedObject.FindProperty("_closeButton");
            _minimizeButton = serializedObject.FindProperty("_minimizeButton");
            _maximizeButton = serializedObject.FindProperty("_maximizeButton");
        }

        public override void OnInspectorGUI() {
            serializedObject.Update();

            EditorGUILayout.LabelField("Sidebar Widget", EditorStyles.boldLabel);
            EditorGUILayout.PropertyField(_closable, new GUIContent("Closable"));
            EditorGUILayout.PropertyField(_resizable, new GUIContent("Resizable"));
            EditorGUILayout.PropertyField(_content, new GUIContent("Content"));

            if (_closable.boolValue)
                EditorGUILayout.PropertyField(_minimizedHeight, new GUIContent("Minimized Height"));

            if (_resizable.boolValue) {
                EditorGUILayout.PropertyField(_minContentHeight, new GUIContent("Min Content Height"));
                EditorGUILayout.PropertyField(_maxContentHeight, new GUIContent("Max Content Height"));
                EditorGUILayout.PropertyField(_preferredContentHeight, new GUIContent("Preferred Content Height"));
                EditorGUILayout.PropertyField(_minimizeButton, new GUIContent("Minimize Button"));
                EditorGUILayout.PropertyField(_maximizeButton, new GUIContent("Maximize Button"));
            }

            if (_closable.boolValue) {
                EditorGUILayout.PropertyField(_title, new GUIContent("Title"));
                EditorGUILayout.PropertyField(_closeButton, new GUIContent("Close Button"));
            }

            serializedObject.ApplyModifiedProperties();
        }
    }
}
