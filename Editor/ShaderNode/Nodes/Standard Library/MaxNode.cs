using Graphlit.Nodes;

namespace Graphlit
{
    [NodeInfo("Math/Max")]
    public class MaxNode : SimpleExpressionNode
    {
        protected override string Expression => $"max({PortData[A].Name}, {PortData[B].Name})";
    }
}