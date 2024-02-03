using System;
using UnityEngine;
using ZSG.Nodes;
using ZSG.Nodes.PortType;

namespace ZSG
{
    [NodeInfo("_/Keyword Property"), Serializable]
    public class KeywordPropertyNode : PropertyNode
    {
        protected override PropertyType propertyType => PropertyType.KeywordToggle;

        const int TRUE = 1;
        const int FALSE = 2;

        public override void AddElements()
        {
            base.AddElements();
            AddPort(new(PortDirection.Input, new Float(1, true), TRUE, "True"));
            AddPort(new(PortDirection.Input, new Float(1, true), FALSE, "False"));
            AddPort(new(PortDirection.Output, new Float(1, true), OUT));
        }

        protected override void Generate(NodeVisitor visitor)
        {
            base.Generate(visitor);

            ChangeComponents(OUT, ImplicitTruncation(TRUE, FALSE).components);

            SetVariable(OUT, UniqueVariable);

            var data = PortData[OUT];
            var type = (Float)data.Type;
            visitor.AppendLine($"{PrecisionString(type.components)} {data.Name};");
            visitor.AppendLine($"#ifdef {propertyDescriptor.KeywordName}");
            visitor.AppendLine($"{data.Name} = {PortData[TRUE].Name};");
            visitor.AppendLine("#else");
            visitor.AppendLine($"{data.Name} = {PortData[FALSE].Name};");
            visitor.AppendLine("#endif");
        }
    }
}