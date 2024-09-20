using UnityEngine;
using Graphlit.Nodes;
using Graphlit.Nodes.PortType;

namespace Graphlit
{
    [NodeInfo("Utility/Remap")]
    public class RemapNode : ShaderNode
    {
        const int IN = 0;
        const int INMINMAX = 1;
        const int OUTMINMAX = 2;
        const int OUT = 3;

        public override void Initialize()
        {
            AddPort(new(PortDirection.Input, new Float(1, true), IN, "In"));
            AddPort(new(PortDirection.Input, new Float(2), INMINMAX, "In Min Max"));
            AddPort(new(PortDirection.Input, new Float(2), OUTMINMAX, "Out Min Max"));

            AddPort(new(PortDirection.Output, new Float(1, true), OUT, "Out"));

            DefaultValues[INMINMAX] = "float2(0, 1)";
            DefaultValues[OUTMINMAX] = "float2(0, 1)";
        }

        protected override void Generate(NodeVisitor visitor)
        {
            ChangeDimensions(OUT, GetDimensions(IN));
           
            Output(visitor, OUT, $"{PortData[OUTMINMAX].Name}.x + ({PortData[IN].Name} - {PortData[INMINMAX].Name}.x) * ({PortData[OUTMINMAX].Name}.y - {PortData[OUTMINMAX].Name}.x) / ({PortData[INMINMAX].Name}.y - {PortData[INMINMAX].Name}.x)");
        }
    }
}