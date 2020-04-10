using UnityEditor.AnimatedValues;
using UnityEditor;
using UnityEngine;
using OpenTibiaUnity.UI.Legacy;

namespace OpenTibiaUnityEditor.UI.Legacy
{
    [CustomEditor(typeof(ScrollRect), true)]
    [CanEditMultipleObjects]
    public class ScrollRectEditor : Editor
    {
        SerializedProperty _content;
        SerializedProperty _horizontal;
        SerializedProperty _vertical;
        SerializedProperty _movementType;
        SerializedProperty _elasticity;
        SerializedProperty _inertia;
        SerializedProperty _decelerationRate;
        SerializedProperty _scrollSensitivity;
        SerializedProperty _horizontalScrollbar;
        SerializedProperty _verticalScrollbar;
        SerializedProperty _minHorizontalScrollbarHeight;
        SerializedProperty _minVerticalScrollbarWidth;
        SerializedProperty _onValueChanged;
        AnimBool _showElasticity;
        AnimBool _showDecelerationRate;

        protected virtual void OnEnable() {
            _content = serializedObject.FindProperty("m_Content");
            _horizontal = serializedObject.FindProperty("m_Horizontal");
            _vertical = serializedObject.FindProperty("m_Vertical");
            _movementType = serializedObject.FindProperty("m_MovementType");
            _elasticity = serializedObject.FindProperty("m_Elasticity");
            _inertia = serializedObject.FindProperty("m_Inertia");
            _decelerationRate = serializedObject.FindProperty("m_DecelerationRate");
            _scrollSensitivity = serializedObject.FindProperty("m_ScrollSensitivity");
            _horizontalScrollbar = serializedObject.FindProperty("m_HorizontalScrollbar");
            _verticalScrollbar = serializedObject.FindProperty("m_VerticalScrollbar");
            _minHorizontalScrollbarHeight = serializedObject.FindProperty("m_MinHorizontalScrollbarHeight");
            _minVerticalScrollbarWidth = serializedObject.FindProperty("m_MinVerticalScrollbarWidth");
            _onValueChanged = serializedObject.FindProperty("m_OnValueChanged");

            _showElasticity = new AnimBool(Repaint);
            _showDecelerationRate = new AnimBool(Repaint);
            SetAnimBools(true);
        }

        protected virtual void OnDisable() {
            _showElasticity.valueChanged.RemoveListener(Repaint);
            _showDecelerationRate.valueChanged.RemoveListener(Repaint);
        }

        void SetAnimBools(bool instant) {
            SetAnimBool(_showElasticity, !_movementType.hasMultipleDifferentValues && _movementType.enumValueIndex == (int)ScrollRect.MovementType.Elastic, instant);
            SetAnimBool(_showDecelerationRate, !_inertia.hasMultipleDifferentValues && _inertia.boolValue == true, instant);
        }

        void SetAnimBool(AnimBool a, bool value, bool instant) {
            if (instant)
                a.value = value;
            else
                a.target = value;
        }

        public override void OnInspectorGUI() {
            SetAnimBools(false);

            serializedObject.Update();

            EditorGUILayout.PropertyField(_content);

            EditorGUILayout.PropertyField(_horizontal);
            EditorGUILayout.PropertyField(_vertical);

            EditorGUILayout.PropertyField(_movementType);
            if (EditorGUILayout.BeginFadeGroup(_showElasticity.faded)) {
                EditorGUI.indentLevel++;
                EditorGUILayout.PropertyField(_elasticity);
                EditorGUI.indentLevel--;
            }
            EditorGUILayout.EndFadeGroup();

            EditorGUILayout.PropertyField(_inertia);
            if (EditorGUILayout.BeginFadeGroup(_showDecelerationRate.faded)) {
                EditorGUI.indentLevel++;
                EditorGUILayout.PropertyField(_decelerationRate);
                EditorGUI.indentLevel--;
            }
            EditorGUILayout.EndFadeGroup();

            EditorGUILayout.PropertyField(_scrollSensitivity);
            EditorGUILayout.Space();

            EditorGUILayout.PropertyField(_horizontalScrollbar);
            EditorGUILayout.PropertyField(_verticalScrollbar);

            EditorGUILayout.Space();

            if (_horizontal.boolValue) {
                EditorGUILayout.PropertyField(_minHorizontalScrollbarHeight, new GUIContent("Horizontal minimum height"));
            }

            if (_vertical.boolValue) {
                EditorGUILayout.PropertyField(_minVerticalScrollbarWidth, new GUIContent("Vertical minimum width"));
            }

            EditorGUILayout.Space();

            EditorGUILayout.PropertyField(_onValueChanged);

            serializedObject.ApplyModifiedProperties();
        }
    }
}