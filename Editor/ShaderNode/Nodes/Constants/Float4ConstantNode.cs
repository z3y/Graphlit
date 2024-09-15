using System;
using UnityEngine.UIElements;
using UnityEngine;
using Graphlit.Nodes;
using Graphlit.Nodes.PortType;
using System.Linq;

namespace Graphlit
{
    [NodeInfo("Constants/Float4"), Serializable]
    public class Float4Node : ShaderNode
    {
        const int OUT = 0;
        [SerializeField] private Vector4 _value;

        PropertyDescriptor _descriptor;
        PropertyDescriptor Descriptor => _descriptor ??= new(PropertyType.Float4) { guid = viewDataKey };

        public override bool DisablePreview => true;
        public override void Initialize()
        {
            AddPort(new(PortDirection.Output, new Float(4), OUT));

            onUpdatePreviewMaterial += (mat) =>
            {
                mat.SetVector(Descriptor.GetReferenceName(GenerationMode.Preview), _value);
            };

            var f = new Vector4Field() { value = _value };
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
                PortData[OUT] = new GeneratedPortData(new Float(4), Descriptor.GetReferenceName(GenerationMode.Preview));
            }
            else
            {
                SetVariable(OUT, $"{PrecisionString(4)}{_value}");
            }
        }
    }
}