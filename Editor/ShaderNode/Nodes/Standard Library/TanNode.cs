using ZSG.Nodes;

namespace ZSG
{
    [NodeInfo("Math/Tan")]
    public class TanNode : PassthroughNode
    {
        protected override void Generate(NodeVisitor visitor)
        {
            base.Generate(visitor);
            Output(visitor, OUT, $"tan({PortData[IN].Name})");
        }
    }
}