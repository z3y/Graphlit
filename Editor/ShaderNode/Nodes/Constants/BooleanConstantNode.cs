using System;
using UnityEngine.UIElements;
using UnityEngine;
using Graphlit.Nodes;
using Graphlit.Nodes.PortType;
using System.Linq;
using UnityEditor.UIElements;

namespace Graphlit
{
    [NodeInfo("Constants/Bool"), Serializable]
    public class BooleanConstantNode : ConstantPropertyNode, IConvertablePropertyNode
    {
        const int OUT = 0;
        [SerializeField] private bool _value = false;

        PropertyDescriptor _descriptor;
        PropertyDescriptor Descriptor => _descriptor ??= new(PropertyType.Bool) { guid = viewDataKey };

        public override bool DisablePreview => true;
        public override void Initialize()
        {
            AddPort(new(PortDirection.Output, new Bool(), OUT));

            onUpdatePreviewMaterial += (mat) =>
            {
                mat.SetFloat(Descriptor.GetReferenceName(GenerationMode.Preview), _value ? 1 : 0);
            };

            var f = new Toggle() { value = _value };
            //f.style.width = 60;
            //f.Children().First().style.minWidth = 0;
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
                Descriptor.FloatValue = _value ? 1 : 0;
                visitor.AddProperty(Descriptor);
                PortData[OUT] = new GeneratedPortData(new Bool(), Descriptor.GetReferenceName(GenerationMode.Preview));
            }
            else
            {
                SetVariable(OUT, $"{_value.ToString(System.Globalization.CultureInfo.InvariantCulture).ToLower()}");
            }
        }

        public void CopyConstant(PropertyDescriptor propertyDescriptor)
        {
            _value = propertyDescriptor.FloatValue > 0;
        }

        public PropertyNode ToProperty()
        {
            var graphData = GraphView.graphData;

            var prop = new BooleanPropertyNode
            {
                _ref = viewDataKey
            };

            var desc = new PropertyDescriptor(PropertyType.Bool, GetSuggestedPropertyName())
            {
                guid = viewDataKey,
                FloatValue = _value ? 1 : 0
            };

            graphData.properties.Add(desc);
            return prop;
        }
    }
}