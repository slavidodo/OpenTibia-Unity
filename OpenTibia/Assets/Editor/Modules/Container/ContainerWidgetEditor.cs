using UnityEditor;
using UnityEngine;

namespace OpenTibiaUnityEditor.Modules.Container
{
    [CustomEditor(typeof(OpenTibiaUnity.Modules.Container.ContainerWidget), true)]
    [CanEditMultipleObjects]
    public class ContainerWidgetEditor : UI.Legacy.SidebarWidgetEditor
    {
        SerializedProperty _iconImage;
        SerializedProperty _itemsScrollRect;
        SerializedProperty _upButton;

        protected override void OnEnable() {
            base.OnEnable();

            _iconImage = serializedObject.FindProperty("_iconImage");
            _itemsScrollRect = serializedObject.FindProperty("_itemsScrollRect");
            _upButton = serializedObject.FindProperty("_upButton");
        }

        public override void OnInspectorGUI() {
            base.OnInspectorGUI();

            EditorGUILayout.LabelField("", GUI.skin.horizontalSlider);
            EditorGUILayout.LabelField("Container Widget", EditorStyles.boldLabel);
            EditorGUILayout.PropertyField(_iconImage, new GUIContent("Icon"));
            EditorGUILayout.PropertyField(_itemsScrollRect, new GUIContent("Items ScrollRect"));
            EditorGUILayout.PropertyField(_upButton, new GUIContent("UP Button"));

            serializedObject.ApplyModifiedProperties();
        }
    }
}
