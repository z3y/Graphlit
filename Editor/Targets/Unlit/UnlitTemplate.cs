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

        const string Vertex = "Packages/com.z3y.graphlit/Editor/Targets/Vertex.hlsl";
        const string FragmentForward = "Packages/com.z3y.graphlit/Editor/Targets/Unlit/FragmentForward.hlsl";
        const string FragmentShadow = "Packages/com.z3y.graphlit/Editor/Targets/FragmentShadow.hlsl";

        public override void OnBeforeBuild(ShaderBuilder builder)
        {
            builder.properties.Add(_surfaceOptionsStart);
            builder.properties.Add(_mode);
            builder.properties.Add(_srcBlend);
            builder.properties.Add(_dstBlend);
            builder.properties.Add(_zwrite);
            builder.properties.Add(_cull);

            if (_customLighting)
            {
                builder.properties.Add(_dfgProperty);
                builder._defaultTextures["_DFG"] = _dfg;
            }

            //if (GraphView.graphData.outlinePass != GraphData.OutlinePassMode.Disabled) builder.properties.Add(_outlineToggle);
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

                if (_customLighting)
                {
                    pass.pragmas.Add("#pragma multi_compile_fwdbase");
                    //pass.pragmas.Add("#pragma skip_variants LIGHTPROBE_SH");
                    pass.pragmas.Add("#pragma shader_feature_fragment VERTEXLIGHT_ON");
                }
                pass.pragmas.Add("#pragma multi_compile_fog");
                pass.pragmas.Add("#pragma multi_compile_instancing");

                pass.attributes.RequirePositionOS();
                pass.attributes.Require("UNITY_VERTEX_INPUT_INSTANCE_ID");

                pass.varyings.RequirePositionCS();
                pass.varyings.RequireCustomString("UNITY_FOG_COORDS(*)");
                if (_customLighting)
                {
                    pass.attributes.RequireUV(1, 2);
                    pass.varyings.RequireCustomString("UNITY_SHADOW_COORDS(*)");
                    pass.varyings.RequireCustomString("#ifdef LIGHTMAP_ON\ncentroid float2 lightmapUV : LIGHTMAPUV;\n#endif");
                }
                pass.varyings.RequireCustomString("UNITY_VERTEX_INPUT_INSTANCE_ID");
                pass.varyings.RequireCustomString("UNITY_VERTEX_OUTPUT_STEREO");

                pass.pragmas.Add("#include \"Packages/com.z3y.graphlit/ShaderLibrary/BuiltInLibrary.hlsl\"");
                builder.AddPass(pass);
            }

            if (_customLighting)
            {
                var pass = new PassBuilder("FORWARD_DELTA", Vertex, FragmentForward, POSITION, NORMAL, TANGENT, COLOR, ALPHA, CUTOFF);
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

                pass.varyings.RequireCustomString("UNITY_FOG_COORDS(*)");
                pass.varyings.RequireCustomString("UNITY_SHADOW_COORDS(*)");
                pass.varyings.RequireCustomString("UNITY_VERTEX_INPUT_INSTANCE_ID");
                pass.varyings.RequireCustomString("UNITY_VERTEX_OUTPUT_STEREO");

                pass.pragmas.Add("#include \"Packages/com.z3y.graphlit/ShaderLibrary/BuiltInLibrary.hlsl\"");
                builder.AddPass(pass);
            }

            {
                var pass = new PassBuilder("SHADOWCASTER", Vertex, FragmentShadow, POSITION, NORMAL, TANGENT, ALPHA, CUTOFF);
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

                pass.pragmas.Add("#include \"Packages/com.z3y.graphlit/ShaderLibrary/BuiltInLibrary.hlsl\"");
                builder.AddPass(pass);
            }
        }
    }
}