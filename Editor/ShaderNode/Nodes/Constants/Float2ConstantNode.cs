using System;
using UnityEngine.UIElements;
using UnityEngine;
using Enlit.Nodes;
using Enlit.Nodes.PortType;
using System.Linq;

namespace Enlit
{
    [NodeInfo("Constants/Float2"), Serializable]
    public class Float2Node : ShaderNode
    {
        const int OUT = 0;
        [SerializeField] private Vector2 _value;

        PropertyDescriptor _descriptor = new(PropertyType.Float2);

        public override bool DisablePreview => true;
        public override void Initialize()
        {
            AddPort(new(PortDirection.Output, new Float(2), OUT));

            onUpdatePreviewMaterial += (mat) =>
            {
                mat.SetVector(_descriptor.GetReferenceName(GenerationMode.Preview), _value);
            };

            var f = new Vector2Field() { value = _value };
            f.Children().First().style.minWidth = 0;
            f.RegisterValueChangedCallback((evt) =>
            {
                _value = evt.newValue;
                UpdatePreviewMaterial();
            });
            inputContainer.Add(f);
        }

        protected override void Generate(NodeVisitor visitor)
        {
            if (visitor.GenerationMode == GenerationMode.Preview)
            {
                _descriptor.VectorValue = _value;
                visitor.AddProperty(_descriptor);
                PortData[OUT] = new GeneratedPortData(new Float(2), _descriptor.GetReferenceName(GenerationMode.Preview));
            }
            else
            {
                SetVariable(OUT, $"{PrecisionString(2)}{_value}");
            }
        }
    }
}