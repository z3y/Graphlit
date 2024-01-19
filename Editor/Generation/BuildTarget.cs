using System;
using System.Linq;
using ZSG.Nodes;
using ZSG.Nodes.PortType;
using static UnityEditor.ShaderData;

namespace ZSG
{
    public abstract class BuildTarget
    {
        public abstract string Name { get; }
        public abstract Type VertexDescription { get; }
        public abstract Type SurfaceDescription { get; }
        public abstract void BuilderPassthourgh(ShaderBuilder builder);
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

    public class UnlitBuildTarget : BuildTarget
    {
        public override string Name { get; } = "Unlit";

        public override Type VertexDescription => typeof(UnlitVertexDescription);
        public override Type SurfaceDescription => typeof(UnlitSurfaceDescription);

        public override void BuilderPassthourgh(ShaderBuilder builder)
        {
            var basePass = new PassBuilder("FORWARD", "Packages/com.z3y.myshadergraph/Editor/Targets/Unlit/UnlitVertex.hlsl", "Packages/com.z3y.myshadergraph/Editor/Targets/Unlit/UnlitFragment.hlsl",
                UnlitVertexDescription.POSITION,
                UnlitSurfaceDescription.COLOR
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

        [NodeInfo("Vertex Description")]
        public sealed class UnlitVertexDescription : TemplateOutput
        {
            public const int POSITION = 0;
            public const int NORMAL = 1;
            public const int TANGENT = 2;

            public override void AddElements()
            {
                AddPort(new(PortDirection.Input, new Float(3, false), POSITION, "Position"));
                AddPort(new(PortDirection.Input, new Float(3, false), NORMAL, "Normal"));
                AddPort(new(PortDirection.Input, new Float(4, false), TANGENT, "Tangent"));
            }
        }

        [NodeInfo("Surface Description")]
        public sealed class UnlitSurfaceDescription : TemplateOutput
        {
            public const int COLOR = 3;

            public override void AddElements()
            {
                AddPort(new(PortDirection.Input, new Float(4, false), COLOR, "Color"));
            }

          /*  public override string SetDefaultInputString(int portID)
            {
                return portID switch
                {
                    COLOR => "1",
                    _ => "0",
                };
            }*/
        }
    }
}