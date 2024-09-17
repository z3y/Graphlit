using System.Collections.Generic;
using UnityEditor.AssetImporters;
using UnityEngine;
using UnityEditor;
using System.IO;
using System;
using UnityEditor.Callbacks;
using System.Linq;
using UnityEngine.Profiling;

namespace Graphlit
{
    [ScriptedImporter(5, new[] { "graphlit", "subgraphlit", "zsg" }, 0)]
    public class ShaderGraphImporter : ScriptedImporter
    {
        internal static Dictionary<string, ShaderGraphView> _graphViews = new();
        internal static string _lastImport;

        private static Texture2D Thumbnail => AssetDatabase.LoadAssetAtPath<Texture2D>("Packages/com.z3y.graphlit/Editor/icon.psd");

        public static SerializableGraph ReadGraphData(string guid)
        {
            var assetPath = AssetDatabase.GUIDToAssetPath(guid);
            var text = File.ReadAllText(assetPath);
            var data = new SerializableGraph();
            if (!string.IsNullOrEmpty(text))
            {
                JsonUtility.FromJsonOverwrite(text, data);
            }
            return data;
        }


        public override void OnImportAsset(AssetImportContext ctx)
        {
            Profiler.BeginSample("Graphlit Shader Import", this);
            bool isSubgraph = assetPath.EndsWith("subgraphlit");
            if (isSubgraph)
            {
                var asset = ScriptableObject.CreateInstance<Subgraph>();
                ctx.AddObjectToAsset("Subgraph Asset", asset);
                return;
            }

            var target = ctx.selectedBuildTarget;
            string guid = AssetDatabase.AssetPathToGUID(assetPath);

            if (_graphViews.TryGetValue(guid, out var graphView))
            {
            }
            else if (graphView is null)
            {
                var data = ReadGraphData(guid);
                graphView = new ShaderGraphView(null, assetPath);
                data.PopulateGraph(graphView);
            }

            var filename = Path.GetFileNameWithoutExtension(ctx.assetPath);
            bool unlocked = graphView.graphData.unlocked;
            var builder = new ShaderBuilder(unlocked ? GenerationMode.Preview : GenerationMode.Final, graphView, target, unlocked);
            if (string.IsNullOrEmpty(builder.shaderName) || builder.shaderName == "Default Shader")
            {
                builder.shaderName = "Graphlit/" + filename;
            }
            builder.BuildTemplate();

            var scriptingDefines = PlayerSettings.GetScriptingDefineSymbols(UnityEditor.Build.NamedBuildTarget.Standalone);
            foreach (var pass in builder.passBuilders)
            {
                pass.pragmas.Add("#define TARGET_" + target.ToString().ToUpper());

                if (!string.IsNullOrEmpty(scriptingDefines))
                {
                    foreach (var define in scriptingDefines.Split(';'))
                    {
                        pass.pragmas.Add("#define SCRIPTING_DEFINE_" + define);
                    }
                }

            }


            var result = builder.ToString();
            _lastImport = result;
            var shader = ShaderUtil.CreateShaderAsset(ctx, result, false);

            if (builder._nonModifiableTextures.Count > 0)
            {
                EditorMaterialUtility.SetShaderNonModifiableDefaults(shader, builder._nonModifiableTextures.Keys.ToArray(), builder._nonModifiableTextures.Values.ToArray());
            }
            if (builder._defaultTextures.Count > 0)
            {
                EditorMaterialUtility.SetShaderNonModifiableDefaults(shader, builder._defaultTextures.Keys.ToArray(), builder._defaultTextures.Values.ToArray());
            }

            foreach (var dependency in builder.dependencies)
            {
                ctx.DependsOnSourceAsset(dependency);
            }

            ctx.AddObjectToAsset("Main Asset", shader, Thumbnail);

            string prefix = unlocked ? "Unlocked " : "";
            var material = new Material(shader)
            {
                name = $"{prefix}{builder.shaderName.Replace("/", "_")}"
            };
            DefaultInspector.SetupRenderingMode(material);
            ctx.AddObjectToAsset("Material", material);

            //ctx.AddObjectToAsset("generation", new TextAsset(result));

            //var text = File.ReadAllText(assetPath);
            //ctx.AddObjectToAsset("json", new TextAsset(text));
            DefaultInspector.Reinitialize();

            Profiler.EndSample();
        }

        public static void CreateEmptyTemplate(TemplateOutput template, Action<ShaderGraphView> onCreate = null)
        {

            if (template is LitTemplate)
            {
                const string samplePath = "Packages/com.z3y.graphlit/Shaders/Lit.graphlit";
                var graph = ReadGraphData(AssetDatabase.AssetPathToGUID(samplePath));
                graph.data.shaderName = "Default Shader";

                var jsonData = JsonUtility.ToJson(graph, true);
                ProjectWindowUtil.CreateAssetWithContent($"New Shader Graph.graphlit", jsonData);
            }
            else
            {
                const string samplePath = "Packages/com.z3y.graphlit/Shaders/Unlit.graphlit";
                var graph = ReadGraphData(AssetDatabase.AssetPathToGUID(samplePath));
                graph.data.shaderName = "Default Shader";

                var jsonData = JsonUtility.ToJson(graph, true);
                ProjectWindowUtil.CreateAssetWithContent($"New Shader Graph.graphlit", jsonData);
            }

        }
        public static void CreateEmptyTemplate<T>() where T : TemplateOutput, new()
        {
            var instance = new T();
            CreateEmptyTemplate(instance);
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
            try
            {
                var importerPath = AssetDatabase.GUIDToAssetPath(guid);
                var data = SerializableGraph.StoreGraph(graphView);
                var jsonData = JsonUtility.ToJson(data, true);

                _graphViews[importerPath] = graphView;

                File.WriteAllText(importerPath, jsonData);
                AssetDatabase.ImportAsset(importerPath, ImportAssetOptions.ForceUpdate);

                graphView.MarkDirtyRepaint();
            }
            catch (Exception ex)
            {
                Debug.LogError(ex);
            }
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