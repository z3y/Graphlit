using System;
using Graphlit.Nodes;
using Graphlit.Nodes.PortType;

namespace Graphlit
{
    [NodeInfo("Input/Float4 Property"), Serializable]
    public class Float4PropertyNode : PropertyNode
    {
        protected override PropertyType propertyType => PropertyType.Float4;
        public override void Initialize()
        {
            base.Initialize();

            AddPort(new(PortDirection.Output, new Float(4), OUT, "Float4"));
        }
    }
}