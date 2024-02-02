using System;
using ZSG.Nodes;
using ZSG.Nodes.PortType;

namespace ZSG
{
    [NodeInfo("_/Intiger Property"), Serializable]
    public class IntigerPropertyNode : PropertyNode
    {
        protected override PropertyType propertyType => PropertyType.Intiger;
        public override void AddElements()
        {
            base.AddElements();
            AddPort(new(PortDirection.Output, new Int(), OUT));
        }
    }
}