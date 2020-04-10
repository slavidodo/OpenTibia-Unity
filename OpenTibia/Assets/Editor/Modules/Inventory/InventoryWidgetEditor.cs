using UnityEditor;
using UnityEngine;

namespace OpenTibiaUnityEditor.Modules.Inventory
{
    [CustomEditor(typeof(OpenTibiaUnity.Modules.Inventory.InventoryWidget), true)]
    [CanEditMultipleObjects]
    public class InventoryWidgetEditor : UI.Legacy.SidebarWidgetEditor
    {
        SerializedProperty _bodyContainer;
        SerializedProperty _combatPanel;
        SerializedProperty _stopButton;
        SerializedProperty _toggleStyleButton;
        SerializedProperty _storeInboxButton;
        SerializedProperty _legacyStoreButton;
        SerializedProperty _legacyStoreInboxButton;
        SerializedProperty _legacyStopButton;
        SerializedProperty _legacyQuestlogButton;
        SerializedProperty _legacyOptionsButton;
        SerializedProperty _legacyHelpButton;
        SerializedProperty _legacyLogoutButton;
        SerializedProperty _legacySkillsToggle;
        SerializedProperty _legacyBattleToggle;
        SerializedProperty _legacyBuddyToggle;
        SerializedProperty _capacityLabel;
        SerializedProperty _soulPointsLabel;
        SerializedProperty _conditionsPanel;
        SerializedProperty _minimizeNormalSprite;
        SerializedProperty _minimizePressedSprite;
        SerializedProperty _maximizeNormalSprite;
        SerializedProperty _maximizePressedSprite;

        protected override void OnEnable() {
            base.OnEnable();

            _bodyContainer = serializedObject.FindProperty("_bodyContainer");
            _combatPanel = serializedObject.FindProperty("_combatPanel");
            _stopButton = serializedObject.FindProperty("_stopButton");
            _toggleStyleButton = serializedObject.FindProperty("_toggleStyleButton");
            _storeInboxButton = serializedObject.FindProperty("_storeInboxButton");
            _legacyStoreButton = serializedObject.FindProperty("_legacyStoreButton");
            _legacyStoreInboxButton = serializedObject.FindProperty("_legacyStoreInboxButton");
            _legacyStopButton = serializedObject.FindProperty("_legacyStopButton");
            _legacyQuestlogButton = serializedObject.FindProperty("_legacyQuestlogButton");
            _legacyOptionsButton = serializedObject.FindProperty("_legacyOptionsButton");
            _legacyHelpButton = serializedObject.FindProperty("_legacyHelpButton");
            _legacyLogoutButton = serializedObject.FindProperty("_legacyLogoutButton");
            _legacySkillsToggle = serializedObject.FindProperty("_legacySkillsToggle");
            _legacyBattleToggle = serializedObject.FindProperty("_legacyBattleToggle");
            _legacyBuddyToggle = serializedObject.FindProperty("_legacyBuddyToggle");
            _capacityLabel = serializedObject.FindProperty("_capacityLabel");
            _soulPointsLabel = serializedObject.FindProperty("_soulPointsLabel");
            _conditionsPanel = serializedObject.FindProperty("_conditionsPanel");
            _minimizeNormalSprite = serializedObject.FindProperty("_minimizeNormalSprite");
            _minimizePressedSprite = serializedObject.FindProperty("_minimizePressedSprite");
            _maximizeNormalSprite = serializedObject.FindProperty("_maximizeNormalSprite");
            _maximizePressedSprite = serializedObject.FindProperty("_maximizePressedSprite");
        }

        public override void OnInspectorGUI() {
            base.OnInspectorGUI();

            EditorGUILayout.LabelField("", GUI.skin.horizontalSlider);
            EditorGUILayout.LabelField("Inventory Widget", EditorStyles.boldLabel);

            EditorGUILayout.PropertyField(_bodyContainer, new GUIContent("Body Container"));
            EditorGUILayout.PropertyField(_combatPanel, new GUIContent("Combat Panel"));
            EditorGUILayout.PropertyField(_stopButton, new GUIContent("Stop Button"));
            EditorGUILayout.PropertyField(_toggleStyleButton, new GUIContent("ToggleStyle Button"));
            EditorGUILayout.PropertyField(_storeInboxButton, new GUIContent("StoreInbox Button"));
            EditorGUILayout.PropertyField(_legacyStoreButton, new GUIContent("Legacy Store Button"));
            EditorGUILayout.PropertyField(_legacyStoreInboxButton, new GUIContent("Legacy StoreInbox Button"));
            EditorGUILayout.PropertyField(_legacyStopButton, new GUIContent("Legacy Stop Button"));
            EditorGUILayout.PropertyField(_legacyQuestlogButton, new GUIContent("Legacy Questlog Button"));
            EditorGUILayout.PropertyField(_legacyOptionsButton, new GUIContent("Legacy Options Button"));
            EditorGUILayout.PropertyField(_legacyHelpButton, new GUIContent("Legacy Help Button"));
            EditorGUILayout.PropertyField(_legacyLogoutButton, new GUIContent("Legacy Logout Button"));
            EditorGUILayout.PropertyField(_legacySkillsToggle, new GUIContent("Legacy Skills Toggle"));
            EditorGUILayout.PropertyField(_legacyBattleToggle, new GUIContent("Legacy Battle Toggle"));
            EditorGUILayout.PropertyField(_legacyBuddyToggle, new GUIContent("Legacy Buddy Toggle"));
            EditorGUILayout.PropertyField(_capacityLabel, new GUIContent("Capacity Label"));
            EditorGUILayout.PropertyField(_soulPointsLabel, new GUIContent("SoulPoints Label"));
            EditorGUILayout.PropertyField(_conditionsPanel, new GUIContent("Conditions Panel"));
            EditorGUILayout.PropertyField(_minimizeNormalSprite, new GUIContent("Minimize Normal Sprite"));
            EditorGUILayout.PropertyField(_minimizePressedSprite, new GUIContent("Minimize Pressed Sprite"));
            EditorGUILayout.PropertyField(_maximizeNormalSprite, new GUIContent("Maximize Normal Sprite"));
            EditorGUILayout.PropertyField(_maximizePressedSprite, new GUIContent("Maximize Pressed Sprite"));

            serializedObject.ApplyModifiedProperties();
        }
    }
}
