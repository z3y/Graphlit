using System;
using ZSG.Nodes;
using ZSG.Nodes.PortType;

namespace ZSG
{
    [NodeInfo("_/Texture Cube Array Property"), Serializable]
    public class TextureCubeArrayPropertyNode : TexturePropertyNode
    {
        protected override PropertyType propertyType => PropertyType.TextureCubeArray;
        public override IPortType TextureType => new TextureCubeArrayObject();
    }
}