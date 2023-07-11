using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;

namespace GPUAnimationBaker
{
    [CustomEditor(typeof(GPUAnimationController))]
    [CanEditMultipleObjects]
    public class GPUAnimationControllerEditor : Editor
    {
        public override void OnInspectorGUI()
        {
            GPUAnimationController script = target as GPUAnimationController;
            base.OnInspectorGUI();
            serializedObject.Update();
            SerializedProperty selectedGPUAnimationFrameIndex = serializedObject.FindProperty("_currentGPUAnimationFrameIndex");
            int index = EditorGUILayout.Popup(
                "Initial Animation Name",
                selectedGPUAnimationFrameIndex.intValue,
                script.AnimationNames
            );
            selectedGPUAnimationFrameIndex.intValue = index;
            serializedObject.ApplyModifiedProperties();
        }
    }
}