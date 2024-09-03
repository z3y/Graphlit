using Graphlit.Nodes;

namespace Graphlit
{
    [NodeInfo("Math/Dot", "dot(a, b)")]
    public class DotNode : SimpleExpressionNode
    {
        protected override string Expression => $"dot({PortData[A].Name}, {PortData[B].Name})";
        protected override bool TruncateOutput => false;
    }
}