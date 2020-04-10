using UnityEngine;
using UnityEditor;

namespace OpenTibiaUnityEditor.Modules.Console
{
    [CustomEditor(typeof(OpenTibiaUnity.Modules.Console.ChannelTab), true)]
    [CanEditMultipleObjects]
    public class ChannelTabEditor : UI.Legacy.ButtonEditor
    {

        SerializedProperty _activeFixImage;

        protected override void OnEnable() {
            base.OnEnable();

            _activeFixImage = serializedObject.FindProperty("_activeFixImage");
        }

        public override void OnInspectorGUI() {
            base.OnInspectorGUI();

            EditorGUILayout.LabelField("", GUI.skin.horizontalSlider);
            EditorGUILayout.LabelField("Channel TabButton", EditorStyles.boldLabel);
            EditorGUILayout.PropertyField(_activeFixImage, new GUIContent("Active Fix Image"));

            serializedObject.ApplyModifiedProperties();
        }
    }
}
