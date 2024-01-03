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
        [NonSerialized] public static Dictionary<string, ShaderGraphWindow> editorInstances = new();

        [SerializeField] private string _importerGuid;


        public void Initialize(string importerGuid, bool focus = true)
        {
            titleContent = new GUIContent("sasf");

            AddStyleVariables();
            AddGraphView();
            AddToolbar();


            var data = ShaderGraphImporter.ReadGraphData(false, importerGuid);
            var graph = graphView;

            data.Deserialize(graph);

            if (focus)
            {
                Show();
                Focus();
            }

            EditorApplication.delayCall += () =>
            {
                graph.FrameAll();
            };

            editorInstances[importerGuid] = this;
            _importerGuid = importerGuid;
        }


        public void OnEnable()
        {
            if (!string.IsNullOrEmpty(_importerGuid) && graphView is null)
            {
                Initialize(_importerGuid, false);
            }
        }


        public void AddToolbar()
        {
            var toolbar = new Toolbar();

            var saveButton = new Button() { text = "Save" };
            saveButton.clicked += () => ShaderGraphImporter.SaveGraphAndReimport(graphView, _importerGuid);
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