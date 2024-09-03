using Graphlit.Nodes;

namespace Graphlit
{
    [NodeInfo("Math/Pow")]
    public class PowNode : SimpleExpressionNode
    {
        protected override string Expression => $"pow({PortData[A].Name}, {PortData[B].Name})";
        public override string AName => "A";
        public override string BName => "Pow";
    }
}