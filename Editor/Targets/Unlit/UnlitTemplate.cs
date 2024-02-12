using UnityEngine.UIElements;
using ZSG.Nodes.PortType;
using ZSG.Nodes;
using UnityEngine;
using UnityEditor;

namespace ZSG
{
    [NodeInfo("Targets/Unlit Target")]
    public class UnlitTemplate : TemplateOutput
    {
        [MenuItem("Assets/Create/Shader Graph Z/Unlit Graph")]
        public static void CreateVariantFile() => ShaderGraphImporter.CreateEmptyTemplate<UnlitTemplate>();

        public override string Name { get; } = "Unlit";
        public override int[] VertexPorts => new int[] { POSITION, NORMAL, TANGENT };
        public override int[] FragmentPorts => new int[] { COLOR, ALPHA, CUTOFF };

        const int POSITION = 0;
        const int NORMAL = 1;
        const int TANGENT = 2;
        const int COLOR = 3;
        const int ALPHA = 4;
        const int CUTOFF = 5;
        public override void Initialize()
        {
            AddPort(new(PortDirection.Input, new Float(3, false), POSITION, "Position"));
            AddPort(new(PortDirection.Input, new Float(3, false), NORMAL, "Normal"));
            AddPort(new(PortDirection.Input, new Float(3, false), TANGENT, "Tangent"));

            var separator = new VisualElement();
            separator.style.height = 16;
            separator.style.backgroundColor = Color.clear;
            inputContainer.Add(separator);

            AddPort(new(PortDirection.Input, new Float(3, false), COLOR, "Color"));
            AddPort(new(PortDirection.Input, new Float(1, false), ALPHA, "Alpha"));
            AddPort(new(PortDirection.Input, new Float(1, false), CUTOFF, "Cutoff"));

            Bind(POSITION, PortBinding.PositionWS);
            Bind(NORMAL, PortBinding.NormalWS);
            Bind(TANGENT, PortBinding.TangentWS);
            DefaultValues[COLOR] = "float3(1.0, 1.0, 1.0)";
            DefaultValues[ALPHA] = "1.0";
            DefaultValues[CUTOFF] = "0.5";
        }

        static readonly PropertyDescriptor _surfaceOptionsStart = new(PropertyType.Float, "SurfaceOptions", "_SurfaceOptions") { customAttributes = "[Foldout]" };
        static readonly PropertyDescriptor _mode = new (PropertyType.Float, "Rendering Mode", "_Mode") { customAttributes = "[Enum(Opaque, 0, Cutout, 1, Fade, 2, Transparent, 3, Additive, 4, Multiply, 5)]" };
        static readonly PropertyDescriptor _srcBlend = new (PropertyType.Float, "Source Blend", "_SrcBlend") { FloatValue = 1, customAttributes = "[Enum(UnityEngine.Rendering.BlendMode)]" };
        static readonly PropertyDescriptor _dstBlend = new(PropertyType.Float, "Destination Blend", "_DstBlend") { FloatValue = 0, customAttributes = "[Enum(UnityEngine.Rendering.BlendMode)]" };
        static readonly PropertyDescriptor _zwrite = new(PropertyType.Float, "ZWrite", "_ZWrite") { FloatValue = 1, customAttributes = "[Enum(Off, 0, On, 1)]" };
        static readonly PropertyDescriptor _cull = new(PropertyType.Float, "Cull", "_Cull") { FloatValue = 2, customAttributes = "[Enum(UnityEngine.Rendering.CullMode)]" };
        static readonly PropertyDescriptor _properties = new(PropertyType.Float, "Properties", "_Properties") { customAttributes = "[Foldout]" };

        const string Vertex = "Packages/com.z3y.zsg/Editor/Targets/Unlit/Vertex.hlsl";
        const string FragmentForward = "Packages/com.z3y.zsg/Editor/Targets/Unlit/FragmentForward.hlsl";
        const string FragmentShadow = "Packages/com.z3y.zsg/Editor/Targets/Unlit/FragmentShadow.hlsl";

        public override void OnBeforeBuild(ShaderBuilder builder)
        {
            builder.properties.Add(_surfaceOptionsStart);
            builder.properties.Add(_mode);
            builder.properties.Add(_srcBlend);
            builder.properties.Add(_dstBlend);
            builder.properties.Add(_zwrite);
            builder.properties.Add(_cull);
            builder.properties.Add(_properties);


            builder.subshaderTags["RenderType"] = "Opaque";
            builder.subshaderTags["Queue"] = "Geometry";


            {
                var pass = new PassBuilder("FORWARD", Vertex, FragmentForward, POSITION, NORMAL, TANGENT, COLOR, ALPHA, CUTOFF);
                pass.tags["LightMode"] = "ForwardBase";

                pass.renderStates["Cull"] = "[_Cull]";
                pass.renderStates["ZWrite"] = "[_ZWrite]";
                pass.renderStates["Blend"] = "[_SrcBlend] [_DstBlend]";


                pass.pragmas.Add("#pragma shader_feature_local _ _ALPHAFADE_ON _ALPHATEST_ON _ALPHAPREMULTIPLY_ON _ALPHAMODULATE_ON");

                //pass.pragmas.Add("#pragma multi_compile_fwdbase");
                pass.pragmas.Add("#pragma multi_compile_fog");
                pass.pragmas.Add("#pragma multi_compile_instancing");

                pass.attributes.RequirePositionOS();
                pass.attributes.Require("UNITY_VERTEX_INPUT_INSTANCE_ID");

                pass.varyings.RequirePositionCS();
                pass.varyings.RequireCustomString("UNITY_FOG_COORDS(*)");
                //pass.varyings.RequireCustomString("#ifdef LIGHTMAP_ON\ncentroid float2 lightmapUV : LIGHTMAPUV\n#endif");
                pass.varyings.RequireCustomString("UNITY_VERTEX_INPUT_INSTANCE_ID");
                pass.varyings.RequireCustomString("UNITY_VERTEX_OUTPUT_STEREO");

                pass.pragmas.Add("#include \"Packages/com.z3y.zsg/ShaderLibrary/BuiltInLibrary.hlsl\"");
                builder.AddPass(pass);
            }

            {
                var pass = new PassBuilder("SHADOWCASTER", Vertex, FragmentShadow, POSITION, NORMAL, ALPHA, CUTOFF);
                pass.tags["LightMode"] = "ShadowCaster";
                pass.renderStates["ZWrite"] = "On";
                pass.renderStates["ZTest"] = "LEqual";
                pass.renderStates["Cull"] = "[_Cull]";

                pass.pragmas.Add("#pragma multi_compile_shadowcaster");
                pass.pragmas.Add("#pragma multi_compile_instancing");
                pass.pragmas.Add("#pragma shader_feature_local _ _ALPHAFADE_ON _ALPHATEST_ON _ALPHAPREMULTIPLY_ON _ALPHAMODULATE_ON");


                pass.attributes.RequirePositionOS();
                pass.attributes.Require("UNITY_VERTEX_INPUT_INSTANCE_ID");

                pass.varyings.RequirePositionCS();
                pass.varyings.RequireCustomString("UNITY_VERTEX_INPUT_INSTANCE_ID");
                pass.varyings.RequireCustomString("UNITY_VERTEX_OUTPUT_STEREO");

                pass.pragmas.Add("#include \"Packages/com.z3y.zsg/ShaderLibrary/BuiltInLibrary.hlsl\"");
                builder.AddPass(pass);
            }
        }
    }
}