using UnityEngine.UIElements;
using ZSG.Nodes.PortType;
using ZSG.Nodes;
using UnityEngine;
using UnityEditor;
using System;

namespace ZSG
{
    [NodeInfo("Targets/Lit Target"), Serializable]
    public class LitTemplate : TemplateOutput
    {
        [MenuItem("Assets/Create/ZSG/Lit Graph")]
        public static void CreateVariantFile() => ShaderGraphImporter.CreateEmptyTemplate<LitTemplate>();

        public override string Name { get; } = "Lit";
        public override int[] VertexPorts => new int[] { POSITION, NORMAL_VERTEX, TANGENT };
        public override int[] FragmentPorts => new int[] { ALBEDO, ALPHA, METALLIC, OCCLUSION, EMISSION, ROUGHNESS, NORMAL_TS, CUTOFF };

        const int POSITION = 0;
        const int NORMAL_VERTEX = 1;
        const int TANGENT = 2;
        const int ALBEDO = 3;
        const int ALPHA = 4;
        const int CUTOFF = 5;
        const int ROUGHNESS = 6;
        const int METALLIC = 7;
        const int OCCLUSION = 8;
        const int EMISSION = 9;
        const int NORMAL_TS = 10;
        public override void Initialize()
        {
            AddPort(new(PortDirection.Input, new Float(3, false), POSITION, "Position"));
            AddPort(new(PortDirection.Input, new Float(3, false), NORMAL_VERTEX, "Normal"));
            AddPort(new(PortDirection.Input, new Float(3, false), TANGENT, "Tangent"));

            var separator = new VisualElement();
            separator.style.height = 16;
            separator.style.backgroundColor = Color.clear;
            inputContainer.Add(separator);

            AddPort(new(PortDirection.Input, new Float(3, false), ALBEDO, "Albedo"));
            AddPort(new(PortDirection.Input, new Float(1, false), ALPHA, "Alpha"));

            AddPort(new(PortDirection.Input, new Float(1, false), ROUGHNESS, "Roughness"));
            AddPort(new(PortDirection.Input, new Float(1, false), METALLIC, "Metallic"));
            AddPort(new(PortDirection.Input, new Float(1, false), OCCLUSION, "Occlusion"));

            AddPort(new(PortDirection.Input, new Float(3, false), NORMAL_TS, "Normal"));
            AddPort(new(PortDirection.Input, new Float(3, false), EMISSION, "Emission"));


            AddPort(new(PortDirection.Input, new Float(1, false), CUTOFF, "Cutoff"));

            Bind(POSITION, PortBinding.PositionOS);
            Bind(NORMAL_VERTEX, PortBinding.NormalOS);
            Bind(TANGENT, PortBinding.TangentOS);
            DefaultValues[ALBEDO] = "float3(1.0, 1.0, 1.0)";
            DefaultValues[ALPHA] = "1.0";
            DefaultValues[CUTOFF] = "0.5";
            DefaultValues[ROUGHNESS] = "0.5";
            DefaultValues[METALLIC] = "0.0";
            DefaultValues[OCCLUSION] = "1.0";
            DefaultValues[EMISSION] = "float3(0.0, 0.0, 0.0)";
            DefaultValues[NORMAL_TS] = "float3(0.0, 0.0, 1.0)";
        }

        enum Shading
        {
            PBR,
            Flat
        }
        [SerializeField] Shading _shading = Shading.PBR;
        [SerializeField] bool _cbirp = false;

        public override void AdditionalElements(VisualElement root)
        {
            base.AdditionalElements(root);

            var shading = new EnumField("Shading", _shading);
            shading.RegisterValueChangedCallback(x => _shading = (Shading)x.newValue);
            root.Add(shading);

            if (_cbirpExists)
            {
                var cbirp = new Toggle("CBIRP") { value = _cbirp };
                cbirp.RegisterValueChangedCallback(x => _cbirp = x.newValue);
                root.Add(cbirp);
            }
        }

        static readonly PropertyDescriptor _surfaceOptionsStart = new(PropertyType.Float, "Surface Options", "_SurfaceOptions") { customAttributes = "[Foldout]" };
        static readonly PropertyDescriptor _mode = new (PropertyType.Float, "Rendering Mode", "_Mode") { customAttributes = "[Enum(Opaque, 0, Cutout, 1, Fade, 2, Transparent, 3, Additive, 4, Multiply, 5)]" };
        static readonly PropertyDescriptor _srcBlend = new (PropertyType.Float, "Source Blend", "_SrcBlend") { FloatValue = 1, customAttributes = "[Enum(UnityEngine.Rendering.BlendMode)]" };
        static readonly PropertyDescriptor _dstBlend = new(PropertyType.Float, "Destination Blend", "_DstBlend") { FloatValue = 0, customAttributes = "[Enum(UnityEngine.Rendering.BlendMode)]" };
        static readonly PropertyDescriptor _zwrite = new(PropertyType.Float, "ZWrite", "_ZWrite") { FloatValue = 1, customAttributes = "[Enum(Off, 0, On, 1)]" };
        static readonly PropertyDescriptor _cull = new(PropertyType.Float, "Cull", "_Cull") { FloatValue = 2, customAttributes = "[Enum(UnityEngine.Rendering.CullMode)]" };
        static readonly PropertyDescriptor _properties = new(PropertyType.Float, "Properties", "_Properties") { customAttributes = "[Foldout]" };
        static readonly PropertyDescriptor _monosh = new(PropertyType.Float, "Mono SH", "_MonoSH") { customAttributes = "[Toggle(_BAKERY_MONOSH)]" };
        static readonly PropertyDescriptor _bicubicLightmap = new(PropertyType.Float, "Bicubic Lightmap", "_BicubicLightmap") { customAttributes = "[Toggle(_BICUBIC_LIGHTMAP)]" };
        static readonly PropertyDescriptor _lmSpec = new(PropertyType.Float, "Lightmapped Specular", "_LightmappedSpecular") { customAttributes = "[Toggle(_LIGHTMAPPED_SPECULAR)]" };
        static readonly PropertyDescriptor _ltcgi = new(PropertyType.Float, "LTCGI", "_LTCGI") { customAttributes = "[Toggle(_LTCGI)]" };

        const string _ltcgiPath = "Packages/at.pimaker.ltcgi/Shaders/LTCGI.cginc";
        const string _cbirpPath = "Packages/z3y.clusteredbirp/Shaders/cbirp.hlsl";
        static bool _ltcgiExists = System.IO.File.Exists(_ltcgiPath);
        static bool _cbirpExists = System.IO.File.Exists(_cbirpPath);

        const string Vertex = "Packages/com.z3y.myshadergraph/Editor/Targets/Lit/Vertex.hlsl";
        const string FragmentForward = "Packages/com.z3y.myshadergraph/Editor/Targets/Lit/FragmentForward.hlsl";
        const string FragmentShadow = "Packages/com.z3y.myshadergraph/Editor/Targets/Lit/FragmentShadow.hlsl";
        const string FragmentMeta = "Packages/com.z3y.myshadergraph/Editor/Targets/Lit/FragmentMeta.hlsl";


        Texture2D _dfg = AssetDatabase.LoadAssetAtPath<Texture2D>("Packages/com.z3y.myshadergraph/Editor/Targets/Lit/dfg-multiscatter.exr");
        static readonly PropertyDescriptor _dfgProperty = new(PropertyType.Texture2D, "", "_DFG")
            { defaultAttributes = MaterialPropertyAttribute.HideInInspector | MaterialPropertyAttribute.NonModifiableTextureData };

        public override void OnBeforeBuild(ShaderBuilder builder)
        {
            builder.dependencies.Add(_ltcgiPath);
            builder.dependencies.Add(_cbirpPath);

            builder.properties.Add(_surfaceOptionsStart);
            builder.properties.Add(_mode);
            builder.properties.Add(_srcBlend);
            builder.properties.Add(_dstBlend);
            builder.properties.Add(_zwrite);
            builder.properties.Add(_cull);
            builder.properties.Add(_dfgProperty);

            builder.properties.Add(_monosh);
            builder.properties.Add(_bicubicLightmap);
            builder.properties.Add(_lmSpec);

            if (_ltcgiExists && builder.BuildTarget != BuildTarget.Android)
            {
                builder.properties.Add(_ltcgi);
                builder.subshaderTags["LTCGI"] = "_LTCGI";
            }

            builder._defaultTextures["_DFG"] = _dfg;
            builder.properties.Add(_properties);

            builder.subshaderTags["RenderType"] = "Opaque";
            builder.subshaderTags["Queue"] = "Geometry";

            {
                var pass = new PassBuilder("FORWARD", Vertex, FragmentForward, POSITION, NORMAL_VERTEX, TANGENT, ALBEDO, ALPHA, CUTOFF, ROUGHNESS, METALLIC, OCCLUSION, EMISSION, NORMAL_TS );
                pass.tags["LightMode"] = "ForwardBase";

                pass.renderStates["Cull"] = "[_Cull]";
                pass.renderStates["ZWrite"] = "[_ZWrite]";
                pass.renderStates["Blend"] = "[_SrcBlend] [_DstBlend]";


                pass.pragmas.Add("#pragma shader_feature_local _ _ALPHAFADE_ON _ALPHATEST_ON _ALPHAPREMULTIPLY_ON _ALPHAMODULATE_ON");

                pass.pragmas.Add("#pragma multi_compile_fwdbase");
                pass.pragmas.Add("#pragma skip_variants LIGHTPROBE_SH");
                pass.pragmas.Add("#pragma multi_compile_fog");
                pass.pragmas.Add("#pragma multi_compile_instancing");

                pass.pragmas.Add("#pragma shader_feature_local _BAKERY_MONOSH");
                pass.pragmas.Add("#pragma shader_feature_local _BICUBIC_LIGHTMAP");
                pass.pragmas.Add("#pragma shader_feature_local _LIGHTMAPPED_SPECULAR");

                if (_shading == Shading.Flat)
                {
                    pass.pragmas.Add("#define _FLATSHADING");
                    pass.pragmas.Add("#pragma skip_variants SHADOWS_SCREEN");
                }
                if (_cbirpExists && _cbirp)
                {
                    pass.pragmas.Add("#define _CBIRP");
                }
                if (_ltcgiExists && builder.BuildTarget != BuildTarget.Android) pass.pragmas.Add("#pragma shader_feature_local_fragment _LTCGI");

                pass.attributes.RequirePositionOS();
                pass.attributes.Require("UNITY_VERTEX_INPUT_INSTANCE_ID");

                pass.varyings.RequirePositionCS();
                pass.attributes.RequireUV(1, 2);
                PortBindings.Require(pass, ShaderStage.Fragment, PortBinding.PositionWS);
                PortBindings.Require(pass, ShaderStage.Fragment, PortBinding.BitangentWS);
                PortBindings.Require(pass, ShaderStage.Fragment, PortBinding.TangentWS);
                PortBindings.Require(pass, ShaderStage.Fragment, PortBinding.PositionWS);

                pass.varyings.RequireCustomString("UNITY_FOG_COORDS(*)");
                pass.varyings.RequireCustomString("UNITY_SHADOW_COORDS(*)");
                pass.varyings.RequireCustomString("#if !UNITY_SAMPLE_FULL_SH_PER_PIXEL\nfloat3 sh : TEXCOORD*;\n#endif");
                pass.varyings.RequireCustomString("#ifdef LIGHTMAP_ON\ncentroid float2 lightmapUV : LIGHTMAPUV;\n#endif");
                pass.varyings.RequireCustomString("UNITY_VERTEX_INPUT_INSTANCE_ID");
                pass.varyings.RequireCustomString("UNITY_VERTEX_OUTPUT_STEREO");

                pass.pragmas.Add("#include \"Packages/com.z3y.myshadergraph/ShaderLibrary/BuiltInLibrary.hlsl\"");
                builder.AddPass(pass);
            }
            {
                var pass = new PassBuilder("FORWARD_DELTA", Vertex, FragmentForward, POSITION, NORMAL_VERTEX, TANGENT, ALBEDO, ALPHA, CUTOFF, ROUGHNESS, METALLIC, OCCLUSION, EMISSION, NORMAL_TS);
                pass.tags["LightMode"] = "ForwardAdd";

                pass.renderStates["Fog"] = "{ Color (0,0,0,0) }";
                pass.renderStates["Cull"] = "[_Cull]";
                pass.renderStates["Blend"] = "[_SrcBlend] One";
                pass.renderStates["ZWrite"] = "Off";
                pass.renderStates["ZTest"] = "LEqual";



                pass.pragmas.Add("#pragma shader_feature_local _ _ALPHAFADE_ON _ALPHATEST_ON _ALPHAPREMULTIPLY_ON _ALPHAMODULATE_ON");

                pass.pragmas.Add("#pragma multi_compile_fwdadd_fullshadows");
                pass.pragmas.Add("#pragma multi_compile_fog");
                pass.pragmas.Add("#pragma multi_compile_instancing");

                pass.attributes.RequirePositionOS();
                pass.attributes.Require("UNITY_VERTEX_INPUT_INSTANCE_ID");

                pass.varyings.RequirePositionCS();
                PortBindings.Require(pass, ShaderStage.Fragment, PortBinding.PositionWS);
                PortBindings.Require(pass, ShaderStage.Fragment, PortBinding.BitangentWS);
                PortBindings.Require(pass, ShaderStage.Fragment, PortBinding.TangentWS);
                PortBindings.Require(pass, ShaderStage.Fragment, PortBinding.PositionWS);

                pass.varyings.RequireCustomString("UNITY_FOG_COORDS(*)");
                pass.varyings.RequireCustomString("UNITY_SHADOW_COORDS(*)");
                pass.varyings.RequireCustomString("UNITY_VERTEX_INPUT_INSTANCE_ID");
                pass.varyings.RequireCustomString("UNITY_VERTEX_OUTPUT_STEREO");

                pass.pragmas.Add("#include \"Packages/com.z3y.myshadergraph/ShaderLibrary/BuiltInLibrary.hlsl\"");
                if (!(_cbirpExists && _cbirp))
                {
                    builder.AddPass(pass);
                }
            }

            {
                var pass = new PassBuilder("SHADOWCASTER", Vertex, FragmentShadow, POSITION, NORMAL_VERTEX, ALPHA, CUTOFF);
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

                pass.pragmas.Add("#include \"Packages/com.z3y.myshadergraph/ShaderLibrary/BuiltInLibrary.hlsl\"");
                builder.AddPass(pass);
            }
            {
                var pass = new PassBuilder("META", Vertex, FragmentMeta, ALPHA, CUTOFF, ALBEDO, METALLIC, ROUGHNESS, EMISSION);
                pass.tags["LightMode"] = "Meta";
                pass.renderStates["Cull"] = "Off";


                pass.pragmas.Add("#pragma shader_feature EDITOR_VISUALIZATION");
                pass.pragmas.Add("#pragma shader_feature_local _ _ALPHAFADE_ON _ALPHATEST_ON _ALPHAPREMULTIPLY_ON _ALPHAMODULATE_ON");

                pass.attributes.RequireUV(0, 2);
                pass.attributes.RequireUV(1, 2);
                pass.attributes.RequireUV(2, 2);

                pass.attributes.RequirePositionOS();
                pass.varyings.RequirePositionCS();

                pass.varyings.RequireCustomString("#ifdef EDITOR_VISUALIZATION\nfloat2 vizUV : TEXCOORD*;\n#endif");
                pass.varyings.RequireCustomString("#ifdef EDITOR_VISUALIZATION\nfloat4 lightCoord : TEXCOORD*;\n#endif");

                pass.varyings.RequireCustomString("UNITY_VERTEX_INPUT_INSTANCE_ID");
                pass.varyings.RequireCustomString("UNITY_VERTEX_OUTPUT_STEREO");

                pass.pragmas.Add("#include \"Packages/com.z3y.myshadergraph/ShaderLibrary/BuiltInLibrary.hlsl\"");
                pass.pragmas.Add("#include \"UnityMetaPass.cginc\"");

                builder.AddPass(pass);
            }
        }
    }
}