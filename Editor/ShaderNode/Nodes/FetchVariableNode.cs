using System;
using System.Collections.Generic;
using System.Linq;
using UnityEditor.Experimental.GraphView;
using UnityEngine;
using UnityEngine.UIElements;
using Graphlit.Nodes;
using Graphlit.Nodes.PortType;

namespace Graphlit
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

        public override IEnumerable<Port> Inputs
        {
            get
            {
                var reg = GraphView.cachedRegisterVariablesForBuilder.FirstOrDefault(x => x._name == _name);
                
                if (reg is not null)
                {
                    return reg.Inputs;
                }
                return Enumerable.Empty<Port>();
            }
        }

        string VariableNotFoundText()
        {
            if (Inputs.Count() == 0)
            {
                return $" Variable \"{_name}\" not found";
            }

            return "";
        }

        public override void AdditionalElements(VisualElement root)
        {
            var text = new TextField("Name") { value = _name };
            GraphView.UpdateCachedNodesForBuilder();
            var variableFound = new Label(VariableNotFoundText());
            text.RegisterValueChangedCallback(x =>
            {
                _name = x.newValue;
                TitleLabel.text = x.newValue;

                var input = Inputs;
                variableFound.text = VariableNotFoundText();
            });
            root.Add(text);
            root.Add(variableFound);

            var update = new Button() { text = "Update Preview" };
            update.clicked += () => GeneratePreviewForAffectedNodes();
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