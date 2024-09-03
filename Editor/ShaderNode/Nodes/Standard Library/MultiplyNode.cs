using Graphlit.Nodes;

namespace Graphlit
{
    [NodeInfo("Math/Multiply", "a * b")]
    public class MultiplyNode : SimpleExpressionNode
    {
        protected override string Operator => "*";
    }
}