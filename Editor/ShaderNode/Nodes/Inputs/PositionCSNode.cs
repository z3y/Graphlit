using ZSG.Nodes;
using ZSG.Nodes.PortType;

namespace ZSG
{
    [NodeInfo("Input/Clip Space Position")]
    public class PositionCSNode : ShaderNode
    {
        public override bool DisablePreview => true;
        public override void Initialize()
        {
            AddPort(new(PortDirection.Output, new Float(4), 0));
            Bind(0, PortBinding.PositionCSRaw);
        }

        protected override void Generate(NodeVisitor visitor)
        {
        }
    }
}