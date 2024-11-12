using System;
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
        [SerializeField] public Subgraph subgraph;
        public override bool DisablePreview => true;
        public override Color Accent => new Color(0.2f, 0.4f, 0.8f);

        public override void Initialize()
        {
            inputContainer.Add(new VisualElement());
            outputContainer.Add(new VisualElement());

            if (subgraph == null)
            {
                return;
            }

            var outputs = subgraph.outputs;
            var inputs = subgraph.inputs;

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
                objectType = typeof(Subgraph)
            };
            file.RegisterValueChangedCallback(x =>
            {
                subgraph = (Subgraph)x.newValue;
            });

            root.Add(file);
        }

        protected override void Generate(NodeVisitor visitor)
        {
            if (subgraph == null)
            {
                return;
            }

            string uniqueID = UniqueVariableID;

            foreach (var item in portDescriptors.Values)
            {
                if (item.Direction == PortDirection.Output)
                {
                    continue;
                }

                int id = item.ID;
                string name = $"SubgraphInput_{id}_{uniqueID}";
                visitor.AppendLine($"{item.Type} {name} = {PortData[id].Name};");
            }

            visitor.AddFunction(subgraph.function);
            var assetPath = AssetDatabase.GetAssetPath(subgraph);
            visitor._shaderBuilder.dependencies.Add(assetPath);
        }
    }
}