using Graphlit.Nodes;
using System;
using System.Linq;
using UnityEngine;
using UnityEngine.UIElements;

namespace Graphlit
{
    [NodeInfo("Constants/Slider"), Serializable]
    public class SliderConstantNode : FloatNode, IConvertablePropertyNode
    {
        [SerializeField] float _min = 0;
        [SerializeField] float _max = 1;

        Slider _slider = null;
        public override void Initialize()
        {
            InitializeFloatNode();

            _slider = new Slider()
            {
                value = _value,
                lowValue = _min,
                highValue = _max,
                showInputField = true,
                style =
                {
                    width = 140
                }
                
            };
            //_slider.Children().First().style.minWidth = 0;

            var input = _slider.Q<TextField>("unity-text-field");
            input.style.width = 30;

            _slider.RegisterValueChangedCallback((evt) =>
            {
                _value = evt.newValue;
                UpdatePreviewMaterial();
            });
            inputContainer.Add(_slider);
        }

        public override void AdditionalElements(VisualElement root)
        {
            var minMax = new Vector2Field("Min Max") { value = new Vector2(_min, _max) };
            minMax.RegisterValueChangedCallback(evt =>
            {
                _min = evt.newValue.x;
                _max = evt.newValue.y;

                if (_slider is not null)
                {
                    _slider.lowValue = _min;
                    _slider.highValue = _max;
                }
            });
            root.Add(minMax);
        }

        public new void CopyConstant(PropertyDescriptor propertyDescriptor)
        {
            _value = propertyDescriptor.FloatValue;
            _min = propertyDescriptor.rangeX;
            _max = propertyDescriptor.rangeY;
        }

        public new PropertyNode ToProperty()
        {
            var graphData = GraphView.graphData;

            var prop = new FloatPropertyNode
            {
                _ref = viewDataKey
            };

            var desc = new PropertyDescriptor(PropertyType.Float, GetSuggestedPropertyName())
            {
                guid = viewDataKey,
                rangeX = _min,
                rangeY = _max,
                FloatValue = _value
            };

            graphData.properties.Add(desc);
            return prop;
        }
    }
}