using ZSG.Nodes;

namespace ZSG
{
    [NodeInfo("Math/Add", "a + b")]
    public class AddNode : SimpleExpressionNode
    {
        protected override string Operator => "+";
    }
}