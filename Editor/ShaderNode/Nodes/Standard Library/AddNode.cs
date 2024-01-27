using ZSG.Nodes;

namespace ZSG
{
    [NodeInfo("Add", "a + b")]
    public class AddNode : SimpleExpressionNode
    {
        protected override string Operator => "+";
    }
}