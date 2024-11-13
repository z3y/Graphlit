using System;
using System.Linq;
using UnityEngine.UIElements;
using UnityEngine;
using Graphlit.Nodes;
using UnityEditor.UIElements;
using UnityEditor;
using UnityEditor.Experimental.GraphView;

namespace Graphlit
{
    [NodeInfo("Input/Subgraph"), Serializable]
    public class SubgraphNode : ShaderNode
    {
        [SerializeField] public SubgraphObject subgraph;
        public override bool DisablePreview => true;
        public override Color Accent => new Color(0.2f, 0.4f, 0.8f);

        ShaderGraphView OpenSubgraph()
        {
            string assetPath = AssetDatabase.GetAssetPath(subgraph);
            var guid = AssetDatabase.AssetPathToGUID(assetPath);
            var data = GraphlitImporter.ReadGraphData(guid);
            var graphView = new ShaderGraphView(null, assetPath);
            data.PopulateGraph(graphView);
            return graphView;
        }

        public override void Initialize()
        {
            inputContainer.Add(new VisualElement());
            outputContainer.Add(new VisualElement());

            if (!subgraph)
            {
                return;
            }

            var subgraphView = OpenSubgraph();
            //var node = subgraphView.graphElements.OfType<SubgraphOutputNode>().First();
            var outputs = subgraphView.graphData.subgraphOutputs;
            var inputs = subgraphView.graphData.subgraphInputs;

            //var outputs = node.outputs;
            //var inputs = node.inputs;

            foreach (var output in outputs)
            {
                output.AddPropertyDescriptor(this, PortDirection.Output);
            }

            foreach (var input in inputs)
            {
                input.AddPropertyDescriptor(this, PortDirection.Input);
            }

            ResetPorts();
        }

        public override void AdditionalElements(VisualElement root)
        {
            var file = new ObjectField("Subgraph")
            {
                objectType = typeof(SubgraphObject),
                value = subgraph
            };
            file.RegisterValueChangedCallback(x =>
            {
                subgraph = (SubgraphObject)x.newValue;
            });

            root.Add(file);
        }

        protected override void Generate(NodeVisitor visitor)
        {
            if (!subgraph)
            {
                return;
            }
            
            var subgraphView = OpenSubgraph();

            var subgraphOutput = subgraphView.graphElements.OfType<SubgraphOutputNode>().First();


            string uniqueID = UniqueVariableID;

            foreach (var item in portDescriptors.Values)
            {
                if (item.Direction == PortDirection.Output)
                {
                    continue;
                }

                int id = item.ID;
                string name = $"SubgraphInput_{id}_{uniqueID}";
                //Debug.Log(subgraphResults[id].Name);
                //PortData[id] = subgraphResults[id];
                visitor.AppendLine($"{item.Type} {name} = {PortData[id].Name};");
                subgraphOutput.subgraphResults[id] = new GeneratedPortData(item.Type, name);
            }

            subgraphView.uniqueID = GraphView.uniqueID;
            //var subgraphResults = visitor._shaderBuilder.BuildSubgraph(sub.graph, visitor, sub.path);
            var subgraphBuilder = new ShaderBuilder(GenerationMode.Final, subgraphView, BuildTarget.StandaloneWindows64, false);
            var subgraphPass = new PassBuilder("", "", "");
            subgraphBuilder.AddPass(subgraphPass);
            subgraphBuilder.Build(subgraphOutput);
            
            foreach (var line in subgraphPass.surfaceDescription)
            {
                visitor.AppendLine(line);
            }
            foreach (var item in Outputs)
            {
                int id = item.GetPortID();
                PortData[id] = subgraphOutput.subgraphResults[id];
            }

            var currentPass = visitor._shaderBuilder.passBuilders[visitor.Pass];
            currentPass.attributes.UnionWith(subgraphPass.attributes);
            currentPass.varyings.UnionWith(subgraphPass.varyings);
            GraphView.uniqueID = subgraphView.uniqueID;

            visitor._shaderBuilder.dependencies.UnionWith(subgraphBuilder.dependencies);
            visitor._shaderBuilder.dependencies.Add(AssetDatabase.GetAssetPath(subgraph));
        }
    }
}