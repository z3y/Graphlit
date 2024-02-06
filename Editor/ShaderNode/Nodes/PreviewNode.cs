using System;
using ZSG.Nodes;

namespace ZSG
{
    [NodeInfo("Utility/Preview"), Serializable]
    public class PreviewNode : PassthroughNode
    {
        protected override void Generate(NodeVisitor visitor)
        {
            base.Generate(visitor);
            Output(visitor, OUT, $"{PortData[IN].Name}");
        }
    }
}