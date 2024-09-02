using Enlit.Nodes;

namespace Enlit
{
    [NodeInfo("Math/Ceil")]
    public class CeilNode : PassthroughNode
    {
        protected override void Generate(NodeVisitor visitor)
        {
            base.Generate(visitor);
            Output(visitor, OUT, $"ceil({PortData[IN].Name})");
        }
    }
}