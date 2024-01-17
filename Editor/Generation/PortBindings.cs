using System;
using ZSG.Nodes.PortType;
using static ZSG.ShaderVaryings;

namespace ZSG
{
    public enum PortBinding
    {
        UV0, UV1, UV2, UV3,
        PositionWS,
        PositionOS
    }

    public static class PortBindings
    {
        public static string GetBindingString(PassBuilder pass, NodeVisitor vistor, Float @float, PortBinding binding)
        {
            int components = @float.components;
            if (vistor.Stage == ShaderStage.Vertex)
            {
                var attributes = pass.attributes;
                return binding switch
                {
                    PortBinding.UV0 => attributes.RequireUV(0, components),
                    PortBinding.UV1 => attributes.RequireUV(1, components),
                    PortBinding.UV2 => attributes.RequireUV(2, components),
                    PortBinding.UV3 => attributes.RequireUV(3, components),
                    _ => throw new NotImplementedException(),
                };
            }
            else
            {
                var varyings = pass.varyings;
                return binding switch
                {
                    PortBinding.UV0 => varyings.RequireUV(0, components),
                    PortBinding.UV1 => varyings.RequireUV(1, components),
                    PortBinding.UV2 => varyings.RequireUV(2, components),
                    PortBinding.UV3 => varyings.RequireUV(3, components),
                    PortBinding.PositionWS => RequirePositionWSFragment(pass),
                    PortBinding.PositionOS => RequirePositionOSFragment(pass),
                    _ => throw new NotImplementedException(),
                };
            }
        }

        private static string RequirePositionWSFragment(PassBuilder pass, int components = 3)
        {
            var a = pass.attributes.RequirePositionOS(3);
            string passthrough = $"UnityObjectToWorldNormal({a})";
            return pass.varyings.RequireInternal("positionWS", components, passthrough);
        }
        private static string RequirePositionOSFragment(PassBuilder pass, int components = 3)
        {
            return pass.varyings.RequireInternal("positionOS", 3, pass.attributes.RequirePositionOS());
        }
    }
}