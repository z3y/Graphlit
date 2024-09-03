using System;
using System.Threading.Tasks;
using UnityEditor.Experimental.GraphView;
using Graphlit.Nodes;
using Graphlit.Nodes.PortType;

namespace Graphlit
{
    [NodeInfo("Input/Texture 3D Property"), Serializable]
    public class Texture3DPropertyNode : TexturePropertyNode
    {
        protected override PropertyType propertyType => PropertyType.Texture3D;
        public override IPortType TextureType => new Texture3DObject();
    }
}