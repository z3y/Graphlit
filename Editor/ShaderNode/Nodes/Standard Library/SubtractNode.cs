using Graphlit.Nodes;

namespace Graphlit
{
    [NodeInfo("Math/Subtract", "a - b")]
    public class SubtractNode : SimpleExpressionNode
    {
        protected override string Operator => "-";
    }
}