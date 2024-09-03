using System;
using Graphlit.Nodes;
using Graphlit.Nodes.PortType;

namespace Graphlit
{
    [NodeInfo("Input/Float Property"), Serializable]
    public class FloatPropertyNode : PropertyNode
    {
        protected override PropertyType propertyType => PropertyType.Float;
        public override void Initialize()
        {
            base.Initialize();

            AddPort(new(PortDirection.Output, new Float(1), OUT, "Float"));
        }
    }
}