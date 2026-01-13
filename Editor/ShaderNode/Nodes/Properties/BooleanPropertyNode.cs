using System;
using Graphlit.Nodes;
using Graphlit.Nodes.PortType;

namespace Graphlit
{
    [NodeInfo("Input/Boolean Property"), Serializable]
    public class BooleanPropertyNode : PropertyNode
    {
        protected override PropertyType propertyType => PropertyType.Toggle;
        public override void Initialize()
        {
            base.Initialize();
            AddPort(new(PortDirection.Output, new Bool(), OUT, "Bool"));
        }
    }
}