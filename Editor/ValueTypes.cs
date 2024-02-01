using UnityEngine;

namespace ZSG.Nodes.PortType
{
    public interface IPortType
    {
        public Color GetPortColor();
    }

    public struct Float : IPortType
    {
        public int components;
        public bool dynamic;
        public Float(int components, bool dynamic = false)
        {
            if (components < 1 || components > 4)
            {
                Debug.LogError("Invalid component count");
            }
            this.components = components;
            this.dynamic = dynamic;
        }

        public override string ToString()
        {
            return components switch
            {
                1 => "float",
                2 => "float2",
                3 => "float3",
                4 => "float4",
                _ => null,
            };
        }

        public static Color Float1Color = Color.grey;
        public static Color Float2Color = new Color(232 / 255.0f, 255 / 255.0f, 183 / 255.0f); // yellow
        public static Color Float3Color = new Color(196 / 255.0f, 245 / 255.0f, 252 / 255.0f); // cyan
        public static Color Float4Color = new Color(226 / 255.0f, 160 / 255.0f, 255 / 255.0f); // magenta
        public Color GetPortColor()
        {
            return components switch
            {
                1 => Float1Color,
                2 => Float2Color,
                3 => Float3Color,
                4 => Float4Color,
                _ => Color.white,
            };
        }
    }

    public struct Texture2DObject : IPortType
    {
        public Color GetPortColor() => new Color(0.8f, 0.3f, 0.3f);
        public override readonly string ToString() => "Texture2D";
    }

    public struct SamplerState : IPortType
    {
        public Color GetPortColor() => new Color(0.8f, 0.8f, 0.8f);
        public override readonly string ToString() => "SamplerState";
    }
    public struct Bool : IPortType
    {
        public Color GetPortColor() => new Color(0.8f, 0.4f, 0.8f);
        public override readonly string ToString() => "bool";
    }
    public struct Int : IPortType
    {
        public Color GetPortColor() => new Color(0.1f, 0.8f, 0.8f);
        public override readonly string ToString() => "int";
    }
    public struct UInt : IPortType
    {
        public Color GetPortColor() => new Color(0.1f, 0.8f, 0.8f);
        public override readonly string ToString() => "uint";
    }
    public struct UnknownType : IPortType
    {
        string _type;
        public string array;
        public UnknownType(string type, string array = "")
        {
            _type = type;
            this.array = array;
        }
        public Color GetPortColor() => new Color(0.4f, 0.3f, 0.3f);
        public override readonly string ToString() => _type;
    }
}