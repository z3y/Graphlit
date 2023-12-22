using UnityEditor;
using UnityEditor.UIElements;
using UnityEngine;
using UnityEngine.UIElements;

namespace z3y.ShaderGraph
{
    public class ShaderGraphWindow : EditorWindow
    {
        public const string ROOT = "Packages/com.z3y.myshadergraph/Editor/";

        [MenuItem("z3y/Shader Graph Window")]
        public static void ShowExample()
        {
            ShaderGraphWindow win = GetWindow<ShaderGraphWindow>();
            win.titleContent = new GUIContent("Shader Graph");
        }
        private void OnEnable()
        {
            AddStyleVariables();
            AddGraphView();
            AddToolbar();
        }

        private void AddToolbar()
        {
            var toolbar = new Toolbar();

            var saveButton = new Button() { text = "Save" };
            toolbar.Add(saveButton);

            var shaderName = new TextField("Name") { value = "Default Shader" };
            toolbar.Add(shaderName);

            var styles = AssetDatabase.LoadAssetAtPath<StyleSheet>(ROOT + "Styles/ToolbarStyles.uss");
            toolbar.styleSheets.Add(styles);
            rootVisualElement.Add(toolbar);
        }

        private void AddStyleVariables()
        {
            var styleVariables = AssetDatabase.LoadAssetAtPath<StyleSheet>(ROOT + "Styles/Variables.uss");
            rootVisualElement.styleSheets.Add(styleVariables);
        }

        private void AddGraphView()
        {
            var graphView = new ShaderGraphView(this);
            graphView.StretchToParentSize();

            var styleSheet = AssetDatabase.LoadAssetAtPath<StyleSheet>(ROOT + "Styles/GraphViewStyles.uss");
            var nodeStyle = AssetDatabase.LoadAssetAtPath<StyleSheet>(ROOT + "Styles/NodeStyles.uss");

            graphView.styleSheets.Add(styleSheet);
            graphView.styleSheets.Add(nodeStyle);

            rootVisualElement.Add(graphView);
        }

    }
}