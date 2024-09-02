using Enlit.Nodes;

namespace Enlit
{
    [NodeInfo("Math/Multiply", "a * b")]
    public class MultiplyNode : SimpleExpressionNode
    {
        protected override string Operator => "*";
    }
}