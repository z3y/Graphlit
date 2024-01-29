using System;
using UnityEngine.UIElements;
using UnityEngine;
using ZSG.Nodes;
using ZSG.Nodes.PortType;
using System.Linq;

namespace ZSG
{

    [NodeInfo("Constants/Float"), Serializable]
    public class FloatNode : ShaderNode
    {
        const int OUT = 0;
        [SerializeField] private float _value;

        PropertyDescriptor _descriptor = new(PropertyType.Float);

        public override bool DisablePreview => true;
        public override void AddElements()
        {
            AddPort(new(PortDirection.Output, new Float(1), OUT));

            onUpdatePreviewMaterial += (mat) =>
            {
                mat.SetFloat(_descriptor.GetReferenceName(GenerationMode.Preview), _value);
            };

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
                _descriptor.FloatValue = _value;
                visitor.AddProperty(_descriptor);
                PortData[OUT] = new GeneratedPortData(new Float(1), _descriptor.GetReferenceName(GenerationMode.Preview));
            }
            else
            {
                SetVariable(OUT, $"{PrecisionString(1)}({_value})");
            }
        }
    }
}