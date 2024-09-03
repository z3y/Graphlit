using System;
using Graphlit.Nodes;
using Graphlit.Nodes.PortType;

namespace Graphlit
{
    [NodeInfo("Texture/Sample Texture Cube")]
    public class SampleTextureCubeNode : SampleTextureNode
    {
        public override IPortType TextureType => new TextureCubeObject();
        public override int Coords => 3;
        public override PortBinding UVBinding => PortBinding.PositionWS;
        public override PreviewType DefaultPreviewOverride => PreviewType.Preview3D;
        public override string SampleMethod => $"SAMPLE_TEXTURECUBE({PortData[TEX].Name}, {GetSamplerName(PortData[TEX].Name)}, {PortData[UV].Name})";
    }
    [NodeInfo("Texture/Sample Texture Cube LOD")]
    public class SampleTextureCubeLodNode : SampleTextureNode
    {
        public override PreviewType DefaultPreviewOverride => PreviewType.Preview3D;
        public override bool HasLod => true;
        public override IPortType TextureType => new TextureCubeObject();
        public override int Coords => 3;
        public override PortBinding UVBinding => PortBinding.PositionWS;
        public override string SampleMethod => $"SAMPLE_TEXTURECUBE_LOD({PortData[TEX].Name}, {GetSamplerName(PortData[TEX].Name)}, {PortData[UV].Name}, {PortData[LOD].Name})";
    }
}