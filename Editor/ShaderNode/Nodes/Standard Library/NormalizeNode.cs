using Enlit.Nodes;

namespace Enlit
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