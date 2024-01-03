using System;
using System.Collections.Generic;
using System.Linq;
using UnityEditor.Experimental.GraphView;
using z3y.ShaderGraph.Nodes;

namespace z3y.ShaderGraph
{
    [Serializable]
    public class SerializableGraph
    {
        public GraphData data = new GraphData();
        public List<SerializableNode> nodes = new List<SerializableNode>();

        public static SerializableGraph StoreGraph(ShaderGraphView graphView)
        {
            var serializableGraph = new SerializableGraph
            {
                data = graphView.graphData,
                nodes = ElementsToSerializableNode(graphView.graphElements).ToList()
            };

            return serializableGraph;
        }

        public static IEnumerable<SerializableNode> ElementsToSerializableNode(IEnumerable<GraphElement> elements)
        {
            var nodes = elements
                .Where(x => x is ShaderNodeVisualElement)
                .Cast<ShaderNodeVisualElement>()
                .Where(x => x.shaderNode is not null)
                .Select(x => new SerializableNode(x));

            return nodes;
        }

        public void PopulateGraph(ShaderGraphView graphView)
        {
            graphView.graphData = data;

            foreach (var node in nodes)
            {
                graphView.AddNode(node);
            }

            SetupNodeConnections(graphView);
        }

        public SerializableGraph GenerateNewGUIDs()
        {
            var guidMap = new Dictionary<string, string>();

            var newGraph = new SerializableGraph
            {
                data = data
            };

            foreach (var node in nodes)
            {
                var newGuid = Guid.NewGuid().ToString();
                guidMap.Add(node.guid, newGuid);
                var newNode = node;
                newNode.guid = newGuid;

                var newConnections = new List<NodeConnection>();
                foreach (NodeConnection connection in node.connections)
                {
                    if (guidMap.TryGetValue(connection.node, out string newInputGuid))
                    {
                        var newConnection = connection;
                        newConnection.node = newInputGuid;
                        newConnections.Add(newConnection);
                    }
                }

                newNode.connections = newConnections;
                newGraph.nodes.Add(newNode);
            }

            return newGraph;
        }

        public List<ShaderNodeVisualElement> PasteNodesAndOverwiteGuids(ShaderGraphView graphView)
        {
            var newElements = GenerateNewGUIDs();
            var graphElements = new List<ShaderNodeVisualElement>();

            foreach (var serializableNode in newElements.nodes)
            {
                graphElements.Add(graphView.AddNode(serializableNode));
            }

            newElements.SetupNodeConnections(graphView);
            return graphElements;
        }

        public void SetupNodeConnections(ShaderGraphView graphView)
        {
            foreach (var node in nodes)
            {
                foreach (var connection in node.connections)
                {
                    var graphNode = graphView.GetNodeByGuid(node.guid);
                    var currentNodeInputID = connection.GetInputIDForThisNode();
                    var inputNodeOutputID = connection.GetOutputIDForInputNode();
                    var inputNode = graphView.GetNodeByGuid(connection.GetInputNodeGuid());

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

                        if (port.userData == null || port.GetPortID() != currentNodeInputID)
                        {
                            continue;
                        }

                        if (inputNode is null)
                        {
                            continue;
                        }

                        foreach (var ve2 in inputNode.outputContainer.Children())
                        {
                            if (ve2 is not Port outPort)
                            {
                                continue;
                            }

                            if (outPort.GetPortID() == inputNodeOutputID)
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

        public void ConnectPort(ShaderNodeVisualElement node, NodeConnection connection)
        {

        }

    }
}