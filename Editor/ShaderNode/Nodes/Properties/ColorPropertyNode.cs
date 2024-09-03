using System;
using Graphlit.Nodes;
using Graphlit.Nodes.PortType;

namespace Graphlit
{
    [NodeInfo("Input/Color Property"), Serializable]
    public class ColorPropertyNode : PropertyNode
    {
        protected override PropertyType propertyType => PropertyType.Color;
        public override void Initialize()
        {
            base.Initialize();
            AddPort(new(PortDirection.Output, new Float(4), OUT, "Color"));
        }
    }
}