using System;
using System.Collections.Generic;
using UnityEditor.Experimental.GraphView;
using UnityEngine;
using z3y.ShaderGraph.Nodes;

namespace z3y.ShaderGraph
{
    [Serializable]
    public class SerializableGraph
    {
        public string shaderName = string.Empty;
        public List<SerializableNode> nodes =  new List<SerializableNode>();

        public static SerializableGraph FromGraphView(ShaderGraphView graphView)
        {
            var seriazableGraph = new SerializableGraph();
            seriazableGraph.shaderName = "uhhh";
            seriazableGraph.nodes = new List<SerializableNode>();

            var elements = graphView.graphElements;
            foreach (var node in elements)
            {
                if (node is ShaderNodeVisualElement shaderNodeVisualElement)
                {
                    var shaderNode = shaderNodeVisualElement.shaderNode;
                    seriazableGraph.nodes.Add(new SerializableNode(shaderNode));
                }
            }

            return seriazableGraph;
        }


        public void Deserialize(ShaderGraphView graphView)
        {
            foreach (var node in nodes)
            {
                graphView.AddNode(node);
            }

            foreach (var node in nodes)
            {
                foreach (var connection in node.connections)
                {
                    var graphNode = graphView.GetNodeByGuid(node.guid);
                    var inID = connection.GetPortIDForInputNode();
                    var outID = connection.GetPortIDForThisNode();
                    var inNode = graphView.GetNodeByGuid(connection.GetInputNodeGuid());

                    foreach (var ve in graphNode.inputContainer.Children())
                    {
                        if (ve is not Port port)
                        {
                            continue;
                        }

                        if (port.userData == null || port.GetPortID() != outID)
                        {
                            continue;
                        }

                        if (inNode is null)
                        {
                            continue;
                        }

                        foreach (var ve2 in inNode.outputContainer.Children())
                        {
                            if (ve2 is not Port outPort)
                            {
                                continue;
                            }

                            if (outPort.GetPortID() == inID)
                            {
                                var newEdge = outPort.ConnectTo(port);
                                graphView.AddElement(newEdge);
                                break;
                            }
                        }
                    }
                }
            }
        }
    }

    [Serializable]
    public struct SerializableNode
    {
        public string type;
        public string guid;
        public Vector2 position;
        public List<NodeConnection> connections;
        public string data;

        public SerializableNode(ShaderNode node)
        {
            var type = node.GetType();

            this.type = type.FullName;
            this.guid = node.Node.viewDataKey;
            this.position = node.Node.GetPosition().position;
            this.connections = NodeConnection.GetConnections(node);

            var seriazableAttribute = Attribute.GetCustomAttribute(type, typeof(SerializableAttribute));
            if (seriazableAttribute is not null)
            {
                data = JsonUtility.ToJson(node);
            }
            else
            {
                data = string.Empty;
            }
        }

        public readonly bool TryDeserialize(out ShaderNode shaderNode)
        {
            Type type = Type.GetType(this.type);
            var instance = Activator.CreateInstance(type);
            if (instance is null)
            {
                Debug.LogError($"Node of type {this.type} not found");
                shaderNode = null;
                return false;
            }

            if (!string.IsNullOrEmpty(data))
            {
                JsonUtility.FromJsonOverwrite(data, instance);
            }

            shaderNode = (ShaderNode)instance;

            return true;
        }
    }
}