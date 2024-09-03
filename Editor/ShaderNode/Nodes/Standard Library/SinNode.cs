using Graphlit.Nodes;

namespace Graphlit
{
    [NodeInfo("Math/Sin", "sin(A)")]
    public class SinNode : PassthroughNode
    {
        protected override void Generate(NodeVisitor visitor)
        {
            base.Generate(visitor);
            Output(visitor, OUT, $"sin({PortData[IN].Name})");
        }
    }
}