using Enlit.Nodes;

namespace Enlit
{
    [NodeInfo("Math/Cos", "cos(A)")]
    public class CosNode : PassthroughNode
    {
        protected override void Generate(NodeVisitor visitor)
        {
            base.Generate(visitor);
            Output(visitor, OUT, $"cos({PortData[IN].Name})");
        }
    }
}