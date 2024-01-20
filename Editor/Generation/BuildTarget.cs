using System;
using System.Linq;
using UnityEngine;
using UnityEngine.UIElements;
using ZSG.Nodes;
using ZSG.Nodes.PortType;

namespace ZSG
{
    public abstract class BuildTarget : TemplateOutput
    {
        public abstract string Name { get; }
        public abstract void BuilderPassthourgh(ShaderBuilder builder);
        public abstract int[] VertexPorts { get; }
        public abstract int[] FragmentPorts { get; }
    }

    public abstract class TemplateOutput : ShaderNode
    {
        public void VisitTemplate(NodeVisitor visitor, int[] ports)
        {
           var structField = visitor.Stage == ShaderStage.Fragment ?
                visitor._shaderBuilder.passBuilders[visitor.Pass].surfaceDescriptionStruct
                : visitor._shaderBuilder.passBuilders[visitor.Pass].vertexDescriptionStruct;

            foreach (var input in Inputs)
            {
                int currentID = input.GetPortID();

                if (!ports.Contains(currentID))
                {
                    continue;
                }

                var portDesc = portDescriptors[currentID];

                if (portDesc.Type is Float @float)
                {
                    var inputData = PortData[currentID];
                    visitor.AppendLine($"output.{portDesc.Name} = {inputData.Name};");
                    structField.Add($"{@float} {portDesc.Name};");
                }
            }
        }

        public override bool EnablePreview => false;

        protected sealed override void Generate(NodeVisitor visitor) { }
    }

    [NodeInfo("Unlit")]
    public class UnlitBuildTarget : BuildTarget
    {
        public override string Name { get; } = "Unlit";
        public override int[] VertexPorts => new int[] { POSITION , NORMAL, TANGENT };
        public override int[] FragmentPorts => new int[] { COLOR };

        public const int POSITION = 0;
        public const int NORMAL = 1;
        public const int TANGENT = 2;
        public const int COLOR = 3;
        public override void AddElements()
        {
            AddPort(new(PortDirection.Input, new Float(3, false), POSITION, "Position"));
            AddPort(new(PortDirection.Input, new Float(3, false), NORMAL, "Normal"));
            AddPort(new(PortDirection.Input, new Float(4, false), TANGENT, "Tangent"));

            var separator = new VisualElement();
            separator.style.height = 2;
            separator.style.backgroundColor = Color.gray;
            inputContainer.Add(separator);
            AddPort(new(PortDirection.Input, new Float(4, false), COLOR, "Color"));
        }

        public override void BuilderPassthourgh(ShaderBuilder builder)
        {
            var basePass = new PassBuilder("FORWARD", "Packages/com.z3y.myshadergraph/Editor/Targets/Unlit/UnlitVertex.hlsl", "Packages/com.z3y.myshadergraph/Editor/Targets/Unlit/UnlitFragment.hlsl",
                POSITION,
                COLOR
                );

            basePass.attributes.RequirePositionOS();
            basePass.attributes.Require("UNITY_VERTEX_INPUT_INSTANCE_ID");

            basePass.varyings.RequirePositionCS();
            basePass.varyings.RequireCustomString("UNITY_VERTEX_INPUT_INSTANCE_ID");
            basePass.varyings.RequireCustomString("UNITY_VERTEX_OUTPUT_STEREO");

            basePass.pragmas.Add("#include \"UnityCG.cginc\"");


            //basePass.vertexDescription.Add("")

            builder.AddPass(basePass);
        }
    }
}