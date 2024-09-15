using System;
using UnityEngine.UIElements;
using UnityEngine;
using Graphlit.Nodes;
using Graphlit.Nodes.PortType;
using System.Linq;

namespace Graphlit
{
    [NodeInfo("Constants/Float2"), Serializable]
    public class Float2Node : ShaderNode
    {
        const int OUT = 0;
        [SerializeField] private Vector2 _value;

        PropertyDescriptor _descriptor;
        PropertyDescriptor Descriptor => _descriptor ??= new(PropertyType.Float2) { guid = viewDataKey };

        public override bool DisablePreview => true;
        public override void Initialize()
        {
            AddPort(new(PortDirection.Output, new Float(2), OUT));

            onUpdatePreviewMaterial += (mat) =>
            {
                mat.SetVector(Descriptor.GetReferenceName(GenerationMode.Preview), _value);
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
                Descriptor.VectorValue = _value;
                visitor.AddProperty(Descriptor);
                PortData[OUT] = new GeneratedPortData(new Float(2), Descriptor.GetReferenceName(GenerationMode.Preview));
            }
            else
            {
                SetVariable(OUT, $"{PrecisionString(2)}{_value}");
            }
        }
    }
}