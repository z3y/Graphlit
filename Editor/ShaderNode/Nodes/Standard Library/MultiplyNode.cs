using ZSG.Nodes;

namespace ZSG
{
    [NodeInfo("Math/Multiply", "a * b")]
    public class MultiplyNode : SimpleExpressionNode
    {
        protected override string Operator => "*";
    }
}