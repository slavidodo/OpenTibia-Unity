using UnityEditor;
using UnityEngine;

namespace OpenTibiaUnityEditor.Modules.ToggleButtons
{
    [CustomEditor(typeof(OpenTibiaUnity.Modules.ToggleButtons.ToggleButtonsWidget), true)]
    [CanEditMultipleObjects]
    public class ToggleButtonsWidgetEditor : UI.Legacy.SidebarWidgetEditor
    {
        SerializedProperty _rightPanel;
        SerializedProperty _leftPanel;
        SerializedProperty _storeButton;
        SerializedProperty _dockToggle;
        SerializedProperty _skillsToggle;
        SerializedProperty _battleToggle;
        SerializedProperty _buddyToggle;
        SerializedProperty _questlogButton;
        SerializedProperty _rewardWallButton;
        SerializedProperty _spellListToggle;
        SerializedProperty _unjustFragsToggle;
        SerializedProperty _preyToggle;
        SerializedProperty _optionsButton;
        SerializedProperty _logoutButton;
        SerializedProperty _analyticsToggle;

        protected override void OnEnable() {
            base.OnEnable();
            _rightPanel = serializedObject.FindProperty("_rightPanel");
            _leftPanel = serializedObject.FindProperty("_leftPanel");
            _storeButton = serializedObject.FindProperty("_storeButton");
            _dockToggle = serializedObject.FindProperty("_dockToggle");
            _skillsToggle = serializedObject.FindProperty("_skillsToggle");
            _battleToggle = serializedObject.FindProperty("_battleToggle");
            _buddyToggle = serializedObject.FindProperty("_buddyToggle");
            _questlogButton = serializedObject.FindProperty("_questlogButton");
            _questlogButton = serializedObject.FindProperty("_questlogButton");
            _rewardWallButton = serializedObject.FindProperty("_rewardWallButton");
            _spellListToggle = serializedObject.FindProperty("_spellListToggle");
            _unjustFragsToggle = serializedObject.FindProperty("_unjustFragsToggle");
            _preyToggle = serializedObject.FindProperty("_preyToggle");
            _optionsButton = serializedObject.FindProperty("_optionsButton");
            _logoutButton = serializedObject.FindProperty("_logoutButton");
            _analyticsToggle = serializedObject.FindProperty("_analyticsToggle");
        }

        public override void OnInspectorGUI() {
            base.OnInspectorGUI();

            EditorGUILayout.LabelField("", GUI.skin.horizontalSlider);
            EditorGUILayout.LabelField("ToggleButtons Widget", EditorStyles.boldLabel);
            EditorGUILayout.PropertyField(_rightPanel, new GUIContent("Right Panel"));
            EditorGUILayout.PropertyField(_leftPanel, new GUIContent("Left Panel"));
            EditorGUILayout.PropertyField(_storeButton, new GUIContent("Store Button"));
            EditorGUILayout.PropertyField(_dockToggle, new GUIContent("Dock Toggle"));
            EditorGUILayout.PropertyField(_skillsToggle, new GUIContent("Skills Toggle"));
            EditorGUILayout.PropertyField(_battleToggle, new GUIContent("Battle Toggle"));
            EditorGUILayout.PropertyField(_buddyToggle, new GUIContent("VIP Button"));
            EditorGUILayout.PropertyField(_questlogButton, new GUIContent("Questlog Button"));
            EditorGUILayout.PropertyField(_rewardWallButton, new GUIContent("RewardWall Button"));
            EditorGUILayout.PropertyField(_spellListToggle, new GUIContent("SpellList Toggle"));
            EditorGUILayout.PropertyField(_unjustFragsToggle, new GUIContent("UnjustFrags Toggle"));
            EditorGUILayout.PropertyField(_preyToggle, new GUIContent("Prey Toggle"));
            EditorGUILayout.PropertyField(_optionsButton, new GUIContent("Options Button"));
            EditorGUILayout.PropertyField(_logoutButton, new GUIContent("Logout Button"));
            EditorGUILayout.PropertyField(_analyticsToggle, new GUIContent("Analytics Toggle"));

            serializedObject.ApplyModifiedProperties();
        }
    }
}
