using ZSG.Nodes;
using ZSG.Nodes.PortType;

namespace ZSG
{
    public abstract class ParameterNode : ShaderNode
    {
        const int OUT = 0;
        public override Precision DefaultPrecisionOverride => Precision.Float;
        public abstract (string, Float) Parameter { get; }
        public override bool DisablePreview => true;
        public sealed override void Initialize()
        {
            AddPort(new(PortDirection.Output, Parameter.Item2, OUT, Parameter.Item1));
        }

        protected sealed override void Generate(NodeVisitor visitor)
        {
            var data = PortData[OUT];
            data.Name = Parameter.Item1;
            PortData[OUT] = data;
        }
    }
}