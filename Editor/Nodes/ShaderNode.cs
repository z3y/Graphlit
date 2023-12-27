using System;
using System.Collections.Generic;
using System.Reflection;
using UnityEditor.Experimental.GraphView;
using UnityEngine;
using UnityEngine.UIElements;
using static UnityEditor.Experimental.GraphView.Port;
using static z3y.ShaderGraph.Nodes.PortType;

namespace z3y.ShaderGraph.Nodes
{
    public class ShaderNodeVisualElement : Node
    {
        public ShaderNode shaderNode;

        public void Initialize(Type type, Vector2 position)
        {
            shaderNode = (ShaderNode)Activator.CreateInstance(type);
            shaderNode.InitializeVisualElement(this);

            SetNodePosition(position);
            AddDefaultElements();
        }

        public void AddAlreadyInitialized(ShaderNode shaderNode)
        {
            this.shaderNode = shaderNode;
            shaderNode.InitializeVisualElement(this);

            SetNodePosition(shaderNode.GetSerializedPosition());
            AddDefaultElements();
        }
        private void AddDefaultElements()
        {
            AddStyles();
            AddTitleElement();
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
        public void InitializeVisualElement(ShaderNodeVisualElement node)
        {
            Node = node;
            Initialize();
        }

        [SerializeField] private Vector2 _position;
        [SerializeField] private List<NodeConnection> _connections;
        public Vector2 GetSerializedPosition() => _position;
        public List<NodeConnection> GetSerializedConnections() => _connections;


        private static int _uniqueVariableID = 0;
        public static void ResetUniqueVariableIDs() => _uniqueVariableID = 0;
        private string TryGetVariableName(int portID, string prefix = null)
        {
            if (PortNames.TryGetValue(portID, out string value))
            {
                return value;
            }

            var name = (prefix ?? "Node") + _uniqueVariableID++;
            PortNames.Add(portID, name);
            return name;
        }
        public string GetInputString(int portID)
        {
            /*if (PortsTypes[portID] is Float @float && !@float.dynamic)
            {
                TryGetVariableName(portID);
                return GetCastInputString(portID, @float.components);
                //return TryGetVariableName(portID);
            }
            else
            {*/
                UpdatePortComponentCount(portID);
                return TryGetVariableName(portID);
            //}
        }
        public string SetOutputString(int portID, string prefix = null)
        {
            return TryGetVariableName(portID, prefix);
        }
        public Float InheritFloatComponentsMax(int outID, int inIDa, int inIDb)
        {
            var typeA = (Float)PortsTypes[inIDa];
            var typeB = (Float)PortsTypes[inIDb];

            int components = Mathf.Max(typeA.components, typeB.components);
            var dynamicFloat = new Float(components);
            PortsTypes[outID] = dynamicFloat;

            var color = GetComponentColor(components);
            Ports[outID].portColor = color;

            return dynamicFloat;
        }

        public void AppendOutputLine(int portID, System.Text.StringBuilder sb, string text)
        {
            sb.AppendLine($"{(Float)PortsTypes[portID]} {PortNames[portID]} = {text};");
        }

        private void UpdatePortComponentCount(int portID)
        {
            if (!Ports[portID].connected)
            {
                var defaultType = PortsTypes[portID] = _defaultPortsTypes[portID];
                PortNames[portID] = SetDefaultInputString(portID);
                if (defaultType is Float floatType)
                {
                    var color = GetComponentColor(floatType.components);
                    Ports[portID].portColor = color;
                }
            }
        }

        /* public PortType.DynamicFloat InheritDynamicFloat(int outID, int inID)
         {
             var inType = (PortType.DynamicFloat)portTypes[inID];
             int components = inType.components;
             var dynamicFloat = new PortType.DynamicFloat(components);
             portTypes[outID] = dynamicFloat;
           //  dynamicFloat.fullPrecision = inType.fullPrecision;
             return dynamicFloat;
         }*/
        public string GetCastInputString(int portID, int targetComponent)
        {
            var name = GetInputString(portID);
            var type = (Float)PortsTypes[portID];
            var components = type.components;
            string typeName = type.fullPrecision ? "float" : "half";

            var color = GetComponentColor(targetComponent);
            Ports[portID].portColor = color;

            if (components == targetComponent)
            {
                return name;
            }

            // downcast
            if (components > targetComponent)
            {
                name = "(" + name + ").xyz"[..(targetComponent + 2)];
                return name;
            }

            // upcast
            if (components == 1)
            {
                // no need to upcast
                // name = "(" + name + ").xxxx"[..(targetComponent + 2)];
                return name;
            }
            else if (components == 2)
            {
                if (targetComponent == 3)
                {
                    name = typeName + "3(" + name + ", 0)";
                }
                if (targetComponent == 4)
                {
                    name = typeName + "4(" + name + ", 0, 0)";
                }
            }
            else if (components == 3)
            {
                if (targetComponent == 4)
                {
                    name = typeName + "4(" + name + ", 0)";
                }
            }
            return name;
        }

        public bool IsConnected(int id) => Ports[id].connected;
        public NodeInfo GetNodeInfo() => _nodeInfo ??= GetType().GetCustomAttribute<NodeInfo>();
        internal void SetNodeVisualElement(ShaderNodeVisualElement node)
        {
            this.Node = node;
        }

        private NodeInfo _nodeInfo = null;
        public ShaderNodeVisualElement Node { get; private set; }

        public void OnBeforeSerialize()
        {
            if (Node is null)
            {
                return;
            }

            var rect = Node.GetPosition();
            _position = new Vector2(rect.x, rect.y);
            _connections = new List<NodeConnection>();

            foreach (var keyValue in Ports)
            {
                Port port = keyValue.Value;

                if (port.direction != Direction.Input)
                {
                    continue;
                }

                int id = keyValue.Key;

                foreach (var edge in port.connections)
                {
                    var inPort = edge.output;
                    int inID = (int)inPort.userData;
                    _connections.Add(new NodeConnection(id, inID, ((ShaderNodeVisualElement)inPort.node).shaderNode));
                    break; // only 1 connection allowed for input
                }
            }
        }

        public void OnAfterDeserialize()
        {
        }

        public Dictionary<int, IPortType> PortsTypes { get; private set; } = new();
        private Dictionary<int, IPortType> _defaultPortsTypes = new();
        public Dictionary<int, Port> Ports { get; private set; } = new();

        public void AddPort(Direction direction, IPortType portType, int id, string name = "")
        {
            if (PortsTypes.ContainsKey(id))
            {
                Debug.LogError($"Port {name} with ID:{id} already exists.");
                return;
            }

            PortsTypes.Add(id, portType);
            _defaultPortsTypes.Add(id, portType);

            if (Node is null)
            {
                return;
            }

            var type = portType.GetType();
            var capacity = direction == Direction.Input ? Capacity.Single : Capacity.Multi;
            var port = Node.InstantiatePort(Orientation.Horizontal, direction, capacity, type);
            port.portName = name;
            port.userData = id;
            if (portType is Float @float)
            {
                var color = GetComponentColor(@float.components);
                port.portColor = color;
            }
            else
            {
                port.portColor = GetPortColor(type);
            }
            Ports.Add(id, port);

            if (direction == Direction.Input) Node.inputContainer.Add(port);
            else Node.outputContainer.Add(port);
        }
        public virtual void Initialize()
        {
        }

        public virtual void AddVisualElements()
        {
        }

        public virtual string SetDefaultInputString(int portID)
        {
            return "0";
        }
        public void Reset()
        {
            visitedPorts = new();
            PortNames = new();
            foreach (var port in Ports)
            {
                UpdatePortComponentCount(port.Key);
            }
        }

        [NonSerialized] public List<int> visitedPorts = new();
        [NonSerialized] public Dictionary<int, string> PortNames = new();

        public virtual void Visit(System.Text.StringBuilder sb, int outID)
        {
        }

        public void Repaint()
        {
            if (Node is null)
            {
                return;
            }
            Node.MarkDirtyRepaint();
            foreach (var port in Ports)
            {
                foreach (var child in port.Value.connections)
                {
                    child.MarkDirtyRepaint();
                }
            }
        }
    }
}