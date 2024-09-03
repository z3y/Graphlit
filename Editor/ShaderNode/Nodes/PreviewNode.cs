using System;
using Graphlit.Nodes;

namespace Graphlit
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