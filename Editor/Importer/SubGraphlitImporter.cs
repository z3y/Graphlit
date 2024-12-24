using System.Linq;
using UnityEditor;
using UnityEditor.AssetImporters;
using UnityEngine;

namespace Graphlit
{
    [ScriptedImporter(9, new[] { "subgraphlit" }, -1)]
    public class SubGraphlitImporter : GraphlitImporter
    {
        internal static void BuildSubgraph(AssetImportContext ctx)
        {
            ctx.AddObjectToAsset("Subgraph Asset", ScriptableObject.CreateInstance<SubgraphObject>());
        }

        [MenuItem("Assets/Create/Graphlit/Subgraph")]
        public static void CreateVariantFile()
        {
            const string samplePath = "Packages/com.z3y.graphlit/Shaders/Subgraph.subgraphlit";
            var graph = ReadGraphData(AssetDatabase.AssetPathToGUID(samplePath));

            var jsonData = EditorJsonUtility.ToJson(graph, true);
            ProjectWindowUtil.CreateAssetWithContent($"Subgraph.subgraphlit", jsonData);
        }
    }
}