using UnityEngine;
using ZSG.Nodes;
using ZSG.Nodes.PortType;

namespace ZSG
{
    [NodeInfo("Utility/Cross")]
    public class CrossNode : ShaderNode
    {
        const int A = 0;
        const int B = 1;
        const int OUT = 2;

        public override void AddElements()
        {
            AddPort(new(PortDirection.Input, new Float(3), A, "A"));
            AddPort(new(PortDirection.Input, new Float(3), B, "B"));

            AddPort(new(PortDirection.Output, new Float(3), OUT));
        }

        protected override void Generate(NodeVisitor visitor)
        {
            Output(visitor, OUT, $"cross({PortData[A].Name}, {PortData[B].Name})");
        }
    }
}