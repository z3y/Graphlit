using System;
using System.Collections.Generic;
using UnityEditor;
using UnityEditor.AssetImporters;
using UnityEditor.UIElements;
using UnityEngine;
using UnityEngine.UIElements;

namespace z3y.ShaderGraph
{

    public class ShaderGraphWindow : EditorWindow
    {
        [NonSerialized] public const string ROOT = "Packages/com.z3y.myshadergraph/Editor/";
        [NonSerialized] public ShaderGraphView graphView;
        [NonSerialized] public static Dictionary<string, ShaderGraphWindow> editorInstances = new();

        [SerializeField] private string _importerGuid;

       // private ShaderGraphImporter _importer;

        public void Initialize(string importerGuid, bool focus = true)
        {
            //_importer = (ShaderGraphImporter)AssetImporter.GetAtPath(AssetDatabase.AssetPathToGUID(importerGuid));


            AddStyleVariables();

            AddGraphView();
            var data = ShaderGraphImporter.ReadGraphData(false, importerGuid);
            data.PopulateGraph(graphView);

            AddToolbar();
            titleContent = new GUIContent(data.data.shaderName);


            if (focus)
            {
                Show();
                Focus();
            }

            EditorApplication.delayCall += () =>
            {
                graphView.FrameAll();
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

            var pingAsset = new Button() { text = "Ping Asset" };
            pingAsset.clicked += () =>
            {
                var assetPath = AssetDatabase.GUIDToAssetPath(_importerGuid);
                EditorGUIUtility.PingObject(AssetDatabase.LoadAssetAtPath(assetPath, typeof(UnityEngine.Object)));
            };
            toolbar.Add(pingAsset);

            var saveButton = new Button() { text = "Save" };
            saveButton.clicked += () => ShaderGraphImporter.SaveGraphAndReimport(graphView, _importerGuid);
            toolbar.Add(saveButton);

            var shaderName = new TextField("Name") { value = graphView.graphData.shaderName };
            shaderName.RegisterValueChangedCallback((evt) =>
            {
                graphView.graphData.shaderName = evt.newValue;
            });
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