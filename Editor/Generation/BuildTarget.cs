using System;
using System.Collections.Generic;
using UnityEngine.UIElements;
using z3y.ShaderGraph.Nodes;
using z3y.ShaderGraph.Nodes.PortType;
using static UnityEditor.ObjectChangeEventStream;

namespace z3y.ShaderGraph
{
    public abstract class BuildTarget
    {
        public abstract string Name { get; }
        public abstract Type VertexDescription { get; }
        public abstract Type SurfaceDescription { get; }

        public abstract void RegisterBuilder(ShaderBuilder builder);
    }

    public abstract class TemplateOutput : ShaderNode
    {
    }

    public class UnlitBuildTarget : BuildTarget
    {
        public override string Name { get; } = "Unlit";

        public override Type VertexDescription => typeof(UnlitVertexDescription);
        public override Type SurfaceDescription => typeof(UnlitSurfaceDescription);

        public override void RegisterBuilder(ShaderBuilder builder)
        {
            builder.AddPass(new PassBuilder("FORWARD", "Somewhere/ForwardVertex.hlsl", "Somewhere/ForwardFragment.hlsl"));
            builder.AddPass(new PassBuilder("FORWARDADD", "Somewhere/ForwardAddVertex.hlsl", "Somewhere/ForwardAddFragment.hlsl"));
        }

        [NodeInfo("Vertex Description")]
        public sealed class UnlitVertexDescription : TemplateOutput, IRequireDescriptionVisitor
        {
            const int POSITION = 0;
            const int NORMAL = 1;
            const int TANGENT = 2;

            public override List<PortDescriptor> Ports { get; } = new List<PortDescriptor>
            {
                new(PortDirection.Input, new Float(3, false), POSITION, "Position"),
                new(PortDirection.Input, new Float(3, false), NORMAL, "Normal"),
                new(PortDirection.Input, new Float(4, false), TANGENT, "Tangent"),
            };

            public void VisitDescription(DescriptionVisitor visitor)
            {
                visitor.AppendLine($"float3 position = {GetInputString(POSITION)};");
                visitor.AppendLine($"float3 normal = {GetInputString(NORMAL)};");
                visitor.AppendLine($"float4 tangent = {GetInputString(TANGENT)};");
            }
        }

        [NodeInfo("Surface Description")]
        public sealed class UnlitSurfaceDescription : TemplateOutput, IRequireDescriptionVisitor
        {
            const int ALBEDO = 0;
            const int ALPHA = 1;
            const int ROUGHNESS = 2;
            const int METALLIC = 3;

            public override List<PortDescriptor> Ports { get; } = new List<PortDescriptor>
            {
                new(PortDirection.Input, new Float(3, false), ALBEDO, "Albedo"),
                new(PortDirection.Input, new Float(1, false), ALPHA, "Alpha"),
                new(PortDirection.Input, new Float(1, false), ROUGHNESS, "Roughness"),
                new(PortDirection.Input, new Float(1, false), METALLIC, "Metallic"),
            };

            public void VisitDescription(DescriptionVisitor visitor)
            {
                visitor.AppendLine($"float3 albedo = {GetInputString(ALBEDO)};");
                visitor.AppendLine($"float alpha = {GetInputString(ALPHA)};");
                visitor.AppendLine($"float roughness = {GetInputString(ROUGHNESS)};");
                visitor.AppendLine($"float metallic = {GetInputString(METALLIC)};");
            }
        }
    }
}