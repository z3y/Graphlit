using Graphlit.Nodes;

namespace Graphlit
{
    [NodeInfo("Math/Normalize", "normalize(a)")]
    public class NormalizeNode : PassthroughNode
    {
        protected override void Generate(NodeVisitor visitor)
        {
            base.Generate(visitor);
            Output(visitor, OUT, $"normalize({PortData[IN].Name})");
        }
    }
}