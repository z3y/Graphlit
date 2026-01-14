#if UNITY_EDITOR && NDMF_INCLUDED
using System.Collections;
using System.Collections.Generic;
using nadena.dev.ndmf;
using UnityEditor;
using UnityEngine;

namespace Graphlit.Optimizer
{
    [CustomEditor(typeof(GraphlitOptimizer))]
    public class GraphlitOptimizerEditor : Editor
    {
        public override void OnInspectorGUI()
        {
            DrawDefaultInspector();

            // GraphlitOptimizer optimizer = (GraphlitOptimizer)target;

            // if (GUILayout.Button("Preview Optimizations"))
            // {

            // }
        }
    }
}
#endif