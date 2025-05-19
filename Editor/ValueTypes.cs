using UnityEngine;

namespace Graphlit.Nodes.PortType
{
    public interface IPortType
    {
        public Color GetPortColor();
    }

    public struct Float : IPortType
    {
        public int dimensions;
        public bool dynamic;
        public Float(int dimensions, bool dynamic = false)
        {
            if (dimensions < 1 || dimensions > 4)
            {
                Debug.LogError("Invalid dimension count");
            }
            this.dimensions = dimensions;
            this.dynamic = dynamic;
        }

        public override string ToString()
        {
            return dimensions switch
            {
                1 => "float",
                2 => "float2",
                3 => "float3",
                4 => "float4",
                _ => null,
            };
        }

        /*public static Color Float1Color = Color.grey;
        public static Color Float2Color = new Color(232 / 255.0f, 255 / 255.0f, 183 / 255.0f); // yellow
        public static Color Float3Color = new Color(196 / 255.0f, 245 / 255.0f, 252 / 255.0f); // cyan
        public static Color Float4Color = new Color(226 / 255.0f, 160 / 255.0f, 255 / 255.0f); // magenta*/

        public static Color Float1Color = new Color(0.5176470588235295f, 0.8941176470588236f, 0.9058823529411765f);
        public static Color Float2Color = new Color(0.44313725490196076f, 0.9098039215686274f, 0.4f);
        public static Color Float3Color = new Color(0.9647058823529412f, 1f, 0.6039215686274509f);
        public static Color Float4Color = new Color(0.984313725490196f, 0.796078431372549f, 0.9568627450980393f);
        public Color GetPortColor()
        {
            return GetPortColor(dimensions);
        }
        public static Color GetPortColor(int dimensions)
        {
            return dimensions switch
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
    public struct Texture3DObject : IPortType
    {
        public Color GetPortColor() => new Color(0.8f, 0.3f, 0.3f);
        public override readonly string ToString() => "Texture3D";
    }
    public struct Texture2DArrayObject : IPortType
    {
        public Color GetPortColor() => new Color(0.8f, 0.3f, 0.3f);
        public override readonly string ToString() => "Texture2DArray";
    }
    public struct TextureCubeObject : IPortType
    {
        public Color GetPortColor() => new Color(0.8f, 0.3f, 0.3f);
        public override readonly string ToString() => "TextureCube";
    }
    public struct TextureCubeArrayObject : IPortType
    {
        public Color GetPortColor() => new Color(0.8f, 0.3f, 0.3f);
        public override readonly string ToString() => "TextureCubeArray";
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
    public struct CustomType : IPortType
    {
        string _type;
        public CustomType(string type)
        {
            _type = type;
        }
        public Color GetPortColor() => new Color(0.4f, 0.3f, 0.3f);
        public override readonly string ToString() => _type;
    }
}