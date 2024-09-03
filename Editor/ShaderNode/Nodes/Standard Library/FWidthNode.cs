using Graphlit.Nodes;

namespace Graphlit
{
    [NodeInfo("Math/FWidth")]
    public class FWidthNode : PassthroughNode
    {
        protected override void Generate(NodeVisitor visitor)
        {
            base.Generate(visitor);
            Output(visitor, OUT, $"fwidth({PortData[IN].Name})");
        }
    }
}