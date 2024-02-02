using System;
using ZSG.Nodes;
using ZSG.Nodes.PortType;

namespace ZSG
{
    [NodeInfo("_/Integer Property"), Serializable]
    public class IntegerPropertyNode : PropertyNode
    {
        protected override PropertyType propertyType => PropertyType.Integer;
        public override void AddElements()
        {
            base.AddElements();
            AddPort(new(PortDirection.Output, new Int(), OUT));
        }
    }
}