using ZSG.Nodes;
using ZSG.Nodes.PortType;

namespace ZSG
{
    [NodeInfo("Input/Scene Color")]
    public class SceneColorNode : ShaderNode
    {
        protected const int UV = 0;
        protected const int OUT = 1;

        public override void Initialize()
        {
            AddPort(new(PortDirection.Input, new Float(2), UV, "UV"));
            AddPort(new(PortDirection.Output, new Float(4), OUT));

            Bind(UV, PortBinding.PositionNDC);
        }

        protected override void Generate(NodeVisitor visitor)
        {
            visitor.RequireGrabpass();
            Output(visitor, OUT, $"SampleSceneColor({PortData[UV].Name})");
        }
    }
}