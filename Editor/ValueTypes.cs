

using UnityEngine;

namespace z3y.ShaderGraph.Nodes
{
    public struct PortType
    {
        public struct DynamicFloat
        {
            public DynamicFloat(int components)
            {
                if (components < 1 || components > 4)
                {
                    Debug.LogError("Invalid component count");
                }
                this.components = components;
            }

            public int components;
        };
        public enum Texture2D { }
        public enum SamplerState { }
    }
}