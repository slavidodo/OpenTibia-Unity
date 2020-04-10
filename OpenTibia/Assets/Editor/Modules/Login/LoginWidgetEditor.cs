using UnityEditor;
using UnityEngine;

namespace OpenTibiaUnityEditor.Modules.Login
{
    [CustomEditor(typeof(OpenTibiaUnity.Modules.Login.LoginWidget), true)]
    [CanEditMultipleObjects]
    public class LoginWidgetEditor : UI.Legacy.PopUpBaseEditor
    {
        SerializedProperty _draggableComponent;
        SerializedProperty _accountIdentifierLabel;
        SerializedProperty _accountIdentifierInput;
        SerializedProperty _passwordInput;
        SerializedProperty _tokenInput;
        SerializedProperty _addressLabel;
        SerializedProperty _addressInput;
        SerializedProperty _clientVersionDropdown;
        SerializedProperty _buildVersionDropdown;
        SerializedProperty _panelAccountManagementV11;

        protected override void OnEnable() {
            base.OnEnable();

            _draggableComponent = serializedObject.FindProperty("_draggableComponent");
            _accountIdentifierLabel = serializedObject.FindProperty("_accountIdentifierLabel");
            _accountIdentifierInput = serializedObject.FindProperty("_accountIdentifierInput");
            _passwordInput = serializedObject.FindProperty("_passwordInput");
            _tokenInput = serializedObject.FindProperty("_tokenInput");
            _addressLabel = serializedObject.FindProperty("_addressLabel");
            _addressInput = serializedObject.FindProperty("_addressInput");
            _clientVersionDropdown = serializedObject.FindProperty("_clientVersionDropdown");
            _buildVersionDropdown = serializedObject.FindProperty("_buildVersionDropdown");
            _panelAccountManagementV11 = serializedObject.FindProperty("_panelAccountManagementV11");
        }

        public override void OnInspectorGUI() {
            base.OnInspectorGUI();

            EditorGUILayout.LabelField("", GUI.skin.horizontalSlider);
            EditorGUILayout.LabelField("Login Widget", EditorStyles.boldLabel);
            EditorGUILayout.PropertyField(_draggableComponent, new GUIContent("Draggable"));
            EditorGUILayout.PropertyField(_accountIdentifierLabel, new GUIContent("Account Label"));
            EditorGUILayout.PropertyField(_accountIdentifierInput, new GUIContent("Account Input"));
            EditorGUILayout.PropertyField(_passwordInput, new GUIContent("Password Input"));
            EditorGUILayout.PropertyField(_tokenInput, new GUIContent("Token Input"));
            EditorGUILayout.PropertyField(_addressLabel, new GUIContent("Address Label"));
            EditorGUILayout.PropertyField(_addressInput, new GUIContent("Address Input"));
            EditorGUILayout.PropertyField(_clientVersionDropdown, new GUIContent("Client Version Dropdown"));
            EditorGUILayout.PropertyField(_buildVersionDropdown, new GUIContent("Build Version Dropdown"));
            EditorGUILayout.PropertyField(_panelAccountManagementV11, new GUIContent("V11 Account Panel"));

            serializedObject.ApplyModifiedProperties();
        }
    }
}
