using System;
using Graphlit.Nodes;
using Graphlit.Nodes.PortType;

namespace Graphlit
{
    [NodeInfo("Input/Texture Cube Array Property"), Serializable]
    public class TextureCubeArrayPropertyNode : TexturePropertyNode
    {
        protected override PropertyType propertyType => PropertyType.TextureCubeArray;
        public override IPortType TextureType => new TextureCubeArrayObject();
    }
}