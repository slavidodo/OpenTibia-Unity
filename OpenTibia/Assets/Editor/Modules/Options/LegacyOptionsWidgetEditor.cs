using UnityEditor;
using UnityEngine;

namespace OpenTibiaUnityEditor.Modules.Options
{
    [CustomEditor(typeof(OpenTibiaUnity.Modules.Options.LegacyOptionsWidget), true)]
    [CanEditMultipleObjects]
    public class LegacyOptionsWidgetEditor : UI.Legacy.PopUpBaseEditor
    {
        SerializedProperty _legacyOptionsWidgetItemTemplate;
        SerializedProperty _legacyOptionsWidgetItemGreenTemplate;

        protected override void OnEnable() {
            base.OnEnable();

            _legacyOptionsWidgetItemTemplate = serializedObject.FindProperty("_legacyOptionsWidgetItemTemplate");
            _legacyOptionsWidgetItemGreenTemplate = serializedObject.FindProperty("_legacyOptionsWidgetItemGreenTemplate");
        }

        public override void OnInspectorGUI() {
            base.OnInspectorGUI();

            EditorGUILayout.LabelField("", GUI.skin.horizontalSlider);
            EditorGUILayout.LabelField("Legacy Options Widget", EditorStyles.boldLabel);
            EditorGUILayout.PropertyField(_legacyOptionsWidgetItemTemplate, new GUIContent("Default Template"));
            EditorGUILayout.PropertyField(_legacyOptionsWidgetItemGreenTemplate, new GUIContent("GreenButton Template"));

            serializedObject.ApplyModifiedProperties();
        }
    }
}
