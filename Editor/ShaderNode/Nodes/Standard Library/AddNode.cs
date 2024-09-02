using Enlit.Nodes;

namespace Enlit
{
    [NodeInfo("Math/Add", "a + b")]
    public class AddNode : SimpleExpressionNode
    {
        protected override string Operator => "+";
    }
}