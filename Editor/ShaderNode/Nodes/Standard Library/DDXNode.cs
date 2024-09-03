using Graphlit.Nodes;

namespace Graphlit
{
    [NodeInfo("Math/DDX")]
    public class DDXNode : PassthroughNode
    {
        public override Precision DefaultPrecisionOverride => Precision.Float;

        protected override void Generate(NodeVisitor visitor)
        {
            base.Generate(visitor);
            Output(visitor, OUT, $"ddx({PortData[IN].Name})");
        }
    }
}