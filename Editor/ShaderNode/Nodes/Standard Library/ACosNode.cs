using Enlit.Nodes;

namespace Enlit
{
    [NodeInfo("Math/ACos")]
    public class ACosNode : PassthroughNode
    {
        protected override void Generate(NodeVisitor visitor)
        {
            base.Generate(visitor);
            Output(visitor, OUT, $"acos({PortData[IN].Name})");
        }
    }
}