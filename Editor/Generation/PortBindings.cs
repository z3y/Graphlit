using System;
using ZSG.Nodes.PortType;

namespace ZSG
{
    public enum PortBinding
    {
        UV0, UV1, UV2, UV3,
        PositionWS,
        PositionOS,
        NormalWS,
        NormalOS,
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
                    PortBinding.PositionWS => RequirePositionWSVertex(pass),
                    PortBinding.PositionOS => RequirePositionOSVertex(pass),
                    PortBinding.NormalOS => RequireNormalOSVertex(pass),
                    PortBinding.NormalWS => RequireNormalWSVertex(pass),
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
                    PortBinding.NormalWS => RequireNormalWSFragment(pass),
                    PortBinding.NormalOS => RequireNormalOSFragment(pass),
                    _ => throw new NotImplementedException(),
                };
            }
        }

        #region Position
        private static string ObjectToWorldPosition(string a) => $"mul(unity_ObjectToWorld, float4({a}, 1))";

        private static string RequirePositionWSFragment(PassBuilder pass)
        {
            var value = AppendPositionWSVertex(pass);
            return pass.varyings.RequireInternal("positionWS", 3, value);
        }
        private static string RequirePositionOSFragment(PassBuilder pass)
        {
            return pass.varyings.RequireInternal("positionOS", 3, pass.attributes.RequirePositionOS());
        }
        private static string RequirePositionOSVertex(PassBuilder pass)
        {
            return pass.attributes.RequirePositionOS();
        }
        private static string AppendPositionWSVertex(PassBuilder pass)
        {
            string value = "positionWS";
            var a = pass.attributes.RequirePositionOS(3);
            pass.generatedBindingsVertex.Add($"float3 {value} = {ObjectToWorldPosition(a)};");
            return value;
        }
        private static string RequirePositionWSVertex(PassBuilder pass)
        {
            return AppendPositionWSVertex(pass);
        }
        #endregion

        #region Normal
        private static string ObjectToWorldNormal(string a) => $"UnityObjectToWorldNormal({a})";
        private static string AppendNormalWSVertex(PassBuilder pass)
        {
            string value = "normalWS";
            var a = pass.attributes.RequireNormalOS(3);
            pass.generatedBindingsVertex.Add($"float3 {value} = {ObjectToWorldNormal(a)};");
            return value;
        }
        private static string RequireNormalWSFragment(PassBuilder pass)
        {
            string value = AppendNormalWSVertex(pass);
            return pass.varyings.RequireInternal("normalWS", 3, value);
        }
        private static string RequireNormalOSFragment(PassBuilder pass)
        {
            string value = pass.attributes.RequireNormalOS(3);
            return pass.varyings.RequireInternal("normalOS", 3, value);
        }
        private static string RequireNormalWSVertex(PassBuilder pass)
        {
            return AppendNormalWSVertex(pass);
        }
        private static string RequireNormalOSVertex(PassBuilder pass)
        {
            return pass.attributes.RequireNormalOS(3);
        }

        #endregion

    }
}