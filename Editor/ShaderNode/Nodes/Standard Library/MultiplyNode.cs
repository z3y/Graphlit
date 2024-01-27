using ZSG.Nodes;

namespace ZSG
{
    [NodeInfo("Multiply", "a * b")]
    public class MultiplyNode : SimpleExpressionNode
    {
        protected override string Operator => "*";
    }
}