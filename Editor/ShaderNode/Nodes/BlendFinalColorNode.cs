using System;
using System.Collections.Generic;
using System.Text;
using Graphlit.Nodes;
using Graphlit.Nodes.PortType;
using UnityEditor.Experimental.GraphView;

namespace Graphlit
{
    [NodeInfo("Utility/Blend Final Color")]
    public class BlendFinalColorNode : ShaderNode
    {
        public const int COLOR = 0;
        public const int ALPHA = 1;
        public const int DIFFUSE = 2;
        public const int SPECULAR = 3;
        public const int EMISSION = 4;
        public const int ALBEDO = 5;
        public const int ROUGHNESS = 6;
        public const int METALLIC = 7;
        public const int IN_ALPHA = 8;

        public override bool DisablePreview => true;
        public override void Initialize()
        {
            AddPort(new(PortDirection.Output, new Float(3), COLOR, "Color"));
            AddPort(new(PortDirection.Output, new Float(1), ALPHA, "Alpha"));

            AddPort(new(PortDirection.Input, new Float(3), ALBEDO, "Albedo"));
            AddPort(new(PortDirection.Input, new Float(1), IN_ALPHA, "Alpha"));
            AddPort(new(PortDirection.Input, new Float(1), ROUGHNESS, "Roughness"));
            AddPort(new(PortDirection.Input, new Float(1), METALLIC, "Metallic"));
            AddPort(new(PortDirection.Input, new Float(1), EMISSION, "Emission"));
            AddPort(new(PortDirection.Input, new Float(3), DIFFUSE, "Diffuse"));
            AddPort(new(PortDirection.Input, new Float(3), SPECULAR, "Specular"));
        }

        protected override void Generate(NodeVisitor visitor)
        {
            string name = "BlendFinalColor" + UniqueVariableID;
            string color = name + "_Color";
            string alpha = name + "_Alpha";

            visitor.AppendLine($"half3 {color};");
            visitor.AppendLine($"half {alpha};");

            if (visitor._shaderBuilder.passBuilders[visitor.Pass].name != "SHADOWCASTER")
            {
                visitor.AppendLine($"BlendFinalColor({color}, {alpha}, {PortData[DIFFUSE].Name}, {PortData[SPECULAR].Name}, {PortData[EMISSION].Name}, {PortData[ALBEDO].Name}, {PortData[ROUGHNESS].Name}, {PortData[METALLIC].Name}, {PortData[IN_ALPHA].Name});");
            }
            else
            {
                visitor.AppendLine($"BlendFinalColor({color}, {alpha}, 1, 0, 0, 1, 0, {PortData[METALLIC].Name}, {PortData[IN_ALPHA].Name});");
            }

            SetVariable(COLOR, color);
            SetVariable(ALPHA, alpha);
        }
    }
}