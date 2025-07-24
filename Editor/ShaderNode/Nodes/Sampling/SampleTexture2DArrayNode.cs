using UnityEditor.Experimental.GraphView;
using UnityEngine;
using Graphlit.Nodes;
using Graphlit.Nodes.PortType;

namespace Graphlit
{
    [NodeInfo("Texture/Sample Texture 2D Array")]
    public class SampleTexture2DArrayNode : SampleTextureNode
    {
        const int INDEX = 10;
        public override IPortType TextureType => new Texture2DArrayObject();
        public override string SampleMethod => $"SAMPLE_TEXTURE2D_ARRAY({PortData[TEX].Name}, {GetSamplerName(PortData[TEX].Name)}, {PortData[UV].Name}, {PortData[INDEX].Name})";

        public override void Initialize()
        {
            base.Initialize();
            AddPort(new(PortDirection.Input, new Float(1), INDEX, "Index"));
        }
    }
    [NodeInfo("Texture/Sample Texture 2D Array LOD")]
    public class SampleTexture2DArrayLodNode : SampleTextureNode
    {
        const int INDEX = 10;
        public override IPortType TextureType => new Texture2DArrayObject();
        public override bool HasLod => true;
        public override string SampleMethod => $"SAMPLE_TEXTURE2D_ARRAY_LOD({PortData[TEX].Name}, {GetSamplerName(PortData[TEX].Name)}, {PortData[UV].Name}, {PortData[INDEX].Name}, {PortData[LOD].Name})";

        public override void Initialize()
        {
            base.Initialize();
            AddPort(new(PortDirection.Input, new Float(1), INDEX, "Index"));
        }
    }

    [NodeInfo("Texture/Sample Texture 2D Array BIAS")]
    public class SampleTexture2DArrayBiasNode : SampleTextureNode
    {
        const int INDEX = 10;
        public override IPortType TextureType => new Texture2DArrayObject();
        public override bool HasBias => true;
        public override string SampleMethod => $"SAMPLE_TEXTURE2D_ARRAY_BIAS({PortData[TEX].Name}, {GetSamplerName(PortData[TEX].Name)}, {PortData[UV].Name}, {PortData[INDEX].Name}, {PortData[BIAS].Name})";

        public override void Initialize()
        {
            base.Initialize();
            AddPort(new(PortDirection.Input, new Float(1), INDEX, "Index"));
        }
    }
}