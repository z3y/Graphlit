using ZSG.Nodes;

namespace ZSG
{
    [NodeInfo("Math/ATan")]
    public class ATanNode : PassthroughNode
    {
        protected override void Generate(NodeVisitor visitor)
        {
            base.Generate(visitor);
            Output(visitor, OUT, $"atan({PortData[IN].Name})");
        }
    }
}