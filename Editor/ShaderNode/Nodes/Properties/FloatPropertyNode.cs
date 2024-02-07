using System;
using ZSG.Nodes;
using ZSG.Nodes.PortType;

namespace ZSG
{
    [NodeInfo("_/Float Property"), Serializable]
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