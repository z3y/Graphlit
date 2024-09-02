using System;
using Enlit.Nodes;
using Enlit.Nodes.PortType;

namespace Enlit
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