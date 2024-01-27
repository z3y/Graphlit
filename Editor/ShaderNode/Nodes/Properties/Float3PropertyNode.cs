using System;
using ZSG.Nodes;
using ZSG.Nodes.PortType;

namespace ZSG
{
    [NodeInfo("Float3 Property"), Serializable]
    public class Float3PropertyNode : PropertyNode
    {
        protected override PropertyType propertyType => PropertyType.Float3;
        public override void AddElements()
        {
            base.AddElements();
            AddPort(new(PortDirection.Output, new Float(3), OUT));
        }
    }
}