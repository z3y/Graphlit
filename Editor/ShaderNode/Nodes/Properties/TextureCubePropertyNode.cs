using System;
using ZSG.Nodes;
using ZSG.Nodes.PortType;

namespace ZSG
{
    [NodeInfo("_/Texture Cube Property"), Serializable]
    public class TextureCubePropertyNode : TexturePropertyNode
    {
        protected override PropertyType propertyType => PropertyType.TextureCube;
        public override IPortType TextureType => new TextureCubeObject();
    }
}