using ZSG.Nodes;
using ZSG.Nodes.PortType;

namespace ZSG
{
    [NodeInfo("Built-in Variables/Time", "_Time")]
    public class TimeNode : ShaderNode
    {
        public override Precision DefaultPrecisionOverride => Precision.Float;
        public override bool DisablePreview => true;
        public sealed override void Initialize()
        {
            AddPort(new(PortDirection.Output, new Float(1), 0, "t/20"));
            AddPort(new(PortDirection.Output, new Float(1), 1, "t"));
            AddPort(new(PortDirection.Output, new Float(1), 2, "t*2"));
            AddPort(new(PortDirection.Output, new Float(1), 3, "t*3"));
        }

        protected sealed override void Generate(NodeVisitor visitor)
        {
            string mask = "xyzw";
            for (int i = 0; i < 4; i++)
            {
                var data = PortData[i];
                data.Name = "_Time." + mask[i];
                PortData[i] = data;
            }
        }
    }
}