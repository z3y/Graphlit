using System.Collections.Generic;
using UnityEditor.AssetImporters;
using UnityEngine;
using UnityEditor;
using System.IO;
using System;

namespace z3y.ShaderGraph
{
    [ScriptedImporter(1, EXTENSION, 0)]
    public class ShaderGraphImporter : ScriptedImporter
    {
        public const string EXTENSION = "zsg";

        // private string _testShaderPath = "Assets/UnlitTest.shader";

        [NonSerialized] internal static Dictionary <string, SerializableGraph> _cachedGraphData = new();
        public static SerializableGraph ReadGraphData(bool useCache, string guid)
        {
            var assetPath = AssetDatabase.GUIDToAssetPath(guid);
            if (_cachedGraphData.TryGetValue(assetPath, out SerializableGraph graphData) && useCache)
            {
                //Debug.Log("using cached data");
                return graphData;
            }
            var text = File.ReadAllText(assetPath);
            var data = new SerializableGraph();
            if (!string.IsNullOrEmpty(text))
            {
                JsonUtility.FromJsonOverwrite(text, data);
            }
            _cachedGraphData[assetPath] = data;
            return data;
       }
        /* 
                public static void VisitConenctedNode(NodeVisitor visitor, ShaderNode node)
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
                        VisitConenctedNode(visitor, inNode);

                        inNode.Visit(visitor);
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
        */
        public override void OnImportAsset(AssetImportContext ctx)
        {
            var guid = AssetDatabase.AssetPathToGUID(assetPath);

            var builder = new ShaderBuilder();
            builder.passBuilders.Add(new PassBuilder("FORWARD", "Somewhere/Vertex.hlsl", "Somewhere/Fragment.hlsl"));
            var visitor = new NodeVisitor(builder);
            //var data = ReadGraphData(true, guid);


            var text = File.ReadAllText(assetPath);
            ctx.AddObjectToAsset("Main Asset", new TextAsset(text));

            _cachedGraphData.Clear();
        }

        [MenuItem("Assets/Create/z3y/Shader Graph")]
        public static void CreateVariantFile()
        {
            ProjectWindowUtil.CreateAssetWithContent($"New Shader Graph.{EXTENSION}", string.Empty);
        }


        public static void OpenInGraphView(string guid, bool reopenWindow = true)
        {
            if (reopenWindow && ShaderGraphWindow.editorInstances.TryGetValue(guid, out var win))
            {
                win.Close();
            }
            win = EditorWindow.CreateWindow<ShaderGraphWindow>(typeof(ShaderGraphWindow), typeof(ShaderGraphWindow));
            win.Initialize(guid);

        }

        public static void SaveGraphAndReimport(ShaderGraphView graphView, string guid)
        {
            var importerPath = AssetDatabase.GUIDToAssetPath(guid);
            var data = SerializableGraph.FromGraphView(graphView);
            var jsonData = JsonUtility.ToJson(data, true);

            _cachedGraphData[importerPath] = data;

            File.WriteAllText(importerPath, jsonData);
            AssetDatabase.ImportAsset(importerPath, ImportAssetOptions.ForceUpdate);

            graphView.MarkDirtyRepaint();
        }
    }
}