using System;
using System.Collections.Generic;
using UnityEditor;
using UnityEditor.UIElements;
using UnityEngine;
using UnityEngine.UIElements;

namespace z3y.ShaderGraph
{
    public class ShaderGraphWindow : EditorWindow
    {
        [NonSerialized] public const string ROOT = "Packages/com.z3y.myshadergraph/Editor/";
        [NonSerialized] public ShaderGraphView graphView;
        //[NonSerialized] public bool nodesLoaded = false;
        //[NonSerialized] private static Dictionary<ShaderGraphImporter, ShaderGraphWindow> _instances = new();

        [SerializeField] private string _importerPath;

        public void Initialize(string importerPath)
        {
            AddStyleVariables();
            AddGraphView();
            AddToolbar();

            var data = ShaderGraphImporter.ReadGraphData(false, importerPath);
            var graph = graphView;

            data.Deserialize(graph);



            Show();
            Focus();

            // wtf
            EditorApplication.delayCall += () =>
            {
                graph.FrameAll();
            };

            _importerPath = importerPath;
        }

        public void OnEnable()
        {
            if (!string.IsNullOrEmpty(_importerPath))
            {
                Initialize(_importerPath);
            }
        }

        public void OnDisable()
        {

        }

        public void AddToolbar()
        {
            var toolbar = new Toolbar();

            var saveButton = new Button() { text = "Save" };
            saveButton.clicked += () => ShaderGraphImporter.SaveGraphAndReimport(graphView, _importerPath);
            toolbar.Add(saveButton);

            var shaderName = new TextField("Name") { value = "uhhh" };
            //TODO: bind
            //shaderName.Bind 
            //shaderNameTextField = shaderName;
            toolbar.Add(shaderName);

            var styles = AssetDatabase.LoadAssetAtPath<StyleSheet>(ROOT + "Styles/ToolbarStyles.uss");
            toolbar.styleSheets.Add(styles);
            rootVisualElement.Add(toolbar);
        }

        public void AddStyleVariables()
        {
            var styleVariables = AssetDatabase.LoadAssetAtPath<StyleSheet>(ROOT + "Styles/Variables.uss");
            rootVisualElement.styleSheets.Add(styleVariables);
        }

        public void AddGraphView()
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