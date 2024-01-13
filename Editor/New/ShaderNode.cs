using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using UnityEditor.Experimental.GraphView;
using UnityEngine;
using UnityEngine.UIElements;
using ZSG.Nodes;
using ZSG.Nodes.PortType;
using static UnityEditor.Experimental.GraphView.Port;

namespace ZSG
{
    public abstract class ShaderNode : Node
    {
        public void Initialize(Vector2 position, string guid = null)
        {
            base.SetPosition(new Rect(position, Vector3.one));
            if (guid is not null) viewDataKey = guid;

            AddDefaultElements();
        }

        public NodeInfo Info => GetType().GetCustomAttribute<NodeInfo>();

        public IEnumerable<Port> PortElements => inputContainer.Children().Concat(outputContainer.Children())
            .Where(x => x is Port)
            .Cast<Port>();

        public abstract void Generate(NodeVisitor visitor);

        internal List<PortDescriptor> _portDescriptors = new();
        public void AddPort(PortDescriptor portDescriptor)
        {
            _portDescriptors.Add(portDescriptor);

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

        public void RemovePort(int id)
        {
            int i = _portDescriptors.FindIndex(x => x.ID == id);
            if (i < 0)
            { 
                return;
            }
            _portDescriptors.RemoveAt(i);
        }

        public abstract void AddElements();

        [NonSerialized] public bool enablePreview = true;

        private void AddDefaultElements()
        {
            AddStyles();
            AddTitleElement();
            AddElements();
            // AddPreview();


            RefreshExpandedState();
            RefreshPorts();
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
            var nodeInfo = Info;

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
    }

    [NodeInfo("*", "a * b"), Serializable]
    public class MultiplyNode : ShaderNode
    {
        const int A = 0;
        const int B = 1;
        const int OUT = 2;

        public override void AddElements()
        {
            AddPort(new(PortDirection.Input, new Float(1, true), A, "A"));
            AddPort(new(PortDirection.Input, new Float(1, true), B, "B"));
            AddPort(new(PortDirection.Output, new Float(1, true), OUT));
        }

        public override void Generate(NodeVisitor visitor)
        {
            //var components = ImplicitTruncation(OUT, A, B);
            //var a = GetCastInputString(A, components);
            //var b = GetCastInputString(B, components);

            //visitor.AppendLine(FormatOutput(OUT, "Multiply", $"{a} * {b}"));
            visitor.AppendLine("a");
        }
    }
}
