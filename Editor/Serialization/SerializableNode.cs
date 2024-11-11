using System.Collections.Generic;
using System;
using UnityEngine;
using UnityEditor;

namespace Graphlit
{
    [Serializable]
    public struct SerializableNode
    {
        public string type;
        public string guid;
        public int x;
        public int y;
        public List<NodeConnection> connections;
        public Precision precision;
        public PreviewType preview;
        public bool previewDisabled;

        public string data;

        public readonly Vector2 Position => new(x, y);

        public SerializableNode(ShaderNode node)
        {
            var type = node.GetType();

            this.type = type.FullName;
            this.guid = node.viewDataKey;
            var pos = node.GetPosition().position;
            this.x = (int)pos.x;
            this.y = (int)pos.y;
            this.precision = node.DefaultPrecision;
            this.preview = node.DefaultPreview;
            this.previewDisabled = node._previewDisabled;

            this.connections = NodeConnection.GetConnections(node.PortElements);

            var seriazableAttribute = Attribute.GetCustomAttribute(type, typeof(SerializableAttribute));
            if (seriazableAttribute is not null)
            {
                data = EditorJsonUtility.ToJson(node);
            }
            else
            {
                data = string.Empty;
            }
        }

        static Dictionary<string, Type> _typeCache = new();

        public readonly bool TryDeserialize(ShaderGraphView graphView, out ShaderNode shaderNode)
        {
            string typeName = this.type.Replace("ZSG", "Graphlit");
            if (!_typeCache.TryGetValue(typeName, out Type type))
            {
                type = Type.GetType(typeName);
                _typeCache[typeName] = type;
            }
            
            if (type is null)
            {
                Debug.LogError($"Node of type {this.type} not found");
                shaderNode = null;
                return false;
            }
            var instance = (ShaderNode)Activator.CreateInstance(type);


            if (!string.IsNullOrEmpty(data))
            {
                EditorJsonUtility.FromJsonOverwrite(data, instance);
            }

            shaderNode = instance;

            shaderNode._defaultPrecision = this.precision;
            shaderNode._defaultPreview = this.preview;
            shaderNode._previewDisabled = this.previewDisabled;

            shaderNode.InitializeInternal(graphView, Position, guid);
            return true;
        }
    }
}