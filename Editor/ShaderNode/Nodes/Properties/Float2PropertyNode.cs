using System;
using ZSG.Nodes;
using ZSG.Nodes.PortType;

namespace ZSG
{
    [NodeInfo("_/Float2 Property"), Serializable]
    public class Float2PropertyNode : PropertyNode
    {
        protected override PropertyType propertyType => PropertyType.Float2;
        public override void AddElements()
        {
            base.AddElements();
            AddPort(new(PortDirection.Output, new Float(2), OUT));
        }
    }
}