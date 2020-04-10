using UnityEditor;
using UnityEngine;

using UnityUI = UnityEngine.UI;

namespace OpenTibiaUnityEditor.UI.Legacy
{
    [CanEditMultipleObjects]
    [CustomEditor(typeof(OpenTibiaUnity.UI.Legacy.Slider), true)]
    public class SliderEditor : UnityEditor.UI.SelectableEditor
    {
        private SerializedProperty m_Direction;
        private SerializedProperty m_FillRect;
        private SerializedProperty m_HandleRect;
        private SerializedProperty m_MinValue;
        private SerializedProperty m_MaxValue;
        private SerializedProperty m_WholeNumbers;
        private SerializedProperty m_Value;
        private SerializedProperty m_Label;
        private SerializedProperty m_OnValueChanged;

        protected override void OnEnable() {
            base.OnEnable();
            this.m_FillRect = this.serializedObject.FindProperty("m_FillRect");
            this.m_HandleRect = this.serializedObject.FindProperty("m_HandleRect");
            this.m_Direction = this.serializedObject.FindProperty("m_Direction");
            this.m_MinValue = this.serializedObject.FindProperty("m_MinValue");
            this.m_MaxValue = this.serializedObject.FindProperty("m_MaxValue");
            this.m_WholeNumbers = this.serializedObject.FindProperty("m_WholeNumbers");
            this.m_Value = this.serializedObject.FindProperty("m_Value");
            this.m_Label = this.serializedObject.FindProperty("label");
            this.m_OnValueChanged = this.serializedObject.FindProperty("m_OnValueChanged");
        }

        public override void OnInspectorGUI() {
            base.OnInspectorGUI();
            EditorGUILayout.Space();
            this.serializedObject.Update();
            EditorGUILayout.PropertyField(this.m_FillRect);
            EditorGUILayout.PropertyField(this.m_HandleRect);
            if (this.m_FillRect.objectReferenceValue != (Object)null || this.m_HandleRect.objectReferenceValue != (Object)null) {
                EditorGUI.BeginChangeCheck();
                EditorGUILayout.PropertyField(this.m_Direction);
                if (EditorGUI.EndChangeCheck()) {
                    UnityUI.Slider.Direction enumValueIndex = (UnityUI.Slider.Direction)this.m_Direction.enumValueIndex;
                    foreach (Object targetObject in this.serializedObject.targetObjects)
                        (targetObject as UnityUI.Slider).SetDirection(enumValueIndex, true);
                }
                EditorGUILayout.PropertyField(this.m_MinValue);
                EditorGUILayout.PropertyField(this.m_MaxValue);
                EditorGUILayout.PropertyField(this.m_WholeNumbers);
                EditorGUILayout.Slider(this.m_Value, this.m_MinValue.floatValue, this.m_MaxValue.floatValue);
                EditorGUILayout.PropertyField(this.m_Label);
                bool flag = false;
                foreach (Object targetObject in this.serializedObject.targetObjects) {
                    UnityUI.Slider slider = targetObject as UnityUI.Slider;
                    switch (slider.direction) {
                        case UnityUI.Slider.Direction.LeftToRight:
                        case UnityUI.Slider.Direction.RightToLeft:
                            flag = slider.navigation.mode != UnityUI.Navigation.Mode.Automatic && ((Object)slider.FindSelectableOnLeft() != (Object)null || (Object)slider.FindSelectableOnRight() != (Object)null);
                            break;
                        default:
                            flag = slider.navigation.mode != UnityUI.Navigation.Mode.Automatic && ((Object)slider.FindSelectableOnDown() != (Object)null || (Object)slider.FindSelectableOnUp() != (Object)null);
                            break;
                    }
                }
                if (flag)
                    EditorGUILayout.HelpBox("The selected slider direction conflicts with navigation. Not all navigation options may work.", MessageType.Warning);
                EditorGUILayout.Space();
                EditorGUILayout.PropertyField(this.m_OnValueChanged);
            } else
                EditorGUILayout.HelpBox("Specify a RectTransform for the slider fill or the slider handle or both. Each must have a parent RectTransform that it can slide within.", MessageType.Info);
            this.serializedObject.ApplyModifiedProperties();
        }
    }
}