using System;
using Enlit.Nodes;
using Enlit.Nodes.PortType;

namespace Enlit
{
    [NodeInfo("Input/Float2 Property"), Serializable]
    public class Float2PropertyNode : PropertyNode
    {
        protected override PropertyType propertyType => PropertyType.Float2;
        public override void Initialize()
        {
            base.Initialize();
            AddPort(new(PortDirection.Output, new Float(2), OUT, "Float2"));
        }
    }
}