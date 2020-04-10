using UnityEditor;
using UnityEngine;

namespace OpenTibiaUnityEditor.Modules.Login
{
    [CustomEditor(typeof(OpenTibiaUnity.Modules.Login.AuthenticatorWidget), true)]
    [CanEditMultipleObjects]
    public class AuthenticatorWidgetEditor : UI.Legacy.PopUpBaseEditor
    {
        SerializedProperty _token;

        protected override void OnEnable() {
            base.OnEnable();

            _token = serializedObject.FindProperty("_token");
        }

        public override void OnInspectorGUI() {
            base.OnInspectorGUI();

            EditorGUILayout.LabelField("", GUI.skin.horizontalSlider);
            EditorGUILayout.LabelField("Authenticator Widget", EditorStyles.boldLabel);
            EditorGUILayout.PropertyField(_token, new GUIContent("Token"));

            serializedObject.ApplyModifiedProperties();
        }
    }
}
