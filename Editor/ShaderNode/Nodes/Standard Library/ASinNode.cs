using ZSG.Nodes;

namespace ZSG
{
    [NodeInfo("Math/ASin")]
    public class ASinNode : PasstroughNode
    {
        protected override void Generate(NodeVisitor visitor)
        {
            base.Generate(visitor);
            Output(visitor, OUT, $"asin({PortData[IN].Name})");
        }
    }
}