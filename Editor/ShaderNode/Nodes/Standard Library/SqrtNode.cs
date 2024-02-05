using ZSG.Nodes;

namespace ZSG
{
    [NodeInfo("Math/Square Root")]
    public class SqrtNode : PasstroughNode
    {
        protected override void Generate(NodeVisitor visitor)
        {
            base.Generate(visitor);
            Output(visitor, OUT, $"sqrt({PortData[IN].Name})");
        }
    }
}