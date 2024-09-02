using Enlit.Nodes;
using Enlit.Nodes.PortType;

namespace Enlit
{
    [NodeInfo("Input/Vertex Color")]
    public class VertexColorNode : ShaderNode
    {
        public override void Initialize()
        {
            AddPort(new(PortDirection.Output, new Float(4), 0));
            Bind(0, PortBinding.VertexColor);
        }

        protected override void Generate(NodeVisitor visitor)
        {
        }
    }
}