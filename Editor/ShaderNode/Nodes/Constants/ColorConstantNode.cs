using System;
using UnityEngine.UIElements;
using UnityEngine;
using Graphlit.Nodes;
using Graphlit.Nodes.PortType;
using System.Linq;
using UnityEditor.UIElements;

namespace Graphlit
{
    [NodeInfo("Constants/Color"), Serializable]
    public class ColorNode : ShaderNode
    {
        const int OUT = 0;
        [SerializeField] private Vector4 _value = Vector4.one;

        PropertyDescriptor _descriptor = new(PropertyType.Color);

        public override bool DisablePreview => true;
        public override void Initialize()
        {
            AddPort(new(PortDirection.Output, new Float(4), OUT));

            onUpdatePreviewMaterial += (mat) =>
            {
                mat.SetColor(_descriptor.GetReferenceName(GenerationMode.Preview), _value);
            };

            var f = new ColorField() { value = _value };
            f.style.width = 60;
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
                PortData[OUT] = new GeneratedPortData(new Float(4), _descriptor.GetReferenceName(GenerationMode.Preview));
            }
            else
            {
                Vector4 v;
                v.x = MathF.Pow(_value.x, 2.2f);
                v.y = MathF.Pow(_value.y, 2.2f);
                v.z = MathF.Pow(_value.z, 2.2f);
                v.w = MathF.Pow(_value.w, 2.2f);

                SetVariable(OUT, $"{PrecisionString(4)}{v}");
            }
        }
    }
}