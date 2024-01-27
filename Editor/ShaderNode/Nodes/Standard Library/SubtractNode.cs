using ZSG.Nodes;

namespace ZSG
{
    [NodeInfo("Subtract", "a - b")]
    public class SubtractNode : SimpleExpressionNode
    {
        protected override string Operator => "-";
    }
}