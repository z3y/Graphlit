using UnityEngine;
using Graphlit.Nodes;
using Graphlit.Nodes.PortType;

namespace Graphlit
{
    [NodeInfo("Utility/Inverse Lerp")]
    public class InverseLerpNode : ShaderNode
    {
        const int A = 0;
        const int B = 1;
        const int T = 2;
        const int OUT = 3;

        public override void Initialize()
        {
            AddPort(new(PortDirection.Input, new Float(1, true), A, "A"));
            AddPort(new(PortDirection.Input, new Float(1, true), B, "B"));
            AddPort(new(PortDirection.Input, new Float(1, true), T, "T"));

            AddPort(new(PortDirection.Output, new Float(1, true), OUT));
        }

        protected override void Generate(NodeVisitor visitor)
        {
            ChangeDimensions(OUT, ImplicitTruncation(A, B, T).dimensions);
            Output(visitor, OUT, $"({PortData[T].Name} - {PortData[A].Name})/({PortData[B].Name} - {PortData[A].Name})");
        }
    }
}