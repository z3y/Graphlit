using Graphlit.Nodes;

namespace Graphlit
{
    [NodeInfo("Math/DDY")]
    public class DDYNode : PassthroughNode
    {
        public override Precision DefaultPrecisionOverride => Precision.Float;

        protected override void Generate(NodeVisitor visitor)
        {
            base.Generate(visitor);
            Output(visitor, OUT, $"ddy({PortData[IN].Name})");
        }
    }

}