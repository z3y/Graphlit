using Graphlit.Nodes;

namespace Graphlit
{
    [NodeInfo("Math/Divide", "a / b")]
    public class DivideNode : SimpleExpressionNode
    {
        protected override string Operator => "/";
    }
}