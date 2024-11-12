using System.Linq;
using UnityEditor;
using UnityEditor.AssetImporters;
using UnityEngine;

namespace Graphlit
{
    [ScriptedImporter(9, new[] { "subgraphlit" }, -1)]
    public class ShaderSubGraphImporter : ShaderGraphImporter
    {
        internal static void BuildSubgraph(AssetImportContext ctx)
        {
            var guid = AssetDatabase.AssetPathToGUID(ctx.assetPath);

            var serializableGraph = ReadGraphData(guid);
            serializableGraph.data.unlocked = false;
            var graphView = new ShaderGraphView(null, ctx.assetPath);
            serializableGraph.PopulateGraph(graphView);


            graphView.UpdateCachedNodesForBuilder();
            var subgraphOutput = graphView.cachedNodesForBuilder.OfType<SubgraphOutputNode>().First();

            var subgraphBuilder = new ShaderBuilder(GenerationMode.Final, graphView, BuildTarget.StandaloneWindows64, false);
            var subgraphPass = new PassBuilder("", "", "");
            subgraphBuilder.AddPass(subgraphPass);
            var fragmentVisitor = new NodeVisitor(subgraphBuilder, ShaderStage.Fragment, 0);
            subgraphBuilder.TraverseGraph(subgraphOutput, fragmentVisitor);
            subgraphOutput.BuilderVisit(fragmentVisitor);

            foreach (var data in subgraphOutput.PortData)
            {
                subgraphPass.surfaceDescription.Add($"{graphView.graphData.subgraphOutputs.First(x => x.id == data.Key).name} = {data.Value.Name};");
            }

            foreach (var dependency in subgraphBuilder.dependencies)
            {
                ctx.DependsOnSourceAsset(dependency);
            }

            var asset = ScriptableObject.CreateInstance<Subgraph>();
            asset.function = string.Join('\n', subgraphPass.surfaceDescription);
            asset.outputs = graphView.graphData.subgraphOutputs;
            asset.inputs = graphView.graphData.subgraphInputs;
            asset.funcionIncludes = subgraphPass.functions;

            ctx.AddObjectToAsset("Subgraph Asset", asset);
            return;
        }
    }
}