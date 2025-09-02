using System;
using UnityEngine.UIElements;
using UnityEngine;
using Graphlit.Nodes;
using Graphlit.Nodes.PortType;
using System.Linq;

namespace Graphlit
{

    [NodeInfo("Constants/Float"), Serializable]
    public class FloatNode : ConstantPropertyNode, IConvertablePropertyNode
    {
        const int OUT = 0;
        [SerializeField] protected float _value;

        PropertyDescriptor _descriptor;
        public PropertyDescriptor Descriptor => _descriptor ??= new(PropertyType.Float) { guid = viewDataKey };

        public override bool DisablePreview => true;

        public void InitializeFloatNode()
        {
            AddPort(new(PortDirection.Output, new Float(1), OUT));

            onUpdatePreviewMaterial += (mat) =>
            {
                mat.SetFloat(Descriptor.GetReferenceName(GenerationMode.Preview), _value);
            };
        }

        public override void Initialize()
        {
            InitializeFloatNode();

            var f = new FloatField("X") { value = _value };
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
                Descriptor.FloatValue = _value;
                visitor.AddProperty(Descriptor);
                PortData[OUT] = new GeneratedPortData(new Float(1), Descriptor.GetReferenceName(GenerationMode.Preview));
            }
            else
            {
                SetVariable(OUT, $"{PrecisionString(1)}({_value.ToString(System.Globalization.CultureInfo.InvariantCulture)})");
            }
        }

        public void CopyConstant(PropertyDescriptor propertyDescriptor)
        {
            _value = propertyDescriptor.FloatValue;
        }

        public PropertyNode ToProperty()
        {
            var graphData = GraphView.graphData;

            var prop = new FloatPropertyNode
            {
                _ref = viewDataKey
            };

            var desc = new PropertyDescriptor(PropertyType.Float, GetSuggestedPropertyName())
            {
                guid = viewDataKey,
                FloatValue = _value
            };

            graphData.properties.Add(desc);
            return prop;
        }
    }
}