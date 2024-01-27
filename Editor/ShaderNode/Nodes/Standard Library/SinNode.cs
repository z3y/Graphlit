using ZSG.Nodes;

namespace ZSG
{
    [NodeInfo("Sin", "sin(A)")]
    public class SinNode : PasstroughNode
    {
        protected override void Generate(NodeVisitor visitor)
        {
            base.Generate(visitor);
            Output(visitor, OUT, $"sin({PortData[IN].Name})");
        }
    }
}