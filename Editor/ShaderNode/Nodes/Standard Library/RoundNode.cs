using Enlit.Nodes;

namespace Enlit
{
    [NodeInfo("Math/Round")]
    public class RoundNode : PassthroughNode
    {
        protected override void Generate(NodeVisitor visitor)
        {
            base.Generate(visitor);
            Output(visitor, OUT, $"round({PortData[IN].Name})");
        }
    }
}