using ZSG.Nodes;
using ZSG.Nodes.PortType;

namespace ZSG
{
    [NodeInfo("Utility/Branch")]
    public class BranchNode : ShaderNode
    {
        const int BOOL = 0;
        const int TRUE = 1;
        const int FALSE = 2;
        const int OUT = 3;
        public override bool DisablePreview => true;
        public override void AddElements()
        {
            AddPort(new(PortDirection.Input, new Bool(), BOOL, "Bool"));
            AddPort(new(PortDirection.Input, new Float(1, true), TRUE, "True"));
            AddPort(new(PortDirection.Input, new Float(1, true), FALSE, "False"));
            AddPort(new(PortDirection.Output, new Float(1, true), OUT));
        }

        protected override void Generate(NodeVisitor visitor)
        {
            ChangeComponents(OUT, ImplicitTruncation(TRUE, FALSE).components);
            Output(visitor, OUT, $"{PortData[BOOL].Name} ? {PortData[TRUE].Name} : {PortData[FALSE].Name}");
        }
    }
}