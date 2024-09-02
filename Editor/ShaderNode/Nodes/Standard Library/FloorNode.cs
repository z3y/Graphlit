using Enlit.Nodes;

namespace Enlit
{
    [NodeInfo("Math/Floor")]
    public class FloorNode : PassthroughNode
    {
        protected override void Generate(NodeVisitor visitor)
        {
            base.Generate(visitor);
            Output(visitor, OUT, $"floor({PortData[IN].Name})");
        }
    }
}