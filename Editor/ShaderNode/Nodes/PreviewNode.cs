using System;
using ZSG.Nodes;

namespace ZSG
{
    [NodeInfo("Preview"), Serializable]
    public class PreviewNode : PasstroughNode
    {
        protected override void Generate(NodeVisitor visitor)
        {
            base.Generate(visitor);
            Output(visitor, OUT, $"{PortData[IN].Name}");
        }
    }
}