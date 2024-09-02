using System;
using Enlit.Nodes;
using Enlit.Nodes.PortType;

namespace Enlit
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