using System;
using UnityEngine;
using ZSG.Nodes;
using ZSG.Nodes.PortType;

namespace ZSG
{
    [NodeInfo("Utility/Color Mask"), Serializable]
    public class ColorMaskNode : ShaderNode
    {
        const int IN = 0;
        const int COLOR = 1;
        const int RANGE = 2;
        const int SOFTNESS = 3;
        const int OUT = 4;
        public override void Initialize()
        {
            AddPort(new(PortDirection.Input, new Float(3), IN, "In"));
            AddPort(new(PortDirection.Input, new Float(3), COLOR, "Color"));
            AddPort(new(PortDirection.Input, new Float(1), RANGE, "Range"));
            AddPort(new(PortDirection.Input, new Float(1), SOFTNESS, "Softness"));
            AddPort(new(PortDirection.Output, new Float(1), OUT, "Out"));

            DefaultValues[RANGE] = "0.04";
            DefaultValues[SOFTNESS] = "0.01";
        }

        protected override void Generate(NodeVisitor visitor)
        {
            string distanceVar = UniqueVariable.ToString() + "Distance";
            visitor.AppendLine($"{PrecisionString(1)} {distanceVar} = distance({PortData[IN].Name}, {PortData[COLOR].Name});");
            Output(visitor, OUT, $"1.0 - saturate(({distanceVar} - {PortData[RANGE].Name}) / {PortData[SOFTNESS].Name})");
        }
    }
}