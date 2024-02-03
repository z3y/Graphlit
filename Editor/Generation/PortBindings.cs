using Microsoft.SqlServer.Server;
using System;
using UnityEngine.UIElements;

namespace ZSG
{
    public enum PortBinding
    {
        UV0 = 0,
        UV1 = 1,
        UV2 = 2,
        UV3 = 3,
        PositionWS = 4,
        PositionOS = 5,
        NormalWS = 6,
        NormalOS = 7,
        TangentWS = 8,
        TangentOS = 9,
        ViewDirectionWS = 10,
        ViewDirectionTS = 11,
        ViewDirectionOS = 12,
        BitangentWS = 13,
        BitangentOS = 14,
        VertexColor = 15
    }

    public enum BindingSpace
    {
        Object = 0,
        World = 1,
        Tangent = 2,
        View = 3
    }
    public enum BindingSpaceObjectWorld
    {
        Object = 0,
        World = 1,
    }

    public static class PortBindings
    {
        public static PortBinding PositionBindingFromSpace(BindingSpaceObjectWorld space)
        {
            return space switch
            {
                BindingSpaceObjectWorld.Object => PortBinding.PositionOS,
                BindingSpaceObjectWorld.World => PortBinding.PositionWS,
                _ => throw new NotImplementedException()
            };
        }
        public static PortBinding NormalBindingFromSpace(BindingSpaceObjectWorld space)
        {
            return space switch
            {
                BindingSpaceObjectWorld.Object => PortBinding.NormalOS,
                BindingSpaceObjectWorld.World => PortBinding.NormalWS,
                _ => throw new NotImplementedException()
            };
        }
        public static PortBinding TangentBindingFromSpace(BindingSpaceObjectWorld space)
        {
            return space switch
            {
                BindingSpaceObjectWorld.Object => PortBinding.TangentOS,
                BindingSpaceObjectWorld.World => PortBinding.TangentWS,
                _ => throw new NotImplementedException()
            };
        }
        public static PortBinding BitangentBindingFromSpace(BindingSpaceObjectWorld space)
        {
            return space switch
            {
                BindingSpaceObjectWorld.Object => PortBinding.BitangentOS,
                BindingSpaceObjectWorld.World => PortBinding.BitangentWS,
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
                BindingSpace.View => PortBinding.ViewDirectionWS,
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
                    PortBinding.VertexColor => attributes.RequireColor(),
                    PortBinding.ViewDirectionWS => RequireViewDirectionWSVertex(pass),
                    PortBinding.ViewDirectionTS => RequireViewDirectionTSVertex(pass),
                    PortBinding.ViewDirectionOS => RequireViewDirectionOSVertex(pass),
                    PortBinding.BitangentWS => RequireBitangentWSVertex(pass),
                    PortBinding.BitangentOS => RequireBitangentOSVertex(pass),
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
                    PortBinding.BitangentOS => RequireBitangentOSFragment(pass),
                    PortBinding.VertexColor => varyings.RequireColor(),
                    _ => throw new NotImplementedException(),
                }; ;
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
            pass.AddVertexBinding($"float3 {value} = {SpaceTransform.ObjectToWorld(positionOS)};");
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
            pass.AddVertexBinding($"float3 {value} = {SpaceTransform.ObjectToWorldNormal(normalOS)};");
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
            pass.AddVertexBinding($"float4 {value} = float4({SpaceTransform.ObjectToWorldDirection(tangentOS)}, {tangentOS}.w);");
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
        private static string RequireBitangentOSFragment(PassBuilder pass)
        {
            RequireTangentWSFragment(pass);
            return "data.bitangentOS";
        }

        private static string RequireViewDirectionWSVertex(PassBuilder pass)
        {
            var positionWS = RequirePositionWSVertex(pass);
            string value = "viewDirectionWS";
            pass.AddVertexBinding($"float3 {value} = normalize(_WorldSpaceCameraPos.xyz - {positionWS});");
            return value;
        }
        private static string RequireViewDirectionOSVertex(PassBuilder pass)
        {
            var viewDirWS = RequireViewDirectionWSVertex(pass);
            string value = "viewDirectionOS";
            pass.AddVertexBinding($"float3 {value} = TransformWorldToObjectDir({viewDirWS});");
            return value;
        }
        private static string RequireViewDirectionTSVertex(PassBuilder pass)
        {
            string value = "viewDirectionTS";
            var viewDirWS = RequireViewDirectionWSVertex(pass);
            var tangentWS = RequireTangentWSVertex(pass);
            var normalWS = RequireNormalWSVertex(pass);
            var bitangentWS = RequireBitangentWSVertex(pass);

            pass.AddVertexBinding($"float3x3 tangentSpaceTransform = float3x3({tangentWS}.xyz, {bitangentWS}, {normalWS});");
            pass.AddVertexBinding($"float {value} = mul(tangentSpaceTransform, {viewDirWS});");

            return value;
        }

        private static string RequireBitangentWSVertex(PassBuilder pass)
        {
            var normal = RequireNormalWSVertex(pass);
            var tangent = RequireTangentWSVertex(pass);
            string value = "bitangentWS";
            pass.AddVertexBinding($"float crossSign = (attributes.tangentOS.w > 0.0 ? 1.0 : -1.0) * unity_WorldTransformParams.w;");
            pass.AddVertexBinding($"float3 {value} = crossSign * cross({normal}, {tangent});");
            return value;
        }
        private static string RequireBitangentOSVertex(PassBuilder pass)
        {
            var bitangentWS = RequireBitangentWSVertex(pass);
            string value = "bitangentOS";
            pass.AddVertexBinding($"float3 {value} = TransformWorldToObjectDir({bitangentWS});");
            return value;
        }
    }
}