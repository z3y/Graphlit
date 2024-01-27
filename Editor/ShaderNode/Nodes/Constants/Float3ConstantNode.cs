using System;
using UnityEngine.UIElements;
using UnityEngine;
using ZSG.Nodes;
using ZSG.Nodes.PortType;
using System.Linq;

namespace ZSG
{
    [NodeInfo("Float3"), Serializable]
    public class Float3Node : ShaderNode
    {
        const int OUT = 0;
        [SerializeField] private Vector3 _value;

        PropertyDescriptor _descriptor = new(PropertyType.Float3);

        public override bool DisablePreview => true;
        public override void AddElements()
        {
            AddPort(new(PortDirection.Output, new Float(3), OUT));

            onUpdatePreviewMaterial += (mat) =>
            {
                mat.SetVector(_descriptor.GetReferenceName(GenerationMode.Preview), _value);
            };

            var f = new Vector3Field() { value = _value };
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
                PortData[OUT] = new GeneratedPortData(new Float(3), _descriptor.GetReferenceName(GenerationMode.Preview));
            }
            else
            {
                SetVariable(OUT, $"{PrecisionString(3)}{_value}");
            }
        }
    }
}