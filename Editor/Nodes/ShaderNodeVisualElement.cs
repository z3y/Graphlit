using System;
using UnityEngine.UIElements;
using UnityEngine;
using UnityEditor.Experimental.GraphView;
using System.Collections.Generic;
using static UnityEditor.Experimental.GraphView.Port;
using z3y.ShaderGraph.Nodes.PortType;
using System.Linq;
using UnityEditor;

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
            SetPosition(serializedNode.Position);
            AddDefaultElements();
        }

        private void AddDefaultElements()
        {
            shaderNode.GUID = viewDataKey;
            AddPortElements();
            shaderNode.AddElements(this);
            AddStyles();
            AddTitleElement();
            AddPreview();

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

            var titleLabel = new Label { text = nodeInfo.name, tooltip = nodeInfo.tooltip + "\n" + viewDataKey };
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

        //internal List<Material> _previewMaterials = new List<Material>();

        public Action<Material> UpdateMaterial = (mat) => { };

        public void UpdatePreview()
        {
            foreach (var material in PreviewDrawer.materials)
            {
                if (material is null) continue;
                UpdateMaterial(material);
            }
        }

        public void UpdateGraphView(ShaderNode shaderNode)
        {
            foreach (var port in Ports)
            {
                int portID = port.GetPortID();
                if (shaderNode.DefaultPortsTypes[portID] is Float defaultFloatType && defaultFloatType.dynamic)
                {
                    var floatType = (Float)shaderNode.Ports[portID].Type;
                    var color = floatType.GetPortColor();
                    port.portColor = color;

                    // caps not getting updated
                    var caps = port.Q("connector");
                    if (caps is not null)
                    {
                        caps.style.borderBottomColor = color;
                        caps.style.borderTopColor = color;
                        caps.style.borderLeftColor = color;
                        caps.style.borderRightColor = color;
                    }
                }
            }
        }

        public PreviewDrawer previewDrawer = new PreviewDrawer();
        private void AddPreview()
        {
            extensionContainer.Add(previewDrawer.GetVisualElement());
        }

    }
}