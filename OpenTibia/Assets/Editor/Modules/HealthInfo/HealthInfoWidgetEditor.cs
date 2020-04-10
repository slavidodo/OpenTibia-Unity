using UnityEditor;
using UnityEngine;

namespace OpenTibiaUnityEditor.Modules.ToggleButtons
{
    [CustomEditor(typeof(OpenTibiaUnity.Modules.HealthInfo.HealthInfoWidget), true)]
    [CanEditMultipleObjects]
    public class HealthInfoWidgetEditor : UI.Legacy.SidebarWidgetEditor
    {
        SerializedProperty _healthBar;
        SerializedProperty _manaBar;

        protected override void OnEnable() {
            base.OnEnable();

            _healthBar = serializedObject.FindProperty("_healthBar");
            _manaBar = serializedObject.FindProperty("_manaBar");
        }

        public override void OnInspectorGUI() {
            base.OnInspectorGUI();

            EditorGUILayout.LabelField("", GUI.skin.horizontalSlider);
            EditorGUILayout.LabelField("HealthInfo Widget", EditorStyles.boldLabel);
            EditorGUILayout.PropertyField(_healthBar, new GUIContent("Health Bar"));
            EditorGUILayout.PropertyField(_manaBar, new GUIContent("Mana Bar"));

            serializedObject.ApplyModifiedProperties();
        }
    }
}
