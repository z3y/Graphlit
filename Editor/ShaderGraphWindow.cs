using System.Collections.Generic;
using UnityEditor;
using UnityEditor.UIElements;
using UnityEngine;
using UnityEngine.UIElements;

namespace z3y.ShaderGraph
{
    public class ShaderGraphWindow : EditorWindow
    {
        public const string ROOT = "Packages/com.z3y.myshadergraph/Editor/";

        public ShaderGraphView graphView;
        private ShaderGraphImporter _impoterInstance;
        public bool nodesLoaded = false;
        //public TextField shaderNameTextField;
        private static Dictionary<ShaderGraphImporter, ShaderGraphWindow> _instances = new();
        public static ShaderGraphWindow InitializeEditor(ShaderGraphImporter importer)
        {
            if (_instances.TryGetValue(importer, out var window) && window != null)
            {
                window.Show();
                return window;
            }

            ShaderGraphWindow win = CreateInstance<ShaderGraphWindow>();
            win.titleContent = new GUIContent(importer.shaderName);
            win._impoterInstance = importer;

            _instances[importer] = win;

            win.AddStyleVariables();
            win.AddGraphView();
            win.AddToolbar();

            win.Show();
            return win;
        }


        private void AddToolbar()
        {
            var toolbar = new Toolbar();

            var saveButton = new Button() { text = "Save" };
            saveButton.clicked += () => ShaderGraphImporter.SaveGraphData(graphView, _impoterInstance.assetPath);
            toolbar.Add(saveButton);

            var shaderName = new TextField("Name") { value = _impoterInstance.shaderName };
            //TODO: bind
            //shaderName.Bind 
            //shaderNameTextField = shaderName;
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
            this.graphView = graphView;
        }

    }
}