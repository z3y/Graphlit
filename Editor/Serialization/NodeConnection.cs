using System;
using System.Collections.Generic;
using UnityEditor.Experimental.GraphView;

namespace Enlit
{
    [Serializable]
    public struct NodeConnection
    {
        public int a;
        public int b;
        public string node;

        public ShaderNode Node { get; private set; }
        public NodeConnection(Edge edge)
        {
            a = edge.output.GetPortID();
            b = edge.input.GetPortID();

            node = ((ShaderNode)edge.output.node).viewDataKey;

            Node = null;
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

        public static List<NodeConnection> GetConnections(IEnumerable<Port> ports)
        {
            var connections = new List<NodeConnection>();


            foreach (var port in ports)
            {
                if (port.direction != Direction.Input)
                {
                    continue;
                }

                foreach (var edge in port.connections)
                {
                    connections.Add(new NodeConnection(edge));
                    break; // only 1 connection allowed for input
                }
            }

            return connections;
        }

        public void MapToNode(ShaderNode shaderNode)
        {
            Node = shaderNode;
        }
    }
}