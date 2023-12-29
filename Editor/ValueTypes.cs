using UnityEngine;

namespace z3y.ShaderGraph.Nodes.PortType
{
    public interface IPortType
    {
        public Color GetPortColor();
    }

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

    public struct Float : IPortType
    {
        public int components;
        public bool fullPrecision;
        public bool dynamic;
        public Float(int components, bool dynamic = false, bool fullPrecision = true)
        {
            if (components < 1 || components > 4)
            {
                Debug.LogError("Invalid component count");
            }
            this.components = components;
            this.fullPrecision = fullPrecision;
            this.dynamic = dynamic;
        }

        public override string ToString()
        {
            if (fullPrecision)
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
            else
            {
                return components switch
                {
                    1 => "half",
                    2 => "half2",
                    3 => "half3",
                    4 => "half4",
                    _ => null,
                };
            }
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

    public struct Texture2D : IPortType
    {
        public Color GetPortColor() => Color.cyan;
    }
    public struct SamplerState : IPortType
    {
        public Color GetPortColor() => Color.gray;
    }
}