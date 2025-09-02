using System;
using UnityEngine.UIElements;
using UnityEngine;
using Graphlit.Nodes;
using Graphlit.Nodes.PortType;
using System.Linq;

namespace Graphlit
{
    [NodeInfo("Constants/Float4"), Serializable]
    public class Float4Node : ConstantPropertyNode, IConvertablePropertyNode
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
        public void CopyConstant(PropertyDescriptor propertyDescriptor)
        {
            _value = propertyDescriptor.VectorValue;
        }

        public PropertyNode ToProperty()
        {
            var graphData = GraphView.graphData;

            var prop = new Float4PropertyNode
            {
                _ref = viewDataKey
            };

            var desc = new PropertyDescriptor(PropertyType.Float4, GetSuggestedPropertyName())
            {
                guid = viewDataKey,
                VectorValue = _value
            };

            graphData.properties.Add(desc);
            return prop;
        }
    }
}