#define USE_CACHE

using System.Collections.Generic;
using UnityEditor.AssetImporters;
using UnityEngine;
using UnityEditor;
using System.IO;
using System;
using UnityEditor.Callbacks;
using System.Linq;

namespace Graphlit
{
    [ScriptedImporter(31, new[] { "graphlit", "zsg" }, 0)]
    public class GraphlitImporter : ScriptedImporter
    {
        internal static Dictionary<string, ShaderGraphView> _graphViews = new();
        internal static string _lastImport;

        public static Texture2D Thumbnail => AssetDatabase.LoadAssetAtPath<Texture2D>("Packages/com.z3y.graphlit/Editor/icon.psd");

        public static SerializableGraph ReadGraphData(string guid)
        {
            var assetPath = AssetDatabase.GUIDToAssetPath(guid);
            var text = File.ReadAllText(assetPath);
            var data = new SerializableGraph();
            if (!string.IsNullOrEmpty(text))
            {
                EditorJsonUtility.FromJsonOverwrite(text, data);
            }
            return data;
        }

        public override void OnImportAsset(AssetImportContext ctx)
        {

            if (this is SubGraphlitImporter)
            {
                SubGraphlitImporter.BuildSubgraph(ctx);
                return;
            }

            var target = ctx.selectedBuildTarget;
            string guid = AssetDatabase.AssetPathToGUID(assetPath);


            ShaderGraphView graphView = null;
#if USE_CACHE
            _graphViews.TryGetValue(guid, out graphView);
#endif

            if (graphView is null)
            {
                var data = ReadGraphData(guid);
                data.data.unlocked = false;
                graphView = new ShaderGraphView(null, assetPath);
                data.PopulateGraph(graphView);
            }

            graphView.UpdateCachedNodesForBuilder();



            if (graphView.graphData.generateVariants)
            {

                string name = graphView.graphData.shaderName;
                var outlineMode = graphView.graphData.outlinePass;

                graphView.graphData.outlinePass = GraphData.OutlinePassMode.Disabled;

                GenerateShaderVariant(ctx, target, graphView, 0);

                if (outlineMode != GraphData.OutlinePassMode.Disabled)
                {
                    graphView.graphData.outlinePass = outlineMode;
                    graphView.graphData.shaderName += " Outline";
                    GenerateShaderVariant(ctx, target, graphView, 1);
                }

                graphView.graphData.shaderName = name;
            }
            else
            {
                GenerateShaderVariant(ctx, target, graphView, 0);
            }

        }

        private static void GenerateShaderVariant(AssetImportContext ctx, BuildTarget target, ShaderGraphView graphView, int id)
        {
            var shaderNodes = graphView.cachedNodesForBuilder;
            var dependencies = new HashSet<string>();


            var template = shaderNodes.OfType<TemplateOutput>().First();

            var filename = Path.GetFileNameWithoutExtension(ctx.assetPath);
            bool unlocked = graphView.graphData.unlocked;


            var builder = new ShaderBuilder(unlocked ? GenerationMode.Preview : GenerationMode.Final, graphView, target, unlocked);
            if (string.IsNullOrEmpty(builder.shaderName) || builder.shaderName == "Default Shader")
            {
                builder.shaderName = "Graphlit/" + filename;
            }

            builder.BuildTemplate(template);

            var scriptingDefines = PlayerSettings.GetScriptingDefineSymbols(UnityEditor.Build.NamedBuildTarget.Standalone);
            foreach (var pass in builder.passBuilders)
            {
                pass.pragmas.Insert(0, "#define TARGET_" + target.ToString().ToUpper());

                if (!string.IsNullOrEmpty(scriptingDefines))
                {
                    foreach (var define in scriptingDefines.Split(';'))
                    {
                        pass.pragmas.Add("#define SCRIPTING_DEFINE_" + define);
                    }
                }

            }

            template.OnImportAsset(ctx, builder, id);

            dependencies.UnionWith(builder.dependencies);

            foreach (var dependency in dependencies)
            {
                ctx.DependsOnSourceAsset(dependency);
            }


            ShaderInspector.Reinitialize();
        }

        public static void CreateEmptyTemplate(TemplateOutput template, Action<ShaderGraphView> onCreate = null)
        {
            string samplePath;
            if (template is LitTemplate)
            {
                samplePath = "Packages/com.z3y.graphlit/Shaders/Lit.graphlit";
            }
            else
            {
                samplePath = "Packages/com.z3y.graphlit/Shaders/Unlit.graphlit";
            }
            var graph = ReadGraphData(AssetDatabase.AssetPathToGUID(samplePath));
            graph.data.shaderName = "Default Shader";
            var jsonData = EditorJsonUtility.ToJson(graph, true);
            ProjectWindowUtil.CreateAssetWithContent($"New Graphlit Shader.graphlit", jsonData);
        }
        public static void CreateEmptyTemplate<T>() where T : TemplateOutput, new()
        {
            var instance = new T();
            CreateEmptyTemplate(instance);
        }

        public static void CreateFromSample(Shader shader) => CreateFromSample(AssetDatabase.GetAssetPath(shader));
        public static void CreateFromSample(string samplePath)
        {
            var graph = GraphlitImporter.ReadGraphData(AssetDatabase.AssetPathToGUID(samplePath));
            graph.data.shaderName = "Default Shader";

            var jsonData = EditorJsonUtility.ToJson(graph, true);
            ProjectWindowUtil.CreateAssetWithContent($"New Shader Graph.graphlit", jsonData);
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

            win.minSize = new Vector2(1000, 600);
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
                var jsonData = EditorJsonUtility.ToJson(data, true);

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
            if (importer is not GraphlitImporter shaderGraphImporter)
            {
                return false;
            }

            var guid = AssetDatabase.GUIDFromAssetPath(shaderGraphImporter.assetPath);
            OpenInGraphView(guid.ToString());
            return true;
        }
    }

}