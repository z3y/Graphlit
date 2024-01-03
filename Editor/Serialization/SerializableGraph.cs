using System;
using System.Collections.Generic;
using UnityEditor.Experimental.GraphView;
using z3y.ShaderGraph.Nodes;

namespace z3y.ShaderGraph
{
    [Serializable]
    public class SerializableGraph
    {
        public string shaderName = "New Shader";
        public List<SerializableNode> nodes =  new List<SerializableNode>();

        public static SerializableGraph FromGraphView(ShaderGraphView graphView)
        {
            var seriazableGraph = new SerializableGraph
            {
                shaderName = graphView.shaderName,
                nodes = new List<SerializableNode>()
            };

            var elements = graphView.graphElements;
            foreach (var node in elements)
            {
                if (node is ShaderNodeVisualElement shaderNodeVisualElement)
                {
                    var shaderNode = shaderNodeVisualElement.shaderNode;
                    if (shaderNode is null)
                    {
                        continue;
                    }
                    seriazableGraph.nodes.Add(new SerializableNode(shaderNode));
                }
            }

            return seriazableGraph;
        }

        public void Deserialize(ShaderGraphView graphView)
        {
            graphView.shaderName = shaderName;

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

                    if (graphNode is null)
                    {
                        continue;
                    }

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
}