using System;
using System.Collections.Generic;
using System.Linq;
using UnityEditor.Experimental.GraphView;
using UnityEngine;

namespace Graphlit
{
    [Serializable]
    public class SerializableGraph
    {
        public GraphData data = new GraphData();
        public List<SerializableNode> nodes = new List<SerializableNode>();
        public List<SerializableGroup> groups = new List<SerializableGroup>();

        [Serializable]
        public struct SerializableGroup
        {
            public SerializableGroup(Group groupNode)
            {
                Vector2 pos = groupNode.GetPosition().position;
                x = (int)pos.x;
                y = (int)pos.y;
                elements = new List<string>();
                foreach (var element in groupNode.containedElements)
                {
                    elements.Add(element.viewDataKey);
                }

                title = groupNode.title;
            }

            public int x;
            public int y;
            public string title;
            public List<string> elements;
        }

        public static SerializableGraph StoreGraph(ShaderGraphView graphView)
        {
            var serializableGraph = new SerializableGraph
            {
                data = graphView.graphData,
                nodes = ElementsToSerializableNode(graphView.graphElements).ToList()
            };

            serializableGraph.groups = graphView.graphElements
                .OfType<Group>()
                .Select(x => new SerializableGroup(x)).ToList();

            return serializableGraph;
        }

        public static IEnumerable<SerializableNode> ElementsToSerializableNode(IEnumerable<GraphElement> elements)
        {
            var nodes = elements
                .OfType<ShaderNode>()
                .Select(x => new SerializableNode(x));

            return nodes;
        }

        public void PopulateGraph(ShaderGraphView graphView)
        {
            graphView.graphData = data;
            var sw = new System.Diagnostics.Stopwatch();
            sw.Start();

            // try catch node connections arent lost
            try
            {
                foreach (var node in nodes)
                {
                    graphView.AddNode(node);
                }
            }
            catch (Exception e)
            {
                Debug.LogError(e);
            }
            //Debug.Log("Add Nodes: " + sw.ElapsedMilliseconds);
            sw.Restart();

            SetupNodeConnections(graphView);
            //Debug.Log("Setup Connections: " + sw.ElapsedMilliseconds);
            sw.Restart();

            SetupGroups(graphView);
            //Debug.Log("Setup Groups: " + sw.ElapsedMilliseconds);
            sw.Stop();
        }

        void SetupGroups(ShaderGraphView graphView)
        {
            var groupGuids = graphView.GetElementsGuidDictionary<Node>();

            foreach (var group in groups)
            {
                var g = new Group();
                g.SetPosition(new Rect(group.x, group.y, 1, 1));
                g.title = group.title;

                foreach (var guid in group.elements)
                {
                    if (groupGuids.TryGetValue(guid, out Node element))
                    {
                        g.AddElement(element);
                    }
                }

                graphView.AddElement(g);
            }
        }

        private void UpdatePreviews(ShaderGraphView graphView)
        {
            foreach (var node in graphView.graphElements)
            {
                if (node is ShaderNode shaderNode)
                {
                    shaderNode.GeneratePreview();
                }
            }
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
                newGraph.nodes.Add(newNode);
            }

            for (int i = 0; i < newGraph.nodes.Count; i++)
            {
                SerializableNode node = newGraph.nodes[i];
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

                node.connections = newConnections;
                newGraph.nodes[i] = node;
            }

            for (int i = 0; i < groups.Count; i++)
            {
                var g = groups[i];

                for (int j = 0; j < g.elements.Count; j++)
                {
                    if (guidMap.TryGetValue(groups[i].elements[j], out string newInputGuid))
                    {
                        groups[i].elements[j] = newInputGuid;
                    }
                }
            }
            return newGraph;
        }

        public List<ShaderNode> PasteElementsAndOverwiteGuids(ShaderGraphView graphView, Vector2? positionOffset = null)
        {
            var newElements = GenerateNewGUIDs();

            foreach (var copiedProp in data.properties)
            {
                var existingProperty = graphView.graphData.properties.Find(x => x.GetReferenceName(GenerationMode.Final) == copiedProp.GetReferenceName(GenerationMode.Final));
                var existingPropertyGuid = graphView.graphData.properties.Find(x => x.guid == copiedProp.guid);

                if (existingProperty is null && existingPropertyGuid is null)
                {
                    graphView.graphData.properties.Add(copiedProp);
                }
            }

            var graphElements = new List<ShaderNode>();

            foreach (var serializableNode in newElements.nodes)
            {
                var graphElement = graphView.AddNode(serializableNode);

                if (positionOffset is Vector2 offset)
                {
                    var previousPosition = serializableNode.Position;
                    graphElement.SetPosition(new Rect(previousPosition + offset, Vector2.one));
                }
                graphElements.Add(graphElement);


            }

            newElements.SetupNodeConnections(graphView);

            SetupGroups(graphView);

            UpdatePreviews(graphView);

            return graphElements;
        }

        public void SetupNodeConnections(ShaderGraphView graphView)
        {
            var nodeGuids = graphView.GetElementsGuidDictionary<Node>();
            foreach (var node in nodes)
            {
                foreach (var connection in node.connections)
                {
                    if (!nodeGuids.TryGetValue(node.guid, out Node graphNode))
                    {
                        continue;
                    }

                    var currentNodeInputID = connection.GetInputIDForThisNode();
                    var inputNodeOutputID = connection.GetOutputIDForInputNode();

                    if (!nodeGuids.TryGetValue(connection.GetInputNodeGuid(), out Node inputNode))
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

        /*        public void ConnectPort(ShaderNodeVisualElement node, NodeConnection connection)
                {

                }*/

    }
}