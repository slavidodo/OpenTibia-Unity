using UnityEditor;
using UnityEngine;

namespace OpenTibiaUnityEditor.Modules.Options
{
    [CustomEditor(typeof(OpenTibiaUnity.Modules.Options.LegacyGraphicsOptionsWidget), true)]
    [CanEditMultipleObjects]
    public class LegacyGraphicsOptionsWidgetEditor : UI.Legacy.PopUpBaseEditor
    {
        SerializedProperty _resolutionDropdown;
        SerializedProperty _qualityDropdown;
        SerializedProperty _fullscreenCheckbox;
        SerializedProperty _vsyncCheckbox;
        SerializedProperty _antialiasingCheckbox;
        SerializedProperty _noFramerateLimitCheckbox;
        SerializedProperty _framerateLimitSlider;
        SerializedProperty _showLightEffectsCheckbox;
        SerializedProperty _ambientLightSlider;

        protected override void OnEnable() {
            base.OnEnable();

            _resolutionDropdown = serializedObject.FindProperty("_resolutionDropdown");
            _qualityDropdown = serializedObject.FindProperty("_qualityDropdown");
            _fullscreenCheckbox = serializedObject.FindProperty("_fullscreenCheckbox");
            _vsyncCheckbox = serializedObject.FindProperty("_vsyncCheckbox");
            _antialiasingCheckbox = serializedObject.FindProperty("_antialiasingCheckbox");
            _noFramerateLimitCheckbox = serializedObject.FindProperty("_noFramerateLimitCheckbox");
            _framerateLimitSlider = serializedObject.FindProperty("_framerateLimitSlider");
            _showLightEffectsCheckbox = serializedObject.FindProperty("_showLightEffectsCheckbox");
            _ambientLightSlider = serializedObject.FindProperty("_ambientLightSlider");
        }

        public override void OnInspectorGUI() {
            base.OnInspectorGUI();

            EditorGUILayout.LabelField("", GUI.skin.horizontalSlider);
            EditorGUILayout.LabelField("Legacy GraphicsOptions Widget", EditorStyles.boldLabel);
            EditorGUILayout.PropertyField(_resolutionDropdown, new GUIContent("Resolution Dropdown"));
            EditorGUILayout.PropertyField(_qualityDropdown, new GUIContent("Quality Dropdown"));
            EditorGUILayout.PropertyField(_fullscreenCheckbox, new GUIContent("Fullscreen Checkbox"));
            EditorGUILayout.PropertyField(_vsyncCheckbox, new GUIContent("VSync Checkbox"));
            EditorGUILayout.PropertyField(_antialiasingCheckbox, new GUIContent("Antialiasing Checkbox"));
            EditorGUILayout.PropertyField(_noFramerateLimitCheckbox, new GUIContent("No FramerateLimit Checkbox"));
            EditorGUILayout.PropertyField(_framerateLimitSlider, new GUIContent("FramerateLimit Slider"));
            EditorGUILayout.PropertyField(_showLightEffectsCheckbox, new GUIContent("Show LightEffects Checkbox"));
            EditorGUILayout.PropertyField(_ambientLightSlider, new GUIContent("Ambient Light Slider"));

            serializedObject.ApplyModifiedProperties();
        }
    }
}
