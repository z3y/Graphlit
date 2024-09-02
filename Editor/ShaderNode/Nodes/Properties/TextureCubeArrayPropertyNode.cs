using System;
using Enlit.Nodes;
using Enlit.Nodes.PortType;

namespace Enlit
{
    [NodeInfo("Input/Texture Cube Array Property"), Serializable]
    public class TextureCubeArrayPropertyNode : TexturePropertyNode
    {
        protected override PropertyType propertyType => PropertyType.TextureCubeArray;
        public override IPortType TextureType => new TextureCubeArrayObject();
    }
}