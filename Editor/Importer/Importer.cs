using System.Collections.Generic;
using UnityEditor.AssetImporters;
using UnityEngine;
using UnityEditor;
using z3y.ShaderGraph.Nodes;
using System.IO;
using UnityEditor.Experimental.GraphView;
using System.Text;
using System;

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

        public static void VisitConenctedNode(StringBuilder sb, ShaderNode node)
        {
            var connections = node.GetSerializedConnections();
            foreach (var connection in connections)
            {
                var inNode = connection.inNode;
                if (inNode.visited)
                {
                    // copy
                    node.PortNames[connection.outID] = inNode.SetOutputString(connection.inID);
                    var portType = connection.inNode.PortsTypes[connection.inID];
                    node.PortsTypes[connection.outID] = portType;

                    continue;
                }
                VisitConenctedNode(sb, inNode);

                inNode.Visit(sb);
                {
                    // copy
                    node.PortNames[connection.outID] = connection.inNode.SetOutputString(connection.inID);
                    var portType = inNode.PortsTypes[connection.inID];
                    node.PortsTypes[connection.outID] = portType;
                }

                inNode.UpdateGraphView();
                inNode.visited = true;
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
                if (node.PortsTypes.Count == 0)
                {
                    node.Initialize();
                }
            }

            foreach (var node in data.shaderNodes)
            {
                if (node.GetType() == typeof(OutputNode))
                {
                    VisitConenctedNode(sb, node);
                    //sb.Append("col = " + node.varibleNames[0] + ";");
                    node.Visit(sb);
                    break;
                }
            }

            foreach (var node in data.shaderNodes)
            {
                node.ResetAfterVisit();
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

            DeserializeNodesToGraph(data, graph);

            win.nodesLoaded = true;
        }

        public static void DeserializeNodesToGraph(SerializedGraphData data, ShaderGraphView graphView, Vector2 mousePosition = new Vector2())
        {
            // offset for paste

            Vector2 minBounds = new Vector2();
            Vector2 maxBounds = new Vector2();
            bool offset = mousePosition != Vector2.zero;

            if (offset)
            {
                foreach (var node in data.shaderNodes)
                {
                    var position = node.GetSerializedPosition();

                    if (minBounds == Vector2.zero)
                    {
                        minBounds = position;
                        maxBounds = position;
                    }
                    minBounds = Vector2.Min(minBounds, position);
                    maxBounds = Vector2.Min(maxBounds, position);
                }
            }
            Vector2 boundCenter = (maxBounds - minBounds) / 2.0f;
            //var positionOffset = Vector2.Distance(mousePosition, boundCenter);
            //var positionDirection = ()

            // create nodes

            foreach (var node in data.shaderNodes)
            {
                if (offset)
                {
                    var currentPosition = node.GetSerializedPosition();
                    currentPosition -= boundCenter;
                    currentPosition += mousePosition;
                    node.SetPosition(currentPosition);
                }
                graphView.AddNode(node);
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

                        if (inNode.Node is null)
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
                                graphView.AddElement(newEdge);
                                break;
                            }
                        }
                    }
                }
            }
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

            graphView.MarkDirtyRepaint();
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