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
        [SerializeField] string _value;
        public float FloatValue
        {
            get
            {
                float.TryParse(_value, out float value);
                return value;
            }
            set
            {
                _value = value.ToString();
            }
        }
        public Vector4 VectorValue
        {
            get
            {
                if (string.IsNullOrEmpty(_value))
                {
                    return Vector4.zero;
                }

                string withoutParens = _value.Replace(")", "").Replace("(", "");
                string[] split = withoutParens.Split(',');
                float.TryParse(split[0], out float x);
                float.TryParse(split[1], out float y);
                float.TryParse(split[2], out float z);
                float.TryParse(split[3], out float w);
                return new Vector4(x, y, z , w);
            }
            set
            {
                _value = value.ToString();
            }
        }
        public Texture DefaultTexture
        {
            get
            {
                if (string.IsNullOrEmpty(_value))
                {
                    return null;
                }
                return Helpers.SerializableReferenceToObject<Texture>(_value);
            }
            set
            {
                _value = Helpers.AssetSerializableReference(value);
            }
        }


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
            return type switch
            {
                PropertyType.Float => _value,
                PropertyType.Float2 => _value,
                PropertyType.Float3 => _value,
                PropertyType.Float4 => _value,
                PropertyType.Color => _value,
                PropertyType.Intiger => _value,
                PropertyType.Texture2D => "\"white\" {}",
                PropertyType.TextureCube => "\"white\" {}",
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
                //PropertyType.Range => $"Range ({range.x.ToString("R")}, {range.y.ToString("R")})",
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