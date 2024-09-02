using System;
using Enlit.Nodes;
using Enlit.Nodes.PortType;

namespace Enlit
{
    [NodeInfo("Input/Boolean Property"), Serializable]
    public class BooleanPropertyNode : PropertyNode
    {
        protected override PropertyType propertyType => PropertyType.Bool;
        public override void Initialize()
        {
            base.Initialize();
            AddPort(new(PortDirection.Output, new Bool(), OUT, "Bool"));
        }
    }
}