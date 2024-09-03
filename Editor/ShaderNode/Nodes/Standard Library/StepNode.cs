using Graphlit.Nodes;

namespace Graphlit
{
    [NodeInfo("Math/Step")]
    public class StepNode : SimpleExpressionNode
    {
        protected override string Expression => $"step({PortData[A].Name}, {PortData[B].Name})";
    }
}