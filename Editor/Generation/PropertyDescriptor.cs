using System;
using System.Collections.Generic;
using System.Text;
using UnityEngine;

namespace ZSG
{
    [Serializable]
    public enum PropertyType
    {
        Float,
        Float2,
        Float3,
        Float4,
        Range,
        Color,
        Intiger,
        Texture2D,
        TextureCube,
    }

    public enum PropertyDeclaration
    {
        Constant,
        Property
    }

    [Serializable]
    public class PropertyDescriptor
    {
        [SerializeField] public string guid;
        [SerializeField] public string referenceName;
        [SerializeField] public string displayName;
        [SerializeField] public PropertyType type;
        [SerializeField] public List<string> attributes;
        [SerializeField] public Vector2 range;
        [SerializeField] public float floatValue;
        [SerializeField] public Vector4 vectorValue;


        public PropertyDescriptor(PropertyType type, string displayName, string referenceName = "", List<string> attributes = null)
        {
            this.type = type;
            this.displayName = displayName;
            this.attributes = attributes;
            this.referenceName = referenceName;
            guid = Guid.NewGuid().ToString();
        }

        public string GetDefaultValue()
        {
            if (floatValue != 0)
            {
                return floatValue.ToString();
            }
            if (vectorValue != Vector4.zero)
            {
                return vectorValue.ToString();
            }
            return type switch
            {
                PropertyType.Float => "0",
                PropertyType.Float2 => "(0,0,0,0)",
                PropertyType.Float3 => "(0,0,0,0)",
                PropertyType.Float4 => "(0,0,0,0)",
                PropertyType.Range => "0",
                PropertyType.Color => "(0,0,0,0)",
                PropertyType.Intiger => "0",
                PropertyType.Texture2D => "\"black\" {}",
                PropertyType.TextureCube => "\"black\" {}",
                _ => throw new System.NotImplementedException(),
            };
        }

/*
        public void SetPreviewName(SerializableNode serializableNode)
        {
            referenceName = "_" + serializableNode.guid;
        }*/

        public string TypeToString()
        {
            return type switch
            {
                PropertyType.Float => "Float",
                PropertyType.Float2 => "Vector",
                PropertyType.Float3 => "Vector",
                PropertyType.Float4 => "Vector",
                PropertyType.Range => $"Range ({range.x.ToString("R")}, {range.y.ToString("R")})",
                PropertyType.Color => "Color",
                PropertyType.Intiger => "Intiger",
                PropertyType.Texture2D => "2D",
                PropertyType.TextureCube => "Cube",
                _ => throw new System.NotImplementedException()
            };
        }

        public string Declaration()
        {
            var referenceName = GetReferenceName();

            return type switch
            {
                PropertyType.Float => $"float {referenceName};",
                PropertyType.Float2 => $"float2 {referenceName};",
                PropertyType.Float3 => $"float3 {referenceName};",
                PropertyType.Float4 => $"float4 {referenceName};",
                PropertyType.Range => $"float {referenceName};",
                PropertyType.Color => $"float4 {referenceName};",
                PropertyType.Intiger => $"int {referenceName};",
                PropertyType.Texture2D => $"Texture2D {referenceName}; SamplerState sampler{referenceName};",
                PropertyType.TextureCube => $"TextureCube {referenceName}; SamplerState sampler{referenceName};",
                _ => throw new System.NotImplementedException()
            };
        }

        public string AttributesToString()
        {
            if (attributes is null)
            {
                return string.Empty;
            }

            var sb = new StringBuilder();
            foreach (var attribute in attributes)
            {
                sb.Append("[");
                sb.Append(attribute.ToString());
                sb.Append("] ");
            }
            return sb.ToString();
        }

        public string GetReferenceName()
        {
            return string.IsNullOrEmpty(this.referenceName) ? "_" + displayName?.RemoveWhitespace() : this.referenceName;
        }

        public override string ToString()
        {
            var referenceName = GetReferenceName();
            var type = TypeToString();
            var attributes = AttributesToString();
            var defaultValue = GetDefaultValue();

            return $"{attributes} {referenceName} (\"{displayName}\", {type}) = {defaultValue}";
        }
    }
}