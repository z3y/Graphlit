using ZSG.Nodes;

namespace ZSG
{
    [NodeInfo("Math/Floor")]
    public class FloorNode : PasstroughNode
    {
        protected override void Generate(NodeVisitor visitor)
        {
            base.Generate(visitor);
            Output(visitor, OUT, $"floor({PortData[IN].Name})");
        }
    }
}