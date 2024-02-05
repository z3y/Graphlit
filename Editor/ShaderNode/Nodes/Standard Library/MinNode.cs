using ZSG.Nodes;

namespace ZSG
{
    [NodeInfo("Math/Min")]
    public class MinNode : SimpleExpressionNode
    {
        protected override string Expression => $"min({PortData[A].Name}, {PortData[B].Name})";
    }
}