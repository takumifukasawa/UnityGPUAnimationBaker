using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;

namespace GPUAnimationBaker
{

    [CustomEditor(typeof(AnimatorBaker))]
    public class AnimatorBakerEditor : Editor
    {
        public override void OnInspectorGUI()
        {
            AnimatorBaker script = target as AnimatorBaker;

            base.OnInspectorGUI();

            if (GUILayout.Button("Generate (only runtime)"))
            {
                // Start();
                EditorCoroutine.Start(script.Generate());
            }
        }

        // void Start()
        // {
        //     EditorApplication.update += Update;
        // }

        // void Stop()
        // {
        //     EditorApplication.update -= Update;
        // }

        // void Update()
        // {
        // }
    }

}