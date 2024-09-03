using System;
using Graphlit.Nodes;
using Graphlit.Nodes.PortType;

namespace Graphlit
{
    [NodeInfo("Texture/Sample Texture Cube Array")]
    public class SampleTextureCubeArrayNode : SampleTextureNode
    {
        public override IPortType TextureType => new TextureCubeArrayObject();
        public override PreviewType DefaultPreviewOverride => PreviewType.Preview3D;

        public override string SampleMethod => $"SAMPLE_TEXTURECUBE_ARRAY({PortData[TEX].Name}, {GetSamplerName(PortData[TEX].Name)}, {PortData[INDEX].Name}, {PortData[UV].Name})";
        const int INDEX = 10;
        public override int Coords => 3;
        public override PortBinding UVBinding => PortBinding.PositionWS;

        public override void Initialize()
        {
            base.Initialize();
            AddPort(new(PortDirection.Input, new Float(1), INDEX, "Index"));
        }

    }
    [NodeInfo("Texture/Sample Texture Cube Array LOD")]
    public class SampleTextureCubeArrayLodNode : SampleTextureNode
    {
        public override PreviewType DefaultPreviewOverride => PreviewType.Preview3D;

        public override bool HasLod => true;
        public override IPortType TextureType => new TextureCubeArrayObject();
        public override int Coords => 3;
        public override PortBinding UVBinding => PortBinding.PositionWS;

        public override string SampleMethod => $"SAMPLE_TEXTURECUBE_ARRAY_LOD({PortData[TEX].Name}, {GetSamplerName(PortData[TEX].Name)}, {PortData[UV].Name}, {PortData[INDEX].Name}, {PortData[LOD].Name})";
        const int INDEX = 10;
        public override void Initialize()
        {
            base.Initialize();
            AddPort(new(PortDirection.Input, new Float(1), INDEX, "Index"));
        }
    }
}