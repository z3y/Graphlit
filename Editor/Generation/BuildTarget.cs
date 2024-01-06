using System;
using System.Collections.Generic;
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
        public void VisitTemplate(DescriptionVisitor visitor, int[] ports)
        {
            foreach (var output in Ports)
            {
                if (!Array.Exists(ports, x => x == output.ID))
                {
                    continue;
                }

                string inputString = GetInputString(output.ID);
                visitor.AppendLine($"{output.Type} {output.Name} = {inputString};");
            }
        }
    }

    public class UnlitBuildTarget : BuildTarget
    {
        public override string Name { get; } = "Unlit";

        public override Type VertexDescription => typeof(UnlitVertexDescription);
        public override Type SurfaceDescription => typeof(UnlitSurfaceDescription);

        public override void BuilderPassthourgh(ShaderBuilder builder)
        {
            builder.AddPass(new PassBuilder("FORWARD", "Somewhere/ForwardVertex.hlsl", "Somewhere/ForwardFragment.hlsl",
                UnlitVertexDescription.POSITION,
                UnlitVertexDescription.NORMAL,
                UnlitVertexDescription.TANGENT,
                UnlitSurfaceDescription.ALBEDO,
                UnlitSurfaceDescription.ALPHA,
                UnlitSurfaceDescription.ROUGHNESS,
                UnlitSurfaceDescription.METALLIC
                ));

            //builder.AddPass(new PassBuilder("FORWARDADD", "Somewhere/ForwardAddVertex.hlsl", "Somewhere/ForwardAddFragment.hlsl"));
            builder.AddPass(new PassBuilder("SHADOWCASTER", "Somewhere/ShadowcasterVertex.hlsl", "Somewhere/ShadowcasterFragment.hlsl",
                UnlitVertexDescription.POSITION,
                UnlitSurfaceDescription.ALPHA
                ));

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
            public const int ALBEDO = 3;
            public const int ALPHA = 4;
            public const int ROUGHNESS = 5;
            public const int METALLIC = 6;

            public override List<PortDescriptor> Ports { get; } = new List<PortDescriptor>
            {
                new(PortDirection.Input, new Float(3, false), ALBEDO, "Albedo"),
                new(PortDirection.Input, new Float(1, false), ALPHA, "Alpha"),
                new(PortDirection.Input, new Float(1, false), ROUGHNESS, "Roughness"),
                new(PortDirection.Input, new Float(1, false), METALLIC, "Metallic"),
            };
        }
    }
}