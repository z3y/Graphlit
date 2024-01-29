using System.Collections.Generic;
using UnityEditor.AssetImporters;
using UnityEngine;
using UnityEditor;
using System.IO;
using System;
using UnityEditor.Callbacks;
using System.Linq;

namespace ZSG
{
    [ScriptedImporter(1, EXTENSION, 0)]
    public class ShaderGraphImporter : ScriptedImporter
    {
        public const string EXTENSION = "zsg";

        // private string _testShaderPath = "Assets/UnlitTest.shader";

        [NonSerialized] internal static Dictionary <string, SerializableGraph> _cachedGraphData = new();
        [NonSerialized] internal static Dictionary<string, ShaderGraphView> _graphViews = new();

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

/*        public static ShaderBuilder UpdateGraph(string guid, ShaderGraphView shaderGraphView)
        {
            //var serializableGraph = ReadGraphData(false, guid);
            if (shaderGraphView is null) _graphViews.TryGetValue(guid, out shaderGraphView);
            var builder = new ShaderBuilder(GenerationMode.Final, shaderGraphView);
            var target = new UnlitBuildTarget();
            target.BuilderPassthourgh(builder);
            builder.Build(target);

            return builder;
        }*/

        public override void OnImportAsset(AssetImportContext ctx)
        {
            var guid = AssetDatabase.AssetPathToGUID(assetPath);
/*
            if (_graphViews.TryGetValue(guid, out var graphView))
            {

            }
            else
            {*/
                var data = ReadGraphData(false, guid);
                var graphView = new ShaderGraphView(null);
                data.PopulateGraph(graphView);
            //}

            var builder = new ShaderBuilder(GenerationMode.Final, graphView);
            //var target = new UnlitBuildTarget();
            //target.BuilderPassthourgh(builder);
            builder.Build<UnlitTemplate>();

            //var text = File.ReadAllText(assetPath);
            //ctx.AddObjectToAsset("Main Asset", new TextAsset(text));

            var result = builder.ToString();
            var shader = ShaderUtil.CreateShaderAsset(ctx, result, false);

            if (builder._nonModifiableTextures.Count > 0)
            {
                EditorMaterialUtility.SetShaderNonModifiableDefaults(shader, builder._nonModifiableTextures.Keys.ToArray(), builder._nonModifiableTextures.Values.ToArray());
            }
            if (builder._defaultTextures.Count > 0)
            {
                EditorMaterialUtility.SetShaderNonModifiableDefaults(shader, builder._defaultTextures.Keys.ToArray(), builder._defaultTextures.Values.ToArray());
            }

            var material = new Material(shader)
            {
                name = "Material"
            };
            ctx.AddObjectToAsset("Main Asset", shader);
            ctx.AddObjectToAsset("Material", material);

            ctx.AddObjectToAsset("generation", new TextAsset(result));

            var text = File.ReadAllText(assetPath);
            ctx.AddObjectToAsset("json", new TextAsset(text));

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

            _graphViews[guid] = win.graphView;
            ShaderBuilder.GenerateAllPreviews(win.graphView);
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