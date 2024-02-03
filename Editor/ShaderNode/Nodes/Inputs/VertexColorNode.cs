using ZSG.Nodes;
using ZSG.Nodes.PortType;

namespace ZSG
{
    [NodeInfo("Input/Vertex Color")]
    public class VertexColorNode : ShaderNode
    {
        public override void AddElements()
        {
            AddPort(new(PortDirection.Output, new Float(4), 0));
            Bind(0, PortBinding.VertexColor);
        }

        protected override void Generate(NodeVisitor visitor)
        {
        }
    }
}