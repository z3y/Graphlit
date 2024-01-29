using UnityEngine.UIElements;
using ZSG.Nodes.PortType;
using ZSG.Nodes;
using UnityEngine;

namespace ZSG
{
    [NodeInfo("Targets/Unlit")]
    public class UnlitTemplate : TemplateOutput
    {
        public override string Name { get; } = "Unlit";
        public override int[] VertexPorts => new int[] { POSITION, NORMAL, TANGENT };
        public override int[] FragmentPorts => new int[] { COLOR, ALPHA };

        const int POSITION = 0;
        const int NORMAL = 1;
        const int TANGENT = 2;
        const int COLOR = 3;
        const int ALPHA = 4;
        public override void AddElements()
        {
            AddPort(new(PortDirection.Input, new Float(3, false), POSITION, "Position"));
            AddPort(new(PortDirection.Input, new Float(3, false), NORMAL, "Normal"));
            AddPort(new(PortDirection.Input, new Float(4, false), TANGENT, "Tangent"));

            var separator = new VisualElement();
            separator.style.height = 2;
            separator.style.backgroundColor = Color.gray;
            inputContainer.Add(separator);
            AddPort(new(PortDirection.Input, new Float(3, false), COLOR, "Color"));
            AddPort(new(PortDirection.Input, new Float(1, false), ALPHA, "Alpha"));

            Bind(POSITION, PortBinding.PositionOS);
            Bind(NORMAL, PortBinding.NormalOS);
            Bind(TANGENT, PortBinding.TangentOS);
        }


        const string Vertex = "Packages/com.z3y.myshadergraph/Editor/Targets/Unlit/Vertex.hlsl";
        const string FragmentForward = "Packages/com.z3y.myshadergraph/Editor/Targets/Unlit/FragmentForward.hlsl";
        const string FragmentShadow = "Packages/com.z3y.myshadergraph/Editor/Targets/Unlit/FragmentShadow.hlsl";

        public override void BuilderPassthourgh(ShaderBuilder builder)
        {
            builder.subshaderTags.Add("RenderType", "Opaque");

            {
                var pass = new PassBuilder("FORWARD", Vertex, FragmentForward, POSITION, NORMAL, TANGENT, COLOR, ALPHA);
                pass.tags["LightMode"] = "ForwardBase";

                pass.pragmas.Add("#pragma multi_compile_fwdbase");
                pass.pragmas.Add("#pragma multi_compile_fog");
                pass.pragmas.Add("#pragma multi_compile_instancing");

                pass.attributes.RequirePositionOS();
                pass.attributes.Require("UNITY_VERTEX_INPUT_INSTANCE_ID");

                pass.varyings.RequirePositionCS();
                pass.varyings.RequireCustomString("UNITY_VERTEX_INPUT_INSTANCE_ID");
                pass.varyings.RequireCustomString("UNITY_VERTEX_OUTPUT_STEREO");

                pass.pragmas.Add("#include \"Packages/com.z3y.myshadergraph/ShaderLibrary/BuiltInLibrary.hlsl\"");
                builder.AddPass(pass);
            }

            {
                var pass = new PassBuilder("SHADOWCASTER", Vertex, FragmentShadow, POSITION, ALPHA);
                pass.tags["LightMode"] = "ShadowCaster";
                pass.renderStates["ZWrite"] = "On";
                pass.renderStates["ZTest"] = "LEqual";

                pass.pragmas.Add("#pragma multi_compile_shadowcaster");
                pass.pragmas.Add("#pragma multi_compile_instancing");

                pass.attributes.RequirePositionOS();
                pass.attributes.Require("UNITY_VERTEX_INPUT_INSTANCE_ID");

                pass.varyings.RequirePositionCS();
                pass.varyings.RequireCustomString("UNITY_VERTEX_INPUT_INSTANCE_ID");
                pass.varyings.RequireCustomString("UNITY_VERTEX_OUTPUT_STEREO");

                pass.pragmas.Add("#include \"Packages/com.z3y.myshadergraph/ShaderLibrary/BuiltInLibrary.hlsl\"");
                builder.AddPass(pass);
            }
        }
    }
}