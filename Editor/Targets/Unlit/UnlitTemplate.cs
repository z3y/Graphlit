using UnityEngine.UIElements;
using Graphlit.Nodes.PortType;
using Graphlit.Nodes;
using UnityEngine;
using UnityEditor;
using System;

namespace Graphlit
{
    [NodeInfo("Targets/Unlit Target"), Serializable]
    public class UnlitTemplate : TemplateOutput
    {
        [MenuItem("Assets/Create/Graphlit/Unlit Graph")]
        public static void CreateVariantFile() => GraphlitImporter.CreateEmptyTemplate(new UnlitTemplate(),
            x => x.graphData.vrcFallbackTags.type = VRCFallbackTags.ShaderType.Unlit);

        public override string Name { get; } = "Unlit";
        public override int[] VertexPorts => new int[] { POSITION, NORMAL, TANGENT };
        public override int[] FragmentPorts => new int[] { COLOR, ALPHA, CUTOFF };
        public override string TemplateGUID => "935ee03eb1b484d47906dc3d36fa00db";

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
            inputContainer.style.paddingBottom = 8;

            Bind(POSITION, PortBinding.PositionWS);
            Bind(NORMAL, PortBinding.NormalWS);
            Bind(TANGENT, PortBinding.TangentWS);
            DefaultValues[COLOR] = "float3(1.0, 1.0, 1.0)";
            DefaultValues[ALPHA] = "1.0";
            DefaultValues[CUTOFF] = "0.5";
        }

        [SerializeField] bool _customLighting = false;
        public override void AdditionalElements(VisualElement root)
        {
            base.AdditionalElements(root);

            var toggle1 = new Toggle("Custom Lighting") { value = _customLighting, tooltip = "Enable to add all the required keywords and passes for full lighting inside the unlit shader" };
            toggle1.RegisterValueChangedCallback(x => _customLighting = x.newValue);
            root.Add(toggle1);
        }

        const string Vertex = "Packages/com.z3y.graphlit/ShaderLibrary/Vertex.hlsl";
        const string FragmentForward = "Packages/com.z3y.graphlit/ShaderLibrary/FragmentForward.hlsl";
        const string FragmentShadow = "Packages/com.z3y.graphlit/ShaderLibrary/FragmentShadow.hlsl";
        const string FragmentDepth = "Packages/com.z3y.graphlit/ShaderLibrary/FragmentDepth.hlsl";
        const string FragmentDepthNormals = "Packages/com.z3y.graphlit/ShaderLibrary/FragmentDepthNormals.hlsl";

        const string _vrcLightVolumesPath = "Packages/red.sim.lightvolumes/Shaders/LightVolumes.cginc";
        bool _lightVolumesExists = System.IO.File.Exists(_vrcLightVolumesPath);

        public override void OnBeforeBuild(ShaderBuilder builder)
        {
            AddTerrainTag(builder);
            builder.dependencies.Add(_graphlitConfigPath);

            builder.dependencies.Add(_vrcLightVolumesPath);

            builder.properties.Add(_surfaceOptions);
            builder.properties.Add(_surfaceBlend);
            builder.properties.Add(_blendModePreserveSpecular);
            builder.properties.Add(_transClipping);
            builder.properties.Add(_alphaClip);
            builder.properties.Add(_alphaToMask);

            builder.properties.Add(_queueOffset);
            //builder.properties.Add(_blendModePreserveSpecular);

            builder.properties.Add(_mode);
            builder.properties.Add(_srcBlend);
            builder.properties.Add(_dstBlend);
            builder.properties.Add(_zwrite);
            builder.properties.Add(_ztest);

            builder.properties.Add(_cull);

            if (forceNoShadowCasting)
            {
                builder.subshaderTags["ForceNoShadowCasting"] = "True";
            }


            builder.properties.Add(_dfgProperty);
            builder._defaultTextures["_DFG"] = _dfg;

            //if (GraphView.graphData.outlinePass != GraphData.OutlinePassMode.Disabled) builder.properties.Add(_outlineToggle);

            builder.subshaderTags["RenderType"] = "Opaque";
            builder.subshaderTags["Queue"] = "Geometry";

            bool urp = GetRenderPipeline() == RenderPipeline.URP;

            if (urp)
            {
                builder.subshaderTags["UniversalMaterialType"] = "Lit";
            }

            {
                var pass = new PassBuilder("Forward", Vertex, FragmentForward, POSITION, NORMAL, TANGENT, COLOR, ALPHA, CUTOFF);
                pass.tags["LightMode"] = urp ? "UniversalForward" : "ForwardBase";

                pass.renderStates["Cull"] = "[_Cull]";
                pass.renderStates["ZWrite"] = "[_ZWrite]";
                pass.renderStates["Blend"] = "[_SrcBlend] [_DstBlend]";
                pass.renderStates["ZTest"] = "[_ZTest]";
                pass.renderStates["AlphaToMask"] = "[_AlphaToMask]";
                
                pass.pragmas.Add("#pragma shader_feature_local_fragment _SURFACE_TYPE_TRANSPARENT");
                pass.pragmas.Add("#pragma shader_feature_local_fragment _ALPHATEST_ON");
                pass.pragmas.Add("#pragma shader_feature_local_fragment _ _ALPHAPREMULTIPLY_ON _ALPHAMODULATE_ON");


                if (_customLighting)
                {
                    if (urp)
                    {
                        AddURPLightingPragmas(pass);
                    }
                    else
                    {
                        pass.pragmas.Add("#pragma multi_compile_fwdbase");
                        pass.pragmas.Add("#pragma shader_feature_fragment VERTEXLIGHT_ON");
                    }
                }
                pass.pragmas.Add("#pragma multi_compile_fog");
                pass.pragmas.Add("#pragma multi_compile_instancing");
                if (urp)
                {
                    pass.pragmas.Add("#pragma instancing_options renderinglayer");
                }

                pass.attributes.RequirePositionOS();
                pass.attributes.Require("UNITY_VERTEX_INPUT_INSTANCE_ID");

                pass.varyings.RequirePositionCS();
                PortBindings.Require(pass, ShaderStage.Fragment, PortBinding.PositionWS);

                if (_customLighting)
                {
                    pass.attributes.RequireUV(1, 2);
                    pass.varyings.RequireCustomString("#if defined(LIGHTMAP_ON) || defined(DYNAMICLIGHTMAP_ON) || defined(SHADOWS_SHADOWMASK)\ncentroid LIGHTMAP_COORD lightmapUV : LIGHTMAP;\n#endif");
                }
                pass.varyings.RequireCustomString("UNITY_VERTEX_INPUT_INSTANCE_ID");
                pass.varyings.RequireCustomString("UNITY_VERTEX_OUTPUT_STEREO");



                if (_customLighting && _lightVolumesExists)
                {
                    pass.pragmas.Add("#define _VRC_LIGHTVOLUMES");
                }

                IncludeConfig(pass);
                pass.pragmas.Add("#include \"Packages/com.z3y.graphlit/ShaderLibrary/Core.hlsl\"");
                builder.AddPass(pass);
            }

            if (_customLighting && !urp)
            {
                var pass = new PassBuilder("ForwardAdd", Vertex, FragmentForward, POSITION, NORMAL, TANGENT, COLOR, ALPHA, CUTOFF);
                pass.tags["LightMode"] = "ForwardAdd";
                TerrainPass(pass);
                pass.renderStates["Fog"] = "{ Color (0,0,0,0) }";
                pass.renderStates["Cull"] = "[_Cull]";
                pass.renderStates["Blend"] = "[_SrcBlend] One";
                pass.renderStates["ZWrite"] = "Off";
                pass.renderStates["ZTest"] = "[_ZTest]";
                pass.renderStates["AlphaToMask"] = "[_AlphaToMask]";

                pass.pragmas.Add("#pragma shader_feature_local_fragment _SURFACE_TYPE_TRANSPARENT");
                pass.pragmas.Add("#pragma shader_feature_local_fragment _ALPHATEST_ON");
                pass.pragmas.Add("#pragma shader_feature_local_fragment _ _ALPHAPREMULTIPLY_ON _ALPHAMODULATE_ON");

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

                if (_customLighting)
                {
                    pass.attributes.RequireUV(1, 2);
                    pass.varyings.RequireCustomString("#if defined(LIGHTMAP_ON) || defined(DYNAMICLIGHTMAP_ON) || defined(SHADOWS_SHADOWMASK)\ncentroid LIGHTMAP_COORD lightmapUV : LIGHTMAP;\n#endif");
                }

                pass.varyings.RequireCustomString("UNITY_VERTEX_INPUT_INSTANCE_ID");
                pass.varyings.RequireCustomString("UNITY_VERTEX_OUTPUT_STEREO");

                IncludeConfig(pass);
                pass.pragmas.Add("#include \"Packages/com.z3y.graphlit/ShaderLibrary/Core.hlsl\"");
                builder.AddPass(pass);
            }
            if (urp)
            {
                {
                    var pass = new PassBuilder("DepthOnly", Vertex, FragmentDepth, POSITION, NORMAL, TANGENT, ALPHA, CUTOFF);
                    CreateUniversalDepthPass(pass);
                    builder.AddPass(pass);
                }
                {
                    var pass = new PassBuilder("DepthNormals", Vertex, FragmentDepthNormals, POSITION, NORMAL, TANGENT, ALPHA, CUTOFF);
                    CreateUniversalDepthNormalsPass(pass);
                    pass.pragmas.Add("#define UNLIT_TEMPLATE");
                    builder.AddPass(pass);
                }
            }
            {
                var pass = new PassBuilder("ShadowCaster", Vertex, FragmentShadow, POSITION, NORMAL, TANGENT, ALPHA, CUTOFF);
                CreateShadowCaster(pass, urp);
                builder.AddPass(pass);
            }

        }
    }
}