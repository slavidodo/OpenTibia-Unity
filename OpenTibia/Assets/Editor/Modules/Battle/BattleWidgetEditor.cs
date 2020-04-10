using UnityEngine;
using UnityEditor;

namespace OpenTibiaUnityEditor.Modules.Battle
{
    [CustomEditor(typeof(OpenTibiaUnity.Modules.Battle.BattleWidget), true)]
    [CanEditMultipleObjects]
    public class BattleWidgetEditor : UI.Legacy.SidebarWidgetEditor
    {

        SerializedProperty _filtersPanel;
        SerializedProperty _showSortTypesButton;
        SerializedProperty _filtersPanelToggle;
        SerializedProperty _filterPlayersToggle;
        SerializedProperty _filterNPCsToggle;
        SerializedProperty _filterMonstersToggle;
        SerializedProperty _filterNonSkulledToggle;
        SerializedProperty _filterPartyToggle;
        SerializedProperty _filterSummonsToggle;
        SerializedProperty _battleList;

        protected override void OnEnable() {
            base.OnEnable();

            _filtersPanel = serializedObject.FindProperty("_filtersPanel");
            _showSortTypesButton = serializedObject.FindProperty("_showSortTypesButton");
            _filtersPanelToggle = serializedObject.FindProperty("_filtersPanelToggle");

            _filterPlayersToggle = serializedObject.FindProperty("_filterPlayersToggle");
            _filterNPCsToggle = serializedObject.FindProperty("_filterNPCsToggle");
            _filterMonstersToggle = serializedObject.FindProperty("_filterMonstersToggle");
            _filterNonSkulledToggle = serializedObject.FindProperty("_filterNonSkulledToggle");
            _filterPartyToggle = serializedObject.FindProperty("_filterPartyToggle");
            _filterSummonsToggle = serializedObject.FindProperty("_filterSummonsToggle");
            _battleList = serializedObject.FindProperty("_battleList");
        }

        bool filtersPanelActive = false;

        public override void OnInspectorGUI() {
            base.OnInspectorGUI();

            EditorGUILayout.LabelField("", GUI.skin.horizontalSlider);
            EditorGUILayout.LabelField("Battle Widget", EditorStyles.boldLabel);
            EditorGUILayout.PropertyField(_filtersPanel, new GUIContent("Filters Panel"));
            EditorGUILayout.PropertyField(_showSortTypesButton, new GUIContent("ShowSortTypes Button"));
            EditorGUILayout.PropertyField(_filtersPanelToggle, new GUIContent("FilterPanels Toggle"));
            EditorGUILayout.PropertyField(_battleList, new GUIContent("Battle List"));

            EditorGUI.indentLevel++;
            filtersPanelActive = EditorGUILayout.Foldout(filtersPanelActive, "Filters Panel", true);
            if (filtersPanelActive) {

                EditorGUILayout.PropertyField(_filterPlayersToggle, new GUIContent("Players Toggle"));
                EditorGUILayout.PropertyField(_filterNPCsToggle, new GUIContent("NPCs Toggle"));
                EditorGUILayout.PropertyField(_filterMonstersToggle, new GUIContent("Monsters Toggle"));
                EditorGUILayout.PropertyField(_filterNonSkulledToggle, new GUIContent("NonSkulled Toggle"));
                EditorGUILayout.PropertyField(_filterPartyToggle, new GUIContent("Party Toggle"));
                EditorGUILayout.PropertyField(_filterSummonsToggle, new GUIContent("Summons Toggle"));
            }
            EditorGUI.indentLevel--;

            serializedObject.ApplyModifiedProperties();
        }
    }
}
