using System.Collections.Generic;
using System;
using UnityEngine;
using z3y.ShaderGraph.Nodes;

namespace z3y.ShaderGraph
{
    [Serializable]
    public struct SerializableNode
    {
        public string type;
        public string guid;
        public Vector2 position;
        public List<NodeConnection> connections;
        public string data;

        public SerializableNode(ShaderNodeVisualElement node)
        {
            var type = node.shaderNode.GetType();

            this.type = type.FullName;
            this.guid = node.viewDataKey;
            this.position = node.GetPosition().position;
            this.connections = NodeConnection.GetConnections(node.Ports);

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
            if (type is null)
            {
                Debug.LogError($"Node of type {this.type} not found");
                shaderNode = null;
                return false;
            }

            var instance = Activator.CreateInstance(type);

            if (!string.IsNullOrEmpty(data))
            {
                JsonUtility.FromJsonOverwrite(data, instance);
            }

            shaderNode = (ShaderNode)instance;

            return true;
        }
    }
}