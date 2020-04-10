using UnityEditor;
using UnityEngine;

namespace OpenTibiaUnityEditor.UI.Legacy
{
    [CustomEditor(typeof(OpenTibiaUnity.UI.Legacy.SplitStackWidget), true)]
    [CanEditMultipleObjects]
    public class SplitStackWidgetEditor : PopUpBaseEditor
    {
        SerializedProperty _slider;
        SerializedProperty _itemPanel;

        protected override void OnEnable() {
            base.OnEnable();

            _slider = serializedObject.FindProperty("_slider");
            _itemPanel = serializedObject.FindProperty("_itemPanel");
        }

        public override void OnInspectorGUI() {
            base.OnInspectorGUI();

            EditorGUILayout.LabelField("", GUI.skin.horizontalSlider);
            EditorGUILayout.LabelField("Split Stack Widget", EditorStyles.boldLabel);
            EditorGUILayout.PropertyField(_slider, new GUIContent("Amount Slider"));
            EditorGUILayout.PropertyField(_itemPanel, new GUIContent("Item Panel"));

            serializedObject.ApplyModifiedProperties();
        }
    }
}
