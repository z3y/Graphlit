using System;
using Graphlit.Nodes;
using Graphlit.Nodes.PortType;

namespace Graphlit
{
    [NodeInfo("Input/Texture 2D Array Property"), Serializable]
    public class Texture2DArrayPropertyNode : TexturePropertyNode
    {
        protected override PropertyType propertyType => PropertyType.Texture2DArray;
        public override IPortType TextureType => new Texture2DArrayObject();
    }
}