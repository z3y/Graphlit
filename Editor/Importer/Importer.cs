using System.Collections.Generic;
using UnityEditor.AssetImporters;
using UnityEngine;
using UnityEditor;
using System.IO;
using System;
using UnityEditor.Callbacks;
using System.Linq;
using z3y.ShaderGraph.Nodes;

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
            var serializableGraph = ReadGraphData(false, guid);

            var builder = new ShaderBuilder(GenerationMode.Final, serializableGraph);
            builder.AddPass(new PassBuilder("FORWARD", "Somewhere/Vertex.hlsl", "Somewhere/Fragment.hlsl"));

            builder.Build();



            //var text = File.ReadAllText(assetPath);
            //ctx.AddObjectToAsset("Main Asset", new TextAsset(text));
            ctx.AddObjectToAsset("Main Asset", new TextAsset(builder.ToString()));

            _cachedGraphData.Clear();
        }

        [MenuItem("Assets/Create/z3y/Shader Graph")]
        public static void CreateVariantFile()
        {
            ProjectWindowUtil.CreateAssetWithContent($"New Shader Graph.{EXTENSION}", string.Empty);
        }


        public static void OpenInGraphView(string guid)
        {
            if (ShaderGraphWindow.editorInstances.TryGetValue(guid, out var win))
            {
                if (!win.disabled)
                {
                    win.Focus();
                    return;
                }

                else
                {
                    ShaderGraphWindow.editorInstances.Remove(guid);
                    win.Close();
                }
            }
            win = EditorWindow.CreateWindow<ShaderGraphWindow>(typeof(ShaderGraphWindow), typeof(ShaderGraphWindow));
            win.Initialize(guid);

        }

        public static void SaveGraphAndReimport(ShaderGraphView graphView, string guid)
        {
            var importerPath = AssetDatabase.GUIDToAssetPath(guid);
            var data = SerializableGraph.StoreGraph(graphView);
            var jsonData = JsonUtility.ToJson(data, true);

            _cachedGraphData[importerPath] = data;

            File.WriteAllText(importerPath, jsonData);
            AssetDatabase.ImportAsset(importerPath, ImportAssetOptions.ForceUpdate);

            graphView.MarkDirtyRepaint();
        }

        [OnOpenAsset]
        public static bool OnOpenAsset(int instanceID, int line)
        {
            var unityObject = EditorUtility.InstanceIDToObject(instanceID);
            var path = AssetDatabase.GetAssetPath(unityObject);
            var importer = AssetImporter.GetAtPath(path);
            if (importer is not ShaderGraphImporter shaderGraphImporter)
            {
                return false;
            }

            var guid = AssetDatabase.GUIDFromAssetPath(shaderGraphImporter.assetPath);
            OpenInGraphView(guid.ToString());
            return true;
        }
    }
}