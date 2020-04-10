using UnityEditor;
using UnityEngine;

namespace OpenTibiaUnityEditor.Modules.Options
{
    [CustomEditor(typeof(OpenTibiaUnity.Modules.Options.LegacyHotkeyOptionsWidget), true)]
    [CanEditMultipleObjects]
    public class LegacyHotkeyOptionsWidgetEditor : UI.Legacy.PopUpBaseEditor
    {
        SerializedProperty _hotkeyScrollRect;
        SerializedProperty _toggleGroup;
        SerializedProperty _inputField;
        SerializedProperty _autoSendCheckbox;
        SerializedProperty _objectImage;
        SerializedProperty _selectObjectButton;
        SerializedProperty _clearObjectButton;
        SerializedProperty _useOnYourselfToggle;
        SerializedProperty _useOnTargetToggle;
        SerializedProperty _useWithCrosshairsToggle;

        protected override void OnEnable() {
            base.OnEnable();

            _hotkeyScrollRect = serializedObject.FindProperty("_hotkeyScrollRect");
            _toggleGroup = serializedObject.FindProperty("_toggleGroup");
            _inputField = serializedObject.FindProperty("_inputField");
            _autoSendCheckbox = serializedObject.FindProperty("_autoSendCheckbox");
            _objectImage = serializedObject.FindProperty("_objectImage");
            _selectObjectButton = serializedObject.FindProperty("_selectObjectButton");
            _clearObjectButton = serializedObject.FindProperty("_clearObjectButton");
            _useOnYourselfToggle = serializedObject.FindProperty("_useOnYourselfToggle");
            _useOnTargetToggle = serializedObject.FindProperty("_useOnTargetToggle");
            _useWithCrosshairsToggle = serializedObject.FindProperty("_useWithCrosshairsToggle");
        }

        public override void OnInspectorGUI() {
            base.OnInspectorGUI();

            EditorGUILayout.LabelField("", GUI.skin.horizontalSlider);
            EditorGUILayout.LabelField("Legacy HotkeyOptions Widget", EditorStyles.boldLabel);
            EditorGUILayout.PropertyField(_hotkeyScrollRect, new GUIContent("Hotkeys ScrollRect"));
            EditorGUILayout.PropertyField(_toggleGroup, new GUIContent("Group"));
            EditorGUILayout.PropertyField(_inputField, new GUIContent("Input Field"));
            EditorGUILayout.PropertyField(_autoSendCheckbox, new GUIContent("AutoSend Checkboc"));
            EditorGUILayout.PropertyField(_objectImage, new GUIContent("Object Image"));
            EditorGUILayout.PropertyField(_selectObjectButton, new GUIContent("Select Object"));
            EditorGUILayout.PropertyField(_clearObjectButton, new GUIContent("Clear Object"));
            EditorGUILayout.PropertyField(_useOnYourselfToggle, new GUIContent("UseOnYourself Button"));
            EditorGUILayout.PropertyField(_useOnTargetToggle, new GUIContent("UseOnTarget Button"));
            EditorGUILayout.PropertyField(_useWithCrosshairsToggle, new GUIContent("UseWithCrosshairs Button"));

            serializedObject.ApplyModifiedProperties();
        }
    }
}
