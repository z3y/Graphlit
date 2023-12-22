using UnityEditor;
using UnityEngine;
using UnityEngine.UIElements;

namespace z3y.ShaderGraph
{
    public class ShaderGraphWindow : EditorWindow
    {
        [MenuItem("z3y/Shader Graph Window")]
        public static void ShowExample()
        {
            ShaderGraphWindow win = GetWindow<ShaderGraphWindow>();
            win.titleContent = new GUIContent("Shader Graph");
        }
        private void OnEnable()
        {
            var graphView = new ShaderGraphView(this);
            graphView.StretchToParentSize();
            rootVisualElement.Add(graphView);
        }

    }
}