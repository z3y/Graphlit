using System.Collections.Generic;

namespace z3y.ShaderGraph
{
    public enum PropertyType
    {
        Float,
        Float2,
        Float3,
        Float4,
        Color,
        Intiger,
        Texture2D,
        TextureAny,
        TextureCube,
        // etc
    }

    public struct PropertyDescriptor
    {
        //TODO implement
        public PropertyDescriptor(PropertyType type, string displayName, string defaultValue, string name = null, List<string> attributes = null)
        {
            Type = type;
            DisplayName = displayName;
            DefaultValue = defaultValue;
            Name = name;
            Attributes = attributes;
        }
        public string Name { get; }
        public string DisplayName { get; }
        public string DefaultValue { get; }
        public PropertyType Type { get; }
        public List<string> Attributes { get; }

        public override string ToString()
        {
            var name = Name;
            return $"{string.Join(' ', Attributes)} {name} (\"{DisplayName}\", Range(0.0, 1.0)) = 0.0";
        }
    }
}