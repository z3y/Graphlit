using System;
using ZSG.Nodes;
using ZSG.Nodes.PortType;

namespace ZSG
{
    [NodeInfo("Input/Texture 2D Array Property"), Serializable]
    public class Texture2DArrayPropertyNode : TexturePropertyNode
    {
        protected override PropertyType propertyType => PropertyType.Texture2DArray;
        public override IPortType TextureType => new Texture2DArrayObject();
    }
}