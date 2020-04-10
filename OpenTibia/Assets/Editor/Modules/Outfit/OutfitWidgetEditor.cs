using UnityEditor;
using UnityEngine;

namespace OpenTibiaUnityEditor.Modules.Outfit
{
    [CustomEditor(typeof(OpenTibiaUnity.Modules.Outfit.OutfitWidget), true)]
    [CanEditMultipleObjects]
    public class OutfitWidgetEditor : UI.Legacy.PopUpBaseEditor
    {
        SerializedProperty _colorsPalette;
        SerializedProperty _headToggle;
        SerializedProperty _bodyToggle;
        SerializedProperty _legsToggle;
        SerializedProperty _feetToggle;
        SerializedProperty _informationLabel;
        SerializedProperty _legacyNextOutfitButton;
        SerializedProperty _nextOutfitButton;
        SerializedProperty _prevOutfitButton;
        SerializedProperty _outfitNamePanel;
        SerializedProperty _outfitImage;
        SerializedProperty _outfitNameLabel;
        SerializedProperty _addonsPanel;
        SerializedProperty _addon1Checkbox;
        SerializedProperty _addon2Checkbox;
        SerializedProperty _addon3Checkbox;
        SerializedProperty _mountPanel;
        SerializedProperty _nextMountButton;
        SerializedProperty _prevMountButton;
        SerializedProperty _mountNameLabel;
        SerializedProperty _mountImage;

        bool outfitPanelActive = false;
        bool mountPanelActive = false;

        protected override void OnEnable() {
            base.OnEnable();

            _colorsPalette = serializedObject.FindProperty("_colorsPalette");
            _headToggle = serializedObject.FindProperty("_headToggle");
            _bodyToggle = serializedObject.FindProperty("_bodyToggle");
            _legsToggle = serializedObject.FindProperty("_legsToggle");
            _feetToggle = serializedObject.FindProperty("_feetToggle");
            _informationLabel = serializedObject.FindProperty("_informationLabel");
            _legacyNextOutfitButton = serializedObject.FindProperty("_legacyNextOutfitButton");
            _nextOutfitButton = serializedObject.FindProperty("_nextOutfitButton");
            _prevOutfitButton = serializedObject.FindProperty("_prevOutfitButton");
            _outfitNamePanel = serializedObject.FindProperty("_outfitNamePanel");
            _outfitNameLabel = serializedObject.FindProperty("_outfitNameLabel");
            _outfitImage = serializedObject.FindProperty("_outfitImage");
            _addonsPanel = serializedObject.FindProperty("_addonsPanel");
            _addon1Checkbox = serializedObject.FindProperty("_addon1Checkbox");
            _addon2Checkbox = serializedObject.FindProperty("_addon2Checkbox");
            _addon3Checkbox = serializedObject.FindProperty("_addon3Checkbox");
            _mountPanel = serializedObject.FindProperty("_mountPanel");
            _nextMountButton = serializedObject.FindProperty("_nextMountButton");
            _prevMountButton = serializedObject.FindProperty("_prevMountButton");
            _mountNameLabel = serializedObject.FindProperty("_mountNameLabel");
            _mountImage = serializedObject.FindProperty("_mountImage");
        }

        public override void OnInspectorGUI() {
            base.OnInspectorGUI();

            EditorGUILayout.LabelField("", GUI.skin.horizontalSlider);
            EditorGUILayout.LabelField("Outfit Widget", EditorStyles.boldLabel);

            EditorGUILayout.PropertyField(_colorsPalette, new GUIContent("Colors Palette"));
            EditorGUILayout.PropertyField(_headToggle, new GUIContent("Head Toggle"));
            EditorGUILayout.PropertyField(_bodyToggle, new GUIContent("Body Toggle"));
            EditorGUILayout.PropertyField(_legsToggle, new GUIContent("Legs Toggle"));
            EditorGUILayout.PropertyField(_feetToggle, new GUIContent("Feet Toggle"));
            EditorGUILayout.PropertyField(_informationLabel, new GUIContent("Info Label"));

            EditorGUI.indentLevel++;
            outfitPanelActive = EditorGUILayout.Foldout(outfitPanelActive, "Outfit Panel", true);
            if (outfitPanelActive) {
                EditorGUILayout.PropertyField(_legacyNextOutfitButton, new GUIContent("Legacy Next Button"));
                EditorGUILayout.PropertyField(_nextOutfitButton, new GUIContent("Next Button"));
                EditorGUILayout.PropertyField(_prevOutfitButton, new GUIContent("Prev Button"));
                EditorGUILayout.PropertyField(_outfitNamePanel, new GUIContent("Name Panel"));
                EditorGUILayout.PropertyField(_outfitNameLabel, new GUIContent("Name Label"));
                EditorGUILayout.PropertyField(_outfitImage, new GUIContent("Outfit Image"));
                EditorGUILayout.PropertyField(_addonsPanel, new GUIContent("Addons Panel"));
                EditorGUILayout.PropertyField(_addon1Checkbox, new GUIContent("Addon-1 Checkbox"));
                EditorGUILayout.PropertyField(_addon2Checkbox, new GUIContent("Addon-2 Checkbox"));
                EditorGUILayout.PropertyField(_addon3Checkbox, new GUIContent("Addon-3 Checkbox"));
            }

            mountPanelActive = EditorGUILayout.Foldout(mountPanelActive, "Mount Panel", true);
            if (mountPanelActive) {
                EditorGUILayout.PropertyField(_mountPanel, new GUIContent("Mount Panel"));
                EditorGUILayout.PropertyField(_nextMountButton, new GUIContent("Next Button"));
                EditorGUILayout.PropertyField(_prevMountButton, new GUIContent("Prev Button"));
                EditorGUILayout.PropertyField(_mountNameLabel, new GUIContent("Name Label"));
                EditorGUILayout.PropertyField(_mountImage, new GUIContent("Mount Image"));
            }
            EditorGUI.indentLevel--;

            serializedObject.ApplyModifiedProperties();
        }
    }
}
