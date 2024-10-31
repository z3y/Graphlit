using Graphlit.Nodes;

namespace Graphlit
{
    [NodeInfo("Math/ATan2")]
    public class Atan2Node : SimpleExpressionNode
    {
        protected override string Expression => $"atan2({PortData[A].Name}, {PortData[B].Name})";
    }
}