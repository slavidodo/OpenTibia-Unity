using UnityEditor;
using UnityEngine;

namespace OpenTibiaUnity.Core.Components
{
    [CustomEditor(typeof(Draggable), true)]
    [CanEditMultipleObjects]
    public class DraggableTransformEditor : Editor
    {
        SerializedProperty m_BindRectToParent;
        SerializedProperty m_DraggablePolicy;
        SerializedProperty m_DragBoxSize;

        protected virtual void OnEnable() {
            m_BindRectToParent = serializedObject.FindProperty("bindRectToParent");
            m_DraggablePolicy = serializedObject.FindProperty("draggingPolicy");
            m_DragBoxSize = serializedObject.FindProperty("draggingBoxSize");
        }

        public override void OnInspectorGUI() {
            serializedObject.Update();

            EditorGUILayout.Space();
            EditorGUILayout.PropertyField(m_BindRectToParent);
            EditorGUILayout.Separator();
            EditorGUILayout.PropertyField(m_DraggablePolicy);
            
            if (m_DraggablePolicy.enumValueIndex != 0) {
                EditorGUILayout.PropertyField(m_DragBoxSize);
            } 
            
            serializedObject.ApplyModifiedProperties();
        }
    }
}