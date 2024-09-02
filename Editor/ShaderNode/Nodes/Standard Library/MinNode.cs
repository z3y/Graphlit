using Enlit.Nodes;

namespace Enlit
{
    [NodeInfo("Math/Min")]
    public class MinNode : SimpleExpressionNode
    {
        protected override string Expression => $"min({PortData[A].Name}, {PortData[B].Name})";
    }
}