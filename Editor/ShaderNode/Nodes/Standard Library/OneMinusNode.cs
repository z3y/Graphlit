using Graphlit.Nodes;

namespace Graphlit
{
    [NodeInfo("Math/One Minus", "1 - a")]
    public class OneMinusNode : PassthroughNode
    {
        protected override void Generate(NodeVisitor visitor)
        {
            base.Generate(visitor);
            Output(visitor, OUT, $"1.0 - {PortData[IN].Name}");
        }
    }
}