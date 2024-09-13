using System;
using UnityEngine.UIElements;
using UnityEngine;
using Graphlit.Nodes;
using UnityEditor.UIElements;
using UnityEditor;
using System.Reflection;
using System.Linq;
using Graphlit.Nodes.PortType;


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

        Subgraph GetSubgraphGraph()
        {
            if (string.IsNullOrEmpty(subgraphRef))
            {
                return null;
            }
            var asset = Helpers.SerializableReferenceToObject<Subgraph>(subgraphRef);

            return asset;
        }

        public override void Initialize()
        {
            inputContainer.Add(new VisualElement());
            outputContainer.Add(new VisualElement());

            var subgraph = GetSubgraphGraph();

            if (subgraph == null)
            {
                return;
            }

            var outputs = subgraph.outputs;
            var inputs = subgraph.inputs;

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

        string MethodParams()
        {
            string param = "";

            PortDescriptor[] array = portDescriptors.Values.ToArray();
            int lastParam = array.Length - 1;
            for (int i = 0; i < array.Length; i++)
            {
                PortDescriptor port = array[i];

                var data = PortData[port.ID];
                param += data.Name;
                if (i != lastParam) param += ", ";
            }

            return param;
        }

        protected override void Generate(NodeVisitor visitor)
        {
            var subgraph = GetSubgraphGraph();

            if (subgraph == null)
            {
                return;
            }

            visitor.AddFunction(subgraph.function);

            string uniqueID = UniqueVariableID;

            foreach (var port in portDescriptors.Values)
            {
                if (port.Direction == PortDirection.Input)
                {
                    continue;
                }

                string outName = $"{port.Name}_{port.ID}_{uniqueID}";
                visitor.AppendLine($"{port.Type} {outName};");
                SetVariable(port.ID, outName);
            }

            visitor.AppendLine($"{subgraph.functionName}({MethodParams()}, data);");
        }
    }
}