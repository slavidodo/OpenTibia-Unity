using UnityEditor;
using UnityEngine;

namespace OpenTibiaUnityEditor.Modules.Login
{
    [CustomEditor(typeof(OpenTibiaUnity.Modules.Login.CharacterSelectionWidget), true)]
    [CanEditMultipleObjects]
    public class CharacterSelectionWidgetEditor : UI.Legacy.PopUpBaseEditor
    {
        SerializedProperty _characterScrollRect;
        SerializedProperty _characterToggleGroup;
        SerializedProperty _getPremiumLegacyButton;
        SerializedProperty _getPremiumV12Button;
        SerializedProperty _accountStatusLabel;
        SerializedProperty _premiumPanel;

        protected override void OnEnable() {
            base.OnEnable();

            _characterScrollRect = serializedObject.FindProperty("_characterScrollRect");
            _characterToggleGroup = serializedObject.FindProperty("_characterToggleGroup");
            _getPremiumLegacyButton = serializedObject.FindProperty("_getPremiumLegacyButton");
            _getPremiumV12Button = serializedObject.FindProperty("_getPremiumV12Button");
            _accountStatusLabel = serializedObject.FindProperty("_accountStatusLabel");
            _premiumPanel = serializedObject.FindProperty("_premiumPanel");
        }

        public override void OnInspectorGUI() {
            base.OnInspectorGUI();

            EditorGUILayout.LabelField("", GUI.skin.horizontalSlider);
            EditorGUILayout.LabelField("Character Selection Widget", EditorStyles.boldLabel);
            EditorGUILayout.PropertyField(_characterScrollRect, new GUIContent("Characters ScrollRect"));
            EditorGUILayout.PropertyField(_characterToggleGroup, new GUIContent("Group"));
            EditorGUILayout.PropertyField(_getPremiumLegacyButton, new GUIContent("Premium Button (main)"));
            EditorGUILayout.PropertyField(_getPremiumV12Button, new GUIContent("Premium Button (V12)"));
            EditorGUILayout.PropertyField(_accountStatusLabel, new GUIContent("AccountStatus Label"));
            EditorGUILayout.PropertyField(_premiumPanel, new GUIContent("Premium Panel"));

            serializedObject.ApplyModifiedProperties();
        }
    }
}
