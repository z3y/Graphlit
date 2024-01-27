using System;
using ZSG.Nodes;
using ZSG.Nodes.PortType;

namespace ZSG
{
    [NodeInfo("Float4 Property"), Serializable]
    public class Float4PropertyNode : PropertyNode
    {
        protected override PropertyType propertyType => PropertyType.Float4;
        public override void AddElements()
        {
            base.AddElements();

            AddPort(new(PortDirection.Output, new Float(4), OUT));
        }
    }
}