using Graphlit.Nodes;
using Graphlit.Nodes.PortType;

namespace Graphlit
{
    [NodeInfo("Input/Front Face")]
    public class FrontFaceNode : ShaderNode
    {
        public override bool DisablePreview => true;
        public override void Initialize()
        {
            AddPort(new(PortDirection.Output, new Bool(), 0));
            Bind(0, PortBinding.FrontFace);
        }

        protected override void Generate(NodeVisitor visitor)
        {
        }
    }
}