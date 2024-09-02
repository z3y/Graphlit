using System;
using UnityEngine;
using Enlit.Nodes;
using Enlit.Nodes.PortType;

namespace Enlit
{
    [NodeInfo("Input/Keyword Property"), Serializable]
    public class KeywordPropertyNode : PropertyNode
    {
        protected override PropertyType propertyType => PropertyType.KeywordToggle;

        const int TRUE = 1;
        const int FALSE = 2;

        public override void Initialize()
        {
            base.Initialize();
            AddPort(new(PortDirection.Input, new Float(1, true), TRUE, "True"));
            AddPort(new(PortDirection.Input, new Float(1, true), FALSE, "False"));
            AddPort(new(PortDirection.Output, new Float(1, true), OUT));
        }

        protected override void Generate(NodeVisitor visitor)
        {
            base.Generate(visitor);

            ChangeDimensions(OUT, ImplicitTruncation(TRUE, FALSE).dimensions);

            SetVariable(OUT, UniqueVariable);

            var data = PortData[OUT];
            var type = (Float)data.Type;
            visitor.AppendLine($"{PrecisionString(type.dimensions)} {data.Name};");
            visitor.AppendLine($"#ifdef {propertyDescriptor.KeywordName}");
            visitor.AppendLine($"{data.Name} = {PortData[TRUE].Name};");
            visitor.AppendLine("#else");
            visitor.AppendLine($"{data.Name} = {PortData[FALSE].Name};");
            visitor.AppendLine("#endif");
        }
    }
}