using ZSG.Nodes;

namespace ZSG
{
    [NodeInfo("Math/Step")]
    public class StepNode : SimpleExpressionNode
    {
        protected override string Expression => $"step({PortData[A].Name}, {PortData[B].Name})";
    }
}