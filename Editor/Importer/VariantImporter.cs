using System.IO;
using UnityEditor;
using UnityEditor.AssetImporters;
using UnityEngine;
using static Graphlit.GraphData;

namespace Graphlit
{
    [ScriptedImporter(1, new[] { "graphlitvariant" }, -1)]
    public class VariantImporter : ScriptedImporter
    {
        [SerializeField][Tooltip("Relative or Absolute Path to the Graphlit asset")] public string graphPath = "";
        [SerializeField] public OutlinePassMode outlinePass = OutlinePassMode.Disabled;
        [SerializeField] public bool depthFillPass = false;
        [SerializeField] public string nameSuffix = "Variant";

        internal void OverrideVariantData(GraphData data)
        {
            data.shaderName += " " + nameSuffix;
            data.outlinePass = outlinePass;
            data.depthFillPass = depthFillPass;

            Debug.Log(data.shaderName);
        }

        public override void OnImportAsset(AssetImportContext ctx)
        {
            var target = ctx.selectedBuildTarget;
            string assetPath = "Packages/com.z3y.graphlit/Shaders/Unlit.graphlit";

            if (!string.IsNullOrEmpty(graphPath))
            {
                bool isAbsolute = graphPath.StartsWith("Assets/") || graphPath.StartsWith("Packages/");
                if (isAbsolute)
                {
                    assetPath = graphPath;
                }
                else
                {
                    var dir = Path.GetDirectoryName(ctx.assetPath);
                    assetPath = Path.Combine(dir, graphPath) + ".graphlit";
                }
                ctx.DependsOnSourceAsset(assetPath);
            }
            string guid = AssetDatabase.AssetPathToGUID(assetPath);

            var data = GraphlitImporter.ReadGraphData(guid);
            OverrideVariantData(data.data);
            data.data.unlocked = false;
            var graphView = new ShaderGraphView(null, assetPath);
            data.PopulateGraph(graphView);

            graphView.UpdateCachedNodesForBuilder();

            GraphlitImporter.GenerateShaderVariant(ctx, target, graphView, 0);
        }

        [MenuItem("Assets/Create/Graphlit/Variant")]
        public static void CreateVariantFile()
        {
            ProjectWindowUtil.CreateAssetWithContent($"Graphlit Variant.graphlitvariant", "");
        }
    }
}