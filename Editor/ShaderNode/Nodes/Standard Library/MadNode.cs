using Graphlit.Nodes;
using Graphlit.Nodes.PortType;

namespace Graphlit
{
    [NodeInfo("Math/Multiply Add", "a * b + c")]
    public class MadNode : ShaderNode
    {
        const int A = 0;
        const int B = 1;
        const int C = 2;
        const int OUT = 3;

        public override void Initialize()
        {
            AddPort(new(PortDirection.Input, new Float(1, true), A, "A"));
            AddPort(new(PortDirection.Input, new Float(1, true), B, "B"));
            AddPort(new(PortDirection.Input, new Float(1, true), C, "C"));
            AddPort(new(PortDirection.Output, new Float(1, true), OUT));
        }

        protected override void Generate(NodeVisitor visitor)
        {
            ChangeDimensions(OUT, ImplicitTruncation(A, B, C).dimensions);
            Output(visitor, OUT, $"mad({PortData[A].Name}, {PortData[B].Name}, {PortData[C].Name})");
        }
    }
}