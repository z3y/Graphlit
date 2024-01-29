using ZSG.Nodes;
using ZSG.Nodes.PortType;

namespace ZSG
{
    [NodeInfo("Utility/Append")]
    public class AppendNode : ShaderNode
    {
        const int A = 0;
        const int B = 1;
        const int C = 2;
        const int D = 3;
        const int OUT = 4;

        public override void AddElements()
        {
            AddPort(new(PortDirection.Input, new Float(1), A, "R"));
            AddPort(new(PortDirection.Input, new Float(1), B, "G"));
            AddPort(new(PortDirection.Input, new Float(1), C, "B"));
            AddPort(new(PortDirection.Input, new Float(1), D, "A"));

            AddPort(new(PortDirection.Output, new Float(4), OUT));
        }

        protected override void Generate(NodeVisitor visitor)
        {
            Output(visitor, OUT, $"{PrecisionString(4)}(" +
                $"{PortData[A].Name}," +
                $"{PortData[B].Name}," +
                $"{PortData[C].Name}," +
                $"{PortData[D].Name})");
        }
    }
}