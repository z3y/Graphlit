using Enlit.Nodes;

namespace Enlit
{
    [NodeInfo("Math/Abs")]
    public class AbsNode : PassthroughNode
    {
        protected override void Generate(NodeVisitor visitor)
        {
            base.Generate(visitor);
            Output(visitor, OUT, $"abs({PortData[IN].Name})");
        }
    }
}