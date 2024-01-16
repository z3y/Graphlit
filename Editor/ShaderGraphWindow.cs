using System;
using System.Collections.Generic;
using UnityEditor;
using UnityEditor.AssetImporters;
using UnityEditor.Experimental.GraphView;
using UnityEditor.UIElements;
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
            conainer.style.flexDirection = FlexDirection.Row;
            rootVisualElement.Add(conainer);
            AddGraphView(conainer);
            var data = ShaderGraphImporter.ReadGraphData(false, importerGuid);
            data.PopulateGraph(graphView);

            AddBar(conainer);
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
            var toolbar = new Toolbar();

            //var bar = new VisualElement();


            var pingAsset = new Button() { text = "Select Asset" };
            pingAsset.clicked += () =>
            {
                var assetPath = AssetDatabase.GUIDToAssetPath(importerGuid);
                var obj = AssetDatabase.LoadAssetAtPath(assetPath, typeof(UnityEngine.Object));
                //EditorGUIUtility.PingObject(obj);
                Selection.activeObject = obj;
            };
            toolbar.Add(pingAsset);

            var saveButton = new Button() { text = "Save" };
            saveButton.clicked += SaveChanges;
            toolbar.Add(saveButton);

            var shaderName = new TextField("Name") { value = graphView.graphData.shaderName };
            shaderName.RegisterValueChangedCallback((evt) =>
            {
                graphView.graphData.shaderName = evt.newValue;
                SetDirty();
            });
            toolbar.Add(shaderName);


            var styles = AssetDatabase.LoadAssetAtPath<StyleSheet>(ROOT + "Styles/ToolbarStyles.uss");
            toolbar.styleSheets.Add(styles);
            rootVisualElement.Add(toolbar);

            var additionalElements = new VisualElement();
            var style = additionalElements.style;
            style.width = 300;
            style.paddingTop = 45;
            style.paddingLeft = 5;

            style.flexGrow = StyleKeyword.Auto;

            additionalElements.pickingMode = PickingMode.Ignore;

            visualElement.Add(additionalElements);
            graphView.additionalNodeElements = additionalElements;
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