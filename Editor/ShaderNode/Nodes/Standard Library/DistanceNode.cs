using Enlit.Nodes;

namespace Enlit
{
    [NodeInfo("Math/Distance")]
    public class DistanceNode : SimpleExpressionNode
    {
        protected override string Expression => $"distance({PortData[A].Name}, {PortData[B].Name})";
        protected override bool TruncateOutput => false;
    }
}