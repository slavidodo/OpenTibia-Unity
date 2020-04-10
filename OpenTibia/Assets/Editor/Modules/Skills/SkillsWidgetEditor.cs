using UnityEngine;
using UnityEditor;

namespace OpenTibiaUnityEditor.Modules.Skills
{
    [CustomEditor(typeof(OpenTibiaUnity.Modules.Skills.SkillsWidget), true)]
    [CanEditMultipleObjects]
    public class SkillsWidgetEditor : UI.Legacy.SidebarWidgetEditor
    {

        SerializedProperty _skillScrollRect;
        SerializedProperty _magicIcon;
        SerializedProperty _fistIcon;
        SerializedProperty _clubIcon;
        SerializedProperty _swordIcon;
        SerializedProperty _axeIcon;
        SerializedProperty _distIcon;
        SerializedProperty _shieldingIcon;
        SerializedProperty _fishingIcon;

        protected override void OnEnable() {
            base.OnEnable();

            _skillScrollRect = serializedObject.FindProperty("_skillScrollRect");
            _magicIcon = serializedObject.FindProperty("_magicIcon");
            _fistIcon = serializedObject.FindProperty("_fistIcon");
            _clubIcon = serializedObject.FindProperty("_clubIcon");
            _swordIcon = serializedObject.FindProperty("_swordIcon");
            _axeIcon = serializedObject.FindProperty("_axeIcon");
            _distIcon = serializedObject.FindProperty("_distIcon");
            _shieldingIcon = serializedObject.FindProperty("_shieldingIcon");
            _fishingIcon = serializedObject.FindProperty("_fishingIcon");
        }

        public override void OnInspectorGUI() {
            base.OnInspectorGUI();


            EditorGUILayout.LabelField("", GUI.skin.horizontalSlider);
            EditorGUILayout.LabelField("Skills Widget", EditorStyles.boldLabel);
            EditorGUILayout.PropertyField(_skillScrollRect, new GUIContent("Skills ScrollRect"));
            EditorGUILayout.PropertyField(_magicIcon, new GUIContent("Magic Icon"));
            EditorGUILayout.PropertyField(_fistIcon, new GUIContent("Fist Icon"));
            EditorGUILayout.PropertyField(_clubIcon, new GUIContent("Club Icon"));
            EditorGUILayout.PropertyField(_swordIcon, new GUIContent("Sword Icon"));
            EditorGUILayout.PropertyField(_axeIcon, new GUIContent("Axe Icon"));
            EditorGUILayout.PropertyField(_distIcon, new GUIContent("Distance Icon"));
            EditorGUILayout.PropertyField(_shieldingIcon, new GUIContent("Shielding Icon"));
            EditorGUILayout.PropertyField(_fishingIcon, new GUIContent("Fishing Icon"));

            serializedObject.ApplyModifiedProperties();
        }
    }
}
