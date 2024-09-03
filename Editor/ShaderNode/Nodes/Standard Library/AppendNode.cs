using Graphlit.Nodes;
using Graphlit.Nodes.PortType;

namespace Graphlit
{
    [NodeInfo("Utility/Append")]
    public class AppendNode : ShaderNode
    {
        const int A = 0;
        const int B = 1;
        const int C = 2;
        const int D = 3;
        const int RGBA = 4;
        const int RGB = 5;
        const int RG = 6;


        public override void Initialize()
        {
            AddPort(new(PortDirection.Input, new Float(1), A, "R"));
            AddPort(new(PortDirection.Input, new Float(1), B, "G"));
            AddPort(new(PortDirection.Input, new Float(1), C, "B"));
            AddPort(new(PortDirection.Input, new Float(1), D, "A"));

            AddPort(new(PortDirection.Output, new Float(4), RGBA, "RGBA"));
            AddPort(new(PortDirection.Output, new Float(3), RGB, "RGB"));
            AddPort(new(PortDirection.Output, new Float(2), RG, "RG"));
        }

        protected override void Generate(NodeVisitor visitor)
        {
            Output(visitor, RGBA, $"{PrecisionString(4)}({PortData[A].Name}, {PortData[B].Name}, {PortData[C].Name}, {PortData[D].Name})", "_RGBA");
            Output(visitor, RGB, $"{PrecisionString(3)}({PortData[A].Name}, {PortData[B].Name}, {PortData[C].Name})", "_RGB");
            Output(visitor, RG, $"{PrecisionString(2)}({PortData[A].Name}, {PortData[B].Name})", "_RG");
        }
    }
}