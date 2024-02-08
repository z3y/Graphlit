using System;
using ZSG.Nodes;
using ZSG.Nodes.PortType;

namespace ZSG
{
    [NodeInfo("Utility/Outline Pass Branch"), Serializable]
    public class OutlinePassBranchNode : ShaderNode
    {
        const int TRUE = 1;
        const int FALSE = 2;
        const int OUT = 3;
        public override bool DisablePreview => true;
        public override void Initialize()
        {
            AddPort(new(PortDirection.Input, new Float(1, true), TRUE, "True"));
            AddPort(new(PortDirection.Input, new Float(1, true), FALSE, "False"));
            AddPort(new(PortDirection.Output, new Float(1, true), OUT));
        }

        protected override void Generate(NodeVisitor visitor)
        {
            ChangeDimensions(OUT, ImplicitTruncation(TRUE, FALSE).dimensions);
            SetVariable(OUT, UniqueVariable);

            var data = PortData[OUT];
            var type = (Float)data.Type;
            visitor.AppendLine($"{PrecisionString(type.dimensions)} {data.Name};");
            visitor.AppendLine($"#ifdef OUTLINE_PASS");
            visitor.AppendLine($"{data.Name} = {PortData[TRUE].Name};");
            visitor.AppendLine("#else");
            visitor.AppendLine($"{data.Name} = {PortData[FALSE].Name};");
            visitor.AppendLine("#endif");
        }
    }
}