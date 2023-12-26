using UnityEngine;

namespace z3y.ShaderGraph.Nodes
{
    public struct PortType
    {
        public struct DynamicFloat
        {
            public int components;
            public bool fullPrecision;
            public DynamicFloat(int components, bool fullPrecision = true)
            {
                if (components < 1 || components > 4)
                {
                    Debug.LogError("Invalid component count");
                }
                this.components = components;
                this.fullPrecision = fullPrecision;
            }

            public override readonly string ToString()
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
        };

        public enum Texture2D { }
        public enum SamplerState { }
    }
}