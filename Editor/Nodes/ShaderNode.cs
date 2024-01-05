using System;
using System.Collections.Generic;
using System.Reflection;
using UnityEngine;
using z3y.ShaderGraph.Nodes.PortType;

/*
namespace z3y.ShaderGraph.Nodes
{

    public abstract class ShaderNode
    {
        public void InitializeVisualElement(ShaderNodeVisualElement node)
        {
            Node = node;
            Initialize();
            AddVisualElements(Node);
        }

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
            UpdatePortDefaultString(portID);
            return TryGetVariableName(portID);
        }

        public string SetOutputString(int portID, string prefix = null)
        {
            return TryGetVariableName(portID, prefix);
        }

        public int ImplicitTruncation(int? outputID = null, params int[] IDs)
        {
            int trunc = 4;
            int max = 1;
            for (int i = 0; i < IDs.Length; i++)
            {
                var ID = IDs[i];
                var type = (Float)PortsTypes[ID];
                var components = type.components;
                if (components == 1)
                {
                    continue;
                }
                max = Mathf.Max(max, components);
                trunc = Mathf.Min(trunc, components);
            }
            trunc = Mathf.Min(trunc, max);

            if (outputID is int outputIDInt)
            {
                if (PortsTypes[outputIDInt] is Float @float)
                {
                    @float.components = trunc;
                    PortsTypes[outputIDInt] = @float;
                }
            }

            return trunc;
        }

        public string FormatOutput(int outID, string prefix, string text)
        {
            SetOutputString(outID, prefix);
            return $"{(Float)PortsTypes[outID]} {PortNames[outID]} = {text};";
        }

        public void UpdateGraphView()
        {
            // previews, port colors etc
            if (Node is null)
            {
                return;
            }

            foreach (var port in Ports)
            {
                if (_defaultPortsTypes[port.Key] is Float defaultFloatType && defaultFloatType.dynamic)
                {
                    var floatType = (Float)PortsTypes[port.Key];
                    var color = floatType.GetPortColor();
                    Ports[port.Key].portColor = color;

                    // caps not getting updated
                    var caps = Ports[port.Key].Q("connector");
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

        private void UpdatePortDefaultString(int portID)
        {
            *//*foreach (var connection in _connections)
            {
                if (connection.outID == portID)
                {
                    return; // connected
                }
            }

            PortsTypes[portID] = _defaultPortsTypes[portID];
            PortNames[portID] = SetDefaultInputString(portID);*//*
        }

      *//*  public bool IsConnected(int inputID)
        {
            foreach (var connection in _connections)
            {
                if (connection.outID == inputID)
                {
                    return true;
                }
            }

            return false;
        }*//*
        public int GetComponentCount(int portID)
        {
            return ((Float)PortsTypes[portID]).components;
        }

        *//* public PortType.DynamicFloat InheritDynamicFloat(int outID, int inID)
         {
             var inType = (PortType.DynamicFloat)portTypes[inID];
             int components = inType.components;
             var dynamicFloat = new PortType.DynamicFloat(components);
             portTypes[outID] = dynamicFloat;
           //  dynamicFloat.fullPrecision = inType.fullPrecision;
             return dynamicFloat;
         }*//*
        public string GetCastInputString(int portID, int targetComponent)
        {
            var name = GetInputString(portID);
            var type = (Float)PortsTypes[portID];
            var components = type.components;
            string typeName = type.fullPrecision ? "float" : "half";


            if (components == targetComponent)
            {
                return name;
            }

            // downcast
            if (components > targetComponent)
            {
                name = "(" + name + ").xyz"[..(targetComponent + 2)];
            }
            else
            {
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
            }

            type.components = targetComponent;
            PortsTypes[portID] = type;
            return name;
        }

        //public bool IsConnected(int id) => Ports[id].connected;
        public NodeInfo GetNodeInfo() => _nodeInfo ??= GetType().GetCustomAttribute<NodeInfo>();
        internal void SetNodeVisualElement(ShaderNodeVisualElement node)
        {
            this.Node = node;
        }

        private NodeInfo _nodeInfo = null;
        public ShaderNodeVisualElement Node { get; private set; }

        public Dictionary<int, IPortType> PortsTypes { get; private set; } = new();
        private Dictionary<int, IPortType> _defaultPortsTypes = new();
        public Dictionary<int, Port> Ports { get; private set; } = new();

        public enum Direction
        {
            Input,
            Output
        }

        public void AddPort(Direction direction, IPortType portType, int id, string name = "")
        {
            if (PortsTypes.ContainsKey(id))
            {
                Debug.LogError($"Port {name} with ID:{id} already exists.");
                return;
            }

            PortsTypes[id] = portType;
            _defaultPortsTypes[id] = portType;

            if (Node is null)
            {
                return;
            }

            var type = portType.GetType();
            var capacity = direction == Direction.Input ? Capacity.Single : Capacity.Multi;

            var port = Node.InstantiatePort(UnityEditor.Experimental.GraphView.Orientation.Horizontal, (UnityEditor.Experimental.GraphView.Direction)direction, capacity, type);
            port.AddManipulator(new EdgeConnector<Edge>(new EdgeConnectorListener()));
            port.portName = name;
            port.userData = id;
            if (portType is Float @float)
            {
                var color = @float.GetPortColor();
                port.portColor = color;
            }
            else
            {
                port.portColor = portType.GetPortColor();
            }
            Ports.Add(id, port);

            if (direction == Direction.Input) Node.inputContainer.Add(port);
            else Node.outputContainer.Add(port);
        }
        public abstract void Initialize();

        public virtual void AddVisualElements(ShaderNodeVisualElement node)
        {

        }

        public virtual string SetDefaultInputString(int portID)
        {
            return "0";
        }
        [NonSerialized] public bool visited = false;
        public void ResetAfterVisit()
        {
            visited = false;
            PortNames.Clear();
            foreach (var port in _defaultPortsTypes)
            {
                PortsTypes[port.Key] = _defaultPortsTypes[port.Key];
            }
        }

        [NonSerialized] public Dictionary<int, string> PortNames = new();

        public abstract void Visit(NodeVisitor sb);

        *//*public void Repaint()
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
        }*//*
    }

    internal class EdgeConnectorListener : IEdgeConnectorListener
    {
        public void OnDrop(GraphView graphView, Edge edge)
        {
            //throw new NotImplementedException();

            if (graphView is not ShaderGraphView shaderGraphView)
            {
                return;
            }
            //var sb = new StringBuilder();

            // temp
            *//*ShaderNode.ResetUniqueVariableIDs();
            graphView.graphElements.ForEach(e => {
                if (e is ShaderNodeVisualElement shaderNodeVe)
                {
                    shaderNodeVe.shaderNode.OnBeforeSerialize();
                    shaderNodeVe.shaderNode.ResetAfterVisit();
                }
            });

            graphView.graphElements.ForEach(e => {
                if (e is ShaderNodeVisualElement shaderNodeVe)
                {
                    ShaderGraphImporter.VisitConenctedNode(sb, shaderNodeVe.shaderNode);
                }
            });

            Debug.Log(sb);*//*
        }


        public void OnDropOutsidePort(Edge edge, Vector2 position)
        {
        }
    }
}*/

namespace z3y.ShaderGraph.Nodes
{
    public enum PortDirection
    {
        Input,
        Output
    }

    public struct PortDescriptor
    {
        public PortDescriptor(PortDirection direction, IPortType type, int id, string name = "")
        {
            Direction = direction;
            Type = type;
            ID = id;
            Name = name;
        }

        public PortDirection Direction { get; }
        public IPortType Type { get; set; }
        public int ID { get; }
        public string Name { get; }
    }

    interface IRequirePropertyVisitor
    {
        void VisitProperty(PropertyVisitor visitor);
    }

    interface IRequireDescriptionVisitor
    {
        void VisitDescription(DescriptionVisitor visitor);
    }
    interface IMayRequirePropertyVisitor : IRequirePropertyVisitor
    {
        bool IsProperty { get; set; }
    }
    interface IRequireFunctionVisitor
    {
        void VisitFunction(FunctionVisitor visitor);
    }
    public abstract class ShaderNode
    {
        public ShaderNode()
        {
            foreach (var port in Ports)
            {
                DefaultPortsTypes.Add(port.ID, port.Type);
            }
        }

        public NodeInfo Info => GetType().GetCustomAttribute<NodeInfo>();
        public virtual void AddElements(ShaderNodeVisualElement node) { }
        public abstract PortDescriptor[] Ports { get; }
        public bool InputConnected(int portID) => Inputs.ContainsKey(portID);

        #region Visit
        public Dictionary<int, NodeConnection> Inputs { get; set; } = new();
        public Dictionary<int, string> VariableNames { get; set; } = new();
        public bool visited;
        private static int _uniqueVariableID = 0;
        public static void ResetUniqueVariableIDs() => _uniqueVariableID = 0;
        private string TryGetVariableName(int portID, string prefix = null)
        {
            if (VariableNames.TryGetValue(portID, out string value))
            {
                return value;
            }

            var name = (prefix ?? "Node") + _uniqueVariableID++;
            VariableNames.Add(portID, name);
            return name;
        }

        public string GetInputString(int portID)
        {
            UpdatePortDefaultString(portID);
            return TryGetVariableName(portID);
        }

        public Dictionary<int, IPortType> DefaultPortsTypes { get; } = new();
        private void UpdatePortDefaultString(int portID)
        {
            if (InputConnected(portID))
            {
                return;
            }

            Ports[portID].Type = DefaultPortsTypes[portID];
            VariableNames[portID] = SetDefaultInputString(portID);
        }

        public virtual string SetDefaultInputString(int portID)
        {
            return "0";
        }

        public string SetOutputString(int portID, string prefix = null)
        {
            return TryGetVariableName(portID, prefix);
        }

        public int ImplicitTruncation(int? outputID = null, params int[] IDs)
        {
            int trunc = 4;
            int max = 1;
            for (int i = 0; i < IDs.Length; i++)
            {
                var ID = IDs[i];
                var type = (Float)Ports[ID].Type;
                var components = type.components;
                if (components == 1)
                {
                    continue;
                }
                max = Mathf.Max(max, components);
                trunc = Mathf.Min(trunc, components);
            }
            trunc = Mathf.Min(trunc, max);

            if (outputID is int outputIDInt)
            {
                if (Ports[outputIDInt].Type is Float @float)
                {
                    @float.components = trunc;
                    Ports[outputIDInt].Type = @float;
                }
            }

            return trunc;
        }

        public string GetCastInputString(int portID, int targetComponent)
        {
            var name = GetInputString(portID);
            var type = (Float)Ports[portID].Type;
            var components = type.components;
            string typeName = type.fullPrecision ? "float" : "half";


            if (components == targetComponent)
            {
                return name;
            }

            // downcast
            if (components > targetComponent)
            {
                name = "(" + name + ").xyz"[..(targetComponent + 2)];
            }
            else
            {
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
            }

            type.components = targetComponent;
            Ports[portID].Type = type;
            return name;
        }

        public string FormatOutput(int outID, string prefix, string text)
        {
            SetOutputString(outID, prefix);
            return $"{(Float)Ports[outID].Type} {VariableNames[outID]} = {text};";
        }
        #endregion
    }
}