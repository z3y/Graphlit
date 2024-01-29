using ZSG.Nodes;

namespace ZSG
{
    [NodeInfo("Math/Subtract", "a - b")]
    public class SubtractNode : SimpleExpressionNode
    {
        protected override string Operator => "-";
    }
}