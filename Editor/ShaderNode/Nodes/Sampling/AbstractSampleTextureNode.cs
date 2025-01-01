using UnityEditor.Experimental.GraphView;
using UnityEngine;
using Graphlit.Nodes;
using Graphlit.Nodes.PortType;
using System.Linq;

namespace Graphlit
{
    [System.Serializable]
    public abstract class SampleTextureNode : ShaderNode
    {
        protected const int UV = 0;
        protected const int TEX = 1;
        protected const int SAMPLER = 8;
        protected const int OUT_RGBA = 3;

        protected const int OUT_RGB = 2;
        protected const int OUT_R = 4;
        protected const int OUT_G = 5;
        protected const int OUT_B = 6;
        protected const int OUT_A = 7;

        protected const int LOD = 9;

        public override Color Accent => new Color(0.8f, 0.2f, 0.2f);


        public override int PreviewResolution => 156;

        public virtual IPortType TextureType => new Texture2DObject();
        public virtual bool HasLod => false;
        public virtual int Coords => 2;
        public virtual PortBinding UVBinding => PortBinding.UV0;
        public virtual string SampleMethod => $"SAMPLE_TEXTURE2D({PortData[TEX].Name}, {GetSamplerName(PortData[TEX].Name)}, {PortData[UV].Name})";

        Port _texturePort;
        Port _samplerPort;
        public override void Initialize()
        {
            _texturePort = AddPort(new(PortDirection.Input, TextureType, TEX, "Texture"));
            _samplerPort = AddPort(new(PortDirection.Input, new SamplerState(), SAMPLER, "Sampler"));
            AddPort(new(PortDirection.Input, new Float(Coords), UV, "UV"));

            AddPort(new(PortDirection.Output, new Float(4), OUT_RGBA, "RGBA"));

            AddPort(new(PortDirection.Output, new Float(3), OUT_RGB, "<color=red>R</color><color=green>G</color><color=blue>B</color>"));

            AddPort(new(PortDirection.Output, new Float(1), OUT_R, "<color=red>R</color>"));
            AddPort(new(PortDirection.Output, new Float(1), OUT_G, "<color=green>G</color>"));
            AddPort(new(PortDirection.Output, new Float(1), OUT_B, "<color=blue>B</color>"));
            AddPort(new(PortDirection.Output, new Float(1), OUT_A, "A"));

            if (HasLod)
            {
                AddPort(new(PortDirection.Input, new Float(1), LOD, "LOD"));
            }

            Bind(UV, UVBinding);
        }

        protected override void Generate(NodeVisitor visitor)
        {
            string name = "TextureSample" + UniqueVariableID;
            SetVariable(OUT_RGBA, name);




            if (_texturePort.connected)
            {
                bool hasKeyword = false;
                if (visitor.GenerationMode == GenerationMode.Final && this is SampleTexture2DNode sample2d)
                {
                    var node = _texturePort.connections.First().output.node;
                    if (node is IHasPropertyDescriptor connectedProp)
                    {
                        var desc = connectedProp.GetPropertyDescriptor();
                        if (sample2d.autoKeyword)
                        {
                            hasKeyword = true;
                            var keyword = desc.GetAutoKeywordName(desc.GetReferenceName(visitor.GenerationMode));
                            var defaultValueStr = desc.DefaultTextureToValue();
                            desc.autoKeyword = true;

                            visitor.AddPragma($"#pragma shader_feature_local {keyword}");


                            visitor.AppendLine($"#ifndef {keyword}");
                            visitor.AppendLine($"{PrecisionString(4)} {PortData[OUT_RGBA].Name} = {defaultValueStr};");
                            visitor.AppendLine($"#else");
                        }
                    }
                }

                visitor.AppendLine($"{PrecisionString(4)} {PortData[OUT_RGBA].Name} = {SampleMethod};");

                if (hasKeyword) visitor.AppendLine($"#endif");
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

        protected string GetSamplerName(string textureName)
        {
            string samplerName;
            if (_samplerPort.connected)
            {
                samplerName = PortData[SAMPLER].Name;
            }
            else
            {
                samplerName = "sampler" + textureName;
            }

            return samplerName;
        }
    }
}