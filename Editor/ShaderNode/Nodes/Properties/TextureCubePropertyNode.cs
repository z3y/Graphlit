using System;
using Enlit.Nodes;
using Enlit.Nodes.PortType;

namespace Enlit
{
    [NodeInfo("Input/Texture Cube Property"), Serializable]
    public class TextureCubePropertyNode : TexturePropertyNode
    {
        protected override PropertyType propertyType => PropertyType.TextureCube;
        public override IPortType TextureType => new TextureCubeObject();
    }
}