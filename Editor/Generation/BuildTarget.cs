using System;
using System.Collections.Generic;
using UnityEngine;
using z3y.ShaderGraph.Nodes;
using z3y.ShaderGraph.Nodes.PortType;

namespace z3y.ShaderGraph
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
        public void VisitTemplate(ExpressionVisitor visitor, int[] ports)
        {
            var structField = visitor.Stage == ShaderStage.Fragment ?
                visitor._shaderBuilder.passBuilders[visitor.Pass].surfaceDescriptionStruct
                : visitor._shaderBuilder.passBuilders[visitor.Pass].vertexDescriptionStruct;

            foreach (var output in Ports)
            {
                if (!Array.Exists(ports, x => x == output.ID))
                {
                    continue;
                }

                string inputString = GetInputString(output.ID);
                visitor.AppendLine($"output.{output.Name} = {inputString};");

                if (DefaultPortsTypes[output.ID] is Float @float)
                {
                    structField.Add($"{@float} {output.Name};");
                }
            }

            visitor.AppendLine($"return output;");
        }
    }

    public class UnlitBuildTarget : BuildTarget
    {
        public override string Name { get; } = "Unlit";

        public override Type VertexDescription => typeof(UnlitVertexDescription);
        public override Type SurfaceDescription => typeof(UnlitSurfaceDescription);

        public override void BuilderPassthourgh(ShaderBuilder builder)
        {
            builder.AddPass(new PassBuilder("FORWARD", "Packages/com.z3y.myshadergraph/Editor/Targets/Unlit/UnlitVertex.hlsl", "Packages/com.z3y.myshadergraph/Editor/Targets/Unlit/UnlitFragment.hlsl",
                UnlitVertexDescription.POSITION,
                UnlitSurfaceDescription.COLOR
                ));

            //builder.AddPass(new PassBuilder("FORWARDADD", "Somewhere/ForwardAddVertex.hlsl", "Somewhere/ForwardAddFragment.hlsl"));
          /*  builder.AddPass(new PassBuilder("SHADOWCASTER", "Somewhere/ShadowcasterVertex.hlsl", "Somewhere/ShadowcasterFragment.hlsl",
                UnlitVertexDescription.POSITION,
                UnlitSurfaceDescription.ALPHA
                ));*/

        }

        [NodeInfo("Vertex Description")]
        public sealed class UnlitVertexDescription : TemplateOutput
        {
            public const int POSITION = 0;
            public const int NORMAL = 1;
            public const int TANGENT = 2;

            public override List<PortDescriptor> Ports { get; } = new List<PortDescriptor>
            {
                new(PortDirection.Input, new Float(3, false), POSITION, "Position"),
                new(PortDirection.Input, new Float(3, false), NORMAL, "Normal"),
                new(PortDirection.Input, new Float(4, false), TANGENT, "Tangent"),
            };
        }

        [NodeInfo("Surface Description")]
        public sealed class UnlitSurfaceDescription : TemplateOutput
        {
            public const int COLOR = 3;

            public override List<PortDescriptor> Ports { get; } = new List<PortDescriptor>
            {
                new(PortDirection.Input, new Float(4, false), COLOR, "Color"),
            };

            public override string SetDefaultInputString(int portID)
            {
                return portID switch
                {
                    COLOR => "1",
                    _ => "0",
                };
            }
        }
    }
}