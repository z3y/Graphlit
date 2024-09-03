using Graphlit.Nodes;

namespace Graphlit
{
    [NodeInfo("Math/Add", "a + b")]
    public class AddNode : SimpleExpressionNode
    {
        protected override string Operator => "+";
    }
}