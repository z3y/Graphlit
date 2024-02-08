using ZSG.Nodes;
using ZSG.Nodes.PortType;

namespace ZSG
{
    [NodeInfo("Built-in Variables/Camera")]
    public class CameraNode : ShaderNode
    {
        public override Precision DefaultPrecisionOverride => Precision.Float;
        public override bool DisablePreview => true;
        public sealed override void Initialize()
        {
            AddPort(new(PortDirection.Output, new Float(3), 0, "Position"));
            AddPort(new(PortDirection.Output, new Float(1), 1, "Near Plane"));
            AddPort(new(PortDirection.Output, new Float(1), 2, "Far Plane"));
            AddPort(new(PortDirection.Output, new Float(1), 3, "Width"));
            AddPort(new(PortDirection.Output, new Float(1), 4, "Height"));
        }

        protected sealed override void Generate(NodeVisitor visitor)
        {
            SetVariable(0, "_WorldSpaceCameraPos");
            SetVariable(1, "_ProjectionParams.y");
            SetVariable(2, "_ProjectionParams.z");
            SetVariable(3, "_ScreenParams.x");
            SetVariable(4, "_ScreenParams.y");
        }
    }
}