using System;
using UnityEngine;
using UnityEngine.UIElements;
using ZSG.Nodes;
using ZSG.Nodes.PortType;

namespace ZSG
{
    [NodeInfo("Utility/Blend Mode"), Serializable]
    public class BlendModeNode : ShaderNode
    {
        enum BlendMode
        {
            Normal,
            Darken,
            Multiply,
            ColorBurn,
            LinearBurn,
            Lighten,
            Screen,
            ColorDodge,
            Overlay,
            SoftLight,
            HardLight,
            VividLight,
            LinearLight,
            PinLight,
            HardMix,
            Difference,
            Exclusion,
            Subtract,
            Divide,
            Hue,
            Saturation,
            Color,
            Luminosity,
            LighterColor,
            DarkerColor,
        }

        [SerializeField] BlendMode _mode;

        const int BASE = 0;
        const int BLEND = 1;
        const int OUT = 2;

        public override void Initialize()
        {
            AddPort(new(PortDirection.Input, new Float(3), BASE, "Base"));
            AddPort(new(PortDirection.Input, new Float(3), BLEND, "Blend"));
            AddPort(new(PortDirection.Output, new Float(3), OUT, "Out"));

            var blend = new EnumField("Mode", _mode);
            blend.RegisterValueChangedCallback((evt) =>
            {
                _mode = (BlendMode)evt.newValue;
                GeneratePreviewForAffectedNodes();
            });
            extensionContainer.Add(blend);
        }

        protected override void Generate(NodeVisitor visitor)
        {
            string methodName = "BlendMode_" + _mode.ToString();

            Output(visitor, OUT, $"{methodName}({PortData[BASE].Name}, {PortData[BLEND].Name})");
        }
    }
}