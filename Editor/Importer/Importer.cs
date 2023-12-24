using System.Collections.Generic;
using UnityEditor.AssetImporters;
using UnityEngine;
using UnityEditor;
using z3y.ShaderGraph.Nodes;
using System.Runtime.Serialization.Formatters.Binary;
using System.IO;
using UnityEditor.Experimental.GraphView;
using UnityEngine.UIElements;

namespace z3y.ShaderGraph
{
    [ScriptedImporter(1, EXTENSION, 0)]
    public class ShaderGraphImporter : ScriptedImporter
    {
        public const string EXTENSION = "zsg";

        private SerializedGraphData ReadGraphData()
        {
            var text = File.ReadAllText(assetPath);
            var data = JsonUtility.FromJson<SerializedGraphData>(text);
            return data;
        }

        public override void OnImportAsset(AssetImportContext ctx)
        {
            var text = File.ReadAllText(assetPath);
            //var shader = ShaderUtil.CreateShaderAsset(ctx, "uhh", false);
            ctx.AddObjectToAsset("Main Asset", new TextAsset(text));
        }

        [MenuItem("Assets/Create/z3y/Shader Graph")]
        public static void CreateVariantFile()
        {
            ProjectWindowUtil.CreateAssetWithContent($"New Shader Graph.{EXTENSION}", string.Empty);
        }

        public void OpenInGraphView()
        {
            var win = ShaderGraphWindow.InitializeEditor(this);
            var data = ReadGraphData();
            var graph = win.graphView;

            if (win.nodesLoaded)
            {
                return;
            }

            // create nodes
            foreach (var node in data.shaderNodes)
            {
                win.graphView.AddNode(node);
            }

            // create connections
            foreach (var node in data.shaderNodes)
            {
                foreach (var connection in node.GetSerializedConnections())
                {
                    var graphNode = node.Node;
                    var id = connection.portID;
                    var connectedInputPorts = connection.ports;

                    foreach (var connectedInputPort in connectedInputPorts)
                    {


                        foreach (var ve in graphNode.outputContainer.Children())
                        {
                            if (ve is not Port port)
                            {
                                continue;
                            }

                            foreach (var ve2 in connectedInputPort.node.Node.inputContainer.Children())
                            {
                                if (ve2 is not Port inputPort)
                                {
                                    continue;
                                }

                                var inputID = (int)inputPort.userData;
                                if (inputID == connectedInputPort.portID)
                                {
                                    var newEdge = port.ConnectTo(inputPort);
                                    graph.AddElement(newEdge);
                                    break;
                                }
                            }
                        }
                    }
                }
            }

            win.nodesLoaded = true;
        }

        public static void SaveGraphData(ShaderGraphView graphView, string importerPath)
        {
            var data = new SerializedGraphData();
            data.shaderName = "uhhh";
            var shaderNodes = new List<ShaderNode>();
            data.shaderNodes = shaderNodes;

            var elements = graphView.graphElements;
            foreach (var node in elements)
            {
                if (node is ShaderNodeVisualElement shaderNodeVisualElement)
                {
                    var shaderNode = shaderNodeVisualElement.shaderNode;
                    shaderNodes.Add(shaderNode);
                }
            }
            var jsonData = JsonUtility.ToJson(data, true);
            File.WriteAllText(importerPath, jsonData);
            AssetDatabase.ImportAsset(importerPath, ImportAssetOptions.ForceUpdate);
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
            base.OnInspectorGUI();


            if (GUILayout.Button("Edit Shader"))
            {
                importer.OpenInGraphView();
            }
        }

/*        protected override bool OnApplyRevertGUI()
        {
            return false;
            //return base.OnApplyRevertGUI();
        }*/

        protected override bool needsApplyRevert => false;
    }
}