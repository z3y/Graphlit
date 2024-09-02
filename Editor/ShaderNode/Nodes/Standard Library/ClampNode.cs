using UnityEngine;
using Enlit.Nodes;
using Enlit.Nodes.PortType;

namespace Enlit
{
    [NodeInfo("Utility/Clamp")]
    public class ClampNode : ShaderNode
    {
        const int IN = 0;
        const int MIN = 1;
        const int MAX = 2;
        const int OUT = 3;

        public override void Initialize()
        {
            AddPort(new(PortDirection.Input, new Float(1, true), IN, "In"));
            AddPort(new(PortDirection.Input, new Float(1, true), MIN, "Min"));
            AddPort(new(PortDirection.Input, new Float(1, true), MAX, "Max"));

            AddPort(new(PortDirection.Output, new Float(1, true), OUT));
        }

        protected override void Generate(NodeVisitor visitor)
        {
            ChangeDimensions(OUT, ImplicitTruncation(IN, MIN, MAX).dimensions);

            Output(visitor, OUT, $"clamp({PortData[IN].Name}, {PortData[MIN].Name}, {PortData[MAX].Name})");
        }
    }
}