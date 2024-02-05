using ZSG.Nodes;

namespace ZSG
{
    [NodeInfo("Math/Length")]
    public class LengthhNode : PasstroughNode
    {
        protected override void Generate(NodeVisitor visitor)
        {
            base.Generate(visitor);
            Output(visitor, OUT, $"length({PortData[IN].Name})");
        }
    }
}