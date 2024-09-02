using Enlit.Nodes;
using Enlit.Nodes.PortType;

namespace Enlit
{
    [NodeInfo("Input/Scene Depth")]
    public class SceneDepthNode : ShaderNode
    {
        protected const int UV = 0;
        protected const int OUT = 1;

        public override void Initialize()
        {
            AddPort(new(PortDirection.Input, new Float(2), UV, "UV"));
            AddPort(new(PortDirection.Output, new Float(4), OUT));

            Bind(UV, PortBinding.ScreenPosition);
        }

        protected override void Generate(NodeVisitor visitor)
        {
            Output(visitor, OUT, $"SampleSceneDepth({PortData[UV].Name})");
        }
    }
}