using System;
using UnityEngine.UIElements;
using UnityEngine;
using ZSG.Nodes;
using ZSG.Nodes.PortType;
using System.Linq;
using UnityEditor.UIElements;

namespace ZSG
{
    [NodeInfo("Constants/Bool"), Serializable]
    public class BooleanConstantNode : ShaderNode
    {
        const int OUT = 0;
        [SerializeField] private bool _value = false;

        PropertyDescriptor _descriptor = new(PropertyType.Bool);

        public override bool DisablePreview => true;
        public override void Initialize()
        {
            AddPort(new(PortDirection.Output, new Bool(), OUT));

            onUpdatePreviewMaterial += (mat) =>
            {
                mat.SetFloat(_descriptor.GetReferenceName(GenerationMode.Preview), _value ? 1 : 0);
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
                _descriptor.FloatValue = _value ? 1 : 0;
                visitor.AddProperty(_descriptor);
                PortData[OUT] = new GeneratedPortData(new Bool(), _descriptor.GetReferenceName(GenerationMode.Preview));
            }
            else
            {
                SetVariable(OUT, $"{_value.ToString(System.Globalization.CultureInfo.InvariantCulture).ToLower()}");
            }
        }
    }
}