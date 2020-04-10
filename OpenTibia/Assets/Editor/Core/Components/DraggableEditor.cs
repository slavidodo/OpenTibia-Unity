using OpenTibiaUnity.Core.Components;
using UnityEditor;

namespace OpenTibiaUnityEditor.Core.Components
{
    [CustomEditor(typeof(Draggable), true)]
    [CanEditMultipleObjects]
    public class DraggableTransformEditor : Editor
    {
        SerializedProperty _bindRectToParent;
        SerializedProperty _draggablePolicy;
        SerializedProperty _dragBoxSize;

        protected virtual void OnEnable() {
            _bindRectToParent = serializedObject.FindProperty("bindRectToParent");
            _draggablePolicy = serializedObject.FindProperty("draggingPolicy");
            _dragBoxSize = serializedObject.FindProperty("draggingBoxSize");
        }

        public override void OnInspectorGUI() {
            serializedObject.Update();

            EditorGUILayout.Space();
            EditorGUILayout.PropertyField(_bindRectToParent);
            EditorGUILayout.Separator();
            EditorGUILayout.PropertyField(_draggablePolicy);
            
            if (_draggablePolicy.enumValueIndex != 0) {
                EditorGUILayout.PropertyField(_dragBoxSize);
            } 
            
            serializedObject.ApplyModifiedProperties();
        }
    }
}