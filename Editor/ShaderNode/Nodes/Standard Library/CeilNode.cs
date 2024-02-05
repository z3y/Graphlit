using ZSG.Nodes;

namespace ZSG
{
    [NodeInfo("Math/Ceil")]
    public class CeilNode : PasstroughNode
    {
        protected override void Generate(NodeVisitor visitor)
        {
            base.Generate(visitor);
            Output(visitor, OUT, $"ceil({PortData[IN].Name})");
        }
    }
}