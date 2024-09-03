using System;
using Graphlit.Nodes;
using Graphlit.Nodes.PortType;

namespace Graphlit
{
    [NodeInfo("Input/Integer Property"), Serializable]
    public class IntegerPropertyNode : PropertyNode
    {
        protected override PropertyType propertyType => PropertyType.Integer;
        public override void Initialize()
        {
            base.Initialize();
            AddPort(new(PortDirection.Output, new Int(), OUT, "Int"));
        }
    }
}