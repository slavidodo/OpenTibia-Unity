using UnityEditor;
using UnityEngine;

namespace OpenTibiaUnityEditor.Modules.ToggleButtons
{
    [CustomEditor(typeof(OpenTibiaUnity.Modules.MiniMap.MiniMapWidget), true)]
    [CanEditMultipleObjects]
    public class MiniMapWidgetEditor : UI.Legacy.SidebarWidgetEditor
    {
        SerializedProperty _zLayerUpButton;
        SerializedProperty _zLayerDownButton;
        SerializedProperty _zoomOutButton;
        SerializedProperty _zoomInButton;
        SerializedProperty _centerButton;

        protected override void OnEnable() {
            base.OnEnable();

            _zLayerUpButton = serializedObject.FindProperty("_zLayerUpButton");
            _zLayerDownButton = serializedObject.FindProperty("_zLayerDownButton");
            _zoomOutButton = serializedObject.FindProperty("_zoomOutButton");
            _zoomInButton = serializedObject.FindProperty("_zoomInButton");
            _centerButton = serializedObject.FindProperty("_centerButton");
        }

        public override void OnInspectorGUI() {
            base.OnInspectorGUI();

            EditorGUILayout.LabelField("", GUI.skin.horizontalSlider);
            EditorGUILayout.LabelField("MiniMap Widget", EditorStyles.boldLabel);
            EditorGUILayout.PropertyField(_zLayerUpButton, new GUIContent("Z-Layer Up Button"));
            EditorGUILayout.PropertyField(_zLayerDownButton, new GUIContent("Z-Layer Down Button"));
            EditorGUILayout.PropertyField(_zoomOutButton, new GUIContent("Zoom Out Button"));
            EditorGUILayout.PropertyField(_zoomInButton, new GUIContent("Zoom In Button"));
            EditorGUILayout.PropertyField(_centerButton, new GUIContent("Center Button"));

            serializedObject.ApplyModifiedProperties();
        }
    }
}
