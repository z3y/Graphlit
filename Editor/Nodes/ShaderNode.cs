using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Xml.Linq;
using UnityEngine;
using z3y.ShaderGraph.Nodes.PortType;

namespace z3y.ShaderGraph.Nodes
{
    public enum PortDirection
    {
        Input,
        Output
    }

    public class PortDescriptor
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

    public static class PortDescriptorExtensions
    {
        public static PortDescriptor GetByID(this List<PortDescriptor> portDescriptors, int ID)
        {
            return portDescriptors.Where(x => x.ID == ID).First();
        }

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
        public abstract List<PortDescriptor> Ports { get; }
        public bool InputConnected(int portID) => Inputs.ContainsKey(portID);

        #region Visit
        public Dictionary<int, NodeConnection> Inputs { get; set; } = new();
        public Dictionary<int, string> VariableNames { get; set; } = new();
        [NonSerialized] public bool visited;
        [NonSerialized] private static int _uniqueVariableID = 0;
        public static void ResetUniqueVariableIDs() => _uniqueVariableID = 0;
        private string TryGetVariableName(int portID, string prefix = null)
        {
            if (VariableNames.TryGetValue(portID, out string value))
            {
                return value;
            }

            var name = (prefix ?? "Node") + _uniqueVariableID++;
            VariableNames[portID] = name;
            return name;
        }

        public string GetInputString(int portID)
        {
            UpdatePortDefaultString(portID);
            if (DefaultPortsTypes[portID] is Float @float && !@float.dynamic)
            {
                return GetCastInputString(portID, @float.components);
            }
            return TryGetVariableName(portID);
        }

        public Dictionary<int, IPortType> DefaultPortsTypes { get; } = new();
        private void UpdatePortDefaultString(int portID)
        {
            if (InputConnected(portID))
            {
                return;
            }

            Ports.GetByID(portID).Type = DefaultPortsTypes[portID];
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
                var type = (Float)Ports.GetByID(ID).Type;
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
                var port = Ports.GetByID(outputIDInt);
                if (port.Type is Float @float)
                {
                    @float.components = trunc;
                    port.Type = @float;
                }
            }

            return trunc;
        }

        public string Cast(int components, string typeName, int targetComponent, string name)
        {
            if (components == targetComponent)
            {
                return name;
            }

            // downcast
            if (components > targetComponent)
            {
                //name = $"(" + name + ").xyz"[..(targetComponent + 2)];
                name = name + ".xyz"[..(targetComponent + 1)];
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

            return name;
        }

        public string GetCastInputString(int portID, int targetComponent, bool updateDefault = true)
        {
            if (updateDefault) UpdatePortDefaultString(portID);

            var name = TryGetVariableName(portID);
            var type = (Float)Ports.GetByID(portID).Type;
            var components = type.components;
            string typeName = type.fullPrecision ? "float" : "half";

            if (components == targetComponent)
            {
                return name;
            }

            // downcast
            if (components > targetComponent)
            {
                //name = $"(" + name + ").xyz"[..(targetComponent + 2)];
                name = name + ".xyz"[..(targetComponent + 1)];
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
            Ports.GetByID(portID).Type = type;
            VariableNames[portID] = name;
            return name;
        }

        public string FormatOutput(int outID, string prefix, string text)
        {
            SetOutputString(outID, prefix);
            return $"{(Float)Ports.GetByID(outID).Type} {VariableNames[outID]} = {text};";
        }

        public void ResetVisit()
        {
            visited = false;
            VariableNames.Clear();
            Ports.Select(x => x.Type = DefaultPortsTypes[x.ID]);
        }

        public abstract void Visit(NodeVisitor visitor);
        #endregion
    }
}

/*
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
        shaderNode.ResetUniqueVariableIDs();
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

        Debug.Log(sb);
    }


    public void OnDropOutsidePort(Edge edge, Vector2 position)
    {
    }
}
*/