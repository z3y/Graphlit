using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using UnityEditor;
using UnityEditor.UIElements;
using UnityEngine;
using UnityEngine.UIElements;

namespace Graphlit
{
    public class ShaderGraphWindow : EditorWindow
    {
        [NonSerialized] public const string ROOT = "Packages/com.z3y.graphlit/Editor/";
        [NonSerialized] public ShaderGraphView graphView;
        [NonSerialized] public static Dictionary<string, ShaderGraphWindow> editorInstances = new();

        [SerializeField] public string importerGuid;

        // private ShaderGraphImporter _importer;
        [NonSerialized] public bool disabled = false;

        public void Initialize(string importerGuid, bool focus = true)
        {
            this.importerGuid = importerGuid;

            //_importer = (ShaderGraphImporter)AssetImporter.GetAtPath(AssetDatabase.AssetPathToGUID(importerGuid));

            AddStyleVariables();

            var container = new VisualElement();
            container.StretchToParentSize();
            container.style.flexDirection = FlexDirection.RowReverse;
            rootVisualElement.Add(container);
            AddGraphView(container);
            var data = ShaderGraphImporter.ReadGraphData(importerGuid);
            data.PopulateGraph(graphView);

            AddBar(rootVisualElement);
            container.Add(GetNodePropertiesElement());

            titleContent = new GUIContent(data.data.shaderName);
            if (string.IsNullOrEmpty(data.data.shaderName) || data.data.shaderName == "Default Shader")
            {
                titleContent = new GUIContent(data.data.shaderName = "Graphlit/" + Path.GetFileNameWithoutExtension(AssetDatabase.GUIDToAssetPath(importerGuid)));
            }

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
            /*var toolbar = new VisualElement()
            {
                style = {
                    flexDirection = FlexDirection.Column,
                    width = 120,
                    backgroundColor  = new Color(0.1f, 0.1f, 0.1f),
                    marginTop = 4,
                    marginLeft = 4
                }
            };
            toolbar.style.SetBorderRadius(8);
            toolbar.style.SetPadding(2);
            toolbar.style.paddingTop = 4;
            toolbar.style.paddingBottom = 4;*/

            var toolbar = new Toolbar()
            {
                style = {
                    justifyContent = Justify.SpaceBetween,
                    alignItems = Align.Center,
                    height = 22,
                }
            };

            var left = new VisualElement() { style = { flexDirection = FlexDirection.Row } };
            toolbar.Add(left);
            var right = new VisualElement() { style = { flexDirection = FlexDirection.Row } };
            toolbar.Add(right);


            var saveButton = new ToolbarButton() { text = "Save Asset", style = { marginRight = 4 } };
            saveButton.clicked += SaveChanges;
            left.Add(saveButton);


            var pingAsset = new ToolbarButton() { text = "Select Asset", style = { marginRight = 4 } };
            pingAsset.clicked += () =>
            {
                var assetPath = AssetDatabase.GUIDToAssetPath(importerGuid);
                var obj = AssetDatabase.LoadAssetAtPath(assetPath, typeof(UnityEngine.Object));
                EditorGUIUtility.PingObject(obj);
                Selection.activeObject = obj;
            };
            right.Add(pingAsset);

            var selectMasterNode = new ToolbarButton() { text = "Master Node", style = { marginRight = 4 } };
            selectMasterNode.clicked += () =>
            {
                //var masterNode = graphView.graphElements.Where(x => x is TemplateOutput || x is SubgraphOutputNode).First();
                var masterNode = graphView.graphElements.Where(x => x is TemplateOutput).First();

                bool contained = graphView.selection.Contains(masterNode);

                graphView.ClearSelection();
                graphView.AddToSelection(masterNode);

                if (contained)
                {
                    masterNode.Focus();
                }
            };
            right.Add(selectMasterNode);

            var unlocked = new Toggle("Live Preview")
            {
                value = graphView.graphData.unlocked,
                tooltip = "Temporarly convert constants to properties and update them live on the imported material",
            };
            var unlockedLabel = unlocked.Q<Label>();
            unlockedLabel.style.minWidth = 60;

            unlocked.RegisterValueChangedCallback(x =>
            {
                graphView.graphData.unlocked = x.newValue;
            });
            left.Add(unlocked);

            visualElement.Add(toolbar);
        }

        private VisualElement GetNodePropertiesElement()
        {
            var properties = new VisualElement();
            var style = properties.style;
            style.width = 400;
            style.paddingTop = 25;
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
            var graphView = new ShaderGraphView(this, AssetDatabase.GUIDToAssetPath(importerGuid));
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