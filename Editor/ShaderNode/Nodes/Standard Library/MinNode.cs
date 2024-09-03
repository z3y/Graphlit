using Graphlit.Nodes;

namespace Graphlit
{
    [NodeInfo("Math/Min")]
    public class MinNode : SimpleExpressionNode
    {
        protected override string Expression => $"min({PortData[A].Name}, {PortData[B].Name})";
    }
}