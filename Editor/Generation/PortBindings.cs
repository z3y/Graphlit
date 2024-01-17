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
        TangentWS,
        TangentOS,
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
                    PortBinding.PositionOS => RequirePositionOSVertex(pass),
                    PortBinding.PositionWS => RequirePositionWSVertex(pass),
                    PortBinding.NormalOS => RequireNormalOSVertex(pass),
                    PortBinding.NormalWS => RequireNormalWSVertex(pass),
                    PortBinding.TangentOS => RequireTangentOSVertex(pass),
                    PortBinding.TangentWS => RequireTangentWSVertex(pass),
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
                    PortBinding.PositionOS => RequirePositionOSFragment(pass),
                    PortBinding.PositionWS => RequirePositionWSFragment(pass),
                    PortBinding.NormalOS => RequireNormalOSFragment(pass),
                    PortBinding.NormalWS => RequireNormalWSFragment(pass),
                    PortBinding.TangentOS => RequireTangentOSFragment(pass),
                    PortBinding.TangentWS => RequireTangentWSFragment(pass),
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

        #region Tangent
        private static string ObjectToWorldDirection(string a) => $"UnityObjectToWorldDir({a}.xyz)";
        private static string AppendTangentWSVertex(PassBuilder pass)
        {
            string value = "tangentWS";
            var a = pass.attributes.RequireTangentOS();
            pass.generatedBindingsVertex.Add($"float4 {value} = float4({ObjectToWorldDirection(a)}, {a}.w);");
            return value;
        }
        private static string RequireTangentWSFragment(PassBuilder pass)
        {
            string value = AppendTangentWSVertex(pass);
            return pass.varyings.RequireInternal("tangentWS", 4, value);
        }
        private static string RequireTangentOSFragment(PassBuilder pass)
        {
            string value = pass.attributes.RequireTangentOS();
            return pass.varyings.RequireInternal("tangentOS", 4, value);
        }
        private static string RequireTangentWSVertex(PassBuilder pass)
        {
            return AppendTangentWSVertex(pass);
        }
        private static string RequireTangentOSVertex(PassBuilder pass)
        {
            return pass.attributes.RequireTangentOS();
        }
        #endregion
    }
}