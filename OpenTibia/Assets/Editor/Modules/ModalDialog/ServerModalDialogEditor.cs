using UnityEditor;
using UnityEngine;

namespace OpenTibiaUnityEditor.Modules.ModalDialog
{
    [CustomEditor(typeof(OpenTibiaUnity.Modules.ModalDialog.ServerModalDialog), true)]
    [CanEditMultipleObjects]
    public class ServerModalDialogEditor : UI.Legacy.MessageWidgetBaseEditor
    {
        SerializedProperty _choiceScrollRect;

        protected override void OnEnable() {
            base.OnEnable();

            _choiceScrollRect = serializedObject.FindProperty("_choiceScrollRect");
        }

        public override void OnInspectorGUI() {
            base.OnInspectorGUI();

            EditorGUILayout.LabelField("", GUI.skin.horizontalSlider);
            EditorGUILayout.LabelField("Server Modal Dialog", EditorStyles.boldLabel);
            EditorGUILayout.PropertyField(_choiceScrollRect, new GUIContent("Choices ScrollRect"));

            serializedObject.ApplyModifiedProperties();
        }
    }
}
