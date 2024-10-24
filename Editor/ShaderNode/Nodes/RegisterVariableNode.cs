using System.Collections.Generic;
using System.Linq;
using UnityEditor.Experimental.GraphView;
using UnityEngine;
using UnityEngine.UIElements;
using Graphlit.Nodes;
using Graphlit.Nodes.PortType;

namespace Graphlit
{
    [NodeInfo("Utility/Register Variable"), System.Serializable]
    public class RegisterVariableNode : ShaderNode
    {
        [SerializeField] internal string _name = "";
        //[SerializeField] internal bool _autoWire = false;

        public override bool DisablePreview => true;
        const int IN = 0;
        const int OUT = 1;

        public override Color Accent => Color.red;

        public override void Initialize()
        {
            AddPort(new(PortDirection.Input, new Float(1, true), IN));
            if (!string.IsNullOrEmpty(_name))
            {
                TitleLabel.text = _name;
            }
        }

        FetchVariableNode _fetch = null;
        public override IEnumerable<Port> Outputs
        {
            get
            {
                if (_fetch is null)
                {
                    _fetch = GraphView.cachedNodesForBuilder.OfType<FetchVariableNode>().FirstOrDefault(x => x._name == _name);
                }
                if (_fetch is not null)
                {
                    return _fetch.Outputs;
                }
                return Enumerable.Empty<Port>();
            }
        }

        public override void AdditionalElements(VisualElement root)
        {
            var text = new TextField("Name") { value = _name };
            text.RegisterValueChangedCallback(x =>
            {
                _name = x.newValue;
                TitleLabel.text = x.newValue;
                _fetch = null;
            });
            root.Add(text);

            /*var auto = new Toggle("Auto Wire") { value = _autoWire };
            auto.RegisterValueChangedCallback(x => _autoWire = x.newValue);
            root.Add(auto);*/

            var update = new Button() { text = "Update Preview" };
            update.clicked += GeneratePreviewForAffectedNodes;
            //update.clicked += GeneratePreviewForAutoWiredNodes;

            root.Add(update);
        }

        /*void GeneratePreviewForAutoWiredNodes()
        {
            var nodes = GraphView.graphElements.OfType<ShaderNode>();
            foreach (var node in nodes)
            {
                if (node is RegisterVariableNode)
                {
                    continue;
                }

                ShaderBuilder.GeneratePreview(GraphView, this);
                node.GeneratePreviewForAffectedNodes();
            }
        }*/

        protected override void Generate(NodeVisitor visitor)
        {
        }
    }
}