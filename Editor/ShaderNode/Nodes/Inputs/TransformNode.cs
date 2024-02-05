using System;
using UnityEngine;
using UnityEngine.UIElements;
using ZSG.Nodes;
using ZSG.Nodes.PortType;

namespace ZSG
{
    [NodeInfo("Input/Transform"), Serializable]
    public class TransformNode : ShaderNode
    {
        public override PreviewType DefaultPreviewOverride => PreviewType.Preview3D;
        [SerializeField] SpaceTransform.Space _from = SpaceTransform.Space.Object;
        [SerializeField] SpaceTransform.Space _to = SpaceTransform.Space.World;
        [SerializeField] SpaceTransform.Type _type = SpaceTransform.Type.Position;
        [SerializeField] bool _normalize = true;

        public override Precision DefaultPrecisionOverride => Precision.Float;

        const int IN = 0;
        const int OUT = 1;

        public override bool DisablePreview => true;

        public override void Initialize()
        {
            AddPort(new(PortDirection.Input, new Float(3), IN, "In"));
            AddPort(new(PortDirection.Output, new Float(3), OUT, "Out"));

            var dropdown = new EnumField("From", _from);
            dropdown.RegisterValueChangedCallback((evt) =>
            {
                _from = (SpaceTransform.Space)evt.newValue;
                EvaluateBindings();
                GeneratePreviewForAffectedNodes();
            });
            extensionContainer.Add(dropdown);

            var to = new EnumField("To", _to);
            to.RegisterValueChangedCallback((evt) =>
            {
                _to = (SpaceTransform.Space)evt.newValue;
                EvaluateBindings();
                GeneratePreviewForAffectedNodes();
            });
            extensionContainer.Add(to);

            var type = new EnumField("Type", _type);
            type.RegisterValueChangedCallback((evt) =>
            {
                _type = (SpaceTransform.Type)evt.newValue;
                EvaluateBindings();
                GeneratePreviewForAffectedNodes();
            });
            extensionContainer.Add(type);
        }

        public override void AdditionalElements(VisualElement root)
        {
            var toggle = new Toggle("Normalize Direction")
            {
                value = _normalize
            };
            toggle.RegisterValueChangedCallback(x =>
            {
                _normalize = x.newValue;
                EvaluateBindings();
                GeneratePreviewForAffectedNodes();
            });
            root.Add(toggle);
        }

        void EvaluateBindings()
        {

        }

        protected override void Generate(NodeVisitor visitor)
        {
            string methodName = "Transform" + _from.ToString() + "To" + _to.ToString() + SpaceTransform.TypeTotring(_type);
            if (_from == _to)
            {
                Output(visitor, OUT, $"{PortData[IN].Name}");
            }
            else if (_type == SpaceTransform.Type.Direction || _type == SpaceTransform.Type.Normal)
            {
                Output(visitor, OUT, $"{methodName}({PortData[IN].Name}, {_normalize.ToString().ToLower()})");
            }
            else
            {
                Output(visitor, OUT, $"{methodName}({PortData[IN].Name})");
            }
        }
    }
}