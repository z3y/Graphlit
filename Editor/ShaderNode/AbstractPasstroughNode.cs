using ZSG.Nodes.PortType;

namespace ZSG
{
    public abstract class PasstroughNode : ShaderNode
    {
        protected const int IN = 0;
        protected const int OUT = 1;

        public override void Initialize()
        {
            AddPort(new(PortDirection.Input, new Float(1, true), IN));
            AddPort(new(PortDirection.Output, new Float(1, true), OUT));
        }

        protected override void Generate(NodeVisitor visitor)
        {
            ChangeDimensions(OUT, GetDimensions(IN));
        }
    }
}