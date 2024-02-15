using System;
using System.Linq;
using UnityEngine;
using UnityEngine.UIElements;
using ZSG.Nodes;
using ZSG.Nodes.PortType;

namespace ZSG
{
    [NodeInfo("Utility/Fetch Variable"), Serializable]
    public class FetchVariableNode : ShaderNode
    {
        [SerializeField] internal string _name = "";
        public override bool DisablePreview => true;
        const int IN = 0;
        const int OUT = 1;

        public override Color Accent => Color.red;

        public override void Initialize()
        {
            AddPort(new(PortDirection.Output, new Float(1, true), OUT));
            if (!string.IsNullOrEmpty(_name))
            {
                TitleLabel.text = _name;
            }
        }

        public override void AdditionalElements(VisualElement root)
        {
            var text = new TextField("Name") { value = _name };
            text.RegisterValueChangedCallback(x =>
            {
                _name = x.newValue;
                TitleLabel.text = x.newValue;
            });
            root.Add(text);

            var update = new Button() { text = "Update Preview" };
            update.clicked += GeneratePreviewForAffectedNodes;
            root.Add(update);
        }

        protected override void Generate(NodeVisitor visitor)
        {
            var input = Inputs.FirstOrDefault();
            if (input is not null && input.connected)
            {
                var node = (ShaderNode)input.connections.First().input.node;
                var data = node.GetInputPortData(IN, visitor);
                PortData[OUT] = data;
            }
            else
            {
                SetVariable(OUT, "0");
            }
        }
    }
}