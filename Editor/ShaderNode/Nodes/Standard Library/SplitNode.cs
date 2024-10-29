using UnityEditor.Experimental.GraphView;
using Graphlit.Nodes;
using Graphlit.Nodes.PortType;

namespace Graphlit
{
    [NodeInfo("Utility/Split")]
    public class SplitNode : ShaderNode
    {
        const int R = 0;
        const int G = 1;
        const int B = 2;
        const int A = 3;
        const int IN = 4;

        public override bool DisablePreview => true;

        public override void Initialize()
        {
            AddPort(new(PortDirection.Input, new Float(4, true), IN));

            AddPort(new(PortDirection.Output, new Float(1), R, "R"));
            AddPort(new(PortDirection.Output, new Float(1), G, "G"));
            AddPort(new(PortDirection.Output, new Float(1), B, "B"));
            AddPort(new(PortDirection.Output, new Float(1), A, "A"));
        }

        protected override void Generate(NodeVisitor visitor)
        {
            int c = GetDimensions(IN);
            string name = PortData[IN].Name;

            string zero = PrecisionString(1) + "(0)";
            SetVariable(R, c >= 1 ? name + ".x" : zero);
            SetVariable(G, c >= 2 ? name + ".y" : zero);
            SetVariable(B, c >= 3 ? name + ".z" : zero);
            SetVariable(A, c >= 4 ? name + ".w" : zero);
        }
    }
}