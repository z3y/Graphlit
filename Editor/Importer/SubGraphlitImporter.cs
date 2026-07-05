using System.Linq;
using UnityEditor;
using UnityEditor.AssetImporters;
using UnityEngine;

namespace Graphlit
{
    [ScriptedImporter(9, new[] { "subgraphlit" }, -1)]
    public class SubGraphlitImporter : GraphlitImporter
    {
        private const string _newSubgraphName = "Subgraph.subgraphlit";

        internal static void BuildSubgraph(AssetImportContext ctx)
        {
            ctx.AddObjectToAsset("Subgraph Asset", ScriptableObject.CreateInstance<SubgraphObject>());
        }

        [MenuItem("Assets/Create/Graphlit/Experimental/Subgraph")]
        public static void CreateVariantFile()
        {
            const string samplePath = "Packages/com.z3y.graphlit/Shaders/Subgraph.subgraphlit";
            var graph = ReadGraphData(AssetDatabase.AssetPathToGUID(samplePath));

            var jsonData = EditorJsonUtility.ToJson(graph, true);
#if UNITY_6000_5_OR_NEWER
            ProjectWindowUtil.CreateAssetWithTextContent(_newSubgraphName, jsonData);
#else
            ProjectWindowUtil.CreateAssetWithContent(_newSubgraphName, jsonData);
#endif
        }
    }
}
