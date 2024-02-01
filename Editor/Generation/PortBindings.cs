using System;

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
        ViewDirectionWS,
        ViewDirectionTS,
        ViewDirectionOS,
        BitangentWS
    }

    public enum BindingSpace
    {
        Object,
        World,
        Tangent,
        View
    }

    public static class PortBindings
    {
        public static PortBinding PositionBindingFromSpace(BindingSpace space)
        {
            return space switch
            {
                BindingSpace.Object => PortBinding.PositionOS,
                BindingSpace.World => PortBinding.PositionWS,
                BindingSpace.Tangent => throw new NotImplementedException(),
                BindingSpace.View => throw new NotImplementedException(),
                _ => throw new NotImplementedException()
            };
        }
        public static PortBinding NormalBindingFromSpace(BindingSpace space)
        {
            return space switch
            {
                BindingSpace.Object => PortBinding.NormalOS,
                BindingSpace.World => PortBinding.NormalWS,
                BindingSpace.Tangent => throw new NotImplementedException(),
                BindingSpace.View => throw new NotImplementedException(),
                _ => throw new NotImplementedException()
            };
        }
        public static PortBinding TangentBindingFromSpace(BindingSpace space)
        {
            return space switch
            {
                BindingSpace.Object => PortBinding.TangentOS,
                BindingSpace.World => PortBinding.TangentWS,
                BindingSpace.Tangent => throw new NotImplementedException(),
                BindingSpace.View => throw new NotImplementedException(),
                _ => throw new NotImplementedException()
            };
        }
        public static PortBinding BitangentBindingFromSpace(BindingSpace space)
        {
            return space switch
            {
                BindingSpace.Object => throw new NotImplementedException(),
                BindingSpace.World => PortBinding.BitangentWS,
                BindingSpace.Tangent => throw new NotImplementedException(),
                BindingSpace.View => throw new NotImplementedException(),
                _ => throw new NotImplementedException()
            };
        }
        public static PortBinding ViewBindingFromSpace(BindingSpace space)
        {
            return space switch
            {
                BindingSpace.Object => PortBinding.ViewDirectionOS,
                BindingSpace.World => PortBinding.ViewDirectionWS,
                BindingSpace.Tangent => PortBinding.ViewDirectionTS,
                BindingSpace.View => throw new NotImplementedException(),
                _ => throw new NotImplementedException()
            };
        }

        public static string GetBindingString(PassBuilder pass, ShaderStage stage, int components, PortBinding binding)
        {
            if (stage == ShaderStage.Vertex)
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
                    PortBinding.ViewDirectionWS => RequireViewDirectionWSFragment(pass),
                    PortBinding.ViewDirectionTS => RequireViewDirectionTSFragment(pass),
                    PortBinding.ViewDirectionOS => RequireViewDirectionOSFragment(pass),
                    PortBinding.BitangentWS => RequireBitangentWSFragment(pass),
                    _ => throw new NotImplementedException(),
                };
            }
        }

        #region Position

        private static string RequirePositionWSFragment(PassBuilder pass)
        {
            var value = AppendPositionWSVertex(pass);
            pass.varyings.RequireInternal("positionWS", 3, value);
            return "data.positionWS";
        }
        private static string RequirePositionOSFragment(PassBuilder pass)
        {
            RequirePositionWSFragment(pass);
            return "data.positionOS";
        }
        private static string RequirePositionOSVertex(PassBuilder pass)
        {
            return pass.attributes.RequirePositionOS();
        }
        private static string AppendPositionWSVertex(PassBuilder pass)
        {
            string value = "positionWS";
            var positionOS = pass.attributes.RequirePositionOS(3);
            pass.generatedBindingsVertex.Add($"float3 {value} = {SpaceTransform.ObjectToWorld(positionOS)};");
            return value;
        }
        private static string RequirePositionWSVertex(PassBuilder pass)
        {
            return AppendPositionWSVertex(pass);
        }
        #endregion

        #region Normal
        private static string AppendNormalWSVertex(PassBuilder pass)
        {
            string value = "normalWS";
            var normalOS = pass.attributes.RequireNormalOS(3);
            pass.generatedBindingsVertex.Add($"float3 {value} = {SpaceTransform.ObjectToWorldNormal(normalOS)};");
            return value;
        }
        private static string RequireNormalWSFragment(PassBuilder pass)
        {
            string value = AppendNormalWSVertex(pass);
            pass.varyings.RequireInternal("normalWS", 3, value);
            return "data.normalWS";
        }
        private static string RequireNormalOSFragment(PassBuilder pass)
        {
            RequireNormalWSFragment(pass);
            return "data.normalOS";
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
        private static string AppendTangentWSVertex(PassBuilder pass)
        {
            string value = "tangentWS";
            RequireNormalWSFragment(pass);
            var tangentOS = pass.attributes.RequireTangentOS();
            pass.generatedBindingsVertex.Add($"float4 {value} = float4({SpaceTransform.ObjectToWorldDirection(tangentOS)}, {tangentOS}.w);");
            return value;
        }
        private static string RequireTangentWSFragment(PassBuilder pass)
        {
            string value = AppendTangentWSVertex(pass);
            pass.varyings.RequireInternal("normalWS", 3, value);
            pass.varyings.RequireInternal("tangentWS", 4, value);
            return "data.tangentWS";
        }
        private static string RequireTangentOSFragment(PassBuilder pass)
        {
            RequireTangentWSFragment(pass);
            return "data.tangentOS";
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
        private static string RequireViewDirectionWSFragment(PassBuilder pass)
        {
            RequirePositionWSFragment(pass);
            return "data.viewDirectionWS";
        }
        private static void RequireBTNFragment(PassBuilder pass)
        {
            RequireBitangentWSFragment(pass);
            RequireTangentWSFragment(pass);
            RequireNormalWSFragment(pass);
        }
        private static string RequireViewDirectionTSFragment(PassBuilder pass)
        {
            RequireBTNFragment(pass);
            RequireViewDirectionWSFragment(pass);
            return "data.viewDirectionTS";
        }
        private static string RequireViewDirectionOSFragment(PassBuilder pass)
        {
            RequireViewDirectionWSFragment(pass);
            return "data.viewDirectionOS";
        }
        private static string RequireBitangentWSFragment(PassBuilder pass)
        {
            RequireTangentWSFragment(pass);
            return "data.bitangentWS";
        }
    }
}