using System.Collections.Generic;
using UnityEditor.AssetImporters;
using UnityEngine;
using UnityEditor;
using UnityEngine.UIElements;
using UnityEditor.UIElements;
using z3y.ShaderGraph.Nodes;
using System.IO;
using UnityEditor.Experimental.GraphView;

namespace z3y.ShaderGraph
{
    [ScriptedImporter(1, EXTENSION, 0)]
    public class ShaderGraphImporter : ScriptedImporter
    {
        public const string EXTENSION = "zsg";

        [SerializeReference, HideInInspector] public List<Nodes.ShaderNode> shaderNodes;
        [SerializeField, HideInInspector] public string shaderName;

        public override void OnImportAsset(AssetImportContext ctx)
        {

            //var shader = ShaderUtil.CreateShaderAsset(ctx, "uhh", false);
            ctx.AddObjectToAsset("Main Asset", new TextAsset("asdf"));
        }

        [MenuItem("Assets/Create/z3y/Shader Graph")]
        public static void CreateVariantFile()
        {
            ProjectWindowUtil.CreateAssetWithContent($"New Shader Graph.{EXTENSION}", string.Empty);
        }

        public void OpenInGraphView()
        {
            var win = ShaderGraphWindow.InitializeEditor(this);

            if (win.nodesLoaded)
            {
                return;
            }

            foreach (var node in shaderNodes)
            {
                win.graphView.AddNode(node);
            }

            win.nodesLoaded = true;
        }

        public static void SaveGraphData(ShaderGraphView graphView, string importerPath)
        {
            var selection = Selection.objects;
            Selection.objects = null;
            {
                var importer = (ShaderGraphImporter)AssetImporter.GetAtPath(importerPath);
                importer.shaderNodes = new List<ShaderNode>();
                var elements = graphView.graphElements;
                foreach (var node in elements)
                {
                    if (node is ShaderNodeVisualElement shaderNodeVisualElement)
                    {
                        var shaderNode = shaderNodeVisualElement.shaderNode;
                        shaderNode.UpdateSerializedPosition();
                        importer.shaderNodes.Add(shaderNode);
                    }
                }
                //importer.shaderName = ShaderGraphWindow.shaderNameTextField.value;

                EditorUtility.SetDirty(importer);
                importer.SaveAndReimport();
            }
            Selection.objects = selection;
        }
    }

    [CustomEditor(typeof(ShaderGraphImporter))]
    internal class ShaderGraphImporterEditor : ScriptedImporterEditor
    {
        /*public override VisualElement CreateInspectorGUI()
        {
            var importer = (ShaderGraphImporter)serializedObject.targetObject;
            var assetPath = AssetDatabase.GetAssetPath(importer);

            var root = new VisualElement();
            root.Add(new ObjectField());

            var revertGui = new IMGUIContainer(RevertGUI);
            root.Add(revertGui);
            return root;
        }

        private void RevertGUI()
        {
            ApplyRevertGUI();
        }*/

        public override void OnInspectorGUI()
        {
            var importer = (ShaderGraphImporter)serializedObject.targetObject;

            if (GUILayout.Button("Edit Shader"))
            {
                importer.OpenInGraphView();
            }

            base.OnInspectorGUI();
        }

/*        protected override bool OnApplyRevertGUI()
        {
            return false;
            //return base.OnApplyRevertGUI();
        }*/

        protected override bool needsApplyRevert => false;
    }
}