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
            var serializableGraph = ShaderGraphImporter.ReadGraphData(importerGuid);
            serializableGraph.PopulateGraph(graphView);

            AddBar(rootVisualElement);
            container.Add(GetNodePropertiesElement());

            titleContent = new GUIContent(GetShaderDisplayName(serializableGraph.data));

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
            saveChangesMessage = "Graph has unsaved changes.";
            hasUnsavedChanges = true;
        }

        public string GetShaderDisplayName(GraphData data)
        {
            if (string.IsNullOrEmpty(data.shaderName) || data.shaderName == "Default Shader")
            {
                return data.shaderName = "Graphlit/" + Path.GetFileNameWithoutExtension(AssetDatabase.GUIDToAssetPath(importerGuid));
            }
            return data.shaderName;
        }

        public override void SaveChanges() => SaveChangesImpl(true);
        public void SaveChangesUser() => SaveChangesImpl(false);
        public void SaveChangesImpl(bool isEditor = false)
        {
            if (isEditor)
            {
                graphView.graphData.unlocked = false;
            }
            var previousSelection = Selection.activeObject;
            Selection.activeObject = null;
            ShaderGraphImporter.SaveGraphAndReimport(graphView, importerGuid);
            base.SaveChanges();

            Selection.activeObject = previousSelection;

            if (!isEditor && graphView.graphData.unlocked)
            {
                saveChangesMessage = "Disable live preview.";
                hasUnsavedChanges = true;
            }

            titleContent = new GUIContent(GetShaderDisplayName(graphView.graphData));
        }

        public void OnEnable()
        {
            if (!string.IsNullOrEmpty(importerGuid) && graphView is null)
            {
                Initialize(importerGuid, false);
                ShaderBuilder.GenerateAllPreviews(graphView);
                ShaderGraphImporter._graphViews[importerGuid] = graphView;
            }
        }

        private void OnDisable()
        {
            ShaderGraphImporter._graphViews[importerGuid] = null;
            disabled = true;

            var nodes = graphView.nodes.OfType<ShaderNode>();
            foreach (var node in nodes)
            {
                if (node.previewDrawer is not null)
                {
                    var shader = node.previewDrawer._previewShader;
                    if (shader != null)
                    {
                        DestroyImmediate(shader);
                    }
                }
            }
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
            saveButton.clicked += SaveChangesUser;
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
                var masterNode = graphView.graphElements.Where(x => x is TemplateOutput || x is SubgraphOutputNode).First();
                //var masterNode = graphView.graphElements.Where(x => x is TemplateOutput).First();

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