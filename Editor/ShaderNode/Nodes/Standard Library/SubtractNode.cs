using Enlit.Nodes;

namespace Enlit
{
    [NodeInfo("Math/Subtract", "a - b")]
    public class SubtractNode : SimpleExpressionNode
    {
        protected override string Operator => "-";
    }
}