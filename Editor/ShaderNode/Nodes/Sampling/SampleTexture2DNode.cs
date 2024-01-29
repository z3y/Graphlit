using UnityEditor.Experimental.GraphView;
using UnityEngine;
using ZSG.Nodes;
using ZSG.Nodes.PortType;

namespace ZSG
{
    [NodeInfo("Texture/Sample Texture 2D")]
    public class SampleTexture2DNode : ShaderNode
    {
        const int UV = 0;
        const int TEX = 1;
        const int SAMPLER = 8;
        const int OUT_RGBA = 3;

        const int OUT_RGB = 2;
        const int OUT_R = 4;
        const int OUT_G = 5;
        const int OUT_B = 6;
        const int OUT_A = 7;

        public override Color Accent => new Color(0.8f, 0.2f, 0.2f);

        public override int PreviewResolution => 156;

        Port _texturePort;
        Port _samplerPort;
        public override void AddElements()
        {
            _texturePort = AddPort(new(PortDirection.Input, new Texture2DObject(), TEX, "Texture 2D"));
            _samplerPort = AddPort(new(PortDirection.Input, new SamplerState(), SAMPLER, "Sampler State"));
            AddPort(new(PortDirection.Input, new Float(2), UV, "UV"));

            AddPort(new(PortDirection.Output, new Float(4), OUT_RGBA, "RGBA"));

            AddPort(new(PortDirection.Output, new Float(3), OUT_RGB, "<color=red>R</color><color=green>G</color><color=blue>B</color>"));

            AddPort(new(PortDirection.Output, new Float(1), OUT_R, "<color=red>R</color>"));
            AddPort(new(PortDirection.Output, new Float(1), OUT_G, "<color=green>G</color>"));
            AddPort(new(PortDirection.Output, new Float(1), OUT_B, "<color=blue>B</color>"));
            AddPort(new(PortDirection.Output, new Float(1), OUT_A, "A"));


            Bind(UV, PortBinding.UV0);
        }

        protected override void Generate(NodeVisitor visitor)
        {
            string name = "TextureSample" + UniqueVariableID++.ToString();
            SetVariable(OUT_RGBA, name);


            if (_texturePort.connected)
            {
                var textureName = PortData[TEX].Name;

                string samplerName;
                if (_samplerPort.connected)
                {
                    samplerName = PortData[SAMPLER].Name;
                }
                else
                {
                    samplerName = "sampler" + textureName;
                }

                visitor.AppendLine($"{PrecisionString(4)} {PortData[OUT_RGBA].Name} = SAMPLE_TEXTURE2D({textureName}, {samplerName}, {PortData[UV].Name});");
            }
            else
            {
                visitor.AppendLine($"{PrecisionString(4)} {PortData[OUT_RGBA].Name} = {PrecisionString(4)}(1,1,1,1);");
            }

            SetVariable(OUT_RGB, $"{PrecisionString(3)}({name}.rgb)");
            SetVariable(OUT_R, $"{PrecisionString(1)}({name}.r)");
            SetVariable(OUT_G, $"{PrecisionString(1)}({name}.g)");
            SetVariable(OUT_B, $"{PrecisionString(1)}({name}.b)");
            SetVariable(OUT_A, $"{PrecisionString(1)}({name}.a)");
        }
    }
}