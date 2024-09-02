using System;
using Enlit.Nodes;

namespace Enlit
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