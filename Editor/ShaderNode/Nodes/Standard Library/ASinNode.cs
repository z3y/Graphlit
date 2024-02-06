using ZSG.Nodes;

namespace ZSG
{
    [NodeInfo("Math/ASin")]
    public class ASinNode : PassthroughNode
    {
        protected override void Generate(NodeVisitor visitor)
        {
            base.Generate(visitor);
            Output(visitor, OUT, $"asin({PortData[IN].Name})");
        }
    }
}