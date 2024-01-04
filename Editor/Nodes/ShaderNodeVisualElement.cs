using System;
using UnityEngine.UIElements;
using UnityEngine;
using UnityEditor.Experimental.GraphView;
using System.Collections.Generic;
using static UnityEditor.Experimental.GraphView.Port;
using z3y.ShaderGraph.Nodes.PortType;
using System.Linq;

namespace z3y.ShaderGraph.Nodes
{
    public class ShaderNodeVisualElement : Node
    {
        public ShaderNode shaderNode;

        public void Create(Type type, Vector2 position)
        {
            shaderNode = (ShaderNode)Activator.CreateInstance(type);
            SetPosition(position);

            AddDefaultElements();
        }

        public void Add(SerializableNode serializedNode)
        {
            if (!serializedNode.TryDeserialize(out var shaderNode))
            {
                return;
            }

            this.viewDataKey = serializedNode.guid;
            this.shaderNode = shaderNode;
            SetPosition(serializedNode.position);
            AddDefaultElements();
        }

        private void AddDefaultElements()
        {
            AddPortElements();
            shaderNode.AddElements(this);
            AddStyles();
            AddTitleElement();
            RefreshExpandedState();
            RefreshPorts();
        }

        private void AddPortElements()
        {
            foreach (var portDescriptor in shaderNode.Ports)
            {
                var container = portDescriptor.Direction == PortDirection.Input ? inputContainer : outputContainer;

                var type = portDescriptor.Type.GetType();
                var capacity = portDescriptor.Direction == PortDirection.Input ? Capacity.Single : Capacity.Multi;

                var port = InstantiatePort(Orientation.Horizontal, (Direction)portDescriptor.Direction, capacity, type);
                //portElement.AddManipulator(new EdgeConnector<Edge>(new EdgeConnectorListener()));
                port.portName = portDescriptor.Name;
                port.userData = portDescriptor.ID;
                if (portDescriptor.Type is Float @float)
                {
                    var color = @float.GetPortColor();
                    port.portColor = color;
                }
                else
                {
                    port.portColor = portDescriptor.Type.GetPortColor();
                }

                container.Add(port);
            }
        }
        private void AddStyles()
        {
            extensionContainer.AddToClassList("sg-node__extension-container");
            titleContainer.AddToClassList("sg-node__title-container");
            inputContainer.AddToClassList("sg-node__input-container");
            outputContainer.AddToClassList("sg-node__output-container");
        }
        private void AddTitleElement()
        {
            var nodeInfo = shaderNode.Info;

            var titleLabel = new Label { text = nodeInfo.name, tooltip = nodeInfo.tooltip };
            titleLabel.style.fontSize = 14;
            var centerAlign = new StyleEnum<Align> { value = Align.Center };
            titleLabel.style.alignSelf = centerAlign;
            titleLabel.style.alignItems = centerAlign;
            titleContainer.Insert(0, titleLabel);

            /*var noRadius = new StyleLength { value = 0 };
            var borderStyle = this.ElementAt(0).style;
            var borderSelectionStyle = this.ElementAt(1).style;

            borderStyle.borderBottomLeftRadius = noRadius;
            borderStyle.borderBottomRightRadius = noRadius;
            borderStyle.borderTopLeftRadius = noRadius;
            borderStyle.borderTopRightRadius = noRadius;

            borderSelectionStyle.borderBottomLeftRadius = noRadius;
            borderSelectionStyle.borderBottomRightRadius = noRadius;
            borderSelectionStyle.borderTopLeftRadius = noRadius;
            borderSelectionStyle.borderTopRightRadius = noRadius;*/
        }

        private void SetPosition(Vector2 position)
        {
            base.SetPosition(new Rect(position, Vector3.one));
        }

        public IEnumerable<Port> Ports
        {
            get
            {
                return inputContainer.Children().Concat(outputContainer.Children())
                    .Where(x => x is Port).Cast<Port>();
            }
        }

        private List<Material> _previewMaterials = new List<Material>();

        public void UpdatePreview(Action<Material> func)
        {
            foreach (var material in _previewMaterials)
            {
                func(material);
            }
        }

    }
}