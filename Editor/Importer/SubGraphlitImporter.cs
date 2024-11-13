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
    }
}