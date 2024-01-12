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
        public int x;
        public int y;
        public List<NodeConnection> connections;
        public string data;

        public readonly Vector2 Position => new (x, y);

        public SerializableNode(ShaderNodeVisualElement node)
        {
            var type = node.shaderNode.GetType();

            this.type = type.FullName;
            this.guid = node.viewDataKey;
            var pos = node.GetPosition().position;
            this.x = (int)pos.x;
            this.y = (int)pos.y;

            this.connections = NodeConnection.GetConnections(node.Ports);

            var seriazableAttribute = Attribute.GetCustomAttribute(type, typeof(SerializableAttribute));
            if (seriazableAttribute is not null)
            {
                data = JsonUtility.ToJson(node.shaderNode);
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

            shaderNode.GUID = guid;

            return true;
        }
    }
}