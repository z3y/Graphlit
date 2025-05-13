using UnityEngine.UIElements;
using Graphlit.Nodes.PortType;
using Graphlit.Nodes;
using UnityEngine;
using UnityEditor;
using System;
using System.Collections.Generic;

namespace Graphlit
{
    [NodeInfo("Targets/Lit Target"), Serializable]
    public class LitTemplate : TemplateOutput
    {
        [MenuItem("Assets/Create/Graphlit/Lit Graph")]
        public static void CreateVariantFile() => GraphlitImporter.CreateEmptyTemplate<LitTemplate>();

        [MenuItem("Assets/Create/Graphlit/Toon Graph")]
        public static void CreateToonVariant()
        {
            const string samplePath = "Packages/com.z3y.graphlit/Shaders/Toon.graphlit";
            var graph = GraphlitImporter.ReadGraphData(AssetDatabase.AssetPathToGUID(samplePath));
            graph.data.shaderName = "Default Shader";

            var jsonData = EditorJsonUtility.ToJson(graph, true);
            ProjectWindowUtil.CreateAssetWithContent($"New Shader Graph.graphlit", jsonData);
        }

        [MenuItem("Assets/Create/Graphlit/Custom Graph")]
        public static void CreateCustomVariant()
        {
            const string samplePath = "Packages/com.z3y.graphlit/Shaders/Custom.graphlit";
            var graph = GraphlitImporter.ReadGraphData(AssetDatabase.AssetPathToGUID(samplePath));
            graph.data.shaderName = "Default Shader";

            var jsonData = EditorJsonUtility.ToJson(graph, true);
            ProjectWindowUtil.CreateAssetWithContent($"New Shader Graph.graphlit", jsonData);
        }


        public override string Name { get; } = "Lit";
        public override int[] VertexPorts => new int[] { POSITION, NORMAL_VERTEX, TANGENT };
        public override int[] FragmentPorts => new int[] { ALBEDO, ALPHA, METALLIC, OCCLUSION, EMISSION, ROUGHNESS, REFLECTANCE, NORMAL_TS, CUTOFF };

        public override string TemplateGUID => "131fe11a59ae68b498c21549d0ebdd85";

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
        const int REFLECTANCE = 11;

        public override void Initialize()
        {
            inputContainer.Clear();
            AddPort(new(PortDirection.Input, new Float(3, false), POSITION, "Position"));
            AddPort(new(PortDirection.Input, new Float(3, false), NORMAL_VERTEX, "Normal"));
            AddPort(new(PortDirection.Input, new Float(3, false), TANGENT, "Tangent"));

            var separator = new VisualElement();
            separator.style.height = 16;
            separator.style.backgroundColor = Color.clear;
            inputContainer.Add(separator);

            AddPort(new(PortDirection.Input, new Float(3, false), ALBEDO, "Albedo"));
            AddPort(new(PortDirection.Input, new Float(1, false), ALPHA, "Alpha"));

            AddPort(new(PortDirection.Input, new Float(1, false), OCCLUSION, "Occlusion"));
            AddPort(new(PortDirection.Input, new Float(1, false), ROUGHNESS, "Roughness"));
            AddPort(new(PortDirection.Input, new Float(1, false), METALLIC, "Metallic"));
            AddPort(new(PortDirection.Input, new Float(1, false), REFLECTANCE, "Reflectance"));

            AddPort(new(PortDirection.Input, new Float(3, false), NORMAL_TS, "Normal"));
            AddPort(new(PortDirection.Input, new Float(3, false), EMISSION, "Emission"));


            AddPort(new(PortDirection.Input, new Float(1, false), CUTOFF, "Cutoff"));
            inputContainer.style.paddingBottom = 8;

            Bind(POSITION, PortBinding.PositionWS);
            Bind(NORMAL_VERTEX, PortBinding.NormalWS);
            Bind(TANGENT, PortBinding.TangentWS);
            DefaultValues[ALBEDO] = "float3(1.0, 1.0, 1.0)";
            DefaultValues[ALPHA] = "1.0";
            DefaultValues[CUTOFF] = "0.5";
            DefaultValues[ROUGHNESS] = "0.5";
            DefaultValues[REFLECTANCE] = "0.5";
            DefaultValues[METALLIC] = "0.0";
            DefaultValues[OCCLUSION] = "1.0";
            DefaultValues[EMISSION] = "float3(0.0, 0.0, 0.0)";
            DefaultValues[NORMAL_TS] = "float3(0.0, 0.0, 1.0)";
        }


        [SerializeField] bool _cbirp = false;
        [SerializeField] bool _cbirpReflections = false;
        [SerializeField] bool _specular = true;
        [SerializeField] bool _fwdAddBlendOpMax = false;

        enum Normal
        {
            Tangent = 0,
            World = 1,
            Object = 2,
        }
        [SerializeField] Normal _normal = Normal.Tangent;
        string NormalDropoffDefine()
        {
            return _normal switch
            {
                Normal.World => "#define _NORMAL_DROPOFF_WS",
                Normal.Object => "#define _NORMAL_DROPOFF_OS",
                Normal.Tangent or _ => "#define _NORMAL_DROPOFF_TS",
            };
        }

        public override void AdditionalElements(VisualElement root)
        {
            base.AdditionalElements(root);

            var normal = new EnumField("Normal", _normal);
            normal.RegisterValueChangedCallback(x => _normal = (Normal)x.newValue);
            root.Add(normal);

            if (_cbirpExists)
            {
                var cbirp = new Toggle("CBIRP") { value = _cbirp };
                cbirp.RegisterValueChangedCallback(x => _cbirp = x.newValue);
                root.Add(cbirp);

                var cbirpReflections = new Toggle("CBIRP Reflections") { value = _cbirpReflections };
                cbirpReflections.RegisterValueChangedCallback(x => _cbirpReflections = x.newValue);
                root.Add(cbirpReflections);
            }
            var spec = new Toggle("Specular") { value = _specular };
            spec.RegisterValueChangedCallback(x => _specular = x.newValue);
            root.Add(spec);
        }

        static readonly PropertyDescriptor _monosh = new(PropertyType.Float, "Mono SH", "_MonoSH") { customAttributes = "[Toggle(_BAKERY_MONOSH)]" };
        static readonly PropertyDescriptor _bicubicLightmap = new(PropertyType.Float, "Bicubic Lightmap", "_BicubicLightmap") { customAttributes = "[Toggle(_BICUBIC_LIGHTMAP)]" };
        static readonly PropertyDescriptor _nonLinearLightprobeSh = new(PropertyType.Float, "Non Linear Light Probe SH", "_NonLinearLightProbeSH") { customAttributes = "[Toggle(_NONLINEAR_LIGHTPROBESH)]" };
        static readonly PropertyDescriptor _specularHighlights = new(PropertyType.Float, "Specular Highlights", "_SpecularHighlights") { customAttributes = "[ToggleOff]", FloatValue = 1 };
        static readonly PropertyDescriptor _glossyReflections = new(PropertyType.Float, "Environment Reflections", "_GlossyReflections") { customAttributes = "[ToggleOff]", FloatValue = 1 };
        static readonly PropertyDescriptor _specularOcclusion = new(PropertyType.Float, "Specular Occlusion", "_SpecularOcclusion") { FloatValue = 1, Range = new Vector2(0, 5)};


        static readonly PropertyDescriptor _lmSpec = new(PropertyType.Float, "Lightmapped Specular", "_LightmappedSpecular") { customAttributes = "[Toggle(_LIGHTMAPPED_SPECULAR)]" };
        static readonly PropertyDescriptor _ltcgi = new(PropertyType.Float, "LTCGI", "_LTCGI") { customAttributes = "[Toggle(_LTCGI)]" };

        const string _ltcgiPath = "Packages/at.pimaker.ltcgi/Shaders/LTCGI.cginc";
        const string _cbirpPath = "Packages/z3y.clusteredbirp/Shaders/cbirp.hlsl";
        static bool _ltcgiExists = System.IO.File.Exists(_ltcgiPath);
        static bool _cbirpExists = System.IO.File.Exists(_cbirpPath);

        const string Vertex = "Packages/com.z3y.graphlit/ShaderLibrary/Vertex.hlsl";
        const string FragmentForward = "Packages/com.z3y.graphlit/ShaderLibrary/FragmentForwardPBR.hlsl";
        const string FragmentShadow = "Packages/com.z3y.graphlit/ShaderLibrary/FragmentShadow.hlsl";
        const string FragmentMeta = "Packages/com.z3y.graphlit/ShaderLibrary/FragmentMeta.hlsl";
        const string FragmentDepth = "Packages/com.z3y.graphlit/ShaderLibrary/FragmentDepth.hlsl";
        const string FragmentDepthNormals = "Packages/com.z3y.graphlit/ShaderLibrary/FragmentDepthNormals.hlsl";


        public override void OnBeforeBuild(ShaderBuilder builder)
        {
            builder.dependencies.Add(_ltcgiPath);
            builder.dependencies.Add(_cbirpPath);

            builder.properties.Add(_surfaceBlend);
            builder.properties.Add(_alphaClip);
            builder.properties.Add(_alphaToMask);
            builder.properties.Add(_blendModePreserveSpecular);
            builder.properties.Add(_surfaceOptions);
            builder.properties.Add(_mode);
            builder.properties.Add(_srcBlend);
            builder.properties.Add(_dstBlend);
            builder.properties.Add(_zwrite);
            builder.properties.Add(_cull);
            //if (GraphView.graphData.outlinePass != GraphData.OutlinePassMode.Disabled) builder.properties.Add(_outlineToggle);
            builder.properties.Add(_dfgProperty);

            builder.properties.Add(_monosh);
            builder.properties.Add(_bicubicLightmap);
            builder.properties.Add(_lmSpec);
            builder.properties.Add(_nonLinearLightprobeSh);


            if (_specular)
            {
                builder.properties.Add(_specularHighlights);
                builder.properties.Add(_glossyReflections);
                builder.properties.Add(_specularOcclusion);
            }


            if (_ltcgiExists && builder.BuildTarget != BuildTarget.Android)
            {
                builder.properties.Add(_ltcgi);
                builder.subshaderTags["LTCGI"] = "_LTCGI";
            }

            builder._defaultTextures["_DFG"] = _dfg;

            builder.subshaderTags["RenderType"] = "Opaque";
            builder.subshaderTags["Queue"] = "Geometry";

            bool urp = GetRenderPipeline() == RenderPipeline.URP;

            if (urp)
            {
                builder.subshaderTags["UniversalMaterialType"] = "Lit";
            }


            {
                var portFlags = new List<int>() { POSITION, NORMAL_VERTEX, TANGENT, ALBEDO, ALPHA, CUTOFF, ROUGHNESS, METALLIC, OCCLUSION, REFLECTANCE, EMISSION, NORMAL_TS };
                var pass = new PassBuilder("Forward", Vertex, FragmentForward, portFlags.ToArray());
                pass.tags["LightMode"] = urp ? "UniversalForward" : "ForwardBase";

                pass.renderStates["Cull"] = "[_Cull]";
                pass.renderStates["ZWrite"] = "[_ZWrite]";
                pass.renderStates["Blend"] = "[_SrcBlend] [_DstBlend]";


                pass.pragmas.Add("#pragma shader_feature_local_fragment _SURFACE_TYPE_TRANSPARENT");
                pass.pragmas.Add("#pragma shader_feature_local_fragment _ALPHATEST_ON");
                pass.pragmas.Add("#pragma shader_feature_local_fragment _ _ALPHAPREMULTIPLY_ON _ALPHAMODULATE_ON");


                if (urp)
                {
                    AddURPLightingPragmas(pass);
                    pass.pragmas.Add("#pragma instancing_options renderinglayer");
                }
                else
                {
                    pass.pragmas.Add("#pragma multi_compile_fwdbase");
                    pass.pragmas.Add("#pragma shader_feature_fragment VERTEXLIGHT_ON");
                }

                //pass.pragmas.Add("#pragma skip_variants LIGHTPROBE_SH");
                pass.pragmas.Add("#pragma multi_compile_fog");
                pass.pragmas.Add("#pragma multi_compile_instancing");

                pass.pragmas.Add("#pragma shader_feature_local _BAKERY_MONOSH");
                pass.pragmas.Add("#pragma shader_feature_local _BICUBIC_LIGHTMAP");
                pass.pragmas.Add("#pragma shader_feature_local _LIGHTMAPPED_SPECULAR");
                pass.pragmas.Add("#pragma shader_feature_local _NONLINEAR_LIGHTPROBESH");


                if (!_specular)
                {
                    pass.pragmas.Add("#define _SPECULARHIGHLIGHTS_OFF");
                    pass.pragmas.Add("#define _GLOSSYREFLECTIONS_OFF");
                }
                else
                {
                    pass.pragmas.Add("#pragma shader_feature_local _SPECULARHIGHLIGHTS_OFF");
                    pass.pragmas.Add("#pragma shader_feature_local _GLOSSYREFLECTIONS_OFF");
                }
                if (_cbirpExists && _cbirp)
                {
                    pass.pragmas.Add("#define _CBIRP");
                    if (_cbirpReflections)
                    {
                        pass.pragmas.Add("#define _CBIRP_REFLECTIONS");
                    }
                }

                pass.pragmas.Add(NormalDropoffDefine());

                if (_ltcgiExists && builder.BuildTarget != BuildTarget.Android) pass.pragmas.Add("#pragma shader_feature_local_fragment _LTCGI");


                pass.attributes.RequirePositionOS();
                pass.attributes.Require("UNITY_VERTEX_INPUT_INSTANCE_ID");

                pass.varyings.RequirePositionCS();
                pass.attributes.RequireUV(1, 2);
                PortBindings.Require(pass, ShaderStage.Fragment, PortBinding.PositionWS);
                PortBindings.Require(pass, ShaderStage.Fragment, PortBinding.BitangentWS);
                PortBindings.Require(pass, ShaderStage.Fragment, PortBinding.TangentWS);
                PortBindings.Require(pass, ShaderStage.Fragment, PortBinding.PositionWS);

                pass.varyings.RequireCustomString("#ifdef LIGHTMAP_ON\ncentroid float2 lightmapUV : LIGHTMAP;\n#endif");
                pass.varyings.RequireCustomString("UNITY_VERTEX_INPUT_INSTANCE_ID");
                pass.varyings.RequireCustomString("UNITY_VERTEX_OUTPUT_STEREO");

                pass.pragmas.Add("#include \"Packages/com.z3y.graphlit/ShaderLibrary/Core.hlsl\"");

                pass.properties.Add(_specularOcclusion);
                builder.AddPass(pass);

            }

            /*if (!urp)
            {
                var portFlags = new List<int>() { POSITION, NORMAL_VERTEX, TANGENT, ALBEDO, ALPHA, CUTOFF, ROUGHNESS, METALLIC, OCCLUSION, REFLECTANCE, NORMAL_TS };
                var pass = new PassBuilder("FORWARD_DELTA", Vertex, FragmentForward, portFlags.ToArray());
                pass.tags["LightMode"] = "ForwardAdd";

                pass.renderStates["Fog"] = "{ Color (0,0,0,0) }";
                pass.renderStates["Cull"] = "[_Cull]";
                pass.renderStates["Blend"] = "[_SrcBlend] One";
                pass.renderStates["ZWrite"] = "Off";
                pass.renderStates["ZTest"] = "LEqual";

                if (_fwdAddBlendOpMax)
                {
                    //pass.renderStates["BlendOp"] = "Max, Add";
                }

                pass.pragmas.Add("#pragma shader_feature_local _ _ALPHAFADE_ON _ALPHATEST_ON _ALPHAPREMULTIPLY_ON _ALPHAMODULATE_ON");

                pass.pragmas.Add("#pragma multi_compile_fwdadd_fullshadows");
                pass.pragmas.Add("#pragma multi_compile_fog");
                pass.pragmas.Add("#pragma multi_compile_instancing");

                if (!_specular)
                {
                    pass.pragmas.Add("#define _SPECULARHIGHLIGHTS_OFF");
                }
                else
                {
                    pass.pragmas.Add("#pragma shader_feature_local _SPECULARHIGHLIGHTS_OFF");
                }

                pass.pragmas.Add(NormalDropoffDefine());

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

                pass.preincludes.Add("Packages/com.z3y.graphlit/Editor/Targets/Lit/Functions.hlsl");
                pass.properties.Add(_specularOcclusion);

                pass.pragmas.Add("#include \"Packages/com.z3y.graphlit/ShaderLibrary/BuiltInLibrary.hlsl\"");
                if (!(_cbirpExists && _cbirp))
                {
                    builder.AddPass(pass);
                }
            }*/
            if (urp)
            {
                {
                    var pass = new PassBuilder("DepthOnly", Vertex, FragmentDepth, NORMAL_VERTEX, TANGENT, POSITION, ALPHA, CUTOFF);
                    CreateUniversalDepthPass(pass);
                    builder.AddPass(pass);
                }
                {
                    var pass = new PassBuilder("DepthNormals", Vertex, FragmentDepthNormals, POSITION, NORMAL_VERTEX, TANGENT, ALPHA, CUTOFF, NORMAL_TS);
                    CreateUniversalDepthNormalsPass(pass);
                    builder.AddPass(pass);
                }
            }
            {
                var pass = new PassBuilder("ShadowCaster", Vertex, FragmentShadow, POSITION, NORMAL_VERTEX, TANGENT, ALPHA, CUTOFF);
                CreateShadowCaster(pass, urp);
                builder.AddPass(pass);
            }
            {
                var pass = new PassBuilder("Meta", Vertex, FragmentMeta, ALPHA, CUTOFF, ALBEDO, METALLIC, ROUGHNESS, EMISSION);
                pass.tags["LightMode"] = "Meta";
                pass.renderStates["Cull"] = "Off";

                pass.pragmas.Add("#pragma shader_feature EDITOR_VISUALIZATION");

                pass.pragmas.Add("#pragma shader_feature_local_fragment _SURFACE_TYPE_TRANSPARENT");
                pass.pragmas.Add("#pragma shader_feature_local_fragment _ALPHATEST_ON");
                pass.pragmas.Add("#pragma shader_feature_local_fragment _ _ALPHAPREMULTIPLY_ON _ALPHAMODULATE_ON");

                pass.attributes.RequireUV(0, 2);
                pass.attributes.RequireUV(1, 2);
                pass.attributes.RequireUV(2, 2);

                pass.attributes.RequirePositionOS();
                pass.varyings.RequirePositionCS();

                pass.varyings.RequireCustomString("#ifdef EDITOR_VISUALIZATION\nfloat2 VizUV : TEXCOORD*;\n#endif");
                pass.varyings.RequireCustomString("#ifdef EDITOR_VISUALIZATION\nfloat4 LightCoord : TEXCOORD*;\n#endif");

                pass.varyings.RequireCustomString("UNITY_VERTEX_INPUT_INSTANCE_ID");
                pass.varyings.RequireCustomString("UNITY_VERTEX_OUTPUT_STEREO");

                pass.properties.Add(_specularOcclusion);

                pass.pragmas.Add("#include \"Packages/com.z3y.graphlit/ShaderLibrary/Core.hlsl\"");
                pass.pragmas.Add("#include \"Packages/com.z3y.graphlit/ShaderLibrary/MetaFragment.hlsl\"");

                builder.AddPass(pass);
            }
        }
    }
}