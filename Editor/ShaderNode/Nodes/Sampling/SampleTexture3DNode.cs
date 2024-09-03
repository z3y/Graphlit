using Graphlit.Nodes;
using Graphlit.Nodes.PortType;

namespace Graphlit
{
    [NodeInfo("Texture/Sample Texture 3D")]
    public class SampleTexture3DNode : SampleTextureNode
    {
        public override IPortType TextureType => new Texture3DObject();
        public override int Coords => 3;
        public override string SampleMethod => $"SAMPLE_TEXTURE3D({PortData[TEX].Name}, {GetSamplerName(PortData[TEX].Name)}, {PortData[UV].Name})";

    }
    [NodeInfo("Texture/Sample Texture 3D LOD")]
    public class SampleTexture3DLodNode : SampleTextureNode
    {
        public override bool HasLod => true;
        public override int Coords => 3;
        public override IPortType TextureType => new Texture3DObject();
        public override string SampleMethod => $"SAMPLE_TEXTURE3D_LOD({PortData[TEX].Name}, {GetSamplerName(PortData[TEX].Name)}, {PortData[UV].Name}, {PortData[LOD].Name})";
    }
}