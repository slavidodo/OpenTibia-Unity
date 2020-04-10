using UnityEditor;
using UnityEngine;

namespace OpenTibiaUnityEditor.UI.Legacy
{
    [CustomEditor(typeof(OpenTibiaUnity.UI.Legacy.MessageWidgetBase), true)]
    [CanEditMultipleObjects]
    public class MessageWidgetBaseEditor : PopUpBaseEditor
    {
        SerializedProperty _message;

        protected override void OnEnable() {
            base.OnEnable();

            _message = serializedObject.FindProperty("_message");
        }

        public override void OnInspectorGUI() {
            base.OnInspectorGUI();

            EditorGUILayout.LabelField("", GUI.skin.horizontalSlider);
            EditorGUILayout.LabelField("Message Widget Base", EditorStyles.boldLabel);
            EditorGUILayout.PropertyField(_message, new GUIContent("Message"));

            serializedObject.ApplyModifiedProperties();
        }
    }
}
