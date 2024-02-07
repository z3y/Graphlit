using System;
using System.Threading.Tasks;
using UnityEditor.Experimental.GraphView;
using ZSG.Nodes;
using ZSG.Nodes.PortType;

namespace ZSG
{
    public abstract class TexturePropertyNode : PropertyNode
    {
        protected override PropertyType propertyType => PropertyType.Texture2D;
        const int samplerID = 1;
        const int scaleOffsetID = 2;
        const int TexelSizeID = 3;
        Port _scaleOffsetPort;
        Port _samplerPort;
        Port _texelSizePort;

        public override Precision DefaultPrecisionOverride => Precision.Float;

        public virtual IPortType TextureType => new Texture2DObject();
        public override void Initialize()
        {
            base.Initialize();

            AddPort(new(PortDirection.Output, TextureType, OUT, "Texture"));
            _samplerPort = AddPort(new(PortDirection.Output, new SamplerState(), samplerID, "Sampler"));
            _scaleOffsetPort = AddPort(new(PortDirection.Output, new Float(4), scaleOffsetID, "Scale Offset"));
            _texelSizePort = AddPort(new(PortDirection.Output, new Float(4), TexelSizeID, "Texel Size"));


            InitializeTexture(); // TODO: figure out why textures arent set on time
        }
        async void InitializeTexture()
        {
            await Task.Delay(1000);
            propertyDescriptor.UpdatePreviewMaterial();
        }

        protected override void Generate(NodeVisitor visitor)
        {
            base.Generate(visitor);
            var generation = visitor.GenerationMode;

            var referenceName = propertyDescriptor.GetReferenceName(generation);
            if (_scaleOffsetPort.connected)
            {
                var scaleOffsetProperty = new PropertyDescriptor(PropertyType.Float4, "", referenceName + "_ST")
                {
                    declaration = PropertyDeclaration.Global
                };
                visitor.AddProperty(scaleOffsetProperty);

                if (generation == GenerationMode.Preview)
                {
                    PortData[scaleOffsetID] = new GeneratedPortData(portDescriptors[scaleOffsetID].Type, "float4(1,1,0,0)");
                }
                else
                {
                    PortData[scaleOffsetID] = new GeneratedPortData(portDescriptors[scaleOffsetID].Type, scaleOffsetProperty.GetReferenceName(GenerationMode.Final));
                }
            }
            if (_samplerPort.connected)
            {
                PortData[samplerID] = new GeneratedPortData(new SamplerState(), "sampler" + referenceName);
            }
            if (_texelSizePort.connected)
            {
                var texelSize = new PropertyDescriptor(PropertyType.Float4, "", referenceName + "_TexelSize")
                {
                    declaration = PropertyDeclaration.Global,
                    useReferenceName = true
                };
                visitor.AddProperty(texelSize);

                PortData[TexelSizeID] = new GeneratedPortData(portDescriptors[TexelSizeID].Type, texelSize.GetReferenceName(GenerationMode.Final));
            }
        }
    }
}