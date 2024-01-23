using System;
using System.Collections.Generic;
using System.Linq;
using UnityEditor;
using UnityEngine;
using UnityEngine.UIElements;

namespace ZSG
{
    public class ShaderGraphWindow : EditorWindow
    {
        [NonSerialized] public const string ROOT = "Packages/com.z3y.myshadergraph/Editor/";
        [NonSerialized] public ShaderGraphView graphView;
        [NonSerialized] public static Dictionary<string, ShaderGraphWindow> editorInstances = new();

        [SerializeField] public string importerGuid;

        // private ShaderGraphImporter _importer;
        [NonSerialized] public bool disabled = false;

        public void Initialize(string importerGuid, bool focus = true)
        {
            //_importer = (ShaderGraphImporter)AssetImporter.GetAtPath(AssetDatabase.AssetPathToGUID(importerGuid));

            AddStyleVariables();

            var conainer = new VisualElement();
            conainer.StretchToParentSize();
            conainer.style.flexDirection = FlexDirection.RowReverse;
            rootVisualElement.Add(conainer);
            AddGraphView(conainer);
            var data = ShaderGraphImporter.ReadGraphData(false, importerGuid);
            data.PopulateGraph(graphView);

            AddBar(rootVisualElement);
            conainer.Add(GetNodePropertiesElement());
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
            this.importerGuid = importerGuid;
            hasUnsavedChanges = false;

            //rootVisualElement.Add(conainer);
        }

        public new void SetDirty()
        {
            hasUnsavedChanges = true;
        }

        public override void SaveChanges()
        {
            var previousSelection = Selection.activeObject;
            Selection.activeObject = null;
            ShaderGraphImporter.SaveGraphAndReimport(graphView, importerGuid);
            base.SaveChanges();

            Selection.activeObject = previousSelection;
        }

        public void OnEnable()
        {
            if (!string.IsNullOrEmpty(importerGuid) && graphView is null)
            {
                Initialize(importerGuid, false);
                ShaderBuilder.GenerateAllPreviews(graphView);
            }
        }

        private void OnDisable()
        {
            disabled = true;
        }


        public void AddBar(VisualElement visualElement)
        {
            var toolbar = new VisualElement();
            {
                var s = toolbar.style;
                s.height = 32;
                s.flexDirection = FlexDirection.Row;
                s.backgroundColor = Color.clear;
            }

            var saveButton = new Button() { text = "Save" };
            saveButton.clicked += SaveChanges;
            toolbar.Add(saveButton);

            var pingAsset = new Button() { text = "Select Asset" };
            pingAsset.clicked += () =>
            {
                var assetPath = AssetDatabase.GUIDToAssetPath(importerGuid);
                var obj = AssetDatabase.LoadAssetAtPath(assetPath, typeof(UnityEngine.Object));
                //EditorGUIUtility.PingObject(obj);
                Selection.activeObject = obj;
            };
            toolbar.Add(pingAsset);

            var selectMasterNode = new Button() { text = "Master Node" };
            selectMasterNode.clicked += () =>
            {
                graphView.ClearSelection();
                graphView.AddToSelection(graphView.graphElements.Where(x => x is BuildTarget).First());
            };
            toolbar.Add(selectMasterNode);

            visualElement.Add(toolbar);
        }

        private VisualElement GetNodePropertiesElement()
        {
            var properties = new VisualElement();
            var style = properties.style;
            style.width = 400;
            style.paddingTop = 6;
            //style.paddingLeft = 5;
            //style.paddingRight = 6;

            style.flexGrow = StyleKeyword.Auto;

            properties.pickingMode = PickingMode.Ignore;
            graphView.additionalNodeElements = properties;
            return properties;
        }

        public void AddStyleVariables()
        {
            var styleVariables = AssetDatabase.LoadAssetAtPath<StyleSheet>(ROOT + "Styles/Variables.uss");
            rootVisualElement.styleSheets.Add(styleVariables);
        }

        public void AddGraphView(VisualElement visualElement)
        {
            var graphView = new ShaderGraphView(this);
            graphView.StretchToParentSize();

            var styleSheet = AssetDatabase.LoadAssetAtPath<StyleSheet>(ROOT + "Styles/GraphViewStyles.uss");
            var nodeStyle = AssetDatabase.LoadAssetAtPath<StyleSheet>(ROOT + "Styles/NodeStyles.uss");

            graphView.styleSheets.Add(styleSheet);
            graphView.styleSheets.Add(nodeStyle);

            visualElement.Add(graphView);
            this.graphView = graphView;
        }

    }
}