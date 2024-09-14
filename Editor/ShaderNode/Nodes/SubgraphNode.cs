using System;
using UnityEngine.UIElements;
using UnityEngine;
using Graphlit.Nodes;
using UnityEditor.UIElements;
using UnityEditor;
using UnityEditor.Experimental.GraphView;
using System.Linq;

namespace Graphlit
{
    [NodeInfo("Input/Subgraph"), Serializable]
    public class SubgraphNode : ShaderNode
    {
        /*public static readonly string[] Tag = new[] { "GraphlitFunction", "ZSGFunction" };
        [MenuItem("Assets/Create/Graphlit/Shader Include", priority = -1)]
        public static void CreateVariantFile()
        {
            ProjectWindowUtil.CreateAssetWithContent("CustomFunction.hlsl", DefaultFunction);
            var include = new ShaderInclude();
            AssetDatabase.SetLabels(include, new[] { Tag[0] });
        }*/

        [SerializeField] public string subgraphRef;
        public void UseFile(ShaderInclude include)
        {
            subgraphRef = Helpers.AssetSerializableReference(include);
        }
        public override bool DisablePreview => true;

        public override Color Accent => new Color(0.2f, 0.4f, 0.8f);

        (ShaderGraphView graph, string path) GetSubgraphGraph()
        {
            if (string.IsNullOrEmpty(subgraphRef))
            {
                return (null, string.Empty);
            }
            var asset = Helpers.SerializableReferenceToObject<Subgraph>(subgraphRef);
            var assetPath = AssetDatabase.GetAssetPath(asset);
            if (string.IsNullOrEmpty(assetPath))
            {
                return (null, string.Empty);
            }
            string guid = AssetDatabase.AssetPathToGUID(assetPath);
            var data = ShaderGraphImporter.ReadGraphData(guid);
            var graphView = new ShaderGraphView(null)
            {
                uniqueID = GraphView.uniqueID++
            };
            data.PopulateGraph(graphView);

            return (graphView, assetPath);
        }


        public override void Initialize()
        {
            inputContainer.Add(new VisualElement());
            outputContainer.Add(new VisualElement());

            var subgraph = GetSubgraphGraph();

            if (subgraph.graph == null)
            {
                return;
            }

            var outputs = subgraph.graph.graphData.subgraphOutputs;
            var inputs = subgraph.graph.graphData.subgraphInputs;

            foreach ( var output in outputs )
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
            var file = new ObjectField("File")
            {
                objectType = typeof(Subgraph)
            };
            if (!string.IsNullOrEmpty(subgraphRef))
            {
                file.value = Helpers.SerializableReferenceToObject<Subgraph>(subgraphRef);
            }
            file.RegisterValueChangedCallback(x =>
            {
                subgraphRef = Helpers.AssetSerializableReference(x.newValue);
            });

            root.Add(file);
        }

        protected override void Generate(NodeVisitor visitor)
        {
            //var subgraphOutput = (ShaderNode)SubgraphView.graphElements.First(x => x is SubgraphOutputNode);

            var sub = GetSubgraphGraph();

            if (sub.graph is null)
            {
                return;
            }

            var subOut = sub.graph.graphElements.OfType<SubgraphOutputNode>().First();


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
                subOut.subgraphResults[id] = new GeneratedPortData(item.Type, name);
            }

            var subgraphResults = visitor._shaderBuilder.BuildSubgraph(sub.graph, visitor, sub.path);

            foreach (var item in Outputs)
            {
                int id = item.GetPortID();
                PortData[id] = subgraphResults[id];
            }

            var asset = Helpers.SerializableReferenceToObject<Subgraph>(subgraphRef);
            var assetPath = AssetDatabase.GetAssetPath(asset);
            visitor._shaderBuilder.dependencies.Add(assetPath);
        }

        /*public override IEnumerable<Port> Outputs
        {
            get
            {
                var subInputs = SubgraphView.graphElements.OfType<SubgraphOutputNode>().First().Inputs.ToArray();

               // Debug.Log(base.Outputs.Count());

                foreach (var baseOutput in base.Outputs)
                {
                    var retargetPort = subInputs.First(x => x.GetPortID() == baseOutput.GetPortID());
                    if (retargetPort.connected)
                    {
                        var retargetOutput = retargetPort.connections.First().output;
                        yield return retargetOutput;
                    }
                    else
                    {
                        yield return baseOutput;
                    }
                }
            }
        }*/
    }
}