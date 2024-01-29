using ZSG.Nodes;

namespace ZSG
{
    [NodeInfo("Math/Cos", "cos(A)")]
    public class CosNode : PasstroughNode
    {
        protected override void Generate(NodeVisitor visitor)
        {
            base.Generate(visitor);
            Output(visitor, OUT, $"cos({PortData[IN].Name})");
        }
    }
}