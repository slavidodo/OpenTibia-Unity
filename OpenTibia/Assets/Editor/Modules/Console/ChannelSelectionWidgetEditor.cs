using UnityEngine;
using UnityEditor;

namespace OpenTibiaUnityEditor.Modules.Console
{
    [CustomEditor(typeof(OpenTibiaUnity.Modules.Console.ChannelSelectionWidget), true)]
    [CanEditMultipleObjects]
    public class ChannelSelectionWidgetEditor : UI.Legacy.PopUpBaseEditor
    {

        SerializedProperty _channelScrollRect;
        SerializedProperty _channelInput;
        SerializedProperty _channelToggleGroup;

        protected override void OnEnable() {
            base.OnEnable();

            _channelScrollRect = serializedObject.FindProperty("_channelScrollRect");
            _channelInput = serializedObject.FindProperty("_channelInput");
            _channelToggleGroup = serializedObject.FindProperty("_channelToggleGroup");
        }

        public override void OnInspectorGUI() {
            base.OnInspectorGUI();

            EditorGUILayout.LabelField("", GUI.skin.horizontalSlider);
            EditorGUILayout.LabelField("Channel Selection Widget", EditorStyles.boldLabel);
            EditorGUILayout.PropertyField(_channelScrollRect, new GUIContent("Channel ScrollRect"));
            EditorGUILayout.PropertyField(_channelInput, new GUIContent("Channel Input"));
            EditorGUILayout.PropertyField(_channelToggleGroup, new GUIContent("Channel ToggleGroup"));

            serializedObject.ApplyModifiedProperties();
        }
    }
}
