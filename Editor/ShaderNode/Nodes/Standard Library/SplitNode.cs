using UnityEditor.Experimental.GraphView;
using Enlit.Nodes;
using Enlit.Nodes.PortType;

namespace Enlit
{
    [NodeInfo("Utility/Split")]
    public class SplitNode : ShaderNode
    {
        const int R = 0;
        const int G = 1;
        const int B = 2;
        const int A = 3;
        const int IN = 4;

        Port _r;
        Port _g;
        Port _b;
        Port _a;

        public override bool DisablePreview => true;

        public override void Initialize()
        {
            AddPort(new(PortDirection.Input, new Float(4, true), IN));

            _r = AddPort(new(PortDirection.Output, new Float(1), R, "R"));
            _g = AddPort(new(PortDirection.Output, new Float(1), G, "G"));
            _b = AddPort(new(PortDirection.Output, new Float(1), B, "B"));
            _a = AddPort(new(PortDirection.Output, new Float(1), A, "A"));
        }

        protected override void Generate(NodeVisitor visitor)
        {
            int c = GetDimensions(IN);
            string name = PortData[IN].Name;

            _g.visible = c >= 2;
            _b.visible = c >= 3;
            _a.visible = c >= 4;
            const int H = 24;
            _g.style.height = c >= 2 ? H : 0;
            _b.style.height = c >= 3 ? H : 0;
            _a.style.height = c >= 4 ? H : 0;

            string zero = PrecisionString(1) + "(0)";
            SetVariable(R, c >= 1 ? name + ".x" : zero);
            SetVariable(G, c >= 2 ? name + ".y" : zero);
            SetVariable(B, c >= 3 ? name + ".z" : zero);
            SetVariable(A, c >= 4 ? name + ".w" : zero);
        }
    }
}