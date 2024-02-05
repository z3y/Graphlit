using UnityEngine;
using ZSG.Nodes;
using ZSG.Nodes.PortType;

namespace ZSG
{
    [NodeInfo("Utility/Smoothstep", "(15)")]
    public class SmoothstepNode : ShaderNode
    {
        const int A = 0;
        const int B = 1;
        const int T = 2;
        const int OUT = 3;

        public override void AddElements()
        {
            AddPort(new(PortDirection.Input, new Float(1, true), A, "Min"));
            AddPort(new(PortDirection.Input, new Float(1, true), B, "Max"));
            AddPort(new(PortDirection.Input, new Float(1, true), T, "X"));

            AddPort(new(PortDirection.Output, new Float(1, true), OUT));
        }

        protected override void Generate(NodeVisitor visitor)
        {
            ChangeDimensions(OUT, ImplicitTruncation(A, B, T).dimensions);
            Output(visitor, OUT, $"smoothstep({PortData[A].Name}, {PortData[B].Name}, {PortData[T].Name})");
        }
    }
}