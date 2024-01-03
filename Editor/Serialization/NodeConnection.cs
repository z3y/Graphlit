using System;
using System.Collections.Generic;
using UnityEditor.Experimental.GraphView;
using z3y.ShaderGraph.Nodes;

namespace z3y.ShaderGraph
{
    [Serializable]
    public struct NodeConnection
    {
        public int a;
        public int b;
        public string node;

        public NodeConnection(Edge edge)
        {
            a = edge.output.GetPortID();
            b = edge.input.GetPortID();
            node = ((ShaderNodeVisualElement)edge.output.node).viewDataKey;
        }

        public string GetInputNodeGuid()
        {
            return node;
        }

        public readonly int GetOutputIDForInputNode()
        {
            return a;
        }

        public readonly int GetInputIDForThisNode()
        {
            return b;
        }

        public static List<NodeConnection> GetConnections(ShaderNode node)
        {
            var connections = new List<NodeConnection>();
            foreach (var keyValue in node.Ports)
            {
                Port port = keyValue.Value;

                if (port.direction != Direction.Input)
                {
                    continue;
                }

                int id = keyValue.Key;

                foreach (var edge in port.connections)
                {
                    connections.Add(new NodeConnection(edge));
                    break; // only 1 connection allowed for input
                }
            }

            return connections;
        }
    }
}