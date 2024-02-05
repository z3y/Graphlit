using ZSG.Nodes;

namespace ZSG
{
    [NodeInfo("Math/FWidth")]
    public class FWidthNode : PasstroughNode
    {
        protected override void Generate(NodeVisitor visitor)
        {
            base.Generate(visitor);
            Output(visitor, OUT, $"fwidth({PortData[IN].Name})");
        }
    }
}