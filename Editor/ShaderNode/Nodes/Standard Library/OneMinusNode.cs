using ZSG.Nodes;

namespace ZSG
{
    [NodeInfo("One Minus", "1 - a")]
    public class OneMinusNode : PasstroughNode
    {
        protected override void Generate(NodeVisitor visitor)
        {
            base.Generate(visitor);
            Output(visitor, OUT, $"1.0 - {PortData[IN].Name}");
        }
    }
}