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
        [SerializeField] private List<NodeConnection> _connections;
        public Vector2 GetSerializedPosition() => _position;
        public List<NodeConnection> GetSerializedConnections() => _connections;

        [NonSerialized] public Dictionary<int, string> varibleNames = new();
        private static int _uniqueVariableID = 0;
        public static void ResetUniqueVariableIDs() => _uniqueVariableID = 0;
        public string GetVariableName(int portID, string prefix = null)
        {
            if (varibleNames.TryGetValue(portID, out string value))
            {
                return value;
            }

            var varibleName = (prefix ?? "var") + _uniqueVariableID++;
            varibleNames.Add(portID, varibleName);
            return varibleName;
        }

        [NonSerialized] public Dictionary<int, object> portTypes = new();
        public PortType.DynamicFloat InheritDynamicFloatMax(int outID, int inputIDa, int inputIDb)
        {
            var typeA = (PortType.DynamicFloat)portTypes[inputIDa];
            var typeB = (PortType.DynamicFloat)portTypes[inputIDb];

            int components = Mathf.Max(typeA.components, typeB.components);
            var dynamicFloat = new PortType.DynamicFloat(components);
           // dynamicFloat.fullPrecision = typeA.fullPrecision || typeB.fullPrecision;
            portTypes[outID] = dynamicFloat;
            return dynamicFloat;
        }
        public PortType.DynamicFloat InheritDynamicFloat(int outID, int inID)
        {
            var inType = (PortType.DynamicFloat)portTypes[inID];
            int components = inType.components;
            var dynamicFloat = new PortType.DynamicFloat(components);
            portTypes[outID] = dynamicFloat;
          //  dynamicFloat.fullPrecision = inType.fullPrecision;
            return dynamicFloat;
        }

        internal void SetNodeVisualElement(ShaderNodeVisualElement node)
        {
            this.Node = node;
        }

        public void OnBeforeSerialize()
        {
            if (Node is null)
            {
                return;
            }

            var rect = Node.GetPosition();
            _position = new Vector2(rect.x, rect.y);
            _connections = new List<NodeConnection>();

            foreach (var ve in Node.inputContainer.Children())
            {
                if (!(ve is Port port && port.connected))
                {
                    continue;
                }

                int outID = (int)port.userData;
                
                foreach (var edge in port.connections)
                {
                    var inPort = edge.output;
                    int inID = (int)inPort.userData;
                    _connections.Add(new NodeConnection(outID, inID, ((ShaderNodeVisualElement)inPort.node).shaderNode));
                    break; // only 1 connection allowed for input
                }

            }
        }

        public void OnAfterDeserialize()
        {
        }

        public NodeInfo GetNodeInfo() => _nodeInfo ??= GetType().GetCustomAttribute<NodeInfo>();
        private NodeInfo _nodeInfo = null;

        public ShaderNodeVisualElement Node { get; private set; }


        private List<int> _addedPortIds = new List<int>();
        public Port AddInput(Type type, int id, string name = "")
        {
            if (_addedPortIds.Contains(id))
            {
                Debug.LogError($"Port {name} with ID:{id} already exists.");
                return null;
            }
            _addedPortIds.Add(id);
            var inPort = Node.InstantiatePort(Orientation.Horizontal, Direction.Input, Port.Capacity.Single, type);
            inPort.portName = name;
            inPort.userData = id;

            Node.inputContainer.Add(inPort);
            return inPort;
        }

        public int outputPortsCount { get; private set; }
        public Port AddOutput(Type type, int id, string name = "")
        {
            if (_addedPortIds.Contains(id))
            {
                Debug.LogError($"Port {name} with ID:{id} already exists.");
                return null;
            }
            _addedPortIds.Add(id);
            var outPort = Node.InstantiatePort(Orientation.Horizontal, Direction.Output, Port.Capacity.Multi, type);
            outPort.portName = name;
            outPort.userData = id;

            Node.outputContainer.Add(outPort);
            outputPortsCount++;
            return outPort;
        }

        // evaluate the output id for the node
        public List<int> visitedPorts = new List<int>();
        public virtual void Visit(System.Text.StringBuilder sb, int outID)
        {
        }
        public virtual void AddElements()
        {
        }
    }
}