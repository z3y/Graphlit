using ZSG.Nodes;

namespace ZSG
{
    [NodeInfo("Math/Abs")]
    public class AbsNode : PasstroughNode
    {
        protected override void Generate(NodeVisitor visitor)
        {
            base.Generate(visitor);
            Output(visitor, OUT, $"abs({PortData[IN].Name})");
        }
    }
}