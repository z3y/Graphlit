using System.Collections.Generic;
using UnityEditor.AssetImporters;
using UnityEngine;
using UnityEditor;
using z3y.ShaderGraph.Nodes;
using System.IO;
using UnityEditor.Experimental.GraphView;
using System.Text;
using System;
using UnityEditor.MemoryProfiler;
using System.Security.Cryptography;

namespace z3y.ShaderGraph
{
    [ScriptedImporter(1, EXTENSION, 0)]
    public class ShaderGraphImporter : ScriptedImporter
    {
        public const string EXTENSION = "zsg";

        private string _testShaderPath = "Assets/UnlitTest.shader";

        [NonSerialized] internal static Dictionary <string, SerializedGraphData> _cachedGraphData = new();
        private SerializedGraphData ReadGraphData(bool useCache)
        {
            if (_cachedGraphData.TryGetValue(assetPath, out SerializedGraphData graphData) && useCache)
            {
                //Debug.Log("using cached data");
                return graphData;
            }
            var text = File.ReadAllText(assetPath);
            var data = new SerializedGraphData();
            if (!string.IsNullOrEmpty(text))
            {
                EditorJsonUtility.FromJsonOverwrite(text, data);
            }
            _cachedGraphData[assetPath] = data;
            return data;
        }

        private void VisitConenctedNode(StringBuilder sb, ShaderNode node)
        {
            var connections = node.GetSerializedConnections();
            foreach (var connection in connections)
            {
                var visitedPorts = connection.inNode.visitedPorts;
                if (visitedPorts.Contains(connection.inID))
                {
                    // copy
                    node.portNames[connection.outID] = connection.inNode.SetOutputVariable(connection.inID);
                    var portType = connection.inNode.portTypes[connection.inID];
                    node.portTypes[connection.outID] = portType;
                    node.UpdateDynamicFloatComponent(connection.outID, portType);

                    continue;
                }
                VisitConenctedNode(sb, connection.inNode);

                connection.inNode.Visit(sb, connection.inID);

                {
                    // copy
                    node.portNames[connection.outID] = connection.inNode.SetOutputVariable(connection.inID);
                    var portType = connection.inNode.portTypes[connection.inID];
                    node.portTypes[connection.outID] = portType;
                    node.UpdateDynamicFloatComponent(connection.outID, portType);
                }

                visitedPorts.Add(connection.inID);
            }
        }

        public override void OnImportAsset(AssetImportContext ctx)
        {
            //var shader = ShaderUtil.CreateShaderAsset(ctx, "uhh", false);
            var sb = new StringBuilder();
            var data = ReadGraphData(true);

            ShaderNode.ResetUniqueVariableIDs();

            if (data.shaderNodes == null)
            {
                return;
            }

            foreach (var node in data.shaderNodes)
            {
                node.portNames.Clear();
                node.visitedPorts.Clear();
                node.portTypes.Clear();
            }

            foreach (var node in data.shaderNodes)
            {
                if (node.GetType() == typeof(OutputNode))
                {
                    VisitConenctedNode(sb, node);
                    //sb.Append("col = " + node.varibleNames[0] + ";");
                    node.Visit(sb, 0);
                    break;
                }
            }

           /* var sh = File.ReadAllLines(_testShaderPath);
            for (int i = 0; i < sh.Length; i++)
            {
                if (sh[i].TrimStart().StartsWith("//result"))
                {
                    sh[i] = sb.ToString();
                    break;
                }
            }*/

            //var result = string.Join('\n', sh);
            //var shader = ShaderUtil.CreateShaderAsset(ctx, result, false);
            ctx.AddObjectToAsset("text", new TextAsset(sb.ToString()));
            //ctx.AddObjectToAsset("Main Asset", shader);
            _cachedGraphData.Clear();
        }

        [MenuItem("Assets/Create/z3y/Shader Graph")]
        public static void CreateVariantFile()
        {
            ProjectWindowUtil.CreateAssetWithContent($"New Shader Graph.{EXTENSION}", string.Empty);
        }

        public void OpenInGraphView()
        {
            var win = ShaderGraphWindow.InitializeEditor(this);
            var data = ReadGraphData(false);
            var graph = win.graphView;

            if (win.nodesLoaded)
            {
                return;
            }

            // remove null elements that prevent graph from loading
            /*for (int i = 0; i < data.shaderNodes.Length; i++)
            {
                ShaderNode node = data.shaderNodes[i];
                if (node is null)
                {
                    Debug.Log($"Node {i} is null.");
                    //data.shaderNodes.RemoveAt(i);
                    //i--;
                }
            }*/

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
                    var outID = connection.outID;
                    var inID = connection.inID;
                    var inNode = connection.inNode;

                    foreach (var ve in graphNode.inputContainer.Children())
                    {
                        if (ve is not Port port)
                        {
                            continue;
                        }

                        if (port.userData == null || ((int)port.userData) != outID)
                        {
                            continue;
                        }

                        foreach (var ve2 in inNode.Node.outputContainer.Children())
                        {
                            if (ve2 is not Port outPort)
                            {
                                continue;
                            }

                            if ((int)outPort.userData == inID)
                            {
                                var newEdge = outPort.ConnectTo(port);
                                graph.AddElement(newEdge);
                                break;
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

            var elements = graphView.graphElements;
            foreach (var node in elements)
            {
                if (node is ShaderNodeVisualElement shaderNodeVisualElement)
                {
                    var shaderNode = shaderNodeVisualElement.shaderNode;
                    shaderNodes.Add(shaderNode);
                }
            }

            data.shaderNodes = shaderNodes.ToArray();

            _cachedGraphData[importerPath] = data;

            var jsonData = EditorJsonUtility.ToJson(data, true);
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