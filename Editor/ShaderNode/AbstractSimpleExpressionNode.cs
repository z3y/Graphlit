using ZSG.Nodes;
using ZSG.Nodes.PortType;

namespace ZSG
{
    public abstract class SimpleExpressionNode : ShaderNode
    {
        protected const int A = 0;
        protected const int B = 1;
        protected const int OUT = 2;

        protected virtual string Operator => "";
        protected virtual bool TruncateOutput => true;
        protected virtual string Expression => $"{PortData[A].Name} {Operator} {PortData[B].Name}";

        public override void AddElements()
        {
            AddPort(new(PortDirection.Input, new Float(1, true), A, "A"));
            AddPort(new(PortDirection.Input, new Float(1, true), B, "B"));
            AddPort(new(PortDirection.Output, new Float(1, TruncateOutput), OUT));
        }

        protected override void Generate(NodeVisitor visitor)
        {
            int trunc = ImplicitTruncation(A, B).components;
            if (TruncateOutput)
            {
                ChangeComponents(OUT, trunc);
            }
            Output(visitor, OUT, Expression);
        }
    }
}