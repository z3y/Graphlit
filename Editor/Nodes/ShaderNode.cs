using System;
using System.Collections.Generic;
using System.Reflection;
using UnityEditor.Experimental.GraphView;
using UnityEditor.MemoryProfiler;
using UnityEngine;
using UnityEngine.UIElements;

namespace z3y.ShaderGraph.Nodes
{
    public class ShaderNodeVisualElement : Node
    {
        public ShaderNode shaderNode;

        public void Initialize(Type type, Vector2 position)
        {
            shaderNode = (ShaderNode)Activator.CreateInstance(type);
            shaderNode.Initialize(this);

            SetNodePosition(position);
            AddDefaultElements();
        }

        public void AddAlreadyInitialized(ShaderNode shaderNode)
        {
            this.shaderNode = shaderNode;
            shaderNode.Initialize(this);

            SetNodePosition(shaderNode.GetSerializedPosition());
            AddDefaultElements();
        }
        private void AddDefaultElements()
        {
            AddStyles();
            AddTitleElement();
            shaderNode.AddElements();
            RefreshExpandedState();
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
            var nodeInfo = shaderNode.GetNodeInfo();

            var titleLabel = new Label { text = nodeInfo.name, tooltip = nodeInfo.tooltip };
            titleLabel.style.fontSize = 13;
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
        private void SetNodePosition(Vector2 position)
        {
            SetPosition(new Rect(position, Vector3.one));
        }
    }

    [System.Serializable]
    [@NodeInfo("Default Title")]
    public class ShaderNode : ISerializationCallbackReceiver
    {
        public void Initialize(ShaderNodeVisualElement node)
        {
            Node = node;
        }

        [SerializeField] private Vector2 _position;
        [SerializeField] private List<Connection> _connections;
        public Vector2 GetSerializedPosition() => _position;

        internal void SetNodeVisualElement(ShaderNodeVisualElement node)
        {
            this.Node = node;
        }

        public void OnBeforeSerialize()
        {
            var rect = Node.GetPosition();
            _position = new Vector2(rect.x, rect.y);
            _connections = new List<Connection>();

            foreach (var ve in Node.outputContainer.Children())
            {
                if (!(ve is Port port && port.connected))
                {
                    continue;
                }

                var connectionPorts = new List<ConnectionPorts>();
                foreach (var edge in port.connections)
                {
                    var connectedToPort = edge.input;
                    connectionPorts.Add(new ConnectionPorts(((ShaderNodeVisualElement)connectedToPort.node).shaderNode, (int)connectedToPort.userData));
                }

                _connections.Add(new Connection((int)port.userData, connectionPorts));
            }
        }

        public void OnAfterDeserialize()
        {
        }

        public NodeInfo GetNodeInfo() => _nodeInfo ??= GetType().GetCustomAttribute<NodeInfo>();
        private NodeInfo _nodeInfo = null;

        public ShaderNodeVisualElement Node { get; private set; }

        public virtual void AddElements()
        {
            var inputPort = Node.InstantiatePort(Orientation.Horizontal, Direction.Input, Port.Capacity.Single, typeof(bool));
            inputPort.portName = "Input Value";
            Node.inputContainer.Add(inputPort);

            var customDataContainer = new VisualElement();
            var textFoldout = new Foldout { text = "cool" };
            textFoldout.Add(new TextField { value = "sometitle xd" });
            customDataContainer.Add(textFoldout);

            customDataContainer.AddToClassList("sg-node__extension-container");

            Node.extensionContainer.Add(customDataContainer);
        }

        public Port AddInput(Type type, int id, string name = "")
        {
            var inPort = Node.InstantiatePort(Orientation.Horizontal, Direction.Input, Port.Capacity.Single, typeof(float));
            inPort.portName = name;
            inPort.userData = id;

            Node.inputContainer.Add(inPort);
            return inPort;
        }

        public Port AddOutput(Type type, int id, string name = "")
        {
            var outPort = Node.InstantiatePort(Orientation.Horizontal, Direction.Output, Port.Capacity.Multi, typeof(float));
            outPort.portName = name;
            outPort.userData = id;

            Node.outputContainer.Add(outPort);
            return outPort;
        }


        public virtual void Visit(Port[] ports)
        {

        }

    }
}