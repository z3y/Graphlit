using ZSG.Nodes;

namespace ZSG
{
    [NodeInfo("Math/Max")]
    public class MaxNode : SimpleExpressionNode
    {
        protected override string Expression => $"max({PortData[A].Name}, {PortData[B].Name})";
    }
}