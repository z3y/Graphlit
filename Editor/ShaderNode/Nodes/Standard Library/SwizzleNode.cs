using System;
using UnityEngine.UIElements;
using UnityEngine;
using Graphlit.Nodes;
using Graphlit.Nodes.PortType;

namespace Graphlit
{
    [@NodeInfo("Utility/Swizzle"), Serializable]
    public sealed class SwizzleNode : ShaderNode
    {
        const int IN = 0;
        const int OUT = 1;
        [SerializeField] internal string swizzle = "x";

        public override void Initialize()
        {
            AddPort(new(PortDirection.Input, new Float(1, true), IN));
            AddPort(new(PortDirection.Output, new Float(1, true), OUT));

            var f = new TextField { value = swizzle };
            f.RegisterValueChangedCallback((evt) =>
            {
                string newValue = Swizzle.Validate(evt, f);
                if (!swizzle.Equals(newValue))
                {
                    swizzle = newValue;
                    GeneratePreviewForAffectedNodes();
                }
            });
            extensionContainer.Add(f);
        }

        protected override void Generate(NodeVisitor visitor)
        {
            int components = swizzle.Length;
            var data = new GeneratedPortData(new Float(components, false), PortData[IN].Name + "." + swizzle);
            PortData[OUT] = data;
        }
    }
}