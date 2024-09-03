using System;
using Graphlit.Nodes;
using Graphlit.Nodes.PortType;

namespace Graphlit
{
    [NodeInfo("Input/Texture Cube Property"), Serializable]
    public class TextureCubePropertyNode : TexturePropertyNode
    {
        protected override PropertyType propertyType => PropertyType.TextureCube;
        public override IPortType TextureType => new TextureCubeObject();
    }
}