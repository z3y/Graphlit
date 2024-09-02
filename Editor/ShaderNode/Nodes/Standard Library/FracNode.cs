using Enlit.Nodes;

namespace Enlit
{
    [NodeInfo("Math/Frac")]
    public class FracNode : PassthroughNode
    {
        protected override void Generate(NodeVisitor visitor)
        {
            base.Generate(visitor);
            Output(visitor, OUT, $"frac({PortData[IN].Name})");
        }
    }
}